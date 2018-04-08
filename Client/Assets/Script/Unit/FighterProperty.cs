using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Message;

public class FighterProperty
{
        public FighterProperty()
        {
        }

        public FighterProperty(SCMsgPetInfo msg)
        {
                CharacterID = msg.idCharacter;
                FighterId = msg.id;
                Level = msg.level;
                Init();
                LoadDataFromPetMsg(msg.strAttr);
        }

        public FighterProperty(int id, long fighterId)
        {
                CharacterID = id;
                FighterId = fighterId;
                Init();
        }

        public bool IsEnemy = false;
        public int CharacterID;
        public long FighterId;
        int m_nLevel = 1;
        public int Level
        {
                get { return m_nLevel; }
                set
                {
                        m_nLevel = value;
                }
        }
        public int Career
        {
                get
                {
                        return CharacterCfg.career;
                }
        }
        public int CareerSys
        {
                get
                {
                        return CharacterCfg.GetPropInt("career_sys");
                }
        }

        int m_nHp;
        public int Hp
        {
                get { return m_nHp; }
                set
                {
                        CombatManager.GetInst().RoundInfo_Log("SetHP " + FighterId + " before=" + m_nHp + " now=" + value + "/" + MaxHp, 6);
                        m_nHp = Mathf.Clamp(value, 0, MaxHp);
                }
        }

        public int MaxHp
        {
                get
                {
                        return GetFinalPropValue("total_max_hp");
                }
        }

        public bool HasTortured = false;

        int m_nPressure;
        public int Pressure
        {
                get { return m_nPressure; }
                set
                {
                        CombatManager.GetInst().RoundInfo_Log("SetPressure " + FighterId + " before=" + m_nPressure + " now=" + value + "/" + MaxPressure, 6);
                        m_nPressure = Mathf.Clamp(value, 0, MaxPressure * 2);
                }
        }
        public bool IsPressureOverLimit()
        {
                return Pressure >= MaxPressure;
        }
        public bool IsPressureOverDeadline()
        {
                return Pressure >= MaxPressure * 2;
        }

        public int MaxPressure
        {
                get
                {
                        return GetFinalPropValue("maxpressure");
                }
        }

        public bool GetPressureToDead(int get_value)
        {
                if (Pressure + get_value < MaxPressure * 2)
                {
                        return false;
                }
                return true;
        }

        public int Pressure_LR;
        public void ResetPressureLR()
        {
                Pressure_LR = Pressure;
        }


        public int HpRatio
        {
                get
                {
                        return Hp * 100 / MaxHp;
                }
        }

        public void ResetHP()
        {
                m_nHp = MaxHp;
        }

        CharacterConfig m_CharacterCfg = null;
        public CharacterConfig CharacterCfg
        {
                get
                {
                        if (m_CharacterCfg == null)
                        {
                                m_CharacterCfg = CharacterManager.GetInst().GetCharacterCfg(CharacterID);
                        }
                        if (m_CharacterCfg == null)
                        {
                                Debuger.Log(CharacterID);
                        }
                        return m_CharacterCfg;
                }
        }

        public int m_nDelay;
        public int DelayVal
        {
                get { return m_nDelay; }
                set {
                        m_nDelay = Mathf.Max(0, value);
                }
        }
        public int MaxDelayVal
        {
                get 
                {
                        return (int)(10000f / Speed); 
                }
        }

        public int Speed
        {
                get
                {
                        int speed = GetFinalPropValue("total_speed");
                        if (speed < 1)
                        {
                                return 1;
                        }
                        return speed;
                }
        }

        public void ResetDelay()
        {
                DelayVal = MaxDelayVal;
        }

        Dictionary<int, int> m_ActiveSkillLevelDict = new Dictionary<int, int>();
        Dictionary<int, int> m_PassiveSkillLevelDict = new Dictionary<int, int>();
        Dictionary<int, int> m_BuffLevelDict = new Dictionary<int, int>();
        public List<int> GetActiveSkillList()
        {
                return new List<int>(m_ActiveSkillLevelDict.Keys);
        }
        public int GetActiveSkillLevel(int skillID)
        {
                if (m_ActiveSkillLevelDict.ContainsKey(skillID))
                {
                        return m_ActiveSkillLevelDict[skillID];
                }
                return 1;
        }


