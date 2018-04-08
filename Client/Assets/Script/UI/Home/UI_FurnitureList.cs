using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using DG.Tweening;

public class UI_FurnitureList : UIBehaviour
{

        Transform mFurniture;
        Transform mContent;

        FURNITURE_TAB m_tab = FURNITURE_TAB.WORK;
        public enum FURNITURE_TAB
        {
                WORK = 0,       //工作
                ASSIST = 1,     //辅助
                DECORATE = 2,   //装饰物
                WALL= 3,        //墙
                Door = 4,       //门

                MAX = 5,
        }

        void Awake()
        {
                for (int i = 0; i < (int)FURNITURE_TAB.MAX; i++)
                {
                        Button btn = FindComponent<Button>("bg/TabGroup/Tab" + i);
                        EventTriggerListener.Get(btn.gameObject).onClick = OnTab;
                        EventTriggerListener.Get(btn.gameObject).SetTag(i);
                }
                mContent = transform.Find("bg/scrollrect/content");
                mFurniture = mContent.GetChild(0);
                EventTriggerListener.Get(transform.Find("mask")).onClick = OnClickClose;
        }

        int nowUseCapacity;
        int maxCapacity;
        public void Refresh()
        {
                UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", false);
                nowUseCapacity = HomeManager.GetInst().GetNowUseCapacity();
                maxCapacity = HomeManager.GetInst().GetBuildAllLimit();
                FindComponent<Text>(transform, "allcapacity").text = nowUseCapacity + " / " + maxCapacity;

                for (int i = 0; i < (int)FURNITURE_TAB.MAX; i++)
                {
                        Image im = FindComponent<Image>("bg/TabGroup/Tab" + i + "/newlabel");
                        
                        int num = HomeManager.GetInst().CheckNewFurnitureList(i);
                        im.gameObject.SetActive(num > 0);
                }

                RefreshGroup(FURNITURE_TAB.WORK);

        }

        private void OnTab(GameObject go, PointerEventData data)
        {
                Button btn = go.GetComponent<Button>();
                FURNITURE_TAB pt = (FURNITURE_TAB)EventTriggerListener.Get(go).GetTag();
                if (pt != m_tab)
                {
                        Image im = FindComponent<Image>("bg/TabGroup/Tab" + (int)pt + "/newlabel");
                        im.gameObject.SetActive(false);
                        RefreshGroup(pt);
                }
        }

        void RefreshGroup(FURNITURE_TAB tab)
        {
                m_tab = tab;
                HightLightTab(tab);
                RefreshFurnitureList(tab);
        }

        void HightLightTab(FURNITURE_TAB tab)
        {
                int index = (int)tab;
                for (int i = 0; i < (int)FURNITURE_TAB.MAX; i++)
                {
                        Image btn = FindComponent<Image>("bg/TabGroup/Tab" + i);
                        Text text = FindComponent<Text>(btn.gameObject, "Text");

                        if (i == index)
                        {
                                text.color = UIUtility.activeColor;
                        }
                        else
                        {
                                text.color = UIUtility.unactiveColor;
                        }
                }
        }

        public override void OnClose(float time)
        {
                base.OnClose(time);
                HomeManager.GetInst().UnNewFurnitureList((int)m_tab);
        }

