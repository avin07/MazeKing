//using UnityEngine;
//using System.Collections;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using System.Collections.Generic;
//using System;

//public class UI_BuildPainting : UI_ScrollRectHelp
//{

//        public GameObject Tab_Group;
//        public GameObject Main_Group;
//        public GameObject TerrainChoose;
//        public GameObject paper;

//        public enum PAINTING_TAB
//        {
//                MAZE = 0,          //迷宫
//                TERRAIN = 1,       //图纸地形
//                FRAGMENT = 2,      //板块
//                MAX = 3,

//        }
//        public GameObject[] Groups = new GameObject[(int)PAINTING_TAB.MAX];  //存储,防止经常查询//
//        PAINTING_TAB m_tab = PAINTING_TAB.MAZE;

//        void Awake()
//        {
//                InitBase();
//                InitMaze();
//        }

//        void InitBase()
//        {
//                for (int i = 0; i < (int)PAINTING_TAB.MAX; i++)
//                {
//                        if (i == 0)
//                        {
//                                Groups[i].SetActive(true);
//                        }
//                        else
//                        {
//                                Groups[i].SetActive(false);
//                        }
//                        Button btn = GetButton(Tab_Group, "Tab" + i);
//                        EventTriggerListener.Get(btn.gameObject).onClick = OnTab;
//                        EventTriggerListener.Get(btn.gameObject).SetTag(i);
//                }
//                Main_Group.SetActive(true);
//                TerrainChoose.SetActive(false);
//                paper.SetActive(false);
//        }

//        public void Refresh()
//        {
//                UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", false);
//                RefreshGroup(PAINTING_TAB.MAZE);
//        }

//        private void OnTab(GameObject go, PointerEventData data)
//        {
//                PAINTING_TAB pt = (PAINTING_TAB)EventTriggerListener.Get(go).GetTag();
//                if (pt != m_tab)
//                {
//                        RefreshGroup(pt);
//                }
//        }

//        void HightLightTab(PAINTING_TAB tab)
//        {
//                int index = (int)tab;
//                for (int i = 0; i < (int)PAINTING_TAB.MAX; i++)
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

//        void RefreshGroup(PAINTING_TAB tab)
//        {
//                m_tab = tab;
//                HightLightTab(tab);
//                for (int i = 0; i < (int)PAINTING_TAB.MAX; i++)
//                {
//                        if ((int)i == (int)tab)
//                        {
//                                Groups[i].SetActive(true);
//                        }
//                        else
//                        {
//                                Groups[i].SetActive(false);
//                        }
//                }
//                switch (tab)
//                {
//                        case PAINTING_TAB.MAZE:
//                                RefreshMaze();
//                                break;
//                        case PAINTING_TAB.TERRAIN:
//                                break;
//                        case PAINTING_TAB.FRAGMENT:
//                                break;
//                        default:
//                                break;
//                }
//        }

//        readonly int Maze_Num = 3;
//        int m_select_index = 0;   //当前选中的迷宫序号


//        void InitMaze()
//        {
//                for (int i = 1; i < Maze_Num + 1; i++)
//                {
//                        Button maze = GetButton(Groups[0], "maze" + i);
//                        EventTriggerListener.Get(maze.gameObject).onClick = OnClickMaze;
//                        EventTriggerListener.Get(maze.gameObject).SetTag(i);                   
//                }
//        }

//        public void RefreshMaze()
//        {
//                int painting_build_lvl = PlayerController.GetInst().GetPropertyInt("diy_raid_defend_lvl");
//                int m_maze_num = DiyRaidManager.GetInst().GetSchemeMazeNum(painting_build_lvl);
//                int now_use_index = PlayerController.GetInst().GetPropertyInt("diy_raid_enable_idx");
//                m_select_index = now_use_index;
//                for (int i = 1; i < Maze_Num + 1; i++)
//                {
//                        MazeInfo m_maze_info = DiyRaidManager.GetInst().GetMyMazeInfo(i);
//                        Button maze = GetButton(Groups[0], "maze" + i);
//                        EventTriggerListener.Get(maze.gameObject).onClick = OnClickMaze;
//                        EventTriggerListener.Get(maze.gameObject).SetTag(i);
//                        if (i < m_maze_num + 1)
//                        {
//                                maze.interactable = true;
//                                if (m_maze_info == null)
//                                {
//                                        GetText(maze.gameObject, "Text").text = "空闲";
//                                }
//                                else
//                                {
//                                        GetText(maze.gameObject, "Text").text = "可编辑";
//                                }
//                        }
//                        else
//                        {
//                                maze.interactable = false;
//                                GetText(maze.gameObject, "Text").text = "未激活";
//                        }

