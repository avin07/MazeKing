using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Message;
using System;
public class Pet : PropertyBase
{
        FighterProperty m_FighterProp;
        public FighterProperty FighterProp
        {
                get
                {
                        return m_FighterProp;
                }
        }

        protected int m_CharacterId;
        public int CharacterID
        {
                get
                {
                        return m_CharacterId;
                }
        }

        public CharacterConfig m_CharacterCfg
        {
                get
                {
                        return CharacterManager.GetInst().GetCharacterCfg(CharacterID);
                }
        }
        List<Skill_Struct> m_SkillList = new List<Skill_Struct>();
        Dictionary<ItemType.EQUIP_PART, Equip> m_Equip = new Dictionary<ItemType.EQUIP_PART, Equip>();

        void InitPropertyHandlers()
        {
                m_PropertyHandlers.Add("cur_state", OnStateUpdate);
                m_PropertyHandlers.Add("level", OnLevelUpdate);
                m_PropertyHandlers.Add("charactar", OnCharacterUpdate);
                m_PropertyHandlers.Add("helpful_behavior_list", OnHelpfulSpecificity);
                m_PropertyHandlers.Add("harmful_behavior_list", OnHarmfulSpecificity);
                m_PropertyHandlers.Add("achieve_reward_list", OnAchieveReward);    //以领取过的
                m_PropertyHandlers.Add("achieve_finish_list", OnAchieveFinish);
                m_PropertyHandlers.Add("belong_build", OnBelongId);
                m_PropertyHandlers.Add("skill_point", OnSkillPointUpdate);                
        }


        public Pet(SCMsgPetInfo msg)
        {
                InitPropertyHandlers();
                ////////////把属性回调逻辑注册放在属性注册前面，保证第一次赋值属性时逻辑也被运行//

                m_ID = msg.id;  //不会变更//
                m_CharacterId = msg.idCharacter;
                m_nLevel = msg.level;
                m_FighterProp = new FighterProperty(msg);       //用于存放msg.strAttr里的属性值。

                OnUpdateProperty("charactar", msg.idCharacter);
                OnUpdateProperty("exp", msg.exp);
                OnUpdateProperty("level", msg.level);

                OnUpdateProperty("helpful_behavior_list", msg.strHelpfulBehaviorList);
                OnUpdateProperty("harmful_behavior_list", msg.strHarmfulBehaviorList);
                OnUpdateProperty("buff_list", msg.strBuffList);
                OnUpdateProperty("cur_state", msg.cur_state);    //0 正常 1 出门 2 死亡 3 治疗
                OnUpdateProperty("hp", m_FighterProp.Hp);
                OnUpdateProperty("pressure", m_FighterProp.Pressure);

                OnUpdateProperty("buff_list", msg.strBuffList);    

                OnUpdateProperty("achieve_reward_list", msg.strAchieveRewardList); //已领取过的成就//
                OnUpdateProperty("achieve_finish_list", msg.strAchieveFinishList); //客户端无法计算的已经完成的成就//
                OnUpdateProperty("belong_build", msg.belongToBuildID); //在那个建筑内
                OnUpdateProperty("skill_point", msg.nSkillPoint);
                for (ItemType.EQUIP_PART i = ItemType.EQUIP_PART.MAIN_HAND; i < ItemType.EQUIP_PART.MAX; i++)
                {
                        m_Equip.Add(i, null);
                }
        }

        public Pet(int idCharacter)
        {
                m_CharacterId = idCharacter;
                OnUpdateProperty("charactar", idCharacter);
        }
        public override string GetPropertyString(string name)
        {
                if (m_PropertyDict.ContainsKey(name))  //先查找服务器发来的信息没有再去character表里找
                {
                        return m_PropertyDict[name];
                }
                else
                {
                        if (FighterProp != null && FighterProp.HasBasicAddProp(name))
                        {
                                return FighterProp.GetTotalProp(name).ToString();
                        }

                        if (m_CharacterCfg != null && m_CharacterCfg.m_pet_info.ContainsKey(name))
                        {
                                if (name.Equals("name"))
                                {
                                        return LanguageManager.GetText(m_CharacterCfg.m_pet_info[name]);
                                }
                                else
                                {
                                        return m_CharacterCfg.m_pet_info[name];
                                }
                        }
                }
                return "";
        }

