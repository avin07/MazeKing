using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding.RVO;
using UnityEngine.EventSystems;

public class HeroUnit : MonoBehaviour, IPointerClickHandler
{
        public bool IsLeave
        {
                get
                {
            return false;
                        //return hero.GetPropertyString("state") == "1";
                }
        }
        public delegate void ReachTargetHandler(object data);
        public Hero hero;
        public long ID;
        public int CharacterID;
        public int Index;       //战斗位置(0-5)
        public ReachTargetHandler m_ReachTargetHandler = null;
        public object m_HandlerData = null;
        public GameObject Mod;
        //         public FighterProperty FighterProp
        //         {
        //                 get
        //                 {
        //                         if (hero != null)
        //                         {
        //                                 return hero.FighterProp;
        //                         }
        //                         return null;
        //                 }
        //         }

        //public FighterBehav RelateFighter;

        public UnitPath PathComp
        {
                get
                {
                        return GetComponent<UnitPath>();
                }
        }
        public UnitAnim AnimComp
        {
                get
                {
                        return GetComponent<UnitAnim>();
                }
        }
        public Transform m_CurrentTarget = null;

        GameObject m_TargetSelectEffect;
        public GameObject TargetUnitSelectEffect
        {
                get
                {
                        if (m_TargetSelectEffect == null)
                        {
                                m_TargetSelectEffect = EffectManager.GetInst().GetEffectObj("effect_battle_good_counter_001");
                                m_TargetSelectEffect.transform.SetParent(this.transform);
                                m_TargetSelectEffect.transform.localPosition = Vector3.zero;
                                m_TargetSelectEffect.transform.localRotation = Quaternion.identity;
                        }
                        return m_TargetSelectEffect;
                }
        }

        GameObject m_ActionUnitSelectEffect;
        public GameObject ActionUnitSelectEffect
        {
                get
                {
                        if (m_ActionUnitSelectEffect == null)
                        {
                                m_ActionUnitSelectEffect = EffectManager.GetInst().GetEffectObj("effect_battle_teammate_circle_001");
                                m_ActionUnitSelectEffect.transform.SetParent(this.transform);
                                m_ActionUnitSelectEffect.transform.localPosition = Vector3.zero;
                                m_ActionUnitSelectEffect.transform.localRotation = Quaternion.identity;
                        }
                        return m_ActionUnitSelectEffect;
                }
        }
        public void DestroySelectEffect()
        {
                GameObject.Destroy(m_TargetSelectEffect);
                m_TargetSelectEffect = null;


                GameObject.Destroy(m_ActionUnitSelectEffect);
                m_ActionUnitSelectEffect = null;
        }

        public bool IsAlive
        {
                get
                {
            return true;
            //hero.Hp > 0;
                }
        }

        bool m_bCollecting = false;
        public bool IsCollecting
        {
                get
                {
                        return m_bCollecting;
                }
                set
                {
                        m_bCollecting = value;
                }
        }

        void Awake()
        {


                if (GetComponent<UnitPath>() == null)
                {
                        UnitPath up = this.gameObject.AddComponent<UnitPath>();
                        up.BelongUnit = this;
                        up.canMove = false;
                        up.speed = GlobalParams.GetFloat("run_speed");
                }
                if (GetComponent<CharacterController>() == null)
                {
                        CharacterController cc = this.gameObject.AddComponent<CharacterController>();
                        cc.radius = 0.2f;
                        cc.height = 1.6f;
                        cc.center = Vector3.up * 0.81f;
                }
                this.gameObject.AddComponent<Pathfinding.FunnelModifier>();

        }

        void Start()
        {
                CurrentPos = new Vector2((int)this.transform.position.x, (int)this.transform.position.z);
                LastPos = CurrentPos;
        }

        public void SetModel(GameObject mod)
        {
                Mod = mod;
                Mod.transform.SetParent(this.transform);
                Mod.transform.localPosition = Vector3.zero;
                Mod.transform.localRotation = Quaternion.identity;
                GameUtility.SetLayer(Mod, LayerMask.LayerToName(this.gameObject.layer));
                Mod.SetActive(this.gameObject.activeSelf);

                if (GetComponent<UnitAnim>() == null)
                {
                        this.gameObject.AddComponent<UnitAnim>();
                }

                AnimComp.Model_Id = CharacterManager.GetInst().GetCharacterModelId(CharacterID);
                AnimComp.PlayAnim("idle_001", true);
        }

