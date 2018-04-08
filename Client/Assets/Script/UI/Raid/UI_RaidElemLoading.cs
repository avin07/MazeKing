using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class UI_RaidElemLoading : UIBehaviour
{
        public Image LoadingBar;

        float m_fMaxTime = 0f;
        float m_fTime = 0f;

        public Canvas m_Canvas;
        public RectTransform m_BaseRT;
        public Transform m_BelongTransform;
        Vector3 m_WorldPos;
        HeroUnit belongUnit;
        public void SetLoading(float maxTime, HeroUnit unit)
        {
                belongUnit = unit;
                LoadingBar.fillAmount = 1f;
                m_fMaxTime = maxTime;
                m_fTime = Time.realtimeSinceStartup;

                m_WorldPos = belongUnit.transform.position + Vector3.up * 2f;
        }

        void Update()
        {
                if (m_BaseRT != null && belongUnit != null)
                {
                        m_WorldPos = belongUnit.transform.position + Vector3.up * 2f;
                        m_BaseRT.anchoredPosition = UIUtility.WorldToCanvas(m_Canvas, m_WorldPos);
                }

                if (Time.realtimeSinceStartup - m_fTime < m_fMaxTime)
                {
                        LoadingBar.fillAmount = 1f - (Time.realtimeSinceStartup - m_fTime) / m_fMaxTime;
                }
                else
                {
                        LoadingBar.fillAmount = 0f;
                        if (belongUnit != null)
                        {
                                belongUnit.FinishLoading();
                        }
                }
        }
}
