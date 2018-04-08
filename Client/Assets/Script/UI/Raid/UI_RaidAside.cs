using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class UI_RaidAside : UIBehaviour
{
        public Text m_Text;
        float m_fTime;
        float m_fMaxtime;
        Image m_NextBtn;
        void Awake()
        {
                m_NextBtn = GetImage("next");
                m_NextBtn.gameObject.SetActive(false);
        }
        bool m_bNeedClick = false;
        public void SetText(string text, bool needClick = false)
        {
                if (string.IsNullOrEmpty(text))
                {
                        UIManager.GetInst().CloseUI(this.name);
                        return;
                }
                
                m_Text.text = text;
                if (needClick)
                {
                        m_NextBtn.gameObject.SetActive(true);
                        SetupFullScreen();
                        m_bNeedClick = true;
                        this.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
                }
                else
                {
                        m_fTime = Time.realtimeSinceStartup;
                        m_fMaxtime = GlobalParams.GetFloat("raid_aside_exist_time");
                }
        }

        void Update()
        {
                if (m_bNeedClick)
                {
                        if (InputManager.GetInst().GetInputUp(true))
                        {
                                UIManager.GetInst().CloseUI(this.name);
                        }
                }
                else
                {
                        if (Time.realtimeSinceStartup - m_fTime >= m_fMaxtime)
                        {
                                UIManager.GetInst().CloseUI(this.name);
                        }
                }
        }
}