        public void LoadModel()
        {
                SetModel(CharacterManager.GetInst().GenerateModel(CharacterID));
        }
        #region 血条

//         public void UpdatePressureValue()
//         {
//                 UI_RaidMain uis = UIManager.GetInst().GetUIBehaviour<UI_RaidMain>();
//                 if (uis != null)
//                 {
//                         uis.SetPressure(this.ID, hero.Pressure);
//                 }
//         }

        //UI_CombatDamage m_UIPressure;
//         public void ShowPressure(int offset)
//         {
//                 if (offset != 0)
//                 {
//                         StartCoroutine(ProcessShowPressure(offset));                        
//                 }
//         }

        //IEnumerator ProcessShowPressure(int offset)
        //{
        //        if (m_UIDamage != null)
        //        {
        //                yield return new WaitForSeconds(0.5f);
        //        }
        //        SkillResultData skillResult = new SkillResultData();
        //        skillResult.type = offset > 0 ? SkillResultType.AddPressure : SkillResultType.MinusPressure;
        //        skillResult.nValue = Mathf.Abs(offset);

        //        if (m_UIPressure != null)
        //        {
        //                GameObject.Destroy(m_UIPressure.gameObject);
        //        }
        //        m_UIPressure = GameUtility.AddDamageUI(this.transform, skillResult, 3f);

        //        GameObject effectObj = EffectManager.GetInst().GetEffectObj(offset > 0 ? "effect_raid_pressure_up_001" : "effect_raid_pressure_down_001");
        //        if (effectObj != null)
        //        {
        //                effectObj.transform.SetParent(this.transform);
        //                effectObj.transform.localPosition = Vector3.zero;
        //                effectObj.transform.localRotation = Quaternion.identity;
        //        }
        //}
//         //UI_CombatDamage m_UIDamage;
//         public void ShowDamage(SkillResultData skillResult)
//         {
//                 StartCoroutine(ProcessShowDamage(skillResult));
//         }
//         IEnumerator ProcessShowDamage(SkillResultData skillResult)
//         {
//                 if (m_UIPressure != null)
//                 {
//                         yield return new WaitForSeconds(0.5f);
//                 }
// 
//                 if (skillResult.nValue > 0)
//                 {
//                         if (m_UIDamage != null)
//                         {
//                                 GameObject.Destroy(m_UIDamage.gameObject);
//                         }
//                         m_UIDamage = GameUtility.AddDamageUI(this.transform, skillResult, 3f);
//                 }
//         }

        void CheckHP()
        {
        }

        UI_RaidElemLoading m_UILoading;
        public delegate void OnLoadedHandler(object[] data);
        object[] m_LoadingData;
        OnLoadedHandler m_LoadingHandler;
        public void StartLoading(float time, OnLoadedHandler handler, object[] data)
        {
                Debug.Log("StartLoading " + time);
                if (time > 0)
                {
                        m_LoadingHandler = handler;
                        m_LoadingData = data;
                        IsCollecting = true;

                        if (m_UILoading != null)
                        {
                                GameObject.Destroy(m_UILoading.gameObject);
                                m_UILoading = null;
                        }
                        GameObject obj = UIManager.GetInst().ShowUI_Multiple<UI_RaidElemLoading>("UI_RaidElemLoading");
                        obj.transform.SetParent(this.gameObject.transform);
                        obj.transform.localPosition = Vector3.up * 2f;
                        GameUtility.SetLayer(obj, "UI");
                        m_UILoading = obj.GetComponent<UI_RaidElemLoading>();

                        m_UILoading.SetLoading(time, this);
                        m_UILoading.m_BelongTransform = this.transform;
                        Debug.Log("SetLoading " + time);
                }
                else
                {
                        handler(data);
                }
        }
        public void StopLoading()
        {
                m_LoadingHandler = null;
                m_LoadingData = null;
                FinishLoading();
        }
        public void FinishLoading()
        {
                if (m_UILoading != null)
                {
                        GameObject.Destroy(m_UILoading.gameObject);
                }
                IsCollecting = false;
                if (m_LoadingHandler != null)
                {
                        m_LoadingHandler(m_LoadingData);
                        m_LoadingHandler = null;
                        m_LoadingData = null;
                }

        }

