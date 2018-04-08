//using UnityEngine;
//using System.Collections;
//using UnityEngine.UI;
//public class UI_AreaMapMenu : UIBehaviour
//{
//        Image m_Bg;
//        void Awake()
//        {
//                m_Bg = GetImage("bg");

//        }


//        public void SetupPosition(Vector3 pos)
//        {
//                Vector3 screenPos = Camera.main.WorldToScreenPoint(pos);
//                float anchors_x = screenPos.x / Screen.width;
//                float anchors_y = screenPos.y / Screen.height;
//                m_Bg.rectTransform.anchorMax = new Vector2(anchors_x, anchors_y);
//                m_Bg.rectTransform.anchorMin = new Vector2(anchors_x, anchors_y);
//                m_Bg.rectTransform.anchoredPosition =
//                        new Vector2(0, 50f);
//        }
//        int m_nDistrictId = 0;
//        public void SetDistrictId(int id)
//        {
//                m_nDistrictId = id;
//        }

//        public void OnClickClose(GameObject go)
//        {
//                UIManager.GetInst().CloseUI(this.name);
//        }
//        public void OnClickExplore(GameObject go)
//        {
//                UIManager.GetInst().CloseUI(this.name);
//                AreaMapManager.GetInst().SendExplore(m_nDistrictId);
//        }
//}
