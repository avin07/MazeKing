using UnityEngine;
using System.Collections;
using System.Collections.Generic;
class RaidConfigManager : SingletonObject<RaidConfigManager>
{
        public RaidElemConfigDict m_RaidElemDict = new RaidElemConfigDict();
        public RaidMapDict m_RaidMapDict = new RaidMapDict();
        public Dictionary<int, RaidInfoHold> m_RaidInfoDict = new Dictionary<int, RaidInfoHold>();

        public Dictionary<int, RaidTrapEffectConfig> m_RaidTrapEffectDict = new Dictionary<int, RaidTrapEffectConfig>();
        public Dictionary<int, RaidInfoRandomConfig> m_RandomPieceInfoDict = new Dictionary<int, RaidInfoRandomConfig>();
        public Dictionary<int, RaidNodeConfig> m_RaidNodesConfigDict = new Dictionary<int, RaidNodeConfig>();
        public Dictionary<int, RaidTaskConfig> m_RaidTaskDict = new Dictionary<int, RaidTaskConfig>();
        public Dictionary<int, RaidTypeConfig> m_RaidTypeDict = new Dictionary<int, RaidTypeConfig>();
        public Dictionary<int, RaidElementOptionConfig> m_RaidElemOptionDict = new Dictionary<int, RaidElementOptionConfig>();
        public Dictionary<int, RaidAsideConfigHold> m_RaidAsideDict = new Dictionary<int, RaidAsideConfigHold>();
        public Dictionary<int, RaidElementTypeConfig> m_RaidElementTypeDict = new Dictionary<int, RaidElementTypeConfig>();
        public Dictionary<int, RaidSupporterConfig> m_RaidSupporterDict = new Dictionary<int, RaidSupporterConfig>();
        public Dictionary<int, RaidSceneConfig> m_RaidSceneDict = new Dictionary<int, RaidSceneConfig>();
        public Dictionary<int, RaidPasswordRoomConfig> m_PWRoomDict = new Dictionary<int, RaidPasswordRoomConfig>();
        public Dictionary<int, RaidConditionRoomConfig> m_ConditionRoomDict = new Dictionary<int, RaidConditionRoomConfig>();
        public Dictionary<int, RaidPieceConfig> m_RaidPieceDict = new Dictionary<int, RaidPieceConfig>();
        public Dictionary<int, RaidTalkConfig> m_RaidTalkDict = new Dictionary<int, RaidTalkConfig>();
        public Dictionary<int, RaidTalkPoolConfig> m_RaidTalkPoolDict = new Dictionary<int, RaidTalkPoolConfig>();

        public string GetRaidTalkText(int id)
        {
                if (m_RaidTalkPoolDict.ContainsKey(id))
                {
                        return m_RaidTalkPoolDict[id].text;
                }
                return "";
        }

        public RaidTalkConfig GetRaidTalkCfg(int id)
        {
                if (m_RaidTalkDict.ContainsKey(id))
                {
                        return m_RaidTalkDict[id];
                }
                return null;
        }

        public RaidPieceConfig GetExPieceCfg(int id)
        {
                if (m_RaidPieceDict.ContainsKey(id))
                {
                        return m_RaidPieceDict[id];
                }
                return null;
        }

        public RaidConditionRoomConfig GetConditionRoomCfg(int id)
        {
                if (m_ConditionRoomDict.ContainsKey(id))
                {
                        return m_ConditionRoomDict[id];
                }
                return null;
        }
        public RaidPasswordRoomConfig GetPasswordRoomCfg(int id)
        {
                if (m_PWRoomDict.ContainsKey(id))
                {
                        return m_PWRoomDict[id];
                }
                return null;
        }

        public RaidElementOptionConfig GetElemOptionCfg(int id)
        {
                if (m_RaidElemOptionDict.ContainsKey(id))
                {
                        return m_RaidElemOptionDict[id];
                }
                return null;
        }

        public RaidAsideConfigHold GetRaidAsideCfg(int id)
        {
                if (m_RaidAsideDict.ContainsKey(id))
                {
                        return m_RaidAsideDict[id];
                }
                return null;
        }

