using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
class UI_NpcIcon : UIBehaviour
{
        public Image icon;
        public Image bg;

        void Awake()
        {
                ResourceManager.GetInst().LoadIconSpriteSyn("Npc#qipao", bg.transform);
                //bg.SetNativeSize();
        }

        public void SetIcon(string url, bool bGray = false)
        {
                if (url.Length == 0)  //不显示
                {
                        icon.enabled = false;
                        bg.enabled = false;
                }
                else
                {
                        ResourceManager.GetInst().LoadIconSpriteSyn(url, icon.transform);
                        //icon.SetNativeSize();
                        UIUtility.SetImageGray(bGray, icon.transform);
                }

        }
}
