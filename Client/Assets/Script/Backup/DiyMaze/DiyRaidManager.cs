//using UnityEngine;
//using UnityEngine.EventSystems;
//using System.Collections;
//using System.Collections.Generic;
//using System;
//using Message;

//public class DiyRaidManager : SingletonObject<DiyRaidManager>
//{
//        public Dictionary<int, BuildingSchemeDefendConfig> m_SchemeConfig = new Dictionary<int, BuildingSchemeDefendConfig>();
//        public Dictionary<int, JigsawTileConfig> m_FragmentConfig = new Dictionary<int, JigsawTileConfig>();
//        public Dictionary<int, JigsawTileUpgradeConfig> m_FragmentUpgradeConfig = new Dictionary<int, JigsawTileUpgradeConfig>();
//        public Dictionary<int, DiyRaidDefendterrainAttrConfig> m_DefendterrainConfig = new Dictionary<int, DiyRaidDefendterrainAttrConfig>();
//        public Dictionary<int, DiyraidSuiteFragmentConfig> m_SuiteFragmentConfig = new Dictionary<int, DiyraidSuiteFragmentConfig>();
//        public Dictionary<int, DiyraidSuiteEffectConfig> m_SuiteEffectConfig = new Dictionary<int, DiyraidSuiteEffectConfig>();
//        public Dictionary<int, List<int>> m_SuiteSet = new Dictionary<int, List<int>>();
//        public Dictionary<int, DiyraidEffectConditionConfig> m_DefendterrainConditionConfig = new Dictionary<int, DiyraidEffectConditionConfig>();
//        public Dictionary<int, DiyraidGlobalEffectConfig> m_GlobalEffectConfig = new Dictionary<int, DiyraidGlobalEffectConfig>();
        
//        public void Init()
//        {
//                ConfigHoldUtility<BuildingSchemeDefendConfig>.LoadXml("Config/building_scheme_defend", m_SchemeConfig);                                 //绘图板建筑
//                ConfigHoldUtility<JigsawTileConfig>.LoadXml("Config/jigsaw_tile", m_FragmentConfig);                                                    //拼板
//                ConfigHoldUtility<JigsawTileUpgradeConfig>.LoadXml("Config/jigsaw_tile_upgrade", m_FragmentUpgradeConfig);                              //拼板升级经验
//                ConfigHoldUtility<DiyRaidDefendterrainAttrConfig>.LoadXml("Config/diyraid_defendterrain_attr", m_DefendterrainConfig);                  //图纸
//                ConfigHoldUtility<DiyraidSuiteFragmentConfig>.LoadXml("Config/diyraid_suite_fragment", m_SuiteFragmentConfig);                          //套装位置
//                ConfigHoldUtility<DiyraidSuiteEffectConfig>.LoadXml("Config/diyraid_suite_effect", m_SuiteEffectConfig);                                //套装效果
//                ConfigHoldUtility<DiyraidEffectConditionConfig>.LoadXml("Config/diyraid_effect_condition", m_DefendterrainConditionConfig);             //图纸激活条件
//                ConfigHoldUtility<DiyraidGlobalEffectConfig>.LoadXml("Config/diyraid_global_effect", m_GlobalEffectConfig);                             //效果表
//                PrepareSuiteFragment();
//                InitMsg();
//        }


//        void PrepareSuiteFragment()
//        {
//                foreach (int id in m_SuiteFragmentConfig.Keys)
//                {
//                        int suite_id = m_SuiteFragmentConfig[id].suite_id;
//                        if (m_SuiteSet.ContainsKey(suite_id))
//                        {
//                                m_SuiteSet[suite_id].Add(id);
//                        }
//                        else
//                        {
//                                List<int> temp = new List<int>();
//                                temp.Add(id);
//                                m_SuiteSet.Add(suite_id, temp);
//                        }
//                }
//        }

//        public List<int> GetFragmentBySuite(int suite_id)
//        {
//                if (m_SuiteSet.ContainsKey(suite_id))
//                {
//                        return m_SuiteSet[suite_id];
//                }
//                return null;
//        }

//        public Dictionary<int, List<int>> GetMySuiteSet()
//        {
//                return m_SuiteSet;
//        }


//        public BuildingSchemeDefendConfig GetSchemeCfg(int level)
//        {
//                foreach (int id in m_SchemeConfig.Keys)
//                {
//                        if (id % 100 == level)
//                        {
//                                return m_SchemeConfig[id];
//                        }
//                }
//                return null;
//        }

