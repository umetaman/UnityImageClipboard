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
    private static extern bool getClipboardImage(IntPtr buffer, ref int width, ref int height, ref int bitsPerPixel);

    public static bool HasClipboardImage()
    {
        return hasClipboardImage();
    }

    public static Texture2D GetClipboardImage()
    {
        IntPtr buffer = IntPtr.Zero;
        int width = 0;
        int height = 0;
        int bitsPerPixel = 0;

        bool success = getClipboardImage(buffer, ref width, ref height, ref bitsPerPixel);

        if(success == false || buffer == IntPtr.Zero)
        {
            throw new Exception("Failed to get image.");
        }

        Debug.Log(width);
        Debug.Log(height);
        Debug.Log(bitsPerPixel);

        return null;
    }
}