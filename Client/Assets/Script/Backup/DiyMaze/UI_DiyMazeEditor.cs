//using UnityEngine;
//using System.Collections;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using System.Collections.Generic;
//using System;
//using DG.Tweening;

//public class UI_DiyMazeEditor : UI_ScrollRectHelp
//{

//        public GameObject Tab_Group;
//        public GameObject FragmentObj;
//        public GameObject MazeGroup;
//        public GameObject Area;
//        public GameObject DragIcon;


//        public enum FRAGMENT_TAB
//        {
//                COMMON = 0,      //通用
//                OTHER = 1,       //其他
//                MAX = 2,

//        }
//        FRAGMENT_TAB m_tab = FRAGMENT_TAB.COMMON;

//        void Awake()
//        {
//                for (int i = 0; i < (int)FRAGMENT_TAB.MAX; i++)
//                {
//                        Button btn = GetButton(Tab_Group, "Tab" + i);
//                        EventTriggerListener.Get(btn.gameObject).onClick = OnTab;
//                        EventTriggerListener.Get(btn.gameObject).SetTag(i);
//                }
//                DragIcon.SetActive(false);
//        }


//        public void Refresh(MazeInfo info)
//        {
//                UIManager.GetInst().SetUIActiveState<UI_BuildPainting>("UI_BuildPainting", false);
//                m_MazeInfo = info;
//                InitFragmentTab();
//                InitDefendterrainEffect();   //地形效果初始化//
//                InitMaze();
//                RefreshFragmentType(FRAGMENT_TAB.COMMON);
//        }

//        readonly string[] temp_chinese = { "", "森林拼版", "山脉拼版", "冰原拼版" };
//        MazeInfo m_MazeInfo
//        {
//                get
//                {
//                        return DiyRaidManager.GetInst().m_UseMazeInfo;
//                }

//                set
//                {
//                        DiyRaidManager.GetInst().m_UseMazeInfo = value;
//                }
                        
//        }
//        List<GameObject> m_MazeMapObj = new List<GameObject>();
//        List<int> m_MazeMap = new List<int>();     //操作后的迷宫存储
//        int now_use_num = 0; //当前已经放置的数量
//        void InitMaze()
//        {
//                m_MazeMapObj.Clear();
//                int size = m_MazeInfo.terrainCfg.size;
//                float area_size_x = (Area.transform as RectTransform).sizeDelta.x;
//                float pos_x = (1 - size) * 0.5f * area_size_x;
//                for (int i = 0; i < size * size; i++)   //初始化obj
//                {
//                        m_MazeMap.Add(0);
//                        GameObject temp = GetGameObject(MazeGroup,"area" + i);
//                        if (temp == null)
//                        {
//                                temp = CloneElement(Area, "area" + i);
//                        }
//                        (temp.transform as RectTransform).anchoredPosition = new Vector2(pos_x + (i % size) * area_size_x, pos_x + (i / size) * area_size_x);
//                        EventTriggerListener.Get(temp).onClick = OnMazeClick;
//                        EventTriggerListener.Get(temp).onBeginDarg = OnBeginMazeDrag;
//                        EventTriggerListener.Get(temp).onEndDarg = OnEndMazeDrag;
//                        EventTriggerListener.Get(temp).onDarg = OnMazeDrag;
//                        EventTriggerListener.Get(temp).onDrop = OnMazeDrop;
//                        EventTriggerListener.Get(temp).SetTag(i);
//                        m_MazeMapObj.Add(temp);
//                }


//                for (int i = 0; i < m_MazeInfo.MapFragment.Count; i++)
//                {
//                        SetMazeFragment(i, m_MazeInfo.MapFragment[i]);
//                        if (m_MazeInfo.MapFragment[i] != 0)
//                        {
//                                now_use_num++;
//                        }
//                }
//                now_use_num -= 2;
         
//                CheckState();
//                SetCanUseNum();
//                DragIcon.transform.SetAsLastSibling();
//        }


//        void InitFragmentTab()
//        {
//                Button btn = GetButton(Tab_Group, "Tab" + (int)FRAGMENT_TAB.OTHER);
//                GetText(btn.gameObject, "Text").text = temp_chinese[m_MazeInfo.terrainCfg.type];
//        }



