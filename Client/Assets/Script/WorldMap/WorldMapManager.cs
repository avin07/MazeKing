using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using Message;
using DG.Tweening;


public struct NodeInfo //id&idRaidConfig&nType&areaspot&byShow&ntimes&nClearTimes|
{
        public long id;
        public int raid_id;
        public int type;                //迷宫类型
        public int area_spot;           //area_spot表主键
        public int area_id;             //所在区域
        public int point;               //客户端的点位//
        public int bOpen;               //是否开启 (0关闭,1开启) 为1的还要客户端通过各区域dp值判断是否显示//
        public int times;               //可进入次数
        public int bShow;               //根据区域dp点决定该点位是否显示,客户端计算所得

        public NodeInfo(string[] detail)
        {
                id = long.Parse(detail[0]);
                raid_id = int.Parse(detail[1]);
                type = int.Parse(detail[2]);
                area_spot = int.Parse(detail[3]);
                bOpen = int.Parse(detail[4]);
                times = int.Parse(detail[5]);

                AreaSpotConfig asc = WorldMapManager.GetInst().GetAreaSpotCfg(area_spot);
                if (asc != null)
                {
                        area_id = asc.area_id;
                        point = asc.spot_id;

                        if (bOpen == 1)
                        {
                                int areaDp = WorldMapManager.GetInst().GetDpByArea(area_id);
                                if (areaDp >= asc.need_goal_point)
                                {
                                        bShow = 1;
                                }
                                else
                                {
                                        bShow = 0;
                                }
                        }
                        else
                        {
                                bShow = 0;
                        }
                }
                else
                {
                        area_id = 0;
                        point = 0;
                        bShow = 0;
                }
        }
}

class WorldMapManager : SingletonObject<WorldMapManager>
{
        bool isShow = false;
        public bool IsShow
        {
                set
                {
                        if (value == false)
                        {
                                if (WorldCamera != null)
                                {
                                        CameraPos = WorldCamera.transform.position;
                                }
                        }
                        isShow = value;
                }
                get
                {
                        return isShow;
                }
        }

        public void SetWorldMapActive(bool isactive)
        {
                if (WorldMapRoot != null)
                {
                        WorldMapRoot.SetActive(isactive);
                        WorldCamera.enabled = isactive;
                        if (isactive)
                        {
                                InputBinding();
                                AllRefreshDp();       
                                RefreshRaid();
                                FuckNewAnimation();
                        }
                        else
                        {
                                StopFuckAnimaCoroutine();
                                CleanAllEffect();
                        }
                }
                else
                {
                        if (isactive)
                        {
                                LoadWorldMap();
                        }
                }
                IsShow = isactive;
        }

        GameObject WorldMapRoot;
        GameObject EffectRoot;
        Camera WorldCamera;

        void CleanAllEffect()
        {
                GameUtility.DestroyChild(EffectRoot.transform);
        }


        public int CarriageLevel   //马车等级
        {
                get
                {
                        return PlayerController.GetInst().GetPropertyInt("gharry_lvl");
                }
        }

        Dictionary<int, WorldMapAreaHold> worldAreaCfg = new Dictionary<int, WorldMapAreaHold>();
        Dictionary<int, AreaSpotConfig> areaSpotCfg = new Dictionary<int, AreaSpotConfig>();
        Dictionary<int, WorldmapEventConfig> worldmapEventCfg = new Dictionary<int, WorldmapEventConfig>();
        Dictionary<int, WorldmapTaskConfig> worldmapTaskCfg = new Dictionary<int, WorldmapTaskConfig>();
        
        Dictionary<long, NodeInfo> m_RaidDic = new Dictionary<long, NodeInfo>();  //服务器通知的迷宫信息(不含教学迷宫)
        Dictionary<long, NodeInfo> m_TeachRaidDic = new Dictionary<long, NodeInfo>();  //教学迷宫

        Dictionary<int, int> m_BrightArea = new Dictionary<int, int>(); //服务器告诉我的解锁区域以及每个区域的dp    
        List<int> m_CanUnlockArea = new List<int>();                    //最最最后一次修改：解锁区域只和马车功能等级有关//

