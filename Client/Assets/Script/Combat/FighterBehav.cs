using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum FIGHTER_ACTION
{
        NONE,
        ACTION,
        END,
        DIE,
};

public class FighterBehav : MonoBehaviour
{
        const string IDLE_ANIM_NAME = "idle_002";
        float HIT_PAUSE_TIME = 0.1f;

        public static int CompareSpeed(FighterBehav x, FighterBehav y)
        {
                if (x.FighterProp.DelayVal < y.FighterProp.DelayVal)
                {
                        return -1;
                }
                else if (x.FighterProp.DelayVal > y.FighterProp.DelayVal)
                {
                        return 1;
                }
                else if (x.FighterProp.CharacterID > y.FighterProp.CharacterID)
                {
                        return -1;
                }
                else if (x.FighterProp.CharacterID < y.FighterProp.CharacterID)
                {
                        return 1;
                }
                else if (x.FighterId > y.FighterId)
                {
                        return -1;
                }
                else
                {
                        return 1;
                }
        }

        int m_nCharacterId = 0;
        public int CharacterId
        {
                get
                {
                        return m_nCharacterId;
                }
                set
                {
                        m_nCharacterId = value;
                }
        }

        long m_nFighterId = 0;
        public long FighterId
        {
                get
                {
                        return m_nFighterId;
                }
                set
                {
                        m_nFighterId = value;
                }
        }
        /// <summary>
        /// 是否敌方
        /// </summary>
        public bool IsEnemy = false;
        /// <summary>
        /// 是否前排
        /// </summary>
        public bool IsFront
        {
                get
                {
                        return (FighterId % 10) <= 3;
                }
        }
        public int BattlePos = -1;

        FighterProperty m_FighterProp;
        public FighterProperty FighterProp
        {
                get
                {
                        if (m_FighterProp == null)
                        {
                                m_FighterProp = new FighterProperty(CharacterId, this.FighterId);
                                m_FighterProp.IsEnemy = this.IsEnemy;
                        }
                        return m_FighterProp;
                }
        }
        CharacterConfig m_CharacterCfg;
        public CharacterConfig CharacterCfg
        {
                get
                {
                        if (m_CharacterCfg == null)
                        {
                                m_CharacterCfg = CharacterManager.GetInst().GetCharacterCfg(CharacterId);
                        }
                        return m_CharacterCfg;
                }
        }
        public UnitAnim AnimComp
        {
                get
                {
                        return GetComponent<UnitAnim>();
                }
        }

        FIGHTER_ACTION m_ActionState = FIGHTER_ACTION.NONE;
        public FIGHTER_ACTION ActionState
        {
                get
                {
                        return m_ActionState;
                }
                set
                {
                        m_ActionState = value;
                }
        }

        FighterBehav m_TauntTarget;
        public FighterBehav TauntTarget
        {
                get
                {
                        return m_TauntTarget;
                }
                set
                {
                        m_TauntTarget = value;
                }
        }

        public bool m_bHide = false;
        public bool IsHide
        {
                get
                {
                        return m_bHide;
                }
                set
                {
                        if (value != m_bHide)
                        {
                                TransparentSwitch ts = this.GetComponent<TransparentSwitch>();
                                if (ts == null)
                                {
                                        ts = this.gameObject.AddComponent<TransparentSwitch>();
                                }
                                ts.enabled = value;
                        }
                        m_bHide = value;
                }
        }
        Dictionary<int, int> m_SkillCdDict = new Dictionary<int, int>();
        List<FighterTrigger> m_PassiveTriggerList = new List<FighterTrigger>();

        public Transform GetTransform(string name)
        {
                Transform belongT = GameUtility.GetTransform(this.transform.gameObject, name);
                if (belongT == null)
                {
                        belongT = this.transform;
                }
                return belongT;
        }

        void Awake()
        {
                if (GetComponent<CharacterController>() == null)
                {
                        CharacterController cc = this.gameObject.AddComponent<CharacterController>();
                        cc.center = Vector3.up * 0.5f;
                }

                if (GetComponent<UnitAnim>() == null)
                {
                        this.gameObject.AddComponent<UnitAnim>();
                }
                PlayIdle();

                HIT_PAUSE_TIME = GlobalParams.GetFloat("hit_pause_time") / 1000f;
        }
        void Start()
        {
                InitStatusUI();
                UpdateHP_UI();
                InitSkillCD();
                InitBufflist();
                InitPassiveSkills();

                FighterProp.ResetDelay();

                if (CharacterCfg.body_type == 2)
                {
                        CharacterController cc = this.gameObject.GetComponent<CharacterController>();
                        cc.radius = 2;
                        cc.height = 5;
                        cc.center = Vector3.up * 2.5f;
                }
        }
        public void PlayIdle()
        {
                if (AnimComp != null && AnimComp.AnimCtr != null)
                {
                        if (AnimComp.AnimCtr.GetClip(IDLE_ANIM_NAME) != null)
                        {
                                AnimComp.PlayAnim(IDLE_ANIM_NAME, true);
                        }
                        else
                        {
                                AnimComp.PlayAnim("idle_001", true);
                        }
                }
        }
        public void SetOriPosition(Vector3 pos)
        {
                m_OriPos = pos;
        }

        public void MoveToOriPos(Vector3 pos)
        {
                StartCoroutine(ProcessingMoveToOri(pos));
        }

        IEnumerator ProcessingMoveToOri(Vector3 pos)
        {
                SetOriPosition(pos);

                GameUtility.RotateTowards(this.transform, pos);
                float time = Vector3.Distance(this.transform.position, pos) / m_fSpeed;
                if (time > 0.05f)
                {
                        ActionState = FIGHTER_ACTION.ACTION;
                        AnimComp.PlayAnim("run_001", true);
                        iTween.moveToWorld(this.gameObject, time, 0f, m_OriPos);
                        yield return new WaitForSeconds(time);
                }
                else
                {
                        this.transform.position = pos;
                }
                AnimComp.StopAnim();
                PlayIdle();
                this.transform.rotation = CombatManager.GetInst().GetForward(this.IsEnemy);

                ActionState = FIGHTER_ACTION.NONE;
        }

        void InitBufflist()
        {
                foreach (int buffId in FighterProp.GetBuffDict().Keys)
                {
                        AddBuff(buffId, this);
                }
        }

        void InitPassiveSkills()
        {
                foreach (int skillId in FighterProp.GetPassiveSkillList().Keys)
                {
                        PassiveSkillConfig cfg = SkillManager.GetInst().GetPassiveSkill(skillId);
                        if (cfg != null)
                        {
                                SkillTriggerConfig triggercfg = SkillManager.GetInst().GetTriggerCfg(cfg.trigger_id);
                                if (triggercfg != null)
                                {
                                        FighterTrigger trigger = new FighterTrigger(this, this, triggercfg);
                                        m_PassiveTriggerList.Add(trigger);
                                }
                                if (!cfg.attributes.Equals(CommonString.zeroStr))
                                {
                                        //被动技能加永久值，由服务端算好了
                                }
                                if (!cfg.attributes_2.Equals(CommonString.zeroStr))
                                {
                                        FighterProp.AddTransformRelations(cfg.attributes_2, 2);
                                }
                                if (!cfg.attributes_3.Equals(CommonString.zeroStr))
                                {
                                        FighterProp.AddTransformRelations(cfg.attributes_3, 3);
                                }
                                if (!cfg.attributes_4.Equals(CommonString.zeroStr))
                                {
                                        FighterProp.AddTransformRelations(cfg.attributes_4, 4);
                                }
                                if (!cfg.attributes_5.Equals(CommonString.zeroStr))
                                {
                                        FighterProp.AddTransformRelations(cfg.attributes_5, 5);
                                }

                        }
                }
        }

        void InitSkillCD()
        {
                foreach (int skillId in FighterProp.GetActiveSkillList())
                {
                        SkillConfig cfg = SkillManager.GetInst().GetActiveSkill(skillId);
                        if (cfg != null)
                        {
                                m_SkillCdDict.Add(cfg.id, cfg.init_cool_down);
                        }
                }
        }

