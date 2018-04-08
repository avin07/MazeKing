using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using Message;
using DG.Tweening;
using System;
using HighlightingSystem;


public enum BuildOperateState
{
        CanMove,
        None,
}

public class BuildBaseBehaviour : MonoBehaviour  //建筑基类
{
        #region 数据

        void Awake()
        {
                GameUtility.SetLayer(gameObject, "BlockObj");
        }   

        protected bool Operational = true; //是否可操作
     
        public BuildInfo mBuildInfo;

        public bool IsForRecruit()
        {
                if (mBuildInfo.buildCfg != null)
                {
                        string[] tmps = mBuildInfo.buildCfg.minor_function.Split(';');
                        foreach (string tmp in tmps)
                        {
                                if (!string.IsNullOrEmpty(tmp))
                                {
                                        string[] infos = tmp.Split(',');
                                        if (infos.Length == 2)
                                        {
                                                int minorFunctionType = int.Parse(infos[0]);
                                                if (minorFunctionType == 5)
                                                {
                                                        return true;
                                                }
                                        }
                                }
                        }
                }
                return false;
        }

        protected BuildOperateState mOperateState = BuildOperateState.None; 

        public EBuildType GetBuildType()
        {
                return mBuildInfo.type;
        }

        public BuildOperateState GetBuildState()
        {
                return mOperateState;
        }

        public void SetNoOperational() //设置不需要操作
        {
                Operational = false;
        }

        public void SetBuildOperateState(BuildOperateState state)
        {
                mOperateState = state;
                if (mOperateState == BuildOperateState.CanMove)
                {
                        if (mBuildInfo.type == EBuildType.eWall)
                        {
                                GetWallModel(true);
                                HomeManager.GetInst().UpdateWallLinkData((int)transform.position.x, (int)transform.position.z, 0);
                        }

                        StarHighlighter();
                }

                if (mOperateState == BuildOperateState.None)
                {
                        EndHighlighter();
                        UIManager.GetInst().CloseUI("UI_BuildingFunction");
                        HomeManager.GetInst().SetState(HomeState.None);
                        if (mBuildInfo.type == EBuildType.eWall)
                        {
                                GetWallModel(false);
                                HomeManager.GetInst().UpdateWallLinkData((int)transform.position.x, (int)transform.position.z, 1);
                        }
                        if (mPlane != null)
                        {
                                mPlane.SetActive(false);
                        }
                }
        }

        void Update()
        {
                if (Time.realtimeSinceStartup - BaseTime < 1.0f)
                {
                        return;
                }
                else
                {
                        BaseTime = Time.realtimeSinceStartup;
                        UpdateStateBar();
                }
        }

        protected virtual void UpdateStateBar()
        {
                UpdateStateCountdown();
        }

        bool bFirstBuild = true;
        public virtual void UpdateBuildInfo(BuildInfo new_bi)
        {
                bool needShowMenu = true;
                if (bFirstBuild)
                {
                        mBuildInfo = new_bi;
                }
                else
                {
                        needShowMenu = CompareBuildInfo(new_bi);
                        mBuildInfo = new_bi;
                }

                SetPostionByServer();

                if (mBuildInfo.eState == EBuildState.eWork)
                {
                        if (restTime <= 0)
                        {
                                HideBuildBar();
                        }
                }

                if (mBuildInfo.eState == EBuildState.eUpgrage)
                {
                        if (mBuildInfo.level == 0)
                        {
                                totalTime = mBuildInfo.buildCfg.cost_time;                                
                        }
                        else  
                        {
                                BuildingInfoHold nextCfg = HomeManager.GetInst().GetBuildInfoCfg(mBuildInfo.buildCfg.id + 1);
                                if (nextCfg != null)
                                {
                                        totalTime = nextCfg.cost_time;            
                                }
                        }
                }
                if (bFirstBuild)
                {
                        bFirstBuild = false;
                }

                if (needShowMenu)
                {
                        if (HomeManager.GetInst().GetSelectBuild() == this)
                        {
                                ShowBuildMenu();
                        }
                }
                else
                {
                        //HomeManager.GetInst().SetSelectBuild(null);
                }

                if (restTime <= 0)
                {
                        if (mBuildInfo.level > 0 && !string.IsNullOrEmpty(mBuildTypeConfig.bubble_hint) && mBuildInfo.id > 0)
                        {
                                BuildStateBar.ShowBubbleText(LanguageManager.GetText(mBuildTypeConfig.bubble_hint));
                        }

                }

                GameUtility.ReScanPath();
        }