//        private void OnMazeClick(GameObject go, PointerEventData data)
//        {
//                if (!isdrag)
//                {
//                        int index = (int)EventTriggerListener.Get(go).GetTag();
//                        int id = GetMazeFragment(index);
//                        if (id != m_MazeInfo.terrainCfg.entrance_fragment && id != m_MazeInfo.terrainCfg.exit_fragment && id != 0)
//                        {
//                                SetMazeFragment(index, 0);  //移除地块
//                                now_use_num--;
//                                SetCanUseNum();
//                                CheckState();
//                        }

//                        if (id == m_MazeInfo.terrainCfg.exit_fragment)  //出口设置防守小队
//                        {
//                                UI_RaidFormation uis = UIManager.GetInst().ShowUI<UI_RaidFormation>("UI_RaidFormation");
//                                uis.SetupDefend(this, m_MazeInfo.petlist);
//                        }
//                }
//        }


//        int drag_index = 0;
//        bool isdrag = false;
//        public void OnBeginMazeDrag(GameObject go, PointerEventData data)
//        {
//                int index = (int)EventTriggerListener.Get(go).GetTag();
//                if (GetMazeFragment(index) != 0)
//                {
//                        drag_index = index;
//                        isdrag = true;
//                        DragIcon.SetActive(true);
//                        go.GetComponent<Image>().DOFade(0.2f, 0.1f);
//                }
//        }

//        public void OnMazeDrag(GameObject go, PointerEventData data)
//        {
//                if (isdrag)
//                {
//                        SetPosition(data);
//                }
//        }


//        Vector2 pos;
//        public void SetPosition(PointerEventData data)
//        {
//                RectTransformUtility.ScreenPointToLocalPointInRectangle(DragIcon.transform.parent.transform as RectTransform, data.position, data.pressEventCamera, out pos);
//                (DragIcon.transform as RectTransform).anchoredPosition = pos;
//        }


//        public void OnEndMazeDrag(GameObject go, PointerEventData data)
//        {
//                isdrag = false;
//                DragIcon.SetActive(false);
//                m_MazeMapObj[drag_index].GetComponent<Image>().DOFade(1.0f, 0.1f);
//        }

//        public void OnMazeDrop(GameObject go, PointerEventData data)
//        {
//                if (isdrag)
//                {
//                        int index = (int)EventTriggerListener.Get(go).GetTag();
//                        int ori_id = GetMazeFragment(drag_index);
//                        int now_id = GetMazeFragment(index);

//                        SetMazeFragment(drag_index, now_id);
//                        SetMazeFragment(index, ori_id);
//                        CheckState();
//                }
//        }

//        void SetMazeFragment(int index, int id)
//        {
//                if (id == 0)
//                {
//                        m_MazeMapObj[index].GetComponent<Image>().color = Color.white;
//                        GetText(m_MazeMapObj[index], "Text").text = "可放置";
//                }
//                else if (id == m_MazeInfo.terrainCfg.entrance_fragment) 
//                {
//                        m_MazeMapObj[index].GetComponent<Image>().color = Color.green;
//                        GetText(m_MazeMapObj[index], "Text").text = "入口";
//                }
//                else if (id == m_MazeInfo.terrainCfg.exit_fragment)  
//                {
//                        m_MazeMapObj[index].GetComponent<Image>().color = Color.green;
//                        GetText(m_MazeMapObj[index], "Text").text = "出口";
//                }                               
//                else
//                {
//                        JigsawTileConfig FragmentCfg = DiyRaidManager.GetInst().GetFragmentCfg(id);
//                        GetText(m_MazeMapObj[index], "Text").text = LanguageManager.GetText(FragmentCfg.name);
//                }
//                m_MazeMapObj[index].GetComponent<Image>().DOKill();
//                m_MazeMap[index] = id;
//        }

//        int GetMazeFragment(int index)
//        {
//                return m_MazeMap[index];
//        }

//        void SetCanUseNum()
//        {
//                GetText(MazeGroup, "can_use_num").text = now_use_num + "/" + m_MazeInfo.terrainCfg.fragment_limit;
//        }

//        private void OnTab(GameObject go, PointerEventData data)
//        {
//                FRAGMENT_TAB pt = (FRAGMENT_TAB)EventTriggerListener.Get(go).GetTag();
//                if (pt != m_tab)
//                {
//                        RefreshFragmentType(pt);
//                }
//        }

