using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Message;

class PressureManager : SingletonObject<PressureManager>
{
        Dictionary<int, BattlePressureConfig> m_BattlePressureDict = new Dictionary<int, BattlePressureConfig>();
        Dictionary<int, PressureJudgeConfig> m_PressureJudgeDict = new Dictionary<int, PressureJudgeConfig>();

        int m_nMaxWeight = 0;
        public void Init()
        {
                ConfigHoldUtility<BattlePressureConfig>.LoadXml("Config/battle_pressure", m_BattlePressureDict);
                ConfigHoldUtility<PressureJudgeConfig>.LoadXml("Config/pressure_result", m_PressureJudgeDict);

                m_nMaxWeight = 0;
                foreach (PressureJudgeConfig pjc in m_PressureJudgeDict.Values)
                {
                        m_nMaxWeight += pjc.weight;
                }

                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgPressureTorture), OnPressureTorture);
        }

        int GeneratePressureValue(int id)
        {
                if (m_BattlePressureDict.ContainsKey(id))
                {
                        BattlePressureConfig bpc = m_BattlePressureDict[id];
                        if (DamageCalc.CheckRandom(bpc.rate, "压力变化"))
                        {
                                return DamageCalc.GetRandomVal(bpc.min_pressure, bpc.max_pressure, "压力变化数值");
                        }
                }
                return 0;
        }

        public void PressureChange(BATTLE_PRESSURE_TYPE type, FighterBehav triggerFighter)
        {
                //敌方不触发压力计算
                if (triggerFighter.IsEnemy)
                        return;

                Debuger.Log("压力变化PressureChange " + type + " FighterID=" + triggerFighter.FighterId);
                foreach (FighterBehav fighter in CombatManager.GetInst().GetFighterList(triggerFighter.IsEnemy))
                {
                        if (fighter.FighterProp.Hp <= 0)
                                continue;

                        bool bMyself = fighter.FighterId == triggerFighter.FighterId;
                        int value = 0;
                        switch (type)
                        {
                                case BATTLE_PRESSURE_TYPE.BE_CRITICAL_HIT:
                                        {
                                                value = GeneratePressureValue(bMyself ? 1 : 2);
                                        }
                                        break;
                                case BATTLE_PRESSURE_TYPE.BE_KILLED:
                                        {
                                                if (!bMyself)
                                                {
                                                        value = GeneratePressureValue(3);
                                                }
                                        }
                                        break;
                                case BATTLE_PRESSURE_TYPE.CRITICAL_HIT:
                                        {
                                                value = GeneratePressureValue(bMyself ? 4 : 5);
                                        }
                                        break;
                                case BATTLE_PRESSURE_TYPE.KILLED:
                                        {
                                                value = GeneratePressureValue(bMyself ? 6 : 7);
                                        }
                                        break;
                        }
                        fighter.PressureChange(value);
                }
        }

        public bool PressureJudge_InBattle(FighterBehav triggerFighter)
        {
                if (triggerFighter.IsEnemy)
                        return false;

                if (triggerFighter.FighterProp.HasTortured)
                        return false;

                int delta = triggerFighter.FighterProp.Pressure - triggerFighter.FighterProp.Pressure_LR;
                if (delta > 0 && triggerFighter.FighterProp.IsPressureOverLimit())
                {
                        CombatManager.GetInst().RoundInfo_Log(triggerFighter.FighterId + "精神拷问");

                        triggerFighter.FighterProp.HasTortured = true;
                        
                        GameUtility.ShowTip(LanguageManager.GetText("pressure_result_hint_word"));
                        PressureJudgeConfig pjc = GetJudgeCfg();
                        if (pjc != null)
                        {
                                triggerFighter.Talk(LanguageManager.GetText(pjc.talk));
                                GameObject obj = EffectManager.GetInst().GetEffectObj(pjc.type == 1 ? "effect_raid_pressure_result_good" : "effect_raid_pressure_result_bad");
                                obj.transform.SetParent(triggerFighter.transform);
                                obj.transform.localPosition = Vector3.zero;
                                obj.transform.localRotation = Quaternion.identity;

                                foreach (FighterBehav fighter in CombatManager.GetInst().GetFighterList(triggerFighter.IsEnemy))
                                {
                                        if (fighter.FighterProp.Hp <= 0)
                                                continue;

                                        if (pjc.range_type == 1 && fighter.FighterId != triggerFighter.FighterId)
                                        {
                                                continue;
                                        }

                                        if (pjc.pressure != 0)
                                        {
                                                fighter.PressureChange(pjc.pressure);
                                        }
                                        if (pjc.heal_multiplier_per != 0)
                                        {
                                                fighter.HpChange((int)(pjc.heal_multiplier_per / 100f * fighter.FighterProp.MaxHp), null);
                                        }
                                }

                                if (pjc.buff > 0)
                                {
                                        triggerFighter.AddBuff(pjc.buff, null, true);
                                }
                        }
                        return true;
                }
                return false;
        }

        PressureJudgeConfig GetJudgeCfg()
        {
                int randomVal = DamageCalc.GetRandomVal(1, m_nMaxWeight, "压力权重");
                int tmp = 0;
                foreach (PressureJudgeConfig pjc in m_PressureJudgeDict.Values)
                {
                        tmp += pjc.weight;
                        if (randomVal <= tmp)
                        {
                                CombatManager.GetInst().RoundInfo_Log("精神拷问结果=" + pjc.mark);
                                return pjc;
                        }
                }
                return null;
        }

        void OnPressureTorture(object sender, SCNetMsgEventArgs e)
        {
                SCMsgPressureTorture msg = e.mNetMsg as SCMsgPressureTorture;
                if (m_PressureJudgeDict.ContainsKey(msg.idCfg))
                {
                        PressureJudgeConfig pjc = m_PressureJudgeDict[msg.idCfg];

                        HeroUnit unit = RaidTeamManager.GetInst().GetHeroUnitByID(msg.idHero);
                        if (unit != null)
                        {
                                string text = CharacterManager.GetInst().GetCharacterName(unit.CharacterID) + LanguageManager.GetText("pressure_result_hint_word");

                                UIManager.GetInst().ShowUI<UI_RaidAside>("UI_RaidAside").SetText(text);
                                RaidManager.GetInst().PressureTorture(unit, pjc);
                        }
                }
        }
}
