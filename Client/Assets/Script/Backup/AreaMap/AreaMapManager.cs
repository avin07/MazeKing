//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using Message;
//using System;


//class AreaMapManager : SingletonObject<AreaMapManager>
//{
//        GameObject m_AreaMapRootNode;
//        GameObject m_CurrMapObj;
//        public GameObject AreaMapRoot
//        {
//                get
//                {
//                        if (m_AreaMapRootNode == null)
//                        {
//                                m_AreaMapRootNode = GameObject.Find("AreaMapRoot");
//                        }

//                        return m_AreaMapRootNode;
//                }
//        }
//        GameObject m_Chessman; //棋子模型

//        public GameObject GetMyChessman()
//        {
//                return m_Chessman;
//        }

//        Dictionary<int, AreaEventPoint> m_AreaEventPoint = new Dictionary<int, AreaEventPoint>();
//        Dictionary<int, WorldMapConfig> m_WorldMapDict = new Dictionary<int, WorldMapConfig>();
//        Dictionary<int, GameObject> m_EventPointObjDict = new Dictionary<int, GameObject>();
//        public void Init()
//        {
//                ConfigHoldUtility<AreaEventPoint>.LoadXml("Config/map_district", m_AreaEventPoint);
//                ConfigHoldUtility<WorldMapConfig>.LoadXml("Config/world_map", m_WorldMapDict);

//                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgAreaMapEnter), OnAreaMapEnter);
//                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgAreaMapExplore), OnAreaMapExplore);
//                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgAreaGuardOpen), OnAreaGuardOpen);

//        }

//        public void EnterWorldMap(int mapid)
//        {
//                CSMsgAreaMapEnter msg = new CSMsgAreaMapEnter();
//                msg.idMap = mapid;
//                NetworkManager.GetInst().SendMsgToServer(msg);
//        }

//        long m_CurrMapId = 0;   //当前地图实例ID
//        int m_CurrDistrictId = 0; //当前任务所在的建筑id
//        SCMsgAreaMapEnter m_area_msg;

//        public void SetCurrDistrictId(int index)
//        {
//            m_CurrDistrictId = index;
//            m_area_msg.nDistrictId = index;
//        }

//        void OnAreaMapEnter(object sender, SCNetMsgEventArgs e)
//        {
//                SCMsgAreaMapEnter msg = e.mNetMsg as SCMsgAreaMapEnter;
//                m_area_msg = msg;
//                m_CurrMapId = msg.id;
//                m_CurrDistrictId = msg.nDistrictId;
//                if (m_CurrDistrictId == 0)
//                {
//                        m_CurrDistrictId = m_WorldMapDict[msg.idMap].bornid;  //防止老账号出错
//                }
//                LoadArea(msg);
//        }

//        public void LoadArea(SCMsgAreaMapEnter msg) //由于navmesh现版本无法动态加载，只能通过场景加载的方法来实现//
//        {
//                Debuger.Log("LoadArea " + msg.idMap);


//                if (m_WorldMapDict.ContainsKey(msg.idMap))
//                {
//                    WorldMapConfig wmc = m_WorldMapDict[msg.idMap];
//                    HelpMap hm = new HelpMap(msg,wmc.res);
//                    AssetBundleStruct m_abs = new AssetBundleStruct("AreaMap/" + wmc.res, MapLoadCallBack, hm,ResourceManager.AssetBundleKind.Sence);
//                    AppMain.GetInst().StartCoroutine(ResourceManager.GetInst().ResourceLoad(m_abs));

//                }
//                //AdventureStageManager.GetInst().InitMapPlayerTeam(PetManager.GetInst().GetMyPetList());
//        }

//        public void LoadArea() //其他场景返回大地图//
//        {
//            if (m_area_msg != null)
//            {
//                if (m_WorldMapDict.ContainsKey(m_area_msg.idMap))
//                {
//                    WorldMapConfig wmc = m_WorldMapDict[m_area_msg.idMap];
//                    HelpMap hm = new HelpMap(m_area_msg, wmc.res);
//                    AssetBundleStruct m_abs = new AssetBundleStruct("AreaMap/" + wmc.res, MapLoadCallBack, hm, ResourceManager.AssetBundleKind.Sence);
//                    AppMain.GetInst().StartCoroutine(ResourceManager.GetInst().ResourceLoad(m_abs));
//                }
//            }
//            else
//            {
//                AreaMapManager.GetInst().EnterWorldMap(1);
//            }

//        }

//        struct HelpMap
//        {
//            public SCMsgAreaMapEnter msg;
//            public string name;
//            public HelpMap(SCMsgAreaMapEnter m_msg,string m_name)
//            {
//                msg = m_msg;
//                name = m_name;
//            }
//        }