//        void RefreshFragmentType(FRAGMENT_TAB tab)
//        {
//                m_tab = tab;
//                HightLightTab(tab);
//                switch (tab)
//                {
//                        case FRAGMENT_TAB.COMMON:
//                                RefresTypeList(0);
//                                break;
//                        case FRAGMENT_TAB.OTHER:
//                                RefresTypeList(m_MazeInfo.terrainCfg.type);
//                                break;
//                        default:
//                                break;
//                }
//        }

//        void HightLightTab(FRAGMENT_TAB tab)
//        {
//                int index = (int)tab;
//                for (int i = 0; i < (int)FRAGMENT_TAB.MAX; i++)
//                {
//                        if (i == index)
//                        {
//                                GetImage(Tab_Group, "Tab" + i).color = Color.red;
//                        }
//                        else
//                        {
//                                GetImage(Tab_Group, "Tab" + i).color = Color.white;
//                        }
//                }
//        }

//        public void RefresTypeList(int type)
//        {
//                SetGameObjectHide("fragment", sr.gameObject);
//                List<RaidFragment> m_fragment_list = DiyRaidManager.GetInst().GetMyFragmentByType(type);
//                int piece_type = 0;
//                if (m_fragment_list != null)
//                {
//                        for (int i = 0; i < m_fragment_list.Count; i++)
//                        {
//                                GameObject temp = GetGameObject(sr.gameObject, "fragment" + i);
//                                if (temp == null)
//                                {
//                                        temp = CloneElement(FragmentObj, "fragment" + i);
//                                }
//                                int quality = m_fragment_list[i].FragmentCfg.quality;
//                                GetText(temp, "quality").text = quality.ToString();
//                                GetText(temp, "name").text = LanguageManager.GetText(m_fragment_list[i].FragmentCfg.name);
//                                GetText(temp, "can_use").text = m_fragment_list[i].FragmentCfg.use_number.ToString();
//                                GetText(temp, "level").text = m_fragment_list[i].level.ToString();

//                                RaidInfoRandomConfig pieceCfg = RaidConfigManager.GetInst().GetPieceConfig(m_fragment_list[i].IdConfig);
//                                if (pieceCfg != null)
//                                {
//                                        piece_type = pieceCfg.type;
//                                        GetText(temp, "type").text = RaidConfigManager.GetInst().GetRaidTypeName(piece_type);
//                                }

//                                int next_lv_need_num = DiyRaidManager.GetInst().LevelupFragmentQuantity(m_fragment_list[i]);
//                                GetText(temp, "exp").text = m_fragment_list[i].count + "/" + next_lv_need_num.ToString();
//                                if (m_fragment_list[i].count >= next_lv_need_num)
//                                {
//                                        GetButton(temp, "can_levelup").gameObject.SetActive(true);
//                                }
//                                else
//                                {
//                                        GetButton(temp, "can_levelup").gameObject.SetActive(false);
//                                }

//                                EventTriggerListener.Get(temp).onClick = OnFragmentClick;
//                                EventTriggerListener.Get(temp).onDarg = OnDarg;
//                                EventTriggerListener.Get(temp).onBeginDarg = OnBeginDrag;
//                                EventTriggerListener.Get(temp).onEndDarg = OnEndDrag;
//                                EventTriggerListener.Get(temp).SetTag(m_fragment_list[i]);
//                                temp.SetActive(true);
//                        }
//                }
//        }


//        private void OnFragmentClick(GameObject go, PointerEventData data)
//        {
//                if (!can_click)
//                {
//                        return;
//                }
//                RaidFragment fragment_info = (RaidFragment)EventTriggerListener.Get(go).GetTag();
//                if (now_use_num >= m_MazeInfo.terrainCfg.fragment_limit)
//                {
//                        GameUtility.PopupMessage("可放置拼版数已达上限！");
//                        return;
//                }

//                if (GetThisFragmentUseNum(fragment_info.IdConfig) >= fragment_info.FragmentCfg.use_number)
//                {
//                        GameUtility.PopupMessage("该拼版可使用次数达到上限！");
//                        return;
//                }