//        public int GetSchemeMazeNum(int level)
//        {
//                BuildingSchemeDefendConfig temp = GetSchemeCfg(level);
//                if (temp != null)
//                {
//                        return temp.raid_file_number;
//                }
//                return 0;
//        }

//        public string GetSchemeMazeTerrain(int level)
//        {
//                BuildingSchemeDefendConfig temp = GetSchemeCfg(level);
//                if (temp != null)
//                {
//                        return temp.unlock_defendterrain;
//                }
//                return "";
//        }

//        public int CanLevelUpLimitByQuality(int level,int quality)
//        {
//                BuildingSchemeDefendConfig temp = GetSchemeCfg(level);
//                if (temp != null)
//                {
//                        return temp.GetLimitByQuality(quality);
//                }
//                return 0;
//        }


//        public JigsawTileConfig GetFragmentCfg(int id)
//        {
//                if (m_FragmentConfig.ContainsKey(id))
//                {
//                        return m_FragmentConfig[id];
//                }
//                return null;
//        }

//        public List<RaidFragment> GetMyFragmentByType(int type)
//        {
//                List<RaidFragment> fragmentList = new List<RaidFragment>();
//                foreach (long id in m_FragmentDic.Keys)
//                {
//                        if (m_FragmentDic[id].FragmentCfg.type == type)
//                        {
//                                fragmentList.Add(m_FragmentDic[id]);
//                        }
//                }
//                fragmentList.Sort(CompareFragment);
//                return fragmentList;
//        }

//        protected int CompareFragment(RaidFragment fragment_a, RaidFragment fragment_b)
//        {
//                int levela = fragment_a.level;
//                int levelb = fragment_b.level;
//                if (levela != levelb)
//                {
//                        return levela - levelb;
//                }
//                return fragment_a.IdConfig - fragment_b.IdConfig;
//        }

//        public JigsawTileUpgradeConfig GetFragmentUpgradeCfg(int id)
//        {
//                if (m_FragmentUpgradeConfig.ContainsKey(id))
//                {
//                        return m_FragmentUpgradeConfig[id];
//                }
//                return null;
//        }


//        public int LevelupFragmentGold(RaidFragment fragment)
//        {
//                JigsawTileUpgradeConfig temp = GetFragmentUpgradeCfg(fragment.level + 1);
//                if (temp != null)
//                {
//                        return temp.GetReqGoldByQuality(fragment.FragmentCfg.quality);
//                }
//                return 0;

//        }

//        public int LevelupFragmentQuantity(RaidFragment fragment)
//        {
//                JigsawTileUpgradeConfig temp = GetFragmentUpgradeCfg(fragment.level + 1);
//                if (temp != null)
//                {
//                        return temp.GetReqQuantityByQuality(fragment.FragmentCfg.quality);
//                }
//                return 0;
//        }


//        public DiyRaidDefendterrainAttrConfig GetDefendterrainCfg(int id)
//        {
//                if (m_DefendterrainConfig.ContainsKey(id))
//                {
//                        return m_DefendterrainConfig[id];
//                }
//                return null;
//        }

//        public DiyraidSuiteFragmentConfig GetSuiteFragmentCfg(int id)
//        {
//                if (m_SuiteFragmentConfig.ContainsKey(id))
//                {
//                        return m_SuiteFragmentConfig[id];
//                }
//                return null;
//        }

//        public DiyraidSuiteEffectConfig GetSuiteEffectCfg(int id)
//        {
//                if (m_SuiteEffectConfig.ContainsKey(id))
//                {
//                        return m_SuiteEffectConfig[id];
//                }
//                return null;
//        }

//        public DiyraidEffectConditionConfig GetFragmentConditionCfg(int id)
//        {
//                if (m_DefendterrainConditionConfig.ContainsKey(id))
//                {
//                        return m_DefendterrainConditionConfig[id];
//                }
//                return null;
//        }

//        public DiyraidGlobalEffectConfig GetGlobalEffectCfg(int id)
//        {
//                if (m_GlobalEffectConfig.ContainsKey(id))
//                {
//                        return m_GlobalEffectConfig[id];
//                }
//                return null;
//        }

//        public List<int> FindLinkLoad(int enterIdx, int exitIdx, List<int> m_map, int size) //只要找出联通路就行了，并不要求最短路劲
//        {
//                List<int> m_road = new List<int>();
//                List<int> m_checked_index = new List<int>();
//                m_road.Add(enterIdx);
//                int check_index = m_road[m_road.Count - 1];
//                bool isLink = false;
//                int next_index = -1;

//                while (check_index != exitIdx)
//                {
//                        for (int i = 0; i < 4; i++)        //上，右，下，左
//                        {