//        void MapLoadCallBack(WWW www, AssetBundleStruct abs)
//        {
//            if (www != null)
//            {
//                AppMain.GetInst().StartCoroutine(LoadSence(www, abs));
//            }

//        }

//        IEnumerator LoadSence(WWW www, AssetBundleStruct abs)
//        {
//            HelpMap tag = (HelpMap)abs.tag;
//            AssetBundle ab = www.assetBundle;
//            SCMsgAreaMapEnter msg = tag.msg;
//            AsyncOperation async = Application.LoadLevelAsync(tag.name);
//            while (async.isDone == false || async.progress < 1.0f)
//            {
//                    yield return null;
//            }
//            InitSence(msg);
//        }



//        List<string> guardstates = new List<string>();
//        public bool IsPointCanGo(int point_id)
//        {
//                if (m_AreaEventPoint.ContainsKey(point_id))
//                {
//                        if (guardstates.Contains(m_AreaEventPoint[point_id].belong_outpost.ToString()) || m_AreaEventPoint[point_id].belong_outpost == -1)
//                        {
//                                return true;
//                        }
//                }
//                return false;
//        }

//        public void InitSence(SCMsgAreaMapEnter msg)
//        {
//                //GameObject.Find("StageRoot").GetComponent<Camera>().enabled = true;
//                WorldMapConfig wmc = m_WorldMapDict[msg.idMap];
//                if (AreaMapRoot == null)
//                {
//                    singleton.GetInst().ShowMessage(ErrorOwner.artist, "美术场景缺少AreaMapRoot节点！！！！！");
//                    return;
//                }
//                m_CurrMapObj = AreaMapRoot.transform.FindChild("map_" + msg.idMap).gameObject;
//                if (m_CurrMapObj == null)
//                {
//                    singleton.GetInst().ShowMessage(ErrorOwner.artist, "美术场景AreaMapRoot下没有map_" + msg.idMap + "节点");
//                    return;
//                }
//                Transform cameraTrans = m_CurrMapObj.transform.FindChild("Camera");
//                if (cameraTrans != null)
//                {
//                    Camera cam = cameraTrans.GetComponent<Camera>();
//                    cam.tag = "MainCamera";
//                }
//                m_Chessman = AreaMapRoot.transform.FindChild("Chessman").gameObject;
//                if (m_Chessman == null)
//                {
//                    singleton.GetInst().ShowMessage(ErrorOwner.artist, "美术场景AreaMapRoot下没有Chessman（棋子）节点");
//                    return;
//                }

//                CAMERA_BOUND_LEFT = wmc.cameraBoundX.x;
//                CAMERA_BOUND_RIGHT = wmc.cameraBoundX.y;
//                CAMERA_BOUND_BOTTOM = wmc.cameraBoundY.x;
//                CAMERA_BOUND_TOP = wmc.cameraBoundY.y;

//                float x = Mathf.Clamp(Camera.main.transform.position.x, CAMERA_BOUND_LEFT, CAMERA_BOUND_RIGHT);
//                float z = Mathf.Clamp(Camera.main.transform.position.z, CAMERA_BOUND_BOTTOM, CAMERA_BOUND_TOP);
//                Camera.main.transform.position = new Vector3(x, Camera.main.transform.position.y, z);

//                guardstates = new List<string>(msg.guardlist.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));

//                foreach (Transform trans in m_CurrMapObj.GetComponentsInChildren<Transform>(true))
//                {
//                        if (trans.name.Contains("EventPoint_"))
//                        {
//                                int id = 0;
//                                int.TryParse(trans.name.Replace("EventPoint_", ""), out id);
//                                if (id > 0)
//                                {
//                                        if (!m_EventPointObjDict.ContainsKey(id))
//                                        {
//                                                m_EventPointObjDict.Add(id, trans.gameObject);
//                                        }
//                                        AreaMapEventPointBehaviour epb = trans.gameObject.AddComponent<AreaMapEventPointBehaviour>();
//                                        epb.Id = id;
//                                }
//                                if (id == m_CurrDistrictId) //棋子站立的地方//
//                                {
//                                    Transform position = trans.FindChild("Position");
//                                    m_Chessman.GetComponent<NavMeshAgent>().enabled = false;
//                                    m_Chessman.transform.position = position.position;
//                                    Quaternion qua = new Quaternion();
//                                    qua.eulerAngles = new Vector3(0f, 160f, 0f);
//                                    m_Chessman.transform.LookAt(trans); //方向暂定//
//                                    ChessmanSeekBehaviour cs = m_Chessman.AddComponent<ChessmanSeekBehaviour>();
//                                    cs.NowDestination = m_CurrDistrictId;
//                                    //Debuger.Log(Camera.main.WorldToViewportPoint(m_Chessman.transform.position));
//                                }
//                        }