        public Dictionary<int, int> GetPassiveSkillList()
        {
                return m_PassiveSkillLevelDict;
        }

        public Dictionary<int, int> GetBuffDict()
        {
                return m_BuffLevelDict;
        }

        public List<int> GetAdventureSkills()
        {
                string[] tmps = CharacterCfg.GetProp("adventure_skill").Split(',');
                List<int> list = new List<int>();
                int skillid = 0;

                foreach (string tmp in tmps)
                {
                        int.TryParse(tmp, out skillid);
                        if (skillid > 0)
                        {
                                list.Add(skillid);
                        }
                }
                return list;
        }

        Dictionary<int, string> m_PropNameDict = new Dictionary<int, string>();

        Dictionary<string, int> m_BasicPropDict = new Dictionary<string, int>();        //战前值   ): HeroUnit时表示从表里读取的初值，FighterBehav则是服务端直接发过来的总和。:(

        Dictionary<string, int> m_BasicAddDict = new Dictionary<string, int>();         //战前附加值（HeroUnit会有，FighterBehav没有）

        Dictionary<string, int> m_BattleAddDict = new Dictionary<string, int>();        //战中附加值 FighterBehav有
        Dictionary<string, int> m_BattleAddPerDict = new Dictionary<string, int>();     //战中附加百分比 FighterBehav有

        Dictionary<string, List<TransformPropData>> m_TransformPropDict = new Dictionary<string, List<TransformPropData>>();     //需要转换的属性。

        public void Init()
        {
                Dictionary<int, AttributeConfig> dict = CharacterManager.GetInst().GetAttributeDict();
                foreach (AttributeConfig cfg in dict.Values)
                {
                        if (!m_BasicPropDict.ContainsKey(cfg.name))
                        {
                                m_BasicPropDict.Add(cfg.name, 0);
                        }
                        if (!m_BasicAddDict.ContainsKey(cfg.name))
                        {
                                m_BasicAddDict.Add(cfg.name, 0);
                        }
                        if (!m_BattleAddDict.ContainsKey(cfg.name))
                        {
                                m_BattleAddDict.Add(cfg.name, 0);
                        }
                        if (!m_BattleAddPerDict.ContainsKey(cfg.name))
                        {
                                m_BattleAddPerDict.Add(cfg.name, 0);
                        }
                }

                if (CharacterCfg != null)
                {
                        //设置基础值，即战前总和。
                        SetBasicProp("atk", CharacterCfg.init_attack + CharacterCfg.attack_growth * (Level - 1));
                        SetBasicProp("speed", CharacterCfg.init_speed);
                        SetBasicProp("def", CharacterCfg.init_defence + CharacterCfg.defence_growth * (Level - 1));
                        SetBasicProp("maxhp", CharacterCfg.init_hp + CharacterCfg.hp_growth * (Level - 1));
                        SetBasicProp("maxpressure", CharacterCfg.init_pressure);
                        SetBasicProp("speciality", 0);

                        //这六个用于战前总和计算，不直接对外
                        SetBasicProp("atk_per", 0);
                        SetBasicProp("speed_per", 0);
                        SetBasicProp("def_per", 0);
                        SetBasicProp("maxhp_per", 0);

                        //下面这些属性均为直接百分比值
                        SetBasicProp("critical_rate_per", CharacterCfg.init_critical_rate);
                        SetBasicProp("critical_damage_per", CharacterCfg.init_critical_damage);
                        SetBasicProp("accurate_per", 0);
                        SetBasicProp("penetrate_per", 0);
                        SetBasicProp("damage_increase_per", 0);
                        SetBasicProp("dodge_per", 0);
                        SetBasicProp("debuff_resistance_per", 0);
                        SetBasicProp("damage_reduce_per", 0);
                        SetBasicProp("parry_per", 0);
                        SetBasicProp("rebound_per", 0);
                        SetBasicProp("tough_per", 0);
                        SetBasicProp("life_steal_per", 0);
                        SetBasicProp("heal_increase_per", 0);
                        SetBasicProp("recover_increase_per", 0);
                        SetBasicProp("cooldown_reduce_per", 0);
                        SetBasicProp("trigger_rate_per", 0);

                        SetBasicProp("level", Level);
                        SetBasicProp("race", CharacterCfg.race);
                        SetBasicProp("career", CharacterCfg.career);
                }
                ResetHP();
                m_nPressure = 0;
        }