        public void Init()
        {
                ConfigHoldUtility<WorldMapAreaHold>.LoadXml("Config/map_area", worldAreaCfg);
                ConfigHoldUtility<AreaSpotConfig>.LoadXml("Config/area_spot", areaSpotCfg);
                ConfigHoldUtility<WorldmapEventConfig>.LoadXml("Config/worldmap_event", worldmapEventCfg);
                ConfigHoldUtility<WorldmapTaskConfig>.LoadXml("Config/worldmap_task", worldmapTaskCfg);

                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgMapInfo), OnGetAreaBright);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgMapAllNodes), OnGetAllRaid);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgMapMapNode), OnGetOneRaid);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgMapNodeDel), OnNodeDelete);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgMapAreaPoint), OnAreaDpPoint);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidRemainTime), OnGetActivityRestTime);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCRaidMaxJumpFloorInfo), OnRaidFloorInfoTrap);
                          
                maskColor = GlobalParams.GetColor32("worldmap_unlock_area_color");
                maskTransparency = (float)maskColor.a / 255;
        }

        public WorldMapAreaHold GetWorldMapAreaCfg(int id)
        {
                if (worldAreaCfg.ContainsKey(id))
                {
                        return worldAreaCfg[id];
                }
                return null;
        }

        public AreaSpotConfig GetAreaSpotCfg(int id)
        {
                if (areaSpotCfg.ContainsKey(id))
                {
                        return areaSpotCfg[id];
                }
                return null;
        }

        public WorldmapEventConfig GetWorldMapEventCfg(int id)
        {
                if (worldmapEventCfg.ContainsKey(id))
                {
                        return worldmapEventCfg[id];
                }
                return null;
        }

        public WorldmapTaskConfig GetWorldMapTaskCfg(int id)
        {
                if (worldmapTaskCfg.ContainsKey(id))
                {
                        return worldmapTaskCfg[id];
                }
                return null;
        }

        public int GetDpByArea(int area)
        {
                if (m_BrightArea.ContainsKey(area))
                {
                        return m_BrightArea[area];
                }
                return 0;
        }

        void OnGetAreaBright(object sender, SCNetMsgEventArgs e)
        {
                SCMsgMapInfo msg = e.mNetMsg as SCMsgMapInfo;
                if (!msg.strUnlockAreaList.Equals(CommonString.zeroStr))
                {
                        string[] info = msg.strUnlockAreaList.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        int area_id;
                        for (int i = 0; i < info.Length; i++)
                        {
                                string[] detail = info[i].Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                                area_id = int.Parse(detail[0]);
                                if (!m_BrightArea.ContainsKey(area_id))
                                {
                                        m_BrightArea.Add(area_id, int.Parse(detail[1]));
                                }
                        }
                        CountUnlockArea();
                        if (IsShow)
                        {
                                RefreshBright();
                                AllRefreshDp();       
                        }                                            
                }
        }

        void OnAreaDpPoint(object sender, SCNetMsgEventArgs e)
        {
                SCMsgMapAreaPoint msg = e.mNetMsg as SCMsgMapAreaPoint;
                if (m_BrightArea.ContainsKey(msg.area))
                {
                        //延时改变区域的dp值                      
                        if(mNeedAniAreaDic.ContainsKey(msg.area))
                        {
                                NewAniInfo info = mNeedAniAreaDic[msg.area];
                                info.mDp = msg.point;
                                mNeedAniAreaDic[msg.area] = info;
                        }
                        else
                        {
                                NewAniInfo info = new NewAniInfo();
                                info.mUpArea = msg.area;
                                info.mDp = msg.point;
                                mNeedAniAreaDic.Add(msg.area, info);
                        }                       
                }
                else
                {
                        singleton.GetInst().ShowMessage(ErrorOwner.exception, "为何下发了未解锁的区域" + msg.area + "的dp点");
                }
        }

        Dictionary<int, NewAniInfo> mNeedAniAreaDic = new Dictionary<int, NewAniInfo>();
        struct NewAniInfo
        {
              public int mUpArea;               //dp增长的区域
              public int mDp;                   //增长前的dp
              public NodeInfo mNewUpNode;       //新通关的副本
        }

        Queue<int> ActivityRaid = new Queue<int>();
        void OnGetAllRaid(object sender, SCNetMsgEventArgs e)
        {
                //strNodeList: id&idRaidConfig&nType&areaSpot&byShow&ntimes&nClearTimes&nRefreshDay|
                SCMsgMapAllNodes msg = e.mNetMsg as SCMsgMapAllNodes;
                string info = msg.strNodeList;
                if (!info.Equals(CommonString.zeroStr))
                {
                        string[] detail = info.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < detail.Length; i++)
                        {
                                string[] temp = detail[i].Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                                long id = long.Parse(temp[0]);
                                NodeInfo m_node = new NodeInfo(temp);

                                if (m_node.type == (int)RAID_TYPE.GUIDE) //教学迷宫
                                {
                                        if (!m_TeachRaidDic.ContainsKey(id))
                                        {
                                                m_node.area_id = 1; //只有1区域有教学迷宫
                                                m_TeachRaidDic.Add(id, m_node);
                                        }
                                }
                                else
                                {
                                        if (!m_RaidDic.ContainsKey(id))
                                        {
                                                m_RaidDic.Add(id, m_node);
                                                ActivityRaidShow(ref m_node);
                                        }
                                }
                        }
                }
        }

        void ActivityRaidShow(ref NodeInfo info)
        {
                if (info.type == (int)RAID_TYPE.ENDLESS || info.type == (int)RAID_TYPE.STAGE) //活动副本要显示提示
                {
                        if (info.bShow == 1)
                        {
                                if (!ActivityRaid.Contains(info.raid_id))
                                {
                                        ActivityRaid.Enqueue(info.raid_id);
                                }
                                ActivityRestTimeReq();
                        }
                }
        }

        public void CheckActivityTip()
        {
                if (ActivityRaid.Count > 0)
                {
                        UIManager.GetInst().ShowUI<UI_ActivityRaidTip>("UI_ActivityRaidTip").Refresh(ActivityRaid.Dequeue());
                }
        }

        public Queue<int> GetTipQue()
        {
                return ActivityRaid;
        }

        void OnGetOneRaid(object sender, SCNetMsgEventArgs e)
        {
                SCMsgMapMapNode msg = e.mNetMsg as SCMsgMapMapNode;
                string info = msg.strNode;
                string[] temp = info.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                long id = long.Parse(temp[0]);
                NodeInfo m_node = new NodeInfo(temp);

                if (m_node.type == (int)RAID_TYPE.GUIDE) //教学迷宫
                {
                        if (m_TeachRaidDic.ContainsKey(id))
                        {
                                m_TeachRaidDic.Remove(id);
                        }
                }
                else
                {
                        if (m_RaidDic.ContainsKey(id))
                        {
                                if (m_node.type == (int)RAID_TYPE.NORMAL)
                                {
                                        //普通副本升级延时表现
                                        if (m_node.raid_id > m_RaidDic[id].raid_id)
                                        {
                                                if (mNeedAniAreaDic.ContainsKey(m_node.area_id))
                                                {
                                                        NewAniInfo nodeInfo = mNeedAniAreaDic[m_node.area_id];
                                                        nodeInfo.mNewUpNode = m_node;
                                                        mNeedAniAreaDic[m_node.area_id] = nodeInfo;
                                                }
                                                else
                                                {
                                                        NewAniInfo nodeInfo = new NewAniInfo();
                                                        nodeInfo.mNewUpNode = m_node;
                                                        mNeedAniAreaDic.Add(m_node.area_id, nodeInfo);
                                                }
                                        }
                                }
                                else
                                {
                                        m_RaidDic[id] = m_node;
                                }
                        }
                        else
                        {
                                m_RaidDic.Add(id, m_node);                   
                        }

                        ActivityRaidShow(ref m_node);
                        if (IsShow)
                        {
                                UpdateOneRaid(id, ref m_node);  //现在只可能是事件点位                               
                        }
                }


        }

        void OnNodeDelete(object sender, SCNetMsgEventArgs e)
        {
                SCMsgMapNodeDel msg = e.mNetMsg as SCMsgMapNodeDel;
                if (m_RaidDic.ContainsKey(msg.id))
                {
                        if (isShow)
                        {
                                CloseRaidIcon(m_RaidDic[msg.id].area_id, m_RaidDic[msg.id].point);
                        }
                        m_RaidDic.Remove(msg.id);
                }
        }

        void OnGetActivityRestTime(object sender, SCNetMsgEventArgs e)
        {
                SCMsgRaidRemainTime msg = e.mNetMsg as SCMsgRaidRemainTime;
                ActivityRestTime = Time.realtimeSinceStartup + msg.time;
                IsActivityRefresh = true;
        }

        public void UnlockArea(int areaid)
        {
                CSMsgMapAreaUnlock msg = new CSMsgMapAreaUnlock();
                msg.areaID = areaid;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

        public void ActivityRestTimeReq()
        {
                CSMsgRaidRemainTimeQuery msg = new CSMsgRaidRemainTimeQuery();
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

        public void GoIntoRaid(int floor, string petlist,string itemlist)
        {
                CSMsgMapRaidEnter msg = new CSMsgMapRaidEnter();
                msg.id = RaidRealID;
                msg.floor = floor;
                msg.strPetList = petlist;
                msg.items = itemlist;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

        public void SendMapEvent(int eventId)
        {
                CSMsgMapEvent msg = new CSMsgMapEvent();
                msg.id = RaidRealID;
                msg.eventId = eventId;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

        public void DestroyWorldMap()
        {
                IsShow = false;
                StopFuckAnimaCoroutine();
                CleanPicData();
                GameObject.Destroy(WorldMapRoot);
                ShowTimeTextList.Clear();
        }

        public void Update()
        {
                if (IsShow)
                {
                        if (Camera.main == null)
                        {
                                return;
                        }
                        if (UIManager.GetInst().HasNormalUIOpen())
                        {
                                return;
                        }

                        InputManager.GetInst().UpdateGlobalInput();
                }
        }

        Vector3 m_vStartMousePos;
        Vector3 m_vStartCameraPos;
        float ZoomSpeed =0.1F;
        float oldDistance;
        float ratio = 0.015f;

        readonly float Max_OrthographicSize = 4.5f;
        readonly float Max_OrthographicSize_Min_X = -9.0f;
        readonly float Max_OrthographicSize_Max_X = 9.0f;
        readonly float Max_OrthographicSize_Min_Y = -5.2f;
        readonly float Max_OrthographicSize_Max_Y = 5.2f;

        readonly float Min_OrthographicSize = 2.0f;
        readonly float Min_OrthographicSize_Min_X = -13.5f;
        readonly float Min_OrthographicSize_Max_X = 13.5f;
        readonly float Min_OrthographicSize_Min_Y = -7.6f;
        readonly float Min_OrthographicSize_Max_Y = 7.6f;

        float CountLimitValue(float y0,float y1)  //根据两点一线计算出x,y的极限值
        {
                float k = (y0 - y1) / (Max_OrthographicSize - Min_OrthographicSize);
                float b = y0 - k * Max_OrthographicSize;
                return k * Camera.main.orthographicSize + b;
        }

        void OnMouseScroll()
        {
                WorldCameraHeight(Input.mousePosition, Input.GetAxis("Mouse ScrollWheel") * 1.2f, 0.0005f);
        }

        void OnClickDown()
        {
                m_vStartMousePos = Input.mousePosition;
                m_vStartCameraPos = Camera.main.transform.position;
        }

        void OnDarg()
        {
                float x = Mathf.Clamp(m_vStartCameraPos.x - (Input.mousePosition.x - m_vStartMousePos.x) * ratio,
                CountLimitValue(Max_OrthographicSize_Min_X, Min_OrthographicSize_Min_X), CountLimitValue(Max_OrthographicSize_Max_X, Min_OrthographicSize_Max_X));
                float y = Mathf.Clamp(m_vStartCameraPos.y - (Input.mousePosition.y - m_vStartMousePos.y) * ratio,
                        CountLimitValue(Max_OrthographicSize_Min_Y, Min_OrthographicSize_Min_Y), CountLimitValue(Max_OrthographicSize_Max_Y, Min_OrthographicSize_Max_Y));
                Camera.main.transform.position = new Vector3(x, y, Camera.main.transform.position.z);
        }

        void OnMultiTouchMove()
        {
                if (Input.GetTouch(0).phase == TouchPhase.Began || Input.GetTouch(1).phase == TouchPhase.Began)
                {
                        oldDistance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
                }

                if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved)
                {
                        Vector2 tempPosition1 = Input.GetTouch(0).position;
                        Vector2 tempPosition2 = Input.GetTouch(1).position;
                        Vector2 point_center = Vector2.Lerp(tempPosition1, tempPosition2, 0.5f);
                        float newDistance = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
                        WorldCameraHeight(point_center, GameUtility.EnlargeDistance(oldDistance,newDistance, ZoomSpeed), 0.0003f);
                        oldDistance = newDistance;
                }
        }

        void InputBinding()
        {
                InputManager.GetInst().UpdateInputReset(); //先清空代理
                InputManager.GetInst().SetbIgnoreUI(true);

                InputManager.GetInst().onClickDown = OnClickDown;
                InputManager.GetInst().onDarg = OnDarg;
                InputManager.GetInst().onMouseScroll = OnMouseScroll;
                InputManager.GetInst().onMultiTouchMove = OnMultiTouchMove;
        }
 
        void WorldCameraHeight(Vector2 point, float speed, float ratio)
        {
                float x_offset = Camera.main.ScreenToViewportPoint(point).x - 0.5f;
                float y_offset = Camera.main.ScreenToViewportPoint(point).y - 0.5f;
                float delta_x = 0;
                float delta_z = 0;
                delta_x = (x_offset * Screen.width) * ratio ;
                delta_z = (y_offset * Screen.height) * ratio;

                Vector3 offset = new Vector3(delta_x, delta_z, 0);
                Vector3 pos = Camera.main.transform.position;

                if (speed < 0)
                {
                        if (Camera.main.orthographicSize >= Max_OrthographicSize)
                        {
                                return;
                        }
                        pos -= offset;
                }
                if (speed > 0)
                {
                        if (Camera.main.orthographicSize <= Min_OrthographicSize)
                        {
                                return;
                        }
                        pos += offset;
                }

                Camera.main.orthographicSize -= speed;
                float x = Mathf.Clamp(pos.x, CountLimitValue(Max_OrthographicSize_Min_X, Min_OrthographicSize_Min_X), CountLimitValue(Max_OrthographicSize_Max_X, Min_OrthographicSize_Max_X));
                float y = Mathf.Clamp(pos.y, CountLimitValue(Max_OrthographicSize_Min_Y, Min_OrthographicSize_Min_Y), CountLimitValue(Max_OrthographicSize_Max_Y, Min_OrthographicSize_Max_Y));
                Camera.main.transform.position = new Vector3(x, y, Camera.main.transform.position.z);
        }

        public void CountUnlockArea()    //现在需求解锁的区域只和马车功能等级有关//
        {
                m_CanUnlockArea.Clear();
                int needCarriageLevel = 0;
                foreach (int id in worldAreaCfg.Keys)
                {
                        needCarriageLevel =  worldAreaCfg[id].need_level;
                        if (needCarriageLevel <= CarriageLevel + 1)  //多显示下一级需要出现的马车点//
                        {
                                if (!m_BrightArea.ContainsKey(id))
                                {
                                        m_CanUnlockArea.Add(id);
                                }
                        }
                }
        }

        long GetTeachRaidID(int areaid)
        {
                foreach (long id in m_TeachRaidDic.Keys)
                {
                        if (m_TeachRaidDic[id].area_id == areaid && m_TeachRaidDic[id].bOpen == 1)
                        {
                                return id;
                        }                       
                }
                return 0;
        }

        Vector3 CameraPos = Vector3.zero;
        public void LoadWorldMap()
        {
                UnityEngine.Object m_ob = ResourceManager.GetInst().Load("Scene/UI_NewWorld", AssetResidentType.Temporary); 
                WorldMapRoot = GameObject.Instantiate(m_ob) as GameObject;
                WorldMapRoot.transform.position = Vector3.zero;
                WorldMapRoot.transform.localEulerAngles = Vector3.zero;
                WorldCamera = Camera.main;
                if (CameraPos != Vector3.zero)
                {
                        WorldCamera.transform.position = CameraPos;
                }
                EffectRoot = new GameObject("EffectRoot");
                EffectRoot.transform.SetParent(WorldMapRoot.transform);
                EffectRoot.transform.position = Vector3.zero;
                EffectRoot.transform.localScale = Vector3.one;

                InputBinding();
                InitWorldPicData();
                RefreshWorldMap();
                GameStateManager.GetInst().GameState = GAMESTATE.HOME;
        }

        readonly byte MASK_ALPHA_MAX = 200;
        readonly byte MASK_ALPHA_MIN = 5;
        readonly byte MASK_LINE = 5;
        readonly string AREA_MARK = "area"; //约定的区域前缀
        readonly string DpString = "dp"; 

        int mask_size_x;
        int mask_size_y;

        struct PicStruct
        {
                public RectTransform rt;
                public Color32[] color;
        }

        Dictionary<int, PicStruct> AreaColorDic = new Dictionary<int, PicStruct>();
        Dictionary<int, Color32> OutLineColor = new Dictionary<int, Color32>();


        Transform PicRoor;

        void InitWorldPicData()
        {
                Transform earth_tf;
                RawImage floor;
                Transform area_tf;
                RawImage area_ri;
                PicStruct ps;
                int index = 0;

                earth_tf = WorldMapRoot.transform.Find("floor");
                floor = earth_tf.GetComponent<RawImage>();
                PicRoor = earth_tf.Find("pic");

                mask_size_x = (int)floor.rectTransform.sizeDelta.x;
                mask_size_y = (int)floor.rectTransform.sizeDelta.y;

                for (int i = 0; i < earth_tf.childCount; i++)  //默认规定名字为area
                {
                        area_tf = earth_tf.GetChild(i);
                        if (!area_tf.name.Contains(AREA_MARK))
                        {
                                continue;
                        }
                        area_ri = area_tf.GetComponent<RawImage>();

                        ps.rt = area_ri.rectTransform;
                        ps.color = (area_ri.texture as Texture2D).GetPixels32();
                        index = int.Parse(area_tf.name.Substring(AREA_MARK.Length));

                        AreaColorDic.Add(index, ps);
                        area_ri.enabled = false;
                        area_ri.texture = null;
                        UnityEngine.Object.Destroy(area_ri.texture);

                        Transform dp = null;

                        for (int j = 0; j < area_tf.childCount; j++)
                        {
                                Transform point = area_tf.GetChild(j);
                                if (point.name.Equals(DpString))
                                {
                                        dp = point;
                                        continue;
                                }

                                point.Find("name").GetComponent<Text>().font = UIManager.GetInst().MicrosoftFont;
                                point.Find("rest_time").GetComponent<Text>().font = UIManager.GetInst().MicrosoftFont;
                                point.SetActive(false);
                        }

                        if (dp != null)  //初始化dp进度条
                        {
                                Text[] text = dp.GetComponentsInChildren<Text>();
                                for (int j = 0; j < text.Length; j++)
                                {
                                        text[j].font = UIManager.GetInst().MicrosoftFont;
                                }
                                dp.name = DpString + index;
                                ResourceManager.GetInst().LoadIconSpriteSyn("Raid#jindu", dp.Find("bg"));
                                ResourceManager.GetInst().LoadIconSpriteSyn("Raid#dengji", dp.Find("icon"));
                                Transform progress = dp.Find("bg/progress");
                                ResourceManager.GetInst().LoadIconSpriteSyn("Raid#jindutiao", progress);

                                dp.SetParent(PicRoor);
                                dp.SetActive(false);
                        }

                }
                //Resources.UnloadUnusedAssets(); //回收原始图片//
        }

        void CleanPicData()
        {
                if (PicRoor != null)
                {
                        for (int i = 0; i < PicNum; i++)
                        {
                                RawImage image = PicRoor.GetChild(i).GetComponent<RawImage>();
                                if (image.texture != null)
                                {
                                        UnityEngine.Object.Destroy(image.texture);
                                }
                        }
                }

                for (int i = 0; i < canUnlockAreaMaskPics.Count; i++)
                {
                        if (canUnlockAreaMaskPics[i] != null)
                        {
                                UnityEngine.Object.Destroy(canUnlockAreaMaskPics[i]);
                        }
                }
                canUnlockAreaMaskPics.Clear();
                OutLineColor.Clear();
                AreaColorDic.Clear();
        }

        void SetMaskPic(int area_id, ref Color32[] m_color, bool is_origin,int picIndex) 
        {
                if (!CheckMapToXml(area_id))
                {
                        return;
                }
                PicStruct ps = AreaColorDic[area_id];
                CopyPixelToMap(ref ps, ref m_color, is_origin, picIndex);
        }

        void CreatWorldImage(ref Color32[] m_color,int picIndex) //最终生成地图
        {
                //画边界
                foreach (int index in OutLineColor.Keys)
                {
                        PaintOutLine(index, ref m_color);
                }

                RawImage image = PicRoor.GetChild(picIndex).GetComponent<RawImage>();
                if (image.texture != null)
                {
                        UnityEngine.Object.Destroy(image.texture);
                }
                
                int half_x = mask_size_x / 2;
                int half_y = mask_size_y / 2;

                Texture2D pic = new Texture2D(half_x, half_y, TextureFormat.ARGB32, false);  //生成地图
                pic.filterMode = FilterMode.Bilinear;
                pic.wrapMode = TextureWrapMode.Clamp;

                pic.SetPixels32(m_color);
                pic.Compress(true);
                pic.Apply(false, true);

                image.texture = pic;
                image.SetNativeSize();
                image.rectTransform.anchoredPosition = new Vector2(-half_x / 2 + (picIndex % 2) * half_x, -half_y / 2 + (picIndex / 2) * half_y);

                OutLineColor.Clear();
        }

        void DestroyContiguousMask(RawImage area_ri)
        {
                area_ri.enabled = false;
                area_ri.texture = null;

                UnityEngine.Object.Destroy(area_ri.GetComponent<CanvasGroup>());
                for (int i = 0; i < area_ri.transform.childCount; i++)
                {
                        Transform tf = area_ri.transform.GetChild(i);
                        UnityEngine.Object.Destroy(tf.GetComponent<CanvasGroup>());
                }
                UnityEngine.Object.Destroy(area_ri.texture);
        }

        void PaintOutLine(int index, ref Color32[] m_color)
        {
                bool need = false;
                int new_index = 0;

                int half_x = mask_size_x / 2;
                int half_y = mask_size_y / 2;
                for (int i = 0; i < 4; i++)  //检查上下左右
                {
                        if (i == 0)
                        {
                                new_index = index + half_x;
                                if (new_index < half_x * half_y)
                                {
                                        if (m_color[new_index].a <= MASK_LINE)
                                        {
                                                need = true;
                                                break;
                                        }
                                }
                        }
                        else if(i == 1)
                        {
                                new_index = index - half_x;
                                if (new_index >= 0)
                                {
                                        if (m_color[new_index].a <= MASK_LINE)
                                        {
                                                need = true;
                                                break;
                                        }
                                }
                        }
                        else if (i == 2)
                        {
                                if (index % half_x == 0)
                                {
                                        continue;
                                }
                                new_index = index - 1;
                                if (new_index >= 0)
                                {
                                        if (m_color[new_index].a <= MASK_LINE)
                                        {
                                                need = true;
                                                break;
                                        }
                                }
                        }
                        else
                        {
                                if ((index + 1) % half_x == 0)
                                {
                                        continue;
                                }
                                new_index = index + 1;
                                if (new_index < half_x * half_y)
                                {
                                        if (m_color[new_index].a <= MASK_LINE)
                                        {
                                                need = true;
                                                break;
                                        }
                                }
                        }               
                }
                if (need)
                {
                        m_color[index] = OutLineColor[index];
                }
        }

        Color32 maskColor;
        float maskTransparency;
        void CopyPixelToMap(ref PicStruct ps ,ref Color32[]m_color, bool is_origin,int picIndex)  
        {
                int mask_index = 0;
                int show_index = 0;
                Vector2 show_size = ps.rt.sizeDelta;
                Vector2 pos = ps.rt.anchoredPosition;

                int half_x = mask_size_x / 2;
                int half_y = mask_size_y / 2;

                for (int i = 0; i < (int)show_size.x; i++)
                {
                        for (int j = 0; j < (int)show_size.y; j++)
                        {
                                bool isMatch = false;
                                mask_index = ((int)pos.x + i) + ((int)pos.y + j) * mask_size_x;
                                if (mask_index >= mask_size_x * mask_size_y)
                                {
                                        Debuger.Log("区域超过遮罩大小！");
                                        continue;
                                }
                                show_index = i + j * (int)show_size.x;

                                int x = mask_index % mask_size_x;
                                int y = mask_index / mask_size_x;
                                if (picIndex == 0)
                                {
                                        if (x >= 0 && x < half_x && y >= 0 && y < half_y)
                                        {
                                                isMatch = true;
                                                mask_index = x + y * half_x;
                                        }
                                }
                                if (picIndex == 1)
                                {
                                        if (x >= half_x && x < mask_size_x && y >= 0 && y < half_y)
                                        {
                                                isMatch = true;
                                                mask_index = (x - half_x) + y * half_x;
                                        }
                                }
                                if (picIndex == 2)
                                {
                                        if (x >= 0 && x < half_x && y >= half_y && y < mask_size_y)
                                        {
                                                isMatch = true;
                                                mask_index = x + (y - half_y) * half_x;
                                        }
                                }
                                if (picIndex == 3)
                                {
                                        if (x >= half_x && x < mask_size_x && y >= half_y && y < mask_size_y)
                                        {
                                                isMatch = true;
                                                mask_index = (x - half_x) + (y - half_y) * half_x;
                                        }
                                }

                                if (isMatch)
                                {
                                        if (ps.color[show_index].a > MASK_ALPHA_MAX)
                                        {
                                                if (is_origin)
                                                {
                                                        m_color[mask_index] = ps.color[show_index];
                                                }
                                                else
                                                {
                                                        m_color[mask_index].r = GetNormalOverlyingColor(ps.color[show_index].r, maskColor.r);
                                                        m_color[mask_index].g = GetNormalOverlyingColor(ps.color[show_index].g, maskColor.g);
                                                        m_color[mask_index].b = GetNormalOverlyingColor(ps.color[show_index].b, maskColor.b);
                                                        m_color[mask_index].a = ps.color[show_index].a;
                                                }
                                        }
                                        else if (ps.color[show_index].a > MASK_ALPHA_MIN)  //整理出边缘像素//
                                        {
                                                //if (is_origin)
                                                {
                                                        if (!OutLineColor.ContainsKey(mask_index))
                                                        {
                                                                OutLineColor.Add(mask_index, ps.color[show_index]);
                                                        }
                                                        else
                                                        {
                                                                if (OutLineColor[mask_index].a < ps.color[show_index].a)
                                                                {
                                                                        OutLineColor[mask_index] = ps.color[show_index];
                                                                }
                                                        }
                                                }
                                        }
                                }
                        }
                }
        }

        
        byte GetNormalOverlyingColor(byte colorDown, byte colorUp)  //ps normal叠加算法
        {
                return (byte)(colorUp * maskTransparency + colorDown * (1 - maskTransparency));
        }

        public List<long> CalculateShowNodeByDp(int area, int dp) //根据dp值计算可以被显示的点
        {
                List<long> mNewNodeList = new List<long>();
                foreach (long id in m_RaidDic.Keys)
                {
                        NodeInfo info = m_RaidDic[id];
                        AreaSpotConfig asc = WorldMapManager.GetInst().GetAreaSpotCfg(info.area_spot);
                        if (asc != null)
                        {
                                if (area == info.area_id)
                                {
                                        if (dp >= asc.need_goal_point)
                                        {
                                                if (info.bShow == 0 && info.bOpen == 1)
                                                {
                                                        mNewNodeList.Add(id);
                                                }
                                        }
                                }
                        }
                }
                for (int i = 0; i < mNewNodeList.Count; i++)
                {
                        NodeInfo info = m_RaidDic[mNewNodeList[i]];
                        info.bShow = 1;
                        m_RaidDic[mNewNodeList[i]] = info;
                        ActivityRaidShow(ref info);
                }
                return mNewNodeList;
        }

        public Transform GetNodeButton(int area,int point)
        {
                if (AreaColorDic.ContainsKey(area))
                {
                        Transform area_tf = AreaColorDic[area].rt;
                        if (area_tf.childCount > point)
                        {
                                return area_tf.GetChild(point).GetChild(0);
                        }

                }
                return null;
        }

        public void FuckNewAnimation()
        {
                //打开地图就先赋值正确数据//
                if (mNeedAniAreaDic.Count > 0)
                {
                        Dictionary<int, List<long>> newOpenNode = new Dictionary<int, List<long>>();
                        List<NewAniInfo> mNeedAniInfo = new List<NewAniInfo>();
                        foreach (NewAniInfo info in mNeedAniAreaDic.Values)
                        {
                                NewAniInfo needInfo = info;
                                needInfo.mDp = m_BrightArea[info.mUpArea]; //存储oldDp
                                //dp赋值
                                m_BrightArea[info.mUpArea] = info.mDp;
                                //通过的副本赋值
                                if (m_RaidDic.ContainsKey(info.mNewUpNode.id))
                                {
                                        m_RaidDic[info.mNewUpNode.id] = info.mNewUpNode;
                                }
                                List<long> mNewNodeList = CalculateShowNodeByDp(info.mUpArea, info.mDp);
                                newOpenNode.Add(info.mUpArea, mNewNodeList);
                                mNeedAniInfo.Add(needInfo);
                        }
                        mNeedAniAreaDic.Clear();

                        //以上处理数据流程保证数据没有出错//
                        //以下进行傻逼动画表现
                        for (int i = 0; i < mNeedAniInfo.Count; i++ )
                        {
                                FuckAnimaCoroutineList.Add(AppMain.GetInst().StartCoroutine(FuckAnima(mNeedAniInfo[i], newOpenNode)));
                        }
                }
        }

        List<Coroutine> FuckAnimaCoroutineList = new List<Coroutine>();
        public void StopFuckAnimaCoroutine()
        {
                for (int i = 0; i < FuckAnimaCoroutineList.Count; i++)
                {
                        if (FuckAnimaCoroutineList[i] != null)
                        {
                                AppMain.GetInst().StopCoroutine(FuckAnimaCoroutineList[i]);
                        }
                }
        }

        IEnumerator FuckAnima(NewAniInfo info,Dictionary<int, List<long>> newRaidDict)
        {
                yield return new WaitForSeconds(0.5f);
                //1 如果有副本,亮星星 2 飞光电  
                if (info.mNewUpNode.id > 0)
                {
                        Transform area_tf = AreaColorDic[info.mUpArea].rt;
                        if (area_tf != null)
                        {
                                Transform stragroup = area_tf.GetChild(info.mNewUpNode.point).Find("stargroup");          //星级
                                Transform star = stragroup.GetChild((info.mNewUpNode.raid_id - 1) % 10);

                                if (star != null)
                                {
                                        NodeInfo new_info = m_RaidDic[info.mNewUpNode.id];
                                        CreatIcon(new_info.id + CommonString.underscoreStr + new_info.raid_id, AreaColorDic[new_info.area_id].rt, ref new_info);

                                        GameObject starEffect = FuckStar(star);
                                        if (starEffect != null)
                                        {
                                                yield return new WaitForSeconds(1.0f);
                                                GameObject.Destroy(starEffect);
                                        }


                                        GameObject ballEffect = FuckFlyLight(star, info.mUpArea,1.0f);
                                        if (ballEffect != null)
                                        {
                                                yield return new WaitForSeconds(1.0f);
                                                GameObject.Destroy(ballEffect);
                                        }                                     
                                }

                                IEnumerator fuckfuckfuck = ShowNewNodeAni(info, newRaidDict);
                                while (true)
                                {
                                        yield return fuckfuckfuck.Current;
                                        var moveNext = fuckfuckfuck.MoveNext();
                                        if (!moveNext)
                                        {
                                                break;
                                        }
                                }
                        }
                }
                else
                {
                        IEnumerator fuckfuckfuck = ShowNewNodeAni(info, newRaidDict);
                        while (true)
                        {
                                yield return fuckfuckfuck.Current;
                                var moveNext = fuckfuckfuck.MoveNext();
                                if (!moveNext)
                                {
                                        break;
                                }
                        }
                }
        }

        IEnumerator ShowNewNodeAni(NewAniInfo info, Dictionary<int, List<long>> newRaidDict)
        {
                IEnumerator fuckdp = FuckDp(info.mUpArea, info.mDp);
                while (true)
                {
                        yield return fuckdp.Current;
                        var moveNext = fuckdp.MoveNext();
                        if (!moveNext)
                        {
                                break;
                        }
                }
               
                foreach (int area in newRaidDict.Keys)
                {
                        IEnumerator fuckraid = FuckShowNewNode(area, newRaidDict[area]);
                        while (true)
                        {
                                yield return fuckraid.Current;
                                var moveNext = fuckraid.MoveNext();
                                if (!moveNext)
                                {
                                        break;
                                }
                        }
                }
        }

        GameObject FuckStar(Transform star)
        {
                GameObject starEffect = EffectManager.GetInst().PlayEffect("worldmap_star_light", star.transform);
                starEffect.transform.SetParent(EffectRoot.transform);
                starEffect.transform.localScale = Vector3.one;             
                return starEffect;
        }

        GameObject FuckFlyLight(Transform star, int area,float flyTime)
        {
                GameObject ballEffect = EffectManager.GetInst().PlayEffect("worldmap_star_move", star.transform);
                ballEffect.transform.SetParent(EffectRoot.transform);
                Transform dp = PicRoor.Find(DpString + area);
                ballEffect.transform.DOMove(dp.position, flyTime);
                return ballEffect;
        }

        IEnumerator FuckDp(int area_id, int oldDp)
        {
                Transform dp = PicRoor.Find(DpString + area_id);
                WorldMapAreaHold worldareacfg = WorldMapManager.GetInst().GetWorldMapAreaCfg(area_id);
                string[] unlock_progress = worldareacfg.dp_unlock_progress.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                int old_exp = 0;
                int old_max_exp = 0;
                int old_level = 0;

                GetExp(unlock_progress, worldareacfg.dp_max, oldDp, ref old_exp, ref old_max_exp, ref old_level);

                int nowDp = m_BrightArea[area_id];
                int exp = 0;
                int max_exp = 0;
                int level = 0;
                GetExp(unlock_progress, worldareacfg.dp_max, nowDp, ref exp, ref max_exp, ref level);

                Image progress = dp.Find("bg/progress").GetComponent<Image>();
                Text dpText = dp.Find("bg/Text").GetComponent<Text>();
                Text dpLevel = dp.Find("level").GetComponent<Text>();

                float duration = 1.0f;
                float time;
                int nowValue;

                if (old_level == level)  //没有超过当前最大值
                {
                        time = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - time <= duration)
                        {
                                nowValue = (int)Mathf.Lerp(old_exp, exp, (Time.realtimeSinceStartup - time) / duration);
                                progress.fillAmount = (float)nowValue / old_max_exp;
                                dpText.text = nowValue + " / " + old_max_exp;
                                yield return null;
                        }
                        dpText.text = exp + " / " + old_max_exp;
                        progress.fillAmount = (float)exp / old_max_exp;
                }
                else
                {
                        //先到最大值
                        time = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - time <= duration)
                        {
                                nowValue = (int)Mathf.Lerp(old_exp, old_max_exp, (Time.realtimeSinceStartup - time) / duration);
                                progress.fillAmount = (float)nowValue / old_max_exp;
                                dpText.text = nowValue + " / " + old_max_exp;
                                yield return null;
                        }
                        dpText.text = old_max_exp + " / " + old_max_exp;
                        progress.fillAmount = 1;

                        dpLevel.text = level.ToString();

                        time = Time.realtimeSinceStartup;
                        while (Time.realtimeSinceStartup - time <= duration) //
                        {
                                nowValue = (int)Mathf.Lerp(0, exp, (Time.realtimeSinceStartup - time) / duration);
                                progress.fillAmount = (float)nowValue / max_exp;
                                dpText.text = nowValue + " / " + max_exp;
                                yield return null;
                        }
                        dpText.text = exp + " / " + max_exp;
                        progress.fillAmount = (float)exp / max_exp; 
                }
        }


   
        IEnumerator FuckShowNewNode(int area,List<long> newList) //出现新点位
        {
                for (int i = 0; i < newList.Count; i++)
                {
                        NodeInfo info = m_RaidDic[newList[i]];
                        Transform icon = CreatIcon(info.id + CommonString.underscoreStr + info.raid_id,  AreaColorDic[area].rt, ref info,true);
                        CanvasGroup cg = icon.GetComponent<CanvasGroup>();
                        if (cg == null)
                        {
                                cg = icon.gameObject.AddComponent<CanvasGroup>();
                        }
                        cg.alpha = 0;
                        cg.DOFade(1, 1.0f);
                        yield return new WaitForSeconds(1.0f);
                }
        }

        public void RefreshWorldMap()
        {
                RefreshCanLockAreaPic();
                RefreshBright();
                RefreshHorsePoint();    
                AllRefreshDp(); //刷新dp条                       
                RefreshRaid();  //刷新副本
                FuckNewAnimation();  //播放新开区域动画
                CheckActivityTip();
        }

        readonly int PicNum = 4; //将大图分成四个小图避免一次性申请过大的内存//
        void RefreshBright()  //刷新亮的区域
        {
                foreach (int area_id in m_BrightArea.Keys)
                {                   
                        InitBrightArea(area_id);
                }

                int half_x = mask_size_x / 2;
                int half_y = mask_size_y / 2;
                Color32[] m_color = new Color32[half_x * half_y];  
                for (int i = 0; i < PicNum; i++)          //每次获取数组
                {
                        InitColor(ref m_color);

                        foreach (int area_id in m_BrightArea.Keys)
                        {
                                SetMaskPic(area_id, ref m_color, true, i);
                        }
                        CreatWorldImage(ref m_color, i);
                }

                foreach (int area_id in m_BrightArea.Keys) //开地图效果//
                {
                        RawImage area_ri = AreaColorDic[area_id].rt.GetComponent<RawImage>();
                        if (area_ri.texture != null)
                        {
                                area_ri.DOFade(0, 2.5f).OnComplete(() => DestroyContiguousMask(area_ri));
                        }
                }
                Resources.UnloadUnusedAssets();
        }

        void AllRefreshDp()
        {
                foreach (int area_id in m_BrightArea.Keys)
                {
                        RefreshDp(area_id);
                }
        }

        public void RefreshCanLockAreaPic()
        {
                foreach (int id in m_CanUnlockArea)
                {
                        //SetMaskPic(id, ref m_color, false, i);
                        CreatCanUnlockAreaPic(id);                        
                }
        }

        void InitColor(ref Color32[] m_color)
        {
                Color32 color = new Color32();
                for (int i = 0; i < m_color.Length; i++)
                {
                        m_color[i] = color;
                }
        }

        List<Texture2D> canUnlockAreaMaskPics = new List<Texture2D>();
        void CreatCanUnlockAreaPic(int id)
        {
                if (!AreaColorDic.ContainsKey(id))
                {
                        //Debuger.Log(id + "世界地图不存在");
                        return;
                }

                Color32[] m_color = new Color32[(int)AreaColorDic[id].rt.sizeDelta.x * (int)AreaColorDic[id].rt.sizeDelta.y];
                for (int i = 0; i < AreaColorDic[id].color.Length; i++)
                {
                        m_color[i].r = GetNormalOverlyingColor(AreaColorDic[id].color[i].r, maskColor.r);
                        m_color[i].g = GetNormalOverlyingColor(AreaColorDic[id].color[i].g, maskColor.g);
                        m_color[i].b = GetNormalOverlyingColor(AreaColorDic[id].color[i].b, maskColor.b);
                        m_color[i].a = AreaColorDic[id].color[i].a;
                }

                RawImage m_Image = AreaColorDic[id].rt.GetComponent<RawImage>();
                Texture2D m_pic = new Texture2D((int)AreaColorDic[id].rt.sizeDelta.x, (int)AreaColorDic[id].rt.sizeDelta.y, TextureFormat.ARGB32, false);  //生成地图

                m_pic.filterMode = FilterMode.Bilinear;
                m_pic.wrapMode = TextureWrapMode.Clamp;

                m_pic.SetPixels32(m_color);
                m_pic.Compress(true);
                m_pic.Apply(false, true);
                m_Image.texture = m_pic;
                m_Image.enabled = true;

                canUnlockAreaMaskPics.Add(m_pic);

                //解决点击问题
                CanvasGroup cg1 = m_Image.gameObject.GetComponent<CanvasGroup>();
                if (cg1 == null)
                {
                        cg1 = m_Image.gameObject.AddComponent<CanvasGroup>();
                }
                cg1.blocksRaycasts = false;
                for (int i = 0; i < m_Image.transform.childCount; i++)
                {
                        Transform tf = m_Image.transform.GetChild(i);
                        CanvasGroup cg = tf.gameObject.GetComponent<CanvasGroup>();
                        if(cg == null)
                        {
                                cg = tf.gameObject.AddComponent<CanvasGroup>();
                        }
                        cg.ignoreParentGroups = true;
                }
        }

        void InitBrightArea(int area_id)
        {
                if (!CheckMapToXml(area_id))
                {
                        return;
                }
                Transform area_tf = AreaColorDic[area_id].rt;
                if (area_tf != null)
                {
                        NodeInfo info = new NodeInfo();
                        info.area_id = area_id;
                        info.point = 0;
                        if (area_id == 1)  //创造出家园按钮
                        {
                                info.bShow = 1;
                                CreatIcon("home", area_tf,ref info);
                        }
                        else
                        {
                                info.bShow = 0;
                                CreatIcon("horse", area_tf,ref info); //关闭马车点
                        }
                }
        }

        void RefreshDp(int area_id)
        {
                Transform dp = PicRoor.Find(DpString + area_id);
                dp.SetActive(true);
                WorldMapAreaHold worldareacfg = WorldMapManager.GetInst().GetWorldMapAreaCfg(area_id);
                string[] unlock_progress = worldareacfg.dp_unlock_progress.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                int exp = 0;
                int max_exp = 0;
                int level = 0;

                GetExp(unlock_progress, worldareacfg.dp_max, m_BrightArea[area_id], ref exp, ref max_exp, ref level);

                Image progress = dp.Find("bg/progress").GetComponent<Image>();
                progress.fillAmount = (float)exp / max_exp;
                dp.Find("bg/Text").GetComponent<Text>().text = exp + " / " + max_exp;
                dp.Find("level").GetComponent<Text>().text = level.ToString();

        }


        void GetExp(string[] unlock_progress , int dp_max ,int nowdp, ref int exp , ref int max_exp, ref int level)
        {
                for (int i = 0; i < unlock_progress.Length; i++)
                {
                        int value = int.Parse(unlock_progress[i]);
                        if (nowdp < value)
                        {
                                if (i == 0)
                                {
                                        exp = nowdp;
                                        max_exp = value;
                                }
                                else
                                {
                                        exp = nowdp - int.Parse(unlock_progress[i - 1]);
                                        max_exp = value - int.Parse(unlock_progress[i - 1]);
                                }
                                level = i + 1;
                                break;
                        }
                        else
                        {
                                exp = nowdp - int.Parse(unlock_progress[i]);
                                max_exp = dp_max - int.Parse(unlock_progress[i]);
                                level = unlock_progress.Length + 1;
                        }
                }
        }

        bool CheckMapToXml(int area_id)
        {
                if (!AreaColorDic.ContainsKey(area_id))
                {
                        singleton.GetInst().ShowMessage(ErrorOwner.designer, area_id + "区域不存在");
                        return false;
                }
                return true;
        }

        Transform CreatIcon(string name, Transform parent,ref NodeInfo info,bool isnew = false)
        {
                Transform icon = null;
                int point_index = info.point;
                if (parent.childCount > point_index)
                {
                        icon = parent.GetChild(point_index);
                }
                else
                {
                        singleton.GetInst().ShowMessage(ErrorOwner.designer, "区域" + info.area_id + "不存在" + info.point + "点位放置");
                }
                
                if (icon != null)
                {
                        icon.gameObject.name = name;
                        RefreshPoint(icon, name, info.bShow,isnew);
                }
                return icon;
        }


        void CloseRaidIcon(int area_id, int point_index) //关闭一个点
        {
                if (!CheckMapToXml(area_id))
                {
                        return;
                }
                Transform area_tf = AreaColorDic[area_id].rt;
                NodeInfo info = new NodeInfo();
                CreatIcon("closed", area_tf, ref info);          
        }

        List<Text> ShowTimeTextList = new List<Text>();
        void RefreshPoint(Transform point, string raid_name, int bShow,bool isNew = false)
        {
                point.SetActive(false);
                if (point.childCount == 0)
                {
                        return;
                }

                Image house = point.Find("house").GetComponent<Image>();  //房子
                Text name = point.Find("name").GetComponent<Text>();   //名字
                Text text1 = point.Find("rest_time").GetComponent<Text>(); //倒计时
                Transform stragroup = point.Find("stargroup");          //星级
                Image flag = point.Find("flag").GetComponent<Image>();  //提示
                Image newflag = point.Find("new").GetComponent<Image>();  //是否是新

                EventTriggerListener listener = null;
                if (house != null)
                {
                        listener = EventTriggerListener.Get(house.gameObject);
                        house.enabled = false;
                        listener.onClick = null;
                        listener.SetTag(null);
                }

                if (name != null)
                {
                        name.text = string.Empty;
                }

                if (text1 != null)
                {
                        text1.text = string.Empty;
                }

                if (stragroup != null)
                {
                        stragroup.SetActive(false);
                }

                if (flag != null)
                {
                        flag.enabled = false;
                        flag.rectTransform.sizeDelta = Vector2.one * 50;
                }

                if (newflag != null)
                {
                        if (isNew)
                        {
                                ResourceManager.GetInst().LoadIconSpriteSyn("Raid#xin", newflag);
                        }
                        newflag.enabled = isNew;
                }

                string home_icon_url = "Raid#icon_house_12";
                string cart_icon_url = "Raid#icon_cart_point";

                if (raid_name.Contains("home"))    //家园             
                {
                        ResourceManager.GetInst().LoadIconSpriteSyn(home_icon_url, house.transform);
                        house.SetNativeSize();
                        name.text = "家园";
                        listener.onClick = OnClickHome;
                        point.SetActive(true);
                }
                else if (raid_name.Contains("horse"))
                {
                        ResourceManager.GetInst().LoadIconSpriteSyn(cart_icon_url, house.transform);
                        listener.onClick = OnClickHorse;
                        point.SetActive(true);
                        if (bShow == 1)
                        {
                                UIUtility.SetImageGray(false, house.transform);
                                string[] detail = raid_name.Split('_');
                                WorldMapAreaHold worldareacfg = WorldMapManager.GetInst().GetWorldMapAreaCfg(int.Parse(detail[1]));
                                if (worldareacfg.unlock_type == 1) //金币
                                {
                                        need_gold = worldareacfg.cost_gold;
                                        name.text = "花费" + worldareacfg.cost_gold + "金币解锁";
                                }
                        }
                        else if(bShow == -1)
                        {
                                UIUtility.SetImageGray(true, house.transform);
                                name.text = "需要建造传送门";
                        }
                        else
                        {
                                point.SetActive(false);
                        }
                }
                else if (raid_name.Contains("closed"))
                {
                            
                }
                else
                {
                        string[] detail = raid_name.Split('_');
                        long id = long.Parse(detail[0]);
                        int raid_id = int.Parse(detail[1]);

                        if (m_RaidDic[id].type == (int)RAID_TYPE.EVENT)  //事件点位
                        {
                                WorldmapEventConfig wec = GetWorldMapEventCfg(raid_id);
                                if (wec != null)
                                {
                                        if (bShow == 1)
                                        {
                                                ResourceManager.GetInst().LoadIconSpriteSyn(wec.icon, house.transform);
                                                house.SetNativeSize();
                                                name.text = LanguageManager.GetText(wec.name);
                                                listener.onClick = OnClickEventNode;
                                                listener.SetTag(wec);
                                                if (PlayerController.GetInst().GetPropertyInt("house_level") >= wec.need_level) //受家园等级限制
                                                {
                                                        UIUtility.SetImageGray(false, house.transform);
                                                }
                                                else
                                                {
                                                        UIUtility.SetImageGray(true, house.transform);
                                                }
                                                point.SetActive(true);
                                        }
                                }
                        }
                        else if (m_RaidDic[id].type == (int)RAID_TYPE.TASK)  //任务点位
                        {
                                WorldmapTaskConfig wtc = GetWorldMapTaskCfg(raid_id);
                                if (wtc != null)
                                {
                                        if (bShow == 1)
                                        {
                                                ResourceManager.GetInst().LoadIconSpriteSyn(wtc.icon, house.transform);
                                                house.SetNativeSize();
                                                name.text = LanguageManager.GetText(wtc.name);
                                                listener.onClick = OnClickTaskNode;
                                                listener.SetTag(raid_id);
                                                point.SetActive(true);
                                                TaskManager.GetInst().SetMapNpcIcon(raid_id, flag);
                                        }
                                }
                        }
                        else //pve迷宫
                        {
                                RaidInfoHold raid_info = null;
                                RaidMapHold raid_map = null;
                                int raidBaseId = 0;
                                int maxRaidId = 0;
                                int realRaidId = 0;
                                if (m_RaidDic[id].type == (int)RAID_TYPE.NORMAL)
                                {
                                        raidBaseId = raid_id - raid_id % 10;
                                        maxRaidId = RaidConfigManager.GetInst().GetRaidMaxDifficulty(raidBaseId);
                                        realRaidId = raid_id + 1 > maxRaidId ? maxRaidId : raid_id + 1;

                                        raid_info = RaidConfigManager.GetInst().GetRaidInfoCfg(realRaidId);
                                        raid_map = RaidConfigManager.GetInst().GetRaidMapCfg(realRaidId * 1000 + 1);
                                        listener.SetTag(realRaidId);

                                        stragroup.SetActive(true);
                                        RefreshRaidStar(stragroup, raid_id - raidBaseId, maxRaidId - raidBaseId);

                                }
                                else
                                {
                                        raid_info = RaidConfigManager.GetInst().GetRaidInfoCfg(raid_id);
                                        raid_map = RaidConfigManager.GetInst().GetRaidMapCfg(raid_id * 1000 + 1);
                                        listener.SetTag(raid_id);
                                }

                                if (raid_info != null)
                                {
                                        if (bShow == 1)
                                        {    
                                                ResourceManager.GetInst().LoadIconSpriteSyn(raid_info.house_icon, house.transform);
                                                house.SetNativeSize();
                                                name.text = LanguageManager.GetText(raid_map.name);
                                                if ((RAID_TYPE)raid_info.type == (RAID_TYPE.ENDLESS) || (RAID_TYPE)raid_info.type == (RAID_TYPE.STAGE))  //活动迷宫
                                                {
                                                        if (!ShowTimeTextList.Contains(text1))
                                                        {
                                                                ShowTimeTextList.Add(text1);
                                                        }
                                                }

                                                listener.onClick = OnClickRaid;
                                                if ((RAID_TYPE)raid_info.type == (RAID_TYPE.NORMAL))   //计算地图点位任务提示
                                                {
                                                        for (int i = raid_id - raid_id % 10 + 1; i <= realRaidId; i++)
                                                        {
                                                                RaidInfoHold cfg = RaidConfigManager.GetInst().GetRaidInfoCfg(i);
                                                                if (cfg != null)
                                                                {
                                                                        for (int j = 0; j < cfg.related_task.Count; j++)
                                                                        {
                                                                                TaskConfig taskCfg = TaskManager.GetInst().GetTaskCfg(cfg.related_task[j]);
                                                                                if (taskCfg != null && taskCfg.type != 2)
                                                                                {
                                                                                        if (TaskManager.GetInst().GetNpcTaskDic().ContainsKey(taskCfg.id) && !TaskManager.GetInst().GetTaskState(taskCfg.id))
                                                                                        {
                                                                                                ResourceManager.GetInst().LoadIconSpriteSyn("Raid#tanhao", flag.transform);
                                                                                                point.SetActive(true);
                                                                                                return;
                                                                                        }                                                                                        
                                                                                }
                                                                        }
                                                                }
                                                        }
                                                }                                         
                                                point.SetActive(true);
                                        }
                                        else
                                        {
                                                if (ShowTimeTextList.Contains(text1))
                                                {
                                                        ShowTimeTextList.Remove(text1);
                                                }
                                        }
                                }                   
                        }                                          
                }
        }

        void RefreshRaidStar(Transform root, int now_star, int max_star)
        {
                Transform star = root.GetChild(0);
                SetChildActive(root, false);
                for (int i = 0; i < max_star; i++)
                {
                        Transform m_star = null;
                        if (i < root.childCount)
                        {
                                m_star = root.GetChild(i);
                        }
                        else
                        {
                                m_star = GameObject.Instantiate(star) as Transform;
                                m_star.transform.SetParent(star.parent);
                                m_star.transform.localScale = Vector3.one;
                        }
                        m_star.SetActive(true);
                        if (i < now_star)
                        {
                                ResourceManager.GetInst().LoadIconSpriteSyn("Raid#star0", m_star);
                        }
                        else
                        {
                                ResourceManager.GetInst().LoadIconSpriteSyn("Raid#star1", m_star);
                        }
                }
        }


        void SetChildActive(Transform root, bool isShow)
        {
                for (int i = 0; i < root.childCount; i++)
                {
                        root.GetChild(i).SetActive(isShow);
                }
        }

        public void RefreshHorsePoint() //刷新可以解锁的马车点
        {
                WorldMapAreaHold wmh;
                for (int i = 0; i < m_CanUnlockArea.Count; i++)
                {
                        if (AreaColorDic.ContainsKey(m_CanUnlockArea[i]))
                        {
                                wmh = GetWorldMapAreaCfg(m_CanUnlockArea[i]);
                                Transform area = AreaColorDic[m_CanUnlockArea[i]].rt;
                                if (area != null && wmh != null)
                                {
                                        NodeInfo info = new NodeInfo();
                                        info.area_id = m_CanUnlockArea[i];
                                        info.point = 0;
                                        if (wmh.need_level > CarriageLevel)
                                        {
                                                info.bShow = -1;
                                        }
                                        else
                                        {
                                                info.bShow = 1;
                                        }
                                        CreatIcon("horse" + CommonString.underscoreStr + m_CanUnlockArea[i], area,ref info);
                                }                                
                        }
                }
        }

        public void RefreshRaid()
        {
                if (WorldMapRoot == null)
                {
                        return;
                }
                int area_id;
                Transform area_tf;
                foreach (long id in m_RaidDic.Keys)
                {
                        NodeInfo info = m_RaidDic[id];
                        area_id = info.area_id;
                        if (!CheckMapToXml(area_id))
                        {
                                return;
                        }
                        area_tf = AreaColorDic[area_id].rt;

                        if (area_tf != null)
                        {
                                CreatIcon(id + CommonString.underscoreStr + info.raid_id, area_tf, ref info);                                      
                        }
                }
        }

        public void UpdateOneRaid(long id, ref NodeInfo info)
        {
                int area_id = info.area_id;
                if (!CheckMapToXml(area_id))
                {
                        return;
                }
                Transform area_tf = AreaColorDic[area_id].rt;
                if (area_tf != null)
                {
                        CreatIcon(id + CommonString.underscoreStr + info.raid_id, area_tf, ref info);                                                                   
                }
        }

        long RaidRealID; //迷宫实例id
        void OnClickHome(GameObject go, PointerEventData data)
        {
                UIManager.GetInst().GetUIBehaviour<UI_WorldMap>().ReturnHome(); 
        }

        void OnClickEventNode(GameObject go, PointerEventData data)  //事件
        {
                WorldmapEventConfig wec = (WorldmapEventConfig)EventTriggerListener.Get(go).GetTag();
                if (PlayerController.GetInst().GetPropertyInt("house_level") < wec.need_level) //受家园等级限制
                {
                        GameUtility.PopupMessage("该事件需家园等级" + wec.need_level + "开启，先加油升级吧~");
                        return;
                }

                string name = go.transform.parent.name;
                string[] detail = name.Split('_');
                RaidRealID = long.Parse(detail[0]);
                UIManager.GetInst().ShowUI<UI_Dialog>("UI_Dialog").RefreshEvent(wec);       
        }

        void OnClickTaskNode(GameObject go, PointerEventData data)  //任务
        {
                int mapId = (int)EventTriggerListener.Get(go).GetTag();
                TaskManager.GetInst().ClickMapTask(mapId);
        }

        void OnClickHorse(GameObject go, PointerEventData data)
        {
                string name = go.transform.parent.name;
                string[] detail = name.Split('_');
                OnClickHorsePoint(int.Parse(detail[1]));
        }

        void OnClickRaid(GameObject go, PointerEventData data)
        {
                string name = go.transform.parent.name;
                string[] detail = name.Split('_');
                //raidID是转换过的
                int raidId = (int)EventTriggerListener.Get(go).GetTag();
                OnClickRaid(raidId);
                RaidRealID = long.Parse(detail[0]);
        }

        int need_gold;
        void OnClickHorsePoint(int area_id)
        {
                WorldMapAreaHold worldareacfg = WorldMapManager.GetInst().GetWorldMapAreaCfg(area_id);
                if (worldareacfg.need_level > CarriageLevel)
                {
                        GameUtility.PopupMessage(string.Format(LanguageManager.GetText("worldmap_area_open_notice_1"), worldareacfg.need_level));
                }
                else
                {
                        if (worldareacfg.unlock_type == 1) //金币
                        {
                                need_gold = worldareacfg.cost_gold;
                                UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox").SetConfirmAndCancel("提示", "是否花费" + need_gold + "金币解锁当前区域？", ConfirmCost, null, area_id);
                        }
                        if (worldareacfg.unlock_type == 2)//教学迷宫
                        {
                                int teach_id = worldareacfg.teach_raid_id;
                                UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox").SetConfirmAndCancel("提示", "是否进入教学迷宫", ConfirmTeachRaid, null, teach_id);
                                RaidRealID = GetTeachRaidID(area_id);
                        }
                }
        }

        void ConfirmCost(object data)
        {
                if (CommonDataManager.GetInst().GetNowResourceNum("gold") >= need_gold)
                {
                        WorldMapManager.GetInst().UnlockArea((int)data);
                }
                else
                {
                        GameUtility.PopupMessage("金币不足！");
                }
        }

        void ConfirmTeachRaid(object data)
        {
                ShowStoryRaid((int)data);
        }

        void OnClickRaid(int raid)
        {
                RaidInfoHold raid_info = RaidConfigManager.GetInst().GetRaidInfoCfg(raid);
                if ((RAID_TYPE)raid_info.type == (RAID_TYPE.ENDLESS))
                {
                        ShowEndlessTower(raid);
                }
                else
                {
                        ShowStoryRaid(raid);
                }
        }

        void ShowEndlessTower(int raid)
        {
                UIManager.GetInst().ShowUI<UI_EndlessTower>("UI_EndlessTower").Refresh(raid);
                UIManager.GetInst().CloseUI("UI_WorldMap");
        }

        void ShowStoryRaid(int raid)
        {
                UIManager.GetInst().ShowUI<UI_StoryRaid>("UI_StoryRaid").Refresh(raid);
                UIManager.GetInst().CloseUI("UI_WorldMap");
        }

        public void RefreshRestTime()  //迷宫剩余时间倒计时，到了通知服务端//
        {              
                ActivityCountDown();
        }

        float ShowActivityRest; //活动副本剩余刷新时间
        float ActivityRestTime;  //活动倒计时
        bool IsActivityRefresh = false;
        void ActivityCountDown()
        {
                if (IsActivityRefresh)
                {
                        ShowActivityRest = ActivityRestTime - Time.realtimeSinceStartup;
                        if (ShowActivityRest > 0)
                        {
                                if (isShow)
                                {
                                        for (int i = 0; i < ShowTimeTextList.Count; i++)
                                        {
                                                if (ShowTimeTextList[i] != null)
                                                {
                                                        if (i > 0)
                                                        {
                                                                ShowTimeTextList[i].text = ShowTimeTextList[0].text;
                                                        }
                                                        else
                                                        {
                                                                ShowTimeTextList[i].text = UIUtility.GetTimeString3((int)ShowActivityRest);
                                                        }
                                                }
                                        }
                                }
                        }
                        else
                        {
                                ActivityRestTimeReq();
                                IsActivityRefresh = false;
                        }

                }
        }
  
        #region 无尽之塔楼层数据

        Dictionary<int, int> m_RaidFloorInfo = new Dictionary<int, int>();
        void OnRaidFloorInfoTrap(object sender, SCNetMsgEventArgs e)
        {
                SCRaidMaxJumpFloorInfo msg = e.mNetMsg as SCRaidMaxJumpFloorInfo;
                string[] info = msg.raid_floor.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < info.Length; i++)
                {
                        int raid = int.Parse(info[i]) / 1000;
                        int floor = int.Parse(info[i]) % 1000;
                        if (floor <= 0)
                        {
                                floor = 1;
                        }
                        if (!m_RaidFloorInfo.ContainsKey(raid))
                        {
                                m_RaidFloorInfo.Add(raid, floor);
                        }
                        else
                        {
                                m_RaidFloorInfo[raid] = floor;
                        }

                }
        }
        public int GetRaidNowMaxFloor(int raid_id)
        {
                if (m_RaidFloorInfo.ContainsKey(raid_id))
                {
                        return m_RaidFloorInfo[raid_id];
                }
                return 1;    //说明没有打过
        }

        #endregion
}
