using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public enum SkillResultType
{
        Dodge,
        Miss,
        Heal,
        Damage,
        AddBuff,
        Relive,
        AddPressure,
        MinusPressure,
}

[System.Serializable]
public class SkillResultData
{
        public SkillResultType type = SkillResultType.Damage;
        public int nValue = 0;
        public bool hasKill = false;    //有击杀
        public bool isParry = false;    //是否格挡
        public bool isCritical = false; //是否有暴击

        public int nLifeSteal = 0;      //角色属性吸血
        public int nRebound = 0;        //角色属性反弹

        public bool isDelay = false;    //是否影响行动条
        public bool isDisperse = false; //是否驱散
        public bool isVampire = false;  //是否技能吸血
        public bool isRecoverPressure = false;  //是否压力回复
        public bool isRecoverLight = false;     //光照回复
        public List<int> targetBuffList = new List<int>();
        public bool isSummon = false;           //是否有召唤

        public float hpRatio;//记录该次结果后目标角色的剩余血量
}

public static class RandomCreater
{
        public static int MOD = 32768;
        public static long seed = 0;

        public static int Next(int min, int max)
        {
                if (min >= max)
                {
                        CombatManager.GetInst().RoundInfo_SetNextRandom(min);
                        return min;
                }
                seed = (seed * 1103515245 + 12345) % MOD;
                //Debuger.LogWarning("seed = " + seed + " min=" + min + " max= " + max);
                int ret = (int)(seed % (max - min + 1) + min);
                CombatManager.GetInst().RoundInfo_SetNextRandom(ret);

                return ret;
        }
}

class DamageCalc
{
        public static bool IsUseSeed = true;    //
        static int DEF_LEVEL_FACTOR = 50;

        /// <summary>
        /// 检查额外附加是否命中
        /// 返回true表示命中（抵抗失败了）
        /// 返回false表示抵抗成功（命中失败）
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool CheckExtraStateHit(FighterProperty target, int attackAccurate = 0, string mark = "")
        {
                int resist_rate = target.GetFinalPropValue("debuff_resistance_per") - attackAccurate;
                if (resist_rate < 100)
                {
                        return CheckRandom(100 - resist_rate, mark);
                }
                return false;
        }
        public static int GetRealTriggerRate(FighterProperty attacker, int rate, float counterFactor)
        {
                CombatManager.GetInst().RoundInfo_Log("技能触发率=" + rate + " 抵抗系数=" + counterFactor + " 行动者触发率=" + attacker.GetFinalPropValue("trigger_rate_per"));
                return (int)(100 - 100 * Mathf.Pow(1f - rate / 100f, counterFactor + attacker.GetFinalPropValue("trigger_rate_per") / 100f));
        }

        public static bool CheckRandom(int needRate, string mark = "")
        {
                return CheckRandom(1, 100, needRate, mark);
        }
        public static bool CheckRandom(int min, int max, int needRate, string mark)
        {
                if (IsUseSeed)
                {
                        int ret = RandomCreater.Next(min, max);
                        CombatManager.GetInst().RoundInfo_Log("检测随机数[" + mark  + "]= " + ret + "   区间=[1, 100] 需求=" + needRate, 1);
                        return ret <= needRate;
                }
                else
                {
                        int ret = UnityEngine.Random.Range(min, max);
                        CombatManager.GetInst().RoundInfo_Log("UseUnityRandom " + ret);
                        return  ret <= needRate;
                }
        }

        /// <summary>
        /// [min,max]闭区间
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GetRandomVal(int min, int max, string mark = "")
        {
                if (IsUseSeed)
                {
                        int ret = RandomCreater.Next(min, max);
                        if (min == max)
                        {
                                CombatManager.GetInst().RoundInfo_Log("取随机数[" + mark + "] = " + min + " 区间[min=max]", 1);
                        }
                        else
                        {
                                CombatManager.GetInst().RoundInfo_Log("取随机数[" + mark + "] = " + ret + "   区间["+ min + "," + max + "]", 1);
                        }
                        return ret;
                }
                else
                {
                        int ret = UnityEngine.Random.Range(min, max + 1);
                        CombatManager.GetInst().RoundInfo_Log("UseUnityRandom " + ret);
                        return ret;
                }
        }

