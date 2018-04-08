using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Message;
public enum COMBAT_STATE
{
        NONE,
        IN_POSITION,    //各就各位
        LOAD_RES,
        START,
        START_TRIGGER_SKILL,
        ROUND_BEFORE_SKILL,        //回合前置技能
        ROUND_CALC,
        ROUND_END,
        PRESSURE_TORTURE,               //精神拷问
        WAIT_INPUT,
        ROUND_PLAY,
        ROUND_AFTER_SKILL,              //回合后置技能
        SHOW_RESULT,
        END,
};


class CombatManager : SingletonObject<CombatManager>
{

        public enum COMBAT_STAGE
        {
                NONE,
                RAID,
                VILLAGE,
        };

        COMBAT_STAGE m_CombatStage = COMBAT_STAGE.NONE;

        public bool HasUnfinishBattle = false;

        bool m_bAuto = false;
        public bool IsAuto
        {
                get
                {
                        return m_bAuto;
                }
                set
                {
                        m_bAuto = value;
                        if (m_CombatState == COMBAT_STATE.WAIT_INPUT)
                        {
                                AutoAction();
                                ClearSelectArrow();
                                m_UISCombatSkill.ShowSkillGroup(false);
                        }
                }
        }
        public void SaveAutoState(bool bAuto)
        {
                IsAuto = bAuto;
                GameUtility.SavePlayerData(PlayerController.GetInst().PlayerID + "CombatAuto", IsAuto ? CommonString.oneStr : CommonString.zeroStr);
        }
        public void LoadAutoState()
        {
                IsAuto = GameUtility.GetPlayerData(PlayerController.GetInst().PlayerID + "CombatAuto") == CommonString.oneStr;
        }

        GameObject m_EnemyTeamNode;
        GameObject EnemyTeamNode
        {
                get
                {
                        if (m_EnemyTeamNode == null)
                        {
                                m_EnemyTeamNode = GameObject.Find("EnemyTeam");
                        }
                        if (m_EnemyTeamNode == null)
                        {
                                m_EnemyTeamNode = new GameObject("EnemyTeam");
                        }
                        return m_EnemyTeamNode;
                }
        }

        public Vector3 CenterPoint
        {
                get
                {
                        if (m_MyBP != null && m_EnemyBP != null)
                        {
                                return m_MyBP.transform.position + (m_EnemyBP.transform.position - m_MyBP.transform.position) / 2f;
                        }
                        return Vector3.zero;
                }
        }

        public Transform GetEffectPoint(bool isEnemy)
        {
                if (isEnemy)
                {
                        return m_EnemyBP.transform;
                }
                else
                {
                        return m_MyBP.transform;
                }
        }

        GameObject m_SelectCircle;
        public void SetSelectFighter(FighterBehav fighter)
        {
                GameObject.Destroy(m_SelectCircle);
                if (fighter.IsEnemy == false)
                {
                        m_SelectCircle = EffectManager.GetInst().GetEffectObj("effect_battle_teammate_circle_001");
                }
                else
                {
                        if (fighter.CharacterCfg.GetPropInt("body_type") == 0)
                        {
                                m_SelectCircle = EffectManager.GetInst().GetEffectObj("effect_battle_monster_small_circle_001");
                        }
                        else
                        {
                                m_SelectCircle = EffectManager.GetInst().GetEffectObj("effect_battle_monster_big_circle_001");
                        }
                }
                if (m_SelectCircle != null)
                {
                        m_SelectCircle.transform.position = fighter.transform.position + Vector3.up * 0.1f;
                }
        }

        GameObject m_BattleScene;

        int m_nCombatId;
        float m_fUpdateTime = 0f;
        bool m_bInCombatStart = true;    //是否正在开场触发技能释放过程
        bool m_bRoundFighterPlayed = false; //本轮行动者主技能放过没
        bool m_bCheckAfterAction = false;       //本轮行动后这个时机判断过没
        FighterBehav m_RoundFighter;//本回合主行动者
        FighterBehav m_ActionFighter;//当前技能行动者
        SkillConfig m_ActionSkill;       //手动或者AI选出来的技能
        FighterBehav m_ActionTarget;//手动或者AI选出来的目标

        COMBAT_STATE m_CombatState = COMBAT_STATE.NONE;

        void SetCombatState(COMBAT_STATE state)
        {
                m_CombatState = state;
                Debug.Log("SetCombatState " + state);
        }
        public bool IsInCombat()
        {
                return m_CombatState != COMBAT_STATE.NONE;
        }

        UI_CombatSkill m_UISCombatSkill;
        UI_CombatTimeline m_UI_CombatTimeline;
        List<ActionSkillData> m_ExtraSkillList = new List<ActionSkillData>();
        List<FighterBehav> m_AllFighters = new List<FighterBehav>();
        List<FighterBehav> m_MyFighterList = new List<FighterBehav>();
        List<FighterBehav> m_EnemyFighterList = new List<FighterBehav>();

        List<FighterTrigger> m_RoundTriggerList = new List<FighterTrigger>();       //本轮触发器ID列表，用来判断是否触发过

        List<SCMsgBattleFighter> m_MsgList = new List<SCMsgBattleFighter>();


        int m_nMaxModelCount = 0;
        int m_BattleID;
        int m_nEventNodeId;     //战斗发生的节点（以该点为战场中心）
        int m_nRound = 0;
        int _nBright = 0;
        int m_nBright
        {
                get
                {
                        return _nBright;
                }
                set
                {
                        _nBright = value;
                        CombatManager.GetInst().RoundInfo_Log("设置光亮值=" + value);
                }
        }
        string m_sBattleSceneName = "";
        public int BrightLevel
        {
                get
                {
                        if (m_nBright > 100)
                        {
                                return 0;
                        }
                        else
                        {
                                return (100 - m_nBright) / 25;
                        }
                }
        }

        public void ChangeBright(int delta)
        {
                m_nBright += delta;
                RaidManager.GetInst().SetBright(m_nBright);
                CombatManager.GetInst().RoundInfo_Log("技能改变了光亮值  delta=" + delta);
        }

