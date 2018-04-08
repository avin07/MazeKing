//#define DEBUG_NO_WAR_FOG
#define USE_COMBINE_CHILDREN
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;
using Message;
using Pathfinding;
using HighlightingSystem;

public class RaidManager : SingletonObject<RaidManager>
{
    public static float RUN_SPEED;
    public static float WALK_SPEED;
    public static float WALK_DISTANCE;

    RaidNodeBehav _TargetNode = null;
    RaidNodeBehav m_TargetNode
    {
        get
        {
            return _TargetNode;
        }
        set
        {
            _TargetNode = value;
        }
    }
    GameObject m_ActionUnitSelectEffect;
    GameObject ActionUnitSelectEffect
    {
        get
        {
            if (m_ActionUnitSelectEffect == null)
            {
                m_ActionUnitSelectEffect = EffectManager.GetInst().GetEffectObj("effect_battle_teammate_circle_001");
            }
            return m_ActionUnitSelectEffect;
        }
    }

    HeroUnit m_FocusUnit = null;
    HeroUnit m_MainHero;
    public HeroUnit MainHero
    {
        get
        {
            return m_MainHero;
        }
        set
        {
            m_MainHero = value;

            if (m_MainHero != null)
            {
                SetupDissolveCollider();
                ActionUnitSelectEffect.transform.SetParent(m_MainHero.transform);
                ActionUnitSelectEffect.transform.localPosition = Vector3.up * 0.2f;
            }
        }
    }
    KeepRotZero m_DissolveCollider;
    void SetupDissolveCollider()
    {
        m_DissolveCollider = m_MainHero.GetComponentInChildren<KeepRotZero>();

        if (m_DissolveCollider == null)
        {
            GameObject obj = new GameObject("DissolveCollider");
            Rigidbody rb = obj.AddComponent<Rigidbody>();
            rb.freezeRotation = true;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezePosition;
            BoxCollider box = obj.AddComponent<BoxCollider>();
            box.size = new Vector3(3f, 3f, 3f);
            box.isTrigger = true;
            box.center = new Vector3(2f, 2.6f, -2f);
            m_DissolveCollider = obj.AddComponent<KeepRotZero>();
            m_DissolveCollider.OffsetX = 0f;
            m_DissolveCollider.OffsetY = 0f;
            m_DissolveCollider.TileX = 1f;
            m_DissolveCollider.TileY = 1f;
        }
        m_DissolveCollider.gameObject.transform.SetParent(m_MainHero.transform);
        m_DissolveCollider.gameObject.transform.localPosition = Vector3.zero;
        m_DissolveCollider.gameObject.transform.localRotation = Quaternion.Euler(0f, 225f, 0f);
    }

    GameObject m_RaidScene;
    GameObject m_ObjectEffect;
    void SetObjectEffect(RaidNodeBehav node)
    {
        if (m_ObjectEffect != null)
        {
            GameObject.Destroy(m_ObjectEffect);
            m_ObjectEffect = null;
        }
        if (node.elemObj)
        {
            m_ObjectEffect = EffectManager.GetInst().GetEffectObj("effect_choose_element");
            m_ObjectEffect.transform.position = Vector3.up * 0.1f + node.GetCenterPosition();
            float size = Mathf.Max(node.elemCfg.size.x, node.elemCfg.size.z);
            m_ObjectEffect.transform.localScale = new Vector3(size, 1f, size);
        }
    }

    GameObject m_ShadowLight;
    GameObject m_CharacterLight;


    GameObject m_FollowingLight;
    UI_RaidMain m_UIRaidMain;
    UI_RaidBag m_UIRaidBag;

    bool m_bLoadingRaid = false;

    public float m_fCameraDist = 15f;
    public Quaternion m_vCameraRotation;
    string m_sRaidMainTaskCount = "";

    int m_nCampTimes = 0;
    int m_EnterRoomIdx;              //入口节点id
    int m_nMapSize = 1;    //表示迷宫是几乘几的，固定迷宫为1，随机迷宫为服务端发过来的数
    public int MapSize
    {
        get
        {
            return m_nMapSize;
        }
    }
    int m_nPieceSize = 0;    //表示每个地块的尺寸
    public int PieceSize
    {
        get
        {
            return m_nPieceSize;
        }
    }

    int m_nWorldSize;
    Vector3 m_vWorldOffset;

    /// <summary>
    /// raid_map表的主键，格式是副本id*1000 + 层数
    /// </summary>
    int m_RaidID = 0;
    public int RaidID
    {
        get
        {
            return m_RaidID;
        }
    }
    public string TeamInfo = "";

    RaidRoomBehav m_FocusRoom = null;
    LINK_DIRECTION m_CurrentFromDirect = LINK_DIRECTION.MAX;//当前房间的来的方向

    Dictionary<int, ROOM_MSG_INFO> m_MapDict = new Dictionary<int, ROOM_MSG_INFO>();
    Dictionary<int, SCMsgRaidMapInitNodeInfo> m_InitNodeDict = new Dictionary<int, SCMsgRaidMapInitNodeInfo>();     //重连时服务端发过来的修改过的节点信息
    Dictionary<int, SCMsgRaidOptionNodeData> m_OptionNodeDataDict = new Dictionary<int, SCMsgRaidOptionNodeData>();     //重连时服务端发过来的修改过的节点信息
    Dictionary<int, RaidNodeBehav> m_NodeDict = new Dictionary<int, RaidNodeBehav>();

    class ROOM_MSG_INFO
    {
        public Dictionary<int, int> linkDict = new Dictionary<int, int>();
        public int pieceCfgId;
        public int flag;
        public void SetFlag(string[] tmps)
        {
            if (tmps[3] == "1")
            {
                flag |= GameUtility.GetFlagValInt((int)ROOM_FLAG.LOCKED);
            }
            if (tmps[4] == "1")
            {
                flag |= GameUtility.GetFlagValInt((int)ROOM_FLAG.DETECTED);
            }
            if (tmps[5] == "1")
            {
                flag |= GameUtility.GetFlagValInt((int)ROOM_FLAG.COMPLETED);
            }
            if (tmps[6] == "1")
            {
                flag |= GameUtility.GetFlagValInt((int)ROOM_FLAG.ACTIVE);
            }
            if (tmps[7] == "1")
            {
                flag |= GameUtility.GetFlagValInt((int)ROOM_FLAG.BROKEN);
            }
        }
    }

    Camera m_MainCamera;
    GameObject m_CameraTargetObj;
    Dictionary<int, RaidRoomBehav> m_RoomDict = new Dictionary<int, RaidRoomBehav>();
    public Dictionary<int, RaidRoomBehav> GetRoomDict()
    {
        return m_RoomDict;
    }
    BrickSceneManager m_BrickManager;

    SceneDayTime m_DayTimeBehav;

