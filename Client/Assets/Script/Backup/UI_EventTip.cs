//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using System.Collections;
//using System.Collections.Generic;

//class UI_EventTip : UIBehaviour
//{
//        float m_fTime;
//        float m_fMaxTime;
//        Image m_TipImage;
//        void Awake()
//        {
//                m_fTime = Time.realtimeSinceStartup;
//                m_fMaxTime = 1.5f;

//                m_TipImage = GetImage("tip");
//        }

//        public void SetImageUrl(string path)
//        {
//                Debuger.Log(path);
//                //ResourceManager.GetInst().LoadOtherIcon(path, m_TipImage.transform);
//        }

//        void Update()
//        {
//                if (Time.realtimeSinceStartup - m_fTime >= m_fMaxTime)
//                {
//                        UIManager.GetInst().CloseUI(this.name);
//                }
//        }
//}