        public override int GetPropertyInt(string name)
        {
                string property = GetPropertyString(name);
                int value = 0;
                int.TryParse(property, out value);
                return value;
        }

        public override long GetPropertyLong(string name)
        {
                string property = GetPropertyString(name);
                long value = 0;
                long.TryParse(property, out value);
                return value;
        }
        
        
        public override void OnUpdateProperty(string name, string value)
        {
                if (FighterProp != null)
                {
                        if (name.Contains("_add"))
                        {
                                string client_name = name.Replace("_add", "");
                                if (FighterProp.HasBasicAddProp(client_name))
                                {
                                        FighterProp.SetBasicAddProp(client_name, int.Parse(value));
                                }
                                else
                                {
                                        Debuger.Log("属性名" + name + "不正确");
                                }
                                return;
                               
                        }
                        if (FighterProp.HasBasicAddProp(name))
                        {
                                FighterProp.SetBasicAddProp(name, int.Parse(value));
                        }
                }

                base.OnUpdateProperty(name, value);
        }

        public void SetMySkill(Skill_Struct skill_info)
        {
                for (int i = 0; i < m_SkillList.Count; i++)
                {
                        if (m_SkillList[i].id == skill_info.id)
                        {
                                m_SkillList[i] = skill_info; //
                                return;
                        }
                }
                m_SkillList.Add(skill_info);
        }
        public void ResetSkills()
        {
                for (int i = 0; i < m_SkillList.Count; i++)
                {
                        Skill_Struct ss = m_SkillList[i];
                        ss.level = 1;
                        m_SkillList[i] = ss;
                }
        }
        public List<Skill_Struct> GetMySkill()
        {
                return m_SkillList;
        }
        public List<Skill_Struct> GetBattleSkills()
        {
                List<Skill_Struct> list = new List<Skill_Struct>();
                foreach (Skill_Struct ss in m_SkillList)
                {
                        SkillLearnConfigHold cfg = SkillManager.GetInst().GetSkillInfo(ss.config_id);
                        if (cfg != null)
                        {
                                if (cfg.type == (int)SKILL_TYPE.ACTIVE || cfg.type == (int)SKILL_TYPE.PASSIVE)
                                {
                                        list.Add(ss);
                                }
                        }
                }
                return list;
        }
        public List<int> GetAllMyClientSkill()
        {
                List<int> skills = new List<int>();
                skills.AddRange(m_CharacterCfg.active_skill);
                skills.AddRange(m_CharacterCfg.passive_skill);
                skills.AddRange(m_CharacterCfg.adventure_skill);
                skills.AddRange(m_CharacterCfg.camp_skill);
                return skills;
        }

        public SkillLearnConfigHold GetCaptainSkill()
        {
                foreach (Skill_Struct ss in m_SkillList)
                {
                        SkillLearnConfigHold cfg = SkillManager.GetInst().GetSkillInfo(ss.config_id);
                        if (cfg != null)
                        {
                                if (cfg.type == 6)
                                {
                                        return cfg;
                                }
                        }
                }
                return null;
        }

        public void SetMyEquip(Equip ep, ItemType.EQUIP_PART part)
        {
                if (m_Equip.ContainsKey(part))
                {
                        m_Equip[part] = ep;
                }
        }

        public void DeleteMyEquip(long equip_id)
        {
                if (EquipManager.GetInst().GetEquip(equip_id) != null)
                {
                        ItemType.EQUIP_PART part = (ItemType.EQUIP_PART)EquipManager.GetInst().GetEquip(equip_id).byPlace;
                        if (m_Equip.ContainsKey(part))
                        {
                                m_Equip[part] = null;
                        }
                }
        }

        public Equip GetMyEquipPart(ItemType.EQUIP_PART part)
        {
                if (m_Equip.ContainsKey(part))
                {
                        return m_Equip[part];
                }
                return null;
        }

        public bool PetHasEquip()
        {
                foreach (ItemType.EQUIP_PART part in m_Equip.Keys)
                {
                        if (m_Equip[part] != null)
                        {
                                return true;
                        }
                }
                return false;
        }