        static bool CalcMiss(FighterProperty attacker, FighterProperty target, int nBrightLvl, SkillConfig skillCfg)
        {
                //行动者是敌人时，不计算光照影响
                if (attacker.IsEnemy)
                {
                        nBrightLvl = 0;
                }

                int hitRate = skillCfg.base_hitrate_per - nBrightLvl * 4;
                //命中率大于等于100，直接返回false表示命中，没有Miss
                if (hitRate < 100)
                {
                        //检测命中率，检测失败则返回true表示未命中，Miss了
                        if (CheckRandom(hitRate, "命中率") == false)
                        {
                                return true;
                        }
                }
                return false;
        }
        static bool CalcDodge(FighterProperty attacker, FighterProperty target, int nBrightLvl, SkillConfig skillCfg)
        {
                //目标无闪避
                if (target.GetFinalPropValue("dodge_per") <= 0)
                {
                        return false;
                }
                //攻击方精准大于等于100
                if (attacker.GetFinalPropValue("accurate_per") >= 100)
                {
                        return false;
                }

                int rate = (int)(target.GetFinalPropValue("dodge_per") * (1f - attacker.GetFinalPropValue("accurate_per") / 100f));
                if (CheckRandom(rate, "闪避率"))
                {
                        return true;
                }

                return false;
        }

        static bool CalcParry(FighterProperty attacker, FighterProperty target)
        {
                float val = target.GetFinalPropValue("parry_per") * (1f - attacker.GetFinalPropValue("accurate_per") / 100f);
                if (val <= 0f)
                {
                        return false;
                }

                if (CheckRandom((int)val, "格挡率"))
                {
                        return true;
                }
                return false;
        }

        static bool CalcCritical(FighterProperty attacker, FighterProperty target, int nBrightLvl, SkillConfig skillCfg, bool bExtra)
        {
                int extraCriRate = bExtra ? skillCfg.extra_critical_rate_per : 0;
                int totalCriRate = skillCfg.base_critical_rate_per + attacker.GetFinalPropValue("critical_rate_per") + extraCriRate;
                if (totalCriRate <= 0)
                {
                        return false;
                }
                if (target.GetFinalPropValue("tough_per") >= 100)
                {
                        return false;
                }
                                
                int rate = (int)((totalCriRate + nBrightLvl * 4f) * (1f - target.GetFinalPropValue("tough_per") / 100f));
                if (CheckRandom(rate, "暴击率"))
                {
                        return true;
                }
                return false;
        }

        /// <summary>
        /// 计算提拉行动条
        /// </summary>
        /// <param name="skillCfg"></param>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <param name="counterFactor"></param>
        /// <param name="rd"></param>
        /// <param name="bUseRealRate">是否计算真实发动率</param>
        static void CheckDelay(SkillConfig skillCfg, FighterProperty attacker, FighterProperty target, float counterFactor, ref SkillResultData rd, bool bUseRealRate)
        {
                if (target.Hp <= 0)
                        return;

                if (skillCfg.speed == 0)
                        return;

                int rate = skillCfg.GetLevelValue(skillCfg.speed_rate, attacker);
                if (rate > 0)
                {
                        if (bUseRealRate)
                        {                                
                                rate = GetRealTriggerRate(attacker, rate, counterFactor);
                                CombatManager.GetInst().RoundInfo_Log("延迟真实发动率=" + rate);
                        }

                        if (CheckRandom(rate, "延迟发动率"))
                        {
                                if (skillCfg.speed < 0 ||       //负值（即结果为提前行动）
                                        bUseRealRate == false ||        //使用配置发动率
                                        CheckExtraStateHit(target, 0, "延迟命中检测"))   //抵抗失败
                                {
                                        rd.isDelay = true;
                                        target.DelayVal += skillCfg.speed;
                                }
                                CombatManager.GetInst().RoundInfo_Log("提拉行动条 " + target.FighterId + " " + target.DelayVal);
                        }
                }
        }