        public void ResetSkillCD(int skillId)
        {
                SkillConfig cfg = SkillManager.GetInst().GetActiveSkill(skillId);
                if (cfg != null)
                {
                        if (m_SkillCdDict.ContainsKey(cfg.id))
                        {
                                m_SkillCdDict[cfg.id] = cfg.GetLevelValue(cfg.cool_down, FighterProp);
                        }
                }
        }

        public int GetSkillCD(int skillID)
        {
                if (m_SkillCdDict.ContainsKey(skillID))
                {
                        return m_SkillCdDict[skillID];
                }
                return 0;
        }

        int CompareCD(SkillConfig x, SkillConfig y)
        {
                if (x.GetLevelValue(x.cool_down, FighterProp) > y.GetLevelValue(y.cool_down, FighterProp))
                {
                        return -1;
                }
                else if (x.GetLevelValue(x.cool_down, FighterProp) < y.GetLevelValue(y.cool_down, FighterProp))
                {
                        return 1;
                }
                else
                {
                        if (x.id > y.id)
                        {
                                return -1;
                        }
                        else
                        {
                                return 1;
                        }
                }
        }

        public SkillConfig GetNormalSkill()
        {
                foreach (int skillId in FighterProp.GetActiveSkillList())
                {
                        SkillLearnConfigHold cfg = SkillManager.GetInst().GetSkillInfo(skillId);

                        if (cfg != null)
                        {
                                if (cfg.is_base_skill == 1)
                                {
                                        return SkillManager.GetInst().GetActiveSkill(skillId);
                                }
                        }
                }

                return null;
        }

        public List<SkillConfig> GetAvailSkill(bool bNeedSort = true)
        {
                List<SkillConfig> list = new List<SkillConfig>();

                foreach (int key in m_SkillCdDict.Keys)
                {
                        if (m_SkillCdDict[key] <= 0)
                        {
                                SkillConfig cfg = SkillManager.GetInst().GetActiveSkill(key);
                                if (IsSilence() == false || cfg.is_silence == 0)
                                {
                                        list.Add(cfg);
                                }
                        }
                }
                if (bNeedSort)
                {
                        list.Sort(CompareCD);
                }
                return list;
        }
        public void UpdateSkillCD()
        {
                foreach (int skillId in new List<int>(m_SkillCdDict.Keys))
                {
                        if (m_SkillCdDict[skillId] > 0)
                        {
                                m_SkillCdDict[skillId]--;
                        }
                }
        }


        SkillConfig m_ActionSkillCfg;
        FighterBehav m_MainTarget;
        Vector3 m_OriPos;
        int m_nSplitCount = 0;
        int m_nMaxSplitCount = 1;
        float m_fMoveTime = 0f;
        float m_fMaxMoveTime = 0.3f;

        float m_fStartAnimTime = 0f;
        float m_fMaxAnimTime = 0f;
        float m_fHitTime = 0f;

        float m_fWaitTime = 0f;
        float m_fMaxWaitTime = 0f;

        class ACTION_RESULT
        {
                public FighterBehav targetBehav;
                public SkillResultData srd;
                public ACTION_RESULT()
                {

                }
                public ACTION_RESULT(FighterBehav behav, SkillResultData _srd)
                {
                        targetBehav = behav;
                        srd = _srd;
                }
        }

        public bool HasKill = false;
        bool m_bHasCritical = false;
        public bool HasCritical
        {
                get
                {
                        return m_bHasCritical;
                }
                set
                {
                        m_bHasCritical = value;
                        CombatManager.GetInst().RoundInfo_Log(FighterId + "HasCritical = " + value);
                }
        }
        public bool HasBuffAdded = false;
        Queue<List<ACTION_RESULT>> m_TargetRetQueue = new Queue<List<ACTION_RESULT>>();
        List<ACTION_RESULT> m_CurrentResultList = new List<ACTION_RESULT>();

        int m_nPlayingHitCount = 0;
        int m_nSkillSourceType = 0;//0 主动释放（包括玩家选择，托管和AI选择） 1 追加释放 2 触发释放 

        int CompareHp(FighterBehav x, FighterBehav y)
        {
                if (x.FighterProp.Hp < y.FighterProp.Hp)
                {
                        return -1;
                }
                else if (x.FighterProp.Hp > y.FighterProp.Hp)
                {
                        return 1;
                }
                else
                {
                        return x.FighterProp.CharacterID.CompareTo(y.FighterProp.CharacterID);
                }
        }

        public void GoExtraSkill(SkillConfig skillCfg, FighterBehav mainTarget)
        {
                m_ActionSkillCfg = skillCfg;
                m_MainTarget = mainTarget;
                m_nSkillSourceType = 1;
                InitAction();
        }

        public void GoTriggerSkill(SkillConfig skillCfg, FighterBehav mainTarget)
        {
                m_ActionSkillCfg = skillCfg;
                m_MainTarget = mainTarget;
                m_nSkillSourceType = 2;
                InitAction();
                if (m_BossStatus != null)
                {
                        m_BossStatus.ShowPassiveSkill(m_ActionSkillCfg.id);
                }
                else if (m_UIStatus != null)
                {
                        m_UIStatus.ShowPassiveSkill(m_ActionSkillCfg.id);
                }
        }
        public void GoActiveSkill(SkillConfig skillCfg, FighterBehav mainTarget)
        {
                m_ActionSkillCfg = skillCfg;
                m_MainTarget = mainTarget;
                m_nSkillSourceType = 0;
                InitAction();
                if (m_BossStatus != null)
                {
                        m_BossStatus.ShowActiveSkill(SkillManager.GetInst().GetSkillName(m_ActionSkillCfg.id));
                }
                else if (m_UIStatus != null)
                {
                        m_UIStatus.ShowActiveSkill(SkillManager.GetInst().GetSkillName(m_ActionSkillCfg.id));
                }
                if (mainTarget != null)
                {
                        mainTarget.SetSelectCircle(true);
                }
        }

        void InitAction()
        {
                if (IsDead())
                {
                        CombatManager.GetInst().RoundInfo_Log("Fighter " + FighterId + " is Dead");
                        return;
                }
                if (m_ActionSkillCfg == null)
                {
                        CombatManager.GetInst().RoundInfo_Log("[ERROR] 技能取不到配置");
                        return;
                }
                CombatManager.GetInst().RoundInfo_Log("行动者= " + FighterId + " 技能= " + m_ActionSkillCfg.id + "(Lv." + FighterProp.GetActiveSkillLevel(m_ActionSkillCfg.id) + ") 主目标= " + m_MainTarget != null ? m_MainTarget.FighterId.ToString() : "0", 2);

                if (IsStun() || (IsSilence() && m_ActionSkillCfg.is_silence == 1))
                {
                        CombatManager.GetInst().RoundInfo_Log("Fighter " + this.FighterId + " stun=" + FighterProp.GetFinalPropValue("stun_buf") + " silence=" + FighterProp.GetFinalPropValue("silence_buf"));
                        ActionEnd();
                        return;
                }

                ActionState = FIGHTER_ACTION.ACTION;
                StartCoroutine(ProcessingSkill());
        }

