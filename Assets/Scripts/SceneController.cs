using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    public Image ImageRef;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)){
            Debug.Log("HasClipboard: " + NativePlugin.HasClipboardImage());
        }

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