        public void BasicPropChange()  //提取了一些会随等级更新的基础值
        {
                if (CharacterCfg != null)
                {
                        SetBasicProp("level", Level);
                        SetBasicProp("atk", CharacterCfg.init_attack + CharacterCfg.attack_growth * (Level - 1));
                        SetBasicProp("def", CharacterCfg.init_defence + CharacterCfg.defence_growth * (Level - 1));
                        SetBasicProp("maxhp", CharacterCfg.init_hp + CharacterCfg.hp_growth * (Level - 1));
                }
        }
        static string[] PET_ATTR_NAMES = new string[]
        {
                "atk","atk_per","def","def_per","hp", "maxhp", "maxhp_per",
                "speed","speed_per","maxpressure","pressure","speciality", "critical_rate_per", "critical_damage_per",
                "accurate_per","penetrate_per","damage_increase_per","dodge_per","debuff_resistance_per", "damage_reduce_per", "parry_per",
                "rebound_per","tough_per","life_steal_per","heal_increase_per","recover_increase_per", "cooldown_reduce_per", "trigger_rate_per",
        };
        public void LoadDataFromPetMsg(string strAttr)
        {
                string[] props = strAttr.Split('&');
                for (int idx = 0; idx < PET_ATTR_NAMES.Length; idx++)
                {
                        SetBasicAddProp(PET_ATTR_NAMES[idx], GameUtility.GetPropInt(props, idx));
                }
        }

        public void LoadDataFromMsg(SCMsgBattleFighter msg)
        {
                Level = msg.nLevel;
                SetBasicProp("level", Level);

                //设置基础值，即战前总和。
                SetBasicProp("atk", msg.atkVal);
                SetBasicProp("speed", msg.speed);
                SetBasicProp("def", msg.defVal);
                SetBasicProp("maxhp", msg.maxHp);
                SetBasicProp("maxpressure", msg.maxPressure);

                //下面这些属性均为直接百分比值
                SetBasicProp("critical_rate_per", msg.criticalRate);
                SetBasicProp("critical_damage_per", msg.criticalDamage);
                SetBasicProp("accurate_per", msg.accurate);
                SetBasicProp("penetrate_per", msg.penetrate);
                SetBasicProp("damage_increase_per", msg.damageIncrease);
                SetBasicProp("dodge_per", msg.dodge);
                SetBasicProp("debuff_resistance_per", msg.debuffResistance);
                SetBasicProp("debuff_accurate_per", msg.lDebuffAccurate);
                HasTortured = msg.nTorture > 0;

                SetBasicProp("damage_reduce_per", msg.damageReduce);
                SetBasicProp("parry_per", msg.parry);
                SetBasicProp("rebound_per", msg.rebound);
                SetBasicProp("tough_per", msg.tough);
                SetBasicProp("life_steal_per", msg.hpSteal);
                SetBasicProp("heal_increase_per", msg.healIncrease);
                SetBasicProp("recover_increase_per", msg.recoverIncrease);
                SetBasicProp("cooldown_reduce_per", msg.CDReduce);
                SetBasicProp("trigger_rate_per", msg.triggerRate);

                m_nHp = msg.hp;
                m_nPressure = msg.pressure;
                if (!string.IsNullOrEmpty(msg.active_skillInfo))
                {
                        string[] tmps = msg.active_skillInfo.Split('|');
                        foreach (string tmp in tmps)
                        {
                                if (string.IsNullOrEmpty(tmp))
                                        continue;
                                string[] info = tmp.Split('&');
                                if (info.Length == 2)
                                {
                                        int skillId = int.Parse(info[0]);
                                        int level = int.Parse(info[1]);
                                        if (!m_ActiveSkillLevelDict.ContainsKey(skillId))
                                        {
                                                m_ActiveSkillLevelDict.Add(skillId, level);
/*                                                Debuger.Log(FighterId + " m_ActiveSkillLevelDict.Add " + skillId);*/
                                        }
                                }
                        }
                }
                if (!string.IsNullOrEmpty(msg.passive_skillInfo))
                {
                        string[] tmps = msg.passive_skillInfo.Split('|');
                        foreach (string tmp in tmps)
                        {
                                if (string.IsNullOrEmpty(tmp))
                                        continue;
                                string[] info = tmp.Split('&');
                                if (info.Length == 2)
                                {
                                        int skillId = int.Parse(info[0]);
                                        int level = int.Parse(info[1]);
                                        if (!m_PassiveSkillLevelDict.ContainsKey(skillId))
                                        {
                                                m_PassiveSkillLevelDict.Add(skillId, level);
                                        }
                                }
                        }
                }
                if (!string.IsNullOrEmpty(msg.bufflist))
                {
                        string[] tmps = msg.bufflist.Split('|');
                        foreach (string tmp in tmps)
                        {
                                if (string.IsNullOrEmpty(tmp))
                                        continue;
                                string[] info = tmp.Split('&');
                                if (info.Length >= 2)
                                {
                                        int buffId = int.Parse(info[0]);
                                        int level = int.Parse(info[1]);
                                        if (!m_BuffLevelDict.ContainsKey(buffId))
                                        {
                                                m_BuffLevelDict.Add(buffId, level);
                                        }
                                }
                        }
                }
        }