        public void SetPostionByServer()
        {
                int y = HomeManager.GetInst().GetMaxBuildHieght((int)mBuildInfo.pos.x, (int)mBuildInfo.pos.y, mBuildInfo.size_x, mBuildInfo.size_y);
                transform.position = new Vector3(mBuildInfo.posClient.x, y, mBuildInfo.posClient.y);
                transform.localEulerAngles = Vector3.zero;

                if (ModelRoot != null)
                {
                        ModelRoot.transform.localRotation = Quaternion.Euler(new Vector3(0f, 360f - mBuildInfo.face * 90f, 0f));
                }

                if (bc != null)
                {
                        bc.size = new Vector3(mBuildInfo.size_x, mBuildInfo.height, mBuildInfo.size_y);
                        bc.center = new Vector3(0, mBuildInfo.height * 0.5f, 0);

                        if (mPlane != null)
                        {
                                mPlane.transform.localScale = new Vector3(mBuildInfo.size_x / 3.0f, 1, mBuildInfo.size_y / 3.0f);
                        }
                }
        }

        protected virtual bool CompareBuildInfo(BuildInfo new_bi)  //对比建筑新旧信息并作出相应的修改
        {
                if (mBuildInfo.eState != EBuildState.eWork && new_bi.eState == EBuildState.eWork)  
                {
                        restTime = 0;
                }

                if (mBuildInfo.level < new_bi.level) //说明建筑升级了
                {
                        PlayBuildFinishEffect(transform, mBuildInfo.size_x, mBuildInfo.size_y);
                }

                return true;
        }
 
        #endregion

        #region 操作模块


        protected GameObject ModelRoot;
        protected EventTriggerListener mListener;
        protected BoxCollider bc;

        public GameObject mPlane;
        Material mPlaneMaterial;

        void CreatPlane()
        {
                mPlane = ModelResourceManager.GetInst().GenerateObject("Models/model_wait_033");
                mPlane.transform.SetParent(gameObject.transform);
                mPlane.transform.localPosition = Vector3.zero;
                mPlane.transform.localScale = new Vector3(mBuildInfo.size_x / 3.0f, 1, mBuildInfo.size_y / 3.0f);
                mPlaneMaterial = mPlane.transform.GetChild(0).GetComponent<Renderer>().material;    
                mPlane.SetActive(false);
        }

        public void UpdateHighlightColor()
        {
                if (canPutDown)
                {
                        SetHighlighterColor(Color.white);
                        mPlaneMaterial.color = Color.green;
                }
                else
                {
                        SetHighlighterColor(Color.red);
                        mPlaneMaterial.color = Color.red;
                }
        }

