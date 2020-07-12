# UnityNativeClipboard

A plugin for reading images on clipboard on Windows.

(It will correspond to macOS later.)

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

![unity_clipboard_result.gif](https://qiita-image-store.s3.ap-northeast-1.amazonaws.com/0/279082/3593129b-f324-e801-cb5c-17d6cb280db0.gif)

# TO DO
- [x] Windows support
- [ ] macOS support
- [ ] 1, 4, 8, 16bit image support.