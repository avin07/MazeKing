using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Message;
public class CampManager : SingletonObject<CampManager>
{
        enum CAMP_STATE
        {
                NONE,
                PREPARE,
                INPOSITION,
                EAT,
                SELECT_ACTOR,
                SELECT_SKILL,
                SELECT_TARGET,
                SKILL_PLAY,
                EVENT,
                END,
        }

        Dictionary<int, RaidCampEventConfig> m_RaidCampEventDict = new Dictionary<int, RaidCampEventConfig>();
        Dictionary<int, RaidCampSkillConfig> m_RaidCampSkillDict = new Dictionary<int, RaidCampSkillConfig>();

        int CAMP_MODEL = 6016;
        public int m_nCampProgress = 1;     //1进餐前，2进餐后
        public int m_nSkillPoint = 0;
        CAMP_STATE m_CampState = CAMP_STATE.NONE;
        GameObject m_SelectCircle;
        GameObject SelectCircle
        {
                get
                {
                        if (m_SelectCircle == null)
                        {
                                m_SelectCircle = EffectManager.GetInst().GetEffectObj("effect_battle_teammate_circle_001");
                        }
                        return m_SelectCircle;
                }
        }
        float m_fTime = 0f;
        float m_fMaxTime = 0f;
        
        int m_nMovingUnitCount = 0;
        HeroUnit m_ActionUnit = null;
        HeroUnit m_TargetUnit = null;
        RaidCampSkillConfig m_ActionSkillCfg;
        GameObject m_CampObj = null;
        public void Init()
        {
                ConfigHoldUtility<RaidCampEventConfig>.LoadXml("Config/raid_camp_event", m_RaidCampEventDict);
                ConfigHoldUtility<RaidCampSkillConfig>.LoadXml("Config/raid_camp_skill", m_RaidCampSkillDict);

                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidCampEvent), OnCampEvent);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidCampState), OnCampEnter);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidCampSP), OnCampSP);
        }

        public RaidCampSkillConfig GetCampSkillCfg(int id)
        {
                if (m_RaidCampSkillDict.ContainsKey(id))
                {
                        return m_RaidCampSkillDict[id];
                }
                return null;
        }

        public void EnterCamp()
        {
                RaidManager.GetInst().EnterCamp();
                m_fTime = Time.realtimeSinceStartup;
                m_fMaxTime = RaidManager.GetInst().MainHero.AnimComp.GetAnimTime(CommonString.skill_001Str);
                RaidManager.GetInst().MainHero.AnimComp.PlayAnim(CommonString.skill_001Str, false, CommonString.idle_001Str);
                m_CampState = CAMP_STATE.PREPARE;
                NetworkManager.GetInst().Suspend();
        }

        void OnCampSP(object sender, SCNetMsgEventArgs e)
        {
                SCMsgRaidCampSP msg = e.mNetMsg as SCMsgRaidCampSP;
                m_nSkillPoint = msg.sp;
                UI_CampSkill uis = UIManager.GetInst().GetUIBehaviour<UI_CampSkill>();
                if (uis != null)
                {
                        uis.SetSP(m_nSkillPoint);
                }
        }

        void OnCampEnter(object sender, SCNetMsgEventArgs e)
        {
                SCMsgRaidCampState msg = e.mNetMsg as SCMsgRaidCampState;
                m_SkillPlayedDict.Clear();
                if (msg.flag == 1)
                {
                        EnterCamp();
                }
        }
        //释放过的技能字典
        Dictionary<long, List<int>> m_SkillPlayedDict = new Dictionary<long, List<int>>();
        public bool IsSkillAvailable(long heroId, int skillId)
        {
                if (m_SkillPlayedDict.ContainsKey(heroId))
                {
                        if (m_SkillPlayedDict.ContainsKey(skillId))
                        {
                                return false;
                        }
                }
                return true;
        }

        public void ContinueCamp(string msg, int progress, int sp)
        {
                m_SkillPlayedDict.Clear();
                m_nCampProgress = progress;
                m_nSkillPoint = sp;
                string[] infos = msg.Split('&');
                foreach (string info in infos)
                {
                        if (string.IsNullOrEmpty(info))
                        {
                                continue;
                        }

                        string[] tmps = info.Split('|');
                        if (tmps.Length == 2)
                        {
                                long petId = 0;
                                long.TryParse(tmps[0], out petId);
                                if (petId > 0)
                                {
                                        m_SkillPlayedDict.Add(petId, new List<int>());
                                        string[] skills = tmps[1].Split(',');
                                        foreach (string skillStr in skills)
                                        {
                                                if (!string.IsNullOrEmpty(skillStr))
                                                {
                                                        m_SkillPlayedDict[petId].Add(int.Parse(skillStr));
                                                }
                                        }
                                }
                        }
                }
                EnterCamp();
        }

        IEnumerator ProcessingMoving(HeroUnit unit, Vector3 point, Transform targetTrans)
        {
                m_nMovingUnitCount++;
                unit.GoTo(point);

                while (unit.PathComp.canMove)
                {
                        yield return null;
                }
                Vector3 dir = targetTrans.position - point;
                Quaternion rotation = Quaternion.LookRotation(dir);
                rotation.eulerAngles = new Vector3(0f, rotation.eulerAngles.y, 0f);

                iTween.moveToWorld(unit.gameObject, 0.5f, 0f, point, iTween.EasingType.linear);
                iTween.rotateTo(unit.gameObject, 0.5f, 0f, rotation.eulerAngles);
                yield return new WaitForSeconds(0.5f);

                GameObject effectObj = EffectManager.GetInst().GetEffectObj("normal_heal_effect");
                effectObj.transform.position = unit.transform.position;

                m_nMovingUnitCount--;
        }

        GameObject GenerateCampModel(Vector3 pos, float rotY)
        {
                GameObject obj = ModelResourceManager.GetInst().GenerateObject(6016);
                obj.transform.position = pos;
                obj.transform.rotation = Quaternion.Euler(new Vector3(0f, rotY, 0f));

                return obj;
        }

        HeroUnit CheckInput()
        {
                if (InputManager.GetInst().GetInputUp(false))
                {
                        RaycastHit hit = new RaycastHit();
                        Ray ray = Camera.main.ScreenPointToRay(InputManager.GetInst().GetInputPosition());
                        int mask = 1 << LayerMask.NameToLayer("Character");
                        if (Physics.Raycast(ray, out hit, Camera.main.farClipPlane, mask))
                        {
                                HeroUnit unit = hit.collider.GetComponent<HeroUnit>();
                                if (unit != null)
                                {
                                        return unit;
                                }
                        }
                }
                return null;
        }
       
        public void Update()
        {
                switch (m_CampState)
                {
                        case CAMP_STATE.NONE:
                                break;
                        case CAMP_STATE.PREPARE:
                                {
                                        if (Time.realtimeSinceStartup - m_fTime > m_fMaxTime)
                                        {
                                                bool bVertical = false;
                                                List<Vector3> poslist = RaidManager.GetInst().GotoCampPoint(ref bVertical);
                                                m_CampObj = GenerateCampModel(poslist[0], bVertical ? 270f : 0f);
                                                if (m_CampObj != null)
                                                {
                                                        int idx = 1;
                                                        foreach (HeroUnit unit in RaidTeamManager.GetInst().GetHeroList())
                                                        {
                                                                if (idx < poslist.Count)
                                                                {
                                                                        AppMain.GetInst().StartCoroutine(ProcessingMoving(unit, poslist[idx], m_CampObj.transform));
                                                                        idx++;
                                                                }
                                                        }
                                                        Vector3 newpos = m_CampObj.transform.position - Camera.main.transform.forward * 10f;
                                                        iTween.moveToWorld(Camera.main.gameObject, 0.5f, 0f, newpos);
                                                }
                                                m_CampState = CAMP_STATE.INPOSITION;
                                        }
                                }
                                break;
                        case CAMP_STATE.INPOSITION:
                                {
                                        if (m_nMovingUnitCount <= 0)
                                        {
                                                NetworkManager.GetInst().WakeUp();

                                                if (m_nCampProgress == 1)
                                                {
                                                        m_CampState = CAMP_STATE.EAT;
                                                        UI_RaidCampFood uis  = UIManager.GetInst().ShowUI<UI_RaidCampFood>();
                                                        uis.Setup();
                                                }
                                                else
                                                {
                                                        UI_CampSkill uis = UIManager.GetInst().ShowUI<UI_CampSkill>("UI_CampSkill");
                                                        uis.SetSP(m_nSkillPoint);
                                                        uis.CloseSkill();
                                                        GotoSelectActor();                                                        
                                                }
                                        }
                                }
                                break;
                        case CAMP_STATE.EAT:
                                {                                        
                                }
                                break;
                        case CAMP_STATE.SELECT_ACTOR:
                                {
                                        HeroUnit actionUnit = CheckInput();
                                        if (actionUnit != null)
                                        {
                                                if (m_ActionUnit != null)
                                                {
                                                        m_ActionUnit.ActionUnitSelectEffect.SetActive(false);
                                                }

                                                actionUnit.ActionUnitSelectEffect.SetActive(true);
                                                UI_CampSkill uis = UIManager.GetInst().GetUIBehaviour<UI_CampSkill>();
                                                if (uis != null)
                                                {
                                                        uis.SetHero(actionUnit);
                                                }
                                                m_ActionUnit = actionUnit;
                                        }
                                }
                                break;
                        case CAMP_STATE.SELECT_SKILL:
                                {
                                        m_TargetUnit = CheckInput();
                                        if (m_TargetUnit != null)
                                        {
                                                m_ActionSkillCfg = null;
                                                UI_CampSkill uis = UIManager.GetInst().GetUIBehaviour<UI_CampSkill>();
                                                if (m_RaidCampSkillDict.ContainsKey(uis.SelectCampSkillCfgId))
                                                {
                                                        m_ActionSkillCfg = m_RaidCampSkillDict[uis.SelectCampSkillCfgId];
                                                        if (m_ActionSkillCfg.cost_point <= m_nSkillPoint)
                                                        {
                                                                m_nSkillPoint -= m_ActionSkillCfg.cost_point;
                                                                uis.SetSP(m_nSkillPoint);
                                                                m_fTime = Time.realtimeSinceStartup;
                                                                m_fMaxTime = m_ActionUnit.AnimComp.GetAnimTime(m_ActionSkillCfg.action_id);
                                                                m_ActionUnit.AnimComp.PlayAnim(m_ActionSkillCfg.action_id, false, CommonString.idle_001Str);

                                                                CSMsgRaidCampSkill msg = new CSMsgRaidCampSkill();
                                                                msg.idSrcHero = m_ActionUnit.hero.ID;
                                                                msg.idDestHero = m_TargetUnit.hero.ID;
                                                                msg.idCampSkillCfg = uis.SelectCampSkillCfgId;
                                                                NetworkManager.GetInst().SendMsgToServer(msg);
                                                                NetworkManager.GetInst().Suspend();
                                                                m_CampState = CAMP_STATE.SKILL_PLAY;
                                                                uis.CloseSkill();

                                                                RaidManager.GetInst().UnitTalk(TALK_TYPE.CAMP_SKILL, m_ActionUnit);
                                                                
                                                                foreach (HeroUnit unit in RaidTeamManager.GetInst().GetHeroList())
                                                                {
                                                                        unit.DestroySelectEffect();
                                                                }

                                                        }
                                                        else
                                                        {
                                                                GameUtility.PopupMessage("技能点不够");
                                                        }
                                                }
                                        }
                                }
                                break;
                        case CAMP_STATE.SKILL_PLAY:
                                {
                                        if (Time.realtimeSinceStartup - m_fTime >= m_fMaxTime)
                                        {
                                                GameObject obj = EffectManager.GetInst().GetEffectObj(m_ActionSkillCfg.skill_effect);
                                                if (obj != null)
                                                {
                                                        obj.transform.position = m_TargetUnit.transform.position;
                                                }
                                                NetworkManager.GetInst().WakeUp();
                                                GotoSelectActor();
                                        }
                                }
                                break;
                        case CAMP_STATE.EVENT:
                                {
                                }
                                break;
                        case CAMP_STATE.END:
                                {
                                        
                                }
                                break;
                }
        }

        void OnCampEvent(object sender, SCNetMsgEventArgs e)
        {
                NetworkManager.GetInst().Suspend();

                SCMsgRaidCampEvent msg = e.mNetMsg as SCMsgRaidCampEvent;
                if (m_RaidCampEventDict.ContainsKey(msg.eventId))
                {
                        HeroUnit unit = RaidTeamManager.GetInst().GetHeroUnitByID(msg.idHero);
                        UI_CampEvent uis = UIManager.GetInst().ShowUI<UI_CampEvent>("UI_CampEvent", 2f);
                        uis.Setup(m_RaidCampEventDict[msg.eventId], unit, msg.strParam);
                }
        }

        public void ExitCamp()
        {
                CSMsgRaidCampFinish msg = new CSMsgRaidCampFinish();
                NetworkManager.GetInst().SendMsgToServer(msg);
                GameObject.Destroy(m_CampObj);
                m_CampObj = null;
                RaidManager.GetInst().ExitCamp();
                UIManager.GetInst().CloseUI("UI_CampSkill");
                foreach (HeroUnit unit in RaidTeamManager.GetInst().GetHeroList())
                {
                        unit.DestroySelectEffect();
                        if (unit.IsAlive)
                        {
                                RaidManager.GetInst().UnitTalk(TALK_TYPE.CAMP_END, unit);
                        }
                }
                m_ActionUnit = null;
                m_TargetUnit = null;
                m_CampState = CAMP_STATE.NONE;
        }
        public void FinishCampEat(string foodStr)
        {
                CSMsgRaidCampEat msg = new CSMsgRaidCampEat();
                msg.strFood = foodStr;
                NetworkManager.GetInst().SendMsgToServer(msg);
                UI_CampSkill uis = UIManager.GetInst().ShowUI<UI_CampSkill>("UI_CampSkill");
                uis.SetSP(m_nSkillPoint);
                uis.CloseSkill();
                GotoSelectActor();
        }
        public void GotoSelectActor()
        {
                if (m_ActionUnit != null)
                {
                        m_ActionUnit.DestroySelectEffect();
                }
                m_ActionUnit = null;
                m_TargetUnit = null;
                
                m_CampState = CAMP_STATE.SELECT_ACTOR;
        }
        public void SelectSkill()
        {
                m_CampState = CAMP_STATE.SELECT_SKILL;
                foreach (HeroUnit unit in RaidTeamManager.GetInst().GetHeroList())
                {
                        if (unit.IsAlive)
                        {
                                unit.TargetUnitSelectEffect.SetActive(true);
                        }
                }
        }
}