//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using Message;

//public class RaidHero : PropertyBase
//{
//    public long PetID;

//    int m_CharacterId;
//    public int CharacterID
//    {
//        get
//        {
//            return m_CharacterId;
//        }
//    }

//    public CharacterConfig CharacterCfg
//    {
//        get
//        {
//            return CharacterManager.GetInst().GetCharacterCfg(CharacterID);
//        }
//    }
//    public int Pressure
//    {
//        get
//        {
//            return GetPropertyInt("pressure");
//        }
//    }
//    public int MaxPressure
//    {
//        get
//        {
//            return GetPropertyInt("max_pressure");
//        }
//    }
//    //public bool HasTortured = false;
//    public bool IsPressureOverLimit()
//    {
//        return Pressure >= MaxPressure;
//    }
//    public bool IsPressureOverDeadline()
//    {
//        return Pressure >= MaxPressure * 2;
//    }
//    public int Hp
//    {
//        get
//        {
//            return GetPropertyInt("hp");
//        }
//    }
//    public int MaxHp
//    {
//        get
//        {
//            return GetPropertyInt("max_hp");
//        }
//    }

//    Dictionary<int, int> m_SkillDict = new Dictionary<int, int>();

//    public HeroUnit BelongUnit = null;

//    public RaidHero(long idHero, long idPet, int idCharactar, int nLevel, long lHp, long lMaxHp, long lPressure, long lMaxPressure, string buffInfo, int nState)
//    {
//        m_ID = idHero;
//        PetID = idPet;
//        m_CharacterId = idCharactar;
//        m_nLevel = nLevel;

//        SetProperty("hp", lHp);
//        SetProperty("max_hp", lMaxHp);
//        SetProperty("pressure", lPressure);
//        SetProperty("max_pressure", lMaxPressure);
//        SetProperty("buff_list", buffInfo);
//        SetProperty("state", nState);

//        m_PropertyHandlers.Add("hp", OnHpUpdate);
//        m_PropertyHandlers.Add("pressure", OnPressureUpdate);
//        m_PropertyHandlers.Add("buff_list", OnBuffListUpdate);
//        m_PropertyHandlers.Add("behavior_obtain", OnBehaviorObtain);
//        m_PropertyHandlers.Add("state", OnHeroStateUpdate);
//        //m_SkillDict = GameUtility.ParseCommonStringToDict(msg.skillInfo);

//        OnBuffListUpdate("buff_list", "", buffInfo);
//        //OnBehaviorObtain("behavior_obtain", "", msg.behavior_obtain);
//    }
//    public RaidHero(SCMsgRaidHero msg)
//    {
//    }

//    public Dictionary<int, int> GetSkills()
//    {
//        return m_SkillDict;
//    }

//    public List<SkillLearnConfigHold> GetCampSkill()
//    {
//        List<SkillLearnConfigHold> list = new List<SkillLearnConfigHold>();
//        foreach (int skillCfgId in m_SkillDict.Keys)
//        {
//            SkillLearnConfigHold cfg = SkillManager.GetInst().GetSkillInfo(skillCfgId);
//            if (cfg.type == (int)SKILL_TYPE.CAMP)
//            {
//                list.Add(cfg);
//            }
//        }
//        return list;
//    }
//    public AdventureSkillConfig GetAdvSkillCfg()
//    {
//        SkillLearnConfigHold cfg = GetAdvSkill();
//        if (cfg != null)
//        {
//            return SkillManager.GetInst().GetAdvSkillCfg(cfg.id);
//        }
//        return null;
//    }

//    public int GetAdvSkillId()
//    {
//        foreach (int skillCfgId in m_SkillDict.Keys)
//        {
//            SkillLearnConfigHold cfg = SkillManager.GetInst().GetSkillInfo(skillCfgId);
//            if (cfg.type == (int)SKILL_TYPE.ADVENTURE)
//            {
//                return cfg.id;
//            }
//        }
//        return 0;
//    }

//    public SkillLearnConfigHold GetAdvSkill()
//    {
//        foreach (int skillCfgId in m_SkillDict.Keys)
//        {
//            SkillLearnConfigHold cfg = SkillManager.GetInst().GetSkillInfo(skillCfgId);
//            if (cfg.type == (int)SKILL_TYPE.ADVENTURE)
//            {
//                return cfg;
//            }
//        }
//        return null;
//    }
//    public SkillLearnConfigHold GetCaptainSkill()
//    {
//        foreach (int skillCfgId in m_SkillDict.Keys)
//        {
//            SkillLearnConfigHold cfg = SkillManager.GetInst().GetSkillInfo(skillCfgId);
//            if (cfg.type == (int)SKILL_TYPE.CAPTAIN)
//            {
//                return cfg;
//            }
//        }
//        return null;
//    }