    public void Init()
    {
        RaidConfigManager.GetInst().Init();
        InitMsg();


        RUN_SPEED = GlobalParams.GetFloat("run_speed");
        WALK_SPEED = GlobalParams.GetFloat("walk_slowdown");
        WALK_DISTANCE = GlobalParams.GetFloat("walk_distance");
    }
    public void InitMsg()
    {
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidEnd), OnRaidNodeInfoEnd);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidMapInfo), OnRaidInfo);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidTriggerTrap), OnRaidTrap);

        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidNodeChange), OnNodeChange);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidOpenDoor), OnDoorOpen);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidUnlock), OnRoomUnlock);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidRoomLockList), OnRoomLockList);

        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidMapInitNodeInfo), OnInitNodeInfo);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidPerceptionNodeInfo), OnPerceptionNodeInfo);

        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidAward), OnRaidAward);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidMapLeave), OnRaidLeave);

        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidStoryTrigger), OnRaidStoryTrigger);

        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidCampInfo), OnCampContinue);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidNeedToolTip), OnNeedToolTip);

        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidCreateRoom), OnNewSingleRoom);

        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidOpenDoorFail), OnDoorOpenFail);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidRoomCompleted), OnRoomCompleted);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidRoomUnCompleted), OnRoomUnCompleted);

        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidSingleRoomDetect), OnRoomDetected);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidDetectTrigger), OnDetectTrigger);


        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidChooseReturn), OnChooseReturn);

        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidAlchemyResult), OnRaidAlchemyResult);

        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCRaidCampTimes), OnCampTimes);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidRescueHero), OnRaidRescueHero);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCRaidMapTeam), OnGetPetTeam);

        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidBehaviourTrigger), OnRaidBehaviourTrigger);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidLostDrop), OnLostDrop);

        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidOperatePasswdTips), null);

        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidOptionNodeData), OnOptionNodeData);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidFallTrap), OnRaidFallTrap);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidLastRoom), OnLastRoom);

        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidDoorBattle), OnRaidDoorBattle);

        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidShowClue), OnRaidShowClue);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidNewClue), OnRaidNewClue);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidBroken), OnRaidBroken);

        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidInputPasswordError), OnInputPasswordError);

        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidDiceResult), OnDiceResult);

        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidSpecialRoom), OnRaidSpecialRoom);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidTeleport), OnRaidTransport);

        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidDiscoverNewBuild), OnDiscoverNewBuild);

        RaidTeamManager.GetInst().Init();
    }

    public RaidNodeBehav GetRaidNodeBehav(int node_id)
    {
        if (m_NodeDict.ContainsKey(node_id))
        {
            return m_NodeDict[node_id];
        }
        return null;
    }
    public void AddNodeToDict(RaidNodeBehav node)
    {
        if (!m_NodeDict.ContainsKey(node.id))
        {
            m_NodeDict.Add(node.id, node);
        }
    }

    public SCMsgRaidMapInitNodeInfo GetInitNode(int nodeId)
    {
        if (m_InitNodeDict.ContainsKey(nodeId))
        {
            return m_InitNodeDict[nodeId];
        }
        return null;
    }
    public SCMsgRaidOptionNodeData GetOptionNodeData(int nodeId)
    {
        if (m_OptionNodeDataDict.ContainsKey(nodeId))
        {
            return m_OptionNodeDataDict[nodeId];
        }
        return null;
    }
    SCMsgRaidStoryTrigger m_StoryMsg;

    public int GetCurrentRaidLevel()
    {
        RaidInfoHold cfg = GetCurrentRaidInfo();
        if (cfg != null)
        {
            return cfg.raid_level;
        }
        return 0;
    }
    public RaidInfoHold GetCurrentRaidInfo()
    {
        RaidMapHold mapCfg = RaidConfigManager.GetInst().GetRaidMapCfg(m_RaidID);
        if (mapCfg != null)
        {
            return RaidConfigManager.GetInst().GetRaidInfoCfg(mapCfg.raid);
        }
        return null;
    }

    #region 副本初始化

    void SetupCamera()
    {
        GameUtility.ResetCameraAspect();

        m_MainCamera = Camera.main;
        Camera.main.cullingMask = ~(1 << LayerMask.NameToLayer("WorldUI"));
        PhysicsRaycaster pr = Camera.main.gameObject.AddComponent<PhysicsRaycaster>();
        pr.eventMask = LayerMask.GetMask(/*"BlockObj",*/ "NpcObj", "Scene", "Character");

        GameObject worldUIObj = new GameObject("WorldUI_Camera");
        worldUIObj.transform.SetParent(Camera.main.transform);
        worldUIObj.transform.localPosition = Vector3.zero;
        worldUIObj.transform.localRotation = Quaternion.identity;
        Camera worldUICam = worldUIObj.AddComponent<Camera>();
        worldUICam.CopyFrom(Camera.main);
        worldUICam.cullingMask = 1 << LayerMask.NameToLayer("WorldUI");
        worldUICam.clearFlags = CameraClearFlags.Depth;
        worldUICam.depth = Camera.main.depth + 1;

        QualitySettings.antiAliasing = 0;       //关掉引擎抗锯齿，会和Fow脚本冲突（待解决）
                                                //                 AntialiasingAsPostEffect aape = m_MainCamera.GetComponent<AntialiasingAsPostEffect>();
                                                //                 if (aape != null)
                                                //                 {
                                                //                         aape.enabled = false;
                                                //                 }
                                                //                 Bloom bloom = m_MainCamera.GetComponent<Bloom>();
                                                //                 if (bloom != null)
                                                //                 {
                                                //                         bloom.enabled = false;
                                                //                 }
                                                //m_MainCamera.enabled = false;
    }
    public void EnterRaid()
    {
        AppMain.GetInst().StartCoroutine(ProcessEnterRaid());
    }
    IEnumerator ProcessEnterRaid()
    {
        NetworkManager.GetInst().Suspend();
        UIManager.GetInst().CloseAllUI();
        UI_SceneLoading uis = UIManager.GetInst().ShowUI<UI_SceneLoading>("UI_SceneLoading");
        uis.FadeIn(2f);

        yield return new WaitForSeconds(2f);

        if (GameStateManager.GetInst().GameState != GAMESTATE.RAID_PLAYING)
        {
            RaidConfigManager.GetInst().InitInRaidConfig(); //加载副本中所需配置
            GameStateManager.GetInst().GameState = GAMESTATE.RAID_PLAYING;
            GameUtility.SetAstarParam();

            if (Camera.main != null)
            {
                //Camera.main.enabled = false;
            }
            WorldMapManager.GetInst().DestroyWorldMap();
            HomeManager.GetInst().DestroyHome();
            ResourceManager.GetInst().UnloadAllAB();
        }
        else
        {
            SceneManager.GetInst().ExitLastScene();
            Resources.UnloadUnusedAssets();
        }
        RaidMapHold mapCfg = RaidConfigManager.GetInst().GetRaidMapCfg(m_RaidID);
        if (mapCfg == null)
        {
            Debuger.LogError("Error m_RaidId = " + m_RaidID);
            yield break;
        }

        m_RaidScene = SceneManager.GetInst().EnterScene(mapCfg.scene_resid);
        SetupCamera();

        m_CharacterLight = GameUtility.GetGameObject(m_RaidScene, "CharacterLight");
        m_ShadowLight = GameUtility.GetGameObject(m_RaidScene, "shadowlight");
        m_FollowingLight = m_CharacterLight;

        GameObject brickRoot = new GameObject("BrickRoot");
        brickRoot.transform.SetParent(m_RaidScene.transform);
        brickRoot.transform.position = Vector3.zero;
        brickRoot.transform.rotation = Quaternion.identity;
        m_BrickManager = brickRoot.AddComponent<BrickSceneManager>();

        m_DayTimeBehav = m_RaidScene.GetComponentInChildren<SceneDayTime>();

        //m_nWorldSize = m_nPieceSize * m_nMapSize;
        m_nWorldSize = 128;
        m_vWorldOffset = Vector3.zero;
        RaidSceneConfig cfg = RaidConfigManager.GetInst().GetSceneCfg(mapCfg.scene_cfgid);
        if (cfg != null && !string.IsNullOrEmpty(cfg.scene_brick))
        {
            m_vWorldOffset = cfg.scene_offset;
            m_nWorldSize = m_BrickManager.SetupScene(cfg.scene_brick, cfg.model_list, cfg.scene_offset);
            InitOrnaments(cfg.ornament);
        }
        else
        {
            m_BrickManager.InitMeshArray(m_nWorldSize);
        }

        GameObject tmpObj = GameObject.Find("Terrain");
        if (tmpObj != null)
        {
            GameObject.Destroy(tmpObj);
        }
        GameObject collisionRoot = GameUtility.GetGameObject(m_RaidScene, "CollisionRoot");
        if (collisionRoot == null)
        {
            collisionRoot = new GameObject("CollisionRoot");
            collisionRoot.transform.SetParent(m_RaidScene.transform);
            collisionRoot.transform.localScale = new Vector3(13, 1, 13);
            collisionRoot.AddComponent<BoxCollider>();
        }
        collisionRoot.AddComponent<RaidTerrainManager>();
        collisionRoot.transform.position = new Vector3(49.5f, -0.2f, 49.5f);
        collisionRoot.transform.localScale = new Vector3(13, 1, 13);
        GameUtility.SetLayer(collisionRoot, "Scene");

        InitFow();
        HighlightingBase hb = m_MainCamera.gameObject.AddComponent<HighlightingMobile>(); //描边
        m_MainCamera.enabled = false;
        UpdateBrightness();

        InitSpecialRoom();

        AppMain.GetInst().StartCoroutine(PrepareRaid());
    }

    void InitOrnaments(string info)
    {
        string[] infos = info.Split(';');
        foreach (string tmp in infos)
        {
            if (string.IsNullOrEmpty(tmp))
                continue;
            string[] tmps = tmp.Split(',');
            if (tmps.Length == 3)
            {
                int modelId = int.Parse(tmps[0]);
                int x = int.Parse(tmps[1]);
                int z = int.Parse(tmps[2]);
                int y = m_BrickManager.GetHeightByPos(x, z) - 1;
                if (y >= 0)
                {
                    GameObject obj = ModelResourceManager.GetInst().GenerateObject(modelId);
                    obj.transform.SetParent(m_RaidScene.transform);
                    obj.transform.position = new Vector3(x, y, z) + m_vWorldOffset;
                }
            }
        }
    }
    FOWRevealer m_Revealer;
    void InitFow()
    {
        Camera.main.farClipPlane = 50f;
        FOWEffect foweffect = m_MainCamera.gameObject.GetComponent<FOWEffect>();
        if (foweffect == null)
        {
            singleton.GetInst().ShowMessage(ErrorOwner.artist, "镜头上没有FOWEffect组件");
        }
        else
        {
            foweffect.unexploredColor = Color.black;
            foweffect.exploredColor = new Color(100f / 255f, 100f / 255f, 100f / 255f);
        }
        m_nWorldSize = 128;
        GameObject fowObj = new GameObject("FowSystem");
        fowObj.transform.SetParent(m_RaidScene.transform);
        fowObj.AddComponent<FOWSystem>();
        FOWSystem.instance.Init();
        FOWSystem.instance.transform.position = new Vector3(m_nWorldSize * 0.5f + m_vWorldOffset.x, 0f, m_nWorldSize * 0.5f + m_vWorldOffset.z);
        FOWSystem.instance.ResetWorldSize(m_nWorldSize);
        FOWSystem.instance.ResetTextureSize(256);
        FOWSystem.instance.CreateGrid();
        FOWSystem.instance.ResetTexture();
        GameObject revealerObj = new GameObject();
        revealerObj.transform.SetParent(m_RaidScene.transform);
        m_Revealer = revealerObj.AddComponent<FOWRevealer>();
    }

    void HighlightSingleRoom(RaidRoomBehav roomData, bool bEnable = true)
    {
        if (FOWSystem.instance == null)
            return;
        for (int x = -1; x < roomData.RealSizeX; x++)
        {
            for (int y = -1; y < roomData.RealSizeY; y++)
            {
                //FOWSystem.instance.HighlightSinglePoint((int)roomData.transform.position.x + x, (int)roomData.transform.position.z + y, bEnable);
            }
        }
    }
    void UnExploredSingleRoom(RaidRoomBehav roomData)
    {
        if (FOWSystem.instance == null)
            return;
        for (int x = 0; x < roomData.RealSizeX - 1; x++)
        {
            for (int y = 0; y < roomData.RealSizeY - 1; y++)
            {
                FOWSystem.instance.SetBufferUnexplored((int)roomData.transform.position.x + x, (int)roomData.transform.position.z + y);
            }
        }
    }

    void RevealSingleRoom(RaidRoomBehav roomData)
    {
        if (FOWSystem.instance == null)
            return;
        for (int x = 0; x < roomData.RealSizeX - 1; x++)
        {
            for (int y = 0; y < roomData.RealSizeY - 1; y++)
            {
                FOWSystem.instance.RevealSinglePoint((int)roomData.transform.position.x + x, (int)roomData.transform.position.z + y);
            }
        }
    }
    bool CalcLinkRoomDoor(int x, int y, RaidRoomBehav roomData, int linkPieceCfgId)
    {
        if (linkPieceCfgId > 0)
        {
            RaidInfoRandomConfig linkPieceCfg = RaidConfigManager.GetInst().GetPieceConfig(linkPieceCfgId);
            if (linkPieceCfg == null)
            {
                Debuger.LogError(linkPieceCfgId);
                return true;
            }
            int doorType0 = RaidConfigManager.GetInst().GetDoorType(roomData.pieceCfgId);
            int doorType1 = RaidConfigManager.GetInst().GetDoorType(linkPieceCfgId);
            if (doorType0 == doorType1)
            {
                if (roomData.pieceInfoCfg.is_random == linkPieceCfg.is_random)
                {
                    if ((x + y) % 2 == 0)
                    {
                        return false;
                    }
                }
                else if (roomData.IsRandomPiece)
                {
                    return false;
                }
            }
            else if (doorType0 < doorType1)
            {
                return false;
            }

        }
        return true;
    }

    void FocusRoom(RaidRoomBehav rd)
    {

        if (m_FocusRoom == rd)
        {
            return;
        }
        if (m_FocusRoom != null && rd != m_FocusRoom)
        {
            m_FocusRoom.SetFocusRoomColor(false);
            m_FocusRoom.SetRoomDissolve(LINK_DIRECTION.SOUTH, false);
            if (m_FocusRoom.IsComplete)
            {
                m_FocusRoom.DisappearCompletedElems();
            }
            //HighlightSingleRoom(m_FocusRoom, false);
        }
        m_FocusRoom = rd;
        if (rd != null)
        {
            Debuger.Log("FocusRoom " + rd.index);

            m_FocusRoom.SetFocusRoomColor(true);
            m_FocusRoom.SetRoomDissolve(LINK_DIRECTION.SOUTH, true);
            //HighlightSingleRoom(m_FocusRoom, true);
            m_Revealer.transform.position = m_FocusRoom.GetCenterPosition();
            m_Revealer.range = new Vector3(2, m_FocusRoom.RealSizeX / 2f - 0.5f, m_FocusRoom.RealSizeY / 2f - 0.5f);

        }
        //SendRaidMove(rd.roomId / 100, rd.roomId % 100);
        SetMiniMapHeroPosition();
    }

    public RaidNodeBehav GetNodeInFocusRoom(int elemId)
    {
        if (m_FocusRoom != null)
        {
            return m_FocusRoom.GetNodeByElemId(elemId);
        }
        return null;
    }
    LINK_DIRECTION GetLastFromDirect()
    {
        if (m_nLastRoomIndex > -1)
        {
            for (LINK_DIRECTION direct = LINK_DIRECTION.NORTH; direct < LINK_DIRECTION.MAX; direct++)
            {
                int linkIdx = m_FocusRoom.GetLinkRoomIndex(direct);
                if (linkIdx == m_nLastRoomIndex)
                {
                    RaidRoomBehav rd = GetRoomData(linkIdx);
                    if (rd != null && rd.IsActive)
                    {
                        return direct;
                    }
                }
            }
        }

        if (m_FocusRoom != null)
        {
            for (LINK_DIRECTION direct = LINK_DIRECTION.NORTH; direct < LINK_DIRECTION.MAX; direct++)
            {

                int linkIdx = m_FocusRoom.GetLinkRoomIndex(direct);
                if (linkIdx > -1)
                {
                    return ReverseDirect(direct);
                }
            }
        }
        return LINK_DIRECTION.MAX;
    }

    IEnumerator PrepareRaid()
    {
        m_bFirstMove = false;
        m_NodeDict.Clear();
        m_DoorDict.Clear();
        m_DissolveDict.Clear();

        foreach (int ownIdx in m_MapDict.Keys)
        {
            int pieceCfgId = m_MapDict[ownIdx].pieceCfgId;

            RaidRoomBehav room = InitRoomData(ownIdx);
            if (room != null && !room.IsActive)
            {
                UnExploredSingleRoom(room);
                //roomData.transform.RotateAround(roomData.GetCenterPosition(), Vector3.up, 90f);
                //RevealSingleRoom(roomData);
            }
        }
        foreach (RaidDoor door in m_DoorDict.Values)
        {
            RaidRoomBehav roomData0 = GetRoomData(GetOwnIdx(door.idx0));
            if (roomData0 != null)
            {
                roomData0.SetDoor(door.idx1, door);
            }
            RaidRoomBehav roomData1 = GetRoomData(GetOwnIdx(door.idx1));
            if (roomData1 != null)
            {
                roomData1.SetDoor(door.idx0, door);
            }
        }

        m_BrickManager.SetupAllBricks();
        yield return null;
        foreach (RaidRoomBehav roomData in m_RoomDict.Values)
        {
            roomData.SetupNodelist();
        }
        m_BrickManager.CombineAll();

        foreach (RaidRoomBehav roomData in m_RoomDict.Values)
        {
            roomData.UpdateAllDoorState();
        }
        yield return null;
        m_BrickManager.CheckAllTransitions();

        foreach (RaidRoomBehav roomData in m_RoomDict.Values)
        {
            if (roomData.IsActive)
            {
                RevealSingleRoom(roomData);
            }
            roomData.SetRoomVisible(roomData.IsActive || roomData.IsRoadPiece);
            roomData.ManualCombine();
            roomData.SetFocusRoomColor(false);
        }
        yield return null;

        foreach (RaidRoomBehav roomdata in m_RoomDict.Values)
        {
            //roomdata.ClearEmptyObjs();
            roomdata.SetCombineMeshCollider();
        }

        ShowMiniMap();    //显示小地图
        PreparePlayerTeam();
        if (m_MainCamera != null)
        {
            m_MainCamera.enabled = true;
        }
        FocusRoom(GetRoomData(m_EnterRoomIdx));

        m_FocusRoom.UpdateAllDoorState(m_nLastRoomIndex);
        GameUtility.RotateTowards(MainHero.transform, m_FocusRoom.GetCenterPosition());
        RaidRoomBehav lastRoom = GetRoomData(m_nLastRoomIndex);

        m_CurrentFromDirect = GetFromDirect(lastRoom, m_FocusRoom);

        List<HeroUnit> herolist = RaidTeamManager.GetInst().GetHeroList();
        for (int i = 0; i < herolist.Count; i++)
        {
            herolist[i].transform.position = GetHeroPosition(i, lastRoom, m_FocusRoom);
        }

        bool bLocal = false;

        GameUtility.ReScanPath();
        NetworkManager.GetInst().WakeUp();

        m_vCameraRotation = Quaternion.Euler(GlobalParams.GetVector3("camera_rot"));
        m_fCameraDist = GlobalParams.GetFloat("camera_focus_dist");

        m_MainCamera.fieldOfView = GlobalParams.GetFloat("field_of_view");
        m_MainCamera.transform.rotation = m_vCameraRotation;
        m_MainCamera.transform.position = CalcCameraPos();

        m_CameraTargetObj = new GameObject("CameraTarget");
        m_CameraTargetObj.transform.SetParent(m_RaidScene.transform);
        m_CameraTargetObj.transform.position = m_FocusRoom.GetCenterPosition();

        //GuideManager.GetInst().CheckRaidPieceGuide(m_FocusRoom.pieceCfgId);

        //m_MainCamera.gameObject.AddComponent<FadeoutOccluder>();

        if (CombatManager.GetInst().HasUnfinishBattle)
        {
            CombatManager.GetInst().HasUnfinishBattle = false;
            NetworkManager.GetInst().SendMsgToServer(new CSMsgContinueBattle());
        }
        else if (m_StoryMsg != null)
        {
            EnterCutscene(m_StoryMsg.nodeId, m_StoryMsg.storyId, bLocal);
            m_StoryMsg = null;
        }
        else if (m_FocusRoom.pieceType == (int)ROOM_TYPE.Roadblock)
        {
            ActiveRoomEvent(m_FocusRoom);
        }
        m_bLoadingRaid = false;
        Resources.UnloadUnusedAssets();
        GC.Collect();

        int idx = UnityEngine.Random.Range(0, 3);
        AudioManager.GetInst().PlayMusic("raid_bgm" + idx);
        //AudioManager.GetInst().PlaySE("enter_raid");

        //CameraManager.GetInst().FadeOut(3f);
        UI_SceneLoading uis = UIManager.GetInst().ShowUI<UI_SceneLoading>("UI_SceneLoading");
        uis.FadeOut(3f);
    }
    void PreparePlayerTeam()
    {
        RaidTeamManager.GetInst().ClearTeam();
        RaidTeamManager.GetInst().InitTeamMember(TeamInfo);

        SetMainHero(RaidTeamManager.GetInst().GetFirstHero());

        m_UIRaidMain = UIManager.GetInst().ShowUI<UI_RaidMain>("UI_RaidMain");
        //m_UIRaidMain.SetupSkills(RaidTeamManager.GetInst().GetLeaderSkills());
        m_UIRaidMain.SetupHero(RaidTeamManager.GetInst().GetHeroList());
        m_UIRaidMain.SetRaidTask(m_RaidID, m_sRaidMainTaskCount);

        m_UIRaidMain.SetBright(RaidTeamManager.GetInst().GetTeamBright());

        m_UIRaidBag = UIManager.GetInst().ShowUI<UI_RaidBag>("UI_RaidBag");
        UpdateTasks();
    }

    public void ExitRaid()
    {
        m_FocusRoom = null;
        m_RaidID = 0;
        SceneManager.GetInst().ExitLastScene();
        RaidTeamManager.GetInst().ClearTeam();
        RaidTeamManager.GetInst().ClearHeroDict();

        m_NodeDict.Clear();
        m_RoomDict.Clear();
        m_InitNodeDict.Clear();
        m_OptionNodeDataDict.Clear();

        m_DissolveDict.Clear();
        m_DoorDict.Clear();

        RaidConfigManager.GetInst().CleanRaidConfig();

        ResourceManager.GetInst().UnloadAllAB();
    }

    #endregion

    #region ROOM_DATA

    public RaidRoomBehav GetPasswordRoom()
    {
        foreach (RaidRoomBehav room in m_RoomDict.Values)
        {
            if (room.pieceType == 24)
            {
                return room;
            }
        }
        return null;
    }

    RaidRoomBehav InitRoomData(int ownIdx)
    {
        if (m_MapDict.ContainsKey(ownIdx))
        {
            ROOM_MSG_INFO roomInfo = m_MapDict[ownIdx];
            if (roomInfo.pieceCfgId <= 0)
                return null;

            int x = ownIdx % m_nMapSize;
            int y = ownIdx / m_nMapSize;

            int index = y * m_nMapSize + x;
            RaidRoomBehav roomData = null;

            if (!m_RoomDict.ContainsKey(index))
            {
                GameObject roomObj = new GameObject();
                roomObj.transform.SetParent(m_RaidScene.transform);
                if (index > MapSize * MapSize)
                {
                    roomObj.transform.position = new Vector3(99, 0f, 99);

                }
                else
                {
                    roomObj.transform.position = new Vector3(x * (m_nPieceSize - 1), 0f, y * (m_nPieceSize - 1));
                }
                roomObj.transform.rotation = Quaternion.identity;
                roomObj.name = "Room" + index + CommonString.underscoreStr + roomInfo.pieceCfgId;

                roomData = roomObj.AddComponent<RaidRoomBehav>();
                roomData.SetLinkState(roomInfo.linkDict);
                roomData.pieceCfgId = roomInfo.pieceCfgId;
                roomData.pieceInfoCfg = RaidConfigManager.GetInst().GetPieceConfig(roomInfo.pieceCfgId);
                roomData.index = index;
                roomData.index_X = x;
                roomData.index_Y = y;
                if (index > MapSize * MapSize)
                {
                    roomData.SetFlag(ROOM_FLAG.ACTIVE);
                    roomData.SetFlag(ROOM_FLAG.DETECTED);
                }
                else
                {
                    roomData.RoomFlag = roomInfo.flag;
                }
                roomData.InitRoots();
                roomData.SetRoomVisible(false);
                roomData.InitNodelist();

                m_RoomDict.Add(index, roomData);
                if (roomData.pieceType == 5)
                {
                    roomObj.name += "_Hide";
                    Debug.Log("HideRoom " + index);
                }
                return roomData;
            }
        }
        return null;
    }

    public RaidRoomBehav GetTeamNowRoom()
    {
        return m_FocusRoom;
    }
    public int CalcRealNodeId(int x, int y)
    {
        x = x - (x / m_nPieceSize * m_nPieceSize) + x / (m_nPieceSize - 1) * (m_nPieceSize - 1) + m_nPieceSize / 2;
        y = y - (y / m_nPieceSize * m_nPieceSize) + y / (m_nPieceSize - 1) * (m_nPieceSize - 1) + m_nPieceSize / 2;
        return x * 100 + y;
    }

    public RaidRoomBehav GetRoomByHeroPos()
    {
        if (MainHero != null)
        {
            int x = (int)MainHero.CurrentPos.x;
            int y = (int)MainHero.CurrentPos.y;

            foreach (RaidRoomBehav room in m_RoomDict.Values)
            {
                if (room.transform.position.x <= MainHero.transform.position.x &&
                        room.transform.position.z <= MainHero.transform.position.z &&
                        MainHero.transform.position.x <= room.transform.position.x + m_nPieceSize - 1 &&
                        MainHero.transform.position.z <= room.transform.position.z + m_nPieceSize - 1)
                {
                    return room;
                }
            }
        }
        return null;
    }

    public RaidRoomBehav GetRoomDataByNodeId(int nodeId)
    {
        RaidNodeBehav node = GetRaidNodeBehav(nodeId);
        if (node != null)
        {
            return node.belongRoom;
        }
        return null;
    }
    public RaidRoomBehav GetRoomData(int index)
    {
        if (m_RoomDict.ContainsKey(index))
        {
            return m_RoomDict[index];
        }
        return null;
    }

    Vector3 GetRoomCenterPosition(int nodeId)
    {
        return GetRoomDataByNodeId(nodeId).GetCenterPosition();
    }

    public Dictionary<int, RaidRoomBehav> GetAllRoomData()
    {
        return m_RoomDict;
    }

    void ActiveRoom(int nextRoomIndex)
    {
        RaidRoomBehav roomData = GetRoomData(nextRoomIndex);
        if (roomData != null)
        {
            if (roomData.IsActive == false)
            {
                TriggerRaidAside(roomData);
            }

            roomData.SetFlag(ROOM_FLAG.ACTIVE);
            roomData.SetRoomVisible(true);
            RevealSingleRoom(roomData);
            MiniMapActiveRoom(roomData);
        }
    }

    public void SetLinkRoomPartVisible(int linkIndex, LINK_DIRECTION direct, bool bVisible)
    {
        RaidRoomBehav linkRoom = GetRoomData(linkIndex);
        if (linkRoom != null)
        {
            linkRoom.SetDirectionVisible(ReverseDirect(direct), bVisible);
        }
    }
    LINK_DIRECTION ConvertToLinkDirect(int elink)
    {
        switch (elink)
        {
            case 1:
                return LINK_DIRECTION.NORTH;
            case 2:
                return LINK_DIRECTION.WEST;
            case 4:
                return LINK_DIRECTION.SOUTH;
            case 8:
                return LINK_DIRECTION.EAST;
        }
        return LINK_DIRECTION.MAX;
    }
    IEnumerator ProcessNewRoom(SCMsgRaidCreateRoom msg)
    {
        NetworkManager.GetInst().Suspend();
        IsPause = true;
        GameUtility.DoCameraShake(Camera.main, 2f);
        yield return new WaitForSeconds(2f);

        LINK_DIRECTION direct = ConvertToLinkDirect(msg.linkState);
        int existIndex = GetLinkIndex(msg.index, direct);
        if (existIndex > 0)
        {
            int ownIdx = GetOwnIdx(existIndex);
            RaidRoomBehav existRoom = GetRoomData(ownIdx);
            if (existRoom.AddLinkState(existIndex, msg.index, ReverseDirect(direct)))
            {
                Dictionary<int, int> linkStateDict = new Dictionary<int, int>();
                linkStateDict.Add(msg.index, msg.linkState);
                //                                 RaidRoomBehav newRoom = InitRoomData(msg.pieceCfgId, msg.index, linkStateDict);
                //                                 newRoom.SetupNodelist();
                //                                 UnExploredSingleRoom(newRoom);
                // 
                //                                 RaidDoor door = newRoom.GetDoor(direct);
                //                                 if (door != null)
                //                                 {
                //                                         door.mainNode.belongRoom = existRoom;
                //                                         
                //                                         existRoom.SetDoor(msg.index, door);
                //                                         existRoom.UpdateAllDoorState();
                //                                 }
            }
        }
        IsPause = false;
        NetworkManager.GetInst().WakeUp();
    }
    void RemoveNodeElemObj(GameObject rootObj, RaidNodeBehav node)
    {
        if (node == null)
            return;
        foreach (MeshFilter mf in node.elemObj.GetComponentsInChildren<MeshFilter>())
        {
            if (mf != null)
            {
                if (rootObj != null)
                {
                    CombineChildren cc = rootObj.GetComponent<CombineChildren>();
                    if (cc != null)
                    {
                        cc.RemoveMesh(mf.mesh);
                    }
                }
            }
        }
    }
    List<GameObject> m_ForbidDoorEffectList = new List<GameObject>();
    void DestroyForbidDoorEffect()
    {
        foreach (GameObject obj in m_ForbidDoorEffectList)
        {
            GameObject.Destroy(obj);
        }
        m_ForbidDoorEffectList.Clear();
    }
    public bool IsInFocusRoom(RaidNodeBehav node)
    {
        return node.belongRoom == m_FocusRoom;
    }

    void UnCompleteRoom(RaidRoomBehav room)
    {
        if (room != null)
        {
            room.SetFlag(ROOM_FLAG.COMPLETED, false);
            room.UpdateAllDoorState();
        }
    }

    public bool CheckDoorVisible(RaidRoomBehav room, int linkIndex)
    {
        int linkPieceCfgId = GetLinkPieceCfgId(linkIndex);
        return CalcLinkRoomDoor(linkIndex % m_nMapSize, linkIndex / m_nMapSize, room, linkPieceCfgId);
    }

    public int GetOwnIdx(int subIndex)
    {
        foreach (int ownIdx in m_MapDict.Keys)
        {
            if (m_MapDict[ownIdx].linkDict.ContainsKey(subIndex))
            {
                return ownIdx;
            }
        }
        return -1;
    }

    Dictionary<string, RaidDoor> m_DoorDict = new Dictionary<string, RaidDoor>();
    string GetDoorKey(int idx0, int idx1)
    {
        if (idx0 < idx1)
        {
            return idx0 + CommonString.underscoreStr + idx1;
        }
        else
        {
            return idx1 + CommonString.underscoreStr + idx0;
        }
    }
    public void AddDoor(RaidDoor door)
    {
        if (!m_DoorDict.ContainsKey(door.key))
        {
            m_DoorDict.Add(door.key, door);
        }
    }
    public RaidDoor GetDoor(int idx0, int idx1)
    {
        string key = GetDoorKey(idx0, idx1);
        if (m_DoorDict.ContainsKey(key))
        {
            return m_DoorDict[key];
        }
        return null;
    }
    public bool IsLinked(int linkState, LINK_DIRECTION direct)
    {
        int bit = 1 << (int)direct;
        return (linkState & bit) != 0;
    }
    public LINK_DIRECTION ReverseDirect(LINK_DIRECTION direct)
    {
        return (LINK_DIRECTION)(((int)direct + 2) % 4);
    }

    int GetLinkPieceCfgId(int linkIndex)
    {
        int ownIdx = GetOwnIdx(linkIndex);
        if (m_MapDict.ContainsKey(ownIdx))
        {
            return m_MapDict[ownIdx].pieceCfgId;
        }
        return 0;
    }

    public int GetLinkIndex(int index, LINK_DIRECTION direct)
    {
        int x = index % m_nMapSize;
        int y = index / m_nMapSize;
        return GetLinkIndex(x, y, direct);
    }

    public int GetLinkIndex(int x, int y, LINK_DIRECTION direct)
    {
        int linkX;
        int linkY;
        switch (direct)
        {
            case LINK_DIRECTION.NORTH:
                if (y + 1 < m_nMapSize)
                {
                    linkX = x;
                    linkY = y + 1;
                    return linkY * m_nMapSize + linkX;
                }
                break;
            case LINK_DIRECTION.WEST:
                if (x > 0)
                {
                    linkX = x - 1;
                    linkY = y;
                    return linkY * m_nMapSize + linkX;
                }
                break;
            case LINK_DIRECTION.SOUTH:
                if (y > 0)
                {
                    linkX = x;
                    linkY = y - 1;
                    return linkY * m_nMapSize + linkX;
                }
                break;
            case LINK_DIRECTION.EAST:
                if (x + 1 < m_nMapSize)
                {
                    linkX = x + 1;
                    linkY = y;
                    return linkY * m_nMapSize + linkX;
                }
                break;
        }
        return -1;
    }

    #endregion

    #region Node

    bool ReplaceNodeElem(RaidNodeBehav node, int newElemId, int newElemCount)
    {

        if (node.elemCfg != null && node.elemCfg.id == newElemId && node.elemCount == newElemCount)
        {
            //这种情况通常调用ResetNodeElem
            return false;
        }

        bool bReplaced = false;
        node.elemCount = newElemCount;

        if (node.elemCfg != null && node.elemCfg.id != newElemId)
        {
            Debug.Log(node.elemCfg.id + " != " + newElemId);
            bReplaced = true;
        }
        else if (node.elemCfg == null && newElemId > 0)
        {
            Debuger.Log("0 " + newElemId);
            bReplaced = true;
        }

        node.elemCfg = RaidConfigManager.GetInst().GetElemCfg(newElemId);
        if (bReplaced)
        {
            node.ResetElemObj(true);
        }
        node.SetNodeVisible();
        node.UpdateBlockState();
        node.CheckInteractiveElemIcon();

        return bReplaced;
    }
    public bool IsNodeBlocked(int nodeId)
    {
        if (m_NodeDict.ContainsKey(nodeId))
        {
            return m_NodeDict[nodeId].IsBlock;
        }
        return false;
    }

    #endregion

    #region HeroUnit

    void SetMainHero(HeroUnit unit)
    {
        if (MainHero != null)
        {
            MainHero.m_ReachTargetHandler = null;
        }
        MainHero = unit;
        if (MainHero != null)
        {
            MainHero.m_ReachTargetHandler = ReachTargetNode;

            if (m_FollowingLight != null)
            {
                Vector3 pos = m_FollowingLight.transform.localPosition;
                m_FollowingLight.transform.SetParent(MainHero.transform);
                m_FollowingLight.transform.localPosition = pos;
            }
            if (m_CharacterLight != null)
            {
                m_CharacterLight.transform.SetParent(MainHero.transform);
                m_CharacterLight.transform.localPosition = Vector3.zero;
                m_CharacterLight.transform.localRotation = Quaternion.identity;
            }
        }
    }

    public void PressureTorture(HeroUnit unit, PressureJudgeConfig pjc)
    {
        NetworkManager.GetInst().Suspend();

        AppMain.GetInst().StartCoroutine(ProcessingPressureTorture(unit, pjc));
    }

    IEnumerator ProcessingPressureTorture(HeroUnit unit, PressureJudgeConfig pjc)
    {
        //m_UIRaidMain.ShowPressureEffect(unit.ID);
        m_FocusUnit = unit;
        RaidTeamManager.GetInst().TeamStop();

        yield return new WaitForSeconds(2f);
        Vector3 newpos = MainHero.transform.position - Camera.main.transform.forward * m_fCameraDist * 0.8f;
        iTween.moveToWorld(Camera.main.gameObject, 0.5f, 0f, newpos);

        string effectname = pjc.type == 1 ? "effect_raid_pressure_result_good" : "effect_raid_pressure_result_bad";

        GameObject effectObj = EffectManager.GetInst().GetEffectObj(effectname);
        effectObj.transform.SetParent(unit.transform);
        effectObj.transform.localPosition = Vector3.zero;
        effectObj.transform.localRotation = Quaternion.identity;

        GameObject obj = UIManager.GetInst().ShowUI_Multiple<UI_UnitStatus>("UI_UnitStatus");
        UI_UnitStatus ui_status = obj.GetComponent<UI_UnitStatus>();
        ui_status.m_BelongTransform = unit.transform;
        ui_status.ShowActiveSkill(LanguageManager.GetText(pjc.name));
        ui_status.HideHpBar();
        GameObject.Destroy(obj, 1.2f);

        yield return new WaitForSeconds(2f);

        unit.UnitTalk(LanguageManager.GetText(pjc.talk)/*, pjc.type*/);
        yield return new WaitForSeconds(2f);

        m_FocusUnit = null;
        NetworkManager.GetInst().WakeUp();
    }
    public void CheckHeroPos(HeroUnit unit)
    {
        return;
        if (GameStateManager.GetInst().GameState != GAMESTATE.RAID_PLAYING)
            return;

        if (unit == MainHero)
        {
            SendRaidMove((int)unit.CurrentPos.x, (int)unit.CurrentPos.y);
        }
    }

    public void ShowHeroLeave(HeroUnit unit, bool bLeave)
    {
        RaidTeamManager.GetInst().TeamStop();
        ClearTargetNode();
        if (bLeave)
        {
            RaidTeamManager.GetInst().MoveUnitToLast(unit);
            SetMainHero(RaidTeamManager.GetInst().GetFirstHero());
            m_UIRaidMain.SetupHero(RaidTeamManager.GetInst().GetHeroList());

            GameObject effectObj = EffectManager.GetInst().GetEffectObj("effect_battle_hero_corpse_disappear_001");
            if (effectObj != null && unit != null)
            {
                effectObj.transform.position = unit.transform.position;
            }
        }

        if (unit.Mod != null)
        {
            unit.Mod.SetActive(!bLeave);
        }
    }


    public void ShowHeroDie(HeroUnit unit)
    {
        AppMain.GetInst().StartCoroutine(HeroDie(unit));
    }
    IEnumerator HeroDie(HeroUnit unit)
    {
        IsPause = true;

        RaidTeamManager.GetInst().TeamStop();
        ClearTargetNode();

        RaidTeamManager.GetInst().MoveUnitToLast(unit);

        List<HeroUnit> herolist = RaidTeamManager.GetInst().GetHeroList();
        for (int i = 0; i < herolist.Count; i++)
        {
            if (herolist[i].IsAlive)
            {
                SetMainHero(herolist[i]);
                break;
            }
        }

        m_UIRaidMain.SetupHero(herolist);
        if (unit.AnimComp.CurrentAnim != "die_001")
        {
            unit.AnimComp.PlayAnim("die_001", false);
            yield return new WaitForSeconds(unit.AnimComp.GetAnimTime("die_001"));
        }
        GameObject effectObj = EffectManager.GetInst().GetEffectObj("effect_battle_hero_corpse_disappear_001");
        if (effectObj != null && unit != null)
        {
            effectObj.transform.position = unit.transform.position;
        }
        if (unit.Mod != null)
        {
            GameObject.Destroy(unit.Mod);
            unit.Mod = null;
        }

        IsPause = false;
    }
    public void SwitchCaptain(int index, float time = 0f)
    {
        if (IsPauseOrFocusing())
            return;

        HeroUnit unit = RaidTeamManager.GetInst().GetHeroUnit(index);
        SwitchCaptain(unit, time);
    }
    public void SwitchCaptain(HeroUnit unit, float time = 0f)
    {
        if (unit == MainHero)
            return;

        if (unit == null)
            return;
        if (unit.hero.PetID <= 0)
        {
            return;
        }
        if (unit.IsAlive == false)
        {
            return;
        }
        if (unit.IsLeave)
        {
            return;
        }

        RaidTeamManager.GetInst().SwitchUnit(unit, time);

        SetMainHero(RaidTeamManager.GetInst().GetFirstHero());
        if (m_UIRaidMain != null)
        {
            m_UIRaidMain.SetupHero(RaidTeamManager.GetInst().GetHeroList());
        }
        //                 UI_RaidSkillSelect uis = UIManager.GetInst().GetUIBehaviour<UI_RaidSkillSelect>();
        //                 if (uis != null)
        //                 {
        //                         uis.SelectHero(MainHero);
        //                         if (m_TargetNode != null)
        //                         {
        //                                 GameUtility.RotateTowards(MainHero.transform, m_TargetNode.transform);
        //                         }
        //                 }
    }

    #endregion

    #region Camera
    bool m_bCameraFocusTarget = false;
    Vector3 m_vCameraFocusTarget;

    Vector3 CalcCameraPos()
    {
        return MainHero.transform.position + Vector3.up * GlobalParams.GetFloat("raid_camera_focus_height") - Camera.main.transform.forward * m_fCameraDist;
    }
    public void UpdateCameraFollow()
    {
        if (Camera.main == null)
            return;
        if (m_FocusUnit != null)
        {
            return;
        }

        if (MainHero != null)
        {
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, CalcCameraPos(), Time.deltaTime * 2f);

        }
    }
    #endregion

    #region 元素操作回调

    public delegate void OpFinishHandler(object data);

    void SendOpElemMsg(RaidNodeBehav targetNode, HeroUnit unit, int advSkillId, int toolId)
    {
        if (m_TargetNode != null)
        {
            CSMsgRaidOperateElem msg = new CSMsgRaidOperateElem();
            msg.idHero = unit != null ? unit.hero.ID : 0;
            msg.nodeId = targetNode.oriCfgNodeId;
            msg.advSkillId = advSkillId;
            msg.toolId = toolId;
            NetworkManager.GetInst().SendMsgToServer(msg);
        }
        ClearSelectEffect();
        ClearTargetNode();
    }

    public void OperateCommonElem(int type = 0, int toolId = 0)
    {
        if (m_TargetNode != null && m_TargetNode.elemCfg != null)
        {
            if (MainHero != null)
            {
                int advSkillId = MainHero.hero.GetAdvSkillId();
                float time = GetActionTime(MainHero, m_TargetNode, advSkillId);
                MainHero.StartLoading(time, OnLoadedElem, new object[] { m_TargetNode, MainHero, advSkillId, toolId });
            }
        }
    }


    bool m_bUsingSkill = false;

    public bool IsPauseOrFocusing()
    {
        return IsPause || m_FocusUnit != null;
    }

    public void OnClickPosition(Vector3 position)
    {
        if (CanInput() == false)
        {
            return;
        }
        ClearTargetNode();
        RaidTeamManager.GetInst().TeamGoto(new Vector3(Mathf.RoundToInt(position.x), (int)position.y, Mathf.RoundToInt(position.z)));
    }

    bool CheckNeedTool(int toolId)
    {
        if (toolId > 0)
        {
            DropObject di = ItemManager.GetInst().GetDropObj(toolId, ItemType.BAG_PLACE.RAID, Thing_Type.ITEM);
            if (di == null)
            {
                ItemConfig itemCfg = ItemManager.GetInst().GetItemCfg(toolId);
                if (itemCfg != null)
                {
                    string text = string.Format(LanguageManager.GetText("need_tool_tips"), LanguageManager.GetText(itemCfg.name));
                    GameUtility.ShowTip(text);
                }
                return false;
            }
        }
        return true;
    }
    void ShowSkillSelect()
    {
        UI_RaidSkillSelect uis = UIManager.GetInst().ShowUI<UI_RaidSkillSelect>();
        uis.Setup(m_TargetNode);

        //等待ConfirmUseSkill
    }
    public void ClearTargetNode()
    {
        if (m_TargetNode != null)
        {
            m_TargetNode.EndHighlighter();
            m_TargetNode = null;
            Debuger.Log("m_TargetNode = 0");
        }
        if (MainHero != null)
        {
            MainHero.StopLoading();
        }
        //GameObject.Destroy(m_ObjectEffect);
    }
    public RaidNodeBehav GetTargetNode()
    {
        return m_TargetNode;
    }
    public RaidNodeBehav SelectTargetNode(RaidNodeBehav node)
    {
        if (CanInput() == false)
        {
            return null;
        }
        if (node == null)
        {
            return null;
        }
        if (m_TargetNode != node)
        {
            m_bFirstMove = false;
            ClearTargetNode();
        }
        RaidTeamManager.GetInst().TeamGoto(node.GetCenterPosition());
        int val = RaidTeamManager.GetInst().GetTeamBright();


        if (node.IsNodeElemAvail() && node.IsInteractive())
        {
            if (val >= node.elemCfg.brightness_limit)
            {
                m_TargetNode = node;
                Debuger.Log("m_TargetNode = " + node.id);
                m_TargetNode.StarHighlighter();
                return node;
            }
        }
        return null;
    }
    void OnConfirmOpenDoor(object data)
    {
        ReachTargetNode();
    }
    void OnConfirmOpenConditionDoor(object data)
    {
        SendOpenConditionDoor((RaidRoomBehav)data);
    }

    void OperateTargetNode(object data)
    {
        if (m_TargetNode != null && m_TargetNode.elemCfg != null)
        {
            if (m_TargetNode.IsDoor())
            {
                int ownIdx = GetOwnIdx(m_FocusRoom.GetLinkIndexByDoor(m_TargetNode));
                RaidRoomBehav linkRoom = GetRoomData(ownIdx);
                if (linkRoom != null)
                {
                    if (linkRoom.IsBroken)
                    {
                        ClearTargetNode();
                        CommonDataManager.GetInst().ShowTip(3003);
                        return;
                    }
                    if (linkRoom.IsLocked)
                    {
                        RaidPieceConfig pieceCfg = RaidConfigManager.GetInst().GetExPieceCfg(linkRoom.pieceCfgId);
                        if (pieceCfg != null)
                        {
                            switch (pieceCfg.special_door_type)
                            {
                                case 1:
                                    {
                                        ShowSkillSelect();
                                    }
                                    break;
                                case 2:
                                    {
                                        m_OperatingDoor = m_TargetNode;
                                        SendQueryPasswordClue();
                                    }
                                    break;
                                case 3:
                                    {
                                        RaidConditionRoomConfig cfg = RaidConfigManager.GetInst().GetConditionRoomCfg(linkRoom.pieceCfgId);
                                        if (cfg != null)
                                        {
                                            m_OperatingDoor = m_TargetNode;
                                            GameUtility.ShowConfirmWnd(cfg.desc, OnConfirmOpenConditionDoor, null, linkRoom);
                                        }
                                    }
                                    break;
                                case 4:
                                    {
                                        m_OperatingDoor = m_TargetNode;
                                        SendOpenHideDoor(linkRoom);
                                    }
                                    break;
                                default:
                                    {
                                        SendOpenDoor(m_TargetNode, MainHero, 0);
                                    }
                                    break;
                            }
                        }
                    }
                    else
                    {
                        SendOpenDoor(m_TargetNode, MainHero, 0);
                    }
                }
            }
            else if (m_TargetNode.elemCfg.is_direct_result == 1)
            {
                switch (m_TargetNode.elemCfg.type)
                {
                    case (int)RAID_ELEMENT_TYPE.SUPPORTER:
                        {
                            if (RaidTeamManager.GetInst().HasSupporter())
                            {
                                GameUtility.ShowConfirmWnd("change_supporter_tips",
                                        (x) =>
                                        {
                                            SendRescueHero(m_TargetNode);
                                        },
                                        null, null);
                            }
                            else
                            {
                                SendRescueHero(m_TargetNode);
                            }
                        }
                        break;
                    case (int)RAID_ELEMENT_TYPE.EXIT:
                        {
                            GameUtility.ShowConfirmWnd("exit_hint_word", (x) =>
                            {
                                OperateCommonElem();
                            },
                            null, null);
                        }
                        break;
                    case (int)RAID_ELEMENT_TYPE.TELEPORT:
                        {
                            SendTeleport(m_TargetNode);
                        }
                        break;
                    case (int)RAID_ELEMENT_TYPE.OPTION_NPC:
                        {
                            UI_RaidEventOption uis = UIManager.GetInst().ShowUI<UI_RaidEventOption>();
                            uis.Setup(m_TargetNode, MainHero);
                        }
                        break;
                    case (int)RAID_ELEMENT_TYPE.ALCHEMY:
                        {
                            /*UIManager.GetInst().ShowUI<UI_RaidBag>();*/
                            UI_RaidAlchemy uis = UIManager.GetInst().ShowUI<UI_RaidAlchemy>();
                            uis.Setup(m_TargetNode);
                        }
                        break;
                    default:
                        {
                            if (m_TargetNode.elemCfg.is_ask_executor == 1)
                            {
                                ShowSkillSelect();
                            }
                            else
                            {
                                OperateCommonElem();
                            }
                        }
                        break;
                }
            }
        }
    }

    bool IsInTargetNodeRange(RaidNodeBehav targetNode)
    {
        BoxCollider box = targetNode.GetComponent<BoxCollider>();
        if (box != null)
        {
            Bounds bounds = new Bounds(box.bounds.center, box.bounds.size);
            bounds.Expand(2f);
            if (bounds.Contains(MainHero.transform.position))
            {
                return true;
            }
        }
        return false;
    }

    bool IsNextRoomDangerous()
    {
        //                 if (m_TargetNode != null && m_TargetNode.IsDoor())
        //                 {
        //                         int linkIdx = m_FocusRoom.GetLinkIndexByDoor(m_TargetNode);
        //                         RaidRoomBehav nextRoom = GetRoomData(linkIdx);
        //                         if (nextRoom != null && nextRoom.IsActive == false)
        //                         {
        //                                 RaidTypeConfig cfg = RaidConfigManager.GetInst().GetRaidTypeCfg(nextRoom.pieceType);
        //                                 if (cfg != null && cfg.is_perception == 1)
        //                                 {
        //                                         return true;
        //                                 }
        //                         }
        //                 }
        return false;
    }

    public void ReachTargetNode(object data)
    {
        HeroUnit unit = (HeroUnit)data;

        if (m_TargetNode != null)
        {
            //GameUtility.RotateTowards(unit.transform, m_TargetNode.GetCenterPosition());

            if (m_TargetNode.IsNodeElemAvail())
            {
                //如果不是长距离技能的话，要判断是否在目标隔壁
                if (IsInTargetNodeRange(m_TargetNode) == false)
                {
                    Debuger.LogError("Hero is away from TargetNode " + m_TargetNode.id);
                    ClearTargetNode();
                    return;
                }
                if (IsNextRoomDangerous())
                {
                    UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox").SetConfirmAndCancel("", LanguageManager.GetText("perception_trap_room_word"), OnConfirmOpenDoor, null, null);
                }
                else
                {
                    ReachTargetNode();
                }
            }
            else
            {
                ClearTargetNode();
            }
        }
        else
        {
            List<HeroUnit> list = RaidTeamManager.GetInst().GetHeroList();
            if (list.Count > 0)
            {
                int idx = UnityEngine.Random.Range(0, list.Count);
                RaidManager.GetInst().UnitTalk(TALK_TYPE.MOVE_END, list[idx]);
            }
        }
    }
    void ReachTargetNode()
    {
        if (m_TargetNode != null && m_TargetNode.elemCfg != null)
        {
            m_TargetNode.PlayOperatingEffect();
            float time = m_TargetNode.PlayAnim(NODE_STATE.OPERATING);
            Debuger.Log("StartWaitOperationActing " + m_TargetNode.id);
            AppMain.GetInst().StartCoroutine(WaitOperationActing(time, OperateTargetNode));
        }
    }
    IEnumerator WaitOperationActing(float time, OpFinishHandler handler = null, object data = null)
    {
        m_bUsingSkill = true;
        yield return new WaitForSeconds(time);
        m_bUsingSkill = false;
        if (handler != null)
        {
            handler(data);
        }
    }
    void GetActionAnimName(RaidNodeBehav targetNode, ref string actionName, ref float actionSpeed)
    {
        if (!string.IsNullOrEmpty(targetNode.elemCfg.player_operation_action))
        {
            actionName = targetNode.elemCfg.player_operation_action;
        }
    }

    float GetActionTime(HeroUnit actionUnit, RaidNodeBehav targetNode, int skillCfgId)
    {
        float actionTime = 0f;
        string actionName = "";
        float actionSpeed = 1f;

        actionName = targetNode.elemCfg.player_operation_action;
        if (targetNode.elemCfg.operation_time > 0f)
        {
            actionTime = targetNode.elemCfg.operation_time;
        }
        else if (actionName != "")
        {
            actionTime = GameUtility.GetAnimTime(actionUnit.gameObject, actionName);
        }

        if (actionTime > 0f && !string.IsNullOrEmpty(actionName))
        {
            bool bActionLoop = targetNode.elemCfg.operation_time > 0;
            actionUnit.AnimComp.PlayAnim(actionName, bActionLoop, "", actionSpeed);
        }
        return actionTime;
    }
    void OnLoadedElem(object[] data)
    {
        if (data != null && data.Length >= 4)
        {
            RaidNodeBehav node = (RaidNodeBehav)data[0];
            if (m_TargetNode == node)
            {
                if (node.IsDoor())
                {
                    SendOpenDoor(node, (HeroUnit)data[1], (int)data[2], (int)data[3]);
                }
                else if (node.IsDice())
                {
                    SendTriggerDice(node);
                }
                else if (node.CanUnlockBuild())
                {
                    SendDiscoverBuild(node);
                }
                else
                {
                    SendOpElemMsg(node, (HeroUnit)data[1], (int)data[2], (int)data[3]);
                }
            }
        }
    }
    void OnLoadedOptionElem(object[] data)
    {
        if (data != null && data.Length >= 4)
        {
            RaidNodeBehav node = (RaidNodeBehav)data[0];
            if (m_TargetNode == node)
            {
                SendOption(node, (HeroUnit)data[1], (int)data[2], (string)data[3]);
            }
        }
    }
    public void ConfirmOption(RaidNodeBehav targetNode, int optionId, string optionData)
    {
        if (targetNode != null && targetNode.elemCfg != null)
        {
            if (MainHero != null)
            {
                float time = GetActionTime(MainHero, targetNode, 0);
                MainHero.StartLoading(time, OnLoadedOptionElem, new object[] { targetNode, MainHero, optionId, optionData });
            }
        }
    }

    IEnumerator CheckOpLoading(SCMsgRaidNodeChange msg)
    {
        m_bUsingSkill = true;
        int nodeId = msg.NodeId;
        RaidNodeBehav targetNode = GetRaidNodeBehav(nodeId);

        if (targetNode == null)
        {
            int roomIndex = nodeId / 10000;
            RaidRoomBehav room = GetRoomData(roomIndex);
            if (room != null)
            {
                room.CreateNode(nodeId);
                targetNode = GetRaidNodeBehav(nodeId);
            }
        }
        if (targetNode != null)
        {
            AppMain.GetInst().StartCoroutine(ProcessElementChange(targetNode, msg.ElemId, msg.ElemCount));

            if (m_LostDropMsg != null)
            {
                ShowLostDrop(targetNode, m_LostDropMsg);
                m_LostDropMsg = null;
            }
        }

        m_bUsingSkill = false;

        yield return null;

    }
    void OnNodeChange(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidNodeChange msg = e.mNetMsg as SCMsgRaidNodeChange;
        AppMain.GetInst().StartCoroutine(CheckOpLoading(msg));
    }
    void OnDoorOpen(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidOpenDoor msg = e.mNetMsg as SCMsgRaidOpenDoor;

        //RaidDoor door = m_FocusRoom.GetDoor(msg.nextIdx);
        if (m_OperatingDoor != null && m_OperatingDoor.elemCfg != null)
        {
            AppMain.GetInst().StartCoroutine(ProcessDoorChange(m_OperatingDoor, m_OperatingDoor.elemCfg.id, 1));
        }
        m_OperatingDoor = null;
    }

    void OnRoomLockList(object sender, SCNetMsgEventArgs e)
    {
    }
    void OnRoomUnlock(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidUnlock msg = e.mNetMsg as SCMsgRaidUnlock;
        RaidRoomBehav room = GetRoomData(msg.roomIndex);
        if (room != null)
        {
            room.SetFlag(ROOM_FLAG.LOCKED, false);
        }
    }

    SCMsgRaidLostDrop m_LostDropMsg = null;
    void OnLostDrop(object sender, SCNetMsgEventArgs e)
    {
        m_LostDropMsg = e.mNetMsg as SCMsgRaidLostDrop;
    }

    void ShowLostDrop(RaidNodeBehav node, SCMsgRaidLostDrop msg)
    {
        int idx = 1;
        string[] items = msg.itemstr.Split('|');
        foreach (string info in items)
        {
            DropObject di = new DropObject(info);
            GameObject uiObj = UIManager.GetInst().ShowUI_Multiple<UI_AwardItemTip>("UI_AwardItemTip");
            uiObj.transform.localScale = Vector3.one / 100f;
            uiObj.transform.position = node.transform.position + new UnityEngine.Vector3(0f, 2.2f, 0f) - Camera.main.transform.forward * 1f;
            uiObj.AddComponent<UI_Billboard>();
            UI_AwardItemTip uis = uiObj.GetComponent<UI_AwardItemTip>();
            uis.SetIcon(di.GetIconName(), false);
            HeroUnit unit = RaidTeamManager.GetInst().GetHeroUnitByID(msg.idHero);
            if (unit != null)
            {
                Vector3 endpos = unit.transform.position + new UnityEngine.Vector3(0f, 2.2f, 0f) - Camera.main.transform.forward * 1f;
                iTween.moveToWorld(uiObj, 1f, 0.5f * idx, endpos);
            }
            idx++;
        }
    }

    IEnumerator PauseBehaviourSpeaking(SCMsgRaidBehaviourTrigger msg)
    {
        m_bUsingSkill = true;
        SpecificityHold cfg = PetManager.GetInst().GetSpecificityCfg(msg.idBehaviour);
        if (cfg != null)
        {
            HeroUnit unit = RaidTeamManager.GetInst().GetHeroUnitByID(msg.idHero);
            if (cfg.good_or_bad == 1)
            {
                EffectManager.GetInst().PlayEffect("effect_specificity_trigger_good", unit.transform, true);
            }
            else
            {
                EffectManager.GetInst().PlayEffect("effect_specificity_trigger_bad", unit.transform, true);
            }

            GameObject obj = UIManager.GetInst().ShowUI_Multiple<UI_UnitStatus>("UI_UnitStatus");
            UI_UnitStatus ui_status = obj.GetComponent<UI_UnitStatus>();
            ui_status.m_BelongTransform = unit.transform;
            ui_status.ShowActiveSkill(LanguageManager.GetText(cfg.name));
            ui_status.HideHpBar();
            GameObject.Destroy(obj, 1.2f);
            if (cfg.result_type == 1)
            {
                if (msg.idHero != MainHero.hero.ID)
                {
                    SwitchCaptain(RaidTeamManager.GetInst().GetHeroIndexById(msg.idHero), 1.2f);
                }
            }

            yield return new WaitForSeconds(1.2f);
            obj = UIManager.GetInst().ShowUI_Multiple<UI_HeroDialog>("UI_HeroDialog");
            UI_HeroDialog uis = obj.GetComponent<UI_HeroDialog>();
            uis.SetText(LanguageManager.GetText(cfg.trigger_speak), unit.transform);
            GameObject.Destroy(obj, 2f);

        }
        m_bUsingSkill = false;
    }

    void OnRaidBehaviourTrigger(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidBehaviourTrigger msg = e.mNetMsg as SCMsgRaidBehaviourTrigger;
        AppMain.GetInst().StartCoroutine(PauseBehaviourSpeaking(msg));
    }

    void OnRaidAlchemyResult(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidAlchemyResult msg = e.mNetMsg as SCMsgRaidAlchemyResult;
        UI_RaidAlchemy uis = UIManager.GetInst().GetUIBehaviour<UI_RaidAlchemy>();
        if (uis != null)
        {
            uis.SetResult(msg.info);
            NetworkManager.GetInst().Suspend();
        }
    }

    IEnumerator ProcessElementChange(RaidNodeBehav node, int elemId, int elemCount)
    {
        NetworkManager.GetInst().Suspend();

        if (node.elemObj != null)
        {
            node.PlayResultEffect();
            if (elemId <= 0 || elemCount > 0)
            {
                yield return new WaitForSeconds(node.PlayAnim(NODE_STATE.RESULT));
            }
        }
        if (ReplaceNodeElem(node, elemId, elemCount))
        {
            if (node.elemCfg != null)
            {
                string[] asides = node.elemCfg.before_aside.Split(',');
                foreach (string aside in asides)
                {
                    UIManager.GetInst().ShowUI<UI_RaidAside>("UI_RaidAside").SetText(LanguageManager.GetText(aside), true);
                    while (UIManager.GetInst().IsUIVisible("UI_RaidAside"))
                    {
                        yield return null;
                    }
                }
            }
            if (node.elemObj != null)
            {
                //                                 node.elemObj.SetActive(false);
                //                                 node.ShowElemAppear(0f);
                //                                 yield return new WaitForSeconds(0.1f);
                node.elemObj.SetActive(true);
                node.StartPlay();
                Debug.Log("StartPlay");
                node.UpdateBlockState();
            }
        }

        m_MiniMap.RefreshIcon(node.belongRoom);

        if (node.IsElemDone())
        {
            if (!string.IsNullOrEmpty(node.elemCfg.finish_aside))
            {
                string[] asides = node.elemCfg.finish_aside.Split(',');
                foreach (string aside in asides)
                {
                    UIManager.GetInst().ShowUI<UI_RaidAside>("UI_RaidAside").SetText(LanguageManager.GetText(aside), true);
                    while (UIManager.GetInst().IsUIVisible("UI_RaidAside"))
                    {
                        yield return null;
                    }
                }
            }
        }
        GameUtility.ReScanPath();

        NetworkManager.GetInst().WakeUp();
    }

    public void ClearSelectEffect()
    {
        return;
        foreach (HeroUnit unit in RaidTeamManager.GetInst().GetHeroList())
        {
            unit.DestroySelectEffect();
            unit.ShowUIStatus(false, null);
        }
    }

    Vector3 GetHeroPosition(int heroIndex, RaidRoomBehav currRoom, RaidRoomBehav nextRoom)
    {
        if (currRoom != null && nextRoom != null)
        {
            foreach (int subIndex in nextRoom.subIndexList)
            {
                RaidDoor door = currRoom.GetDoor(subIndex);
                if (door != null)
                {
                    int currSubIndex = door.idx0 == subIndex ? door.idx1 : door.idx0;
                    LINK_DIRECTION fromDirect = nextRoom.GetDoorDirection(currSubIndex);

                    Vector3 pos = door.mainNode.GetCenterPosition();
                    return GetHeroPosition(heroIndex, door.mainNode.GetCenterPosition(), fromDirect);
                }
            }
        }

        if (nextRoom != null)
        {
            return GetHeroPosition(heroIndex, nextRoom.GetCenterPosition(), LINK_DIRECTION.SOUTH);
        }
        return Vector3.zero;
    }

    Vector3 GetHeroPosition(int heroIndex, Vector3 pos, LINK_DIRECTION fromDirect)
    {
        Vector2 offset = new Vector2();

        string[] posstr = GlobalParams.GetString("room_enter_position").Split(';');
        if (heroIndex < posstr.Length)
        {
            if (!string.IsNullOrEmpty(posstr[heroIndex]))
            {
                string[] tmps = posstr[heroIndex].Split(',');
                if (tmps.Length == 2)
                {
                    offset.x = float.Parse(tmps[0]);
                    offset.y = float.Parse(tmps[1]);
                }
            }
        }

        switch (fromDirect)
        {
            default:
            case LINK_DIRECTION.SOUTH:
                {
                    pos.x += offset.x;
                    pos.z += offset.y;
                }
                break;
            case LINK_DIRECTION.NORTH:
                {
                    pos.x -= offset.x;
                    pos.z -= offset.y;
                }
                break;
            case LINK_DIRECTION.WEST:
                {
                    pos.z -= offset.x;
                    pos.x += offset.y;
                }
                break;
            case LINK_DIRECTION.EAST:
                {
                    pos.z += offset.x;
                    pos.x -= offset.y;
                }
                break;
        }
        return pos;
    }

    public void TeamFall()
    {
        List<HeroUnit> list = RaidTeamManager.GetInst().GetHeroList();
        for (int i = 0; i < list.Count; i++)
        {
            iTween.moveTo(list[i].gameObject, 1f, UnityEngine.Random.Range(0.01f, 0.1f), list[i].transform.position - Vector3.up * 50f);
        }
    }
    public void TeamGoBack()
    {
        if (m_nLastRoomIndex > -1)
        {
            RaidDoor door = m_FocusRoom.GetDoorByOwnIdx(m_nLastRoomIndex);
            if (door != null)
            {
                SelectTargetNode(door.mainNode);
            }
        }
    }
    public void TeamGoNextRoom()
    {
        Debug.Log("TeamGoNextRoom");
        if (m_nLastRoomIndex > -1)
        {
            foreach (int linkSubIndex in m_FocusRoom.m_DoorDict.Keys)
            {
                int linkOwnIdx = RaidManager.GetInst().GetOwnIdx(linkSubIndex);
                if (linkOwnIdx != m_nLastRoomIndex)
                {
                    SelectTargetNode(m_FocusRoom.m_DoorDict[linkSubIndex].mainNode);
                }
            }
        }
    }

    RaidRoomBehav GetNextRoom(RaidRoomBehav currentRoom, RaidNodeBehav doorNode, ref LINK_DIRECTION doorDirect)
    {
        RaidRoomBehav nextRoom = null;
        if (currentRoom != null)
        {
            if (currentRoom.index != doorNode.belongRoom.index)     //如果门不属于当前房间
            {
                nextRoom = GetRoomData(doorNode.belongRoom.index);
                doorDirect = ReverseDirect(doorDirect);
            }
            else
            {
                nextRoom = GetRoomData(currentRoom.GetLinkRoomIndex(doorDirect));
            }
        }
        return nextRoom;
    }
    LINK_DIRECTION GetFromDirect(RaidRoomBehav currRoom, RaidRoomBehav nextRoom)
    {
        if (currRoom != null && nextRoom != null)
        {
            foreach (int subIndex in nextRoom.subIndexList)
            {
                RaidDoor door = currRoom.GetDoor(subIndex);
                if (door != null)
                {
                    int currSubIndex = door.idx0 == subIndex ? door.idx1 : door.idx0;
                    return nextRoom.GetDoorDirection(currSubIndex);

                }
            }
        }
        return LINK_DIRECTION.SOUTH;
    }
    void ChangeRoom(RaidRoomBehav currRoom, RaidRoomBehav nextRoom)
    {
        ActiveRoom(nextRoom.index);
        FocusRoom(nextRoom);
        m_CurrentFromDirect = GetFromDirect(currRoom, nextRoom);
        Debuger.Log("ChangeRoom " + nextRoom.index + " currFromDirect=" + m_CurrentFromDirect);
    }

    IEnumerator ProcessDoorChange(RaidNodeBehav doorNode, int elemId, int elemCount)
    {
        NetworkManager.GetInst().Suspend();
        IsPause = true;

        if (doorNode.elemObj != null)
        {

            if (elemCount > 0)
            {
                AudioManager.GetInst().PlaySE("open_wood_door");
            }
            doorNode.PlayResultEffect();
            yield return new WaitForSeconds(doorNode.PlayAnim(NODE_STATE.RESULT));
        }
        doorNode.elemCount = elemCount;
        doorNode.UpdateBlockState();

        if (elemCount <= 0)
        {
            AudioManager.GetInst().PlaySE("close_wood_door", 1f, 0.7f);
            if (doorNode.elemCfg == null || elemId != doorNode.elemCfg.id)
            {
                doorNode.elemCfg = RaidConfigManager.GetInst().GetElemCfg(elemId);
                doorNode.ResetElemObj();
            }
            else
            {
                doorNode.RewindAnim();
            }
        }
        else if (elemCount > 0)    //表示开门了
        {
            yield return null;
            yield return null;

            //                         RaidRoomBehav needUncompRoom = null;
            //                         if (m_FocusRoom.pieceType == (int)ROOM_TYPE.Roadblock)
            //                         {
            //                                 needUncompRoom = m_FocusRoom;
            //                         }

            RaidRoomBehav currentRoom = m_FocusRoom;  //队长所在的房间
            int nextIdx = currentRoom.GetLinkIndexByDoor(doorNode);
            LINK_DIRECTION doorDirect = currentRoom.GetDoorDirection(doorNode);     //门的方位
            RaidRoomBehav nextRoom = GetRoomData(GetOwnIdx(nextIdx));

            if (currentRoom != null && nextRoom != null)
            {

                if (!currentRoom.IsRoadPiece || !nextRoom.IsRoadPiece)
                {
                    ChangeRoom(currentRoom, nextRoom);

                    List<HeroUnit> herolist = RaidTeamManager.GetInst().GetHeroList();
                    for (int i = 0; i < herolist.Count; i++)
                    {
                        herolist[i].GoTo(GetHeroPosition(i, currentRoom, nextRoom));
                    }

                    yield return null;

                    foreach (HeroUnit unit in RaidTeamManager.GetInst().GetHeroList())
                    {
                        while (unit.PathComp.canMove)
                        {
                            yield return null;
                        }
                    }
                    //RaidTeamManager.GetInst().TeamRotateTo(m_FocusRoom.GetCenterPosition());
                    SendResetDoor(doorNode);
                }
                SwitchTeamStealth(false);


                ActiveRoomEvent(nextRoom);
                //                                 if (needUncompRoom != null)
                //                                 {
                //                                         UnCompleteRoom(needUncompRoom);
                //                                 }
                yield return null;
                m_nLastRoomIndex = currentRoom.index;
                m_FocusRoom.UpdateAllDoorState(m_nLastRoomIndex);
                //GuideManager.GetInst().CheckRaidPieceGuide(m_FocusRoom.pieceCfgId);
                //GuideManager.GetInst().CheckRaidPieceGuide(currentRoom.pieceCfgId);

            }
        }

        IsPause = false;
        NetworkManager.GetInst().WakeUp();
    }
    void ActiveRoomEvent(RaidRoomBehav roomData)
    {
        foreach (RaidNodeBehav node in roomData.nodeList)
        {
            if (node.elemCfg != null)
            {
                switch (node.elemCfg.type)
                {
                    case (int)RAID_ELEMENT_TYPE.OPTION_NPC:
                        if (roomData.pieceType == (int)ROOM_TYPE.Roadblock)
                        {
                            SelectTargetNode(node);
                        }
                        break;
                        //                                         case (int)RAID_ELEMENT_TYPE.STORY:
                        //                                                 {
                        //                                                         SendStoryTrigger(node);
                        //                                                 }
                        //                                                 break;
                }
            }
        }
    }

    #endregion

    #region 消息发送
    void SendRaidMove(int x, int y)
    {
        if (m_bFirstMove == false)
        {
            m_bFirstMove = true;
        }
        CSMsgRaidMove msg = new CSMsgRaidMove();
        msg.x = x;
        msg.y = y;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }

    public void SendTeleport(RaidNodeBehav node)
    {
        CSMsgRaidTeleport msg = new CSMsgRaidTeleport();
        msg.ownIdx = node.belongRoom.index;
        msg.idNode = node.oriCfgNodeId;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }
    public void SendTriggerHideTrap(RaidNodeBehav node, Collider other)
    {
        HeroUnit unit = other.GetComponent<HeroUnit>();
        if (unit != null)
        {
            if (unit.ID == MainHero.ID)
            {
                CSMsgRaidTriggerHideTrap msg = new CSMsgRaidTriggerHideTrap();
                msg.nodeId = node.oriCfgNodeId;
                NetworkManager.GetInst().SendMsgToServer(msg);
            }
        }
    }

    void SendRescueHero(RaidNodeBehav node)
    {
        CSMsgRaidRescueHero msg = new CSMsgRaidRescueHero();
        msg.nodeId = node.oriCfgNodeId;
        NetworkManager.GetInst().SendMsgToServer(msg);

        ClearSelectEffect();
        ClearTargetNode();
    }

    RaidNodeBehav m_OperatingDoor = null;
    void SendOpenDoor(RaidNodeBehav doorNode, HeroUnit unit, long skillCfgId, int idTool = 0)
    {
        RaidRoomBehav currentRoom = GetTeamNowRoom();  //队长所在的房间
        if (currentRoom != null && doorNode.belongRoom != null)
        {
            CSMsgRaidOpenDoor msg = new CSMsgRaidOpenDoor();
            msg.idHero = unit.ID;
            msg.idTool = idTool;
            msg.skillCfgId = skillCfgId;
            int subIndex = currentRoom.GetLinkIndexByDoor(doorNode);
            msg.nextIndex = GetOwnIdx(subIndex);
            NetworkManager.GetInst().SendMsgToServer(msg);

            m_OperatingDoor = doorNode;
        }
    }
    public void SendOpenConditionDoor(RaidRoomBehav targetRoom)
    {
        CSMsgRaidEnterConditionRoom msg = new CSMsgRaidEnterConditionRoom();
        msg.roomIndex = targetRoom.index;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }
    public void SendOpenHideDoor(RaidRoomBehav targetRoom)
    {
        CSMsgRaidEnterHideRoom msg = new CSMsgRaidEnterHideRoom();
        msg.roomIndex = targetRoom.index;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }
    public void SendOpenPasswordDoor(RaidNodeBehav doorNode, string str)
    {
        m_OperatingDoor = doorNode;
        CSMsgRaidInputPassword msg = new CSMsgRaidInputPassword();
        msg.strPwd = str;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }
    public void SendQueryPasswordClue()
    {
        CSMsgRaidShowClue msg = new CSMsgRaidShowClue();
        NetworkManager.GetInst().SendMsgToServer(msg);
    }

    void SendResetDoor(RaidNodeBehav doorNode)
    {
        AppMain.GetInst().StartCoroutine(ProcessDoorChange(doorNode, doorNode.elemCfg.id, 0));
        //                 CSMsgRaidResetDoor msg = new CSMsgRaidResetDoor();
        //                 msg.nodeId = doorNodeId;
        //                 NetworkManager.GetInst().SendMsgToServer(msg);
    }
    void SendStoryTrigger(RaidNodeBehav node)
    {
        CSMsgRaidStory msg = new CSMsgRaidStory();
        msg.nodeid = node.oriCfgNodeId;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }

    public void SendOption(RaidNodeBehav targetNode, HeroUnit actionUnit, int option, string data)
    {
        CSMsgRaidElemChoose msg = new CSMsgRaidElemChoose();
        msg.nodeId = targetNode.oriCfgNodeId;
        msg.option = option;
        msg.idHero = actionUnit.ID;
        msg.param = data;
        NetworkManager.GetInst().SendMsgToServer(msg);

        ClearTargetNode();
    }

    public void SendRoadCreateElem(int id)
    {
        CSMsgRaidDoRandomElem msg = new CSMsgRaidDoRandomElem();
        msg.x = id / 100;
        msg.y = id % 100;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }
    public void SendPerceptNode(int id)
    {
        CSMsgRaidPerceptionNode msg = new CSMsgRaidPerceptionNode();
        msg.x = id / 100;
        msg.y = id % 100;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }
    public void SendAlchemy(string info, RaidNodeBehav node)
    {
        CSMsgRaidAlchemy msg = new CSMsgRaidAlchemy();
        msg.info = info;
        msg.nodeId = node.oriCfgNodeId;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }

    public void SendAwardDrop(string info)
    {
        if (string.IsNullOrEmpty(info))
            return;

        CSMsgRaidAwardDrop msg = new CSMsgRaidAwardDrop();
        msg.info = info;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }
    public void SendAwardAdd(string info)
    {
        if (string.IsNullOrEmpty(info))
            return;

        CSMsgRaidAwardAdd msg = new CSMsgRaidAwardAdd();
        msg.info = info;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }
    public void SendLeave()
    {
        NetworkManager.GetInst().SendMsgToServer(new CSMsgRaidMapLeave());
    }

    public void SendTriggerDice(RaidNodeBehav node)
    {
        CSMsgRaidTriggerDice msg = new CSMsgRaidTriggerDice();
        msg.nodeId = node.oriCfgNodeId;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }
    public void SendDiscoverBuild(RaidNodeBehav node)
    {
        CSMsgRaidDiscoverNewBuild msg = new CSMsgRaidDiscoverNewBuild();
        msg.idhero = MainHero.ID;
        msg.nodeId = node.oriCfgNodeId;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }

    #endregion

    #region 消息接收
    string m_sRaidInfo = "";
    void OnRaidInfo(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidMapInfo msg = e.mNetMsg as SCMsgRaidMapInfo;
    }
    public void SetupRaid(int enterRoomId, int idRaid, int size,string strInfo)
    {
        m_bLoadingRaid = true;

        m_SpecialRoom.Clear();
        m_RoomDict.Clear();
        m_InitNodeDict.Clear();
        m_OptionNodeDataDict.Clear();

        m_EnterRoomIdx = enterRoomId;
        m_RaidID = idRaid;
        m_nMapSize = size;
        m_nPieceSize = 0;

        RaidMapHold cfg = RaidConfigManager.GetInst().GetRaidMapCfg(m_RaidID);
        if (cfg != null)
        {
            m_nPieceSize = cfg.size;
        }

        m_sRaidMainTaskCount = "";

        InitRaidAsides();
        m_MapDict.Clear();

        m_sRaidInfo = strInfo;
        string[] infos = m_sRaidInfo.Split('|');
        for (int i = 0; i < infos.Length; i++)
        {
            if (string.IsNullOrEmpty(infos[i]))
                continue;
            string[] tmps = infos[i].Split('&');
            if (tmps.Length >= 7)
            {
                int pieceCfgId = int.Parse(tmps[0]);
                int ownIdx = int.Parse(tmps[2]);

                ROOM_MSG_INFO roomInfo = null;

                int linkState = int.Parse(tmps[1]);
                if (!m_MapDict.ContainsKey(ownIdx))
                {
                    roomInfo = new ROOM_MSG_INFO();
                    roomInfo.pieceCfgId = pieceCfgId;
                    roomInfo.SetFlag(tmps);
                    m_MapDict.Add(ownIdx, roomInfo);
                }
                else
                {
                    roomInfo = m_MapDict[ownIdx];
                }
                roomInfo.linkDict.Add(i, linkState);
            }
        }
        DebugPrint(m_sRaidInfo);
    }

    void InitSpecialRoom()
    {
        foreach (var param in m_SpecialRoom)
        {
            if (!m_MapDict.ContainsKey(param.Key))
            {
                ROOM_MSG_INFO roomInfo = new ROOM_MSG_INFO();
                roomInfo.pieceCfgId = param.Value;

                m_MapDict.Add(param.Key, roomInfo);
            }
        }
    }
    void OnDetectTrigger(object sender, SCNetMsgEventArgs e)
    {
        if (m_MiniMap != null)
        {
            AppMain.GetInst().StartCoroutine(ProcessingDetect());
        }
    }
    IEnumerator ProcessingDetect()
    {
        NetworkManager.GetInst().Suspend();
        IsPause = true;
        CommonDataManager.GetInst().ShowTip(5001);
        m_MiniMap.PlayDetect();

        yield return new WaitForSeconds(2f);
        UIManager.GetInst().CloseUI("UI_RaidAside");
        IsPause = false;
        NetworkManager.GetInst().WakeUp();
    }

    void OnRoomDetected(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidSingleRoomDetect msg = e.mNetMsg as SCMsgRaidSingleRoomDetect;
        RaidRoomBehav room = GetRoomData(msg.ownIdx);
        if (room != null)
        {
            if (room.IsDetected == false)
            {
                room.SetFlag(ROOM_FLAG.DETECTED);
                if (room.pieceType == (int)ROOM_TYPE.Hide)
                {
                    m_FocusRoom.UpdateAllDoorState();
                    MiniMapActiveRoom(m_FocusRoom);
                }
                m_MiniMap.RefreshIcon(room);
            }
        }
    }
    void OnRoomCompleted(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidRoomCompleted msg = e.mNetMsg as SCMsgRaidRoomCompleted;
        RaidRoomBehav room = GetRoomData(msg.index);
        if (room != null)
        {
            room.SetFlag(ROOM_FLAG.COMPLETED);
            room.UpdateAllDoorState();
            TriggerAside_Finish(room);
        }
    }
    void OnRoomUnCompleted(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidRoomUnCompleted msg = e.mNetMsg as SCMsgRaidRoomUnCompleted;
        RaidRoomBehav room = GetRoomData(msg.index);
        if (room != null)
        {
            room.SetFlag(ROOM_FLAG.COMPLETED, false);
            room.UpdateAllDoorState();
        }
    }

    void OnChooseReturn(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidChooseReturn msg = e.mNetMsg as SCMsgRaidChooseReturn;
        SelectTargetNode(GetRaidNodeBehav(msg.doorId));
    }

    int m_nLastRoomIndex = -1;
    void OnLastRoom(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidLastRoom msg = e.mNetMsg as SCMsgRaidLastRoom;
        m_nLastRoomIndex = msg.index;
    }
    void OnInitNodeInfo(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidMapInitNodeInfo msg = e.mNetMsg as SCMsgRaidMapInitNodeInfo;
        if (!m_InitNodeDict.ContainsKey(msg.NodeId))
        {
            m_InitNodeDict.Add(msg.NodeId, msg);
        }
    }

    void OnPerceptionNodeInfo(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidPerceptionNodeInfo msg = e.mNetMsg as SCMsgRaidPerceptionNodeInfo;
        int nodeId = msg.x * 100 + msg.y;
        RaidNodeBehav node = GetRaidNodeBehav(nodeId);
        if (node != null)
        {
            int old_type = node.elemCfg != null ? node.elemCfg.type : 0;
            ReplaceNodeElem(node, msg.ElemId, msg.ElemCount);
            if (node.belongRoom.IsActive)
            {
                m_MiniMap.RefreshIcon(node.belongRoom);
            }
        }
    }

    void OnRaidNodeInfoEnd(object sende, SCNetMsgEventArgs e)
    {
        EnterRaid();
    }

    void OnRaidTrap(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidTriggerTrap msg = e.mNetMsg as SCMsgRaidTriggerTrap;

        RaidTrapEffectConfig cfg = RaidConfigManager.GetInst().GetTrapCfg(msg.idTrap);
        if (cfg != null)
        {
            CommonDataManager.GetInst().ShowTip(cfg.hint_word);
            //GameUtility.ShowTip(LanguageManager.GetText(cfg.hint_word));
            if (cfg.is_shock_screen == 1)
            {
                GameUtility.DoCameraShake(Camera.main);
            }

            if (!string.IsNullOrEmpty(cfg.sound_effect))
            {
                AudioManager.GetInst().PlaySE(cfg.sound_effect);
            }

            if (cfg.effect_type == 2)
            {
                GameObject effect = EffectManager.GetInst().GetEffectObj(cfg.effect);
                if (effect != null)
                {
                    effect.transform.position = m_FocusRoom.GetCenterPosition();
                }
            }
            else
            {
                List<HeroUnit> list = new List<HeroUnit>();

                if (msg.idHero > 0)
                {
                    list.Add(RaidTeamManager.GetInst().GetHeroUnitByID(msg.idHero));
                }
                else
                {
                    list = RaidTeamManager.GetInst().GetHeroList();
                }
                if (!string.IsNullOrEmpty(cfg.effect))
                {
                    foreach (HeroUnit unit in list)
                    {
                        if (unit.IsAlive)
                        {
                            GameObject effect = EffectManager.GetInst().GetEffectObj(cfg.effect);
                            if (effect != null)
                            {
                                effect.transform.position = unit.transform.position;
                            }
                        }
                    }
                }
                if (cfg.is_stop == 1)
                {
                    AppMain.GetInst().StartCoroutine(PauseOnTrap());
                }
            }
        }
    }

    void OnRaidFallTrap(object sender, SCNetMsgEventArgs e)
    {
        NetworkManager.GetInst().Suspend();
        AppMain.GetInst().StartCoroutine(ProcessFall());
    }
    IEnumerator ProcessFall()
    {
        //                GameUtility.DoCameraShake(Camera.main, 5f);
        //                yield return new WaitForSeconds(2f);
        m_FocusRoom.Broken();
        //                 RaidCameraFollow rcf = m_MainCamera.GetComponent<RaidCameraFollow>();
        //                 if (rcf != null)
        //                 {
        //                         rcf._target = null;
        //                 }
        yield return new WaitForSeconds(0.2f);
        TeamFall();
        IsPause = true;
        yield return new WaitForSeconds(2f);
        IsPause = false;
        NetworkManager.GetInst().WakeUp();
    }

    void OnRaidAward(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidAward msg = e.mNetMsg as SCMsgRaidAward;

        NetworkManager.GetInst().Suspend();
        IsPause = true;
        UI_DropBag uis = UIManager.GetInst().ShowUI<UI_DropBag>("UI_DropBag", 0.5f);
        uis.SetItems(msg.info);
    }
    void OnRaidLeave(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidMapLeave msg = e.mNetMsg as SCMsgRaidMapLeave;
        UI_RaidResult uis = UIManager.GetInst().ShowUI<UI_RaidResult>("UI_RaidResult");
        uis.SetRaidResult(msg);

        UIManager.GetInst().CloseUI("UI_RaidMain");
        UIManager.GetInst().CloseUI("UI_RaidBag");
        UIManager.GetInst().CloseUI("UI_MiniMap");
    }

    void OnRaidStoryTrigger(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidStoryTrigger msg = e.mNetMsg as SCMsgRaidStoryTrigger;
        if (m_bLoadingRaid)
        {//加载副本的时候先存起来
            m_StoryMsg = msg;
        }
        else
        {
            EnterCutscene(msg.nodeId, msg.storyId, false);
        }
    }
    void OnDoorOpenFail(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidOpenDoorFail msg = e.mNetMsg as SCMsgRaidOpenDoorFail;
        m_OperatingDoor = null;
        GameUtility.ShowTip(LanguageManager.GetText("door_close_tips"));
    }
    void OnNeedToolTip(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidNeedToolTip msg = e.mNetMsg as SCMsgRaidNeedToolTip;
        ItemConfig itemCfg = ItemManager.GetInst().GetItemCfg(msg.idToolCfg);
        if (itemCfg != null)
        {
            string text = string.Format(LanguageManager.GetText("need_tool_tips"), LanguageManager.GetText(itemCfg.name));
            GameUtility.ShowTip(text);
        }
    }
    void OnNewSingleRoom(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidCreateRoom msg = e.mNetMsg as SCMsgRaidCreateRoom;
        AppMain.GetInst().StartCoroutine(ProcessNewRoom(msg));
    }

    void OnOptionNodeData(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidOptionNodeData msg = e.mNetMsg as SCMsgRaidOptionNodeData;
        if (!m_OptionNodeDataDict.ContainsKey(msg.NodeId))
        {
            m_OptionNodeDataDict.Add(msg.NodeId, msg);
        }
    }

    public bool IsOpenDoorBattle = false;   //下场战斗是否开门战斗（是的话要按照进门方向来计算站位）
    void OnRaidDoorBattle(object sender, SCNetMsgEventArgs e)
    {
        IsOpenDoorBattle = true;
    }

    void OnRaidShowClue(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidShowClue msg = e.mNetMsg as SCMsgRaidShowClue;
        UI_RaidPassword uis = UIManager.GetInst().ShowUI<UI_RaidPassword>();
        uis.Setup(m_TargetNode, msg.info);
    }
    void OnRaidNewClue(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidNewClue msg = e.mNetMsg as SCMsgRaidNewClue;
        UI_RaidPasswordTips uis = UIManager.GetInst().ShowUI<UI_RaidPasswordTips>();
        uis.Setup(msg.info, GetPasswordRoom());
    }

    void OnInputPasswordError(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidInputPasswordError msg = e.mNetMsg as SCMsgRaidInputPasswordError;

        string text = string.Format(CommonDataManager.GetInst().GetHintWord(3001), msg.valueCorrectCount, msg.orderCorrectCount);

        UIManager.GetInst().ShowUI<UI_RaidAside>("UI_RaidAside").SetText(text);
        //GameUtility.ShowTip(text);
    }

    void OnDiceResult(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidDiceResult msg = e.mNetMsg as SCMsgRaidDiceResult;
        AppMain.GetInst().StartCoroutine(ShowDiceResult(msg));
    }
    IEnumerator ShowDiceResult(SCMsgRaidDiceResult msg)
    {
        NetworkManager.GetInst().Suspend();
        IsPause = true;
        RaidNodeBehav node = GetRaidNodeBehav(msg.NodeId);
        if (node != null && node.elemObj != null)
        {
            GameUtility.ObjPlayAnim(node.elemObj, "Rotate_" + msg.ret, false);
            float time = GameUtility.GetAnimTime(node.elemObj, "Rotate_" + msg.ret) + 1;
            yield return new WaitForSeconds(time / 2f);
        }
        GameUtility.ShowTipAside(LanguageManager.GetText("raid_destiny_room_aside_" + msg.ret));

        m_UIRaidMain.PlayDiceEffect();
        yield return new WaitForSeconds(3f);
        IsPause = false;
        NetworkManager.GetInst().WakeUp();
    }

    Dictionary<int, int> m_SpecialRoom = new Dictionary<int, int>();
    void OnRaidSpecialRoom(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidSpecialRoom msg = e.mNetMsg as SCMsgRaidSpecialRoom;
        m_SpecialRoom.Add(msg.idOwnidx, msg.idFragment);
    }
    void OnRaidTransport(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidTeleport msg = e.mNetMsg as SCMsgRaidTeleport;
        AppMain.GetInst().StartCoroutine(ProcessTransport(GetRoomData(msg.index)));
    }

    IEnumerator ProcessTransport(RaidRoomBehav nextRoom)
    {
        List<HeroUnit> herolist = RaidTeamManager.GetInst().GetHeroList();

        EffectManager.GetInst().PlayEffect("adventure_skill_effect_8", MainHero.transform.position);

        foreach (HeroUnit unit in herolist)
        {
            unit.gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(1f);

        ChangeRoom(m_FocusRoom, nextRoom);
        for (int i = 0; i < herolist.Count; i++)
        {
            herolist[i].StopSeek();
            herolist[i].transform.position = GetHeroPosition(i, null, m_FocusRoom);
        }
        Vector3 newpos = MainHero.transform.position + Vector3.up;
        newpos -= Camera.main.transform.forward * m_fCameraDist;
        Camera.main.transform.position = newpos;

        EffectManager.GetInst().PlayEffect("adventure_skill_effect_8", MainHero.transform.position);
        yield return new WaitForSeconds(1f);
        foreach (HeroUnit unit in herolist)
        {
            unit.gameObject.SetActive(true);
        }
    }

    void OnRaidBroken(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidBroken msg = e.mNetMsg as SCMsgRaidBroken;
        RaidRoomBehav room = GetRoomData(msg.ownidx);
        if (room != null)
        {
            room.SetFlag(ROOM_FLAG.BROKEN, true);
        }
    }

    void OnDiscoverNewBuild(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidDiscoverNewBuild msg = e.mNetMsg as SCMsgRaidDiscoverNewBuild;
        BuildingInfoHold buildCfg = HomeManager.GetInst().GetBuildInfoCfg(msg.idBuild * 100 + 1);
        if (buildCfg != null)
        {
            string text = string.Format(LanguageManager.GetText("unlock_building_success_tips"), LanguageManager.GetText(buildCfg.name));
            GameUtility.ShowTip(text);

            UI_NewFurniture uis = UIManager.GetInst().ShowUI<UI_NewFurniture>();
            uis.SetInfo(msg.idBuild);
        }
    }

    #endregion

    #region 隐身

    public void SwitchTeamStealth(bool bEnable)
    {
        foreach (HeroUnit unit in RaidTeamManager.GetInst().GetHeroList())
        {
            TransparentSwitch ts = unit.GetComponent<TransparentSwitch>();
            if (ts == null)
            {
                ts = unit.gameObject.AddComponent<TransparentSwitch>();
            }
            ts.enabled = bEnable;
            unit.IsWalking = bEnable;
        }
    }

    #endregion

    #region 战斗

    public void EnterCombat()
    {
        if (m_UIRaidMain != null)
        {
            m_UIRaidMain.EnableCombatMode(true);

        }
        if (m_UIRaidBag != null)
        {
            m_UIRaidBag.GetComponent<Canvas>().enabled = false;
        }

        SetMiniMapActive(false);
        GameUtility.EnableCameraRaycaster(false);
        GameStateManager.GetInst().GameState = GAMESTATE.RAID_COMBAT;
        m_bFirstMove = false;
        ClearTargetNode();
        RaidTeamManager.GetInst().TeamStop();
        UIManager.GetInst().CloseUI("UI_RaidTaskFinish");

        m_CharacterLight.transform.SetParent(m_RaidScene.transform);
    }
    public void ExitCombat()
    {
        if (m_UIRaidMain != null)
        {
            m_UIRaidMain.EnableCombatMode(false);
        }
        if (m_UIRaidBag != null)
        {
            m_UIRaidBag.GetComponent<Canvas>().enabled = true;
        }
        SetMiniMapActive(true);
        GameStateManager.GetInst().GameState = GAMESTATE.RAID_PLAYING;
        ReplayRoom();

        RaidTeamManager.GetInst().TeamStop();
        GameUtility.ReScanPath();
        GameUtility.EnableCameraRaycaster(true);
        SwitchCameraToRaid();

        m_CharacterLight.transform.SetParent(MainHero.transform);
        m_CharacterLight.transform.localPosition = Vector3.zero;
        m_CharacterLight.transform.localRotation = Quaternion.identity;
    }

    public bool GetBattlePoints(int nodeId, ref RaidBattlePointBehav myBp, ref RaidBattlePointBehav enemyBp)
    {
        Vector3 enemyPos = Vector3.zero;
        Vector3 myPos = Vector3.zero;

        if (IsOpenDoorBattle)
        {
            enemyPos = m_FocusRoom.GetCenterPosition();
            myPos = m_FocusRoom.GetCenterPosition();
            switch (m_CurrentFromDirect)
            {
                case LINK_DIRECTION.NORTH:
                    myPos.z += GlobalParams.GetFloat("battle_position_mid_distance");
                    enemyPos.z -= GlobalParams.GetFloat("battle_position_mid_distance");
                    break;
                case LINK_DIRECTION.SOUTH:
                    myPos.z -= GlobalParams.GetFloat("battle_position_mid_distance");
                    enemyPos.z += GlobalParams.GetFloat("battle_position_mid_distance");
                    break;
                case LINK_DIRECTION.EAST:
                    myPos.x += GlobalParams.GetFloat("battle_position_mid_distance");
                    enemyPos.x -= GlobalParams.GetFloat("battle_position_mid_distance");
                    break;
                case LINK_DIRECTION.WEST:
                    myPos.x -= GlobalParams.GetFloat("battle_position_mid_distance");
                    enemyPos.x += GlobalParams.GetFloat("battle_position_mid_distance");
                    break;
            }
        }
        else if (m_FocusRoom != null)
        {
            RaidNodeBehav node = GetRaidNodeBehav(m_FocusRoom.idOffset + nodeId);
            if (node != null)
            {
                if (node.elemCfg != null && node.elemCfg.is_open_door_trigger == 1)
                {
                    enemyPos = m_FocusRoom.GetCenterPosition();
                    myPos = m_FocusRoom.GetCenterPosition();
                    switch (m_CurrentFromDirect)
                    {
                        case LINK_DIRECTION.NORTH:
                            myPos.z += GlobalParams.GetFloat("battle_position_mid_distance");
                            enemyPos.z -= GlobalParams.GetFloat("battle_position_mid_distance");
                            break;
                        case LINK_DIRECTION.SOUTH:
                            myPos.z -= GlobalParams.GetFloat("battle_position_mid_distance");
                            enemyPos.z += GlobalParams.GetFloat("battle_position_mid_distance");
                            break;
                        case LINK_DIRECTION.EAST:
                            myPos.x += GlobalParams.GetFloat("battle_position_mid_distance");
                            enemyPos.x -= GlobalParams.GetFloat("battle_position_mid_distance");
                            break;
                        case LINK_DIRECTION.WEST:
                            myPos.x -= GlobalParams.GetFloat("battle_position_mid_distance");
                            enemyPos.x += GlobalParams.GetFloat("battle_position_mid_distance");
                            break;
                    }
                }
                else
                {
                    GameUtility.RotateTowards(node.transform, m_FocusRoom.GetCenterPosition());
                    enemyPos = node.GetCenterPosition();
                    myPos = node.GetCenterPosition() + node.transform.forward * GlobalParams.GetFloat("battle_position_mid_distance") * 2;
                }
            }
        }
        Vector3 dir = (enemyPos - myPos);

        Quaternion rotation = Quaternion.LookRotation(dir);
        GameObject myBpObj = new GameObject("MyBP");
        myBp = myBpObj.AddComponent<RaidBattlePointBehav>();

        myBp.transform.position = myPos;
        myBp.transform.rotation = Quaternion.LookRotation(dir);
        myBp.Init();
        //                 if (m_CurrentFromDirect == LINK_DIRECTION.SOUTH)
        //                 {
        //                         GameObject obj = new GameObject();
        //                         obj.transform.SetParent(myBp.transform);
        //                         obj.transform.localPosition = Vector3.zero;
        //                         obj.transform.localRotation = Quaternion.Euler(0f, 225f, 0f);
        //                         Rigidbody rb = obj.AddComponent<Rigidbody>();
        //                         rb.freezeRotation = true;
        //                         rb.useGravity = false;
        //                         rb.constraints = RigidbodyConstraints.FreezePosition;
        //                         BoxCollider box = obj.AddComponent<BoxCollider>();
        //                         box.size = new Vector3(12f, 3f, 2f);
        //                         box.isTrigger = true;
        //                         box.center = new Vector3(0f, 2.6f, -3f);
        //                         KeepRotZero krz = obj.AddComponent<KeepRotZero>();
        //                         krz.OffsetX = 0.17f;
        //                         krz.OffsetY = 0.5f;
        //                         krz.TileX = 0.6f;
        //                         krz.TileY = 0.6f;
        //                 }

        GameObject enemyBpObj = new GameObject("EnemyBP");
        enemyBp = enemyBpObj.AddComponent<RaidBattlePointBehav>();
        enemyBp.transform.position = enemyPos;
        enemyBp.transform.rotation = Quaternion.LookRotation(dir * (-1));
        enemyBp.Init();

        return true;
    }
    public void ReplayRoom()
    {
        m_FocusRoom.PlayInteractiveElems();
    }

    public Dictionary<GameObject, int> GetBattleEnemyObjList()
    {
        Dictionary<GameObject, int> npcObjList = new Dictionary<GameObject, int>();
        RaidRoomBehav room = m_FocusRoom;
        if (room != null)
        {
            foreach (RaidNodeBehav tmpnode in room.nodeList)
            {
                if (tmpnode.elemCfg != null && tmpnode.elemCfg.IsCharacter() && tmpnode.elemObj != null)
                {
                    Debug.Log("npcObjList.Add " + tmpnode.elemCfg.mainModel);
                    npcObjList.Add(tmpnode.elemObj, tmpnode.elemCfg.mainModel);
                }
            }
        }
        return npcObjList;
    }
    #endregion

    #region 过场动画
    public void EnterCutscene(int nodeId, int storyId, bool bLocal)
    {
        RaidNodeBehav node = GetRaidNodeBehav(nodeId);
        if (node == null)
            return;

        if (CutSceneManager.GetInst().EnterCutscene(node.id, storyId, node.belongRoom, bLocal))
        {
            node.elemCfg = null;
            if (m_UIRaidMain != null)
            {
                m_UIRaidMain.gameObject.SetActive(false);
            }
            if (m_UIRaidBag != null)
            {
                m_UIRaidBag.gameObject.SetActive(false);
            }
            SetMiniMapActive(false);
        }
        m_bFirstMove = false;
        RaidTeamManager.GetInst().TeamStop();
        UIManager.GetInst().CloseUI("UI_RaidTaskFinish");
    }
    public void ExitCutscene()
    {
        GameStateManager.GetInst().GameState = GAMESTATE.RAID_PLAYING;
        m_UIRaidMain.gameObject.SetActive(true);
        m_UIRaidBag.gameObject.SetActive(true);
        SetMiniMapActive(true);
        SwitchCameraToRaid();
    }
    #endregion

    #region 生成完地图的合并处理

    #endregion

    #region 扎营

    List<RaidNodeBehav> m_HideCampObjs = new List<RaidNodeBehav>();
    public void EnterCamp()
    {
        GameObject.Destroy(m_ActionUnitSelectEffect);
        m_ActionUnitSelectEffect = null;
        GameStateManager.GetInst().GameState = GAMESTATE.RAID_CAMPING;
        m_UIRaidMain.EnableCampMode(true);
        m_UIRaidBag.EnableCampMode(true);

        SetMiniMapActive(false);
        GameUtility.EnableCameraRaycaster(false);
        GameUtility.ShowTip(LanguageManager.GetText("camp_hint_word"));
        foreach (RaidNodeBehav node in m_FocusRoom.nodeList)
        {
            if (node.elemCfg != null)
            {
                if (node.posX >= m_FocusRoom.posX + m_FocusRoom.RealSizeX / 2 - 2 &&
                        node.posX <= m_FocusRoom.posX + m_FocusRoom.RealSizeX / 2 + 2 &&
                        node.posY >= m_FocusRoom.posY + m_FocusRoom.RealSizeY / 2 - 2 &&
                        node.posY <= m_FocusRoom.posY + m_FocusRoom.RealSizeY / 2 + 2)
                {
                    node.SetNodeVisible(false);
                    m_HideCampObjs.Add(node);
                }
            }
        }
    }
    public void ExitCamp()
    {
        m_UIRaidMain.EnableCampMode(false);
        m_nCampTimes--;

        GameStateManager.GetInst().GameState = GAMESTATE.RAID_PLAYING;
        SetMiniMapActive(true);
        GameUtility.EnableCameraRaycaster(true);

        foreach (RaidNodeBehav node in m_HideCampObjs)
        {
            node.SetNodeVisible(true);
            node.StartPlay();
        }
        m_HideCampObjs.Clear();
    }

    public RaidNodeBehav GetNearestBattlePoint()
    {
        if (m_FocusRoom != null)
        {
            int x = (int)m_FocusRoom.GetCenterPosition().x;
            int y = (int)m_FocusRoom.GetCenterPosition().z;
            return GetRaidNodeBehav(x * 100 + y);
        }
        return null;
    }

    public List<Vector3> GotoCampPoint(ref bool bVertical)
    {
        List<Vector3> poslist = new List<Vector3>();

        Vector3 objPos = m_FocusRoom.GetCenterPosition();
        poslist.Add(objPos);
        poslist.Add(objPos + new Vector3(0f, 0f, -1f));
        poslist.Add(objPos + new Vector3(-1f, 0f, 0f));
        poslist.Add(objPos + new Vector3(0f, 0f, 1f));
        poslist.Add(objPos + new Vector3(1f, 0f, 1f));
        poslist.Add(objPos + new Vector3(2f, 0f, 0f));
        poslist.Add(objPos + new Vector3(1f, 0f, -1f));
        return poslist;
    }

    SCMsgRaidCampInfo m_CampMsg = null;
    void OnCampContinue(object sender, SCNetMsgEventArgs e)
    {
        m_CampMsg = e.mNetMsg as SCMsgRaidCampInfo;
        if (m_CampMsg != null)
        {
            CampManager.GetInst().ContinueCamp(m_CampMsg.m_strCampSkillInfo, m_CampMsg.state, m_CampMsg.skillPoint);
            m_CampMsg = null;
        }
    }

    void OnCampTimes(object sender, SCNetMsgEventArgs e)
    {
        SCRaidCampTimes msg = e.mNetMsg as SCRaidCampTimes;
        RaidInfoHold cfg = GetCurrentRaidInfo();
        if (cfg != null)
        {
            m_nCampTimes = cfg.camp_number - msg.campTimes;
        }
        else
        {
            m_nCampTimes = 0;
        }
    }

    void OnRaidRescueHero(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidRescueHero msg = e.mNetMsg as SCMsgRaidRescueHero;
        RaidSupporterConfig cfg = RaidConfigManager.GetInst().GetSupporterCfg(msg.id);
        if (cfg != null)
        {
            GameUtility.ShowTip(LanguageManager.GetText(cfg.save_speak));
        }
    }
    void OnGetPetTeam(object sender, SCNetMsgEventArgs e)
    {
        SCRaidMapTeam msg = e.mNetMsg as SCRaidMapTeam;
        TeamInfo = msg.info;
    }
    #endregion

    #region 小地图辅助

    UI_MiniMap m_MiniMap;

    public void SetMiniMapHeroPosition()
    {
        if (m_MiniMap != null)
        {
            m_MiniMap.SetHeroPosition(GetTeamNowRoom());
        }
    }

    void ShowMiniMap()
    {
        UIManager.GetInst().CloseUI("UI_MiniMap");
        m_MiniMap = UIManager.GetInst().ShowUI<UI_MiniMap>("UI_MiniMap");
        m_MiniMap.ShowMiniMap(m_nMapSize, m_nPieceSize);
        m_MiniMap.SetFloor(m_RaidID % 1000);

    }

    void SetMiniMapActive(bool isActive)
    {
        if (m_MiniMap != null)
        {
            m_MiniMap.gameObject.SetActive(isActive);
        }
    }

    void MiniMapActiveRoom(RaidRoomBehav roomData)
    {
        if (m_MiniMap != null)
        {
            m_MiniMap.ActiveRoom(roomData);
        }
    }

    #endregion

    #region CAMERA

    public void SwitchCameraToRaid()
    {
        Vector3 rot;
        Camera.main.fieldOfView = GlobalParams.GetFloat("field_of_view");
        rot = GlobalParams.GetVector3("camera_rot");
        m_fCameraDist = GlobalParams.GetFloat("camera_focus_dist");

        Quaternion tmpQua = Camera.main.transform.rotation;
        Camera.main.transform.rotation = Quaternion.Euler(rot);

        Vector3 newpos = MainHero.transform.position + Vector3.up - Camera.main.transform.forward * m_fCameraDist;
        iTween.moveToWorld(Camera.main.gameObject, 1f, 0f, newpos);

        Camera.main.transform.rotation = tmpQua;
        iTween.rotateTo(Camera.main.gameObject, 1f, 0f, rot);
    }
    public void SwitchCameraToCombat()
    {
        bool bVertical = m_CurrentFromDirect == LINK_DIRECTION.NORTH || m_CurrentFromDirect == LINK_DIRECTION.SOUTH;
        bool bSmall = m_CurrentFromDirect == LINK_DIRECTION.SOUTH || m_CurrentFromDirect == LINK_DIRECTION.WEST;

        Camera.main.fieldOfView = GlobalParams.GetFloat("fight_field_of_view");
        m_fCameraDist = GlobalParams.GetFloat("fight_camera_focus_dist");

        Vector3 rot;
        if (bVertical)
        {
            rot = GlobalParams.GetVector3(bSmall ? "fight_camera_rot_D" : "fight_camera_rot_U");
        }
        else
        {
            rot = GlobalParams.GetVector3(bSmall ? "fight_camera_rot_L" : "fight_camera_rot_R");
        }
        Quaternion tmpQua = Camera.main.transform.rotation;
        Camera.main.transform.rotation = Quaternion.Euler(rot);
        Vector3 newpos = CombatManager.GetInst().CenterPoint - Camera.main.transform.forward * m_fCameraDist;
        iTween.moveToWorld(Camera.main.gameObject, 1f, 0f, newpos);

        Camera.main.transform.rotation = tmpQua;
        iTween.rotateTo(Camera.main.gameObject, 1f, 0f, rot);
    }
    public void SwitchToBossCamera(float rotY)
    {
        Camera.main.fieldOfView = GlobalParams.GetFloat("fight_field_of_view");
        m_fCameraDist = GlobalParams.GetFloat("fight_camera_focus_dist");

        Quaternion tmpQua = Camera.main.transform.rotation;
        Vector3 rot = new Vector3(tmpQua.eulerAngles.x, rotY, tmpQua.eulerAngles.z);

        Camera.main.transform.rotation = Quaternion.Euler(rot);
        Vector3 newpos = CombatManager.GetInst().CenterPoint - Camera.main.transform.forward * m_fCameraDist;
        iTween.moveToWorld(Camera.main.gameObject, 1f, 0f, newpos);

        Camera.main.transform.rotation = tmpQua;
        iTween.rotateTo(Camera.main.gameObject, 1f, 0f, rot);
    }


    float m_fCameraRot = 0f;
    void CameraGoto(Vector3 pos)
    {
        iTween.moveToWorld(m_CameraTargetObj, 2f, 0f, pos);
    }
    void CameraRotateTo(LINK_DIRECTION direct)
    {
        switch (direct)
        {
            case LINK_DIRECTION.NORTH:
                m_CameraTargetObj.transform.rotation = Quaternion.Euler(Vector3.up * 0f);
                m_fCameraRot = 0f;
                break;
            case LINK_DIRECTION.SOUTH:
                m_CameraTargetObj.transform.rotation = Quaternion.Euler(Vector3.up * 180f);
                m_fCameraRot = 180f;
                break;
            case LINK_DIRECTION.WEST:
                m_CameraTargetObj.transform.rotation = Quaternion.Euler(Vector3.up * 270f);
                m_fCameraRot = 270f;
                break;
            case LINK_DIRECTION.EAST:
                m_CameraTargetObj.transform.rotation = Quaternion.Euler(Vector3.up * 90f);
                m_fCameraRot = 90f;
                break;
        }
    }
    #endregion

    #region 旁白

    List<KeyValuePair<int, string>> m_EnterAsideList = new List<KeyValuePair<int, string>>();
    List<KeyValuePair<int, string>> m_FinishAsideList = new List<KeyValuePair<int, string>>();

    void InitRaidAsides()
    {
        m_EnterAsideList.Clear();
        m_FinishAsideList.Clear();

        RaidAsideConfigHold asideCfg = RaidConfigManager.GetInst().GetRaidAsideCfg(m_RaidID);
        if (asideCfg != null)
        {
            string[] asideInfos = asideCfg.enter_aside.Split(';');
            foreach (string info in asideInfos)
            {
                if (string.IsNullOrEmpty(info))
                    continue;

                string[] tmps = info.Split(',');
                if (tmps.Length == 2)
                {
                    int type = int.Parse(tmps[0]);
                    m_EnterAsideList.Add(new KeyValuePair<int, string>(type, tmps[1]));
                }
            }
            asideInfos = asideCfg.complete_aside.Split(';');
            foreach (string info in asideInfos)
            {
                if (string.IsNullOrEmpty(info))
                    continue;

                string[] tmps = info.Split(',');
                if (tmps.Length == 2)
                {
                    int type = int.Parse(tmps[0]);
                    m_FinishAsideList.Add(new KeyValuePair<int, string>(type, tmps[1]));
                }
            }
        }
    }

    bool m_bFirstMove = false;
    IEnumerator WaitForPlayerFirstMove(string text)
    {
        Debuger.LogError("WaitForPlayerFirstMove " + text);
        while (m_bFirstMove == false && m_RaidID <= 0)
        {
            yield return null;
        }
        UIManager.GetInst().ShowUI<UI_RaidAside>("UI_RaidAside").SetText(text);
    }

    void TriggerRaidAside(RaidRoomBehav roomdata)
    {
        if (roomdata != null)
        {
            KeyValuePair<int, string> pair = m_EnterAsideList.Find((x) =>
            {
                return x.Key == roomdata.pieceType;
            });
            if (!string.IsNullOrEmpty(pair.Value))
            {
                Debuger.LogWarning("TriggerRaidAside " + roomdata.pieceType + " " + pair.Value);
                UIManager.GetInst().ShowUI<UI_RaidAside>("UI_RaidAside").SetText(LanguageManager.GetText(pair.Value));
            }
        }
    }

    public void TriggerAside_Finish(RaidRoomBehav rd)
    {
        if (m_bLoadingRaid)
            return;

        if (rd != null)
        {
            KeyValuePair<int, string> pair = m_FinishAsideList.Find((x) =>
            {
                return x.Key == rd.pieceType;
            });
            if (!string.IsNullOrEmpty(pair.Value))
            {
                Debuger.LogWarning("TriggerAside_Finish " + rd.index + " " + pair.Value);
                UIManager.GetInst().ShowUI<UI_RaidAside>("UI_RaidAside").SetText(LanguageManager.GetText(pair.Value));
            }
        }
    }

    #endregion

    #region 任务
    public void UpdateTasks()
    {
        Dictionary<int, string> dict = TaskManager.GetInst().GetAllTaskDic();
        foreach (int taskId in dict.Keys)
        {
            UpdateBranchTask(taskId);
        }
    }
    public void UpdateTaskCount(string taskCount)
    {
        m_sRaidMainTaskCount = taskCount;
        if (m_UIRaidMain != null)
        {
            m_UIRaidMain.UpdateMainTask(taskCount);
        }
    }

    public void UpdateBranchTask(int taskId)
    {
        if (m_UIRaidMain != null)
        {
            RaidInfoHold cfg = GetCurrentRaidInfo();
            if (cfg != null)
            {
                if (cfg.related_task != null && cfg.related_task.Contains(taskId))
                {
                    m_UIRaidMain.UpdateBranchTask(taskId);
                }
            }
        }
    }
    #endregion

    #region AwardItemTip
    Queue<DropObject> m_AwardItemQueue = new Queue<DropObject>();
    float m_fAwardItemShow = 0f;

    public void AddItem(DropObject di)
    {
        if (m_UIRaidBag != null)
        {
            m_UIRaidBag.RefreshBag();
        }
    }
    public void UpdateItem(DropObject di)
    {
        if (m_UIRaidBag != null)
        {
            m_UIRaidBag.RefreshBag();
        }
    }
    public void RemoveItem(DropObject di)
    {
        if (m_UIRaidBag != null)
        {
            m_UIRaidBag.RefreshBag();
        }
    }
    public void RefreshBag()
    {
        if (m_UIRaidBag != null)
        {
            m_UIRaidBag.RefreshBag();
        }
    }
    void UpdateAwardItemShow()
    {
        if (Time.realtimeSinceStartup - m_fAwardItemShow < 1f)
            return;
        if (MainHero == null)
            return;
        if (m_AwardItemQueue.Count > 0)
        {
            m_fAwardItemShow = Time.realtimeSinceStartup;
            DropObject di = m_AwardItemQueue.Dequeue();
            GameObject uiObj = UIManager.GetInst().ShowUI_Multiple<UI_AwardItemTip>("UI_AwardItemTip");
            uiObj.transform.localScale = Vector3.one / 50f;
            uiObj.transform.position = MainHero.transform.position + new UnityEngine.Vector3(0f, 2.2f, 0f) - Camera.main.transform.forward * 1f;
            uiObj.AddComponent<UI_Billboard>();
            UI_AwardItemTip uis_damage = uiObj.GetComponent<UI_AwardItemTip>();
            uis_damage.SetIcon(di.GetIconName());
        }
    }
    #endregion

    #region 地形

    #endregion

    #region 光亮

    DayTime GetDayTime(int val)
    {
        DayTime dt = DayTime.NO_LIGHT;
        if (val > 150)
        {
            dt = DayTime.Day;
        }
        else if (val > 100)
        {
            dt = DayTime.Dusk;
        }
        else if (val > 75)
        {
            dt = DayTime.Night;
        }
        else if (val > 50)
        {
            dt = DayTime.MidNight;
        }
        else if (val > 25)
        {
            dt = DayTime.LateNight;
        }
        else if (val > 0)
        {
            dt = DayTime.DeepNight;
        }

        return dt;
    }

    public void InitBrightness()
    {
        int val = RaidTeamManager.GetInst().GetTeamBright();
        DayTime dt = GetDayTime(val);
        if (m_DayTimeBehav != null)
        {
            m_DayTimeBehav.SetModeImme((int)dt);
        }
        if (m_CharacterLight != null)
        {
            SceneLightAdjust sla = m_CharacterLight.GetComponentInChildren<SceneLightAdjust>();
            if (sla != null)
            {
                sla.SetModeImme((int)dt);
            }
        }

    }

    public void UpdateBrightness()
    {
        int val = RaidTeamManager.GetInst().GetTeamBright();
        SetBright(val);
    }
    public void SetBright(int val)
    {
        if (m_UIRaidMain != null)
        {
            m_UIRaidMain.SetBright(val);
        }
        DayTime dt = GetDayTime(val);
        if (m_DayTimeBehav != null)
        {
            m_DayTimeBehav.SetMode((int)dt);
        }
        if (m_CharacterLight != null)
        {
            SceneLightAdjust sla = m_CharacterLight.GetComponentInChildren<SceneLightAdjust>();
            if (sla != null)
            {
                sla.SetMode((int)dt);
            }
        }
    }

    #endregion

    #region PAUSE
    bool m_bPause = false;
    public bool IsPause
    {
        get
        {
            return m_bPause;
        }
        set
        {
            m_bPause = value;
            Debuger.Log("IsPause = " + value);
        }
    }
    IEnumerator PauseOnTrap()
    {
        IsPause = true;
        RaidTeamManager.GetInst().TeamStop();
        yield return new WaitForSeconds(1.5f);
        IsPause = false;
    }
    bool CanInput()
    {
        if (IsPauseOrFocusing())
        {
            return false;
        }
        if (UIManager.GetInst().IsUIVisible("UI_RaidSkillSelect"))
        {
            return false;
        }
        if (m_bUsingSkill)
        {
            return false;
        }
        return true;
    }
    #endregion

    public void Update()
    {
        if (UIManager.GetInst().IsUIVisible("UI_RaidSkillSelect"))
        {
            return;
        }
        UpdateCameraFollow();
        UpdateAwardItemShow();
    }

    #region DEBUG_PRINT
    void DebugPrint(string info)
    {
        string log = "";

        int[,] mapArray = new int[m_nMapSize, m_nMapSize];
        foreach (int ownIdx in m_MapDict.Keys)
        {
            log += ownIdx.ToString() + " : ";
            log += m_MapDict[ownIdx].pieceCfgId + " ";
            foreach (var param in m_MapDict[ownIdx].linkDict)
            {
                log += param.Key + "_" + param.Value;
            }
            log += "\n";
            //                         int x = ownIdx % m_nMapSize;
            //                         int y = ownIdx / m_nMapSize;
            //                         mapArray[x, y] = ownIdx;
        }
        //                 for (int i = m_nMapSize - 1; i >= 0; i--)
        //                 {
        //                         for (int j = 0; j < m_nMapSize; j++)
        //                         {
        //                                 if (mapArray[j, i] > 0)
        //                                 {
        //                                         log += string.Format("{0:D6}", mapArray[j, i]);
        //                                 }
        //                                 else
        //                                 {
        //                                         log += "--------";
        //                                 }
        //                                 if (j < m_nMapSize - 1)
        //                                 {
        //                                         log += " | ";
        //                                 }
        //                         }
        //                         log += "\n";
        //                 }
        Debuger.Log(log);
    }
    #endregion

    #region RaidTalk

    public string GetTalkText(TALK_TYPE type)
    {
        RaidTalkConfig cfg = RaidConfigManager.GetInst().GetRaidTalkCfg((int)type);
        if (cfg != null)
        {
            if (UnityEngine.Random.Range(0, 100) <= cfg.rate)
            {
                int idx = UnityEngine.Random.Range(0, cfg.talk_pool.Count);
                return RaidConfigManager.GetInst().GetRaidTalkText(cfg.talk_pool[idx]);
            }
        }
        return "";
    }

    public void UnitShowBuff(SkillBuffConfig buffCfg, HeroUnit unit)
    {
        if (unit != null)
        {
            if (unit.IsAlive && buffCfg != null)
            {
                if (buffCfg.good_or_bad == 2)
                {
                    unit.UnitTalk(GetTalkText(TALK_TYPE.DEBUFF), LanguageManager.GetText(buffCfg.name));
                }
                else
                {
                    unit.UnitTalk("", LanguageManager.GetText(buffCfg.name));
                }
            }
        }
    }

    public void UnitTalk(TALK_TYPE type, HeroUnit unit = null)
    {
        string text = GetTalkText(type);
        if (!string.IsNullOrEmpty(text))
        {
            Debug.Log("UnitTalk + " + type + " " + text);
            if (unit == null)
            {
                MainHero.UnitTalk(text);
            }
            else
            {
                if (unit.IsAlive)
                {
                    unit.UnitTalk(text);
                }
            }
        }
    }
    #endregion

    Dictionary<Renderer, DissolveInfo> m_DissolveDict = new Dictionary<Renderer, DissolveInfo>();
    public List<DissolveInfo> GetDissolveList()
    {
        return new List<DissolveInfo>(m_DissolveDict.Values);
    }
    public void SetRenderDissolve(Renderer renderer)
    {
        if (m_DissolveDict.ContainsKey(renderer))
        {
            m_DissolveDict[renderer].bExit = true;
        }
    }
    public void AddRenderDissolve(Renderer renderer)
    {
        if (!m_DissolveDict.ContainsKey(renderer))
        {
            m_DissolveDict.Add(renderer, new DissolveInfo(renderer));
        }
        else
        {
            m_DissolveDict[renderer].bExit = false;
        }
    }
    public void RemoveDissolve(Renderer renderer)
    {
    }
}


public enum TALK_TYPE
{
    NONE,
    USE_ITEM,       // 1, 使用物品
    GET_DROP,       // 2, 获取掉落
    DEBUFF,         // 3, 中debuff
    HP_MINUS,       // 4, 血量减少
    PRESSURE_ADD,   // 5, 压力增加
    DYING,          // 6.濒死
    MOVE_START,     // 7.开始移动
    MOVE_END,       // 8.结束移动
    CRITICAL_HIT,           // 9.暴击
    BE_CRITICAL_HIT,        // 10.被暴击
    DEBUFF_IN_BATTLE,        //战斗中DEBUFF
    DYING_IN_BATTLE,        //战斗中濒死
    CAMP_SKILL,             // 11.使用扎营技能
    CAMP_END,               // 12.扎营结束
};