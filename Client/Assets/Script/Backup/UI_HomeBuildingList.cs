//using UnityEngine;
//using System.Collections;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using System.Collections.Generic;
//using System;
//using DG.Tweening;

//public class UI_HomeBuildingList : UI_ScrollRectHelp
//{

//    public GameObject m_build;  

//    public enum BUILDING_TAB
//    {
//        RESOURSE = 0,      //资源
//        SUPPORT = 1,       //支持
//        DRAWING = 2,       //图纸
//        DECORATE = 3,      //装饰
//        MAX = 4,

//    }
//    BUILDING_TAB m_tab = BUILDING_TAB.RESOURSE;

//    void Awake()
//    {
//        for (int i = 0; i < (int)BUILDING_TAB.MAX; i++)
//        {
//            Button btn = GetButton(gameObject, "Tab" + i);
//            EventTriggerListener.Get(btn.gameObject).onClick = OnTab;
//            EventTriggerListener.Get(btn.gameObject).SetTag(i);
//        }
//        m_build.SetActive(false);
//    }

//    public void Refresh()
//    {
//        RefreshGroup(BUILDING_TAB.RESOURSE);
//    }

//    private void OnTab(GameObject go, PointerEventData data)
//    {
//        Button btn = go.GetComponent<Button>();
//        BUILDING_TAB pt = (BUILDING_TAB)EventTriggerListener.Get(go).GetTag();
//        if (pt != m_tab)
//        {
//            RefreshGroup(pt);
//        }
//    }

//    void RefreshGroup(BUILDING_TAB tab)
//    {
//        m_tab = tab;
//        HightLightTab(tab);
//        RefreshBuildingList(tab);
//    }

//    void HightLightTab(BUILDING_TAB tab)
//    {
//        int index = (int)tab;
//        for (int i = 0; i < (int)BUILDING_TAB.MAX; i++)
//        {
//            Image btn = GetImage(gameObject, "Tab" + i);
//            Image left = GetImage(btn.gameObject, "left");
//            Image right = GetImage(btn.gameObject, "right");
//            if (i < index)
//            {
//                btn.enabled = false;
//                left.enabled = true;
//                right.enabled = false;
//            }
//            if (i == index)
//            {
//                btn.enabled = true;
//                left.enabled = false;
//                right.enabled = false;
//            }
//            if (i > index)
//            {
//                btn.enabled = false;
//                left.enabled = false;
//                right.enabled = true;
//            }
//        }
//    }

//    public void RefreshBuildingList(BUILDING_TAB tab)
//    {
//        SetGameObjectHide("build", sr.gameObject);
//        List<int> m_build_list = HomeManager.GetInst().GetMyBuildList((int)tab + 1);
//        if (m_build_list != null)
//        {
//            for (int i = 0; i < m_build_list.Count; i++)
//            {
//                GameObject temp = GetGameObject(sr.gameObject, "build" + i);
//                if (temp == null)
//                {
//                    temp = CloneElement(m_build, "build" + i);
//                }

//                GameObject nobuild = GetGameObject(temp,"nobuild");
//                GameObject canbuild = GetGameObject(temp,"canbuild");


//                BuildInfo m_info = new BuildInfo(m_build_list[i]);
//                BuildingInfoHold m_build_info = m_info.build_cfg;

//                GetText(temp, "name").text = LanguageManager.GetText(m_build_info.name);
//                GetText(temp, "des").text = LanguageManager.GetText(m_build_info.desc);


//                //string icon_url = ModelResourceManager.GetInst().GetIconRes(m_build_info.model);
//                //Image icon = GetImage("icon");
//                //ResourceManager.GetInst().LoadIconSpriteSyn(icon_url, icon.transform);
//                //icon.SetNativeSize();
//                Button btn = GetButton(temp, "bg");
//                EventTriggerListener.Get(btn.gameObject).onDarg = OnDarg;
//                EventTriggerListener.Get(btn.gameObject).onBeginDarg = OnBeginDrag;
//                EventTriggerListener.Get(btn.gameObject).onEndDarg = OnEndDrag;


//                int can_build_num = 0;
//                HomeBuildingMainHold m_main_build = HomeManager.GetInst().GetBuildingMainCfg(m_build_list[i]);
//                if (m_main_build != null)
//                {
//                    can_build_num = HomeManager.GetInst().GetBuildingMainCfg(m_build_list[i]).quality;
//                }
//                int now_num = HomeManager.GetInst().GetBuildNumByID(m_build_list[i]);
//                int rest_num = can_build_num - now_num;