        public List<FighterBehav> CalcSkillTargetGroup(SkillConfig skillCfg, bool bCheckStealth = true)
        {
                List<FighterBehav> targetlist = new List<FighterBehav>();
                //AOE
                switch (skillCfg.target_group)
                {
                        case (int)SkillTargetGroup.ENEMY:
                                {
                                        foreach (FighterBehav behav in CombatManager.GetInst().GetFighterList(!this.IsEnemy))
                                        {
                                                if (behav.FighterProp.Hp > 0)
                                                {
                                                        if (bCheckStealth && behav.IsStealth())
                                                                continue;

                                                        targetlist.Add(behav);
                                                }
                                        }
                                }
                                break;

                        case (int)SkillTargetGroup.ENEMY_FRONT:
                                {
                                        foreach (FighterBehav behav in CombatManager.GetInst().GetFighterList(!this.IsEnemy))
                                        {
                                                if (behav.FighterProp.Hp > 0 && behav.IsFront)
                                                {
                                                        if (bCheckStealth && behav.IsStealth())
                                                                continue;

                                                        targetlist.Add(behav);
                                                }
                                        }

                                        //找不到前排时，重新找
                                        if (targetlist.Count <= 0)
                                        {
                                                foreach (FighterBehav behav in CombatManager.GetInst().GetFighterList(!this.IsEnemy))
                                                {
                                                        if (behav.FighterProp.Hp > 0)
                                                        {
                                                                targetlist.Add(behav);
                                                        }
                                                }
                                        }
                                }
                                break;
                        case (int)SkillTargetGroup.ENEMY_BACK:
                                {
                                        foreach (FighterBehav behav in CombatManager.GetInst().GetFighterList(!this.IsEnemy))
                                        {
                                                if (behav.FighterProp.Hp > 0 && behav.IsFront == false)
                                                {
                                                        if (bCheckStealth && behav.IsStealth())
                                                                continue;

                                                        targetlist.Add(behav);
                                                }
                                        }

                                        if (targetlist.Count <= 0)
                                        {
                                                foreach (FighterBehav behav in CombatManager.GetInst().GetFighterList(!this.IsEnemy))
                                                {
                                                        if (behav.FighterProp.Hp > 0)
                                                        {
                                                                targetlist.Add(behav);
                                                        }
                                                }
                                        }
                                }
                                break;
                        case (int)SkillTargetGroup.TEAMMATE_ALL:
                                {
                                        foreach (FighterBehav behav in CombatManager.GetInst().GetFighterList(this.IsEnemy))
                                        {
                                                if (skillCfg.effect_type == (int)Skill_Effect_Type.REVIVE || behav.FighterProp.Hp > 0)
                                                {
                                                        targetlist.Add(behav);
                                                }
                                        }
                                }
                                break;
                        case (int)SkillTargetGroup.TEAMMATE_EXCEPT_SELF:
                                {
                                        foreach (FighterBehav behav in CombatManager.GetInst().GetFighterList(this.IsEnemy))
                                        {
                                                if (behav.FighterId == this.FighterId)
                                                        continue;

                                                if (skillCfg.effect_type == (int)Skill_Effect_Type.REVIVE || behav.FighterProp.Hp > 0)
                                                {
                                                        targetlist.Add(behav);
                                                }
                                        }
                                }
                                break;
                        case (int)SkillTargetGroup.SELF:
                                {
                                        targetlist.Add(this);
                                }
                                break;
                        case (int)SkillTargetGroup.TEAMMATE_SAMEROW:
                                {
                                        foreach (FighterBehav behav in CombatManager.GetInst().GetFighterList(this.IsEnemy))
                                        {
                                                if (behav.IsFront != this.IsFront)
                                                        continue;

                                                if (skillCfg.effect_type == (int)Skill_Effect_Type.REVIVE || behav.FighterProp.Hp > 0)
                                                {
                                                        targetlist.Add(behav);
                                                }
                                        }
                                }
                                break;
                }
                return targetlist;
        }

        List<FighterBehav> GetSubRandomList(bool bIncludeMainTarget)
        {
                List<FighterBehav> randomlist = new List<FighterBehav>();
                switch (m_ActionSkillCfg.target_group)
                {
                        case (int)SkillTargetGroup.ENEMY:
                                {
                                        foreach (FighterBehav behav in CombatManager.GetInst().GetFighterList(!this.IsEnemy))
                                        {
                                                if (!bIncludeMainTarget && behav.FighterId == m_MainTarget.FighterId)
                                                        continue;

                                                if (behav.FighterProp.Hp > 0)
                                                {
                                                        randomlist.Add(behav);
                                                }
                                        }
                                }
                                break;
                        case (int)SkillTargetGroup.ENEMY_FRONT:
                                {
                                        foreach (FighterBehav behav in CombatManager.GetInst().GetFighterList(!this.IsEnemy))
                                        {
                                                if (!bIncludeMainTarget && behav.FighterId == m_MainTarget.FighterId)
                                                        continue;
                                                if (behav.IsFront == false)
                                                        continue;

                                                if (behav.FighterProp.Hp > 0)
                                                {
                                                        randomlist.Add(behav);
                                                }
                                        }
                                }
                                break;
                        case (int)SkillTargetGroup.ENEMY_BACK:
                                {
                                        foreach (FighterBehav behav in CombatManager.GetInst().GetFighterList(!this.IsEnemy))
                                        {
                                                if (!bIncludeMainTarget && behav.FighterId == m_MainTarget.FighterId)
                                                        continue;

                                                if (behav.IsFront)
                                                        continue;

                                                if (behav.FighterProp.Hp > 0)
                                                {
                                                        randomlist.Add(behav);
                                                }
                                        }

                                }
                                break;


                        case (int)SkillTargetGroup.TEAMMATE_EXCEPT_SELF:
                                {
                                        foreach (FighterBehav behav in CombatManager.GetInst().GetFighterList(this.IsEnemy))
                                        {
                                                if (!bIncludeMainTarget && behav.FighterId == m_MainTarget.FighterId)
                                                        continue;

                                                if (behav.FighterId == this.FighterId)
                                                        continue;

                                                if (m_ActionSkillCfg.effect_type == (int)Skill_Effect_Type.REVIVE || behav.FighterProp.Hp > 0)
                                                {
                                                        randomlist.Add(behav);
                                                }
                                        }
                                }
                                break;
                        case (int)SkillTargetGroup.TEAMMATE_ALL:
                                {
                                        foreach (FighterBehav behav in CombatManager.GetInst().GetFighterList(this.IsEnemy))
                                        {
                                                if (!bIncludeMainTarget && behav.FighterId == m_MainTarget.FighterId)
                                                        continue;

                                                if (m_ActionSkillCfg.effect_type == (int)Skill_Effect_Type.REVIVE || behav.FighterProp.Hp > 0)
                                                {
                                                        randomlist.Add(behav);
                                                }
                                        }
                                }
                                break;

                        case (int)SkillTargetGroup.TEAMMATE_SAMEROW:
                                {
                                        foreach (FighterBehav behav in CombatManager.GetInst().GetFighterList(this.IsEnemy))
                                        {
                                                if (!bIncludeMainTarget && behav.FighterId == m_MainTarget.FighterId)
                                                        continue;

                                                if (behav.IsFront != this.IsFront)
                                                        continue;

                                                if (m_ActionSkillCfg.effect_type == (int)Skill_Effect_Type.REVIVE || behav.FighterProp.Hp > 0)
                                                {
                                                        randomlist.Add(behav);
                                                }
                                        }
                                }
                                break;
                        case (int)SkillTargetGroup.SELF:
                                {
                                        if (!bIncludeMainTarget && this.FighterId != m_MainTarget.FighterId)
                                        {
                                                randomlist.Add(this);
                                        }
                                }
                                break;
                }

                return randomlist;
        }

        IEnumerator MoveBack()
        {
                if (Vector3.Distance(transform.position, m_OriPos) > 0.5f)
                {
                        ActionState = FIGHTER_ACTION.ACTION;

                        iTween.moveToWorld(this.gameObject, m_fMaxMoveTime, 0f, m_OriPos);
                        PlayIdle();
                        yield return new WaitForSeconds(m_fMaxMoveTime);
                }
                ActionEnd();
        }

        float m_fSpeed = 16f;
        float MoveToTarget(int type)
        {
                float fTime = 0f;
                Vector3 targetPos = transform.position;
                if (type == 1)          //单位面前
                {
                        targetPos = m_MainTarget.transform.position + m_MainTarget.transform.forward;
                }
                else if (type == 2)     //全局位置
                {
                        targetPos = CombatManager.GetInst().CenterPoint;
                }
                else
                {
                        return 0f;
                }

                fTime = Vector3.Distance(transform.position, targetPos) / m_fSpeed;
                if (fTime > 0f)
                {
                        iTween.moveToWorld(this.gameObject, fTime, 0f, targetPos, iTween.EasingType.easeInQuad);
                        AnimComp.PlayAnim("run_001", true);
                }
                return fTime;
        }

