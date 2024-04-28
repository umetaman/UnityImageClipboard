# UnityImageClipboard

A plugin for reading images on clipboard on Windows.

- It works in editor and runtime.
- Read the bitmap of image on clipboard.
- Get the raw pixel array.

# Example
```c#
  
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    public Image ImageRef;

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (NativePlugin.HasClipboardImage())
            {
                var texture = NativePlugin.GetClipboardImage();
                ImageRef.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            }
        }
    }
}
```

![unity_clipboard_result](https://user-images.githubusercontent.com/32408639/87244076-035f8c80-c476-11ea-909c-43d3b9380079.gif)

# TO DO
- [x] Windows support
- [x] macOS support
- [ ] 1, 4, 8, 16bit image support.