//                        if (trans.name.Contains("WarFog_"))
//                        {
//                                string tmpId = trans.name.Replace("WarFog_", "");
//                                trans.gameObject.SetActive(!guardstates.Contains(tmpId));
//                        }
//                }
//                //AdventureStageManager.GetInst().InitMapPlayerTeam(PetManager.GetInst().GetMyPetList());
//                GameStateManager.GetInst().GameState = GAMESTATE.AREA_MAP;
//                CameraManager.GetInst().SetFadeCameraActive(false);
//                UIManager.GetInst().ShowUI<UI_MainMenu>("UI_MainMenu");
//        }

//        void OnAreaMapExplore(object sender, SCNetMsgEventArgs e)
//        {

//        }

//        void OnAreaGuardOpen(object sender, SCNetMsgEventArgs e)
//        {
//                SCMsgAreaGuardOpen msg = e.mNetMsg as SCMsgAreaGuardOpen;
//                foreach (Transform trans in m_CurrMapObj.GetComponentsInChildren<Transform>(true))
//                {
//                        if (trans.name.Contains("WarFog_"))
//                        {
//                                string tmpId = trans.name.Replace("WarFog_", "");
//                                if (tmpId == msg.idGuard.ToString())
//                                {
//                                        trans.gameObject.SetActive(false);
//                                        if (!guardstates.Contains(tmpId))
//                                        {
//                                                guardstates.Add(tmpId);
//                                        }
//                                }
//                        }
//                }
//                m_area_msg.guardlist = m_area_msg.guardlist + "|" + msg.idGuard;
//        }

//        public void SendExplore(int idDistrict)
//        {
//                CSMsgAreaMapExplore msg = new CSMsgAreaMapExplore();
//                msg.id = this.m_CurrMapId;
//                msg.nDistrictId = idDistrict;
//                NetworkManager.GetInst().SendMsgToServer(msg);
//        }

//        bool m_bMouseDown = false;
//        Vector2 m_vStartMousePos;
//        Vector3 m_vStartDragPos;

//        public float CAMERA_BOUND_LEFT = -700f;
//        public float CAMERA_BOUND_RIGHT = 700f;
//        public float CAMERA_BOUND_BOTTOM = 700f;
//        public float CAMERA_BOUND_TOP = 700f;



//        void UpdateAreaMap()
//        {

//                if (Camera.main == null)
//                {
//                    return;
//                }
//                if (UIManager.GetInst().HasNormalUIOpen())
//                {
//                        return;
//                }
//                if (m_bMouseDown == false)
//                {
//                        if (InputManager.GetInst().GetInputDown(false))
//                        {
//                                m_vStartMousePos = Input.mousePosition;
//                                m_vStartDragPos = Camera.main.transform.position;
//                                m_bMouseDown = true;

//                        }
//                }
//                else
//                {
//                        if (InputManager.GetInst().GetInputHold(false))
//                        {
//                                float x = Mathf.Clamp(m_vStartDragPos.x - (Input.mousePosition.x - m_vStartMousePos.x) / 10f,
//                                        CAMERA_BOUND_LEFT, CAMERA_BOUND_RIGHT);
//                                float z = Mathf.Clamp(m_vStartDragPos.z - (Input.mousePosition.y - m_vStartMousePos.y) / 10f,
//                                        CAMERA_BOUND_BOTTOM, CAMERA_BOUND_TOP);
//                                Camera.main.transform.position = new Vector3(x, Camera.main.transform.position.y, z);
//                        }
//                        else
//                        {
//                                m_bMouseDown = false;
//                        }
//                }
//        }

//        public void Update()
//        {

//            UpdateAreaMap();

//        }


//        public void ClickBuilding(int id , Vector3 pos)
//        {
//            if (m_AreaEventPoint[id].type == 6) //迷宫
//            {
//                int raid = int.Parse(m_AreaEventPoint[id].para);
//                if (raid < 1000)
//                {
//                    RaidManager.GetInst().ShowEndlessTower(raid);
//                }
//                else
//                {
//                    RaidManager.GetInst().ShowStoryRaid(raid);
//                }
//                AreaMapManager.GetInst().SendExplore(id);    //服务器记录位置//

//            }
//            else
//            {
//                UI_AreaMapMenu uis = UIManager.GetInst().ShowUI<UI_AreaMapMenu>("UI_AreaMapMenu");
//                uis.SetDistrictId(id);
//                uis.SetupPosition(pos);
//            }

//        }

//}