        static void CheckDisperse(SkillConfig skillCfg, FighterProperty attacker, FighterBehav targetBehav, float counterFactor, ref SkillResultData rd, bool bUseRealRate)
        {
                if (targetBehav.FighterProp.Hp <= 0)
                        return;
                if (skillCfg.disperse <= 0)
                        return;

                //计算驱散
                int rate = skillCfg.GetLevelValue(skillCfg.disperse_rate, attacker);

                if (rate > 0)
                {
                        if (bUseRealRate)
                        {
                                rate = GetRealTriggerRate(attacker, rate, counterFactor);
                                CombatManager.GetInst().RoundInfo_Log("驱散真实发动率=" + rate);
                        }

                        if (CheckRandom(rate, "驱散发动率"))
                        {
                                if (bUseRealRate == false ||                                            //使用配置发动率
                                        CheckExtraStateHit(targetBehav.FighterProp, 0, "驱散命中检测"))  //抵抗失败
                                {
                                        rd.isDisperse = true;
                                        targetBehav.RemoveBuff(skillCfg.disperse, skillCfg.disperse_number);
                                        targetBehav.PlayDisperseEffect(skillCfg);
                                }
                        }
                }
        }

        /// <summary>
        /// 检查BUFF
        /// </summary>
        /// <param name="skillCfg"></param>
        /// <param name="attacker"></param>
        /// <param name="target"></param>
        /// <param name="counterFactor"></param>
        /// <param name="rd"></param>
        /// <param name="bCheckResistance"></param>
        static void CheckBuff(SkillConfig skillCfg, FighterBehav attackerBehav, FighterBehav targetBehav, float counterFactor, ref SkillResultData rd, bool bCheckResistance)
        {
                if (targetBehav.FighterProp.Hp <= 0)
                        return;


                if (skillCfg.buff.Count <= 0 || skillCfg.buff[0] <= 0)
                {
                        return;
                }

                if (targetBehav.HasChangeBuff(skillCfg))
                {
                        FighterProperty attackerProp = attackerBehav.FighterProp;
                        FighterProperty targetProp = targetBehav.FighterProp;

                        int rate = skillCfg.GetLevelValue(skillCfg.buff_rate, attackerProp);

                        if (rate <= 0)
                        {
                                return;
                        }

                        if (bCheckResistance)
                        {
                                rate = GetRealTriggerRate(attackerProp, rate, counterFactor);
                                CombatManager.GetInst().RoundInfo_Log("BUFF真实发动率=" + rate);
                        }

                        if (CheckRandom(rate, "状态发动率"))
                        {
                                List<int> bufflist = new List<int>(skillCfg.buff);

                                for (int i = 0; i < skillCfg.add_buff_number; i++)
                                {
                                        if (bufflist.Count > 0)
                                        {
                                                int idx = GetRandomVal(0, bufflist.Count - 1, "选取添加的buff列表中的一个");
                                                SkillBuffConfig buffCfg = SkillManager.GetInst().GetBuff(bufflist[idx]);
                                                if (buffCfg != null)
                                                {
                                                        if (buffCfg.good_or_bad == 1 ||
                                                                bCheckResistance == false ||
                                                                CheckExtraStateHit(targetProp, attackerProp.GetFinalPropValue("debuff_accurate_per"), "Buff命中检测"))
                                                        {
                                                                rd.targetBuffList.Add(buffCfg.id);
                                                                targetBehav.AddBuff(buffCfg.id, attackerBehav, true);
                                                        }
                                                }
                                                bufflist.RemoveAt(idx);
                                        }
                                }
                        }
                }

                if (rd.targetBuffList.Count > 0)
                {
                        attackerBehav.HasBuffAdded = true;
                }
        }


