using UnityEngine;
using System.Collections;

public class FighterBuffBehav : MonoBehaviour 
{
        public int BuffID
        {
                get
                {
                        if (m_BuffCfg != null)
                        {
                                return m_BuffCfg.id;
                        }
                        return 0;
                }
        }

        public FighterBehav m_Caster;
        public FighterBehav m_BelongFighter;
        public SkillBuffConfig m_BuffCfg;
        GameObject m_EffectObj;
        int m_nRound = 0;
        bool m_bPlayStartEffect = false;

        public int LeftRound
        {
                get
                {
                        return m_nRound;
                }
                set
                {
                        m_nRound = value;
                }
        }
        public FighterTrigger m_Trigger;

        bool m_bShield = false;
        int m_nShieldHp = 0;
        public int ShieldHp
        {
                get { return m_nShieldHp; }
                set { 
                        m_nShieldHp = value;
                        if (m_bShield && m_nShieldHp <= 0)
                        {
                                End();
                        }
                }
        }

        public int Attack;
        public int DamageIncreasePer;
        public int PenetratePer;
        public int HealIncreasePer;

        public int m_nTriggerCount;

        public void Setup(SkillBuffConfig cfg, FighterBehav caster, FighterBehav belong)
        {
                m_Caster = caster;
                m_BelongFighter = belong;
                m_BuffCfg = cfg;

                m_nTriggerCount = m_BuffCfg.trigger_number;
                m_bShield = cfg.is_shield == 1;
                if (m_bShield)
                {
                        if (cfg.target_heal_multiplier_per > 0)
                        {
                                m_nShieldHp = (int)(belong.FighterProp.MaxHp * cfg.target_heal_multiplier_per / 100f);
                        }
                        else if (cfg.cast_attack_multiplier_per > 0)
                        {
                                if (caster != null)
                                {
                                        m_nShieldHp = (int)(caster.FighterProp.GetFinalPropValue("total_attack") * cfg.cast_attack_multiplier_per / 100f);
                                }
                        }
                }
                if (caster != null)
                {
                        Attack = caster.FighterProp.GetFinalPropValue("total_attack");
                        CombatManager.GetInst().RoundInfo_Log("初始化BUFF：" + m_BuffCfg.id + " 记录施法者攻击=" + Attack);
                        DamageIncreasePer = caster.FighterProp.GetFinalPropValue("damage_increase_per");
                        PenetratePer = caster.FighterProp.GetFinalPropValue("penetrate_per");
                        HealIncreasePer = caster.FighterProp.GetFinalPropValue("heal_increase_per");
                }

                if (m_BuffCfg.effective_type == 2)
                {
                        if (m_BelongFighter != null)
                        {
                                m_BelongFighter.CalcExtraAttribute(m_Caster, m_BuffCfg.attributes);
                        }
                }

                m_nRound = cfg.time;
                if (m_BuffCfg.call_trigger_id > 0)
                {
                        SkillTriggerConfig triggercfg = SkillManager.GetInst().GetTriggerCfg(m_BuffCfg.call_trigger_id);
                        if (triggercfg != null)
                        {
                                m_Trigger = new FighterTrigger(this.m_BelongFighter, this.m_Caster, triggercfg);
                        }
                }

                if (m_BuffCfg.is_action_fail == 1)
                {
                        m_bSkipCheckActionFail = true;  //添加的当回合不检测是否行动删除该BUFF
                }
        }

        public void StartEffect()
        {
                if (m_BuffCfg == null)
                        return;
                if (m_bPlayStartEffect)
                {
                        Debuger.Log("m_bPlayStartEffect = true");
                        return;
                }
                m_BelongFighter.AddBuffIcon(this);
                m_bPlayStartEffect = true;
                if (!string.IsNullOrEmpty(m_BuffCfg.add_effect) && m_BuffCfg.add_effect != "-1")
                {
                        GameObject effectObj = EffectManager.GetInst().GetEffectObj(m_BuffCfg.add_effect);
                        if (effectObj != null)
                        {
                                effectObj.transform.SetParent(this.gameObject.transform);
                                effectObj.transform.localPosition = Vector3.zero;
                                effectObj.transform.localRotation = Quaternion.identity;
//                                 GameObject.Destroy(effectObj, 10f);
                        }
                }
                if (!string.IsNullOrEmpty(m_BuffCfg.exist_effect) && m_BuffCfg.exist_effect != "-1")
                {
                        m_EffectObj = EffectManager.GetInst().GetEffectObj(m_BuffCfg.exist_effect);
                        if (m_EffectObj != null)
                        {
                                m_EffectObj.transform.SetParent(this.gameObject.transform);
                                m_EffectObj.transform.localPosition = Vector3.zero;
                                m_EffectObj.transform.localRotation = Quaternion.identity;
                        }
                }
        }
        public void RecoverAttributeChange()
        {
                if (m_BelongFighter != null)
                {
                        if (m_BuffCfg.effective_type == 2 && !string.IsNullOrEmpty(m_BuffCfg.attributes))
                        {
                                m_BelongFighter.RecoverAttributeChange(m_BuffCfg.attributes);
                        }
                }
        }

        public void DestroySelf()
        {
                RecoverAttributeChange();
                GameObject.Destroy(m_EffectObj);
                GameObject.Destroy(this.gameObject);
        }
        public void End()
        {
                if (m_BelongFighter != null)
                {
                        m_BelongFighter.DelBuff(this);
                }
        }

        void CalcEffect()
        {
                if (m_BelongFighter != null)
                {
                        m_BelongFighter.CalcBuffResult(this, m_Caster, m_BuffCfg);
                }
        }

        public void OnFighterAction()
        {
                if (m_nRound < 99)
                {
                        m_nRound--;
                        m_BelongFighter.UpdateBuffIcon(this);
                }
                //每回合行动前都生效
                if (m_BuffCfg.effective_type == 1)
                {
                        CalcEffect();
                }
                if (m_nRound == 0)
                {
                        //仅最后一回合生效
                        if (m_BuffCfg.effective_type == 3)
                        {
                                CalcEffect();
                        }
                        //最后一回合生效后失效
                        if (m_BuffCfg.fail_type == 1)
                        {
                                End();
                        }
                }
        }
        public void AfterFighterAction()
        {
                if (m_nRound == 0)
                {
                        //最后一回合行动后失效
                        if (m_BuffCfg.fail_type == 2)
                        {
                                End();
                        }
                }
        }
        public void TryTrigger(FighterBehav triggerTarget, FIGHTER_TRIGGER_TYPE action, FighterBehav damageTarget = null)
        {
                if (m_Trigger != null)
                {
                        if (m_Trigger.TryTrigger(triggerTarget, action, damageTarget))
                        {
                                if (m_nTriggerCount > 0)
                                {
                                        m_nTriggerCount--;
                                        if (m_nTriggerCount == 0)
                                        {
                                                End();
                                        }
                                }
                        }
                }
        }

        public bool m_bSkipCheckActionFail = false;
        public bool IsActionFail()
        {
                if (m_bSkipCheckActionFail)
                {
                        m_bSkipCheckActionFail = false;
                        return false;
                }

                return m_BuffCfg.is_action_fail == 1;
        }
}
