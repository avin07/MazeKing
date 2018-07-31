using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Message;


class SkillManager : SingletonObject<SkillManager>
{
        Dictionary<int, SkillConfig> m_ActiveSkillDict = new Dictionary<int, SkillConfig>();
        Dictionary<int, PassiveSkillConfig> m_PassiveSkillDict = new Dictionary<int, PassiveSkillConfig>();
        Dictionary<int, SkillLearnConfigHold> m_SkillLearnDict = new Dictionary<int, SkillLearnConfigHold>();
        Dictionary<int, SkillUpgradeConfig> m_SkillUpDict = new Dictionary<int, SkillUpgradeConfig>();

        Dictionary<int, SkillBuffConfig> m_BuffDict = new Dictionary<int, SkillBuffConfig>();
        Dictionary<int, SkillMoveConfig> m_SkillMoveDict = new Dictionary<int, SkillMoveConfig>();
        Dictionary<int, SkillSummonConfig> m_SkillSummonDict = new Dictionary<int, SkillSummonConfig>();
        Dictionary<int, SkillTriggerConfig> m_SkillTriggerDict = new Dictionary<int, SkillTriggerConfig>();

        Dictionary<int, AdventureSkillConfig> m_AdvSkillDict = new Dictionary<int, AdventureSkillConfig>();