//                int index = GetEmptyIndex();  //点击默认把板块放到可放置的位置
//                SetMazeFragment(index, fragment_info.IdConfig);
//                now_use_num++;
//                SetCanUseNum();
//                CheckState();

//        }

//        int GetThisFragmentUseNum(int fragmentid) //获得某种分片的已使用次数
//        {
//                int num = 0;
//                for (int i = 0; i < m_MazeMap.Count; i++)
//                {
//                        if (m_MazeMap[i] == fragmentid)
//                        {
//                                num++;
//                        }
//                }
//                return num;
//        }


//        int GetEmptyIndex()
//        {
//                int index = 0;
//                for (int i = 0; i < m_MazeMap.Count; i++)
//                {
//                        if (m_MazeMap[i] == 0)
//                        {
//                                index = i;
//                                break;
//                        }
//                }
//                return index;
//        }

//        void FindEnterAndExit(out int enter, out int exit)
//        {
//                enter = 0;
//                exit = 0;
//                for (int i = 0; i < m_MazeMap.Count; i++)
//                {
//                        if (m_MazeMap[i] == m_MazeInfo.terrainCfg.entrance_fragment)
//                        {
//                                enter = i;
//                        }

//                        if (m_MazeMap[i] == m_MazeInfo.terrainCfg.exit_fragment)
//                        {
//                                exit = i;
//                        }
//                }
//        }


//        List<int> ActiveSuiteList = new List<int>();
//        void CheckState()
//        {
//                CheckForAlone();
//                CheckDefendterrainEffect();
//                ActiveSuiteList = CheckHasSuite();
//                ShowSuiteTip();
//        }

//        void CheckForAlone()
//        {
//                int enter = 0;
//                int exit = 0;
//                FindEnterAndExit(out enter, out exit);
//                for (int i = 0; i < m_MazeMap.Count; i++)
//                {
//                        if (m_MazeMap[i] != 0 && i != enter && i != exit)
//                        {
//                                List<int> road1 = DiyRaidManager.GetInst().FindLinkLoad(i, exit, m_MazeMap, m_MazeInfo.terrainCfg.size);
//                                List<int> road2 = DiyRaidManager.GetInst().FindLinkLoad(i, enter, m_MazeMap, m_MazeInfo.terrainCfg.size);
//                                if (road1.Count > 0 && road2.Count > 0)
//                                {
//                                        m_MazeMapObj[i].GetComponent<Image>().color = Color.yellow;
//                                        m_MazeMapObj[i].GetComponent<Image>().DOKill();
//                                }
//                                else
//                                {
//                                        m_MazeMapObj[i].GetComponent<Image>().color = Color.red;
//                                        m_MazeMapObj[i].GetComponent<Image>().DOKill();
//                                }
//                        }
//                        else
//                        {

//                                SetMazeFragment(i, m_MazeMap[i]);
//                        }
//                }
//        }

//        readonly Color[] show_color = { Color.white, Color.blue, Color.cyan, Color.grey, Color.black };
 
//        List<int> CheckHasSuite() //检测是否有套装//  以及显示套装提示//
//        {
//                List<int> m_Suite = new List<int>();
//                Dictionary<int, List<int>> m_suiteset = DiyRaidManager.GetInst().GetMySuiteSet();

//                int lead_fragment_pos_x = 0;
//                int lead_fragment_pos_y = 0;
//                int pos_x = 0;
//                int pos_y = 0;
//                List<int> lead_fragment_index = new List<int>();
//                int lead_fragment = 0;
//                int show_suite_num = 0;

//                foreach (int suiteid in m_suiteset.Keys)
//                {
//                        lead_fragment_index.Clear();
//                        lead_fragment = DiyRaidManager.GetInst().GetSuiteFragmentCfg(suiteid * 10000).fragment_id; //队长拼板
//                        for (int i = 0; i < m_MazeMap.Count; i++)
//                        {
//                                if (m_MazeMap[i] == lead_fragment)
//                                {
//                                        lead_fragment_index.Add(i);
//                                }
//                        }

//                        for (int i = 0; i < lead_fragment_index.Count; i++)
//                        {
//                                show_suite_num++;
//                                //if (m_Suite.Contains(suiteid))
//                                //{
//                                //        break;  //同一个套装效果就激活一次//
//                                //}
//                                bool is_suite = true;

