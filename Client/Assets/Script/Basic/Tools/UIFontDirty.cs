using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIFontDirty : MonoBehaviour
{
        bool isDirty = false;
        Font dirtyFont = null;

        void Awake()
        {
                Font.textureRebuilt += delegate(Font font1)
                {
                        isDirty = true;
                        dirtyFont = font1;
                };
        }

        void LateUpdate()
        {
                if (isDirty)
                {
                        isDirty = false;
                        foreach (Text text in GameObject.FindObjectsOfType<Text>())
                        {
                                if (text.font == dirtyFont)
                                {
                                        text.FontTextureChanged();
                                }
                        }
                        //print("雨松MOMO textureRebuilt " + dirtyFont.name);
                        dirtyFont = null;
                }
        }
}