        GameObject m_UIObj;
        UI_UnitStatus m_UIStatus;
        public void ShowUIStatus(bool bShow, RaidElemConfig elemCfg)
        {
                if (bShow)
                {
                        if (m_UIObj == null)
                        {
                                m_UIObj = UIManager.GetInst().ShowUI_Multiple<UI_UnitStatus>("UI_UnitStatus");
                                m_UIObj.transform.SetParent(this.gameObject.transform);
                                //m_UIObj.AddComponent<UI_Billboard>();
                                GameUtility.SetLayer(m_UIObj, "UI");

                                m_UIStatus = m_UIObj.GetComponent<UI_UnitStatus>();
                                m_UIStatus.m_BelongTransform = this.transform;
                                m_UIStatus.SetEnemy(false);
                                //m_UIStatus.SetHp(hero.Hp, hero.MaxHp);
                                //m_UIStatus.SetPressure(hero.Pressure);

//                                 foreach (var param in hero.GetSkills())
//                                 {
//                                         if (param.Key == elemCfg.right_adventure_skill_1 || param.Key == elemCfg.right_adventure_skill_2)
//                                         {
//                                                 m_UIStatus.AddSkill(param.Key);
//                                         }
//                                 }
                        }
                        m_UIObj.transform.localPosition = Vector3.up * 2f;
                }
                else
                {
                        GameObject.Destroy(m_UIObj);
                }
        }
        #endregion
        #region 寻路

        const float WALKING_DIST = 3f;
        Vector3 m_TargetPosition;

        public Vector3 LastPathPoint1;
        public Vector3 LastPathPoint2;

        public void SetTargetDirection(Vector3 p1, Vector3 p2)
        {
                LastPathPoint1 = p1;
                LastPathPoint2 = p2;
        }

        public void GoTo(Vector3 targetPosition, bool bForceRun = false)
        {
                IsForceRunning = bForceRun;
                if (IsForceRunning)
                {
                        AnimComp.PlayAnim("run_001", true);
                }
                m_TargetPosition = targetPosition;
                PathComp.canMove = true;
                PathComp.SearchPath(targetPosition - (targetPosition - this.transform.position).normalized * 0.1f);
        }

        public void GoTo(Transform target)
        {
                m_CurrentTarget = target;
                GoTo(target.position);
        }
        public void StopSeek()
        {
                //GetComponent<Seeker>().enabled = false;
                PathComp.canMove = false;
        }

        public void OnTargetReached()
        {
                //Debuger.LogWarning("OnTargetReached");
                PathComp.canMove = false;
                IsForceRunning = false;
                if (m_CurrentTarget != null)
                {
                        GameUtility.RotateTowards(this.transform, m_CurrentTarget);
                }

                if (m_ReachTargetHandler != null)
                {
                        m_ReachTargetHandler(m_HandlerData);
                }
        }
        #endregion 寻路
        #region TeamFollow

        public HeroUnit FollowingUnit;
        Vector2 m_vCurrentPos;
        public Vector2 CurrentPos
        {
                get
                {
                        return m_vCurrentPos;
                }
                set
                {
                        m_vCurrentPos = value;
//                         if (RaidManager.GetInst().MainHero == this)
//                         {
//                                 RaidManager.GetInst().SetMiniMapHeroPosition();
//                         }
                }
        }
        Vector2 m_vLastPos;
        public Vector2 LastPos
        {
                get
                {
                        return m_vLastPos;
                }
                set
                {
                        m_vLastPos = value;
                }
        }

        public int CurrentNodeId
        {
                get
                {
                        return (int)transform.position.x * 100 + (int)transform.position.z;
                }
        }

        public bool IsForceRunning = false;

        bool m_bWalking = false;
        public bool IsWalking
        {
                get
                {
                        return m_bWalking;
                }
                set
                {
                        m_bWalking = value;

                        PathComp.speed = m_bWalking ? RaidManager.WALK_SPEED : RaidManager.RUN_SPEED;
                }
        }

        bool m_bFollowing = false;
        public bool IsFollowing
        {
                get
                {
                        return m_bFollowing;
                }
                set
                {
                        m_bFollowing = value;
                }
        }
        bool m_bFollowingGather = false;
        public bool IsFollowingGather
        {
                get
                {
                        return m_bFollowingGather;
                }
                set
                {
                        m_bFollowingGather = value;
                }
        }

        public void UpdateCurrentPos()
        {
                Vector2 tmp = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
                CurrentPos = tmp;
        }