        public int GetBattleAddVal(string name)
        {
                if (m_BattleAddDict.ContainsKey(name))
                {
                        return m_BattleAddDict[name];
                }
                return 0;
        }
        public void SetBattlePropAdd(int id, int value, bool bReplace)
        {
                string name = CharacterManager.GetInst().GetPropertyName(id);
                SetBattleAdd(name, value, bReplace);
        }
        public void SetBattleAdd(string name, int value, bool bReplace)
        {
                if (!m_BattleAddDict.ContainsKey(name))
                {
                        m_BattleAddDict.Add(name, value);
                }
                else
                {
                        if (bReplace)
                        {
                                m_BattleAddDict[name] = value;
                        }
                        else
                        {
                                m_BattleAddDict[name] += value;
                        }
                }
                Debuger.Log("<color=#54f267>BattleProp " + FighterId + " " + name + " add " + value + " result=" + m_BattleAddDict[name] +"</color>");
        }

        public int GetBasicAddProp(string name)
        {
                switch (name)
                {
                        case "hp":
                                return Hp;
                        case "pressure":
                                return Pressure;
                        default:
                                {
                                        if (m_BasicAddDict.ContainsKey(name))
                                        {
                                                return m_BasicAddDict[name];
                                        }
                                }
                                break;
                }
                return 0;
        }

        public void SetBasicAddProp(string name, int value)
        {
                switch (name)
                {
                        case "hp":
                                Hp = value;
                                break;
                        case "pressure":
                                Pressure = value;
                                break;
                        default:
                                {
                                        if (!m_BasicAddDict.ContainsKey(name))
                                        {
                                                m_BasicAddDict.Add(name, value);
                                        }
                                        else
                                        {
                                                m_BasicAddDict[name] = value;
                                        }
                                }
                                break;
                }
        }

        public void SetBasicProp(string name, int value)
        {
                if (!m_BasicPropDict.ContainsKey(name))
                {
                        m_BasicPropDict.Add(name, value);
                }
                else
                {
                        m_BasicPropDict[name] = value;
                }
        }

        public bool HasBasicAddProp(string name)
        {
                if (name == "hp" || name == "pressure")
                {
                        return true;
                }

                if (m_BasicAddDict.ContainsKey(name))
                {
                        return true;
                }
                return false;
        }