        void PlayEffect(string effect, Transform belongTrans)
        {
                if (CombatManager.GetInst().IsResume())
                        return;

                if (!string.IsNullOrEmpty(effect))
                {
                        GameObject effectObj = EffectManager.GetInst().GetEffectObj(effect);
                        if (effectObj != null && belongTrans != null)
                        {
                                effectObj.transform.SetParent(belongTrans);
                                effectObj.transform.localPosition = Vector3.zero;
                                effectObj.transform.localRotation = Quaternion.identity;
                                GameObject.Destroy(effectObj, 10f);
                        }
                }
        }

        public void PlayDisperseEffect(SkillConfig actionSkill)
        {
                PlayEffect(actionSkill.disperse_effect, this.transform);
        }

        IEnumerator ProcessingShake(string hit_action, float maxtime, bool bFinalHit)
        {
                float delay = AnimComp.GetAnimTime(hit_action);

                if (maxtime > 0f)
                {
                        yield return new WaitForSeconds(0.18f);
                        AnimComp.Pause(hit_action, maxtime);
                        yield return new WaitForSeconds(maxtime + delay - 0.18f);
                }
                else
                {
                        yield return new WaitForSeconds(delay);
                }
                //if (bFinalHit)
                {
                        PlayIdle();
                }
        }
        IEnumerator ProcessingDodge()
        {
                iTween.moveTo(this.gameObject, 0.2f, 0f, this.transform.position - this.transform.forward * 0.5f + this.transform.right * 0.2f);
                yield return new WaitForSeconds(0.2f);
                iTween.moveTo(this.gameObject, 0.2f, 0f, this.transform.position + this.transform.forward * 0.5f - this.transform.right * 0.2f);
                this.transform.position = m_OriPos;
        }
        void PlayDodge(SkillConfig fromSkillCfg)
        {
                if (CombatManager.GetInst().IsResume() == false)
                {
                        StartCoroutine(ProcessingDodge());
                }
        }
        void PlayTargetHit(SkillConfig fromSkillCfg, bool bFinalHit)
        {
                if (CombatManager.GetInst().IsResume() == false)
                {
                        if (ActionState != FIGHTER_ACTION.DIE)
                        {
                                if (IsStun())
                                {
                                        this.AnimComp.PlayAnim(fromSkillCfg.hit_action, false);
                                }
                                else
                                {
                                        this.AnimComp.StopAnim();
                                        this.AnimComp.PlayAnim(fromSkillCfg.hit_action, false);
                                        StartCoroutine(ProcessingShake(fromSkillCfg.hit_action, HIT_PAUSE_TIME, bFinalHit));
                                }
                        }
                        else
                        {
                                this.AnimComp.PlayAnim("die_001", false);
                        }

                        if (!string.IsNullOrEmpty(fromSkillCfg.hit_effect) && fromSkillCfg.hit_effect != "-1")
                        {
                                Transform belongT = GameUtility.GetTransform(this.gameObject, "hitpoint");
                                if (belongT != null)
                                {
                                        PlayEffect(fromSkillCfg.hit_effect, belongT);
                                }
                                else
                                {
                                        PlayEffect(fromSkillCfg.hit_effect, this.transform);
                                }
                        }
                        AudioManager.GetInst().PlaySE(fromSkillCfg.hit_sound);
                }
        }

        public void Relive(SkillConfig fromSkillCfg)
        {
                ActionState = FIGHTER_ACTION.NONE;
                PlayEffect(fromSkillCfg.revive_effect, this.transform);
                if (m_UIStatus != null)
                {
                        m_UIStatus.gameObject.SetActive(true);
                }
                FighterProp.ResetDelay();
        }

        public int CheckShield(int nValue)
        {
                for (int i = m_BuffList.Count - 1; i >= 0; i--)
                {
                        FighterBuffBehav buff = m_BuffList[i];
                        if (nValue <= 0)
                        {
                                continue;
                        }

                        if (buff.ShieldHp > 0)
                        {
                                if (buff.ShieldHp >= nValue)
                                {
                                        buff.ShieldHp -= nValue;
                                        CombatManager.GetInst().RoundInfo_Log("护盾抵扣全部伤害=" + nValue + " 护盾剩余=" + buff.ShieldHp);
                                        nValue = 0;
                                }
                                else
                                {
                                        CombatManager.GetInst().RoundInfo_Log("护盾抵扣部分伤害=" + buff.ShieldHp);
                                        nValue -= buff.ShieldHp;
                                        buff.ShieldHp = 0;
                                }
                        }
                }
                return nValue;
        }

        public void UpdateHP_UI(bool bNeedAnim = false)
        {
                if (FighterProp != null)
                {
                        if (m_BossStatus != null)
                        {
                                m_BossStatus.SetHp(FighterProp.Hp, FighterProp.MaxHp, bNeedAnim);
                        }
                        else if (m_UIStatus != null)
                        {
                                m_UIStatus.SetHp(FighterProp.Hp, FighterProp.MaxHp, bNeedAnim);
                                m_UIStatus.SetPressure(FighterProp.Pressure);
                                if (FighterProp.Hp <= 0)
                                {
                                        if (m_UIStatus != null)
                                        {
                                                m_UIStatus.gameObject.SetActive(false);
                                        }
                                }
                        }
                }
        }

        UI_CombatDamage m_UIDamage;
        public void ShowDamage(FighterBehav attacker, SkillResultData skillResult)
        {
                //if (skillResult.nValue > 0)
                {
                        if (IsEnableShow)
                        {
                                if (m_UIDamage != null)
                                {
                                        GameObject.Destroy(m_UIDamage.gameObject);
                                }
                                m_UIDamage = GameUtility.AddDamageUI(GetTransform("hitpoint"), skillResult);
                        }
                        UpdateHP_UI(true);
                }
        }

        public void HpChange(int value, FighterBehav attacker)
        {
                if (value > 0)
                {
                        HpAdd(value);
                }
                else if (value < 0)
                {
                        HpMinus(-value, attacker);
                }
        }

        public void HpAdd(int value)
        {
                FighterProp.Hp += value;
        }

        public void HpMinus(int rdValue, FighterBehav attacker, bool bPlayAnim = false)
        {
                int nValue = this.CheckShield(rdValue);

                if (nValue > 0)
                {
                        FighterProp.Hp -= nValue;
                        this.CheckBuffWhenDamaged();
                        if (FighterProp.HpRatio <= GlobalParams.GetInt("AI_dying_line"))
                        {
                                Talk(TALK_TYPE.DYING_IN_BATTLE);
                        }
                }
                if (FighterProp.Hp <= 0)
                {
                        Die(bPlayAnim);

                        if (attacker != null)
                        {
                                PressureManager.GetInst().PressureChange(BATTLE_PRESSURE_TYPE.BE_KILLED, this);

                                attacker.HasKill = true;
                                CombatManager.GetInst().CheckAllTriggers(attacker, FIGHTER_TRIGGER_TYPE.KILL_OTHER);
                                PressureManager.GetInst().PressureChange(BATTLE_PRESSURE_TYPE.KILLED, attacker);
                        }
                }
        }
        void Die(bool bPlayAnim = false)
        {
                FighterProp.Hp = 0;
                List<FighterBuffBehav> toremove = new List<FighterBuffBehav>();
                foreach (FighterBuffBehav buff in m_BuffList)
                {
                        if (buff.m_BuffCfg.is_dead_fail == 1)
                        {
                                DelBuffIcon(buff);
                                buff.DestroySelf();
                                toremove.Add(buff);
                        }
                }
                foreach (FighterBuffBehav buff in toremove)
                {
                        m_BuffList.Remove(buff);
                }
                if (ActionState != FIGHTER_ACTION.DIE)
                {
                        if (bPlayAnim)
                        {
                                AnimComp.PlayAnim("die_001", false);
                        }
                        ActionState = FIGHTER_ACTION.DIE;

                }

                CombatManager.GetInst().CheckAllTriggers(this, FIGHTER_TRIGGER_TYPE.ON_DEAD);
                CombatManager.GetInst().RoundInfo_Log("Fighter死亡 " + this.FighterId);
        }