        public RaidNodeConfig GetRaidNodesConfig(int id)
        {
                if (m_RaidNodesConfigDict.ContainsKey(id))
                {
                        return m_RaidNodesConfigDict[id];

                }
                return null;
        }
        public RaidMapHold GetRaidMapCfg(int id)
        {
                if (m_RaidMapDict.ContainsKey(id))
                {
                        return m_RaidMapDict[id];
                }
                return null;
        }

        public RaidInfoHold GetRaidInfoCfg(int id)
        {
                if (m_RaidInfoDict.ContainsKey(id))
                {
                        return m_RaidInfoDict[id];
                }
                return null;
        }

        public int GetRaidMaxDifficulty(int RaidBaseId)
        {
                int id = RaidBaseId + 1;
                while (m_RaidInfoDict.ContainsKey(id))
                {
                        id++;
                }
                if (id - 1 > RaidBaseId + 4)
                {
                        return RaidBaseId + 4;
                }
                return id - 1;
        }

        public RaidSupporterConfig GetSupporterCfg(int id)
        {
                if (m_RaidSupporterDict.ContainsKey(id))
                {
                        return m_RaidSupporterDict[id];
                }
                return null;
        }

        public int GetDoorType(int pieceId)
        {
                RaidInfoRandomConfig cfg = GetPieceConfig(pieceId);
                if (cfg != null)
                {
                        RaidTypeConfig typeCfg = GetRaidTypeCfg(cfg.type);
                        if (typeCfg != null)
                        {
                                return typeCfg.door_type;
                        }
                }
                return 0;
        }

        public string GetRoomTypeIcon(int typeid)
        {
                RaidTypeConfig typeCfg = GetRaidTypeCfg(typeid);
                if (typeCfg != null)
                {
                        return typeCfg.room_type_icon;
                }
                return "";
        }

        public string GetRaidTypeName(int typeid)
        {
                RaidTypeConfig typeCfg = GetRaidTypeCfg(typeid);
                if (typeCfg != null)
                {
                        return typeCfg.mark;
                }

                return "";
        }

        public string GetRoomFinishCondition(int typeid)
        {
                RaidTypeConfig typeCfg = GetRaidTypeCfg(typeid);
                if (typeCfg != null)
                {
                        return typeCfg.condition;
                }
                return "-1";
        }
        public RaidTypeConfig GetRaidTypeCfg(int id)
        {
                if (m_RaidTypeDict.ContainsKey(id))
                {
                        return m_RaidTypeDict[id];
                }
                return null;
        }

        public RaidInfoRandomConfig GetPieceConfig(int pieceId)
        {
                if (m_RandomPieceInfoDict.ContainsKey(pieceId))
                {
                        return m_RandomPieceInfoDict[pieceId];
                }
                else
                {
                        Debug.LogError(pieceId);
                }
                return null;
        }
                        
        public RaidElemConfig GetElemCfg(int id)
        {
                if (id > 0 && m_RaidElemDict.ContainsKey(id))
                {
                        return m_RaidElemDict[id];
                }
                return null;
        }
        public RaidTrapEffectConfig GetTrapCfg(int id)
        {
                if (m_RaidTrapEffectDict.ContainsKey(id))
                {
                        return m_RaidTrapEffectDict[id];
                }
                return null;
        }

        public RaidTaskConfig GetTaskConfig(int taskId)
        {
                if (m_RaidTaskDict.ContainsKey(taskId))
                {
                        return m_RaidTaskDict[taskId];
                }
                return null;
        }

        public RaidElementTypeConfig RaidElementTypeCfg(int id)
        {
                if (m_RaidElementTypeDict.ContainsKey(id))
                {
                        return m_RaidElementTypeDict[id];
                }
                return null;
        }

        public RaidTaskConfig GetRaidMainTask()
        {
                RaidMapHold mapCfg = GetRaidMapCfg(RaidManager.GetInst().RaidID);
                if (mapCfg != null)
                {
                        RaidInfoHold cfg = GetRaidInfoCfg(mapCfg.raid);
                        if (cfg != null)
                        {
                                return GetTaskConfig(cfg.raid_task_id);
                        }
                }
                return null;
        }
        public RaidSceneConfig GetSceneCfg(int id)
        {
                if (m_RaidSceneDict.ContainsKey(id))
                {
                        return m_RaidSceneDict[id];
                }
                return null;
        }

