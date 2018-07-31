using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class UI_AwardItemTip : UIBehaviour
{
        float m_fTime;
        float m_fMaxTime;
        float m_fShowTime;
        float m_fDelay;
        float m_fOffsetY;
        public Image m_Num0;
        public GameObject m_NumberRoot;
        bool m_bUpdatePos = false;
        void Awake()
        {
                m_fOffsetY = 0.01f;
                m_fTime = Time.realtimeSinceStartup;
                m_fMaxTime = 2f;
                m_fShowTime = 1f;
        }
        public void SetIcon(string name, bool bUpdate = true)
        {
                m_bUpdatePos = bUpdate;
                ResourceManager.GetInst().LoadIconSpriteSyn(name, m_Num0.transform);
        }

        void Update()
        {
                if (Time.realtimeSinceStartup - m_fTime >= m_fShowTime)
                {
                        if (Time.realtimeSinceStartup - m_fTime < m_fMaxTime)
                        {
                                float alpha = Mathf.Lerp(1f, 0f, (Time.realtimeSinceStartup - m_fTime - m_fShowTime) / (m_fMaxTime - m_fShowTime));
                                m_Num0.color = new Color(m_Num0.color.r, m_Num0.color.g, m_Num0.color.b, alpha);

                                if (m_bUpdatePos)
                                {
                                        transform.position += Vector3.up * m_fOffsetY;
                                }
                        }
                        else
                        {
                                GameObject.Destroy(this.gameObject);
                        }
                }
        }

}