//                        if (now_use_index == i)
//                        {
//                                GetImage(Groups[0], "select").rectTransform.anchoredPosition = (maze.transform as RectTransform).anchoredPosition;
//                                GetText(maze.gameObject, "Text").text = "启用中";
//                        }
//                }
//                if (now_use_index == 0)
//                {
//                        GetButton(Groups[0], "use").gameObject.SetActive(true);
//                        m_select_index = 1;
//                }
//                else
//                {
//                        GetButton(Groups[0], "use").gameObject.SetActive(false);
//                }
//        }

//        private void OnClickMaze(GameObject go, PointerEventData data)
//        {
//                if (go.GetComponent<Button>().interactable)
//                {
//                        m_select_index = (int)EventTriggerListener.Get(go).GetTag();
//                        GetImage(Groups[0], "select").rectTransform.anchoredPosition = (go.transform as RectTransform).anchoredPosition;

//                        if (m_select_index != PlayerController.GetInst().GetPropertyInt("diy_raid_enable_idx"))
//                        {
//                                GetButton(Groups[0], "use").gameObject.SetActive(true);
//                        }
//                        else
//                        {
//                                GetButton(Groups[0], "use").gameObject.SetActive(false);
//                        }
//                }
//        }

//        public void SendUseMaze()
//        {
//                if (DiyRaidManager.GetInst().GetMyMazeInfo(m_select_index) != null)
//                {
//                        DiyRaidManager.GetInst().SendUseDiyMapIndex(m_select_index);
//                }
//                else
//                {
//                        GameUtility.PopupMessage("请先建造好迷宫再启用！");
//                }

//        }

//        public void EditorMaze()
//        {
//                //选择图纸
//                MazeInfo m_maze_info = DiyRaidManager.GetInst().GetMyMazeInfo(m_select_index);
//                if (m_maze_info == null)
//                {
//                        RefrehTerrainChoose();
//                }
//                else
//                {
//                        //直接进入编辑界面
//                        UIManager.GetInst().ShowUI<UI_DiyMazeEditor>("UI_DiyMazeEditor").Refresh(m_maze_info);
//                }
//        }

//        public void RefrehTerrainChoose()
//        {
//                gameObject.SetActive(true);
//                sr = GetGameObject(TerrainChoose, "ScrollRect").GetComponent<ScrollRect>();
//                Main_Group.SetActive(false);
//                TerrainChoose.SetActive(true);
//                int painting_build_lvl = PlayerController.GetInst().GetPropertyInt("diy_raid_defend_lvl");
//                string m_terrain = DiyRaidManager.GetInst().GetSchemeMazeTerrain(painting_build_lvl);

//                string[] terrains = m_terrain.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
//                GameObject paper_obj = null;
//                for (int i = 0; i < terrains.Length; i++)
//                {
//                        paper_obj = GetGameObject(TerrainChoose, "paper" + i);
//                        if (paper_obj == null)
//                        {
//                                paper_obj = CloneElement(paper, "paper" + i);
//                                EventTriggerListener.Get(paper_obj).onClick = OnPaperClick;
//                                EventTriggerListener.Get(paper_obj).onDarg = OnDarg;
//                                EventTriggerListener.Get(paper_obj).onBeginDarg = OnBeginDrag;
//                                EventTriggerListener.Get(paper_obj).onEndDarg = OnEndDrag;
//                        }
//                        paper_obj.SetActive(true);
//                        int id = int.Parse(terrains[i]);
//                        EventTriggerListener.Get(paper_obj).SetTag(id);
//                        GetText(paper_obj,"Text").text = LanguageManager.GetText(DiyRaidManager.GetInst().GetDefendterrainCfg(id).name);
//                }
//        }


//        private void OnPaperClick(GameObject go, PointerEventData data)
//        {
//                int id = (int)EventTriggerListener.Get(go).GetTag();
//                MazeInfo m_maze_info = DiyRaidManager.GetInst().GetMyMazeInfo(m_select_index);
//                if (m_maze_info != null)
//                {
//                        if (id == m_maze_info.terrainCfg.id)  //还是这个图纸
//                        {
//                                UIManager.GetInst().ShowUI<UI_DiyMazeEditor>("UI_DiyMazeEditor").Refresh(m_maze_info);
//                                return;
//                        }
//                }

//                UIManager.GetInst().ShowUI<UI_DiyMazeEditor>("UI_DiyMazeEditor").Refresh(new MazeInfo(m_select_index, id));
                        
               
//        }

//        public void OnReturnMain()
//        {
//                gameObject.SetActive(true);
//                Main_Group.SetActive(true);
//                TerrainChoose.SetActive(false);
//                RefreshMaze();
//        }

//        public void OnClickClose()
//        {
//                UIManager.GetInst().CloseUI(this.name);
//                UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", true);
//        }





//}