        public void PressureChange(int value)
        {
                if (value == 0)
                        return;

                FighterProp.Pressure += value;
                if (IsEnableShow)
                {
                        ShowPressure(value);
                        GameObject effectObj = EffectManager.GetInst().GetEffectObj(value > 0 ? "effect_raid_pressure_up_001" : "effect_raid_pressure_down_001");
                        if (effectObj != null)
                        {
                                effectObj.transform.SetParent(this.transform);
                                effectObj.transform.localPosition = Vector3.zero;
                                effectObj.transform.localRotation = Quaternion.identity;
                        }
                }

                if (FighterProp.IsPressureOverDeadline())
                {
                        Die(true);
                        FighterProp.Pressure = FighterProp.MaxPressure * 2 - 1;
                }
        }

        UI_CombatDamage m_UIPressure;
        public void ShowPressure(int offset)
        {
                SkillResultData skillResult = new SkillResultData();
                skillResult.type = offset > 0 ? SkillResultType.AddPressure : SkillResultType.MinusPressure;
                skillResult.nValue = Mathf.Abs(offset);

                if (m_UIPressure != null)
                {
                        GameObject.Destroy(m_UIPressure.gameObject);
                }
                m_UIPressure = GameUtility.AddDamageUI(this.transform, skillResult, 3f);
        }


        #region UI_STATUS
        UI_UnitStatus m_UIStatus = null;
        UI_BossStatus m_BossStatus = null;
        public void InitStatusUI()
        {
                if (CharacterCfg.body_type == 2)
                {
                        m_BossStatus = UIManager.GetInst().ShowUI<UI_BossStatus>();
                        m_BossStatus.SetupSkills(this);
                }
                else
                {
                        if (m_UIStatus == null)
                        {
                                GameObject uiObj = UIManager.GetInst().ShowUI_Multiple<UI_UnitStatus>("UI_UnitStatus");
                                uiObj.transform.SetParent(this.gameObject.transform);
                                m_UIStatus = uiObj.GetComponent<UI_UnitStatus>();
                                m_UIStatus.SetEnemy(IsEnemy);
                                m_UIStatus.m_BelongTransform = this.transform;
                                GameUtility.SetLayer(uiObj, "UI");
                        }
                }
        }

        GameObject m_CounterEffect;
        GameObject m_SelectCircle;
        GameObject SelectCircleObj
        {
                get
                {
                        if (m_SelectCircle == null)
                        {
                                m_SelectCircle = EffectManager.GetInst().GetEffectObj("effect_battle_target_001");
                                m_SelectCircle.transform.SetParent(this.transform);
                                m_SelectCircle.transform.localPosition = Vector3.zero;
                                m_SelectCircle.transform.localRotation = Quaternion.identity;
                        }
                        return m_SelectCircle;
                }
        }

        public void SetSelectedEffect(bool bEnable)
        {
                if (bEnable == false)
                {
                        GameObject.Destroy(m_CounterEffect);
                }
        }
        public void SetSelectCircle(bool bEnable)
        {
                SelectCircleObj.SetActive(bEnable);
        }

        public bool CanSelect()
        {
                if (m_CounterEffect != null)
                {
                        return true;
                }
                return false;
        }

        public void SetCounter(int counterFactor)
        {
                GameObject.Destroy(m_CounterEffect);
                if (counterFactor > 100)
                {
                        m_CounterEffect = EffectManager.GetInst().GetEffectObj("effect_battle_good_counter_001");
                }
                else if (counterFactor < 100)
                {
                        m_CounterEffect = EffectManager.GetInst().GetEffectObj("effect_battle_bad_counter_001");
                }
                else
                {
                        m_CounterEffect = EffectManager.GetInst().GetEffectObj("effect_battle_no_counter_001");
                }
                if (m_CounterEffect != null)
                {

                        m_CounterEffect.transform.SetParent(this.transform);
                        m_CounterEffect.transform.localPosition = Vector3.zero;
                        m_CounterEffect.transform.localRotation = Quaternion.identity;
                }
        }
        #endregion
        #region BUFF

        List<FighterBuffBehav> m_BuffList = new List<FighterBuffBehav>();

        public int GetDeBuffCount()
        {

                int ret = 0;
                for (int i = 0; i < m_BuffList.Count; i++)
                {
                        if (m_BuffList[i].m_BuffCfg.good_or_bad == 2)
                        {
                                ret++;
                        }
                }
                return ret;
        }
        public bool HasDeBuff()
        {
                for (int i = 0; i < m_BuffList.Count; i++)
                {
                        if (m_BuffList[i].m_BuffCfg.good_or_bad == 2)
                        {
                                return true;
                        }
                }

                return false;
        }

        public bool HasBuff(int buffid)
        {
                for (int i = 0; i < m_BuffList.Count; i++)
                {
                        if (m_BuffList[i].BuffID == buffid)
                        {
                                return true;
                        }
                }
                return false;
        }

        public bool HasChangeBuff(SkillConfig skillCfg)
        {
                if (skillCfg.change_buff <= 0)
                {
                        return true;
                }

                List<FighterBuffBehav> toremove = new List<FighterBuffBehav>();
                foreach (FighterBuffBehav buff in m_BuffList)
                {
                        if (buff.m_BuffCfg.id == skillCfg.change_buff)
                        {
                                toremove.Add(buff);
                        }
                }
                if (toremove.Count > 0)
                {
                        foreach (FighterBuffBehav buff in toremove)
                        {
                                DelBuff(buff);
                        }
                        return true;
                }

                return false;
        }

        public void AddBuff(int buffId, FighterBehav attacker, bool bShowGet = false)
        {
                SkillBuffConfig cfg = SkillManager.GetInst().GetBuff(buffId);
                if (cfg != null)
                {
                        List<FighterBuffBehav> sameGroupBuffs = m_BuffList.FindAll((x =>
                        {
                                return x.m_BuffCfg.group == cfg.group;
                        }));
                        foreach (FighterBuffBehav buff in sameGroupBuffs)
                        {
                                if (buff.m_BuffCfg.id != cfg.id)
                                {
                                        DelBuff(buff);
                                }
                        }

                        List<FighterBuffBehav> sameBuffList = m_BuffList.FindAll((x =>
                        {
                                return x.m_BuffCfg.id == cfg.id;
                        }));
                        if (sameBuffList.Count > 0)
                        {
                                if (cfg.overlay_type == 1)
                                {
                                        if (cfg.overlay_number > 0 && sameBuffList.Count >= cfg.overlay_number)
                                        {
                                                DelBuff(sameBuffList[0]);
                                        }
                                }
                                else if (cfg.overlay_type == 2)
                                {
                                        sameBuffList[0].LeftRound = sameBuffList[0].LeftRound + cfg.time;
                                        if (sameBuffList[0].LeftRound > cfg.overlay_number)
                                        {
                                                sameBuffList[0].LeftRound = cfg.overlay_number;
                                        }
                                        CombatManager.GetInst().RoundInfo_Log("Fighter" + this.FighterId + " 刷新BUFF ID= " + buffId + " 剩余回合数=" + sameBuffList[0].LeftRound, 3);
                                        return;
                                }
                        }
                        GameObject buffObj = new GameObject("Buff" + buffId);
                        buffObj.transform.SetParent(this.gameObject.transform);
                        buffObj.transform.localRotation = Quaternion.identity;
                        buffObj.transform.localPosition = Vector3.zero;
                        FighterBuffBehav newBuff = buffObj.AddComponent<FighterBuffBehav>();
                        newBuff.Setup(cfg, attacker, this);
                        m_BuffList.Add(newBuff);

                        CombatManager.GetInst().RoundInfo_Log("Fighter" + this.FighterId + " 添加Buff ID=" + buffId, 4);
                        newBuff.StartEffect();
                        if (bShowGet)
                        {
                                if (newBuff.m_BuffCfg.good_or_bad == 2)
                                {
                                        Talk(TALK_TYPE.DEBUFF_IN_BATTLE);
                                }
                                if (m_BossStatus != null)
                                {
                                        m_BossStatus.ShowGetBuff(LanguageManager.GetText(newBuff.m_BuffCfg.name));
                                }
                                else if (m_UIStatus != null)
                                {
                                        m_UIStatus.ShowGetBuff(LanguageManager.GetText(newBuff.m_BuffCfg.name));
                                }
                        }
                }
        }

