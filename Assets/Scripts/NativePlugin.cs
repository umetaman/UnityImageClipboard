using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public static class NativePlugin
{
    [DllImport("Clipboard")]
    private static extern bool hasClipboardImage();
    [DllImport("Clipboard")]
    private static extern void getClipboardSize(ref int width, ref int height, ref int bitsPerPixel);
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
        getClipboardSize(ref width, ref height, ref bitsPerPixel);
    
        if(width * height < 1)
        {
            throw new Exception("Not Support Image Size.");
        }

        int channel = bitsPerPixel / 8;

        if(channel != 3 && channel != 4)
        {
            throw new Exception("Not Support Channel Count.");
        }

        // C#側のバッファを用意する
        byte[] buffer = new byte[width * height * channel];
        // GCによって移動しないように固定する。必ず開放する。
        GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        // 確保したバッファのポインタ
        IntPtr bufferPtr = handle.AddrOfPinnedObject();

        bool successCopy = false;
        successCopy = getClipboardImage(bufferPtr);
        

    }
}