//                bool can_show = false;
//                if (rest_num > 0) //可建造
//                {
//                    nobuild.SetActive(false);
//                    canbuild.SetActive(true);
//                    string creat_need = m_build_info.level_up_cost;
//                    bool can_build = RefreshNeed(creat_need, temp);
//                    GetText(canbuild, "num").text = now_num + "/" + can_build_num;
//                    GetText(canbuild, "times").text = UIUtility.GetTimeString3(m_build_info.level_up_time);
//                    UIUtility.SetGroupGray(false, temp);
//                    if (can_build)
//                    {
//                        EventTriggerListener.Get(btn.gameObject).onClick = OnClick;
//                        EventTriggerListener.Get(btn.gameObject).SetTag(m_info);
//                    }
//                    else
//                    {
//                        EventTriggerListener.Get(btn.gameObject).onClick = OnClickCannot;
//                        EventTriggerListener.Get(btn.gameObject).SetTag(null);
//                    }
//                    can_show = true;

//                }
//                else  
//                {
//                    nobuild.SetActive(true);
//                    canbuild.SetActive(false);
//                    UIUtility.SetGroupGray(true, temp);
//                    EventTriggerListener.Get(btn.gameObject).onClick = null;

//                    if (can_build_num == 0) //说明当前主城等级还不能激活
//                    {
//                            if (m_main_build != null)
//                            {
//                                    int need_level = HomeManager.GetInst().GetBuildNeedLevel(m_build_list[i]);
//                                    GetText(nobuild, "tip").text = "主城等级" + need_level + "级时解锁";
//                            }
//                            else
//                            {
//                                    GetText(nobuild, "tip").text = "敢不敢在building表里配下" + (HomeManager.GetInst().MainBuildLevel * 10000 + m_build_list[i]).ToString();
//                            }
//                            can_show = true;

//                    }
//                    else   //当前等级已经建造满了
//                    {
//                        int next_can_build = 0;
//                        for (int j = HomeManager.GetInst().MainBuildLevel + 1; j < 100; j++)
//                        {
//                            HomeBuildingMainHold hbm = HomeManager.GetInst().GetBuildingMainCfgBylevel(m_build_list[i], j);
//                            if (hbm == null)
//                            {
//                                break;
//                            }
//                            next_can_build = hbm.quality;
//                            if (next_can_build > can_build_num)
//                            {
//                                GetText(nobuild, "tip").text = "主城等级" + j + "级时可以建造更多";
//                                can_show = true;
//                                break;
//                            }
//                        }
//                    }
//                }
//                temp.SetActive(can_show);             
//            }
//        }     
//    }


//    bool RefreshNeed(string need_info,GameObject root)  
//    {
//        bool can_build = true;
//        GameObject item = GetGameObject(root, "item0");
//        string[] need = need_info.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
//        SetGameObjectHide("item", root);

//        int num = 0;
//        string name = "";
//        int id = 0;
//        string des = "";
//        Thing_Type type;

//        for (int i = 0; i < need.Length; i++)
//        {
//            GameObject temp = GetGameObject(root, "item" + i);
//            if (temp == null)
//            {
//                temp = CloneElement(item, "item" + i);
//            }
//            Transform icon = GetImage(temp, "icon").transform;
//            CommonDataManager.GetInst().SetThingIcon(need[i], icon, out name, out num, out id, out des,out type);
//            int has_num = CommonDataManager.GetInst().GetThingNum(need[i], out num);
//            if (has_num >= num)
//            {
//                GetText(temp, "item_num").text = num.ToString();
//            }
//            else
//            {
//                GetText(temp, "item_num").text = "<color=red>" + num + "</color>";
//                can_build = false;
//            }
//            temp.SetActive(true);
//        }
//        return can_build;
//    }


//    public void OnClickClose()
//    {
//        UIManager.GetInst().CloseUI(this.name);
//        UI_HomeMain uhm = UIManager.GetInst().GetUIBehaviour<UI_HomeMain>();
//        if (uhm != null)
//        {
//            uhm.ShowBtnGroup(true);
//        }
//    }

//    public void OnClick(GameObject go, PointerEventData data)
//    {
//        if (!can_click)
//        {
//            return;
//        }
//        BuildInfo build_info = (BuildInfo)EventTriggerListener.Get(go).GetTag();
//        Vector2 pos = HomeManager.GetInst().CalculateNewBuildPosition(build_info.size);
//        if (pos.x < 0)
//        {
//            GameUtility.PopupMessage("没有可建造的区域！");
//        }
//        else
//        {
//            build_info.SetPos(pos);
//            HomeManager.GetInst().SetBuild(build_info);
//            OnClickClose();
//        }

//    }

//    public void OnClickCannot(GameObject go, PointerEventData data)
//    {
//        if (!can_click)
//        {
//            return;
//        }
//        GameUtility.PopupMessage("建造所需材料不足！");


//    }



//}