        public void DelBuff(FighterBuffBehav buff)
        {
                CombatManager.GetInst().RoundInfo_Log("Fighter" + this.FighterId + " 删除Buff ID=" + buff.m_BuffCfg.id + " " + buff.m_BuffCfg.mark, 4);
                DelBuffIcon(buff);
                buff.DestroySelf();
                m_BuffList.Remove(buff);
        }
        public void RemoveBuff(int buffGoodBadType, int number)
        {
                List<FighterBuffBehav> toremove = new List<FighterBuffBehav>();
                foreach (FighterBuffBehav buff in m_BuffList)
                {
                        if (buff.m_BuffCfg.good_or_bad == buffGoodBadType)
                        {
                                toremove.Add(buff);
                        }
                }
                if (number == 0)
                {
                        for (int i = 0; i < toremove.Count; i++)
                        {
                                DelBuff(toremove[i]);
                        }
                }
                else
                {
                        for (int i = 0; i < number; i++)
                        {
                                if (toremove.Count > 0)
                                {
                                        int idx = DamageCalc.GetRandomVal(0, toremove.Count - 1, "选取移除的buff");
                                        DelBuff(toremove[idx]);
                                }
                        }
                }
        }

        public void CheckBuffBeforeAction()
        {
                foreach (FighterBuffBehav behav in new List<FighterBuffBehav>(m_BuffList))
                {
                        if (this.FighterProp.Hp <= 0)
                        {
                                return;
                        }
                        if (behav != null)
                        {
                                behav.OnFighterAction();
                        }
                }
        }
        public void CheckBuffAfterAction()
        {
                foreach (FighterBuffBehav behav in new List<FighterBuffBehav>(m_BuffList))
                {
                        if (this.FighterProp.Hp <= 0)
                        {
                                return;
                        }

                        if (behav != null)
                        {
                                behav.AfterFighterAction();
                        }
                }
        }

        public void RecoverAttributeChange(string info)
        {
                CombatManager.GetInst().RoundInfo_Log("RecoverAttributeChange " + this.FighterId + " " + info);
                string[] attributes = info.Split(';');
                foreach (string tmp in attributes)
                {
                        if (string.IsNullOrEmpty(tmp))
                                continue;

                        string[] tmps = tmp.Split(',');
                        if (tmps.Length >= 2)
                        {
                                int id = int.Parse(tmps[0]);
                                FighterProp.SetBattlePropAdd(id, int.Parse(tmps[1]) * (-1), false);

                                string name = CharacterManager.GetInst().GetPropertyName(id);
                                if (name == "taunt_buf")
                                {
                                        TauntTarget = null;
                                }
                                if (name == "conceal_buf")
                                {
                                        this.IsHide = IsStealth();
                                }
                        }
                }
        }


        public void CalcBuffResult(FighterBuffBehav buff, FighterBehav attacker, SkillBuffConfig buffCfg)
        {
                if (!string.IsNullOrEmpty(buffCfg.effective_effect) && buffCfg.effective_effect != "-1")
                {
                        PlayEffect(buffCfg.effective_effect, this.transform);
                }
                SkillResultData srd = DamageCalc.CalcBuffResult(buff, buffCfg, attacker.FighterProp, this.FighterProp);
                switch (srd.type)
                {
                        case SkillResultType.Heal:
                                {
                                        HpAdd(srd.nValue);
                                }
                                break;
                        case SkillResultType.Damage:
                                {
                                        HpMinus(srd.nValue, null);
                                }
                                break;
                }

                ShowDamage(attacker, srd);
                CombatManager.GetInst().RoundInfo_Log("计算Buff伤害 " + this.FighterId + " buff=" + buffCfg.mark + " id=" + buffCfg.id + " value=" + srd.nValue, 5);
        }
        public void CalcExtraAttribute(FighterBehav attacker, string attributestr)
        {
                if (!string.IsNullOrEmpty(attributestr))
                {
                        string[] attributes = attributestr.Split(';');
                        foreach (string tmp in attributes)
                        {
                                if (string.IsNullOrEmpty(tmp))
                                        continue;

                                string[] tmps = tmp.Split(',');
                                if (tmps.Length >= 2)
                                {
                                        int id = int.Parse(tmps[0]);
                                        FighterProp.SetBattlePropAdd(id, int.Parse(tmps[1]), false);

                                        string name = CharacterManager.GetInst().GetPropertyName(id);
                                        if (name == "taunt_buf")
                                        {
                                                TauntTarget = attacker;
                                        }
                                        if (name == "conceal_buf")
                                        {

                                                this.IsHide = IsStealth();
                                        }
                                }
                        }
                }
                if (!string.IsNullOrEmpty(attributestr))
                {
                        CombatManager.GetInst().RoundInfo_Log("临时追加属性 " + this.FighterId + " " + attributestr);
                }
        }


        public void TryTriggerList(FighterBehav triggerTarget, FIGHTER_TRIGGER_TYPE action, FighterBehav damageTarget = null)
        {
                for (int i = m_PassiveTriggerList.Count - 1; i >= 0; i--)
                {
                        m_PassiveTriggerList[i].TryTrigger(triggerTarget, action, damageTarget);
                }
                for(int i = m_BuffList.Count - 1; i >= 0;  i--)
                {
                        m_BuffList[i].TryTrigger(triggerTarget, action, damageTarget);
                }
        }

        public void AddBuffIcon(FighterBuffBehav buff)
        {
                if (m_BossStatus != null)
                {
                        m_BossStatus.AddBuff(buff);
                }
                else if (m_UIStatus != null)
                {
                        m_UIStatus.AddBuff(buff);
                }
        }
        public void UpdateBuffIcon(FighterBuffBehav buff)
        {
                if (m_BossStatus != null)
                {
                        m_BossStatus.UpdateBuffCount(buff);
                }
                else if (m_UIStatus != null)
                {
                        m_UIStatus.UpdateBuffCount(buff);
                }
        }
        public void DelBuffIcon(FighterBuffBehav buff)
        {
                if (m_BossStatus != null)
                {
                        m_BossStatus.DelBuff(buff);
                }
                else if (m_UIStatus != null)
                {
                        m_UIStatus.DelBuff(buff);
                }
        }
        public void CheckBuffWhenDamaged()
        {
                for (int i = m_BuffList.Count - 1; i >= 0; i--)
                {
                        if (m_BuffList[i].m_BuffCfg.is_damage_fail == 1)
                        {
                                DelBuff(m_BuffList[i]);
                        }
                }
        }
        public void CheckBuffWhenAction()
        {
                for (int i = m_BuffList.Count - 1; i >= 0; i--)
                {
                        if (m_BuffList[i].IsActionFail())
                        {
                                DelBuff(m_BuffList[i]);
                        }
                }

        }
        #endregion

        bool CalcExtraSkill()
        {
                if (m_nSkillSourceType == 0)
                {
                        CombatManager.GetInst().RoundInfo_Log("判断追加技能 " + m_ActionSkillCfg.add_skill_id + " hasCritical=" + HasCritical + " hasKill=" + HasKill + " hasbuff=" + HasBuffAdded);
                }
                else
                {
                        return false;
                }
                if (m_ActionSkillCfg.add_skill_id <= 0)
                {
                        return false;
                }
                bool bAvailable = false;
                switch (m_ActionSkillCfg.add_skill_condition)
                {
                        default:
                        case 0:
                                bAvailable = true;
                                break;
                        case 1:
                                if (HasCritical)
                                {
                                        bAvailable = true;
                                }
                                break;
                        case 2:
                                if (HasKill)
                                {
                                        bAvailable = true;
                                }
                                break;
                        case 3:
                                if (HasBuffAdded)
                                {
                                        bAvailable = true;
                                }
                                break;
                }
                if (bAvailable)
                {
                        CombatManager.GetInst().AddExtraSkill(this, SkillManager.GetInst().GetActiveSkill(m_ActionSkillCfg.add_skill_id), this.m_MainTarget);
                }
                return bAvailable;
        }

        void ActionEnd()
        {
                transform.rotation = CombatManager.GetInst().GetForward(IsEnemy);
                //更新一遍正确的血量，判断显示是否已死亡
                UpdateHP_UI(true);
                PlayIdle();

                ActionState = FIGHTER_ACTION.END;
        }