//                                if ((check_index + 1) % size == 0)
//                                {
//                                        if (i == 1)
//                                        {
//                                                continue;
//                                        }
//                                }

//                                if (check_index % size == 0)
//                                {
//                                        if (i == 3)
//                                        {
//                                                continue;
//                                        }
//                                }


//                                switch (i)
//                                {
//                                        case 0:
//                                                next_index = check_index + size;
//                                                break;
//                                        case 1:
//                                                next_index = check_index + 1;
//                                                break;
//                                        case 2:
//                                                next_index = check_index - size;
//                                                break;
//                                        case 3:
//                                                next_index = check_index - 1;
//                                                break;
//                                }

//                                if (next_index >= size * size || next_index < 0)
//                                {
//                                        continue;
//                                }

//                                if (m_road.Contains(next_index) || m_checked_index.Contains(next_index))
//                                {
//                                        continue;
//                                }


//                                if (m_map[next_index] != 0)
//                                {
//                                        isLink = true;
//                                        break;
//                                }


//                        }

//                        if (isLink)
//                        {
//                                m_road.Add(next_index);
//                        }
//                        else
//                        {
//                                m_checked_index.Add(m_road[m_road.Count - 1]);
//                                m_road.RemoveAt(m_road.Count - 1);
//                                if (m_road.Count == 0)
//                                {
//                                        break;
//                                }

//                        }
//                        check_index = m_road[m_road.Count - 1];
//                        isLink = false;
//                }
//                return m_road;
                
//        }


//        public void InitMsg()
//        {
//                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgFragment), OnGetFragment);
//                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgDiyMapSaveSuc), OnGetDiyMazeSaveSuc);
//                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgDefenseMap), OnGetMazeInfo);

//        }


//        Dictionary<long, RaidFragment> m_FragmentDic = new Dictionary<long, RaidFragment>();

//        public MazeInfo m_UseMazeInfo;


//        void OnGetFragment(object sender, SCNetMsgEventArgs e)
//        {
//                SCMsgFragment msg = e.mNetMsg as SCMsgFragment;
//                if (m_FragmentDic.ContainsKey(msg.id))
//                {
//                        m_FragmentDic[msg.id].ChangeFragment(msg);
//                }
//                else
//                {
//                        m_FragmentDic.Add(msg.id, new RaidFragment(msg));
//                }
//        }


//        void OnGetDiyMazeSaveSuc(object sender, SCNetMsgEventArgs e)                //保存成功客户端自己变换数据
//        {
//                SCMsgDiyMapSaveSuc msg = e.mNetMsg as SCMsgDiyMapSaveSuc;
//                int index = msg.index;
//                if (m_UseMazeInfo.index == index)
//                {
//                        m_MazeInfo[index] = m_UseMazeInfo;
//                        GameUtility.PopupMessage("自建迷宫" + index + "保存成功");
//                        UIManager.GetInst().GetUIBehaviour<UI_DiyMazeEditor>().Cancel();
//                }
//                else
//                {
//                        GameUtility.PopupMessage("客户端保存的和服务器保存的迷宫不同！！！");
//                }

//        }

//        Dictionary<int, MazeInfo> m_MazeInfo = new Dictionary<int, MazeInfo>();
//        void OnGetMazeInfo(object sender, SCNetMsgEventArgs e)
//        {
//                SCMsgDefenseMap msg = e.mNetMsg as SCMsgDefenseMap;
//                int index = msg.index;
//                MazeInfo info = new MazeInfo(msg);
//                if (m_MazeInfo.ContainsKey(index))
//                {
//                        m_MazeInfo[index] = info;
//                }
//                else
//                {
//                        m_MazeInfo.Add(index, info);
//                }
//        }

//        public MazeInfo GetMyMazeInfo(int index)
//        {
//                if (m_MazeInfo.ContainsKey(index))
//                {
//                        return m_MazeInfo[index];
//                }
//                return null;
//        }

//        public HashSet<long> GetDiyMazeUsedPet()
//        {
//                HashSet<long> PetSet = new HashSet<long>();
//                foreach(MazeInfo info in m_MazeInfo.Values)
//                {
//                        if(info != null)
//                        {
//                                string petlist = info.petlist;
//                                string[] pet = petlist.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
//                                for(int i = 0; i < pet.Length; i++)
//                                {
//                                        long id = long.Parse(pet[i]);
//                                        if(id != 0)
//                                        {
//                                                if (!PetSet.Contains(id))
//                                                {
//                                                        PetSet.Add(id);
//                                                }
//                                        }
//                                }

//                        }

//                }
//                return PetSet;
//        }