//                                lead_fragment_pos_x = lead_fragment_index[i] % m_MazeInfo.terrainCfg.size;
//                                lead_fragment_pos_y = lead_fragment_index[i] / m_MazeInfo.terrainCfg.size;

//                                List<int> suite_fragment = m_suiteset[suiteid];
//                                for (int j = 0; j < suite_fragment.Count; j++)
//                                {
//                                        if (suite_fragment[j] != suiteid * 10000)
//                                        {
//                                                DiyraidSuiteFragmentConfig suitecfg = DiyRaidManager.GetInst().GetSuiteFragmentCfg(suite_fragment[j]);
//                                                pos_x = suitecfg.pos_x + lead_fragment_pos_x;
//                                                pos_y = suitecfg.pos_y + lead_fragment_pos_y;
//                                                int index = pos_x + pos_y * m_MazeInfo.terrainCfg.size;

//                                                //显示套装提示，同一套只显示一次提示//
//                                                //if (i == 0)
//                                                //{
//                                                if (pos_x < m_MazeInfo.terrainCfg.size && pos_y < m_MazeInfo.terrainCfg.size && m_MazeMap[index] == 0) //空闲的显示提示
//                                                        {
//                                                                JigsawTileConfig FragmentCfg = DiyRaidManager.GetInst().GetFragmentCfg(suitecfg.fragment_id);
//                                                                GetText(m_MazeMapObj[index], "Text").text = LanguageManager.GetText(FragmentCfg.name);
//                                                                m_MazeMapObj[index].GetComponent<Image>().color = show_color[show_suite_num];
//                                                                m_MazeMapObj[index].GetComponent<Image>().DOFade(0.2f, 1.0f).SetLoops(-1, LoopType.Yoyo);
//                                                        }
//                                                //}


//                                                if (pos_x < m_MazeInfo.terrainCfg.size && pos_y < m_MazeInfo.terrainCfg.size && m_MazeMap[index] == suitecfg.fragment_id)
//                                                {

//                                                }
//                                                else
//                                                {
//                                                        is_suite = false;
//                                                        //break;
//                                                }
//                                        }
//                                }

//                                if (is_suite)
//                                {
//                                        if (!m_Suite.Contains(suiteid))
//                                        {
//                                                m_Suite.Add(suiteid);
//                                        }
//                                }
//                        }
//                }
//                return m_Suite;
              
//        }


//        void ShowSuiteTip()
//        {
//                string tip = "";
//                for (int i = 0; i < ActiveSuiteList.Count; i++)
//                {
//                        DiyraidGlobalEffectConfig globaleffectcfg = DiyRaidManager.GetInst().GetGlobalEffectCfg(DiyRaidManager.GetInst().GetSuiteEffectCfg(ActiveSuiteList[i]).global_effect);
//                        tip += LanguageManager.GetText(globaleffectcfg.desc) + "\n";
//                }

//                if (tip.Length > 0)
//                {
//                        GetText("suite_effect").text = "激活套装特性：" + tip;
//                }
//                else
//                {
//                        GetText("suite_effect").text = "";
//                }


//        }


//        int map_effect_id = 0;
//        int map_condition_type = 0;
//        List<int> map_detail_condition_type = new List<int>();
//        List<int> map_detail_condition_num = new List<int>();
//        List<int> match_condition_num = new List<int>();
//        void InitDefendterrainEffect()
//        {

//                map_condition_type = m_MazeInfo.terrainCfg.terrain_effect_condition;
//                map_effect_id = m_MazeInfo.terrainCfg.global_effect;
//                if (map_effect_id == 0)
//                {
//                        GetText("map_effect").text = "无地形特性";
//                        return;
//                }

//                if (map_condition_type == 0)
//                {
//                        DiyraidGlobalEffectConfig globaleffectcfg = DiyRaidManager.GetInst().GetGlobalEffectCfg(map_effect_id);
//                        GetText("map_effect").text = "已激活地形特性：" + LanguageManager.GetText(globaleffectcfg.desc);
//                }
//                else
//                {
//                        DiyraidEffectConditionConfig conditioncfg = DiyRaidManager.GetInst().GetFragmentConditionCfg(map_condition_type);
//                        map_condition_type = conditioncfg.type;
//                        if (map_condition_type == 1) //需求条件
//                        {
//                                map_detail_condition_type.Clear();
//                                map_detail_condition_num.Clear();
//                                match_condition_num.Clear();
//                                string need_condition = conditioncfg.need_fragment;
//                                string[] needs = need_condition.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
//                                for (int i = 0; i < needs.Length; i++)
//                                {
//                                        string[] need = needs[i].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
//                                        map_detail_condition_type.Add(int.Parse(need[0]));
//                                        map_detail_condition_num.Add(int.Parse(need[1]));
//                                        match_condition_num.Add(0);
//                                }
//                        }
//                }
//        }