        public void GoMoveBack()
        {
                StartCoroutine(MoveBack());
        }
        /// <summary>
        /// 是否晕眩
        /// </summary>
        /// <returns></returns>
        public bool IsStun()
        {
                return FighterProp.GetFinalPropValue("stun_buf") > 0;
        }
        /// <summary>
        /// 是否隐身
        /// </summary>
        /// <returns></returns>
        public bool IsStealth()
        {
                return FighterProp.GetFinalPropValue("conceal_buf") > 0;
        }
        /// <summary>
        /// 是否沉默
        /// </summary>
        /// <returns></returns>
        public bool IsSilence()
        {
                return FighterProp.GetFinalPropValue("silence_buf") > 0;
        }

        public bool CanAction()
        {
                if (IsStun() || IsDead())
                {
                        CombatManager.GetInst().RoundInfo_Log(FighterId + " CanAction()==false " + FighterProp.GetFinalPropValue("stun_buf"));
                        return false;
                }
                return true;
        }

        public bool IsDead()
        {
                return m_ActionState == FIGHTER_ACTION.DIE;
        }

        #region

        GameObject m_TalkUIObj = null;
        public void Talk(string text)
        {
                if (IsEnemy)
                        return;
                if (m_BossStatus != null)
                {
                        m_BossStatus.Talk(text);
                }
                else if (m_UIStatus != null)
                {
                        m_UIStatus.Talk(text);
                }
        }
        public void Talk(TALK_TYPE type)
        {
                string text = RaidManager.GetInst().GetTalkText(type);
                if (!string.IsNullOrEmpty(text))
                {
                        Talk(text);
                }
        }

        #endregion

        int GetRealRangeType()
        {
                int range_type = 0;
                if (m_nSkillSourceType == 0)
                {
                        range_type = m_ActionSkillCfg.range_type;
                }
                else if (m_nSkillSourceType == 1)
                {
                        //追加释放时，range_type为1 3的是单体技能，单体目标已在MainTarget里存放
                        if (m_ActionSkillCfg.add_skill_range_type == 1 || m_ActionSkillCfg.add_skill_range_type == 3)
                        {
                                range_type = 1;
                        }
                        else if (m_ActionSkillCfg.add_skill_range_type == 2 || m_ActionSkillCfg.add_skill_range_type == 4)
                        {
                                range_type = 2;
                        }
                        else
                        {
                                range_type = 0;
                        }
                }
                else if (m_nSkillSourceType == 2)
                {
                        switch (m_ActionSkillCfg.trigger_skill_range_type)
                        {
                                case 1:
                                case 3:
                                case 5:
                                        {
                                                range_type = 1;
                                        }
                                        break;
                                case 2:
                                case 4:
                                case 6:
                                        {
                                                range_type = 2;
                                        }
                                        break;
                                case 0:
                                        {
                                                range_type = m_ActionSkillCfg.range_type;
                                        }
                                        break;
                        }
                }

                return range_type;
        }

        List<FighterBehav> GetTargetList(int range_type)
        {
                List<FighterBehav> retTargetlist = null;
                switch (range_type)
                {
                        case (int)SkillRangeType.ALL:
                                {
                                        retTargetlist = CalcSkillTargetGroup(m_ActionSkillCfg, false);
                                }
                                break;

                        case (int)SkillRangeType.SAME_ROW:
                                {
                                        retTargetlist = new List<FighterBehav>();
                                        foreach (FighterBehav behav in CombatManager.GetInst().GetFighterList(m_MainTarget.IsEnemy))
                                        {
                                                if (behav.IsFront != m_MainTarget.IsFront)
                                                        continue;

                                                if (m_ActionSkillCfg.effect_type == (int)Skill_Effect_Type.REVIVE || behav.FighterProp.Hp > 0)
                                                {
                                                        retTargetlist.Add(behav);
                                                }
                                        }
                                }
                                break;
                        case (int)SkillRangeType.MAIN:
                                {
                                        retTargetlist = new List<FighterBehav>();
                                        retTargetlist.Add(m_MainTarget);
                                }
                                break;

                        case (int)SkillRangeType.MAIN_WITH_RANDOM:
                                {

                                        //选择单体
                                        retTargetlist = new List<FighterBehav>();
                                        retTargetlist.Add(m_MainTarget);

                                        //获得随机目标列表
                                        List<FighterBehav> randomlist = GetSubRandomList(false);
                                        //选择随机多人，并从容器里每次去掉这个人
                                        for (int i = 0; i < m_ActionSkillCfg.random_multiple_number; i++)
                                        {
                                                if (randomlist.Count > 0)
                                                {
                                                        int idx = DamageCalc.GetRandomVal(0, randomlist.Count - 1, "选取单体附带随机多人的目标");
                                                        retTargetlist.Add(randomlist[idx]);
                                                        randomlist.RemoveAt(idx);
                                                }
                                        }
                                }
                                break;
                }
                return retTargetlist;
        }

        IEnumerator NormalAttack(FighterBehav targetBehav, int splitIndex, SkillResultData srd)
        {
                if (IsEnableShow)
                {
                        if (m_ActionSkillCfg.hit_type == 3)
                        {
                                yield return new WaitForSeconds(m_ActionSkillCfg.hit_delay / 1000f);
                        }
                        OnSkillHit(targetBehav, splitIndex, srd);
                        if (srd.type != SkillResultType.Miss && srd.type != SkillResultType.Dodge)
                        {
                                targetBehav.PlayTargetHit(m_ActionSkillCfg, splitIndex == m_nMaxSplitCount - 1);
                        }
                        else if (srd.type == SkillResultType.Dodge)
                        {
                                targetBehav.PlayDodge(m_ActionSkillCfg);
                        }
                        if (HIT_PAUSE_TIME > 0f)
                        {
                                AnimComp.Pause(AnimComp.GetAnimName(m_ActionSkillCfg.cast_action_id), HIT_PAUSE_TIME);
                                m_fMaxAnimTime += HIT_PAUSE_TIME;
                        }
                }
                else
                {
                        OnSkillHit(targetBehav, splitIndex, srd);
                        if (targetBehav.IsStun())
                        {
                                targetBehav.AnimComp.PlayAnim(m_ActionSkillCfg.hit_action, false);
                        }
                }
        }

        IEnumerator GenerateBullet(FighterBehav targetBehav, int bulletIndex, int splitIndex, SkillResultData srd)
        {
                if (IsEnableShow)
                {
                        GameUtility.RotateTowards(this.transform, targetBehav.transform);
                        if (m_ActionSkillCfg.is_hit_sequence == 1)
                        {
                                yield return new WaitForSeconds(bulletIndex * m_ActionSkillCfg.hit_sequence_interval_time / 1000f);
                        }

                        float bulletTime = 0f;
                        if (m_ActionSkillCfg.bullet_time > 0)
                        {
                                GameObject bulletObj = EffectManager.GetInst().GetEffectObj(m_ActionSkillCfg.bullet_effect);

                                if (bulletObj != null)
                                {
                                        AudioManager.GetInst().PlaySE(m_ActionSkillCfg.bullet_sound);
                                        Transform belongT = targetBehav.GetTransform("hitpoint");
//                                         float dist = Vector3.Distance(this.transform.position, belongT.position);
//                                         bulletTime dist / (float)m_ActionSkillCfg.bullet_speed;
                                        bulletTime = m_ActionSkillCfg.bullet_time;
                                        bulletObj.transform.SetParent(this.transform);

                                        Transform fromT = GetTransform("weapon_1");
                                        if (fromT != null)
                                        {
                                                bulletObj.transform.position = fromT.position;
                                        }
                                        GameUtility.RotateTowards(bulletObj.transform, belongT);
                                        iTween.moveToWorld(bulletObj, bulletTime, 0f, belongT.position, iTween.EasingType.easeInQuad);
                                        yield return new WaitForSeconds(bulletTime);
                                        GameObject.Destroy(bulletObj);
                                }
                        }
                        OnSkillHit(targetBehav, splitIndex, srd);
                        if (srd.type != SkillResultType.Miss && srd.type != SkillResultType.Dodge)
                        {
                                targetBehav.PlayTargetHit(m_ActionSkillCfg, splitIndex == m_nMaxSplitCount - 1);
                        }
                        else if (srd.type == SkillResultType.Dodge)
                        {
                                targetBehav.PlayDodge(m_ActionSkillCfg);
                        }
                        if (HIT_PAUSE_TIME > 0f)
                        {
                                AnimComp.Pause(AnimComp.GetAnimName(m_ActionSkillCfg.cast_action_id), HIT_PAUSE_TIME);
                                m_fMaxAnimTime += HIT_PAUSE_TIME;
                        }
                }
                else
                {
                        OnSkillHit(targetBehav, splitIndex, srd);
                        if (targetBehav.IsStun())
                        {
                                targetBehav.AnimComp.PlayAnim(m_ActionSkillCfg.hit_action, false);
                        }
                }
        }