        public bool CanUseBetterEquip()
        {
                if (GetPropertyInt("cur_state") > (int)PetState.Normal)
                {
                        return false;
                }
                Equip m_equip;
                for (ItemType.EQUIP_PART i = ItemType.EQUIP_PART.MAIN_HAND; i < ItemType.EQUIP_PART.MAX; i++)
                {
                        m_equip = m_Equip[i];
                        List<Equip> m_equip_list = EquipManager.GetInst().GetEquipInBagByPartBySort((ItemType.EQUIP_PART)i, this);
                        if (m_equip == null && m_equip_list.Count > 0)
                        {
                                return true;
                        }
                        if (m_equip != null && m_equip_list.Count > 0)
                        {
                                if (EquipManager.GetInst().GetEquipCfg(m_equip_list[0].equip_configid).score > EquipManager.GetInst().GetEquipCfg(m_equip.equip_configid).score)
                                {
                                        return true;
                                }
                        }
                }
                return false;
        }


        public bool CanAdvanced()  //是否可以进阶
        {
                if (GetPropertyInt("cur_state") > (int)PetState.Normal)
                {
                        return false;
                }

                if (GetPropertyInt("level") == GetPropertyInt("max_level")) //等级到达最大
                {
                        string target = GetPropertyString("target");
                        string[] target_info = target.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        int target_id = 0;
                        for (int i = 0; i < target_info.Length; i++)
                        {
                                int.TryParse(target_info[i],out target_id);
                                CharacterConfig cc = CharacterManager.GetInst().GetCharacterCfg(target_id);
                                if (cc != null)
                                {
                                        bool is_gold_enough = false;
                                        bool is_pet_enough = false;
                                        bool is_item_enough = false;
                                        if (CommonDataManager.GetInst().GetNowResourceNum("gold") >= cc.GetPropInt("trans_need_gold")) //金钱足够
                                        {
                                                is_gold_enough = true;
                                        }
                                        int really_need_num = cc.GetPropInt("trans_need_count");
                                        if (PetManager.GetInst().GetMyPetAdvanceFood(ID, target_id).Count >= really_need_num)
                                        {
                                                is_pet_enough = true;
                                        }
                                        string require = cc.GetProp("trans_need_item");
                                        if(CommonDataManager.GetInst().CheckIsThingEnough(require,"3,"))
                                        {
                                                is_item_enough = true;
                                        }
                                        if (is_gold_enough && is_pet_enough && is_item_enough)
                                        {
                                                return true;
                                        }                       
                                }
                        }
                }
                return false;
        }


        public List<Specificity_Struct> GetMyPetSpecificity(Specificity_Type type)
        {
                switch (type)
                {
                        case Specificity_Type.GOOD_RANDOM:
                                return HelpfulSpecificity;
                        case Specificity_Type.BAD_RANDOM:
                                return HarmfulSpecificity;
                        case Specificity_Type.ALL:
                                List<Specificity_Struct> AllSpecificity = new List<Specificity_Struct>();
                                AllSpecificity.AddRange(HelpfulSpecificity);
                                AllSpecificity.AddRange(HarmfulSpecificity);
                                return AllSpecificity;
                        default:
                                return null;
                }
        }

        public int GetSpecificityNum(Specificity_Type type)
        {
                return GetMyPetSpecificity(type).Count;
        }

        public bool HasSpecificity(int id)
        {
                List<Specificity_Struct> specificity_list = GetMyPetSpecificity(Specificity_Type.ALL);
                for (int i = 0; i < specificity_list.Count; i++)
                {
                        if (specificity_list[i].id == id)
                        {
                                return true;
                        }
                }
                return false;
        }