        public void Init()
        {
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgBattle), OnBattle);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgBattleFighter), OnBattleFighter);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgBattleStart), OnBattleStart);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgBattleRoundInfo), OnBattleRoundInfo);
        }

        class ResumeInfo
        {
                public int nRound;
                public long idFighter;
                public int idSkill;
                public long idTarget;
                public ResumeInfo(string info)
                {
                        string[] infos = info.Split('&');
                        if (infos.Length >= 4)
                        {
                                nRound = int.Parse(infos[0]);
                                idFighter = long.Parse(infos[1]);
                                idSkill = int.Parse(infos[2]);
                                idTarget = long.Parse(infos[3]);
                        }
                }
        }

        Dictionary<int, ResumeInfo> m_ResumeInfoList = new Dictionary<int, ResumeInfo>();
        int m_nMaxResumeRound = -1;

        public bool IsResume()
        {
                return m_nMaxResumeRound >= m_nRound;
        }

        void OnBattle(object sender, SCNetMsgEventArgs e)
        {
                SCMsgBattle msg = e.mNetMsg as SCMsgBattle;
                m_BattleID = msg.idBattle;
                RandomCreater.seed = msg.nSeed;
                m_MsgList.Clear();
                m_sBattleSceneName = msg.battleScene;
                m_nBright = msg.nBright;
                m_nEventNodeId = msg.idNode;

                m_ServerRoundDict.Clear();
                m_LocalRoundDict.Clear();
                m_ResumeInfoList.Clear();
                m_nMaxResumeRound = -1;
                string[] replaylist = msg.strCmdList.Split('|');
                foreach (string info in replaylist)
                {
                        if (!string.IsNullOrEmpty(info))
                        {
                                ResumeInfo ri = new ResumeInfo(info);
                                if (ri.nRound > 0 && !m_ResumeInfoList.ContainsKey(ri.nRound))
                                {
                                        m_ResumeInfoList.Add(ri.nRound, ri);
                                        m_nMaxResumeRound = Mathf.Max(m_nMaxResumeRound, ri.nRound);                                        
                                }
                        }
                }
        }

        void OnBattleFighter(object sender, SCNetMsgEventArgs e)
        {
                SCMsgBattleFighter msg = e.mNetMsg as SCMsgBattleFighter;
                m_MsgList.Add(msg);
        }

        void OnBattleStart(object sender, SCNetMsgEventArgs e)
        {
                SCMsgBattleStart msg = e.mNetMsg as SCMsgBattleStart;
                m_nRound = 0;
                EnterCombat();

                if (!m_LocalRoundDict.ContainsKey(m_nRound))
                {
                        m_LocalRoundDict.Add(m_nRound, new BattleRoundInfo());
                }
        }

        public void SendRoundInfo(SkillConfig skillCfg, FighterBehav target)
        {
                CSMsgBattleRound msg = new CSMsgBattleRound();
                msg.idBattle = m_BattleID;
                msg.round = m_nRound;
                msg.fighterId = m_ActionFighter.FighterId;
                msg.skillId = skillCfg != null ? skillCfg.id : 0;
                msg.targetId = target != null ? target.FighterId : 0;

                NetworkManager.GetInst().SendMsgToServer(msg);
        }

        public FighterBehav GetFighter(long idFighter)
        {
                return m_AllFighters.Find((x) => { return x.FighterId == idFighter; });
        }
        public List<FighterBehav> GetFighterList(bool isEnemy)
        {
                if (isEnemy)
                {
                        return new List<FighterBehav>(m_EnemyFighterList);
                }
                else
                {
                        return new List<FighterBehav>(m_MyFighterList);
                }
        }

        public List<FighterBehav> GetAllFighters()
        {
                return m_AllFighters;
        }

        void DestroyAllFighters()
        {
                if (m_MyBP != null)
                {
                        GameObject.Destroy(m_MyBP.gameObject);
                        m_MyBP = null;
                }
                if (m_EnemyBP != null)
                {
                        GameObject.Destroy(m_EnemyBP.gameObject);
                        m_EnemyBP = null;
                }
                foreach (HeroUnit unit in RaidTeamManager.GetInst().GetHeroList())
                {
                        if (unit != null && unit.Mod != null)
                        {
                                if (unit.RelateFighter != null)
                                {
                                        unit.gameObject.SetActive(true);
                                        unit.RelateFighter.IsHide = false;

                                        unit.transform.position = unit.Mod.transform.position;
                                        unit.transform.rotation = unit.Mod.transform.rotation;
                                        unit.UpdateCurrentPos();
                                        unit.Mod.transform.SetParent(unit.transform);
                                        unit.Mod.transform.localPosition = Vector3.zero;
                                        unit.Mod.transform.localRotation = Quaternion.identity;

                                        unit.hero.SetProperty("pressure", unit.RelateFighter.FighterProp.Pressure);
                                        unit.hero.SetProperty("hp", unit.RelateFighter.FighterProp.Hp);
                                        Debug.Log(unit.ID + " hp=" + unit.RelateFighter.FighterProp.Hp);
                                        if (unit.RelateFighter.FighterProp.Hp > 0)
                                        {
                                                unit.AnimComp.PlayAnim(CommonString.idle_001Str, true);
                                        }
                                        else
                                        {
                                                unit.AnimComp.CurrentAnim = "die_001";  //Fighter已经播放过die 了，所以unit不用播放
                                        }
                                }
                        }
                }

                foreach (FighterBehav tb in m_AllFighters)
                {
                        if (tb != null && tb.gameObject != null)
                        {
                                if (tb.IsEnemy)
                                {
                                        GameObject obj = EffectManager.GetInst().GetEffectObj("effect_battle_monster_corpse_disappear_001");
                                        obj.transform.position = tb.gameObject.transform.position;
                                }
                                GameObject.Destroy(tb.gameObject);
                        }
                }
                m_AllFighters.Clear();
                m_EnemyFighterList.Clear();
                m_MyFighterList.Clear();
        }
        RaidBattlePointBehav m_MyBP;
        RaidBattlePointBehav m_EnemyBP;
        Dictionary<GameObject, int> m_ExistedObjList;

        public void EnterCombat()
        {
                InputManager.GetInst().UpdateInputReset();
                InputManager.GetInst().onMultiTouchMove = OnMultiTouchMove;

                LoadAutoState();
                m_UISCombatSkill = UIManager.GetInst().ShowUI<UI_CombatSkill>("UI_CombatSkill", 0f);
                m_UISCombatSkill.SetBtnAuto(IsAuto);
                m_fUpdateTime = Time.realtimeSinceStartup;
                SetCombatState(COMBAT_STATE.IN_POSITION);
                DestroyAllFighters();

                if (GameStateManager.GetInst().GameState == GAMESTATE.RAID_PLAYING)
                {
                        m_CombatStage = COMBAT_STAGE.RAID;
                        RaidManager.GetInst().GetBattlePoints(m_nEventNodeId, ref m_MyBP, ref m_EnemyBP);
                        RaidManager.GetInst().EnterCombat();
                        m_ExistedObjList = RaidManager.GetInst().GetBattleEnemyObjList();
                        //RaidTeamManager.GetInst().TeamGotoBattle(m_MyBattlePoint, GetForward(false));
                }
                else if (GameStateManager.GetInst().GameState == GAMESTATE.VILLAGE)
                {
                        m_CombatStage = COMBAT_STAGE.VILLAGE;
                        VillageManager.GetInst().EnterCombat();
                        VillageManager.GetInst().GetBattlePoints(m_nEventNodeId, ref m_MyBP, ref m_EnemyBP);
                        m_ExistedObjList = VillageManager.GetInst().GetBattleEnemyObjList();
                }

                PrepareFighters();

        }

        FighterBehav InitFighter(SCMsgBattleFighter msg)
        {
                GameObject fighterObj = new GameObject();
                fighterObj.name = "Fighter" + msg.idFighter;

                FighterBehav behav = fighterObj.AddComponent<FighterBehav>();
                behav.CharacterId = msg.characterId;
                behav.FighterId = msg.idFighter;
                behav.IsEnemy = msg.group == 1;
                behav.AnimComp.Model_Id = CharacterManager.GetInst().GetCharacterModelId(msg.characterId);
                behav.FighterProp.LoadDataFromMsg(msg);
                return behav;
        }
        int m_nHeroInPosCount = 0;
        void AttachHeroUnit(FighterBehav fighterBehav)
        {
                List<HeroUnit> herolist = RaidTeamManager.GetInst().GetHeroList();
                foreach (HeroUnit unit in herolist)
                {
                        if (unit.RelateFighter != null)
                                continue;

                        if (unit.CharacterID == fighterBehav.CharacterId)
                        {
                                unit.RelateFighter = fighterBehav;

                                fighterBehav.transform.position = unit.Mod.transform.position;
                                unit.Mod.transform.SetParent(fighterBehav.transform);
                                unit.Mod.transform.localPosition = Vector3.zero;
                                unit.Mod.transform.localRotation = Quaternion.identity;
                                unit.gameObject.SetActive(false);
                                break;
                        }
                }
        }
        void AttachEnemyObj(FighterBehav fighterBehav, CharacterConfig cfg)
        {
                GameObject enemyObj = null;
                bool bUseExist = false;
                if (m_ExistedObjList != null)
                {
                        foreach (var param in m_ExistedObjList)
                        {
                                if (cfg.modelid == param.Value)
                                {
                                        fighterBehav.transform.position = param.Key.transform.position;
                                        fighterBehav.transform.rotation = param.Key.transform.rotation;
                                        enemyObj = param.Key;
                                        m_ExistedObjList.Remove(param.Key);
                                        Debug.LogError("UseExistObj " + param.Value);
                                        bUseExist = true;
                                        break;
                                }
                        }
                }
                if (enemyObj == null)
                {
                        enemyObj = CharacterManager.GetInst().GenerateModel(cfg);
                }

                enemyObj.transform.SetParent(fighterBehav.transform);
                enemyObj.transform.localPosition = Vector3.zero;
                enemyObj.transform.localRotation = Quaternion.identity;
                fighterBehav.AnimComp.ResetAnimCtr();
                fighterBehav.PlayIdle();

                int idx = (int)fighterBehav.FighterId % 10 - 1;
                fighterBehav.transform.rotation = GetForward(true);
                fighterBehav.transform.position = m_EnemyBP.posArray[idx];
                if (bUseExist == false)
                {
                        fighterBehav.transform.position += Vector3.up * 20f;
                }
                iTween.moveTo(fighterBehav.gameObject, 0.5f, idx * 0.2f, m_EnemyBP.posArray[idx]);
                fighterBehav.SetOriPosition(m_EnemyBP.posArray[idx]);
        }

        IEnumerator FighterInPosition(FighterBehav fighter)
        {
                m_nHeroInPosCount++;
                Vector3 targetPos = m_MyBP.posArray[fighter.BattlePos];
                float dist = Vector3.Distance(targetPos, fighter.transform.position);
                if (dist >= UNMOVE_DIST)
                {
                        GameUtility.RotateTowards(fighter.transform, targetPos);
                        float time = dist / RaidManager.RUN_SPEED * 1.5f;
                        fighter.AnimComp.ResetAnimCtr();
                        fighter.AnimComp.PlayAnim("run_001", true);
                        iTween.moveToWorld(fighter.gameObject, time, 0f, targetPos, iTween.EasingType.linear);
                        yield return new WaitForSeconds(time);
                        fighter.AnimComp.StopAnim();
                }

                fighter.transform.rotation = GetForward(false);
                fighter.SetOriPosition(fighter.transform.position);
                fighter.AnimComp.ResetAnimCtr();
                fighter.PlayIdle();

                m_nHeroInPosCount--;
        }
        float UNMOVE_DIST = 0.5f;
        void PrepareFighters()
        {
                m_nMaxModelCount = m_MsgList.Count;
                m_AllFighters.Clear();

                FighterBehav bossBehav = null;
                foreach (SCMsgBattleFighter msg in m_MsgList)
                {
                        CharacterConfig cfg = CharacterManager.GetInst().GetCharacterCfg(msg.characterId);
                        if (cfg != null)
                        {
                                FighterBehav fighterBehav = InitFighter(msg);
                                if (fighterBehav.IsEnemy)
                                {
                                        m_EnemyFighterList.Add(fighterBehav);
                                        AttachEnemyObj(fighterBehav, cfg);
                                        if (fighterBehav.CharacterCfg.body_type == 2)
                                        {
                                                bossBehav = fighterBehav;
                                        }
                                }
                                else
                                {
                                        m_MyFighterList.Add(fighterBehav);
                                        AttachHeroUnit(fighterBehav);
                                }
                                m_AllFighters.Add(fighterBehav);
                                GameUtility.SetLayer(fighterBehav.gameObject, "Character");
                        }
                }

                foreach (FighterBehav fighter in m_MyFighterList)
                {
                        int startIdx = fighter.IsFront ? 0 : 3;
                        for (int i = startIdx; i < startIdx + 3; i++)
                        {
                                if (m_MyBP.fighterArray[i] == null)
                                {
                                        if (Vector3.Distance(fighter.transform.position, m_MyBP.posArray[i]) < UNMOVE_DIST)
                                        {
                                                m_MyBP.fighterArray[i] = fighter;
                                                m_MyBP.ResetPos(fighter.IsFront, i, fighter.transform.position);
                                                fighter.BattlePos = i;
                                        }
                                }
                        }
                }
                foreach (FighterBehav fighter in m_MyFighterList)
                {
                        if (fighter.BattlePos < 0)
                        {
                                fighter.BattlePos = m_MyBP.GetEmptyPos(fighter);
                                m_MyBP.fighterArray[fighter.BattlePos] = fighter;
                        }

                        AppMain.GetInst().StartCoroutine(FighterInPosition(fighter));
                }

                if (bossBehav != null)
                {
                        Vector3 rot = m_EnemyBP.transform.rotation.eulerAngles;
                        rot.y += 180f;
                        RaidManager.GetInst().SwitchToBossCamera(rot.y);
                }
                else
                {
                        RaidManager.GetInst().SwitchCameraToCombat();
                }

                foreach (GameObject obj in m_ExistedObjList.Keys)
                {
                        if (obj != null)
                        {
                                obj.SetActive(false);
                        }
                }
        }

        public Quaternion GetForward(bool isEnemy)
        {
                if (isEnemy)
                {
                        if (m_EnemyBP != null)
                        {
                                return m_EnemyBP.transform.rotation;
                        }
                }
                else
                {
                        if (m_MyBP != null)
                        {
                                return m_MyBP.transform.rotation;
                        }
                }
                return Quaternion.identity;
        }

        bool IsMyTeamAllDead()
        {
                bool bAllDead = true;
                foreach (FighterBehav fighter in m_MyFighterList)
                {
                        if (fighter.FighterProp.Hp > 0)
                        {
                                bAllDead = false;
                        }
                }
                return bAllDead;
        }
        bool IsEnemyTeamAllDead()
        {
                bool bAllDead = true;
                foreach (FighterBehav fighter in m_EnemyFighterList)
                {
                        if (fighter.FighterProp.Hp > 0)
                        {
                                bAllDead = false;
                        }
                }
                return bAllDead;
        }

        void CalcReplayInfo()
        {
        }

        void CheckRoundAction()
        {
                foreach (FighterBehav behav in m_AllFighters)
                {
                        //若有战斗单位行为未结束，则返回
                        if (behav.ActionState != FIGHTER_ACTION.NONE && !behav.IsDead())
                        {
                                return;
                        }
                }

                if (IsMyTeamAllDead())
                {
                        CombatResult(false);
                        return;
                }
                else if (IsEnemyTeamAllDead())
                {
                        CombatResult(true);
                        return;
                }


                m_nRound++;
                if (!m_LocalRoundDict.ContainsKey(m_nRound))
                {
                        m_LocalRoundDict.Add(m_nRound, new BattleRoundInfo());
                }
                Debuger.LogError("COMBAT_ROUND = " + m_nRound);

                //m_UI_CombatTimeline.ResetTimeline();
                m_AllFighters.Sort(FighterBehav.CompareSpeed);

                string delaylog = "";
                foreach (FighterBehav behav in m_AllFighters)
                {
                        behav.FighterProp.ResetPressureLR();
                        delaylog += (behav.FighterId + " delay=" + behav.FighterProp.DelayVal + " hp=" + behav.FighterProp.Hp + "/" + behav.FighterProp.MaxHp + "\n");
                }
                CombatManager.GetInst().RoundInfo_Log(delaylog, 0);

                List<FighterBehav> timelineList = new List<FighterBehav>(m_AllFighters);
                int delta = -1;
                for (int i = 0; i < m_AllFighters.Count; i++)
                {
                        FighterBehav behav = m_AllFighters[i];
                        if (behav.FighterProp.Hp <= 0)
                                continue;

                        if (delta < 0f)
                        {
                                delta = behav.FighterProp.DelayVal;
                                behav.FighterProp.ResetDelay();

                                if (behav.IsEnemy == false)
                                {
                                        m_nBright += GlobalParams.GetInt("brightness_cost_round");
                                        if (m_nBright <= 0)
                                        {
                                                m_nBright = 0;
                                        }
                                }
                                m_RoundFighter = behav;
                                m_RoundFighter.CheckBuffBeforeAction();
                                if (m_RoundFighter.FighterProp.Hp > 0)
                                {
                                        CombatManager.GetInst().CheckAllTriggers(m_RoundFighter, FIGHTER_TRIGGER_TYPE.BEFORE_ACTION);
                                        if (IsResume() == false)
                                        {
                                                SetSelectFighter(m_RoundFighter);
                                        }
                                        m_bRoundFighterPlayed = false;
                                        m_bCheckAfterAction = false;
                                        SetCombatState(COMBAT_STATE.ROUND_BEFORE_SKILL);
                                }
                                else
                                {
                                        SetCombatState(COMBAT_STATE.ROUND_AFTER_SKILL);
                                }
                        }
                        else
                        {
                                behav.FighterProp.DelayVal -= delta;
                                if (behav.FighterProp.DelayVal <= 0)
                                {
                                        behav.FighterProp.DelayVal = 0;
                                }
                        }
                }
        }

        public FighterBehav GetRandomTarget(List<FighterBehav> list, string mark="")
        {
                if (list.Count > 0)
                {
                        int idx = DamageCalc.GetRandomVal(0, list.Count - 1, mark);
                        
                        return list[idx];
                }
                return null;
        }

        /// <summary>
        /// 克制判定：修改后的克制变得不再普遍，作为技能属性与单位属性的相互关系存在，例如屠龙术技能对龙属性的单位造成额外100%伤害			
        // ★	当存在克制/被克制且最大克制系数唯一时，必定选择该目标（多个克制系数相加最大值）		
        // ★	当存在克制/被克制且多个相同时，在该范围内继续判定濒死目标
        // ★	不存在时，继续判定濒死目标
        /// </summary>
        /// <param name="targetlist"></param>
        /// <returns></returns>
        public FighterBehav CalcCounterFighter(ref List<FighterBehav> targetlist)
        {
                return null;
        }

        /// <summary>
        /// 濒死判定：倾向于攻击血量%最低的目标；初始判定几率为a1%，若该单位血量%低于b%时，判定几率提高到a2%（a1，b，a2均全局配置）			
        // ★	判定成功且最低唯一时，选择该目标		
        // ★	判定成功且存在多个相同时，在该范围内继续4		
        // ★	判定失败时，继续4		
        // 此处概率判定流程为，按几率判定一次，判定成功后走星号条件		
        /// <param name="targetlist">目标列表</param>
        /// <returns></returns>
        public FighterBehav CalcDyingFighter(ref List<FighterBehav> targetlist)
        {
                List<FighterBehav> retlist = new List<FighterBehav>();
                int minVal = int.MaxValue;
                foreach (FighterBehav fighter in targetlist)
                {
                        int tmpVal = fighter.FighterProp.HpRatio;
                        if (tmpVal <= minVal)
                        {
                                if (tmpVal < minVal)
                                {
                                        retlist.Clear();
                                }
                                minVal = tmpVal;
                                retlist.Add(fighter);
                        }
                }
                if (retlist.Count > 0)
                {
                        bool bDying = minVal <= GlobalParams.GetInt("AI_dying_line");
                        if (DamageCalc.CheckRandom(GlobalParams.GetInt(bDying ? "AI_reap_per" : "AI_focus_per"), "AI选择濒死判定"))
                        {
                                if (retlist.Count == 1)
                                {
                                        return retlist[0];
                                }
                                else
                                {
                                        targetlist = retlist;
                                }
                        }
                }
                return null;
        }

        /// <summary>
        /// 判断是否存在剩余生命低于e%的目标（e全局配置）			
        /// ★若存在时，选择其中最低的		
        /// ★若存在多个相同的，在该范围内继续3		
        /// ★若不存在时，剔除该技能并重选技能		
        /// </summary>
        /// <param name="targetlist"></param>
        /// <returns></returns>
        FighterBehav CalcLowHpFighter(ref List<FighterBehav> targetlist)
        {
                List<FighterBehav> retlist = new List<FighterBehav>();
                int minVal = GlobalParams.GetInt("AI_heal_line");
                foreach (FighterBehav fighter in targetlist)
                {
                        int tmpVal = fighter.FighterProp.HpRatio;

                        if (tmpVal <= minVal)
                        {
                                if (tmpVal < minVal)
                                {
                                        retlist.Clear();
                                }
                                minVal = tmpVal;
                                retlist.Add(fighter);
                        }
                }
                if (retlist.Count == 1)
                {
                        return retlist[0];
                }
                else if (retlist.Count > 1)
                {//存在多个最低血量相同时，返回该相同列表，进行下一步随机
                        targetlist = retlist;
                }
                else
                {//不存在低于血量最低线的单位时，也把列表清空，无法进行下一步随机，则返回空主目标到上层，进行下一个技能判定。
                        targetlist = retlist;
                }
                return null;
        }
        /// <summary>
        /// 战力判定：倾向于治疗战力有最高的目标			
        /// ★必定选择战力最高的目标		
        /// ★存在多个相同时，在该范围内继续4		
        /// ★不判定或判定失败时，继续4		
        /// </summary>
        /// <param name="targetlist"></param>
        /// <returns></returns>
        FighterBehav CalcHighBpFighter(ref List<FighterBehav> targetlist)
        {
                return null;
        }

        /// <summary>
        /// 判断是否存在友方尸体			
        ///★	若存在且唯一时，选择该目标		
        ///★	若存在多个时，继续3		
        ///★	若不存在时，剔除该技能并重选技能		
        /// </summary>
        /// <param name="targetlist"></param>
        /// <returns></returns>
        FighterBehav CalcDeadFighter(ref List<FighterBehav> targetlist)
        {
                List<FighterBehav> retlist = new List<FighterBehav>();
                foreach (FighterBehav fighter in targetlist)
                {
                        if (fighter.FighterProp.Hp <= 0)
                        {
                                retlist.Add(fighter);
                        }
                }

                if (retlist.Count == 1)
                {//死者唯一时，返回他
                        return retlist[0];
                }
                else if (retlist.Count > 1)
                {//死者多于1个时，返回空，更新list进入下一步随机。
                        targetlist = retlist;
                }
                else
                {//没有死者时，返回空，重置list(使下一步随机不能进行），返回上层重新选技能
                        targetlist = retlist;
                }
                return null;
        }

        public FighterBehav CalcAISkillTarget(FighterBehav actionFighter, SkillConfig skillCfg, bool bRandom = false)
        {
                FighterBehav mainTarget = null;
                if (skillCfg != null)
                {
                        List<FighterBehav> list = actionFighter.CalcSkillTargetGroup(skillCfg);
                        if (list != null && list.Count > 0)
                        {
                                switch (skillCfg.effect_type)
                                {
                                        case (int)Skill_Effect_Type.DAMAGE:
                                                {
                                                        mainTarget = CalcCounterFighter(ref list);
                                                        if (mainTarget == null)
                                                        {
                                                                mainTarget = CalcDyingFighter(ref list);
                                                        }
                                                        if (mainTarget == null)
                                                        {
                                                                mainTarget = GetRandomTarget(list, "AI技能伤害类型目标");
                                                        }
                                                }
                                                break;
                                        case (int)Skill_Effect_Type.HEAL:
                                                {
                                                        mainTarget = CalcLowHpFighter(ref list);
                                                        if (list.Count > 0)
                                                        {
                                                                if (mainTarget == null)
                                                                {
                                                                        mainTarget = CalcHighBpFighter(ref list);
                                                                }
                                                                if (mainTarget == null)
                                                                {
                                                                        mainTarget = GetRandomTarget(list, "AI技能治疗类型目标");
                                                                }
                                                        }
                                                }
                                                break;
                                        case (int)Skill_Effect_Type.BUFF:
                                                {
                                                        mainTarget = GetRandomTarget(list, "AI技能BUFF类型目标");
                                                }
                                                break;
                                        case (int)Skill_Effect_Type.REVIVE:
                                                {
                                                        mainTarget = CalcDeadFighter(ref list);
                                                        if (list.Count > 0)
                                                        {
                                                                if (mainTarget == null)
                                                                {
                                                                        mainTarget = CalcHighBpFighter(ref list);
                                                                        if (mainTarget == null)
                                                                        {
                                                                                mainTarget = GetRandomTarget(list, "AI技能复活类型目标");
                                                                        }
                                                                }
                                                        }
                                                }
                                                break;
                                }
                        }
                }
                return mainTarget;
        }

        bool CheckAIConfused(FighterBehav fighter)
        {
                int chaos = fighter.CharacterCfg.GetPropInt("chaos_rate");
                if (chaos > 0)
                {
                        if (DamageCalc.CheckRandom(chaos,"AI混乱概率"))
                        {
                                return true;
                        }
                }
                return false;
        }
        /// <summary>
        /// 攻击嘲讽源
        /// </summary>
        void DoAttackTauntTarget()
        {
                m_ActionSkill = m_ActionFighter.GetNormalSkill();
                m_ActionTarget = m_ActionFighter.TauntTarget;
                if (m_ActionSkill != null && m_ActionTarget != null)
                {
                        m_ActionFighter.GoActiveSkill(m_ActionSkill, m_ActionTarget);
                        SetCombatState(COMBAT_STATE.ROUND_PLAY);
                        Debug.Log(m_ActionFighter.FighterId + " AttackTauntTarget = " + m_ActionFighter.TauntTarget.FighterId.ToString());
                }
                else
                {
                        SetCombatState(COMBAT_STATE.ROUND_AFTER_SKILL);
                }
        }

        void DoSelfishAction()
        {
                EffectManager.GetInst().PlayEffect("effect_raid_pressure_result_fear", m_ActionFighter.transform);

                FighterBehav skillTarget = null;
                SkillConfig skillCfg = m_ActionFighter.GetNormalSkill();
                if (skillCfg != null)
                {
                        skillTarget = GetRandomTarget(m_ActionFighter.CalcSkillTargetGroup(skillCfg), "自私选取目标");
                }

                if (skillTarget != null && skillCfg != null)
                {
                        m_ActionSkill = skillCfg;
                        m_ActionTarget = skillTarget;
                        m_ActionFighter.GoActiveSkill(m_ActionSkill, skillTarget);
                        SetCombatState(COMBAT_STATE.ROUND_PLAY);
                }
                else
                {
                        SetCombatState(COMBAT_STATE.ROUND_AFTER_SKILL);
                }
        }

        ActionSkillData CalcAIAction()
        {                
                ActionSkillData asd = new ActionSkillData(null, m_ActionFighter, null);
                if (CheckAIConfused(m_ActionFighter))
                {
                        List<SkillConfig> list = m_ActionFighter.GetAvailSkill(false);
                        if (list.Count > 0)
                        {
                                SkillConfig skillCfg = list[DamageCalc.GetRandomVal(0, list.Count - 1, "AI选取技能")];
                                List<FighterBehav> targetlist = m_ActionFighter.CalcSkillTargetGroup(skillCfg);
                                if (targetlist != null && targetlist.Count > 0)
                                {
                                        asd.mainTarget = GetRandomTarget(targetlist, "AI技能混乱时选取目标");
                                        if (asd.mainTarget != null)
                                        {
                                                asd.skillCfg = skillCfg;
                                        }
                                }
                        }
                }
                else
                {
                        List<SkillConfig> list = m_ActionFighter.GetAvailSkill();
                        foreach (SkillConfig skillCfg in list)
                        {
                                if (skillCfg != null)
                                {
                                        asd.mainTarget = CalcAISkillTarget(m_ActionFighter, skillCfg);
                                        if (asd.mainTarget != null)
                                        {
                                                asd.skillCfg = skillCfg;
                                                break;
                                        }
                                }
                        }
                }
                return asd;
        }
        void AutoAction()
        {
                if (m_PressureState == PRESSURE_STATE.FEAR)
                {
                        EffectManager.GetInst().PlayEffect("effect_raid_pressure_result_fear", m_ActionFighter.transform);
                        CombatManager.GetInst().RoundInfo_Log("触发压力恐惧");
                        //TODO:压力导致的恐惧
                        SetCombatState(COMBAT_STATE.ROUND_AFTER_SKILL);
                        SendRoundInfo(null, null);
                }
                else
                {
                        DamageCalc.IsUseSeed = false;

                        ActionSkillData asd = CalcAIAction();

                        if (m_ActionFighter != null && asd.skillCfg != null && asd.mainTarget != null)
                        {
                                m_ActionSkill = asd.skillCfg;
                                m_ActionTarget = asd.mainTarget;
                                m_ActionFighter.GoActiveSkill(asd.skillCfg, asd.mainTarget);
                                SetCombatState(COMBAT_STATE.ROUND_PLAY);
                        }
                        else
                        {
                                SetCombatState(COMBAT_STATE.ROUND_AFTER_SKILL);
                        }
                        SendRoundInfo(asd.skillCfg, asd.mainTarget);

                        DamageCalc.IsUseSeed = true;
                }
        }

        void AIAction()
        {
                ActionSkillData asd = CalcAIAction();

                if (m_ActionFighter != null && asd.skillCfg != null && asd.mainTarget != null)
                {
                        m_ActionSkill = asd.skillCfg;
                        m_ActionTarget = asd.mainTarget;
                        m_ActionFighter.GoActiveSkill(asd.skillCfg, asd.mainTarget);
                        SetCombatState(COMBAT_STATE.ROUND_PLAY);
                }
                else
                {
                        SetCombatState(COMBAT_STATE.ROUND_AFTER_SKILL);
                }
        }
        public bool CanInput()
        {
                return m_CombatState == COMBAT_STATE.WAIT_INPUT;
        }

        public void ClearSelectArrow()
        {
                foreach (FighterBehav tb in m_AllFighters)
                {
                        tb.SetSelectedEffect(false);
                }
        }
        public void ClearSelectCircle()
        {
                foreach (FighterBehav tb in m_AllFighters)
                {
                        tb.SetSelectCircle(false);
                }
        }

        public void SetTeamSelectIcon(int skillId)
        {
                ClearSelectArrow();
                SkillConfig skillCfg = SkillManager.GetInst().GetActiveSkill(skillId);
                if (skillCfg != null)
                {
                        foreach (FighterBehav behav in m_ActionFighter.CalcSkillTargetGroup(skillCfg))
                        {
                                //behav.SetSelectedEffect(true);
                                if (behav.IsEnemy != m_ActionFighter.IsEnemy)
                                {
                                        int counterFactor = CharacterManager.GetInst().GetCareerFamilyCounter(m_ActionFighter.FighterProp.CareerSys, behav.FighterProp.CareerSys);
                                        behav.SetCounter(counterFactor);
                                }
                                else
                                {
                                        behav.SetCounter(100);
                                }
                        }
                }
        }
        public void ConfirmInput(FighterBehav skillTarget)
        {
                if (skillTarget == null)
                {
                        SendRoundInfo(null, null);
                        SetCombatState(COMBAT_STATE.ROUND_AFTER_SKILL);
                }
                else
                {
                        SkillConfig skillCfg = SkillManager.GetInst().GetActiveSkill(m_UISCombatSkill.GetSelectSkillID());
                        if (skillCfg == null)
                        {
                                Debuger.LogError("Skill " + m_UISCombatSkill.GetSelectSkillID() + " is not exist");
                                return;
                        }

                        if (m_PressureState == PRESSURE_STATE.FEAR)
                        {
                                EffectManager.GetInst().PlayEffect("effect_raid_pressure_result_fear", m_ActionFighter.transform);
                                RoundInfo_Log("触发压力恐惧");
                                SetCombatState(COMBAT_STATE.ROUND_AFTER_SKILL);
                        }
                        else
                        {
                                if (m_ActionFighter != null)
                                {
                                        m_ActionSkill = skillCfg;
                                        m_ActionTarget = skillTarget;
                                        m_ActionFighter.GoActiveSkill(skillCfg, skillTarget);
                                        SetCombatState(COMBAT_STATE.ROUND_PLAY);
                                }
                        }
                        SendRoundInfo(skillCfg, skillTarget);
                }

                ClearSelectArrow();
                m_UISCombatSkill.ShowSkillGroup(false);
        }


        public bool CanTriggerSkill(FighterTrigger trigger)
        {
                if (trigger.m_TriggerCfg.is_repeat == 0)
                {
                        if (m_RoundTriggerList.Find((x) => { return x.m_TriggerId == trigger.m_TriggerId; }) != null)
                        {
                                return false;
                        }
                }
                return true;
        }

        public void AddTriggerSkill(FighterTrigger triggerBehav)
        {
                m_RoundTriggerList.Add(triggerBehav);
        }

        public void AddExtraSkill(FighterBehav behav, SkillConfig extraSkill, FighterBehav mainTarget)
        {
                m_ExtraSkillList.Add(new ActionSkillData(extraSkill, behav, mainTarget));
        }

        public void FighterDie(FighterBehav behav)
        {
                //                 if (behav != null)
                //                 {
                //                         if (behav.IsEnemy)
                //                         {
                //                                 m_EnemyFighterList.Remove(behav);
                //                         }
                //                         else
                //                         {
                //                                 m_MyFighterList.Remove(behav);
                //                         }
                //                 }
        }
        public void CombatResult(bool bWin)
        {
                if (bWin)
                {
                        RoundInfo_Log("战斗胜利 总回合数=" + m_nRound);
                }
                else
                {
                        RoundInfo_Log("战斗失败 总回合数" + m_nRound);
                }
                SetCombatState(COMBAT_STATE.END);
                m_fUpdateTime = Time.realtimeSinceStartup;
        }
        public void CombatEnd()
        {
                InputManager.GetInst().onMultiTouchMove = null;

                SetCombatState(COMBAT_STATE.NONE);
                DestroyAllFighters();

                GameObject.Destroy(m_BattleScene);
                m_BattleScene = null;
                GameObject.Destroy(m_SelectCircle);
                m_SelectCircle = null;

                UIManager.GetInst().CloseUI("UI_CombatSkill");
                UIManager.GetInst().CloseUI("UI_CombatTimeline");
                UIManager.GetInst().CloseUI("UI_BossStatus");
                m_UISCombatSkill = null;

                foreach (GameObject obj in m_ExistedObjList.Keys)
                {
                        if (obj != null)
                        {
                                obj.SetActive(true);
                                //node.StartPlay();
                        }
                }
                m_ExistedObjList.Clear();

                SendBattleOver();

                if (m_CombatStage == COMBAT_STAGE.RAID)
                {
                        RaidManager.GetInst().ExitCombat();
                }
                else if (m_CombatStage == COMBAT_STAGE.VILLAGE)
                {
                        VillageManager.GetInst().ExitCombat();
                }

#if UNITY_STANDALONE
                string log = "";
                foreach (var param in m_LocalRoundDict)
                {
                        log += "\nROUND_" + param.Key.ToString() + "\n";

                        log += "随机数汇总：\n";
                        foreach (int randomVal in param.Value.randomList)
                        {
                                log += randomVal + "\n";
                        }
                        log += "\n";

                        foreach (string text in param.Value.loglist)
                        {
                                log += text + "\n";
                        }
                }
                LocalFileIO.WriteDataWithFile("Battle_" + m_BattleID + ".txt", log);
#endif
        }

        void CalcTriggerAndExtraSkill()
        {
                if (m_RoundTriggerList.Count > 0)
                {
                        ActionSkillData asd = m_RoundTriggerList[0].CalcSkillTarget();
                        if (asd != null)
                        {
                                m_ActionFighter = asd.caster;
                                asd.caster.GoTriggerSkill(asd.skillCfg, asd.mainTarget);
                                SetCombatState(COMBAT_STATE.ROUND_PLAY);
                        }
                        m_RoundTriggerList.RemoveAt(0);
                }
                else if (m_ExtraSkillList.Count > 0)
                {
                        CheckExtraSkill(m_ExtraSkillList[0]);
                        m_ExtraSkillList.RemoveAt(0);
                }
                else
                {
                        if (m_bCheckAfterAction == false)
                        {
                                m_bCheckAfterAction = true;

                                if (m_RoundFighter != null && !m_RoundFighter.IsDead())
                                {
                                        CombatManager.GetInst().CheckAllTriggers(m_RoundFighter, FIGHTER_TRIGGER_TYPE.AFTER_ACTION);
                                        m_RoundFighter.CheckBuffAfterAction();
                                        m_RoundFighter.UpdateSkillCD();
                                        if (m_ActionSkill != null)
                                        {
                                                m_RoundFighter.ResetSkillCD(m_ActionSkill.id);
                                        }
                                        return;
                                }
                        }
                        SetCombatState(COMBAT_STATE.ROUND_END);
                }
        }

        void CheckExtraSkill(ActionSkillData extraSkillData)
        {
                if (extraSkillData.caster.IsDead())
                {
                        return;
                }

                if (DamageCalc.CheckRandom(m_ActionSkill.GetLevelValue(m_ActionSkill.add_skill_rate, m_ActionFighter.FighterProp), "追加技能概率"))
                {
                        FighterBehav caster = extraSkillData.caster;
                        SkillConfig skillCfg = extraSkillData.skillCfg;
                        FighterBehav mainTarget = null;
                        {
                                switch (skillCfg.add_skill_range_type)
                                {
                                        case 0:
                                                {
                                                        List<FighterBehav> list = caster.CalcSkillTargetGroup(skillCfg);
                                                        mainTarget = GetRandomTarget(list, "追加技能0类型选取目标");
                                                }
                                                break;
                                        default:
                                        case 1:
                                        case 2:
                                                {
                                                        mainTarget = extraSkillData.mainTarget;
                                                }
                                                break;
                                        case 3:
                                        case 4:
                                                {
                                                        List<FighterBehav> list = caster.CalcSkillTargetGroup(skillCfg);
                                                        mainTarget = GetRandomTarget(list, "追加技能3,4类型选取目标");
                                                }
                                                break;
                                }
                        }
                        if (caster != null && mainTarget != null)
                        {
                                //判断技能targetgroup合理性
                                if (skillCfg.IsAffectEnemy())
                                {
                                        if (caster.IsEnemy != mainTarget.IsEnemy)
                                        {
                                                m_ActionFighter = caster;
                                                caster.GoExtraSkill(skillCfg, mainTarget);
                                                SetCombatState(COMBAT_STATE.ROUND_PLAY);
                                        }
                                }
                                else
                                {
                                        if (caster.IsEnemy == mainTarget.IsEnemy)
                                        {
                                                m_ActionFighter = caster;
                                                caster.GoExtraSkill(skillCfg, mainTarget);
                                                SetCombatState(COMBAT_STATE.ROUND_PLAY);
                                        }
                                }
                        }
                }
                else
                {
                        if (extraSkillData.caster != null)
                        {
                                m_ActionFighter = extraSkillData.caster;
                                m_ActionFighter.GoMoveBack();
                                SetCombatState(COMBAT_STATE.ROUND_PLAY);
                        }
                }
        }

        public void CheckAllTriggers(FighterBehav triggerTarget, FIGHTER_TRIGGER_TYPE action, FighterBehav damageTarget = null)
        {
                RoundInfo_Log("触发行为=" + action.ToString() + "触发对象=" + triggerTarget.FighterId);
                //先算谁再算谁，此处待议
                foreach (FighterBehav behav in GetFighterList(!triggerTarget.IsEnemy))
                {
                        if (behav.FighterProp.Hp > 0)
                        {
                                behav.TryTriggerList(triggerTarget, action, damageTarget);
                        }
                }
                foreach (FighterBehav behav in GetFighterList(triggerTarget.IsEnemy))
                {
                        if (behav.FighterProp.Hp > 0)
                        {
                                behav.TryTriggerList(triggerTarget, action, damageTarget);
                        }
                }
        }

        bool CheckRoundTriggerBefore()
        {
                if (m_RoundTriggerList.Count > 0)
                {
                        ActionSkillData asd = m_RoundTriggerList[0].CalcSkillTarget();
                        if (asd != null)
                        {
                                m_ActionFighter = asd.caster;
                                asd.caster.GoTriggerSkill(asd.skillCfg, asd.mainTarget);
                                SetCombatState(COMBAT_STATE.ROUND_PLAY);
                        }
                        m_RoundTriggerList.RemoveAt(0);
                        return false;
                }

                return true;
        }

        void DoResumeRound()
        {
                if (m_ResumeInfoList.ContainsKey(m_nRound))
                {
                        ResumeInfo ri = m_ResumeInfoList[m_nRound];

                        m_ActionSkill = SkillManager.GetInst().GetActiveSkill(ri.idSkill);
                        if (m_ActionSkill == null)
                        {
                                Debug.LogError(ri.idSkill);
                        }
                        if (m_ActionFighter != null && m_ActionSkill != null)
                        {
                                m_ActionTarget = GetFighter(ri.idTarget);
                                m_ActionFighter.GoActiveSkill(m_ActionSkill, m_ActionTarget);
                                SetCombatState(COMBAT_STATE.ROUND_PLAY);
                        }
                        else
                        {
                                SetCombatState(COMBAT_STATE.ROUND_AFTER_SKILL);
                        }
                }
                else
                {
                        SetCombatState(COMBAT_STATE.ROUND_AFTER_SKILL);
                }
        }

        FighterBehav m_InputBehav = null;
        void CheckInput()
        {
                if (InputManager.GetInst().GetInputDown(false))
                {
                        RaycastHit hit = new RaycastHit();
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        int mask = 1 << LayerMask.NameToLayer("Character");

                        if (Physics.Raycast(ray, out hit, Camera.main.farClipPlane, mask))
                        {
                                FighterBehav behav = hit.collider.GetComponent<FighterBehav>();
                                if (behav != null && behav.CanSelect())
                                {
                                        m_InputBehav = behav;
                                        m_InputBehav.SetSelectCircle(true);
                                }
                        }
                }

                if (InputManager.GetInst().GetInputUp(true))
                {
                        if (m_InputBehav != null)
                        {
                                m_InputBehav.SetSelectCircle(false);
                        }

                        RaycastHit hit = new RaycastHit();
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                        int mask = 1 << LayerMask.NameToLayer("Character") /*& 1 << LayerMask.NameToLayer("NonBlockObj")*/;

                        if (Physics.Raycast(ray, out hit, Camera.main.farClipPlane, mask))
                        {
                                FighterBehav behav = hit.collider.GetComponent<FighterBehav>();
                                if (behav != null && behav.CanSelect())
                                {
                                        if (m_InputBehav == behav)
                                        {
                                                ConfirmInput(behav);
                                        }
                                }
                        }

                        m_InputBehav = null;
                }
        }

        enum PRESSURE_STATE
        {
                NORMAL,
                FEAR,
                SELFISH,
        };
        PRESSURE_STATE m_PressureState = PRESSURE_STATE.NORMAL;  //0正常，1恐惧，2自私
        void CheckFighterPressure()
        {
                m_PressureState = PRESSURE_STATE.NORMAL;
                if (m_ActionFighter.FighterProp.IsPressureOverLimit())
                {
                        int val = DamageCalc.GetRandomVal(1, 100, "战斗中压力状态判定");
                        if (val <= GlobalParams.GetInt("pressure_result_fear_per"))
                        {
                                m_PressureState = PRESSURE_STATE.FEAR;
                        }
                        else if (val <= GlobalParams.GetInt("pressure_result_fear_per") + GlobalParams.GetInt("pressure_result_selfish_per"))
                        {
                                m_PressureState = PRESSURE_STATE.SELFISH;
                        }
                }
                RoundInfo_Log("压力状态检测结果=" + m_PressureState);
        }

        public void Update()
        {

                if (m_CombatState != COMBAT_STATE.NONE)
                {
                        InputManager.GetInst().UpdateGlobalInput();
#if UNITY_STANDALONE
                        if (Input.GetKey(KeyCode.LeftArrow))
                        {
                                Camera.main.transform.RotateAround(CenterPoint, Vector3.up, -1f);
                        }
                        else if (Input.GetKey(KeyCode.RightArrow))
                        {
                                Camera.main.transform.RotateAround(CenterPoint, Vector3.up, 1f);
                        }
#endif
                }

                switch (m_CombatState)
                {
                        case COMBAT_STATE.LOAD_RES:
                                {
                                }
                                break;

                        case COMBAT_STATE.IN_POSITION:
                                {
                                        if (m_nHeroInPosCount <= 0)
                                        {
                                                EffectManager.GetInst().PlayEffect("effect_battle_start", Camera.main.transform, true);
                                                if (m_ResumeInfoList.Count > 0)
                                                {
                                                        CalcReplayInfo();
                                                }
                                                m_RoundTriggerList.Clear();
                                                SetCombatState(COMBAT_STATE.START_TRIGGER_SKILL);
                                        }
                                }
                                break;
                        case COMBAT_STATE.START_TRIGGER_SKILL:
                                {
                                        if (Time.realtimeSinceStartup - m_fUpdateTime > 1f)
                                        {
                                                m_AllFighters.Sort(FighterBehav.CompareSpeed);

                                                SetCombatState(COMBAT_STATE.START);

                                                //计算开场触发器
                                                m_bInCombatStart = true;
                                                foreach (FighterBehav behav in m_AllFighters)
                                                {
                                                        if (behav.FighterProp.Hp > 0)
                                                        {
                                                                behav.TryTriggerList(behav, FIGHTER_TRIGGER_TYPE.COMBAT_START);
                                                        }
                                                }
                                        }
                                }
                                break;
                        case COMBAT_STATE.START:
                                {
                                        if (CheckRoundTriggerBefore())
                                        {
                                                m_bInCombatStart = false;
                                                SetCombatState(COMBAT_STATE.ROUND_CALC);
                                        }
                                }
                                break;

                        case COMBAT_STATE.ROUND_CALC:
                                {
                                        CheckRoundAction();
                                }
                                break;
                        case COMBAT_STATE.ROUND_BEFORE_SKILL:
                                {
                                        if (CheckRoundTriggerBefore())
                                        {
                                                if (m_RoundFighter != null)
                                                {
                                                        m_ActionFighter = m_RoundFighter;
                                                        m_bRoundFighterPlayed = true;
                                                        if (m_ActionFighter.CanAction() == false)
                                                        {
                                                                SetCombatState(COMBAT_STATE.ROUND_AFTER_SKILL);
                                                        }
                                                        else if (m_ActionFighter.TauntTarget != null && m_ActionFighter.TauntTarget.FighterProp.Hp > 0)
                                                        {
                                                                DoAttackTauntTarget();
                                                        }
                                                        else
                                                        {
                                                                m_ActionFighter.TauntTarget = null;
                                                                if (m_ActionFighter.IsEnemy)
                                                                {
                                                                        AIAction();
                                                                }
                                                                else
                                                                {
                                                                        CheckFighterPressure();
                                                                        if (m_PressureState == PRESSURE_STATE.SELFISH)  //压力过大导致自私
                                                                        {
                                                                                DoSelfishAction();
                                                                        }
                                                                        else
                                                                        {
                                                                                if (IsResume())
                                                                                {
                                                                                        if (m_PressureState == PRESSURE_STATE.FEAR)
                                                                                        {
                                                                                                SetCombatState(COMBAT_STATE.ROUND_AFTER_SKILL);
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                                DoResumeRound();
                                                                                        }
                                                                                }
                                                                                else
                                                                                {
                                                                                        if (IsAuto)
                                                                                        {
                                                                                                AutoAction();
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                                m_UISCombatSkill.ShowSkillGroup(true);
                                                                                                m_UISCombatSkill.SetupSkillIcons(m_ActionFighter);
                                                                                                m_UISCombatSkill.SelectFirstAvailbleSkill();
                                                                                                SetCombatState(COMBAT_STATE.WAIT_INPUT);
                                                                                        }
                                                                                }
                                                                        }
                                                                }
                                                        }
                                                }
                                        }
                                }
                                break;
                        case COMBAT_STATE.WAIT_INPUT:
                                {
                                        CheckInput();
                                }
                                break;
                        case COMBAT_STATE.ROUND_PLAY:
                                {
                                        if (m_ActionFighter.ActionState == FIGHTER_ACTION.END || m_ActionFighter.ActionState == FIGHTER_ACTION.DIE)
                                        {
                                                m_ActionFighter.ActionState = FIGHTER_ACTION.NONE;
                                                if (m_bInCombatStart)
                                                {
                                                        SetCombatState(COMBAT_STATE.START);
                                                }
                                                else
                                                {
                                                        if (m_bRoundFighterPlayed)
                                                        {
                                                                SetCombatState(COMBAT_STATE.ROUND_AFTER_SKILL);
                                                        }
                                                        else
                                                        {
                                                                SetCombatState(COMBAT_STATE.ROUND_BEFORE_SKILL);
                                                        }
                                                }
                                        }
                                }
                                break;
                        case COMBAT_STATE.ROUND_AFTER_SKILL:
                                {
                                        CalcTriggerAndExtraSkill();
                                }
                                break;
                        case COMBAT_STATE.PRESSURE_TORTURE:
                                {
                                        if (Time.realtimeSinceStartup - m_fUpdateTime > 2f)
                                        {
                                                SetCombatState(COMBAT_STATE.ROUND_END);
                                        }
                                }
                                break;
                        case COMBAT_STATE.ROUND_END:
                                {
                                        if (m_RoundTriggerList.Count <= 0)
                                        {
                                                foreach (FighterBehav fighter in m_MyFighterList)
                                                {
                                                        if (fighter.FighterProp.Hp > 0)
                                                        {
                                                                if (fighter.FighterProp.HasTortured == false)
                                                                {
                                                                        //TODO：拷问表现
                                                                        if (PressureManager.GetInst().PressureJudge_InBattle(fighter))
                                                                        {
                                                                                SetCombatState(COMBAT_STATE.PRESSURE_TORTURE);
                                                                                m_fUpdateTime = Time.realtimeSinceStartup;
                                                                                return;
                                                                        }
                                                                }
                                                        }
                                                        else
                                                        {
                                                                //检测下死亡的人，猝死的
                                                                fighter.UpdateHP_UI(true);
                                                        }
                                                }

                                                SetCombatState(COMBAT_STATE.ROUND_CALC);
                                                ClearSelectCircle();
                                                //CheckRoundInfo(m_nRound);
                                        }
                                }
                                break;
                        case COMBAT_STATE.END:
                                {
                                        if (Time.realtimeSinceStartup - m_fUpdateTime > 0.5f)
                                        {
                                                CombatEnd();
                                        }
                                }
                                break;
                }
        }

        public void SendBattleOver()
        {
                CSMsgBattleOver msg = new CSMsgBattleOver();
                msg.idBattle = m_BattleID;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

        #region BattleRoundInfo
        Dictionary<int, BattleRoundInfo> m_LocalRoundDict = new Dictionary<int, BattleRoundInfo>();
        Dictionary<int, BattleRoundInfo> m_ServerRoundDict = new Dictionary<int, BattleRoundInfo>();

        string[] COLOR_STRS = new string[]
        {
                "<color=#44ff44>",
                "<color=#8866ff>",
                "<color=#ffff66>",
                "<color=#ff2466>",
                "<color=#ff8866>",
                "<color=#54ff65>",
                "<color=#ff1111>",
        };
        public void RoundInfo_Log(string log, int colorIndex = -1)
        {
                if (GameStateManager.GetInst().GameState == GAMESTATE.RAID_COMBAT ||
                        GameStateManager.GetInst().GameState == GAMESTATE.RAID_PLAYING ||
                        GameStateManager.GetInst().GameState == GAMESTATE.RAID_CAMPING)
                {
                        if (!m_LocalRoundDict.ContainsKey(m_nRound))
                        {
                                m_LocalRoundDict.Add(m_nRound, new BattleRoundInfo());
                        }
                        m_LocalRoundDict[m_nRound].loglist.Add(log);
                        if (colorIndex >= 0 && colorIndex < COLOR_STRS.Length)
                        {
                                Debug.Log(COLOR_STRS[colorIndex] + log + "</color>");
                        }
                        else
                        {
                                Debug.Log(log);
                        }
                }
        }


        public void RoundInfo_AddSkillInfo(FighterBehav actionFighter, FighterBehav targetFighter, SkillConfig skillCfg, SkillResultData srd)
        {
                if (!m_LocalRoundDict.ContainsKey(m_nRound))
                {
                        m_LocalRoundDict.Add(m_nRound, new BattleRoundInfo());
                }

                m_LocalRoundDict[m_nRound].skillList.Add(new RoundSkillInfo(actionFighter.FighterId, targetFighter.FighterId, skillCfg.id, srd));
        }

        public void RoundInfo_SetNextRandom(int next)
        {
                if (!m_LocalRoundDict.ContainsKey(m_nRound))
                {
                        m_LocalRoundDict.Add(m_nRound, new BattleRoundInfo());
                }
                m_LocalRoundDict[m_nRound].randomList.Add(next);
        }
        void CheckRoundInfo(int round)
        {
                if (m_LocalRoundDict.ContainsKey(round) && m_ServerRoundDict.ContainsKey(round))
                {
                        BattleRoundInfo local = m_LocalRoundDict[round];
                        BattleRoundInfo server = m_ServerRoundDict[round];

                        if (local.randomList.Count != server.randomList.Count)
                        {
                                Debug.LogError("CheckRoundInfo round=" + round + " localRandomCount=" + local.randomList.Count + " serverRandomCount=" + server.randomList.Count);
                                singleton.GetInst().ShowMessage("战斗" + m_BattleID + "出错：第" + round + "回合" + " 随机数个数不一致");
                                return;
                        }
                        else
                        {
                                for (int i = 0; i < local.randomList.Count; i++)
                                {
                                        if (local.randomList[i] != server.randomList[i])
                                        {
                                                singleton.GetInst().ShowMessage("战斗" + m_BattleID + "出错：第" + round + "回合" + " 第" + i + "个随机数不一致");
                                                Debug.LogError("CheckRoundInfo round=" + round + " random[" + i + "] local=" + local.randomList[i] + " server=" + server.randomList[i]);
                                                return;
                                        }
                                }
                        }

                        Debug.LogWarning("CheckRoundInfo round=" + round + " success");
                }
                else
                {
                        if (!m_LocalRoundDict.ContainsKey(round))
                        {
                                Debuger.LogError("CheckRoundInfo local " + round + " is not exist");
                        }
                        else if (!m_LocalRoundDict.ContainsKey(round))
                        {
                                Debuger.LogError("CheckRoundInfo server " + round + " is not exist");
                        }
                }
        }
        void OnBattleRoundInfo(object sender, SCNetMsgEventArgs e)
        {
                SCMsgBattleRoundInfo msg = e.mNetMsg as SCMsgBattleRoundInfo;
                if (!m_ServerRoundDict.ContainsKey(msg.nRound))
                {
                        m_ServerRoundDict.Add(msg.nRound, new BattleRoundInfo(msg));
                }

                if (msg.nRound < m_nRound)
                {
                        CheckRoundInfo(msg.nRound);
                }
        }

        #endregion
        public void TeammateTalk(FighterBehav targetFighter, TALK_TYPE type)
        {

                List<FighterBehav> list = new List<FighterBehav>();
                foreach (FighterBehav fighter in m_AllFighters)
                {
                        if (fighter == targetFighter)
                                continue;
                        if (fighter.IsDead())
                                continue;

                        if (fighter.IsEnemy == targetFighter.IsEnemy)
                        {
                                list.Add(fighter);
                        }
                }
                if (list.Count > 0)
                {
                        int idx = UnityEngine.Random.Range(0, list.Count);
                        list[idx].Talk(type);
                }
        }


        bool bTouchRotation = false;
        float oldDistance;
        Vector2 oldVec;
        void OnMultiTouchMove()
        {
                Touch finger0 = Input.GetTouch(0);
                Touch finger1 = Input.GetTouch(1);
                if (finger0.phase == TouchPhase.Began || finger1.phase == TouchPhase.Began)
                {
                        oldDistance = Vector2.Distance(finger0.position, finger1.position);
                        oldVec = finger1.position - finger0.position;
                        bTouchRotation = false;
                }

                if (finger0.phase == TouchPhase.Moved || finger1.phase == TouchPhase.Moved)
                {
                        Vector2 curVec = finger1.position - finger0.position;
                        float angle = Vector2.Angle(oldVec, curVec);
                        angle *= Mathf.Sign(Vector3.Cross(oldVec, curVec).z);
                        if (Mathf.Abs(angle) > 5)
                        {
                                bTouchRotation = true;
                        }
                        if (bTouchRotation)
                        {
                                Camera.main.transform.RotateAround(CenterPoint, Vector3.up, angle * 0.5f);
                                oldVec = curVec;
                        }
                }
        }
}