        float m_fAcc = 0f;
        Vector3 m_vEndPos = Vector3.zero;
        void UpdateFollow()
        {
                Vector2 tmp = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));

                if (CurrentPos.x != tmp.x || CurrentPos.y != tmp.y)
                {
                        int nodeId = (int)(tmp.x * 100 + tmp.y);
                        bool bBlock = RaidManager.GetInst().IsNodeBlocked(nodeId);
                        if (!bBlock)
                        {
                                LastPos = CurrentPos;
                                CurrentPos = tmp;

                                if (FollowingUnit == null)
                                {
                                        RaidManager.GetInst().CheckHeroPos(this);
                                }
                        }
                }
//                 if (FollowingUnit != null && this.PathComp.canMove == false)
//                 {
//                         if (RaidManager.GetInst().MainHero.PathComp.canMove || FollowingUnit.IsFollowing)
//                         {
//                                 if (IsFollowing == false)
//                                 {
//                                         if (Vector3.Distance(FollowingUnit.transform.position, this.transform.position) >= 1f)
//                                         {
//                                                 if (m_fAcc <= 0)
//                                                 {
//                                                         m_fAcc = 0.5f - 0.1f * RaidTeamManager.GetInst().GetHeroIndex(this);
//                                                 }
//                                                 IsFollowing = true;
//                                         }
//                                 }
//                                 if (IsFollowing)
//                                 {
//                                         m_fAcc = Mathf.Clamp(m_fAcc + 0.005f, 0.2f, 0.6f);
//                                         transform.position = Vector3.Lerp(transform.position, FollowingUnit.transform.position, Time.deltaTime * PathComp.speed * m_fAcc);
//                                         GameUtility.RotateTowards(transform, FollowingUnit.transform);
//                                 }
//                         }
//                         else
//                         {
//                                 if (IsFollowing)
//                                 {
//                                         Vector3 endPOs = FollowingUnit.transform.position;
//                                         //if (m_bFollowingGather == false)
//                                         {
//                                                 endPOs -= FollowingUnit.transform.forward * 1.1f;
//                                         }
// 
//                                         if (Vector3.Distance(transform.position, endPOs) >= 0.1f)
//                                         {
//                                                 transform.position = Vector3.Lerp(transform.position, endPOs, Time.deltaTime * PathComp.speed * 1.5f);
//                                                 GameUtility.RotateTowards(transform, FollowingUnit.transform);
//                                         }
//                                         else
//                                         {
//                                                 m_fAcc = 0f;
//                                                 IsFollowing = false;
//                                         }
//                                 }
//                         }
//                 }
        }

        #endregion
        GameObject m_TalkObj = null;
        public void OnPointerClick(PointerEventData data)
        {
                if (m_TalkObj == null)
                {
                        //UnitTalk(LanguageManager.GetText("raid_hero_speak_" + UnityEngine.Random.Range(1, 6)));
                        RaidManager.GetInst().SwitchCaptain(RaidTeamManager.GetInst().GetHeroIndex(this));
                }
        }

        public void Update()
        {
                if (this.Mod == null)
                        return;

                if (GameStateManager.GetInst().GameState == GAMESTATE.RAID_PLAYING)
                {
                        UpdateFollow();
                        if (IsCollecting == false && IsForceRunning == false)
                        {
                                if (PathComp.canMove)
                                {
                                        IsWalking = Vector3.Distance(m_TargetPosition, transform.position) <= RaidManager.WALK_DISTANCE;
                                        AnimComp.PlayAnim(IsWalking ? "walk_001" : "run_001", true);
                                }
                                else
                                {
                                        AnimComp.PlayAnim("idle_001", true);
                                }
                        }
                }
        }
                
        public void UnitTalk(string text, string buffname = "", float time = 2f)
        {
                if (m_TalkObj == null)
                {
                        m_TalkObj = UIManager.GetInst().ShowUI_Multiple<UI_UnitTalk>("UI_UnitTalk");
                        m_TalkObj.transform.SetParent(this.gameObject.transform);
                        m_TalkObj.transform.localPosition = Vector3.up * 2f;
                        UI_UnitTalk uis = m_TalkObj.GetComponent<UI_UnitTalk>();
                        uis.SetText(buffname, text);
                        m_TalkObj.AddComponent<UIBillboard>();
                        GameUtility.SetLayer(m_TalkObj, "Default");
                        GameObject.Destroy(m_TalkObj, time);
                }
        }
}