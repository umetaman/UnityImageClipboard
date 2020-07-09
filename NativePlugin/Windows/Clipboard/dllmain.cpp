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

PLUGIN_API bool getClipboardImage(unsigned char* buffer) {
	bool isOpened = OpenClipboard(NULL);
	bool successCopy = false;

	if (isOpened) {
		if (IsClipboardFormatAvailable(CF_DIB)) {
			BITMAPINFO* bitmapInfoPtr = getLockedBitmapInfoPtr();

			if (bitmapInfoPtr != nullptr) {
				int width, height, bitsPerPixel;
				getClipboardImageSize(&width, &height, &bitsPerPixel);

				const unsigned char* pixelData = (unsigned char*)bitmapInfoPtr + bitmapInfoPtr->bmiHeader.biSize;
				memcpy(buffer, pixelData, width * height * (bitsPerPixel / 8));

				successCopy = true;
			}

			GlobalUnlock(bitmapInfoPtr);
		}
	}

	CloseClipboard();

	return successCopy;
}