        static void CheckBrightness(SkillConfig skillCfg, FighterProperty attacker, FighterBehav targetBehav, float counterFactor, ref SkillResultData rd, bool bUseRealRate)
        {
                if (targetBehav.FighterProp.Hp <= 0)
                        return;
                if (targetBehav.IsEnemy)        //目标是地方阵营时，不计算附加压力
                        return;
                if (skillCfg.brightness == 0)
                        return;

                int rate = skillCfg.GetLevelValue(skillCfg.brightness_rate, attacker);
                if (rate > 0)
                {
                        if (bUseRealRate)
                        {
                                rate = GetRealTriggerRate(attacker, rate, counterFactor);
                                CombatManager.GetInst().RoundInfo_Log("光照真实发动率=" + rate);
                        }

                        if (CheckRandom(rate, "光照变化发动率"))
                        {
                                if (skillCfg.brightness > 0 ||
                                        bUseRealRate == false ||
                                        CheckExtraStateHit(targetBehav.FighterProp, 0, "光照变化命中检测"))
                                {
                                        CombatManager.GetInst().ChangeBright(skillCfg.brightness);
                                }
                        }
                }

        }
        static void CheckPressure(SkillConfig skillCfg, FighterProperty attacker, FighterBehav targetBehav, float counterFactor, ref SkillResultData rd, bool bUseRealRate)
        {
                if (targetBehav.FighterProp.Hp <= 0)
                        return;
               if (targetBehav.IsEnemy)        //目标是地方阵营时，不计算附加压力
                        return;
                if (skillCfg.pressure == 0)
                        return;

                int rate = skillCfg.GetLevelValue(skillCfg.pressure_rate, attacker);
                if (rate > 0)
                {
                        if (bUseRealRate)
                        {
                                rate = GetRealTriggerRate(attacker, rate, counterFactor);
                                CombatManager.GetInst().RoundInfo_Log("压力真实发动率=" + rate);
                        }

                        if (CheckRandom(rate, "压力变化发动率"))
                        {
                                if (skillCfg.pressure < 0 || 
                                        bUseRealRate == false || 
                                        CheckExtraStateHit(targetBehav.FighterProp, 0, "压力变化命中检测"))
                                {
                                        targetBehav.PressureChange(skillCfg.pressure);
                                        //target.Pressure += skillCfg.pressure;
                                }
                        }
                }
        }

        public static SkillResultData CalcBuffResult(FighterBuffBehav buff, SkillBuffConfig buffCfg, FighterProperty attacker, FighterProperty target)
        {
                SkillResultData rd = new SkillResultData();
                float counterFactor = 1f;
                if (buffCfg.multiplier < 0)
                {
                        counterFactor = CharacterManager.GetInst().GetCareerFamilyCounter(attacker.CareerSys, target.CareerSys) / 100f;

                        float damage = 0;
                        damage = Mathf.Abs(buffCfg.multiplier) / 100f * buff.Attack;
                        damage *= counterFactor;                                                        //计算相克
                        damage *= (1f + buff.DamageIncreasePer / 100f);
                        damage *= (1f - target.GetFinalPropValue("damage_reduce_per") / 100f);//减伤

                        float realDef = target.GetFinalPropValue("total_defence") * (1f - buff.PenetratePer / 100f);

                        if (realDef >= 0f)
                        {
                                float reducePer = realDef / (realDef + DEF_LEVEL_FACTOR * attacker.Level);
                                damage = damage * (1f - reducePer);
                        }
                        else
                        {
                                float reducePer = Mathf.Pow((1f - 1f / (DEF_LEVEL_FACTOR * attacker.Level)), (-1 * realDef)) - 1;
                                damage = damage * (1f - reducePer);
                        }

                        rd.nValue = (int)damage;
                        rd.type = SkillResultType.Damage;
                }
                else if (buffCfg.multiplier > 0)
                {
                        float fHeal = 0f;

                        fHeal = buffCfg.multiplier / 100f * buff.Attack;
                        fHeal *= (1 + buff.HealIncreasePer / 100f);
                        fHeal *= (1 + target.GetFinalPropValue("recover_increase_per") / 100f);

                        rd.nValue = (int)fHeal;
                        rd.type = SkillResultType.Heal;
                }
                return rd;
        }
        static float _nDamage;
        static float m_nDamage
        {
                get
                {
                        return _nDamage;
                }
                set
                {
                        _nDamage = value;
                        //Debuger.Log("damage = " + value);
                }
        }
        static bool IsTargetCondition(FighterBehav attackerBehav, FighterBehav targetBehav, SkillConfig skillCfg)
        {
                if (skillCfg.target_condition.Count < 2)
                        return false;

                int conditionType = skillCfg.target_condition[0];
                int conditionVal = skillCfg.target_condition[1];
                switch (conditionType)
                {
                        default:
                        case 0:
                                {
                                        return false;
                                }
                        case 1:
                                if (targetBehav.FighterProp.Hp >= targetBehav.FighterProp.MaxHp)
                                {
                                        CombatManager.GetInst().RoundInfo_Log("目标满血，符合条件");
                                        return true;
                                }
                                break;
                        case 2:
                                if (targetBehav.HasDeBuff())
                                {
                                        CombatManager.GetInst().RoundInfo_Log("目标有Debuff，符合条件");
                                        return true;
                                }
                                break;
                        case 3:
                                {
                                        int value = targetBehav.FighterProp.GetFinalPropValue(conditionVal);
                                        if (value > 0)
                                        {
                                                CombatManager.GetInst().RoundInfo_Log("检测目标属性id=" + conditionVal + " value=" + value);
                                                return true;
                                        }
                                        else
                                        {
                                                CombatManager.GetInst().RoundInfo_Log("检测目标属性id = " + conditionVal + " 值为0");
                                        }
                                }
                                break;
                        case 4:
                                {
                                        int debuffCnt = targetBehav.GetDeBuffCount();
                                }
                                break;
                }
                return false;
        }