//        public void SendFragmentUpgrade(int id)
//        {
//                CSMsgFragmentUpgrade msg = new CSMsgFragmentUpgrade();
//                msg.id = id;
//                NetworkManager.GetInst().SendMsgToServer(msg);
//        }

//        public void SendUseDiyMapIndex(int index)
//        {
//                CSMsgDiyMapEnable msg = new CSMsgDiyMapEnable();
//                msg.index = index;
//                NetworkManager.GetInst().SendMsgToServer(msg);
//        }

//        public void SendDiyMapInfo(MazeInfo info, string strEnterToExit, string strSuitEffect)
//        {
//                CSMsgDiyMapSave msg = new CSMsgDiyMapSave();
//                if (info != null)
//                {
//                        msg.saveIndex = info.index;
//                        msg.idTerrain = info.terrainCfg.id;
//                        msg.idEnterIdx = info.enter_index;
//                        msg.idExitIdx = info.exit_index;
//                        string fragmentinfo = GameUtility.ListToString<int>(info.MapFragment, '|'); 
//                        msg.strFragmentInfo = fragmentinfo;
//                        msg.strEnterToExit = strEnterToExit;
//                        msg.strSuitEffect = strSuitEffect;
//                        msg.strPetlist = info.petlist;
//                        NetworkManager.GetInst().SendMsgToServer(msg);
//                }
//        }


//        public void TestDiyRaid()
//        {
//                CSMsgAckDefendDiyMap msg = new CSMsgAckDefendDiyMap();
//                string petlist = "";
//                List<Pet> m_petlist = PetManager.GetInst().GetMyPetList();
//                for (int i = 0; i < 6; i++)
//                {
//                        if (i < m_petlist.Count)
//                        {
//                                petlist += m_petlist[i].ID.ToString() + "|";
//                        }
//                        else
//                        {
//                                petlist += "0|";        
//                        }
//                }
//                msg.petlist = petlist;
//                NetworkManager.GetInst().SendMsgToServer(msg);
//        }

//}


//public struct RaidFragment  //自制迷宫板块
//{
//        public long id;
//        public int IdConfig;
//        public int level;
//        public int count;
//        public JigsawTileConfig FragmentCfg;

//        public RaidFragment(SCMsgFragment msg)
//        {
//                id = msg.id;
//                IdConfig = msg.idConfig;
//                level = msg.nLevel;
//                count = msg.nCount;
//                FragmentCfg = DiyRaidManager.GetInst().GetFragmentCfg(IdConfig);
//        }

//        public void ChangeFragment(SCMsgFragment msg)
//        {               
//                this.level = msg.nLevel;
//                this.count = msg.nCount;
//        }

//}


//public class MazeInfo  //自制迷宫板块
//{
//        public int index;
//        public int enter_index;
//        public int exit_index;
//        public List<int> MapFragment;
//        public string petlist;
//        public DiyRaidDefendterrainAttrConfig terrainCfg;

//        public MazeInfo(SCMsgDefenseMap msg)
//        {
//                index = msg.index;
//                terrainCfg = DiyRaidManager.GetInst().GetDefendterrainCfg(msg.idterrain);
//                enter_index = msg.enter_index;
//                exit_index = msg.exit_index;

//                MapFragment = new List<int>();
//                string[] info = msg.fraginfo.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
//                for (int i = 0; i < info.Length; i++)
//                {
//                        string[] temp = info[i].Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
//                        int id = (int)(int.Parse(temp[1]) * 0.01f);
//                        MapFragment.Add(id);
//                }

//                petlist = msg.petlist;
//        }


//        public MazeInfo(int index, int idterrain)
//        {
//                this.index = index;
//                terrainCfg = DiyRaidManager.GetInst().GetDefendterrainCfg(idterrain);
//                int all_index = terrainCfg.size * terrainCfg.size;
//                enter_index = 0;
//                exit_index = all_index - 1;

//                MapFragment = new List<int>();
//                for (int i = 0; i < all_index; i++)
//                {
//                        if (i == enter_index)
//                        {
//                                MapFragment.Add(terrainCfg.entrance_fragment);
//                        }
//                        else if (i == exit_index)
//                        {
//                                MapFragment.Add(terrainCfg.exit_fragment);
//                        }
//                        else
//                        {
//                                MapFragment.Add(0);
//                        }
//                }

//                petlist = "";
//        }

//        public void SetPetList(string defendlist)
//        {
//                petlist = defendlist;
//        }

//        public void GetNewInfo(int enter, int exit, List<int> map)
//        {
//                enter_index = enter;
//                exit_index = exit;
//                MapFragment = map;
//        }

        


//}