        public void Init()
        {
                m_RaidMapDict = (ResourceManager.GetInst().Load("Config/raid_map") as raid_map).m_Dict;
                m_RaidInfoDict = (ResourceManager.GetInst().Load("Config/raid_info") as raid_info).m_Dict;             
                ConfigHoldUtility<RaidTaskConfig>.LoadXml("Config/raid_task", m_RaidTaskDict);            
                GetTowerStep();
        }

        public void InitInRaidConfig()  //部分只有迷宫需求的配置拆分到迷宫进入在加载到内存
        {
                m_RaidElemDict = (ResourceManager.GetInst().Load("Config/raid_common_element") as raid_common_element).m_Dict;
                m_RaidNodesConfigDict = (ResourceManager.GetInst().Load("Config/raidmap_random") as raidmap_random).m_Dict;

                ConfigHoldUtility<RaidTrapEffectConfig>.LoadXml("Config/raid_trap_effect", m_RaidTrapEffectDict);
                ConfigHoldUtility<RaidInfoRandomConfig>.LoadXml("Config/raidmap_random_info", m_RandomPieceInfoDict);
                ConfigHoldUtility<RaidTypeConfig>.LoadXml("Config/raid_fragment_type", m_RaidTypeDict);
                ConfigHoldUtility<RaidAsideConfigHold>.LoadXml("Config/raid_aside", m_RaidAsideDict);
                ConfigHoldUtility<RaidElementTypeConfig>.LoadXml("Config/raid_element_type", m_RaidElementTypeDict);
                ConfigHoldUtility<RaidSupporterConfig>.LoadXml("Config/raid_supporter", m_RaidSupporterDict);
                ConfigHoldUtility<RaidElementOptionConfig>.LoadXml("Config/raid_element_option", m_RaidElemOptionDict);
                ConfigHoldUtility<RaidSceneConfig>.LoadXml("Config/raid_scene", m_RaidSceneDict);
                ConfigHoldUtility<RaidPasswordRoomConfig>.LoadXml("Config/raid_password_room", m_PWRoomDict);
                ConfigHoldUtility<RaidConditionRoomConfig>.LoadXml("Config/raid_condition_room", m_ConditionRoomDict);
                ConfigHoldUtility<RaidPieceConfig>.LoadXml("Config/fragment", m_RaidPieceDict);

                ConfigHoldUtility<RaidTalkPoolConfig>.LoadXml("Config/raid_talk_pool", m_RaidTalkPoolDict);
                ConfigHoldUtility<RaidTalkConfig>.LoadXml("Config/raid_talk", m_RaidTalkDict);
        }

        public void CleanRaidConfig()
        {
                m_RaidElemDict.Clear();
                m_RaidNodesConfigDict.Clear();
                
                m_RaidTrapEffectDict.Clear();
                m_RandomPieceInfoDict.Clear();
                m_RaidTypeDict.Clear();
                m_RaidAsideDict.Clear();
                m_RaidElementTypeDict.Clear();
                m_RaidSupporterDict.Clear();
                m_RaidElemOptionDict.Clear();
                m_RaidSceneDict.Clear();
                m_PWRoomDict.Clear();
                m_ConditionRoomDict.Clear();
                m_RaidPieceDict.Clear();
                m_RaidTalkDict.Clear();
                m_RaidTalkPoolDict.Clear();
        }


        Dictionary<int, List<int>> mTowerStep = new Dictionary<int, List<int>>();  //副本踏板层

        public List<int> MyTowerStep(int raid)
        {
                return mTowerStep[raid];
        }

        void GetTowerStep()
        {
                int raid_id = 0;
                foreach (int index in m_RaidMapDict.Keys)
                {
                        raid_id = m_RaidMapDict[index].raid;
                        if (raid_id < 1000) //塔的副本id小于1000
                        {
                                if (m_RaidMapDict[index].is_jump_floor == 1) //是踏板层
                                {
                                        if (!mTowerStep.ContainsKey(raid_id))
                                        {
                                                List<int> floor_list = new List<int>();
                                                floor_list.Add(m_RaidMapDict[index].floor);
                                                mTowerStep.Add(raid_id, floor_list);
                                        }
                                        else
                                        {
                                                mTowerStep[raid_id].Add(m_RaidMapDict[index].floor);
                                        }
                                }
                        }
                }
        }

}
