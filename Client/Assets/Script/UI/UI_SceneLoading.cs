using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class UI_SceneLoading : UIBehaviour
{
        public Image[] StarList;
        Image m_Bg;
        
        bool m_bFadeOut = false;
        bool m_bFadeIn = false;
        float m_fFadeTime = 0f;
        float m_fFadeMaxTime = 0f;

        void Awake()
        {
                m_Bg = GetImage("bg");
                Object.DontDestroyOnLoad(this.gameObject);
        }

        public void FadeOut(float maxtime)
        {
                m_bFadeOut = true;
                m_bFadeIn = false;
                m_fFadeMaxTime = maxtime;
                m_fFadeTime = Time.realtimeSinceStartup;
        }
        public void FadeIn(float maxtime)
        {
                m_bFadeOut = false;
                m_bFadeIn = true;
                m_fFadeMaxTime = maxtime;
                m_fFadeTime = Time.realtimeSinceStartup;
        }

        void Update()
        {
                if (Time.realtimeSinceStartup - m_fFadeTime < m_fFadeMaxTime)
                {
                        if (m_bFadeOut)
                        {
                                m_Bg.color = new Color(m_Bg.color.r, m_Bg.color.g, m_Bg.color.b, 1f - (Time.realtimeSinceStartup - m_fFadeTime) / m_fFadeMaxTime);
                        }
                        else if (m_bFadeIn)
                        {
                                m_Bg.color = new Color(m_Bg.color.r, m_Bg.color.g, m_Bg.color.b, (Time.realtimeSinceStartup - m_fFadeTime) / m_fFadeMaxTime);
                        }
                        else
                        {
                                UIManager.GetInst().CloseUI(this.name);
                        }
                }
                else
                {
                        if (m_bFadeOut)
                        {
                                UIManager.GetInst().CloseUI(this.name);
                        }
                }
        }
}
