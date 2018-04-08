using UnityEngine;
using System.Collections;

class CameraManager : SingletonBehaviour<CameraManager>
{
        public Camera UI_Camera;
        public override void Awake()
        {
                base.Awake();
                m_BlackTex = new Texture2D(1, 1);
                m_BlackTex.SetPixel(0, 0, Color.black);
                m_BlackTex.Apply();
        }
        Texture2D m_BlackTex;
        bool m_bFadeOut = false;
        bool m_bFadeIn = false;
        float m_fFadeTime = 0f;
        float m_fFadeMaxTime = 0f;
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
        GUIStyle m_GuiStyle;
        void DrawFade()
        {
                if (m_bFadeIn)
                {
                        if (Time.realtimeSinceStartup - m_fFadeTime < m_fFadeMaxTime)
                        {
                                float a = 1f - (Time.realtimeSinceStartup - m_fFadeTime) / m_fFadeMaxTime;
                                m_BlackTex.SetPixel(0, 0, new Color(1, 1, 1, a));
                                Debug.Log(a);
                        }
                        else
                        {
                                m_bFadeIn = false;
                        }
                        GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), m_BlackTex);

                }
                else if (m_bFadeOut)
                {
                        if (Time.realtimeSinceStartup - m_fFadeTime < m_fFadeMaxTime)
                        {
                                float a = (Time.realtimeSinceStartup - m_fFadeTime) / m_fFadeMaxTime;
                                m_BlackTex.SetPixel(0, 0, new Color(1, 1, 1, a));
                                Debug.Log(a);
                        }
                        else
                        {
                                m_bFadeOut = false;
                        }
                        GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), m_BlackTex);
                        
                }

        }
        void OnGUI()
        {
                DrawFade();
        }

}