        List<Specificity_Struct> HelpfulSpecificity = new List<Specificity_Struct>();
        List<Specificity_Struct> HarmfulSpecificity = new List<Specificity_Struct>();
        void SetSpecificity(string specificity, Specificity_Type type)
        {
                List<Specificity_Struct> specificity_list = new List<Specificity_Struct>();
                if (!specificity.Equals(CommonString.zeroStr))
                {
                        string[] one_specificity = specificity.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < one_specificity.Length; i++)
                        {
                                string[] temp = one_specificity[i].Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                                int id = 0;
                                int.TryParse(temp[0], out id);
                                Specificity_Struct ss;
                                ss.state = (Specificity_State)int.Parse(temp[1]);
                                ss.id = id;
                                specificity_list.Add(ss);
                        }
                }
                switch (type)
                {
                        case Specificity_Type.GOOD_RANDOM:
                                HelpfulSpecificity = specificity_list;
                                break;
                        case Specificity_Type.BAD_RANDOM:
                                HarmfulSpecificity = specificity_list;
                                break;
                        default:
                                break;
                }
        }

                            

        public bool CheckCanCure()  //是否有病以及治愈该病的材料//
        {
                if (GetPropertyInt("cur_state") > (int)PetState.Normal)
                {
                        return false;
                }
                List<Specificity_Struct> specificity_list = new List<Specificity_Struct>();
                specificity_list = GetMyPetSpecificity(Specificity_Type.BAD_RANDOM);
                for (int i = 0; i < specificity_list.Count; i++)
                {
                        SpecificityHold sfh = PetManager.GetInst().GetSpecificityCfg(specificity_list[i].id);
                        if (sfh != null)
                        {
                                string require = sfh.require;
                                if (CommonDataManager.GetInst().CheckIsThingEnough(require))
                                {
                                        return true;
                                }
                        }
                }
                return false;
        }

        void OnStateUpdate(string name, string oldval, string newval)
        {
                PetState old_value = (PetState)int.Parse(oldval);
                PetState new_value = (PetState)int.Parse(newval);

                if (HomeManager.GetInst().HomeRoot != null)
                {
                        if ((new_value == PetState.Normal || new_value == PetState.Curing) && (old_value == PetState.Death || old_value == PetState.Out))
                        {
                                HomeManager.GetInst().SetPetModelOne(this);
                        }
                        if (new_value == PetState.Death || new_value == PetState.Out)  //出门或者死亡宠物在家园里不显示
                        {
                                HomeManager.GetInst().DeletePetObj(this.ID);
                        }
                }

                UI_HeroMain uhm = UIManager.GetInst().GetUIBehaviour<UI_HeroMain>();
                if (uhm != null)
                {
                        uhm.RefreshMyHeroList();
                }              
        }


        void OnLevelUpdate(string name, string oldval, string newval)
        {
                m_FighterProp.Level = int.Parse(newval);
                m_FighterProp.BasicPropChange();
        }

        void OnCharacterUpdate(string name, string oldval, string newval)
        {
                m_CharacterId = int.Parse(newval);
                m_FighterProp.CharacterID = int.Parse(newval);
                m_FighterProp.BasicPropChange();
        }

        void OnSkillPointUpdate(string name, string oldval, string newval)
        {
                UI_SkillUpgrade uis = UIManager.GetInst().GetUIBehaviour<UI_SkillUpgrade>();
                if (uis != null)
                {
                        uis.RefreshSkillPoint();
                }
        }
        void OnHelpfulSpecificity(string name, string oldval, string newval)
        {
                SetSpecificity(newval, Specificity_Type.GOOD_RANDOM);
        }

        void OnHarmfulSpecificity(string name, string oldval, string newval)
        {
                SetSpecificity(newval, Specificity_Type.BAD_RANDOM);
        }


        void OnBelongId(string name, string oldval, string newval)
        {
                long id = long.Parse(newval);
                if (id > 0)
                {
                        BuildInfo info = HomeManager.GetInst().GetBuildInfo(id);
                        if (info.type == EBuildType.eStay)
                        {
                                UI_CheckIn uci = UIManager.GetInst().GetUIBehaviour<UI_CheckIn>();
                                if (uci != null)
                                {
                                        uci.RefreshUI();
                                }
                        }
                }
                HomeManager.GetInst().SetPetModelOne(this);
        }

        #region 成就

        HashSet<int> PersonalGetAchieve = new HashSet<int>();    //个人领取过的成就
        HashSet<int> PersonalFinishAchieve = new HashSet<int>(); //个人完成的成就（客户端无法获取是否完成的服务器通知）
        void OnAchieveReward(string name, string oldval, string newval)
        {
                PersonalGetAchieve.Clear();
                string achieves = newval;
                if (!achieves.Equals(CommonString.zeroStr))
                {
                        PersonalGetAchieve = achieves.ToHashSet<int>('&', (s) => int.Parse(s));
                }

                UI_HeroMain uhm = UIManager.GetInst().GetUIBehaviour<UI_HeroMain>();
                if (uhm != null)
                {
                        uhm.RefreshAchievement();
                }
        }

        void OnAchieveFinish(string name, string oldval, string newval)
        {
                PersonalFinishAchieve.Clear();
                string achieves = newval;
                if (!achieves.Equals(CommonString.zeroStr))
                {
                        PersonalFinishAchieve = achieves.ToHashSet<int>('&', (s) => int.Parse(s));
                }
        }

        public bool HasGetAchieve(int achivee_id)
        {
                return PersonalGetAchieve.Contains(achivee_id);
        }

        public bool HasFinishAchieve(int achivee_id)
        {
                return PersonalFinishAchieve.Contains(achivee_id);
        }


        int GetNeedShowAchievementBySeriesID(int series_id)
        {
                List<int> AchievementID = new List<int>();
                var AchievementDict_Enumerator = PetManager.GetInst().GetAchievementDict().GetEnumerator();
                try
                {
                        while (AchievementDict_Enumerator.MoveNext())
                        {
                                if (AchievementDict_Enumerator.Current.Value.series_id == series_id)
                                {
                                        AchievementID.Add(AchievementDict_Enumerator.Current.Key);
                                }
                        }
                }
                finally
                {
                        AchievementDict_Enumerator.Dispose();
                }

                int achieve_id = 0;
                for (int i = 0; i < AchievementID.Count; i++)
                {
                        achieve_id = AchievementID[i];
                        if (!isAchievementGet(achieve_id))
                        {
                                return achieve_id;  //显示这个
                        }
                }
                return AchievementID[AchievementID.Count - 1];  //如果所有成就都被领取，则返回该系列成就中的最后一个//
        }


        public bool isAchievementGet(int achieve_id)  //该成就是否被领取过
        {
                if (!PlayerController.GetInst().HasGlobalAchieve(achieve_id)) //全局获取成就中不存在//
                {
                        if (!HasGetAchieve(achieve_id)) //个人获取成就中不存在
                        {
                                return false;
                        }
                }
                return true;
        }



        public List<int> GetNeedShowAchievements()
        {
                List<int> show_ids = new List<int>();
                string achievement_series_ids = GetPropertyString("achievement_series_list");
                if (!GameUtility.IsStringValid(achievement_series_ids))
                {
                        return show_ids;
                }

                string[] achievement_series_id = achievement_series_ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < achievement_series_id.Length; i++)
                {
                        int series_id = int.Parse(achievement_series_id[i]);
                        show_ids.Add(GetNeedShowAchievementBySeriesID(series_id));
                }
                return show_ids;
        }

        public int GetMaxSkillLevel()
        {
                int max_skill_level = 0;
                for (int i = 0; i < m_SkillList.Count; i++)
                {
                        if (m_SkillList[i].level > max_skill_level)
                        {
                                max_skill_level = m_SkillList[i].level;
                        }
                }
                return max_skill_level;
        }

        public int GetSkillLevel(int skill_id)
        {
                int skill_level = 0;
                for (int i = 0; i < m_SkillList.Count; i++)
                {
                        if (m_SkillList[i].config_id == skill_id)
                        {
                                if (m_SkillList[i].level > skill_level)
                                {
                                        skill_level = m_SkillList[i].level;
                                }
                        }
                }
                return skill_level;
        }

        #endregion


        float not_normal_time = 0;
        float show_time = 0;
        public void SetNotNormalTime(int time)
        {
                not_normal_time = Time.realtimeSinceStartup + time;
        }
        public float GetNotNormalTime()
        {
                return not_normal_time;
        }

        public void SetShowTime(float time)
        {
                show_time = time;
        }
        public float GetShowTime()
        {
                return show_time;
        }


}

public struct Specificity_Struct  //怪癖（特性）
{
        public Specificity_State state;
        public int id;

}

public struct Skill_Struct
{
        public long id;
        public int config_id;
        public int level;
}

public enum PetState
{
        Normal,
        Out,
        Death,
        Curing,
        Max,
}