        /// <summary>
        /// 得到战前值
        /// HeroUnit: m_BasicPropDict是配置的初值+等级成长，没有则是0；m_BasicAddDict是服务端发过来的附加值（各种系统）
        /// FighterBehav：m_BasicPropDict是服务端计算好的战前值总和。；没有m_BasicAddDict。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetBasicProp(string name)
        {
                int ret = 0;
                if (m_BasicPropDict.ContainsKey(name))
                {
                        ret = m_BasicPropDict[name];
                }
                if (m_BasicAddDict.ContainsKey(name))
                {
                        ret += m_BasicAddDict[name];
                }
                return ret;
        }

        public bool HasBasicProp(string name)
        {
            if (m_BasicPropDict.ContainsKey(name))
            {
                return true;
            }
            return false;
        }

        
        /// <summary>
        /// 得到战前值总和
        /// 攻击力，防御力，生命上限，心情，专精会用到这个公式
        /// </summary>
        /// <param name="val_name"></param>
        /// <param name="per_name"></param>
        /// <returns></returns>
        int GetBasicProp(string val_name, string per_name)
        {
                if (!string.IsNullOrEmpty(per_name))
                {
                        return (int)(GetBasicProp(val_name) * (1f + GetBasicProp(per_name) / 100f));
                }
                else
                {
                        return (int)(GetBasicProp(val_name));
                }
        }

        class TransformPropData
        {
                public int type;                        //类型，2或3，用不同公式转换
                public string affect_name;      //受影响的属性名，即转换后的
                public string relate_name;      //关联的属性名，即用来转换的
                public int relate_param;                       //关联参数，用于转换计算的参数或者比较
                public int affect_param;     //影响的属性值
                
                public int equtionType; //不等式类型（1大于关联属性值，2小于）
        }
        public void AddTransformRelations(string info, int type)
        {
                string[] infos = info.Split(';');
                foreach (string tmp in infos)
                {
                        if (string.IsNullOrEmpty(tmp))
                                continue;
                        string[] tmps = tmp.Split(',');
                        if (tmps.Length >= 3)
                        {
                                int toId = int.Parse(tmps[0]);
                                int fromId = int.Parse(tmps[1]);
                                int param = int.Parse(tmps[2]);

                                TransformPropData tpd = new TransformPropData();
                                tpd.affect_name = CharacterManager.GetInst().GetPropertyName(toId);
                                tpd.relate_name = CharacterManager.GetInst().GetPropertyName(fromId);
                                tpd.relate_param = param;
                                tpd.type = type;
                                if (tmps.Length >= 5)
                                {
                                        int.TryParse(tmps[3], out tpd.affect_param);
                                        int.TryParse(tmps[4], out tpd.equtionType);
                                }

                                if (!m_TransformPropDict.ContainsKey(tpd.affect_name))
                                {
                                        m_TransformPropDict.Add(tpd.affect_name, new List<TransformPropData>());
                                        //Debuger.Log("m_TransformPropDict.Add " + tpd.affect_name + " " + tpd.relate_name);
                                }
                                m_TransformPropDict[tpd.affect_name].Add(tpd);
                        }
                }
        }

        int GetTransformValue(string val_name)
        {
                int ret = 0;
                if (m_TransformPropDict.ContainsKey(val_name))
                {
                        foreach (TransformPropData tpd in m_TransformPropDict[val_name])
                        {
                                int affectProp = GetTotalProp(tpd.affect_name);
                                int relateProp = GetTotalProp(tpd.relate_name);
                                Debuger.Log("TransformProp " + tpd.affect_name + "=" + affectProp + " relateprop=" + tpd.relate_name + " " + relateProp + " param=" + tpd.relate_param);

                                if (tpd.type == 2)
                                {
                                        ret += (int)(relateProp * tpd.relate_param / 100f);
                                }
                                if (tpd.type == 3)
                                {
                                        ret += (int)(affectProp * relateProp * tpd.relate_param / 100f);
                                }
                                if (tpd.type == 4)
                                {
                                        if (tpd.relate_param > 0)
                                        {
                                                ret += affectProp * relateProp / tpd.relate_param - affectProp;
                                        }
                                }
                                if (tpd.type == 5)
                                {
                                        //要加属性id，关联属性id，要加属性值，关联属性值，大于 / 小于（1大 2小）
                                        switch (tpd.equtionType)
                                        {
                                                case 1:
                                                        if (relateProp > tpd.relate_param)
                                                        {
                                                                ret += tpd.affect_param;
                                                                Debuger.Log(ret);
                                                        }
                                                        break;
                                                case 2:
                                                        if (relateProp < tpd.relate_param)
                                                        {
                                                                ret += tpd.affect_param;
                                                                Debuger.Log(ret);
                                                        }
                                                        break;
                                        }
                                }
                        }
                }
                return ret;
        }

