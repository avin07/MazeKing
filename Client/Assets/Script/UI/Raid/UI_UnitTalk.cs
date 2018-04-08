using UnityEngine;
using System.Collections;
using UnityEngine.UI;

class UI_UnitTalk : UIBehaviour
{
        public Sprite m_GoodTalk;
        public Sprite m_BadTalk;
        public Text m_NameText;
        public Text m_TalkText;
        public Image m_TalkBg;

        public void SetText(string name, string text)
        {
                m_NameText.text = name;
                m_TalkText.text = text;
        }
}
