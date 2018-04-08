//using UnityEngine;
//using System.Collections;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using System.Collections.Generic;
//using System;

//public class UI_BuildCreatList : UIBehaviour
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
//                CreatDraw(HomeManager.GetInst().GetSelectBuild().mBuildInfo);
//        }

//        void CreatDraw(BuildInfo info)
//        {
//                SetChildActive(m_drawbtn.transform.parent,false);
//                List<int> buildList = HomeManager.GetInst().CheckCanCreatOnFoundation(info);
//                GameObject draw_btn = null;
//                for (int i = 0; i < buildList.Count; i++)
//                {
//                        BuildingInfoHold build_cfg = HomeManager.GetInst().GetBuildInfoCfg(buildList[i] * 100);  //0级是建造;
//                        draw_btn = GetGameObject(m_drawbtn.transform.parent.gameObject, "draw" + i);
//                        if (draw_btn == null)
//                        {
//                                draw_btn = CloneElement(m_drawbtn, "draw" + i);
//                        }
//                        draw_btn.SetActive(true);
                      
//                        if (build_cfg != null)
//                        {
//                                UpdateDraw(build_cfg, draw_btn);
//                        }
//                        else
//                        {
//                                draw_btn.SetActive(false);
//                        }
                    
//                }
//                GetGameObject(sr.gameObject, "content").GetComponent<RectTransform>().anchoredPosition = Vector2.zero;


//        }

//        void UpdateDraw(BuildingInfoHold build_cfg, GameObject go)
//        {
//                string[] cost = build_cfg.level_up_cost.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
//                if (!build_cfg.level_up_cost.Contains("5,"))
//                {
//                        singleton.GetInst().ShowMessage(ErrorOwner.designer, "建筑" + build_cfg.id + "升级消耗没有配图纸");
//                }
//                int num = 0;
//                string name = "";
//                int id = 0;
//                string des = "";
//                Thing_Type type;
//                bool isDrwa = false;
//                bool isResource = false;
//                for (int i = 0; i < cost.Length; i++)
//                {
//                        if (cost[i][0].Equals('5')) //图纸
//                        {
//                                CommonDataManager.GetInst().SetThingIcon(cost[i],  GetImage(go, "icon").transform,null,out name, out num, out id, out des, out type);
//                                GetText(go, "name").text = name;
//                                GetText(go, "des").text = des;
//                                int has_num = HomeManager.GetInst().GetDrawingNum(id);
//                                if (has_num < 0)
//                                {
//                                        has_num = 0;
//                                }
//                                GetText(go, "draw_num").text = num + " / " + has_num;
//                                if (has_num >= num)
//                                {
//                                        isDrwa = true;
//                                }

//                                if (has_num <= 0)
//                                {
//                                        go.transform.SetActive(false);
//                                }
//                        }
//                        else
//                        {
//                                CommonDataManager.GetInst().SetThingIcon(cost[i], GetImage(go, "cost_icon").transform,null, out name, out num, out id, out des, out type);
//                                GetText(go, "res").text = num.ToString();
//                                isResource = CommonDataManager.GetInst().CheckIsThingEnough(cost[i]);
//                        }
//                }

//                if (isDrwa && isResource)
//                {
//                        GetButton(go, "creat").interactable = true;
//                }
//                else
//                {
//                        GetButton(go, "creat").interactable = false;
//                }

//                EventTriggerListener.Get(GetGameObject(go, "creat")).onClick = OnCreat;
//                EventTriggerListener.Get(GetGameObject(go, "creat")).SetTag(build_cfg.id / 100);
//        }


//        private void OnCreat(GameObject go, PointerEventData data)
//        {
//                if (go.GetComponent<Button>().interactable == false)
//                {
//                        GameUtility.PopupMessage("材料不足！");
//                }
//                else
//                {
//                        int build_id = (int)EventTriggerListener.Get(go).GetTag();
//                        HomeManager.GetInst().SendCreatBuild(build_id);
//                        OnClose();
//                }
//        }

//        public void OnClose()
//        {
//                UIManager.GetInst().CloseUI(this.name);
//                UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", true);
//        }
//}
