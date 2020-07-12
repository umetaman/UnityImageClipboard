#define PLUGIN_API extern "C" __declspec(dllexport)

// https://docs.microsoft.com/en-us/windows/win32/dataxchg/clipboard
#include <Windows.h>

PLUGIN_API bool hasClipboardImage() {
	// クリップボードから取り出すときは開く
	bool isOpened = OpenClipboard(NULL);
	bool hasImage = false;

	if (isOpened) {
		hasImage = IsClipboardFormatAvailable(CF_DIB);
	}

	// 必ず閉じる
	CloseClipboard();

	return hasImage;
}

// ロックしたBITMAPINFOをクリップボードからとってくる。
// 解放は必須である
static BITMAPINFO* getLockedBitmapInfoPtr() {
	HANDLE hClipboardData = GetClipboardData(CF_DIB);

	LPVOID clipboardDataPtr = GlobalLock(hClipboardData);
	return static_cast<BITMAPINFO*>(clipboardDataPtr);
}

// 画像サイズを取得する
// C#側でメモリを確保するために必要
PLUGIN_API void getClipboardImageSize(int* width, int* height, int* bitsPerPixel) {
	bool isOpened = OpenClipboard(NULL);
	if (isOpened) {
		if (IsClipboardFormatAvailable(CF_DIB)) {
			BITMAPINFO* bitmapInfoPtr = getLockedBitmapInfoPtr();

			if (bitmapInfoPtr != nullptr) {
				*width = bitmapInfoPtr->bmiHeader.biWidth;
				*height = bitmapInfoPtr->bmiHeader.biHeight;
				*bitsPerPixel = bitmapInfoPtr->bmiHeader.biBitCount;
			}

			GlobalUnlock(bitmapInfoPtr);
		}
	}

	CloseClipboard();
}

// C#側で用意したバッファに書き込む
PLUGIN_API bool getClipboardImage(unsigned char* buffer) {
	bool isOpened = OpenClipboard(NULL);
	bool successCopy = false;

	if (isOpened) {
		if (IsClipboardFormatAvailable(CF_DIB)) {
			BITMAPINFO* bitmapInfoPtr = getLockedBitmapInfoPtr();

			if (bitmapInfoPtr != nullptr) {
				int width, height, bitsPerPixel;
				getClipboardImageSize(&width, &height, &bitsPerPixel);

				// 1ピクセル辺り24bit(JPEG), 32bit(PNG-32)の画像をサポートする
				if (bitsPerPixel == 24 || bitsPerPixel == 32) {
					unsigned char* pixelData = (unsigned char*)(bitmapInfoPtr)+bitmapInfoPtr->bmiHeader.biSize;
					// 1ピクセル当たりのバイト数
					int bytesPerPixel = bitsPerPixel / 8;
				
					// カラーマスクがあるときはその分シフトする
					if (bitmapInfoPtr->bmiHeader.biCompression == BI_BITFIELDS) {
						pixelData += 4 * 3;
					}

					// C#で確保したBufferにコピーする
					int bytesPerLine = width * bytesPerPixel;
					if (bytesPerLine % 4 != 0) {
						bytesPerLine += 4 - (bytesPerLine % 4);
					}

					unsigned char* dst = buffer;
					unsigned char* src = pixelData;

					for (int h = 0; h < height; h++) {
						memcpy(
							dst + (width * h * bytesPerPixel),
							src + (h * bytesPerLine),
							width * bytesPerPixel
						);
					}

					successCopy = true;
				}
			}

			GlobalUnlock(bitmapInfoPtr);
		}
	}

	CloseClipboard();

	return successCopy;
}