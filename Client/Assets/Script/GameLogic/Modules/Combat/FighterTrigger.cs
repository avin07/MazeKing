// using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;
// public enum FIGHTER_TRIGGER_TYPE
// {
//         COMBAT_START,                           //开场
//         ON_DEAD,                                //死亡
//         KILL_OTHER,                             // 击杀单位
//         BEFORE_ACTION,                  // 行动前
//         AFTER_ACTION,                           // 行动后
//         ON_ACTIVE_SKILL_HIT,        // 受到伤害类的主动技能和其追加技能的打击
//         ON_ACTIVE_SKILL_HEALED,         // 受到治疗类的主动技能和其追加技能的治疗
//         CRITICAL,                                       // 攻击暴击
//         ON_ATTACK_BELONGFIGHTER,        //攻击挂载者
// };
// 
// public class FighterTrigger 
// {
//         public SkillTriggerConfig m_TriggerCfg;
//         public FighterBehav m_BelongFighter;
//         public FighterBehav m_Caster;
//         public long m_TriggerId = 0;
// 
//         public FighterTrigger(FighterBehav belongFighter, FighterBehav caster, SkillTriggerConfig triggerCfg)
//         {
//                 m_TriggerCfg = triggerCfg;
//                 m_BelongFighter = belongFighter;
//                 m_Caster = caster;
//                 m_TriggerId = belongFighter.FighterId * 1000 + triggerCfg.id;
//         }
// 
//         FighterBehav m_TriggerTarget;
//         public bool TryTrigger(FighterBehav triggerTarget, FIGHTER_TRIGGER_TYPE type, FighterBehav damageTarget)
//         {
//                 if (m_TriggerCfg.trigger_action == (int)type)
//                 {
//                         //如果触发行为是攻击挂载者，则检测攻击对象是否挂载者。
//                         if (type == FIGHTER_TRIGGER_TYPE.ON_ATTACK_BELONGFIGHTER && damageTarget.FighterId != this.m_BelongFighter.FighterId)
//                         {
//                                 return false;
//                         }
// 
//                         if (CombatManager.GetInst().CanTriggerSkill(this) == false)
//                         {
//                                 CombatManager.GetInst().RoundInfo_Log("已经触发过 " + this.m_TriggerId);
//                                 return false;
//                         }
// 
//                         bool bAvailable = false;
//                         switch (m_TriggerCfg.target)
//                         {
//                                 case 0:
//                                         if (triggerTarget == m_BelongFighter)
//                                         {
//                                                 bAvailable = true;
//                                         }
//                                         break;
//                                 case 1:
//                                         if (triggerTarget == m_Caster)
//                                         {
//                                                 bAvailable = true;
//                                         }
//                                         break;
//                                 case 2:
//                                         if (triggerTarget.IsEnemy == m_BelongFighter.IsEnemy)
//                                         {
//                                                 bAvailable = true;
//                                         }
//                                         break;
//                                 case 3:
//                                         if (triggerTarget.IsEnemy != m_BelongFighter.IsEnemy)
//                                         {
//                                                 bAvailable = true;
//                                         }
//                                         break;
//                                 case 4:
//                                         if (triggerTarget.IsEnemy != m_BelongFighter.IsEnemy && triggerTarget.IsFront)
//                                         {
//                                                 bAvailable = true;
//                                         }
//                                         break;
//                                 case 5:
//                                         if (triggerTarget.IsEnemy != m_BelongFighter.IsEnemy && triggerTarget.IsFront == false)
//                                         {
//                                                 bAvailable = true;
//                                         }
//                                         break;
//                                 case 6:
//                                         if (triggerTarget.IsEnemy == m_BelongFighter.IsEnemy && triggerTarget.IsFront == m_BelongFighter.IsFront)
//                                         {
//                                                 bAvailable = true;
//                                         }
//                                         break;
//                         }
//                         if (bAvailable)
//                         {
//                                 m_TriggerTarget = triggerTarget;
//                                 if (DamageCalc.GetRandomVal(1, 10000, "触发器概率") <= m_TriggerCfg.trigger_rate)
//                                 {
//                                         CombatManager.GetInst().AddTriggerSkill(this);
//                                         CombatManager.GetInst().RoundInfo_Log("触发成功"+m_TriggerCfg.id + " 挂载者=" + m_BelongFighter.FighterId + " 类型=" + type.ToString() + " 触发对象=" + triggerTarget.FighterId);
//                                         return true;
//                                 }
//                         }
//                 }
//                 return false;
//         }
// 
//         public ActionSkillData CalcSkillTarget()
//         {
//                 CombatManager.GetInst().RoundInfo_Log("触发技能Begin id=" + m_TriggerCfg.id);
//                 ActionSkillData ret = null;
// 
//                 FighterBehav caster = null;
//                 FighterBehav mainTarget = null;
//                 switch (m_TriggerCfg.trigger_skill_releaser)
//                 {
//                         case 0:
//                                 caster = m_BelongFighter;
//                                 break;
//                         case 1:
//                                 caster = m_Caster;
//                                 break;
//                         case 2:
//                                 caster = m_TriggerTarget;
//                                 break;
//                 }
// 
//                 if (caster == null)
//                 {
//                         CombatManager.GetInst().RoundInfo_Log("触发技能 skillId=" + m_TriggerCfg.trigger_skill_id + " 施法者 = null" );
//                         return null;
//                 }
// 
//                 SkillConfig skillCfg = SkillManager.GetInst().GetActiveSkill(m_TriggerCfg.trigger_skill_id);
//                 if (skillCfg != null)
//                 {                        
//                         if (caster.FighterProp.Hp <= 0)
//                         {                        
//                                 CombatManager.GetInst().RoundInfo_Log("触发技能 skillId=" + m_TriggerCfg.trigger_skill_id + " 施放者已死");
//                                 return null;
//                         }
//                         if (caster.IsStun())
//                         {
//                                 CombatManager.GetInst().RoundInfo_Log("触发技能 skillId=" + m_TriggerCfg.trigger_skill_id + " 施放者晕眩中");
//                                 return null;
//                         }
// 
//                         if (caster.IsSilence() && skillCfg.is_silence == 1)
//                         {
//                                 CombatManager.GetInst().RoundInfo_Log("触发技能 skillId=" + m_TriggerCfg.trigger_skill_id + " 施放者被沉默");
//                                 return null;
//                         }
// 
//                         switch (skillCfg.trigger_skill_range_type)
//                         {
//                                 case 0:
//                                         {
//                                                 List<FighterBehav> list = caster.CalcSkillTargetGroup(skillCfg);
//                                                 mainTarget = CombatManager.GetInst().GetRandomTarget(list, "触发器类型0选取目标"+ skillCfg.id);
//                                         }
//                                         break;
//                                 case 1:
//                                 case 2:
//                                         {
//                                                 mainTarget = m_BelongFighter;
//                                         }
//                                         break;
//                                 case 3:
//                                 case 4:
//                                         {
//                                                 mainTarget = m_Caster;
//                                         }
//                                         break;
//                                 case 5:
//                                 case 6:
//                                         {
//                                                 mainTarget = m_TriggerTarget;
//                                         }
//                                         break;
//                         }
//                 }
// 
// 
//                 if (mainTarget == null)
//                 {
//                         CombatManager.GetInst().RoundInfo_Log("触发技能 skillId=" + skillCfg.id + " 主目标 == " + null);
//                 }
//                 else
//                 {
//                         //判断技能targetgroup合理性
//                         if (skillCfg.IsAffectEnemy())
//                         {
//                                 if (caster.IsEnemy != mainTarget.IsEnemy)
//                                 {
//                                         ret = new ActionSkillData(skillCfg, caster, mainTarget);
//                                 }
//                         }
//                         else
//                         {
//                                 if (caster.IsEnemy == mainTarget.IsEnemy)
//                                 {
//                                         ret = new ActionSkillData(skillCfg, caster, mainTarget);
//                                 }
//                         }
//                         if (ret == null)
//                         {
//                                 CombatManager.GetInst().RoundInfo_Log("触发技能 skillId=" + skillCfg.id + "选取的主目标不符合targetgroup合理性");
//                         }
//                 }
//                 return ret;
//         }
// }