//    void OnPressureUpdate(string name, string oldval, string newval)
//    {
//        UI_RaidMain uis = UIManager.GetInst().GetUIBehaviour<UI_RaidMain>();
//        if (uis != null)
//        {
//            uis.SetPressure(this.ID, Pressure);
//        }
//        if (BelongUnit != null)
//        {
//            int offset = int.Parse(newval) - int.Parse(oldval);
//            if (offset > 0)
//            {
//                RaidManager.GetInst().UnitTalk(TALK_TYPE.PRESSURE_ADD, BelongUnit);
//            }
//            BelongUnit.ShowPressure(offset);
//            if (IsPressureOverDeadline())
//            {
//                BelongUnit.UnitTalk(LanguageManager.GetText("sudden_death_desc_1"), "", 1f);
//            }
//        }
//    }
//    void OnHpUpdate(string name, string oldval, string newval)
//    {
//        UI_RaidMain uis = UIManager.GetInst().GetUIBehaviour<UI_RaidMain>();
//        if (uis != null)
//        {
//            uis.SetHeroHp(this.ID, Hp, MaxHp, true);
//        }
//        if (BelongUnit != null)
//        {
//            int deltaHp = int.Parse(newval) - int.Parse(oldval);
//            if (deltaHp != 0)
//            {
//                SkillResultData srd = new SkillResultData();
//                srd.nValue = Mathf.Abs(deltaHp);
//                srd.type = deltaHp > 0 ? SkillResultType.Heal : SkillResultType.Damage;
//                BelongUnit.ShowDamage(srd);


//                if (deltaHp < 0)
//                {
//                    if (BelongUnit.hero.Hp <= GlobalParams.GetFloat("AI_dying_line"))
//                    {
//                        RaidManager.GetInst().UnitTalk(TALK_TYPE.DYING, BelongUnit);
//                    }
//                    else
//                    {
//                        RaidManager.GetInst().UnitTalk(TALK_TYPE.HP_MINUS, BelongUnit);
//                    }
//                }
//            }

//            if (BelongUnit.hero.Hp <= 0)
//            {
//                RaidManager.GetInst().ShowHeroDie(BelongUnit);
//            }
//        }
//    }
//    int m_nGoodSpecCount = 0;
//    public int GoodSpecCount
//    {
//        get
//        {
//            return m_nGoodSpecCount;
//        }
//    }

//    int m_nBadSpecCount = 0;
//    public int BadSpecCount
//    {
//        get
//        {
//            return m_nBadSpecCount;
//        }
//    }
//    void OnHeroStateUpdate(string name, string oldval, string newval)
//    {
//        if (BelongUnit != null)
//        {
//            RaidManager.GetInst().ShowHeroLeave(BelongUnit, newval == "1");
//        }
//    }
//    void OnBehaviorObtain(string name, string oldval, string newval)
//    {
//        string[] tmps = newval.Split('&');
//        if (tmps.Length == 2)
//        {
//            int goodCount = int.Parse(tmps[0]);
//            int badCount = int.Parse(tmps[1]);
//            if (BelongUnit != null)
//            {
//                UI_RaidMain uis = UIManager.GetInst().GetUIBehaviour<UI_RaidMain>();

//                if (goodCount > m_nGoodSpecCount)
//                {
//                    GameObject effectObj = EffectManager.GetInst().PlayEffect("effect_specificity_get_good", BelongUnit.transform.position);
//                    if (uis != null)
//                    {
//                        uis.SetBehaviourShow(this.ID, goodCount, badCount, true, effectObj);
//                    }
//                    GameUtility.ShowTip(LanguageManager.GetText("specificity_get_good_tips"));
//                }
//                else if (badCount > m_nBadSpecCount)
//                {
//                    GameObject effectObj = EffectManager.GetInst().PlayEffect("effect_specificity_get_bad", BelongUnit.transform.position);
//                    if (uis != null)
//                    {
//                        uis.SetBehaviourShow(this.ID, goodCount, badCount, false, effectObj);
//                    }
//                    GameUtility.ShowTip(LanguageManager.GetText("specificity_get_bad_tips"));
//                }
//            }

//            m_nGoodSpecCount = goodCount;
//            m_nBadSpecCount = badCount;
//        }
//    }

//    public List<HeroBuff> m_BuffList = new List<HeroBuff>();
//    public class HeroBuff
//    {
//        public int cfgId;
//        public int level;
//        public int count;
//        public HeroBuff(string msg)
//        {
//            if (!string.IsNullOrEmpty(msg))
//            {
//                string[] infos = msg.Split('&');
//                if (infos.Length >= 3)
//                {
//                    int.TryParse(infos[0], out cfgId);
//                    int.TryParse(infos[1], out level);
//                    int.TryParse(infos[2], out count);
//                }
//            }
//        }
//    }

//    void OnBuffListUpdate(string name, string oldval, string newval)
//    {
//        List<HeroBuff> oldlist = new List<HeroBuff>(m_BuffList);
//        m_BuffList.Clear();
//        string[] buffs = newval.Split('|');
//        foreach (string buff in buffs)
//        {
//            if (string.IsNullOrEmpty(buff))
//                continue;
//            HeroBuff hb = new HeroBuff(buff);
//            HeroBuff oldhb = oldlist.Find((x) =>
//            {
//                return x.cfgId == hb.cfgId;
//            });
//            if (oldhb == null || oldhb.count < hb.count)
//            {
//                RaidManager.GetInst().UnitShowBuff(SkillManager.GetInst().GetBuff(hb.cfgId), BelongUnit);
//            }
//            m_BuffList.Add(hb);
//        }
//        UI_RaidMain uis = UIManager.GetInst().GetUIBehaviour<UI_RaidMain>();
//        if (uis != null)
//        {
//            uis.SetHeroBuff(this.ID, m_BuffList);
//        }
//    }
//}
