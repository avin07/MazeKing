//using UnityEngine;
//using System.Collections;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using System.Collections.Generic;
//using System;

//public class UI_BuildDrawList : UIBehaviour
//{
//        GameObject m_drawbtn;
//        ScrollRect sr;

//        void Awake()
//        {
//                sr = FindComponent<ScrollRect>("GameObject/ScrollRect");
//                m_drawbtn = sr.transform.Find("content/draw").gameObject;
//                m_drawbtn.SetActive(false);
//                FindComponent<Button>("close").onClick.AddListener(OnClose);
//        }

//        public void Refresh()
//        {               
//                UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", false);
//                CreatDraw();
//        }

//        void CreatDraw()
//        {
//                Dictionary<int, int> m_drawDict = HomeManager.GetInst().GetDrawingDict();
//                SetChildActive(m_drawbtn.transform.parent,false);
//                GameObject draw_btn = null;
//                int i = 0;
//                foreach (int id in m_drawDict.Keys)
//                {
//                        if (m_drawDict[id] <= 0)
//                        {
//                                continue;
//                        }

//                        draw_btn = GetGameObject(m_drawbtn.transform.parent.gameObject, "draw" + i);
//                        if (draw_btn == null)
//                        {
//                                draw_btn = CloneElement(m_drawbtn, "draw" + i);
//                        }
//                        draw_btn.SetActive(true);
//                        BuildingDrawingConfig bd = HomeManager.GetInst().GetDrawingCfg(id);

//                        UpdateDraw(bd,m_drawDict[id],draw_btn);
//                        i++;
//                }
//                GetGameObject(sr.gameObject, "content").GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
//        }

//        void UpdateDraw(BuildingDrawingConfig bd, int has_num, GameObject go)
//        {

//                GetText(go, "name").text = LanguageManager.GetText(bd.name);
//                GetText(go, "des").text = LanguageManager.GetText(bd.desc);
//                ResourceManager.GetInst().LoadIconSpriteSyn(bd.icon, GetImage(go, "icon").transform);
//                if (has_num < 0)
//                {
//                        has_num = 0;
//                }
//                GetText(go, "draw_num").text = has_num.ToString();            
//        }

//        public void OnClose()
//        {
//                UIManager.GetInst().CloseUI(this.name);
//                UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", true);
//        }
//}