        static int GetDamageIncreaseRate(FighterProperty attackerProp, FighterProperty targetProp, SkillConfig skillCfg)
        {
                return attackerProp.GetFinalPropValue("damage_increase_per");
        }

        public static SkillResultData CalcSkillResult(SkillConfig skillCfg, FighterBehav attackerBehav, FighterBehav targetBehav, int sourceType, int nIndex, int nBrightLvl)
        {
                FighterProperty attackerProp = attackerBehav.FighterProp;
                FighterProperty targetProp = targetBehav.FighterProp;

                SkillResultData rd = new SkillResultData();
                float counterFactor = 1f;
                if (attackerBehav.IsEnemy != targetBehav.IsEnemy)
                {
                        counterFactor = CharacterManager.GetInst().GetCareerFamilyCounter(attackerProp.CareerSys, targetProp.CareerSys) / 100f;
                        CombatManager.GetInst().RoundInfo_Log("抵抗系数 = " + counterFactor);
                }
                bool bCalcRate = false; //是否计算实际发动率。伤害类型要，治疗类型和复活类型不要，辅助类型同阵营不要，敌对阵营需要。
                switch (skillCfg.effect_type)
                {
                        case (int)Skill_Effect_Type.DAMAGE:
                                {
                                        bool bTargetCondition = IsTargetCondition(attackerBehav, targetBehav, skillCfg);
                                        if (CalcMiss(attackerProp, targetProp, nBrightLvl, skillCfg))
                                        {
                                                rd.type = SkillResultType.Miss;
                                                CombatManager.GetInst().RoundInfo_Log("未命中");
                                                return rd;
                                        }
                                        else if (CalcDodge(attackerProp, targetProp, nBrightLvl, skillCfg))
                                        {
                                                rd.type = SkillResultType.Dodge;
                                                CombatManager.GetInst().RoundInfo_Log("闪避成功!!!");
                                                return rd;
                                        }
                                        else
                                        {
                                                rd.type = SkillResultType.Damage;

                                                if (sourceType == 0 || sourceType == 1)
                                                {
                                                        CombatManager.GetInst().CheckAllTriggers(attackerBehav, FIGHTER_TRIGGER_TYPE.ON_ATTACK_BELONGFIGHTER, targetBehav);
                                                        CombatManager.GetInst().CheckAllTriggers(targetBehav, FIGHTER_TRIGGER_TYPE.ON_ACTIVE_SKILL_HIT);
                                                }

                                                m_nDamage = 0;

                                                if (nIndex < skillCfg.GetMultiplier(skillCfg.attack_multiplier, attackerProp).Count)
                                                {
                                                        m_nDamage = skillCfg.GetMultiplier(skillCfg.attack_multiplier, attackerProp)[nIndex] / 100f * attackerProp.GetFinalPropValue("total_attack");    //计算伤害量
                                                        CombatManager.GetInst().RoundInfo_Log("基础伤害=" + m_nDamage.ToString());
                                                }

                                                if (nIndex < skillCfg.GetMultiplier(skillCfg.extra_attributes_multiplier, attackerProp).Count)
                                                {
                                                        m_nDamage += skillCfg.GetMultiplier(skillCfg.extra_attributes_multiplier, attackerProp)[nIndex] / 100f * attackerProp.GetFinalPropValue(skillCfg.extra_attributes);
                                                        CombatManager.GetInst().RoundInfo_Log("额外属性，伤害=" + m_nDamage.ToString());
                                                }

                                                m_nDamage *= counterFactor;                                                        //计算相克
                                                CombatManager.GetInst().RoundInfo_Log("相克，伤害=" + m_nDamage.ToString());

                                                int increaseRate = attackerProp.GetFinalPropValue("damage_increase_per");
                                                if (bTargetCondition)
                                                {
                                                        increaseRate += skillCfg.extra_damage_increase_per;
                                                }
                                                m_nDamage *= (1f + increaseRate / 100f);             //计算伤害增强
                                                CombatManager.GetInst().RoundInfo_Log("伤害增强，伤害=" + m_nDamage.ToString());

                                                if (CalcCritical(attackerProp, targetProp, nBrightLvl, skillCfg, bTargetCondition))
                                                {
                                                        m_nDamage *= (1f + attackerProp.GetFinalPropValue("critical_damage_per") / 100f);

                                                        CombatManager.GetInst().RoundInfo_Log("暴击，伤害=" + m_nDamage.ToString());

                                                        rd.isCritical = true;

                                                        attackerBehav.HasCritical = true;
                                                        attackerBehav.Talk(TALK_TYPE.CRITICAL_HIT);
                                                        CombatManager.GetInst().TeammateTalk(targetBehav, TALK_TYPE.BE_CRITICAL_HIT);
                                                        if (sourceType == 0 || sourceType == 1)
                                                        {
                                                                CombatManager.GetInst().CheckAllTriggers(attackerBehav, FIGHTER_TRIGGER_TYPE.CRITICAL);

                                                                PressureManager.GetInst().PressureChange(BATTLE_PRESSURE_TYPE.CRITICAL_HIT, attackerBehav);
                                                                PressureManager.GetInst().PressureChange(BATTLE_PRESSURE_TYPE.BE_CRITICAL_HIT, targetBehav);
                                                        }
                                                }

                                                if (attackerBehav.IsEnemy)
                                                {
                                                        m_nDamage *= (1f - targetProp.GetFinalPropValue("damage_reduce_per") / 100f + nBrightLvl * 0.08f);//减伤
                                                }
                                                else
                                                {
                                                        m_nDamage *= (1f - targetProp.GetFinalPropValue("damage_reduce_per") / 100f);//减伤
                                                }
                                                CombatManager.GetInst().RoundInfo_Log("目标减伤，伤害=" + m_nDamage.ToString());

                                                if (CalcParry(attackerProp, targetProp))
                                                {
                                                        m_nDamage *= 0.5f;
                                                        CombatManager.GetInst().RoundInfo_Log("目标招架，伤害=" + m_nDamage.ToString());

                                                        rd.isParry = true;

                                                        targetProp.DelayVal -= (int)(targetProp.MaxDelayVal * 0.3f);
                                                        CombatManager.GetInst().RoundInfo_Log("触发格挡时缩减行动条延迟 " + targetProp.FighterId + " " + targetProp.DelayVal);
                                                }

                                                float realDef = targetProp.GetFinalPropValue("total_defence") * (1f - attackerProp.GetFinalPropValue("penetrate_per") / 100f);
                                                if (realDef >= 0f)
                                                {
                                                        float reducePer = realDef / (realDef + DEF_LEVEL_FACTOR * attackerProp.Level);
                                                        m_nDamage = m_nDamage * (1f - reducePer);
                                                }
                                                else
                                                {
                                                        float reducePer = Mathf.Pow((1f - 1f / (DEF_LEVEL_FACTOR * attackerProp.Level)), (-1 * realDef)) - 1;
                                                        m_nDamage = m_nDamage * (1f - reducePer);
                                                }

                                                CombatManager.GetInst().RoundInfo_Log("计算防御 : def=" + realDef + " atklevel=" + attackerProp.Level + " 伤害=" + m_nDamage.ToString());
                                                rd.nLifeSteal = (int)(m_nDamage * (skillCfg.vampire + attackerProp.GetFinalPropValue("life_steal_per")) / 100f);
                                                rd.nRebound = (int)(m_nDamage * attackerProp.GetFinalPropValue("rebound_per") / 100f);
                                                rd.nValue = (int)m_nDamage;

                                                targetBehav.HpMinus(rd.nValue, attackerBehav);

                                                if (rd.nLifeSteal > 0 || skillCfg.vampire > 0)
                                                {
                                                        attackerBehav.HpAdd(rd.nLifeSteal);
                                                        attackerBehav.ShowLifeSteal(rd.nLifeSteal);
                                                }
                                                if (rd.nRebound > 0)
                                                {
                                                        attackerBehav.HpMinus(rd.nRebound, attackerBehav, true);
                                                }
                                                bCalcRate = true;
                                        }
                                }
                                break;
                        case (int)Skill_Effect_Type.HEAL:
                                {
                                        rd.nValue = (int)CalcHeal(skillCfg, attackerProp, targetProp, nIndex);
                                        rd.type = SkillResultType.Heal;
                                        targetBehav.HpAdd(rd.nValue);
                                        if (sourceType == 0 || sourceType == 1)
                                        {
                                                CombatManager.GetInst().CheckAllTriggers(targetBehav, FIGHTER_TRIGGER_TYPE.ON_ACTIVE_SKILL_HEALED);
                                        }
                                        bCalcRate = false;
                                }
                                break;
                        case (int)Skill_Effect_Type.BUFF:
                                {
                                        rd.type = SkillResultType.AddBuff;
                                        bCalcRate = attackerProp.IsEnemy != targetProp.IsEnemy;
                                }
                                break;
                        case (int)Skill_Effect_Type.REVIVE:
                                {
                                        float fReviveVal = 0f;
                                        if (targetProp.Hp <= 0)
                                        {
                                                fReviveVal = skillCfg.GetLevelValue(skillCfg.revive, attackerProp) / 100f * targetProp.MaxHp;
                                                fReviveVal *= (1 + attackerProp.GetFinalPropValue("heal_increase_per") / 100f);
                                                fReviveVal *= (1 + targetProp.GetFinalPropValue("recover_increase_per") / 100f);
                                                rd.type = SkillResultType.Relive;
                                                targetBehav.Relive(skillCfg);
                                        }
                                        else
                                        {
                                                rd.type = SkillResultType.Heal;
                                        }

                                        float fHeal = CalcHeal(skillCfg, attackerProp, targetProp, nIndex);
                                        rd.nValue = (int)(fReviveVal + fHeal);
                                        targetBehav.HpAdd(rd.nValue);

                                        if (sourceType == 0 || sourceType == 1)
                                        {
                                                CombatManager.GetInst().CheckAllTriggers(targetBehav, FIGHTER_TRIGGER_TYPE.ON_ACTIVE_SKILL_HEALED);
                                        }
                                        bCalcRate = false;

                                        CombatManager.GetInst().RoundInfo_Log("复活：" + fReviveVal + " " + fHeal);
                                }
                                break;
                        default:
                                break;
                }
                CheckDelay(skillCfg, attackerProp, targetProp, counterFactor, ref rd, bCalcRate);
                CheckDisperse(skillCfg, attackerProp, targetBehav, counterFactor, ref rd, bCalcRate);
                CheckPressure(skillCfg, attackerProp, targetBehav, counterFactor, ref rd, bCalcRate);
                CheckBrightness(skillCfg, attackerProp, targetBehav, counterFactor, ref rd, bCalcRate);
                CheckBuff(skillCfg, attackerBehav, targetBehav, counterFactor, ref rd, bCalcRate);
                
                //CheckSummon(skillCfg, attackerProp, targetProp, counterFactor, ref rd, bCalcRate);
                //TODO:联合攻击
                return rd;
        }