        public void RefreshFurnitureList(FURNITURE_TAB tab)
        {
                SetChildActive(mContent, false);
                List<int> mBuildList = HomeManager.GetInst().GetBuildFurnitureList((int)tab + 1);

                if (mBuildList.Count > 0)
                {
                        for (int i = 0; i < mBuildList.Count; i++)
                        {
                                Transform temp = GetChildByIndex(mContent, i);
                                if (temp == null)
                                {
                                        temp = CloneElement(mFurniture.gameObject).transform;
                                }
                                temp.SetActive(true);

                                int buildId = mBuildList[i];
                                BuildingInfoHold infoCfg = HomeManager.GetInst().GetBuildInfoCfg(buildId * 100 + 1);
                                BuildingTypeHold typeCfg = HomeManager.GetInst().GetBuildTypeCfg(buildId);
                                if (infoCfg == null || typeCfg == null)
                                {
                                        temp.SetActive(false);
                                        continue;
                                }

                                Transform nobuild = temp.Find("nobuild");
                                Transform canbuild = temp.Find("canbuild");

                                Image newlabel = FindComponent<Image>(temp, "bg/newlabel");
                                temp.name = typeCfg.id.ToString();
                                FindComponent<Text>(temp, "bg/name").text = LanguageManager.GetText(infoCfg.name);
                                FindComponent<Text>(temp, "bg/des").text = LanguageManager.GetText(infoCfg.desc);
                                ResourceManager.GetInst().LoadIconSpriteSyn(infoCfg.icon, FindComponent<Image>(temp, "bg/icon"));
                                Button btn = FindComponent<Button>(temp, "bg");
                                btn.onClick.RemoveAllListeners();

                                int capacity = typeCfg.take_capacity;
                                FindComponent<Text>(canbuild, "capacity").text = capacity.ToString();
                                int buildNowNum = HomeManager.GetInst().GetBuildTypeInHome(buildId).Count;   //当前已建造

                                int minLevel = HomeManager.GetInst().GetBuildMinLimitLevel(buildId);
                                if (minLevel > HomeManager.GetInst().MainBuildLevel) 
                                {
                                        canbuild.SetActive(false);
                                        nobuild.SetActive(true);
                                        newlabel.enabled = false;

                                        FindComponent<Text>(nobuild, "Image/tip").text = "需要旗帜等级" + minLevel;
                                        UIUtility.SetGroupGray(true, temp.gameObject);
                                }
                                else
                                {
                                        canbuild.SetActive(true);
                                        nobuild.SetActive(false);
                                        newlabel.enabled = HomeManager.GetInst().IsNewFurniture(buildId);
                                        BuildingLimitHold limit = HomeManager.GetInst().GetBuildingLimitCfg(buildId);
                                        if (limit != null)
                                        {
                                                int maxBuildNum = limit.max_number;
                                                if (buildNowNum >= maxBuildNum)  //当前已建造数量大于该等级最大可建造数量（不可见）
                                                {
                                                        temp.SetActive(false);
                                                        continue;
                                                }
                                                else
                                                {
                                                        canbuild.Find("num_tip").SetActive(true);
                                                        if (maxBuildNum >= 100)
                                                        {
                                                                FindComponent<Text>(canbuild, "num").text = "∞";
                                                        }
                                                        else
                                                        {
                                                                FindComponent<Text>(canbuild, "num").text = (maxBuildNum - buildNowNum).ToString();
                                                        }
                                                }
                                        }
                                        else  //程序判定为没有建造数量限制
                                        {
                                                canbuild.Find("num_tip").SetActive(false);
                                                FindComponent<Text>(canbuild, "num").text = string.Empty;
                                        }

                                        string creatCost = infoCfg.cost_material;
                                        bool canBuild = RefreshNeed(creatCost, canbuild);
                                        bool bCapacityEnough = true;
                                        FindComponent<Text>(canbuild, "times").text = UIUtility.GetTimeString3(infoCfg.cost_time);

                                        if (capacity + nowUseCapacity > maxCapacity)
                                        {
                                                bCapacityEnough = false;
                                        }
                                        else
                                        {
                                                bCapacityEnough = true;
                                        }

                                        if (!bCapacityEnough)
                                        {
                                                btn.onClick.AddListener(OnClickCapacityNotEnough);
                                        }
                                        else
                                        {
                                                if (canBuild)
                                                {
                                                        btn.onClick.AddListener(() => OnClick(buildId));
                                                }
                                                else
                                                {
                                                        btn.onClick.AddListener(OnClickNeedNotEnough);
                                                }
                                        }
                                        UIUtility.SetGroupGray(false, temp.gameObject);
                                }
                        }
                }
                (mContent as RectTransform).anchoredPosition = new Vector2(200, (mContent as RectTransform).anchoredPosition.y);
        }


        bool RefreshNeed(string needInfo, Transform root)
        {
                bool canBuild = true;
                Transform content = root.Find("cost");
                Transform item = content.GetChild(0);
                string[] need = needInfo.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                SetChildActive(content, false);

                int num = 0;
                string name = string.Empty;
                int id = 0;
                string des = string.Empty;
                Thing_Type type;

                for (int i = 0; i < need.Length; i++)
                {
                        Transform temp = GetChildByIndex(content, i);
                        if (temp == null)
                        {
                                temp = CloneElement(item.gameObject).transform;
                        }
                        CommonDataManager.GetInst().SetThingIcon(need[i], temp.Find("icon"), null, out name, out num, out id, out des, out type);
                        int has_num = CommonDataManager.GetInst().GetThingNum(need[i], out num);
                        if (has_num >= num)
                        {
                                FindComponent<Text>(temp, "item_num").text = num.ToString();
                        }
                        else
                        {
                                FindComponent<Text>(temp, "item_num").text = "<color=red>" + num + "</color>";
                                canBuild = false;
                        }
                        temp.SetActive(true);
                }
                return canBuild;
        }


        public void OnClickClose(GameObject go, PointerEventData data)
        {
                UIManager.GetInst().CloseUI(this.name);
                UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", true);
        }

        public void OnClick(int buildId)
        {
                HomeManager.GetInst().EnterCreatMode(buildId, HomeManager.GetInst().GetScreenCenterPos());
        }

        public void OnClickNeedNotEnough()
        {
                GameUtility.PopupMessage("建造所需材料不足！");
        }

        public void OnClickCapacityNotEnough()
        {
                GameUtility.PopupMessage("建造容量不够！");
        }
}