        public void Init()
        {
                ConfigHoldUtility<SkillConfig>.LoadXml("Config/active_skill", m_ActiveSkillDict);          //主动技能
                ConfigHoldUtility<SkillLearnConfigHold>.LoadXml("Config/skill", m_SkillLearnDict);   //技能总表（技能的公用属性名字，描述，icon，类型等）
                ConfigHoldUtility<SkillUpgradeConfig>.LoadXml("Config/skill_upgrade", m_SkillUpDict);

                ConfigHoldUtility<SkillBuffConfig>.LoadXml("Config/buff", m_BuffDict);
                ConfigHoldUtility<SkillTriggerConfig>.LoadXml("Config/battle_trigger", m_SkillTriggerDict);

                ConfigHoldUtility<AdventureSkillConfig>.LoadXml("Config/raid_adventure_skill", m_AdvSkillDict);//冒险技能

                ConfigHoldUtility<PassiveSkillConfig>.LoadXml("Config/passive_skill", m_PassiveSkillDict);//被动技能

                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgPetSkillAll), OnGetPetSkillAll);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgPetSkillOne), OnGetPetSkillOne);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgSkillReset), OnPetSkillReset);
        }

        void OnGetPetSkillAll(object sender, SCNetMsgEventArgs e)
        {
                SCMsgPetSkillAll msg = e.mNetMsg as SCMsgPetSkillAll; //info: id&idConfig&level|id&idConfig&level|...
                string[] skill_info = msg.skill_info.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < skill_info.Length; i++)
                {
                        SetPetSkill(skill_info[i]);
                }
        }

        void OnGetPetSkillOne(object sender, SCNetMsgEventArgs e)
        {
                SCMsgPetSkillOne msg = e.mNetMsg as SCMsgPetSkillOne;
                SetPetSkill(msg.skill_info);

        }
        void OnPetSkillReset(object sender, SCNetMsgEventArgs e)
        {
                SCMsgSkillReset msg = e.mNetMsg as SCMsgSkillReset;
                Pet pet = PetManager.GetInst().GetPet(msg.idPet);
                if (pet != null)
                {
                        pet.ResetSkills();
                        UI_SkillUpgrade uis = UIManager.GetInst().GetUIBehaviour<UI_SkillUpgrade>();
                        if (uis != null)
                        {
                                uis.RefreshHero();
                        }
                }
        }

        void SetPetSkill(string skill_info)
        {
                string[] info = skill_info.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                if (info.Length == 3)
                {
                        long pet_id = long.Parse(info[0]) / 100;
                        Pet pet = PetManager.GetInst().GetPet(pet_id);
                        if (pet == null)
                        {
                                Debuger.Log("为什么没有这个英雄！" + pet_id);
                        }
                        else
                        {
                                Skill_Struct ss;
                                ss.id = long.Parse(info[0]);
                                ss.config_id = int.Parse(info[1]);
                                ss.level = int.Parse(info[2]);
                                pet.SetMySkill(ss);
                                UI_SkillUpgrade uis = UIManager.GetInst().GetUIBehaviour<UI_SkillUpgrade>();
                                if (uis != null)
                                {
                                        uis.RefreshHero();
                                }
                        }
                }
                else
                {
                        Debuger.Log("数据格式不对");
                }
        }


        public SkillConfig GetActiveSkill(int id)
        {
                if (m_ActiveSkillDict.ContainsKey(id))
                {
                        return m_ActiveSkillDict[id];
                }
                return null;
        }

        public PassiveSkillConfig GetPassiveSkill(int id)
        {
                if (m_PassiveSkillDict.ContainsKey(id))
                {
                        return m_PassiveSkillDict[id];
                }
                return null;
        }

        public SkillBuffConfig GetBuff(int id)
        {
                if (m_BuffDict.ContainsKey(id))
                {
                        return m_BuffDict[id];
                }
                return null;
        }
        public SkillMoveConfig GetMoveCfg(int id)
        {
                if (m_SkillMoveDict.ContainsKey(id))
                {
                        return m_SkillMoveDict[id];
                }
                return null;
        }
        public SkillSummonConfig GetSummonCfg(int id)
        {
                if (m_SkillSummonDict.ContainsKey(id))
                {
                        return m_SkillSummonDict[id];
                }
                return null;
        }
        public SkillTriggerConfig GetTriggerCfg(int id)
        {
                if (m_SkillTriggerDict.ContainsKey(id))
                {
                        return m_SkillTriggerDict[id];
                }
                return null;
        }

        public SkillLearnConfigHold GetSkillInfo(int id)
        {
                if (m_SkillLearnDict.ContainsKey(id))
                {
                        return m_SkillLearnDict[id];
                }
                return null;
        }

        public string GetSkillName(int skill_id)
        {
                if (m_SkillLearnDict.ContainsKey(skill_id))
                {
                        return LanguageManager.GetText(m_SkillLearnDict[skill_id].name);
                }
                return "";
        }


        public string GetSkillDesc(int skill_id, int level = 1)
        {
                if (m_SkillLearnDict.ContainsKey(skill_id))
                {
                        string key = m_SkillLearnDict[skill_id].desc + "_" + level.ToString();

                        if (LanguageManager.HasText(key))
                        {
                                return LanguageManager.GetText(key);
                        }
                        else
                        {
                                return LanguageManager.GetText(m_SkillLearnDict[skill_id].desc);
                        }
                }
                return "";
        }

        public string GetSkillIconUrl(int skill_id)
        {
                if (m_SkillLearnDict.ContainsKey(skill_id))
                {
                        return m_SkillLearnDict[skill_id].icon;
                }
                else
                {
                        Debuger.Log(skill_id + " icon=null");
                }
                return "";
        }

        public int GetSkillMaxLevel(int skill_id, int star)
        {
                if (m_SkillLearnDict.ContainsKey(skill_id))
                {
                        return m_SkillLearnDict[skill_id].max_level[star - 1];
                }
                return 1;
        }

        public int GetSkillLevelUpByStar(int star0, int star1, int skill_id)
        {
                if (m_SkillLearnDict.ContainsKey(skill_id))
                {
                        return m_SkillLearnDict[skill_id].max_level[star1 - 1] - m_SkillLearnDict[skill_id].max_level[star0 - 1];
                }
                return 0;
        }

        public AdventureSkillConfig GetAdvSkillCfg(int cfgId)
        {
                if (m_AdvSkillDict.ContainsKey(cfgId))
                {
                        return m_AdvSkillDict[cfgId];
                }
                return null;
        }
        public int GetSkillUpgradeCost(int target_level, int quality)
        {
                if (m_SkillUpDict.ContainsKey(target_level))
                {
                        if (quality < m_SkillUpDict[target_level].quality_cost_gold.Count)
                        {
                                return m_SkillUpDict[target_level].quality_cost_gold[quality];
                        }
                }
                return 0;
        }
}