        static float CalcHeal(SkillConfig skillCfg, FighterProperty attackerProp, FighterProperty targetProp, int nIndex)
        {
                float fHeal = 0f;
                if (nIndex < skillCfg.GetMultiplier(skillCfg.heal_multiplier_per, attackerProp).Count)
                {
                        fHeal = skillCfg.GetMultiplier(skillCfg.heal_multiplier_per, attackerProp)[nIndex] / 100f * targetProp.MaxHp;
                }
                if (fHeal <= 0f && nIndex < skillCfg.GetMultiplier(skillCfg.heal_multiplier, attackerProp).Count)
                {
                        fHeal += skillCfg.GetMultiplier(skillCfg.heal_multiplier, attackerProp)[nIndex] / 100f * attackerProp.GetFinalPropValue("total_attack");
                }
                if (nIndex < skillCfg.GetMultiplier(skillCfg.extra_attributes_multiplier, attackerProp).Count)
                {
                        fHeal += skillCfg.GetMultiplier(skillCfg.extra_attributes_multiplier, attackerProp)[nIndex] / 100f * attackerProp.GetFinalPropValue(skillCfg.extra_attributes);
                }

                fHeal *= (1 + attackerProp.GetFinalPropValue("heal_increase_per") / 100f);
                fHeal *= (1 + targetProp.GetFinalPropValue("recover_increase_per") / 100f);

                return fHeal;
        }
}