public class ActionSkillData
{
        public FighterBehav caster;
        public SkillConfig skillCfg;
        public FighterBehav mainTarget;
        public ActionSkillData(SkillConfig _skill, FighterBehav _caster, FighterBehav _target)
        {
                caster = _caster;
                skillCfg = _skill;
                mainTarget = _target;
        }
}
public class RoundSkillInfo
{
        public long idFighter;
        public long idTarget;
        public int idSkill;
        public int state;
        public int hp;

        public RoundSkillInfo(long _idFighter, long _idTarget, int _idSkill, SkillResultData srd)
        {
                idFighter = _idFighter;
                idTarget = _idTarget;
                idSkill = _idSkill;
                state = 0;
                if (srd.type == SkillResultType.Dodge)
                {
                        state |= (int)SKILL_STATE_TYPE.DODGE;
                }
                else
                {
                        if (srd.isCritical)
                        {
                                state |= (int)SKILL_STATE_TYPE.CRITICAL;
                        }
                        if (srd.isParry)
                        {
                                state |= (int)SKILL_STATE_TYPE.PARRY;
                        }
                        if (srd.isDelay)
                        {
                                state |= (int)SKILL_STATE_TYPE.DELAY;
                        }
                        if (srd.isDisperse)
                        {
                                state |= (int)SKILL_STATE_TYPE.DISPERSE;
                        }
                        if (srd.targetBuffList.Count > 0)
                        {
                                state |= (int)SKILL_STATE_TYPE.BUFF;
                        }
                }
        }

