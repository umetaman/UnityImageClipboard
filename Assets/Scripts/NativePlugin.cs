using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public static class NativePlugin
{
    [DllImport("Clipboard")]
    private static extern bool hasClipboardImage();
    [DllImport("Clipboard")]
    private static extern void getClipboardImageSize(ref int width, ref int height, ref int bitsPerPixel);
    [DllImport("Clipboard")]
    private static extern bool getClipboardImage(IntPtr buffer);

    public static bool HasClipboardImage()
    {
        return hasClipboardImage();
    }

    public static Texture2D GetClipboardImage()
    {
        int width = 0;
        int height = 0;
        int bitsPerPixel = 0;
        getClipboardImageSize(ref width, ref height, ref bitsPerPixel);
    
        if(width * height < 1)
        {
            throw new Exception("Not Support Image Size.");
        }

        int channel = bitsPerPixel / 8;

        if(channel != 3 && channel != 4)
        {
            throw new Exception("Not Support Channel Count.");
        }

        Debug.LogFormat("[NativePlugin] GetClipboardImage() Width: {0}, Height: {1}, Channel: {2}", width, height, channel);

        // C#側の領域を用意する
        byte[] buffer = new byte[width * height * channel];

        // GCによって移動しないように固定する。必ず開放する。
        GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        // 確保したバッファのポインタ
        IntPtr bufferPtr = handle.AddrOfPinnedObject();

        // クリップボードからコピーする
        bool successCopy = false;
        successCopy = getClipboardImage(bufferPtr);

        // 解放
        handle.Free();

        if (successCopy)
        {
#if UNITY_STANDALONE_WIN
            Texture2D texture = createClipboardTextureWindows(width, height, channel, ref buffer);
#elif UNITY_STANDALONE_OSX
            Texture2D texture = createClipboardTextureOSX(width, height, channel, ref buffer);
#endif

            buffer = null;
            return texture;
        }

        return null;
    }

    private static Texture2D createClipboardTextureWindows(int width, int height, int channel, ref byte[] buffer)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.BGRA32, false);

        // BGRA
        if (channel == 4)
        {
            texture.LoadRawTextureData(buffer);
        }
        // BGR
        else if (channel == 3)
        {
            Color32[] pixels = new Color32[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i].b = buffer[channel * i];
                pixels[i].g = buffer[channel * i + 1];
                pixels[i].r = buffer[channel * i + 2];
                pixels[i].a = (byte)255;
            }

            texture.SetPixels32(pixels);
            pixels = null;
        }

        texture.Apply();
        return texture;
    }

    private static Texture2D createClipboardTextureOSX(int width, int height, int channel, ref byte[] buffer)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[width * height];

        for(int i = 0; i < pixels.Length; i++)
        {
            byte alpha = channel == 4 ? buffer[i * channel + 3] : (byte)255;
            pixels[i].a = alpha;

            pixels[i].r = alpha != 0 ? (byte)(buffer[i * channel] * 255 / alpha) : buffer[i * channel];
            pixels[i].g = alpha != 0 ? (byte)(buffer[i * channel + 1] * 255 / alpha) : buffer[i * channel + 1];
            pixels[i].b = alpha != 0 ? (byte)(buffer[i * channel + 2] * 255 / alpha) : buffer[i * channel + 2];
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        pixels = null;

        return texture;
    }
}