        #region 属性封装

        /// <summary>
        /// 战斗内通过ID取属性
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetFinalPropValue(int id)
        {
                string name = CharacterManager.GetInst().GetPropertyName(id);
                Debug.Log(name);
                return GetTotalProp(name) + GetTransformValue(name); 
        }
        /// <summary>
        /// 战斗内通过属性名取属性
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetFinalPropValue(string name)
        {
                return GetTotalProp(name) + GetTransformValue(name); 
        }

        /// <summary>
        /// 战斗外Pet可以用此接口
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetTotalProp(string name)
        {
                //"atk""atk_per""speed""speed_per""def""def_per""maxhp""maxhp_per"这8个只用于中间计算。访问要加total
                switch (name)
                {
                        case "total_attack":
                                {
                                        return (int)(GetBasicProp("atk", "atk_per") * (1f + GetFinalPropValue("atk_per") / 100f) + GetBattleAddVal("atk"));
                                }
                        case "total_speed":
                                {
                                        return (int)(GetBasicProp("speed", "speed_per") * (1f + GetFinalPropValue("speed_per") / 100f) + GetBattleAddVal("speed"));
                                }
                        case "total_defence":
                                {
                                        return (int)(GetBasicProp("def", "def_per") * (1f + GetFinalPropValue("def_per") / 100f) + GetBattleAddVal("def"));
                                }
                        case "total_max_hp":
                                {
                                        return (int)(GetBasicProp("maxhp", "maxhp_per") * (1f + GetFinalPropValue("maxhp_per") / 100f) + GetBattleAddVal("maxhp"));
                                }
                        case "atk_per":
                        case "def_per":
                        case "speed_per":
                        case "maxhp_per":
                        case "critical_rate_per":
                        case "critical_damage_per":
                        case "accurate_per":
                        case "penetrate_per":
                        case "damage_increase_per":
                        case "dodge_per":
                        case "debuff_resistance_per":
                        case "damage_reduce_per":
                        case "parry_per":
                        case "rebound_per":
                        case "tough_per":
                        case "life_steal_per":
                        case "heal_increase_per":
                        case "recover_increase_per":
                        case "cooldown_reduce_per":
                        case "trigger_rate_per":
                        case "debuff_accurate_per":
                                {
                                        return GetBasicProp(name) + GetBattleAddVal(name);
                                }
                        case "stun_buf":
                        case "silence_buf":
                        case "disarm_buf":
                        case "taunt_buf":
                        case "confusion_buf":
                        case "fear_buf":
                        case "conceal_buf":
                        case "mark_buf":
                                {
                                        return GetBattleAddVal(name);
                                }
                        case "maxpressure":
                        case "speciality":
                        default:
                                {
                                        return GetBasicProp(name);
                                }
                        case "lost_hp":
                                {
                                        return MaxHp - Hp;
                                }
                        case "hp_per":
                                {
                                        return (Hp * 100 / MaxHp );
                                }
                        case "pressure_per":
                                {
                                        return (Pressure * 100 / MaxPressure);
                                }
                        case "pressure":
                                {
                                        return Pressure;
                                }
                        case "hp":
                                {
                                        return Hp;
                                }
                }
        }

        #endregion
}