        public RoundSkillInfo(string info)
        {
                string[] tmps = info.Split('&');
                if (tmps.Length >= 5)
                {
                        idFighter = long.Parse(tmps[0]);
                        idTarget = long.Parse(tmps[1]);
                        idSkill = int.Parse(tmps[2]);
                        state = int.Parse(tmps[3]);
                        hp = int.Parse(tmps[4]);
                }
        }

}
public class BattleRoundInfo
{
        public List<RoundSkillInfo> skillList = new List<RoundSkillInfo>();
        public List<int> randomList = new List<int>();
        public List<string> loglist = new List<string>();
        public BattleRoundInfo()
        {

        }
        public BattleRoundInfo(SCMsgBattleRoundInfo msg)
        {
//                 string[] skillInfoMsg = msg.strInfo.Split('|');
//                 foreach (string info in skillInfoMsg)
//                 {
//                         if (string.IsNullOrEmpty(info))
//                         {
//                                 continue;
//                         }
// 
//                         RoundSkillInfo rsi = new RoundSkillInfo(info);
//                         if (rsi != null)
//                         {
//                                 skillList.Add(rsi);
//                         }
//                 }
                if (!string.IsNullOrEmpty(msg.strRandomList) && msg.strRandomList != "-1")
                {
                        string[] infos = msg.strRandomList.Split('|');
                        foreach (string info in infos)
                        {
                                if (string.IsNullOrEmpty(info))
                                        continue;

                                randomList.Add(int.Parse(info));
                        }
                }

//                 if (!string.IsNullOrEmpty(msg.strBuffAdd) && msg.strBuffAdd != "-1")
//                 {
//                         string[] infos = msg.strBuffAdd.Split('|');
//                         foreach (string info in infos)
//                         {
//                                 if (string.IsNullOrEmpty(info))
//                                         continue;
// 
//                                 addBuffList.Add(int.Parse(info));
//                         }
//                 }
//                 if (!string.IsNullOrEmpty(msg.strBuffDel) && msg.strBuffDel != "-1")
//                 {
//                         string[] infos = msg.strBuffDel.Split('|');
//                         foreach (string info in infos)
//                         {
//                                 if (string.IsNullOrEmpty(info))
//                                         continue;
// 
//                                 delBuffList.Add(int.Parse(info));
//                         }
//                 }
        }

}