//        void CheckDefendterrainEffect()  //根据条件检测是否激活//
//        {
//                if (map_condition_type == 1)  //现在的条件是满足某种类型的分片数量的个数
//                {
//                        for (int i = 0; i < m_MazeMap.Count; i++)
//                        {
//                                if (m_MazeMap[i] != 0)
//                                {
//                                        for (int j = 0; j < map_detail_condition_type.Count; j++)
//                                        {
//                                                RaidInfoRandomConfig pieceCfg = RaidConfigManager.GetInst().GetPieceConfig(m_MazeMap[i]);
//                                                if (pieceCfg != null)
//                                                {
//                                                        if (pieceCfg.type == map_detail_condition_type[j])
//                                                        {
//                                                                match_condition_num[j]++;
//                                                        }
//                                                }
//                                        }
//                                }
//                        }

//                        bool active = true;
//                        for (int i = 0; i < map_detail_condition_num.Count; i++)
//                        {
//                                if (match_condition_num[i] < map_detail_condition_num[i])
//                                {
//                                        active = false;
//                                        break;
//                                }
//                        }

//                        DiyraidGlobalEffectConfig globaleffectcfg = DiyRaidManager.GetInst().GetGlobalEffectCfg(map_effect_id);
//                        DiyraidEffectConditionConfig conditioncfg = DiyRaidManager.GetInst().GetFragmentConditionCfg(map_condition_type);
//                        if (active)
//                        {
//                                GetText("map_effect").text = "已激活地形特性：" + LanguageManager.GetText(globaleffectcfg.desc);
//                        }
//                        else
//                        {
//                                string aString = "<color=red>未激活地形特性：" + LanguageManager.GetText(globaleffectcfg.desc) + "</color>\n";
//                                string bString = String.Format(LanguageManager.GetText(conditioncfg.effect_condition_desc), map_detail_condition_num.ToArray());
//                                GetText("map_effect").text = aString + bString;
//                        }
//                }
//        }



//        public void SetMyDefendList(string defendlist)
//        {
//                m_MazeInfo.SetPetList(defendlist);
//        }

        
//        public void Save()
//        {
//                int enter = 0;
//                int exit = 0;
//                FindEnterAndExit(out enter,out exit);
//                List<int> m_link_road = DiyRaidManager.GetInst().FindLinkLoad(enter, exit, m_MazeMap, m_MazeInfo.terrainCfg.size);
//                if (m_link_road.Count > 0)
//                {
//                        if (m_MazeInfo.petlist.Length > 0)
//                        {


//                                string link_info = GameUtility.ListToString<int>(m_link_road, '|');


//                                string suite_info = GameUtility.ListToString<int>(ActiveSuiteList, '|');
//                                if(suite_info.Length <= 0)
//                                {
//                                        suite_info = "0";
//                                }

//                                m_MazeInfo.GetNewInfo(enter, exit, m_MazeMap);
//                                DiyRaidManager.GetInst().SendDiyMapInfo(m_MazeInfo, link_info, suite_info);
//                        }
//                        else
//                        {
//                                GameUtility.PopupMessage("请在出口设置您的防守小队！");
//                        }
//                }
//                else
//                {
//                        GameUtility.PopupMessage("出口入口没有联通，无法保存！");
//                }
//        }

//        public void ChangeMap()
//        {
//                UIManager.GetInst().CloseUI(this.name);
//                UIManager.GetInst().GetUIBehaviour<UI_BuildPainting>().RefrehTerrainChoose();
//        }


//        public void Cancel()
//        {
//                UIManager.GetInst().CloseUI(this.name);
//                UIManager.GetInst().GetUIBehaviour<UI_BuildPainting>().OnReturnMain();
//        }










//}