        public Vector2 oldHitPos;
        public bool canPutDown = true; //是否可以放下
        protected virtual void UpdateBuildingObjMove()  //统一使用射线检测//
        {
                RaycastHit hit = new RaycastHit();
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, Camera.main.farClipPlane, 1 << LayerMask.NameToLayer("Scene")))
                {
                        //转换后的客户端点击位置//
                        float x = (int)(hit.point.x - mBuildInfo.offset.x);
                        float z = (int)(hit.point.z - mBuildInfo.offset.y);
                        if (x != oldHitPos.x || z != oldHitPos.y)
                        {
                                x = Mathf.Clamp(x, 0, HomeManager.HomeSize - mBuildInfo.size_x);
                                z = Mathf.Clamp(z, 0, HomeManager.HomeSize - mBuildInfo.size_y);
                                int y = HomeManager.GetInst().GetMaxBuildHieght((int)x, (int)z, mBuildInfo.size_x,mBuildInfo.size_y);
                                Vector3 pos_client = new Vector3(x + mBuildInfo.offset.x, y, z + mBuildInfo.offset.y);
                                Vector2 pos = new Vector2(x, z);
                                if (transform.position != pos_client)
                                {
                                        transform.position = pos_client;
                                        canPutDown = HomeManager.GetInst().CheckCanPutDown(pos, mBuildInfo);
                                }
                                oldHitPos = pos;
                        }
                }
        }

        public virtual void OnBuildClick(GameObject go, PointerEventData data)
        {
                if (HomeManager.GetInst().GetState() == HomeState.None)
                {
                        if (mOperateState == BuildOperateState.None)
                        {
                                ShowBuildMenu();
                        }
                        else if (mOperateState == BuildOperateState.CanMove)
                        {
                                HomeManager.GetInst().SetSelectBuild(null);
                        }
                }
        }

        public void OnBuildDarg(GameObject go, PointerEventData data)
        {
                if (mOperateState != BuildOperateState.CanMove)
                {
                        return;
                }

                UpdateBuildingObjMove();
                UpdateHighlightColor();
        }

        public virtual void OnBeginBuildDrag(GameObject go, PointerEventData data)
        {
                if (mOperateState != BuildOperateState.CanMove)
                {
                        return;
                }
                HomeManager.GetInst().SetState(HomeState.Move);
                UIManager.GetInst().SetUIRender<UI_BuildingFunction>("UI_BuildingFunction", false);
                mPlane.SetActive(true);
        }

        public virtual void OnEndBuildDrag(GameObject go, PointerEventData data)
        {
                if (mOperateState != BuildOperateState.CanMove)
                {
                        return;
                }
                BuildPutDown();
                UIManager.GetInst().SetUIRender<UI_BuildingFunction>("UI_BuildingFunction", canPutDown);
                mPlane.SetActive(!canPutDown);
        }

        public void ShowBuildMenu()
        {
                if (HomeManager.GetInst().GetState() != HomeState.None)
                {
                        return;
                }

                string function = GetFunctionStr();
                if (HomeManager.GetInst().GetSelectBuild() != this)
                {
                        HomeManager.GetInst().SetSelectBuild(this);
                        SelectAnima();
                        SetBuildOperateState(BuildOperateState.CanMove);
                }

                if (function.Contains("2,"))
                {
                        int id = mBuildInfo.buildCfg.id + 1;
                        if (HomeManager.GetInst().GetBuildInfoCfg(id) == null)
                        {
                               function = function.Replace("2,", string.Empty);
                        }
                }

                if (GameUtility.IsStringValid(function))
                {
                        UIManager.GetInst().ShowUI<UI_BuildingFunction>("UI_BuildingFunction").RefreshMenu(function);
                }

        }

        protected readonly string cancelAndQuickStr = "11,10";
        protected virtual string GetFunctionStr()
        {
                string function = "";

                switch (mBuildInfo.eState)
                {
                        case EBuildState.eWork:
                                function = mBuildTypeConfig.function_bottom;
                                break;
                        case EBuildState.eClear:
                                function = mBuildTypeConfig.function_bottom;
                                break;
                        case EBuildState.eUpgrage:
                                function = cancelAndQuickStr;
                                break;
                }
                return function;
        }

        Sequence selectAnima;
        protected void SelectAnima()  //选中反馈
        {
                if (selectAnima != null)
                {
                        selectAnima.Kill(true);
                }
              
                selectAnima = transform.DOLocalJump(transform.localPosition, 0.3f, 1, 0.3f);
        }

        protected virtual void BuildPutDown()
        {
                if (canPutDown)
                {
                        if (mBuildInfo.posClient.x != transform.position.x || mBuildInfo.posClient.y != transform.position.z)
                        {
                                HomeManager.GetInst().SendBuildMove(mBuildInfo.id, new Vector2(transform.position.x - mBuildInfo.offset.x, transform.position.z - mBuildInfo.offset.y));
                                SetBuildOperateState(BuildOperateState.CanMove);
                        }

                }
                HomeManager.GetInst().SetState(HomeState.None);
        }

        public void BackToOldPos()
        {
                if (!canPutDown)
                {
                        float x = mBuildInfo.posClient.x;
                        float z = mBuildInfo.posClient.y;
                        int y = HomeManager.GetInst().GetHeightByPos((int)x, (int)z);
                        transform.position = new Vector3(x, y, z);
                        //Debug.Log("BackToOldPos " + transform.position);
                        HomeManager.GetInst().UpdateWallLinkData((int)x, (int)z, 1);
                }
                canPutDown = true;
        }

        protected virtual Highlighter GetHighlighter()
        {
                if (ModelRoot != null)
                {
                        Highlighter h = ModelRoot.GetComponent<Highlighter>();
                        if (h == null)
                        {
                                h = ModelRoot.AddComponent<Highlighter>();
                                h.SeeThroughOff();
                                h.OccluderOn();
                        }
                        return h;
                }
                return null;
        }

        public GameObject GetModelRoot()
        {
                return ModelRoot;
        }

        protected virtual void StarHighlighter()
        {
                HomeManager.GetInst().SetBrickOccluderOn();
                Highlighter h = GetHighlighter();
                if (h != null)
                {
                        h.ReinitMaterials();
                        h.ConstantOn(Color.white);
                }
        }


        protected virtual void SetHighlighterColor(Color color)
        {
                Highlighter h = GetHighlighter();
                if (h != null)
                {
                        h.ReinitMaterials();
                        h.ConstantOn(color);
                }
        }

        public void EndHighlighter()
        {
                Highlighter h = GetHighlighter();
                if (h != null)
                {
                        h.Off();
                }            
        }

        #endregion

        #region 建造模块

        protected BuildingTypeHold mBuildTypeConfig;

        public void SetBuildInfo(BuildInfo build_info)
        {
                mBuildTypeConfig = HomeManager.GetInst().GetBuildTypeCfg(build_info.buildId);
                UpdateBuildInfo(build_info);
                gameObject.name = build_info.id.ToString();
                if (Operational)
                {
                        int canMoveed = mBuildTypeConfig.can_be_moved;
                        mListener = gameObject.AddComponent<EventTriggerListener>();
                        if (canMoveed == 1)
                        {
                                mListener.onDrag = OnBuildDarg;
                                mListener.onBeginDrag = OnBeginBuildDrag;
                                mListener.onEndDrag = OnEndBuildDrag;
                                CreatPlane();
                        }
                        mListener.onClick = OnBuildClick;
                        bc = gameObject.AddComponent<BoxCollider>();
                        bc.size = new Vector3(build_info.size_x, build_info.height, build_info.size_y);
                        bc.center = new Vector3(0, build_info.height * 0.5f, 0);

                }
        }
        public virtual void SetBuild(BuildInfo build_info)
        {
                SetBuildInfo(build_info);
                GetModel(build_info);      
        }

        //GameObject m_WallMod;
        public void ResetWallModel()
        {
                GetWallModel(false);
        }
        List<Vector3> m_MeshList = new List<Vector3>();
        void GetWallModel(bool bMoving = false)
        {
                string[] modelIds = mBuildInfo.buildCfg.model.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < modelIds.Length; i++)
                {
                        int x = (int)this.transform.position.x;
                        int y = (int)this.transform.position.y + i + 1;
                        int z = (int)this.transform.position.z;

                        int modId = int.Parse(modelIds[i]);
                        if (bMoving)
                        {
                                if (i < m_MeshList.Count)
                                {
                                        HomeManager.GetInst().m_CubeManager.RemoveMod((int)m_MeshList[i].x, (int)m_MeshList[i].y, (int)m_MeshList[i].z);
                                }
                                if (ModelRoot == null)
                                {
                                        ModelRoot = new GameObject("ModelRoot");
                                        ModelRoot.transform.SetParent(transform);
                                        ModelRoot.transform.localPosition = Vector3.zero;                                        
                                }
                                GameObject model = ModelResourceManager.GetInst().GenerateCommonObject(modId);
                                if (model != null)
                                {
                                        model.transform.SetParent(ModelRoot.transform);
                                        model.transform.localPosition = Vector3.up * (i + 1);
                                }
                        }
                        else
                        {
                                if (ModelRoot != null)
                                {
                                        GameObject.Destroy(ModelRoot);
                                        ModelRoot = null;
                                }

                                ulong linkState = HomeManager.GetInst().GetWallLink(x, z);
                                if (i < modelIds.Length - 1)
                                {
                                        linkState |= 1 << 4;
                                }
                                HomeManager.GetInst().m_CubeManager.AddMod(modId, x, y, z, linkState);
                                m_MeshList.Add(new Vector3(x,y, z));
                        }
                }
                if (bMoving)
                {
                        m_MeshList.Clear();
                }
        }

        protected virtual void GetModel(BuildInfo build_info, bool bMoving = false)
        {
                if (build_info.buildCfg == null)
                        return;
                
                if (mBuildInfo.type == EBuildType.eWall)
                {
                        GetWallModel(bMoving);
                }
                else
                {
                        if (ModelRoot == null)
                        {
                                ModelRoot = new GameObject("ModelRoot");
                                ModelRoot.transform.SetParent(transform);
                                ModelRoot.transform.localPosition = Vector3.zero;
                                ModelRoot.transform.localRotation = Quaternion.Euler(new Vector3(0f, 360f - mBuildInfo.face * 90f, 0f));
                        }
                        string[] modelIds = build_info.buildCfg.model.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < modelIds.Length; i++)
                        {
                                GameObject model;
                                {
                                        model = ModelResourceManager.GetInst().GenerateObject(int.Parse(modelIds[i]));
                                        if (model != null)
                                        {
                                                model.transform.SetParent(ModelRoot.transform);
                                                model.transform.localPosition = Vector3.up * i;
                                                model.transform.localRotation = Quaternion.identity;
                                        }
                                }
                        }
                }
        }

        public void PlayBuildFinishEffect(Transform tf, int size_x, int size_y) //建筑完成特效
        {
                GameObject effect = EffectManager.GetInst().PlayEffect(GlobalParams.GetString("building_finish_effect"), tf);
                effect.transform.localScale = new Vector3(size_x * 0.5f, 1, size_y * 0.5f);
                effect.transform.position += new Vector3(0, 0.1f, 0);
        }

        #endregion

        #region 清除模块

        public void DeleteBuildingObj() //建造状态下点击ui就清除建造建造
        {
                PlayCleanEffect();
                DestroyBuildState();
        }

        public virtual void PlayCleanEffect()
        {
                NormalClean();
        }


        protected void NormalClean()
        {
                string effect_url = mBuildTypeConfig.clean_effect;
                int height = HomeManager.GetInst().GetHeightByPos(mBuildInfo.pos);
                EffectManager.GetInst().PlayEffect(effect_url, new Vector3(mBuildInfo.posClient.x, height, mBuildInfo.posClient.y));
                Destroy(gameObject);
                HomeManager.GetInst().RemoveBuildingBehaviour(mBuildInfo.id);
        }

        #endregion

        #region 状态

        protected UI_BuildState mBuildStateBar;  //用到的时候再去生成//
        public UI_BuildState BuildStateBar
        {
                get
                {
                        if (mBuildStateBar == null)
                        {
                                GameObject m_State = UIManager.GetInst().ShowUI_Multiple<UI_BuildState>("UI_BuildState");
                                m_State.transform.SetParent(transform);
                                if (mBuildInfo.buildCfg != null)
                                {
                                        m_State.transform.localPosition = new Vector3(0, 1.0f + mBuildInfo.height, 0);                                       
                                }
                                else
                                {
                                        m_State.transform.localPosition = new Vector3(0, 2.0f, 0);
                                }

                                mBuildStateBar = m_State.GetComponent<UI_BuildState>();
                        }
                        return mBuildStateBar;
                }
        }

        public void SetBuildingState(int rest_time, int max_time, string url = "")
        {
                BuildStateBar.ShowBar(rest_time, max_time, url);
        }

        protected void HideBuildBar()
        {
                if (mBuildStateBar != null)
                {
                        SetBuildingState(0, 0);
                }
        }
 
        void DestroyBuildState()
        {
                if (mBuildStateBar != null)
                {
                        GameObject.Destroy(mBuildStateBar.gameObject); 
                }
        }

        public void SetStateIsShow(bool isshow)
        {
                if (mBuildStateBar != null)
                {
                        mBuildStateBar.gameObject.SetActive(isshow);
                }
        }

        protected float restTime;  //剩余时间
        protected int totalTime;     //总时间
        protected float bufferTime = 1.0f;

        float BaseTime = Time.realtimeSinceStartup;
        void UpdateStateCountdown()
        {
                if (mBuildInfo.eState == EBuildState.eWork)
                {
                        return;
                }

                if (totalTime != 0)
                {
                        restTime = totalTime - mBuildInfo.passTime + mBuildInfo.clientTime - Time.realtimeSinceStartup + bufferTime;     //客户端延时一点//    
                        SetBuildingState((int)restTime, totalTime);
                        if (restTime <= bufferTime)
                        {
                                HomeManager.GetInst().SendBuildStateReq(mBuildInfo.id);
                                mBuildInfo.eState = EBuildState.eWork;
                                restTime = 0;
                        }
                }
                else
                {
                        HomeManager.GetInst().SendBuildStateReq(mBuildInfo.id);
                        mBuildInfo.eState = EBuildState.eWork;
                        restTime = 0;
                }
        }

        #endregion

        #region 操作

        //提取一些共有的基础操作

        public virtual void Cancel()
        {
                string text = "";
                if (mBuildInfo.eState == EBuildState.eUpgrage)  //取消升级
                {
                        if (mBuildInfo.level == 0) //取消建造
                        {
                                text = "您确定要停止建造" + LanguageManager.GetText(mBuildInfo.buildCfg.name) + "?" + "返还材料的" + GlobalParams.GetInt("building_back_per") + "%";
                        }
                        else                       //取消升级
                        {
                                text = "您确定要停止升级" + LanguageManager.GetText(mBuildInfo.buildCfg.name) + "?" + "停返还材料的" + GlobalParams.GetInt("building_back_per") + "%";
                        }
                        UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox").SetConfirmAndCancel("停止", text, ConfirmUpgradeBuildCancel, null, null);
                }
        }

        void ConfirmUpgradeBuildCancel(object data)
        {
                HomeManager.GetInst().SendUpgradeBuildCancel();
        }

        public virtual void Clean()
        {
                string need = mBuildInfo.buildCfg.clean_cost;
                string back = mBuildInfo.buildCfg.clean_restore;
                UIManager.GetInst().ShowUI<UI_NeedConfirm>("UI_NeedConfirm").SetConfirmAndCancel("清理", need, back,ConfirmClean, null, null);
        }

        void ConfirmClean(object data)
        {
                HomeManager.GetInst().SendCleanBuild();
        }

        public virtual void Info()
        {
                UIManager.GetInst().ShowUI<UI_BuildingInfo>("UI_BuildingInfo").Refresh(mBuildInfo);
        }

        public virtual void Quick()
        {
                string text = String.Empty;
                if (mBuildInfo.eState == EBuildState.eUpgrage)
                {
                        int cost = Mathf.CeilToInt(restTime / GlobalParams.GetInt("diamond_time_build"));
                        text = "您确定要花费" + cost + "钻石快速完成？";
                        UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox").SetConfirmAndCancel("快速建造", text, ConfirmQuick, null, null);
                }
        }

        void ConfirmQuick(object data)
        {
                int cost = Mathf.CeilToInt(restTime / GlobalParams.GetInt("diamond_time_build"));
                if (CommonDataManager.GetInst().GetNowResourceNum("diamond") >= cost)
                {
                        HomeManager.GetInst().SendUpgradeBuildQuick();
                }
                else
                {
                        GameUtility.PopupMessage("钻石不足！");
                }
        }



        #endregion
}

