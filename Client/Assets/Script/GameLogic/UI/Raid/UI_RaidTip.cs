using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class UI_RaidTip : UIBehaviour
{
        public Text m_Text;
        float m_fTime;
        public void ShowText(string text, float time = 1f)
        {
                m_Text.text = text;
                m_fTime = Time.realtimeSinceStartup;
        }
        void Update()
        {
                if (Time.realtimeSinceStartup - m_fTime > 2f)
                {
                        OnClickClose(null);
                }
        }        

        void Awake()
        {
                this.UILevel = UI_LEVEL.TIP;
        }
}