        public void ShowLifeSteal(int nLifeSteal)
        {
                SkillResultData stealResult = new SkillResultData();
                stealResult.nValue = nLifeSteal;
                stealResult.type = SkillResultType.Heal;        //这里后面会改成吸血的字样
                ShowDamage(this, stealResult);
        }

        void OnSkillHit(FighterBehav targetBehav, int splitIndex, SkillResultData srd)
        {
                targetBehav.ShowDamage(this, srd);
                m_nPlayingHitCount--;
                CombatManager.GetInst().RoundInfo_AddSkillInfo(this, targetBehav, m_ActionSkillCfg, srd);
                CombatManager.GetInst().RoundInfo_Log("TakeSkillResult " + FighterId + " value=" + srd.nValue, 6);
        }

        void CalcSingleResult(FighterBehav targetBehav, int bulletIdx, int splitIndex)
        {
                if (targetBehav.FighterProp.Hp > 0)
                {
                        m_nPlayingHitCount++;
                        SkillResultData srd = DamageCalc.CalcSkillResult(m_ActionSkillCfg, this, targetBehav, m_nSkillSourceType, splitIndex, CombatManager.GetInst().BrightLevel);
                        if (!string.IsNullOrEmpty(m_ActionSkillCfg.bullet_effect))
                        {
                                StartCoroutine(GenerateBullet(targetBehav, bulletIdx, splitIndex, srd));
                        }
                        else
                        {
                                StartCoroutine(NormalAttack(targetBehav, splitIndex, srd));
                        }
                }
        }

        bool IsEnableShow
        {
                get
                {
                        return CombatManager.GetInst().IsResume() == false;
                }
        }

        IEnumerator ProcessingSkill()
        {
                //计算临时属性，
                CalcExtraAttribute(this, m_ActionSkillCfg.temporary_attribute);

                m_nPlayingHitCount = 0;
                m_TargetRetQueue.Clear();
                HasKill = false;
                HasCritical = false;
                HasBuffAdded = false;

                if (m_ActionSkillCfg.effect_type == (int)Skill_Effect_Type.DAMAGE)
                {
                        HIT_PAUSE_TIME = 0.3f;
                }
                else
                {
                        HIT_PAUSE_TIME = 0f;
                }

                int range_type = GetRealRangeType();     //实际目标数量 0：全体 1：主目标 2：主目标附带随机多人 3:同排全体

                m_nSplitCount = 0;      //带逗号的表示随机区间
                if (m_ActionSkillCfg.attack_heal_number.Contains(","))
                {
                        string[] tmps = m_ActionSkillCfg.attack_heal_number.Split(',');
                        if (tmps.Length == 2)
                        {
                                int minVal = int.Parse(tmps[0]);
                                int maxVal = int.Parse(tmps[1]);
                                m_nMaxSplitCount = DamageCalc.GetRandomVal(minVal, maxVal, "多段段数选取");
                        }
                        else
                        {
                                Debug.LogError(m_ActionSkillCfg.attack_heal_number);
                        }
                }
                else
                {
                        int.TryParse(m_ActionSkillCfg.attack_heal_number, out m_nMaxSplitCount);
                }

                if (IsEnableShow)
                {
                        //近战移动
                        if (m_ActionSkillCfg.forward_type > 0)
                        {
                                yield return new WaitForSeconds(MoveToTarget(m_ActionSkillCfg.forward_type));
                                PlayIdle();
                        }

                        //播放技能动作
                        AnimComp.PlayAnim(m_ActionSkillCfg.cast_action_id, false, "");
                        Invoke("PlayIdle", AnimComp.GetAnimTime(m_ActionSkillCfg.cast_action_id) + HIT_PAUSE_TIME);
                        //施法特效
                        PlayEffect(m_ActionSkillCfg.cast_effect, GetTransform("bodypoint"));
                        AudioManager.GetInst().PlaySE(m_ActionSkillCfg.cast_sound);
                }
                m_fHitTime = 0f;
                m_fMaxAnimTime = AnimComp.GetAnimTime(m_ActionSkillCfg.cast_action_id) ;
                List<FighterBehav> retTargetlist = null;

                for (m_nSplitCount = 0; m_nSplitCount < m_nMaxSplitCount; m_nSplitCount++)
                {
                        if (IsEnableShow)
                        {
                                float delta = AnimComp.GetHitTime(m_ActionSkillCfg.cast_action_id, m_nSplitCount) - m_fHitTime;
                                m_fHitTime += delta;
                                yield return new WaitForSeconds(delta);

                                ///开始打击点
                                if (m_MainTarget != null)
                                {
                                        PlayEffect(m_ActionSkillCfg.location_effect, CombatManager.GetInst().GetEffectPoint(m_MainTarget.IsEnemy));
                                        AudioManager.GetInst().PlaySE(m_ActionSkillCfg.location_sound);
                                }
                        }
                        if (m_ActionSkillCfg.is_hit_sequence == 1 && range_type == 2)   //有顺序的情况只有单体随机多人存在
                        {
                                if (m_MainTarget != null)
                                {
                                        CalcSingleResult(m_MainTarget, 0, m_nSplitCount);
                                }
                                for (int i = 1; i <= m_ActionSkillCfg.random_multiple_number; i++)
                                {
                                        while (m_nPlayingHitCount > 0)
                                        {
                                                yield return null;
                                        }

                                        List<FighterBehav> randomlist = GetSubRandomList(true);
                                        if (randomlist.Count > 0)
                                        {
                                                int idx = DamageCalc.GetRandomVal(0, randomlist.Count - 1, "有顺序的单体随机多人目标选取");
                                                CalcSingleResult(randomlist[idx], i, m_nSplitCount);
                                        }
                                }
                        }
                        else
                        {//无顺序情况下，选好目标后才结算
                                if (retTargetlist == null)
                                {
                                        retTargetlist = GetTargetList(range_type);
                                }
                                for (int i = 0; i < retTargetlist.Count; i++)
                                {
                                        CalcSingleResult(retTargetlist[i], i, m_nSplitCount);
                                }
                        }
                        if (HIT_PAUSE_TIME > 0f)
                        {
                                yield return new WaitForSeconds(HIT_PAUSE_TIME);
                        }
                }
                //如果有额外技能，则不考虑要不要回去，等额外技能来回去。
                bool bExtraSkill = CalcExtraSkill();
                if (!bExtraSkill)
                {
                        if (Vector3.Distance(transform.position, m_OriPos) > 0.5f)
                        {
                                iTween.moveToWorld(this.gameObject, m_fMaxMoveTime, 0f, m_OriPos);
                                if (!AnimComp.AnimCtr.isPlaying)
                                {
                                        PlayIdle();
                                }
                        }
                }

                if (IsEnableShow && m_fHitTime < m_fMaxAnimTime)
                {
                        yield return new WaitForSeconds(m_fMaxAnimTime - m_fHitTime);
                }

                while (m_nPlayingHitCount > 0)
                {
                        yield return null;
                }

                RecoverAttributeChange(m_ActionSkillCfg.temporary_attribute);
                CheckBuffWhenAction();
                ActionEnd();
        }

        void OnDestroy()
        {
                if (m_BossStatus != null)
                {
                        UIManager.GetInst().CloseUI("UI_BossStatus");
                }
        }
}
