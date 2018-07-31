using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using Message;
using UnityEngine.EventSystems;
using Pathfinding;
using DG.Tweening;
using System.Linq;
using HighlightingSystem;

public struct BuildInfo
{
    public long id;                                         //实例Id
    public int buildId;                                    //buildtype表里的id
    public int level;                                       //建筑等级
    public Vector2 pos;                                     //服务器左下角位置
    public Vector2 posClient;                              //客户端位置 由于当前合并默认中心点为左下角所以 房间类都是相同的，存在不可旋转的问题 有空需要进行修改
    public int size_x;
    public int size_y;
    public int height;
    public Vector2 offset;
    public int face;                                        //朝向（由于客户端实现问题当前无效）
    public EBuildState eState;
    public int passTime;                                   //读条经过的时间
    public float clientTime;                               //客户端记录的时间//
    public BuildingInfoHold buildCfg;
    public EBuildType type;
    public long belongRoom;

    public int layout;


    public BuildInfo(SCMsgHouseBuildInfo msg) //家园建筑
    {
        id = msg.id;
        int x = msg.pos / 100;
        int z = msg.pos % 100;
        face = msg.face;
        pos = new Vector2(x, z);

        buildId = msg.bulid_id;
        level = msg.level;
        eState = (EBuildState)msg.state;
        passTime = msg.pass_time;
        clientTime = Time.realtimeSinceStartup;
        if (level == 0)
        {
            buildCfg = HomeManager.GetInst().GetBuildInfoCfg(buildId * 100 + 1);
        }
        else
        {
            buildCfg = HomeManager.GetInst().GetBuildInfoCfg(buildId * 100 + level);
        }

        BuildingTypeHold bth = HomeManager.GetInst().GetBuildTypeCfg(buildId);
        if (face == 1 || face == 3)
        {
            size_x = bth.size_y;
            size_y = bth.size_x;
        }
        else
        {
            size_x = bth.size_x;
            size_y = bth.size_y;
        }
        height = bth.height;
        type = (EBuildType)bth.function_type;

        offset = new Vector2(size_x * 0.5f - 0.5f, size_y * 0.5f - 0.5f);
        posClient = pos + offset;
        belongRoom = msg.belong_room;

        layout = 0;
    }

    public BuildInfo(int buildId, Vector3 centerPos)  //客户端建造
    {
        id = 0;
        face = 0;
        pos = new Vector2((int)centerPos.x, (int)centerPos.z);

        this.buildId = buildId;
        level = 1;
        eState = EBuildState.eWork;
        passTime = 0;
        clientTime = 0;
        buildCfg = HomeManager.GetInst().GetBuildInfoCfg(buildId * 100 + level);
        BuildingTypeHold bth = HomeManager.GetInst().GetBuildTypeCfg(buildId);
        size_x = bth.size_x;
        size_y = bth.size_y;
        height = bth.height;
        type = (EBuildType)bth.function_type;

        offset = new Vector2(size_x * 0.5f - 0.5f, size_y * 0.5f - 0.5f);
        posClient = pos + offset;
        belongRoom = 0;

        layout = 0;
    }

    public BuildInfo(long m_id, int m_pos, int m_build_id, int level, int layout) //村庄建筑(不知道要怎么弄了)
    {
        id = m_id;
        int x = m_pos / 100;
        int z = m_pos % 100;
        face = 0;
        posClient = pos = new Vector2(x, z);

        buildId = m_build_id;
        this.level = level;

        BuildingTypeHold bth = HomeManager.GetInst().GetBuildTypeCfg(buildId);
        size_x = bth.size_x;
        size_y = bth.size_y;
        height = 1;
        eState = EBuildState.eWork;
        passTime = 0;
        clientTime = 0;
        buildCfg = HomeManager.GetInst().GetBuildInfoCfg(buildId * 100 + level);
        offset = Vector2.zero;
        //以下信息暂时没用//
        type = (EBuildType)HomeManager.GetInst().GetBuildTypeCfg(buildId).function_type;
        belongRoom = 0;

        this.layout = layout;
    }

}

public enum HomeState
{
    None,                  //正常
    Move,                  //移动
    FurnitureCreat,        //家具建造
    BrickEditor,           //砖块编辑   
    ShotCamera,            //近景
}


public struct Treasure   //宝藏
{
    public int id;
    public int x;
    public int y;
    public int height;
    public int cfgId;
    public bool isCleaned; //是否清理完成

    public Treasure(int id, int cfgId)
    {
        this.id = id;
        this.cfgId = cfgId;
        isCleaned = false;
        height = id % 100;
        int index = (id / 100);
        x = index % HomeManager.HomeSize;
        y = index / HomeManager.HomeSize;
    }
}


public struct Room   //房间信息
{
    public long id;
    public int idsuit;      //激活的套装
    public List<long> buildInRoom; //房间里面的只能成为套装的家具
    public HashSet<long> wallAndDoor;  //墙和门

    public Room(SCMsgHouseBuildRoomInfo msg)
    {
        id = msg.idroom;
        idsuit = msg.idsuit;
        buildInRoom = new List<long>();
        if (GameUtility.IsStringValid(msg.allbuild))
        {
            string[] temp = msg.allbuild.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < temp.Length; i++)
            {
                buildInRoom.Add(long.Parse(temp[i]));
            }
        }

        wallAndDoor = new HashSet<long>();
        if (GameUtility.IsStringValid(msg.alldoor_wall))
        {
            string[] temp = msg.alldoor_wall.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < temp.Length; i++)
            {
                wallAndDoor.Add(long.Parse(temp[i]));
            }
        }
    }
}

public class HomeManager : SingletonObject<HomeManager>
{
    #region 建筑物基础

    public int MainBuildLevel //旗帜等级
    {
        get
        {
            long id = PlayerController.GetInst().PlayerID * 1000; //主城建筑id
            if (m_BuildDic.ContainsKey(id))
            {
                return m_BuildDic[id].level;
            }
            return 0;
        }
    }

    public int OpenArea
    {
        get
        {
            return PlayerController.GetInst().GetPropertyInt("open_area");
        }
    }

    public void Init()
    {
        InitData();        //初始化一些公共配置数据
        InitBuild();
        InitMake();        //制作
        InitGain();        //产出类建筑
        InitHotel();       //驿站
        InitCurePressure();//压力治疗
        InitCheckIn();     //入住
        InitHomeHeight();  //家园高度
        InitTreasure();    //宝藏       
    }

    void InitData()
    {
        moveRatio = GlobalParams.GetFloat("screen_move_speed");
        zoomSpeed = GlobalParams.GetFloat("screen_zoom_speed");
    }

    /// <summary>
    /// 当前版本 家具和建筑是一个概念统一变为 可操作实体
    /// </summary>

    Dictionary<long, BuildInfo> m_BuildDic = new Dictionary<long, BuildInfo>(); //服务器给的建筑数据//
    Dictionary<long, BuildBaseBehaviour> m_BuildBehaviourDic = new Dictionary<long, BuildBaseBehaviour>();
    Dictionary<long, Room> m_RoomDic = new Dictionary<long, Room>();  //房间数据//

    Dictionary<int, BuildingInfoHold> m_BuildInfoDict = new Dictionary<int, BuildingInfoHold>(); //表里的建筑数据信息

    Dictionary<int, FurnitureSetConfig> m_FurnitureSetDict = new Dictionary<int, FurnitureSetConfig>();          //家具套装
    List<int> m_SortFurnitureSetId = new List<int>();   //套装生效规则 等级高 id大 ,方便查找先进行排序

    Dictionary<int, BuildingLimitHold> m_BuildLimitDict = new Dictionary<int, BuildingLimitHold>(); //功能建筑数量等级限制表
    Dictionary<int, BuildFunctionHold> m_BuildFunctionDict = new Dictionary<int, BuildFunctionHold>();
    Dictionary<int, BuildingTypeHold> m_BuildTypeDict = new Dictionary<int, BuildingTypeHold>();
    Dictionary<int, BuildingDesignConfig> m_MainBuildDict = new Dictionary<int, BuildingDesignConfig>();//设计室表 
    Dictionary<int, BuildingBookshelvesConfig> m_BookShelvesDict = new Dictionary<int, BuildingBookshelvesConfig>();//设计室表 
    Dictionary<int, BuildNodeConfig> m_BuildNodesConfigDict = new Dictionary<int, BuildNodeConfig>(); //房间造型
    Dictionary<int, RoomLayoutConfig> m_RoomLayoutDict = new Dictionary<int, RoomLayoutConfig>(); //房间布局


    void InitBuild()
    {
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgHouseBuildInfo), OnGetHomeBuildingInfo);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgHouseBuildRoomInfo), OnGetHomeRoom);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgNeedObstaclePos), OnNeedObstaclePos);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgHouseBuildInfoEnd), OnGetHomeBuildingEnd);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgHouseBuildClear), OnGetHomeBuildingClear);

        ConfigHoldUtility<BuildingInfoHold>.LoadXml("Config/building_furniture", m_BuildInfoDict);
        ConfigHoldUtility<FurnitureSetConfig>.LoadXml("Config/furniture_set", m_FurnitureSetDict);
        ConfigHoldUtility<BuildingLimitHold>.LoadXml("Config/building", m_BuildLimitDict);

        ConfigHoldUtility<BuildingDesignConfig>.LoadXml("Config/building_design", m_MainBuildDict);
        ConfigHoldUtility<BuildFunctionHold>.LoadXml("Config/building_function", m_BuildFunctionDict);
        ConfigHoldUtility<BuildingTypeHold>.LoadXml("Config/building_type", m_BuildTypeDict);
        ConfigHoldUtility<BuildingBookshelvesConfig>.LoadXml("Config/building_bookshelves", m_BookShelvesDict);
        //房间造型先留着//
        ConfigHoldUtility<BuildNodeConfig>.LoadXml("Config/building_nodes", m_BuildNodesConfigDict);
        ConfigHoldUtility<RoomLayoutConfig>.LoadXml("Config/room_layout", m_RoomLayoutDict);

        m_SortFurnitureSetId = m_FurnitureSetDict.Keys.ToList();
        m_SortFurnitureSetId.Sort(CompareFurnitureSet);
    }

    protected int CompareFurnitureSet(int ida, int idb) //取高等级id大的
    {
        FurnitureSetConfig seta = m_FurnitureSetDict[ida];
        FurnitureSetConfig setb = m_FurnitureSetDict[idb];

        if (null == seta || null == setb)
        {
            return -1;
        }
        int levela = seta.level;
        int levelb = setb.level;
        if (levela != levelb)
        {
            return levelb - levela;
        }
        return idb - ida;
    }

    public List<int> GetAllFurnitureSuitId()
    {
        List<int> list = new List<int>(m_SortFurnitureSetId);
        list.Reverse();
        return list;
    }

    public BuildingInfoHold GetBuildInfoCfg(int id)
    {
        if (m_BuildInfoDict.ContainsKey(id))
        {
            return m_BuildInfoDict[id];
        }
        else
        {
            //if (id > 0)
            //{
            //        singleton.GetInst().ShowMessage("building_furniture表中不存在" + id);
            //}
        }
        return null;
    }

    public List<long> GetBuildTypeInHome(int buildId)//查找家园某种建筑的数量 
    {
        List<long> builds = new List<long>();
        foreach (BuildInfo info in m_BuildDic.Values)
        {
            if (info.buildId == buildId)
            {
                builds.Add(info.id);
            }
        }
        return builds;
    }

    public List<long> GetBuildTypeInHome(int buildId, long id)//查找家园某种建筑的数量,排除id
    {
        List<long> builds = new List<long>();
        foreach (BuildInfo info in m_BuildDic.Values)
        {
            if (info.buildId == buildId)
            {
                if (info.id != id)
                {
                    builds.Add(info.id);
                }
            }
        }
        return builds;
    }


    public List<long> GetBuildTypeInHome(EBuildType type)//查找家园某种建筑的数量
    {
        List<long> builds = new List<long>();
        foreach (BuildInfo info in m_BuildDic.Values)
        {
            if (info.type == type)
            {
                builds.Add(info.id);
            }
        }
        return builds;
    }

    public FurnitureSetConfig GetFurnitureSetCfg(int id)
    {
        if (m_FurnitureSetDict.ContainsKey(id))
        {
            return m_FurnitureSetDict[id];
        }
        return null;
    }

    public Dictionary<int, int> GetSuitFurniture(string furnitures)
    {
        Dictionary<int, int> dict = new Dictionary<int, int>();
        string[] temp = furnitures.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < temp.Length; i++)
        {
            int id = int.Parse(temp[i]);
            if (dict.ContainsKey(id))
            {
                dict[id] += 1;
            }
            else
            {
                dict.Add(id, 1);
            }
        }
        return dict;
    }

    public List<int> GetBuildFurnitureList(int type, int needLevel = 0)
    {
        List<int> buildList = new List<int>();
        string builds = CommonDataManager.GetInst().GetExtraBuildStr();  //额外解锁的排在前面
        for (int i = 1; i <= m_MainBuildDict.Count; i++)
        {
            if (needLevel <= 0 || i <= needLevel)
            {
                BuildingDesignConfig bdc = GetMainBuildCfg(i);
                builds += bdc.unlock_furniture_list + ",";
            }
        }
        string[] build = builds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < build.Length; i++)
        {
            int id = int.Parse(build[i]);
            BuildingTypeHold bt = GetBuildTypeCfg(id);
            if (bt != null)
            {
                if (bt.page_in_list == type)
                {
                    buildList.Add(id);
                }
            }
        }
        return buildList;
    }

    public int GetNowUseCapacity()
    {
        int capacity = 0;
        foreach (BuildInfo info in m_BuildDic.Values)
        {
            BuildingTypeHold bt = GetBuildTypeCfg(info.buildId);
            if (bt != null)
            {
                capacity += bt.take_capacity;
            }
        }
        return capacity;
    }

    public BuildingBookshelvesConfig GetBookShelvesCfg(int furnitureId)
    {
        if (m_BookShelvesDict.ContainsKey(furnitureId))
        {
            return m_BookShelvesDict[furnitureId];
        }
        return null;
    }

    public BuildingLimitHold GetBuildingLimitCfg(int build_id)
    {
        int level = 0;
        if (m_BuildTypeDict[build_id].function_type == (int)EBuildType.eMain) //主城等级受家园等级控制
        {
            level = PlayerController.GetInst().GetPropertyInt("house_level");
        }
        else                                      //其他建筑等级受主城等级控制
        {
            level = MainBuildLevel;
        }
        return CheckBuildCFg(level, build_id);
    }

    BuildingLimitHold CheckBuildCFg(int level, int build_id) //向低等级兼容
    {
        for (int i = level; i >= 0; i--)
        {
            int id = i * 10000 + build_id;
            if (m_BuildLimitDict.ContainsKey(id))
            {
                return m_BuildLimitDict[id];
            }
        }
        return null;
    }

    public int GetBuildMinLimitLevel(int build)
    {
        foreach (int level in m_MainBuildDict.Keys)
        {
            HashSet<int> set = GameUtility.ToHashSet<int>(m_MainBuildDict[level].unlock_furniture_list, ',', (x) => int.Parse(x));
            if (set.Contains(build))
            {
                return level;
            }
        }
        return 0;
    }

    public int GetBuildAllLimit()
    {
        if (m_MainBuildDict.ContainsKey(MainBuildLevel))
        {
            return m_MainBuildDict[MainBuildLevel].overall_capacity_limit;
        }
        return 0;
    }

    public string GetBuildFunctionIcon(int id)
    {
        if (m_BuildFunctionDict.ContainsKey(id))
        {
            return m_BuildFunctionDict[id].icon;
        }
        return string.Empty;
    }

    public BuildingTypeHold GetBuildTypeCfg(int build_id)
    {
        if (m_BuildTypeDict.ContainsKey(build_id))
        {
            return m_BuildTypeDict[build_id];
        }
        return null;
    }

    public BuildingDesignConfig GetMainBuildCfg(int level)
    {
        if (m_MainBuildDict.ContainsKey(level))
        {
            return m_MainBuildDict[level];
        }
        return null;
    }

    public BuildNodeConfig GetBuildNodesConfig(int id)
    {
        if (m_BuildNodesConfigDict.ContainsKey(id))
        {
            return m_BuildNodesConfigDict[id];

        }
        return null;
    }

    public RoomLayoutConfig GetRoomLayoutConfig(int id)
    {
        if (m_RoomLayoutDict.ContainsKey(id))
        {
            return m_RoomLayoutDict[id];

        }
        return null;
    }

    void OnGetHomeBuildingClear(object sender, SCNetMsgEventArgs e)
    {
        SCMsgHouseBuildClear msg = e.mNetMsg as SCMsgHouseBuildClear;
        CleanBuild(msg.id);
        GameUtility.ReScanPath();
    }

    public void CleanBuild(long id)  //清理建筑物
    {
        if (!m_BuildBehaviourDic.ContainsKey(id))
        {
            return;
        }

        if (m_BuildDic.ContainsKey(id))   //只有服务器建筑数据
        {
            int x = (int)m_BuildDic[id].pos.x;
            int y = (int)m_BuildDic[id].pos.y;
            if (SquarePointList.Contains(y * HomeSize + x))  //广场
            {
                SetHomePoint(m_BuildDic[id].pos, m_BuildDic[id].size_x, m_BuildDic[id].size_y, 3);
            }
            else
            {
                SetHomePoint(m_BuildDic[id].pos, m_BuildDic[id].size_x, m_BuildDic[id].size_y, 0);
            }

            if (m_BuildDic[id].type == EBuildType.eDoor || m_BuildDic[id].type == EBuildType.eWall)
            {
                SetHomeWallDoorPoint(m_BuildDic[id], 0);
                if (m_BuildDic[id].belongRoom != 0)
                {
                    DeleteRoom(m_BuildDic[id].belongRoom);
                }

            }
            else  //处理家具拆除
            {
                if (m_BuildDic[id].belongRoom != 0)
                {
                    RemoveRoomFurniture(m_BuildDic[id].belongRoom, m_BuildDic[id].id);
                }
            }

            m_BuildDic.Remove(id);
        }

        if (m_ProduceTime.ContainsKey(id))
        {
            m_ProduceTime.Remove(id);
        }

        if (BenchInfoDic.ContainsKey(id))
        {
            BenchInfoDic.Remove(id);
        }

        DeleteBuilding(id);
        SetSelectBuild(null);
    }

    void DeleteBuilding(long id)
    {
        m_BuildBehaviourDic[id].DeleteBuildingObj();
    }

    Vector3 GetContinousWallPos(BuildInfo bi)
    {
        int y = (int)GetScreenCenterPos().y;
        int x = (int)bi.pos.x;
        int z = (int)bi.pos.y;

        if (IsWallLink(x, z + 1))
        {
            return new Vector3(x, y, z - 1);
        }
        if (IsWallLink(x + 1, z))
        {
            return new Vector3(x - 1, y, z);
        }
        if (IsWallLink(x, z - 1))
        {
            return new Vector3(x, y, z + 1);
        }

        return new Vector3(x + 1, y, z);
    }

    void OnGetHomeBuildingInfo(object sender, SCNetMsgEventArgs e)
    {
        SCMsgHouseBuildInfo msg = e.mNetMsg as SCMsgHouseBuildInfo;
        BuildInfo new_bi = new BuildInfo(msg);

        if (m_BuildDic.ContainsKey(msg.id))
        {
            BuildInfo old_bi = m_BuildDic[msg.id];
            m_BuildDic[msg.id] = new_bi;

            if (new_bi.pos != old_bi.pos || new_bi.face != old_bi.face) //说明是改变了位置
            {
                SetHomePoint(old_bi.pos, old_bi.size_x, old_bi.size_y, 0);
                SetHomePoint(new_bi.pos, new_bi.size_x, new_bi.size_y, 1);

                if (old_bi.type == EBuildType.eWall || old_bi.type == EBuildType.eDoor) //墙和门
                {
                    SetHomeWallDoorPoint(old_bi, 0);
                    SetHomeWallDoorPoint(new_bi, msg.id);
                    if (old_bi.belongRoom != 0) //房间拆除//
                    {
                        DeleteRoom(old_bi.belongRoom);
                    }
                    CheckCanBeRoom();
                }
                else   //家具//
                {
                    long roomId = CheckFurnitureInRoom(new_bi.pos);
                    if (roomId != new_bi.belongRoom)  //通知服务器家具归属改变
                    {
                        SendBuildMove(new_bi.id, new_bi.pos, new_bi.face, roomId);  //通知服务器家具归属地改变
                        RemoveRoomFurniture(new_bi.belongRoom, new_bi.id);
                        AddRoomFurniture(roomId, new_bi.id);
                    }
                }
            }
            else
            {
                if (old_bi.type == EBuildType.eWall || old_bi.type == EBuildType.eDoor)
                {
                    if (old_bi.level == 0)
                    {
                        CheckCanBeRoom();
                    }
                }
                else
                {
                    if (old_bi.level == 0)
                    {
                        long roomId = CheckFurnitureInRoom(new_bi.pos);
                        if (roomId != new_bi.belongRoom)  //通知服务器家具归属改变
                        {
                            SendBuildMove(new_bi.id, new_bi.pos, new_bi.face, roomId);  //通知服务器家具归属地改变
                            AddRoomFurniture(roomId, new_bi.id);
                        }
                    }
                }
            }
            if (HomeRoot != null)
            {
                if (m_BuildBehaviourDic.ContainsKey(msg.id))
                {
                    m_BuildBehaviourDic[msg.id].UpdateBuildInfo(new_bi);
                }
            }
        }
        else
        {
            m_BuildDic.Add(msg.id, new_bi);
            if (HomeRoot != null)
            {
                if (mClientObj != null)
                {
                    QuitCreatMode();
                    SetBuild(new_bi);
                    if (new_bi.type == EBuildType.eWall)
                    {
                        Vector3 pos = GetContinousWallPos(new_bi);
                        EnterCreatMode(new_bi.buildId, pos);
                    }
                }
            }
            //存储位置方便查询
            SetHomePoint(new_bi.pos, new_bi.size_x, new_bi.size_y, 1);
            if (new_bi.type == EBuildType.eWall || new_bi.type == EBuildType.eDoor)
            {
                SetHomeWallDoorPoint(new_bi, msg.id);
            }
        }

        //隐藏的圣火房间不算建筑摆放//
        if (new_bi.type == EBuildType.eFire)
        {
            BuildingFireRoomConfig fire = GetBuildingFireRoom(new_bi.buildId);
            if (fire != null)
            {
                if (fire.unlock_area - 1 > OpenArea)
                {
                    SetHomePoint(new_bi.pos, new_bi.size_x, new_bi.size_y, 0);
                }
            }
        }
    }

    void OnGetHomeRoom(object sender, SCNetMsgEventArgs e)
    {
        SCMsgHouseBuildRoomInfo msg = e.mNetMsg as SCMsgHouseBuildRoomInfo;
        Room mRoom = new Room(msg);

        if (m_RoomDic.ContainsKey(msg.idroom))
        {
            m_RoomDic[msg.idroom] = mRoom;
        }
        else
        {
            m_RoomDic.Add(msg.idroom, mRoom);
        }

        if (HomeRoot != null) //新形成了房间 给房间里的家具绑定房间号
        {
            foreach (long id in mRoom.wallAndDoor)
            {
                if (m_BuildDic.ContainsKey(id))
                {
                    BuildInfo info = m_BuildDic[id];
                    info.belongRoom = mRoom.id;
                    m_BuildDic[id] = info;
                }
            }

            for (int i = 0; i < mRoom.buildInRoom.Count; i++)
            {
                if (m_BuildDic.ContainsKey(mRoom.buildInRoom[i]))
                {
                    BuildInfo info = m_BuildDic[mRoom.buildInRoom[i]];
                    info.belongRoom = mRoom.id;
                    m_BuildDic[mRoom.buildInRoom[i]] = info;
                }
            }

            GameUtility.PopupMessage("房间组成！");
            //检测新生成的房间内是否有套装//
            int suitId = CheckSuitInRoom(mRoom.buildInRoom);
            if (suitId > 0)
            {
                SendRoomChangeSuit(mRoom.id, suitId);
                FurnitureSetConfig cfg = GetFurnitureSetCfg(suitId);
                if (cfg != null)
                {
                    GameUtility.PopupMessage("组成套装" + LanguageManager.GetText(cfg.name));
                }

                mRoom.idsuit = suitId;
                m_RoomDic[msg.idroom] = mRoom;
            }
        }
    }

    void OnNeedObstaclePos(object sender, SCNetMsgEventArgs e)
    {
        SCMsgNeedObstaclePos msg = e.mNetMsg as SCMsgNeedObstaclePos;
        int count = msg.count;  //服务器需要的点数
        List<int> EmptyPointList = GetEmptyPoint();
        int pos;
        int x, y;
        for (int i = 0; i < count; i++)
        {
            if (EmptyPointList.Count > 0)
            {
                int random_index = UnityEngine.Random.Range(0, EmptyPointList.Count);
                pos = EmptyPointList[random_index];
                EmptyPointList.RemoveAt(random_index);
                x = pos % HomeSize;
                y = pos / HomeSize;
                SendObstacle(x, y);
                SetHomePoint(new Vector2(x, y), 1, 1, 1);
            }
            else
            {
                break;
            }
        }
    }

    void OnGetHomeBuildingEnd(object sender, SCNetMsgEventArgs e)
    {
        //由于异常关服或者其他bug导致 建筑数据上存储了不存在的房间号。 客户端对服务器数据进行重新删选//
        List<long> list = new List<long>();
        foreach (long id in m_BuildDic.Keys)
        {
            long belongRoom = m_BuildDic[id].belongRoom;
            if (belongRoom != 0)
            {
                if (!m_RoomDic.ContainsKey(belongRoom))
                {
                    list.Add(id);

                }
            }
        }
        for (int i = 0; i < list.Count; i++)
        {
            BuildInfo info = m_BuildDic[list[i]];
            info.belongRoom = 0;
            m_BuildDic[list[i]] = info;
        }
        GameStateManager.GetInst().EnterGame();
    }

    public void SendCreatBuild(int build_id, int pos)
    {
        CSMsgHouseBuildCreate msg = new CSMsgHouseBuildCreate();
        msg.bulid_id = build_id;
        msg.pos = pos;
        msg.face = 0;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }

    public void SendBuildMove(long id, Vector2 pos, int face, long belongRoom)
    {
        CSMsgHouseBuildMove msg = new CSMsgHouseBuildMove();
        msg.id = id;
        msg.pos = (int)pos.x * 100 + (int)pos.y;
        msg.face = face;
        msg.idRoom = belongRoom;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }

    public void SendBuildMove(long id, Vector2 pos)
    {
        SendBuildMove(id, pos, m_BuildDic[id].face, m_BuildDic[id].belongRoom);
    }

    public void SendCleanBuild()
    {
        if (m_SelectBuild != null)
        {
            CSMsgHouseBuildClear msg = new CSMsgHouseBuildClear();
            msg.id = m_SelectBuild.mBuildInfo.id;
            NetworkManager.GetInst().SendMsgToServer(msg);
        }
    }

    public void SendUpgradeBuild()
    {
        if (m_SelectBuild != null)
        {
            CSMsgHouseBuildUpgrade msg = new CSMsgHouseBuildUpgrade();
            msg.id = m_SelectBuild.mBuildInfo.id;
            NetworkManager.GetInst().SendMsgToServer(msg);
        }
    }

    bool CheckCanRotate(BuildInfo bi)
    {
        int x = (int)bi.pos.x;
        int y = (int)bi.pos.y;

        List<int> currPointList = new List<int>();

        for (int i = y; i < y + bi.size_y; i++)
        {
            for (int j = x; j < x + bi.size_x; j++)
            {
                if (j < 0 || j >= HomeSize || i < 0 || i >= HomeSize)
                {
                    continue;
                }
                currPointList.Add(j * 100 + i);
            }
        }
        List<int> resultPointList = new List<int>();
        for (int i = y; i < y + bi.size_x; i++)
        {
            for (int j = x; j < x + bi.size_y; j++)
            {
                if (j < 0 || j >= HomeSize || i < 0 || i >= HomeSize)
                {
                    continue;
                }
                if (currPointList.Contains(j * 100 + i))
                {
                    continue;
                }

                //如果是墙或者门类型是否安放的隔壁已经有两个相接了
                if (bi.type == EBuildType.eWall || bi.type == EBuildType.eDoor)
                {
                    if (CheckIsWallOrDoorAdjoin(j, i, bi.id))
                    {
                        GameUtility.PopupMessage("没有空间可以旋转！");
                        return false;
                    }
                }

                if (!IsBrickInOpenArea(j, i)) //区域不符合
                {
                    GameUtility.PopupMessage("没有空间可以旋转！");
                    return false;
                }

                if (HasAnyThingOnBrick(j, i))
                {
                    Debug.LogError("[" + j + "," + i + "] has already exist thing");
                    GameUtility.PopupMessage("没有空间可以旋转！");
                    return false;
                }
            }
        }
        return true;
    }

    public void RotateBuild()
    {
        if (m_SelectBuild != null)
        {
            if (m_SelectBuild.mBuildInfo.size_x != m_SelectBuild.mBuildInfo.size_y)
            {
                if (CheckCanRotate(m_SelectBuild.mBuildInfo) == false)
                {
                    return;
                }
            }

            CSMsgHouseBuildMove msg = new CSMsgHouseBuildMove();
            msg.id = m_SelectBuild.mBuildInfo.id;
            msg.pos = (int)m_SelectBuild.mBuildInfo.pos.x * 100 + (int)m_SelectBuild.mBuildInfo.pos.y;
            Debug.Log(m_SelectBuild.mBuildInfo.face);
            msg.face = (m_SelectBuild.mBuildInfo.face + 1) % 4;
            msg.idRoom = m_SelectBuild.mBuildInfo.belongRoom;
            NetworkManager.GetInst().SendMsgToServer(msg);
        }
    }

    public void SendUpgradeBuildCancel()
    {
        if (m_SelectBuild != null)
        {
            CSMsgHouseBuildUpgradeCancel msg = new CSMsgHouseBuildUpgradeCancel();
            msg.id = m_SelectBuild.mBuildInfo.id;
            NetworkManager.GetInst().SendMsgToServer(msg);
        }
    }

    public void SendUpgradeBuildQuick()
    {
        if (m_SelectBuild != null)
        {
            CSMsgBuildQuickUpdrage msg = new CSMsgBuildQuickUpdrage();
            msg.id = m_SelectBuild.mBuildInfo.id;
            NetworkManager.GetInst().SendMsgToServer(msg);
        }
    }

    public void SendBuildStateReq(long id)
    {
        CSMsgBuildStateReq msg = new CSMsgBuildStateReq();
        msg.id = id;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }

    public void SendObstacle(int x, int y)
    {
        CSMsgCreateObstacle msg = new CSMsgCreateObstacle();
        msg.x = x;
        msg.y = y;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }

    public BuildInfo GetBuildInfo(long id)
    {
        if (m_BuildDic.ContainsKey(id))
        {
            return m_BuildDic[id];
        }
        return new BuildInfo();
    }

    public void RemoveBuildingBehaviour(long id)
    {
        if (m_BuildBehaviourDic.ContainsKey(id))
        {
            m_BuildBehaviourDic.Remove(id);
        }
    }

    void SaveBuildingBehaviour(long id, BuildBaseBehaviour bb)
    {
        if (m_BuildBehaviourDic.ContainsKey(id))
        {
            m_BuildBehaviourDic[id] = bb;
        }
        else
        {
            m_BuildBehaviourDic.Add(id, bb);
        }
    }

    public BuildBaseBehaviour GetBuildingBehaviour(long id)
    {
        if (m_BuildBehaviourDic.ContainsKey(id))
        {
            return m_BuildBehaviourDic[id];
        }
        return null;
    }

    public void SetHomeActive(bool isactive)
    {
        if (HomeRoot != null)
        {
            HomeRoot.SetActive(isactive);
            HomeCamera.enabled = isactive;
            UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", isactive);

            GameStateManager.GetInst().GameState = GAMESTATE.HOME;
            if (isactive)
            {
                InputBinding();
                SetState(HomeState.None);
                SetAllObjMove(true);
                if (EnvironmentSequence != null)
                {
                    EnvironmentSequence.Play();
                }
                GameUtility.ReScanPath();
            }
            else
            {
                if (EnvironmentSequence != null)
                {
                    EnvironmentSequence.Pause();
                }
            }
        }
        else
        {
            if (isactive)
            {
                LoadHome();
            }
        }
    }

    public void DestroyHome()
    {
        ResetHome();
        GameObject.Destroy(HomeRoot);
    }

    public void ResetBuildingState()
    {
        if (m_SelectBuild != null)
        {
            SetSelectBuild(null);
        }
    }

    public void ResetHome()
    {
        EnvironmentSequence.Kill();
        m_BuildBehaviourDic.Clear();
        PetObjDict.Clear();
        NpcObjDict.Clear();
    }

    public void LoadHome()
    {
        GameStateManager.GetInst().GameState = GAMESTATE.OTHER;
        EnterHome();
    }

    public void EnterHome()
    {
        LoadUnNewFurnitureList();
        GetRecruitSquarePoint();

        if (Camera.main != null)
        {
            Camera.main.enabled = false;
        }

        //if (HomeManager.HomeSize == 0)
        {
            ReGetHomeData();
        }
        {
            Debug.Log("Homesize = " + HomeSize);
        }
        AppMain.GetInst().StartCoroutine(CreatHome());

#if UNITY_ANDROID || UNITY_IPHONE
               UIManager.GetInst().ShowUI<UI_Chat>("UI_Chat");
#endif
    }

    IEnumerator CreatHome()
    {
        string SceneName = "80001";
        //AssetBundle ab = ResourceManager.GetInst().LoadAB("Scene/" + SceneName);
        string scenePath = ResourceManager.LOCAL_PATH + "Scene/" + SceneName + ".unity3d";
        AssetBundleCreateRequest  abcr = AssetBundle.LoadFromFileAsync(scenePath);
        while (!abcr.isDone)
        {
            Debug.Log("null");
            yield return null;
        }

        AssetBundle ab = abcr.assetBundle;
        if (ab == null)
        {
            Debug.Log("ab null");
        }
        AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(SceneName);
        yield return async;
        ab.Unload(false);
        SetHomeRoot(SceneName);
        SetAmbient();
        SetHomeCamera();

        yield return null;
        SetAllBrick();          //生成地形//
        yield return null;
        SetAllBuild();          //生成建筑物//
        yield return null;
        SetNpcModelAll();       //访客以及建筑物产生的npc//
        SetPetModelAll();       //我的宠物//
        yield return null;
        SetAllTreasure();       //生成宝藏//

        //设置天气//
        BeginLight(DayTime.Day);

        UIManager.GetInst().ShowUI<UI_HomeMain>("UI_HomeMain");
        SetState(HomeState.None);
        InputBinding();

        yield return null;
//        SetUnOpenArea();       //改变未解锁区域地貌//

        GameStateManager.GetInst().GameState = GAMESTATE.HOME;
        GameUtility.SetAstarParam();
        GameUtility.ReScanPath();
        isTestWorld = false;
        AudioManager.GetInst().PlayMusic("village_theme");
    }

    bool isTestWorld = false;
    IEnumerator CreatTestWorld()  //室外桃园
    {
        WorldMapManager.GetInst().DestroyWorldMap();
        HomeManager.GetInst().DestroyHome();
        ResourceManager.GetInst().UnloadAllAB();
        string SceneName = "90001";
        AssetBundle ab = ResourceManager.GetInst().LoadAB("Scene/" + SceneName);
        AsyncOperation async = Application.LoadLevelAsync(SceneName);
        yield return async;
        ab.Unload(false);
        HomeRoot = GameObject.Find(SceneName);
        GameUtility.ResetCameraAspect();
        HomeCamera = Camera.main;
        CreatTestModel();
        SetState(HomeState.None);
        InputBindingForTest();
        yield return null;
        GameStateManager.GetInst().GameState = GAMESTATE.HOME;
        isTestWorld = true;
    }

    public void EnterTestWorld()
    {
        AppMain.GetInst().StartCoroutine(CreatTestWorld());
    }

    public void SetAllBuild()
    {
        foreach (BuildInfo info in m_BuildDic.Values)
        {
            //不显示未解锁区域的圣火房//
            if (info.type == EBuildType.eFire)
            {
                BuildingFireRoomConfig fire = GetBuildingFireRoom(info.buildId);
                if (fire != null)
                {
                    if (fire.unlock_area - 1 > OpenArea)
                    {
                        continue;
                    }
                }
            }
            SetBuild(info);
        }
        ShowMakeTips();
    }

    void SetBuild(BuildInfo build_info)
    {
        GameObject Build = new GameObject();
        Build.transform.SetParent(BuildRoot.transform);
        BuildBaseBehaviour bb = BlindBuildBehaviour(Build, build_info.type);
        bb.SetBuild(build_info);
        SaveBuildingBehaviour(build_info.id, bb);
    }

    GameObject mClientObj;  //客户端建造的模型//
    public void SetClientBuild(BuildInfo build_info)
    {
        if (mClientObj != null)
        {
            return;
        }
        mClientObj = new GameObject();
        mClientObj.transform.SetParent(BuildRoot.transform);
        BuildBaseBehaviour bb = mClientObj.AddComponent<BuildClientBehaviour>();
        bb.SetBuild(build_info);
    }

    public void QuitCreatMode() //退出建造模式
    {
        UIManager.GetInst().CloseUI("UI_BuildCreatConfirm");
        HomeManager.GetInst().SetState(HomeState.None);
        UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", true);
        GameObject.Destroy(mClientObj);
        mClientObj = null;
    }

    public void EnterCreatMode(int buildId, Vector3 pos) //退出建造模式
    {
        BuildInfo info = new BuildInfo(buildId, pos);
        HomeManager.GetInst().SetClientBuild(info);
        //进入建造模式
        UIManager.GetInst().CloseUI("UI_FurnitureList");
        UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", false);
        HomeManager.GetInst().SetState(HomeState.FurnitureCreat);
    }

    public GameObject SetNpcBuild(BuildInfo build_info)
    {
        GameObject Build = new GameObject();
        Build.transform.SetParent(BuildRoot.transform);
        BuildCommonRoomBehaviour bb = Build.AddComponent<BuildCommonRoomBehaviour>();
        bb.SetNoOperational();  //不需要操作;
        bb.SetBuild(build_info);
        return Build;
    }

    //为了方便处理，将游戏中唯一的系统级功能单独保存//
    BuildBoardBehaviour mBoardBehaviour;            //公告栏（每日任务）
    public void CheckBoardBar()
    {
        if (mBoardBehaviour != null)
        {
            mBoardBehaviour.ShowBoardBar();
        }
    }

    BuildBaseBehaviour BlindBuildBehaviour(GameObject Build, EBuildType type)
    {
        BuildBaseBehaviour bb = null;
        if (type == EBuildType.eFire)
        {
            bb = Build.AddComponent<BuildFireBehaviour>();
        }
        else if (type == EBuildType.eProduce)
        {
            bb = Build.AddComponent<BuildProduceBehaviour>();
        }
        else if (type == EBuildType.eObstacle)
        {
            bb = Build.AddComponent<BuildObstacleBehaviour>();
        }
        else if (type == EBuildType.eBench)
        {
            bb = Build.AddComponent<BuildBenchBehaviour>();
        }
        else if (type == EBuildType.eSquare)
        {
            bb = Build.AddComponent<BuildSquareBehaviour>();
        }
        else if (type == EBuildType.eTaskBoard)
        {
            mBoardBehaviour = Build.AddComponent<BuildBoardBehaviour>();
            bb = mBoardBehaviour;
        }
        else if (type == EBuildType.eCure)
        {
            bb = Build.AddComponent<BuildCureBehaviour>();
        }
        else if (type == EBuildType.eSpecificity)
        {
            bb = Build.AddComponent<BuildCureBehaviour>();
        }
        else
        {
            bb = Build.AddComponent<BuildBaseBehaviour>();
        }
        return bb;
    }


    HomeState m_state = HomeState.None;
    bool bBrickAdd = true;

    public void SetBrickEditor()
    {
        ChangeBrickEditorState();
        CleanEditorBrickEffect();
        ShowEditorBrickEffect();
    }

    void ChangeBrickEditorState()
    {
        string url = string.Empty;
        bBrickAdd = !bBrickAdd;
        if (bBrickAdd)
        {
            url = "Bg#tianzhuan";
        }
        else
        {
            url = "Bg#shanzhuan";
        }
        UIManager.GetInst().GetUIBehaviour<UI_HomeMain>().SetBtnUrl(url);
    }

    public void SetState(HomeState state)
    {
        m_state = state;
        if (state == HomeState.None)
        {
            SetHomeCameraEventMask(InitLayer);
        }

        if (state == HomeState.FurnitureCreat)
        {
            SetHomeCameraEventMask("TestObj");
        }

        if (state == HomeState.BrickEditor)
        {
            InputManager.GetInst().enterLongPress = EnterLongPress;
            InputManager.GetInst().onLongPressDarg = OnLongPressDarg;
            InputManager.GetInst().onLongPressDargOver = OnLongPressDargOver;
            bBrickAdd = true;
            ChangeBrickEditorState();
        }
        else
        {
            InputManager.GetInst().enterLongPress = null;
            InputManager.GetInst().onLongPressDarg = null;
            InputManager.GetInst().onLongPressDargOver = null;
        }

        InputManager.GetInst().InputStateReset();
    }

    public HomeState GetState()
    {
        return m_state;
    }

    public bool ChangeState() //变换模式
    {
        if (m_state == HomeState.None)
        {
            SetSelectBuild(null);
            SetState(HomeState.BrickEditor);
            ShowEditorBrickEffect();
            ChangeCameraRotation(new Vector3(90, HomeCamera.transform.eulerAngles.y, HomeCamera.transform.eulerAngles.z));
            Camera.main.cullingMask = ~(1 << 9);  //关闭Character 
            return true;
        }
        else if (m_state == HomeState.BrickEditor)
        {
            SetState(HomeState.None);
            CleanEditorBrickEffect();
            ChangeCameraRotation(new Vector3(GlobalParams.GetVector3("village_camera_rot").x, HomeCamera.transform.eulerAngles.y, HomeCamera.transform.eulerAngles.z));
            Camera.main.cullingMask |= 1 << 9;  //打开Character 
            return false;
        }
        return true;
    }

    #endregion

    #region 家园数据

    byte[,] m_HomePointData;    //记录家园的放置情况（0是没有摆放建筑，1是摆放了建筑,2是地基,现在没有地基 3是不可操作的点广场等）
    long[,] m_HomeWallDoorData; //记录墙和门的id方便计算围墙//
    byte[,] m_HomeWallData; //记录墙和门的id方便计算围墙//

    public bool IsWallLink(int x, int y)
    {
        if (x >= 0 && x < HomeSize &&
                y >= 0 && y < HomeSize)
        {
            if (m_HomeWallData[x, y] > 0)
            {
                return true;
            }
        }
        return false;
    }

    public void UpdateWallLinkData(int x, int y, byte state)
    {
        if (x >= 0 && x < HomeSize &&
                y >= 0 && y < HomeSize)
        {
            m_HomeWallData[x, y] = state;
            ResetLinkWall(x, y);
        }
    }

    public ulong GetWallLink(int x, int y)
    {
        ulong bit = 0;

        if (IsWallLink(x, y + 1))
        {
            bit |= 1 << 7;
        }
        if (IsWallLink(x - 1, y))
        {
            bit |= 1 << 3;
        }
        if (IsWallLink(x, y - 1))
        {
            bit |= 1 << 1;
        }
        if (IsWallLink(x + 1, y))
        {
            bit |= 1 << 5;
        }
        return bit;
    }

    BuildBaseBehaviour m_SelectBuild;
    public BuildBaseBehaviour GetSelectBuild()
    {
        return m_SelectBuild;
    }

    public void SetSelectBuild(BuildBaseBehaviour bb)
    {
        StopCameraYAutoTweener();
        if (m_SelectBuild != null)
        {
            m_SelectBuild.BackToOldPos();
            m_SelectBuild.SetBuildOperateState(BuildOperateState.None);
            //如果位置不对，恢复到正确的位置//
        }
        m_SelectBuild = bb;
    }

    public void SetHomePoint(Vector2 point, int sizeX, int sizeY, byte value)
    {
        int x = (int)point.x;
        int y = (int)point.y;

        for (int i = y; i < y + sizeY; i++)
        {
            for (int j = x; j < x + sizeX; j++)
            {
                if (j < 0 || j >= HomeSize || i < 0 || i >= HomeSize)
                {
                    continue;
                }
                m_HomePointData[j, i] = value;
            }
        }
    }


    public void SetHomeWallDoorPoint(BuildInfo bi, long id)
    {
        int x = (int)bi.pos.x;
        int y = (int)bi.pos.y;

        for (int i = y; i < y + bi.size_y; i++)
        {
            for (int j = x; j < x + bi.size_x; j++)
            {
                if (j < 0 || j >= HomeSize || i < 0 || i >= HomeSize)
                {
                    continue;
                }

                if (bi.type == EBuildType.eWall)
                {
                    m_HomeWallData[j, i] = (byte)(id > 0 ? 1 : 0);
                    if (m_HomeWallDoorData[j, i] != id)
                    {
                        ResetLinkWall(j, i);
                    }
                }
                m_HomeWallDoorData[j, i] = id;
            }
        }
    }

    void ResetLinkWall(int x, int y)
    {
        if (IsWallLink(x, y + 1))
        {
            long id = m_HomeWallDoorData[x, y + 1];
            if (m_BuildBehaviourDic.ContainsKey(id))
            {
                m_BuildBehaviourDic[id].ResetWallModel();
            }
        }
        if (IsWallLink(x - 1, y))
        {
            long id = m_HomeWallDoorData[x - 1, y];
            if (m_BuildBehaviourDic.ContainsKey(id))
            {
                m_BuildBehaviourDic[id].ResetWallModel();
            }
        }
        if (IsWallLink(x, y - 1))
        {
            long id = m_HomeWallDoorData[x, y - 1];
            if (m_BuildBehaviourDic.ContainsKey(id))
            {
                m_BuildBehaviourDic[id].ResetWallModel();
            }
        }
        if (IsWallLink(x + 1, y))
        {
            long id = m_HomeWallDoorData[x + 1, y];

            if (m_BuildBehaviourDic.ContainsKey(id))
            {
                m_BuildBehaviourDic[id].ResetWallModel();
            }
        }

    }

    List<int> GetEmptyPoint()
    {
        List<int> EmptyPointList = new List<int>();
        int height = 0;
        for (int i = 0; i < m_HomePointData.GetLength(1); i++)
        {
            for (int j = 0; j < m_HomePointData.GetLength(0); j++)
            {
                if (m_HomePointData[j, i] == 0) //说明没有建筑物
                {
                    height = GetHeightByPos(j, i);
                    if (IsBrickInOpenArea(j, i))   //在可操作区域内
                    {
                        if (m_BrickElevationDict.ContainsKey(height))
                        {
                            int need_level = m_BrickElevationDict[height].req_level;
                            if (MainBuildLevel >= need_level)
                            {
                                EmptyPointList.Add(j + i * HomeSize);
                            }
                        }
                    }
                }
            }
        }
        return EmptyPointList;
    }

    public bool CheckCanPutDown(Vector2 pos, BuildInfo m_info)  //检测是否可以移动建筑到指定的位置
    {
        int x = (int)pos.x;
        int y = (int)pos.y;
        int sizeX = m_info.size_x;
        int sizeY = m_info.size_y;
        int height = GetHeightByPos(x, y);
        if (height != BaseHeight)  //只能放置在基准层
        {
            return false;
        }

        //如果是墙或者门类型是否安放的隔壁已经有两个相接了
        if (m_info.type == EBuildType.eWall || m_info.type == EBuildType.eDoor)
        {
            for (int i = x; i < x + sizeX; i++)
            {
                for (int j = y; j < y + sizeY; j++)
                {
                    if (CheckIsWallOrDoorAdjoin(i, j, m_info.id))
                    {
                        return false;
                    }
                }
            }
        }

        if (!IsBuildInOpenArea(x, y, sizeX, sizeY)) //区域不符合
        {
            return false;
        }

        for (int i = y; i < y + sizeY; i++)
        {
            for (int j = x; j < x + sizeX; j++)
            {
                if (m_info.id > 0)  //客户端建造的不需要排除
                {
                    if (GameUtility.IsRectAllInRect(m_info.pos, new Vector2(sizeX, sizeY), new Vector2(j, i), Vector2.one)) //排除自己
                    {
                        continue;
                    }
                }

                if (GetHeightByPos(j, i) != height)
                {
                    return false;
                }

                if (HasAnyThingOnBrick(j, i))  //此处已经存在建筑物或者广场//
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// 以下是专门用来计算墙和门是否形成封闭空间，封闭空间内是否有家具，这些家具是否行程套装  呵呵。
    /// </summary>
    /// 

    public int CheckSuitInRoom(List<long> furnitureId)  //实例
    {
        List<int> furniture = new List<int>();
        for (int i = 0; i < furnitureId.Count; i++)
        {
            if (m_BuildDic[furnitureId[i]].level != 0)
            {
                furniture.Add(m_BuildDic[furnitureId[i]].buildId);
            }
        }

        int suitId = 0;
        for (int i = 0; i < m_SortFurnitureSetId.Count; i++)
        {
            FurnitureSetConfig fsc = GetFurnitureSetCfg(m_SortFurnitureSetId[i]);
            Dictionary<int, int> furnitureDic = HomeManager.GetInst().GetSuitFurniture(fsc.furniture_list);
            bool bSuit = true;

            foreach (int id in furnitureDic.Keys)
            {
                if (furniture.Contains(id) && GameUtility.GetNumInList(furniture, id) >= furnitureDic[id])
                {

                }
                else
                {
                    bSuit = false;
                    break;
                }
            }

            if (bSuit)
            {
                suitId = m_SortFurnitureSetId[i];
                break;
            }
        }
        return suitId;
    }

    public bool CheckDoorIsValid(long doorId, ref long wall0, ref long wall1)
    {
        int sizeX = m_BuildDic[doorId].size_x;
        int sizeY = m_BuildDic[doorId].size_y;
        int posX = (int)m_BuildDic[doorId].pos.x;
        int posY = (int)m_BuildDic[doorId].pos.y;

        if (sizeX > sizeY)  //横着的门  
        {
            //可以开始寻找封闭 ，假定一个墙或者门只能和两个方向连接
            if (posX - 1 >= 0 && m_HomeWallDoorData[posX - 1, posY] != 0 && posX + sizeX < HomeSize && m_HomeWallDoorData[posX + sizeX, posY] != 0)
            {
                //以左边为起点右边的为终点
                wall0 = m_HomeWallDoorData[posX - 1, posY];
                wall1 = m_HomeWallDoorData[posX + sizeX, posY];
                return true;
            }
        }

        if (sizeX < sizeY)  //竖着的门
        {
            //可以开始寻找封闭 ，假定一个墙或者门只能和两个方向连接
            if (posY - 1 >= 0 && m_HomeWallDoorData[posX, posY - 1] != 0 && posY + sizeY < HomeSize && m_HomeWallDoorData[posX, posY + sizeY] != 0)
            {
                //以上边边为起点下边为终点
                wall0 = m_HomeWallDoorData[posX, posY + sizeY];
                wall1 = m_HomeWallDoorData[posX, posY - 1];
                return true;
            }
        }
        return false;
    }


    public void CheckCanBeRoom()
    {
        HashSet<long> wallAndDoors = new HashSet<long>();
        foreach (long id in m_BuildDic.Keys)
        {
            if (m_BuildDic[id].type == EBuildType.eDoor)  //优先检测门//
            {
                if (m_BuildDic[id].belongRoom == 0)
                {
                    wallAndDoors = CheckIsRoom(id);
                    if (wallAndDoors.Count > 0)
                    {
                        break;
                    }
                }
            }
        }

        if (wallAndDoors.Count > 0)  //产生了房间//
        {
            List<long> furnitureList = new List<long>();
            furnitureList = GetFurnitureInRoom(wallAndDoors);

            string wallStr = string.Empty;
            string furnitureStr = string.Empty;
            foreach (long id in wallAndDoors)
            {
                wallStr += id + "|";
            }

            if (furnitureList.Count > 0)
            {
                for (int i = 0; i < furnitureList.Count; i++)
                {
                    furnitureStr += furnitureList[i] + "|";
                }
            }
            else
            {
                furnitureStr = "0";
            }

            SendCreatRoom(wallStr, furnitureStr);
        }

    }

    public List<Vector2> GetPolygonVertex(HashSet<long> set)  //获得房间的所有顶点
    {
        List<Vector2> vertexs = new List<Vector2>();
        foreach (long id in set)
        {
            if (m_BuildDic[id].type == EBuildType.eWall)
            {
                if (bPointVertex(id))
                {
                    vertexs.Add(m_BuildDic[id].pos);
                }
            }
        }
        return vertexs;
    }

    public List<long> GetFurnitureInRoom(HashSet<long> set)
    {
        List<long> mFurnitureList = new List<long>();
        List<Vector2> vertexs = GetPolygonVertex(set);
        foreach (long id in m_BuildDic.Keys)
        {
            BuildingTypeHold typeCfg = GetBuildTypeCfg(m_BuildDic[id].buildId);
            if (typeCfg != null && typeCfg.mSuit.Count > 0) //只计算在房间内有作用的家具//
            {
                if (m_BuildDic[id].belongRoom == 0)
                {
                    if (GameUtility.pointInRegion(m_BuildDic[id].pos, vertexs))
                    {
                        mFurnitureList.Add(id);
                    }
                }
            }
        }
        //if (mFurnitureList.Count > 0)
        //{
        //        Debug.Log("有家具");
        //        foreach (long id in mFurnitureList)
        //        {
        //                Debuger.Log(id + "|");
        //        }

        //}
        return mFurnitureList;
    }


    public bool bPointVertex(long id)
    {
        int x = (int)m_BuildDic[id].pos.x;
        int y = (int)m_BuildDic[id].pos.y;
        //上下都有 或者左右都有 就肯定不是拐点
        if (y + 1 < HomeSize && m_HomeWallDoorData[x, y + 1] != 0)
        {
            if (y - 1 >= 0 && m_HomeWallDoorData[x, y - 1] != 0)
            {
                return false;
            }
        }

        if (x - 1 >= 0 && m_HomeWallDoorData[x - 1, y] != 0)
        {
            if (x + 1 < HomeSize && m_HomeWallDoorData[x + 1, y] != 0)
            {
                return false;
            }
        }

        return true;
    }


    public long CheckFurnitureInRoom(Vector2 pos)
    {
        foreach (long id in m_RoomDic.Keys)
        {
            List<Vector2> vertexs = GetPolygonVertex(m_RoomDic[id].wallAndDoor);
            if (GameUtility.pointInRegion(pos, vertexs))
            {
                return id;
            }
        }
        return 0;
    }

    public HashSet<int> GetHomeActiveSuits()
    {
        HashSet<int> mSuit = new HashSet<int>();
        foreach (long id in m_RoomDic.Keys)
        {
            if (m_RoomDic[id].idsuit > 0)
            {
                mSuit.Add(m_RoomDic[id].idsuit);
            }
        }
        return mSuit;
    }

    public HashSet<long> CheckIsRoom(long doorId)  //检测是否形成了房间//
    {
        long wall0 = 0;
        long wall1 = 0;
        HashSet<long> set = new HashSet<long>();
        if (CheckDoorIsValid(doorId, ref wall0, ref wall1))
        {
            long nextId = wall0;
            long endId = wall1;

            set.Add(doorId);
            set.Add(nextId);

            while (nextId != endId)
            {
                nextId = GetNextWallOrDoor(nextId, set);
                if (nextId == 0) //不存在环
                {
                    set.Clear();
                    break;
                }
                else
                {
                    if (m_BuildDic[nextId].belongRoom != 0) //是别的房间的  
                    {
                        set.Clear();
                        break;
                    }

                    if (m_BuildDic[nextId].type == EBuildType.eDoor)  //该死的门
                    {
                        set.Add(nextId);
                        long temp_wall0 = 0;
                        long temp_wall1 = 0;
                        if (CheckDoorIsValid(nextId, ref temp_wall0, ref temp_wall1))
                        {
                            if (set.Contains(temp_wall0))
                            {
                                nextId = temp_wall1;
                            }
                            else
                            {
                                nextId = temp_wall0;
                            }
                            set.Add(nextId);
                        }
                        else
                        {
                            set.Clear();
                            break;
                        }
                    }
                    else
                    {
                        set.Add(nextId);
                    }
                }
            }
        }
        //if (set.Count > 0)
        //{
        //        Debuger.Log("变成房间了");
        //        foreach (long id in set)
        //        {
        //                Debuger.Log(id + "|");
        //        }
        //}
        //else
        //{
        //        Debuger.Log("没有房间啊");
        //}

        return set;
    }

    long GetNextWallOrDoor(long nextId, HashSet<long> set)
    {
        int x = (int)m_BuildDic[nextId].pos.x;
        int y = (int)m_BuildDic[nextId].pos.y;
        //上下左右
        if (y + 1 < HomeSize && m_HomeWallDoorData[x, y + 1] != 0)
        {
            if (!set.Contains(m_HomeWallDoorData[x, y + 1]))
            {
                return m_HomeWallDoorData[x, y + 1];
            }
        }

        if (y - 1 >= 0 && m_HomeWallDoorData[x, y - 1] != 0)
        {
            if (!set.Contains(m_HomeWallDoorData[x, y - 1]))
            {
                return m_HomeWallDoorData[x, y - 1];
            }
        }

        if (x - 1 >= 0 && m_HomeWallDoorData[x - 1, y] != 0)
        {
            if (!set.Contains(m_HomeWallDoorData[x - 1, y]))
            {
                return m_HomeWallDoorData[x - 1, y];
            }
        }

        if (x + 1 < HomeSize && m_HomeWallDoorData[x + 1, y] != 0)
        {
            if (!set.Contains(m_HomeWallDoorData[x + 1, y]))
            {
                return m_HomeWallDoorData[x + 1, y];
            }
        }

        return 0;
    }

    bool CheckIsWallOrDoorAdjoin(int x, int y, long id)
    {

        //检查自己的上下左右

        int num_self = 0;  //上左右
        if (y + 1 < HomeSize && m_HomeWallDoorData[x, y + 1] != 0 && m_HomeWallDoorData[x, y + 1] != id)
        {
            num_self++;
        }
        if (y - 1 < HomeSize && m_HomeWallDoorData[x, y - 1] != 0 && m_HomeWallDoorData[x, y - 1] != id)
        {
            num_self++;
        }
        if (x - 1 < HomeSize && m_HomeWallDoorData[x - 1, y] != 0 && m_HomeWallDoorData[x - 1, y] != id)
        {
            num_self++;
        }
        if (x + 1 < HomeSize && m_HomeWallDoorData[x + 1, y] != 0 && m_HomeWallDoorData[x + 1, y] != id)
        {
            num_self++;
        }
        if (num_self > 2)
        {
            return true;
        }


        //检查相邻的上下左右
        int X;
        int Y;

        if (y + 1 < HomeSize && m_HomeWallDoorData[x, y + 1] != 0 && m_HomeWallDoorData[x, y + 1] != id)
        {
            X = x;
            Y = y + 1;
            int num = 0;  //上左右
            if (Y + 1 < HomeSize && m_HomeWallDoorData[X, Y + 1] != 0 && m_HomeWallDoorData[X, Y + 1] != id)
            {
                num++;
            }
            if (X - 1 < HomeSize && m_HomeWallDoorData[X - 1, Y] != 0 && m_HomeWallDoorData[X - 1, Y] != id)
            {
                num++;
            }
            if (X + 1 < HomeSize && m_HomeWallDoorData[X + 1, Y] != 0 && m_HomeWallDoorData[X + 1, Y] != id)
            {
                num++;
            }
            if (num >= 2)
            {
                return true;
            }
        }

        if (y - 1 >= 0 && m_HomeWallDoorData[x, y - 1] != 0 && m_HomeWallDoorData[x, y - 1] != id)
        {
            X = x;
            Y = y - 1;
            int num = 0;  //下左右
            if (Y - 1 < HomeSize && m_HomeWallDoorData[X, Y - 1] != 0 && m_HomeWallDoorData[X, Y - 1] != id)
            {
                num++;
            }
            if (X - 1 < HomeSize && m_HomeWallDoorData[X - 1, Y] != 0 && m_HomeWallDoorData[X - 1, Y] != id)
            {
                num++;
            }
            if (X + 1 < HomeSize && m_HomeWallDoorData[X + 1, Y] != 0 && m_HomeWallDoorData[X + 1, Y] != id)
            {
                num++;
            }
            if (num >= 2)
            {
                return true;
            }
        }

        if (x - 1 >= 0 && m_HomeWallDoorData[x - 1, y] != 0 && m_HomeWallDoorData[x - 1, y] != id)
        {
            X = x - 1;
            Y = y;
            int num = 0;  //左上下

            if (Y + 1 < HomeSize && m_HomeWallDoorData[X, Y + 1] != 0 && m_HomeWallDoorData[X, Y + 1] != id)
            {
                num++;
            }
            if (Y - 1 < HomeSize && m_HomeWallDoorData[X, Y - 1] != 0 && m_HomeWallDoorData[X, Y - 1] != id)
            {
                num++;
            }

            if (X - 1 < HomeSize && m_HomeWallDoorData[X - 1, Y] != 0 && m_HomeWallDoorData[X - 1, Y] != id)
            {
                num++;
            }
            if (num >= 2)
            {
                return true;
            }
        }

        if (x + 1 < HomeSize && m_HomeWallDoorData[x + 1, y] != 0 && m_HomeWallDoorData[x + 1, y] != id)
        {
            X = x + 1;
            Y = y;
            int num = 0;  //右上下

            if (Y + 1 < HomeSize && m_HomeWallDoorData[X, Y + 1] != 0 && m_HomeWallDoorData[X, Y + 1] != id)
            {
                num++;
            }
            if (Y - 1 < HomeSize && m_HomeWallDoorData[X, Y - 1] != 0 && m_HomeWallDoorData[X, Y - 1] != id)
            {
                num++;
            }

            if (X + 1 < HomeSize && m_HomeWallDoorData[X + 1, Y] != 0 && m_HomeWallDoorData[X + 1, Y] != id)
            {
                num++;
            }
            if (num >= 2)
            {
                return true;
            }
        }
        return false;
    }

    public void DeleteRoom(long roomId)  //客户端主动发起房间拆除
    {
        if (m_RoomDic.ContainsKey(roomId))
        {
            Room mRoom = m_RoomDic[roomId];
            foreach (long id in mRoom.wallAndDoor)
            {
                if (m_BuildDic.ContainsKey(id))
                {
                    BuildInfo info = m_BuildDic[id];
                    info.belongRoom = 0;
                    m_BuildDic[id] = info;
                }
            }

            for (int i = 0; i < mRoom.buildInRoom.Count; i++)
            {
                if (m_BuildDic.ContainsKey(mRoom.buildInRoom[i]))
                {
                    BuildInfo info = m_BuildDic[mRoom.buildInRoom[i]];
                    info.belongRoom = 0;
                    m_BuildDic[mRoom.buildInRoom[i]] = info;
                }
            }
        }
        m_RoomDic.Remove(roomId);

        SendRoomClear(roomId);

        //房间被摧毁了,此处播放特效//
        GameUtility.PopupMessage("房间摧毁！");
    }


    void RemoveRoomFurniture(long roomId, long furnitureId)  //去除房间中的家具//
    {
        if (m_RoomDic.ContainsKey(roomId))
        {
            if (m_RoomDic.ContainsKey(roomId))
            {
                Room info = m_RoomDic[roomId];
                List<long> furniture = info.buildInRoom;
                furniture.Remove(furnitureId);
                info.buildInRoom = furniture;
                m_RoomDic[roomId] = info;

                int newSuit = CheckSuitInRoom(furniture);
                if (newSuit != info.idsuit)
                {
                    SendRoomChangeSuit(roomId, newSuit);  //通知服务器房间套装更改
                    FurnitureSetConfig cfg = GetFurnitureSetCfg(newSuit);
                    if (cfg != null)
                    {
                        GameUtility.PopupMessage("组成套装" + LanguageManager.GetText(cfg.name));
                    }
                    else
                    {
                        if (newSuit == 0)
                        {
                            FurnitureSetConfig old_cfg = GetFurnitureSetCfg(info.idsuit);
                            GameUtility.PopupMessage("失去套装" + LanguageManager.GetText(old_cfg.name));
                        }
                    }

                    info.idsuit = newSuit;
                    m_RoomDic[roomId] = info;
                }
            }
        }
    }

    void AddRoomFurniture(long roomId, long furnitureId)  //增加//
    {
        if (m_RoomDic.ContainsKey(roomId))
        {
            if (m_RoomDic.ContainsKey(roomId))
            {
                Room info = m_RoomDic[roomId];
                List<long> furniture = info.buildInRoom;
                furniture.Add(furnitureId);
                info.buildInRoom = furniture;
                m_RoomDic[roomId] = info;

                int newSuit = CheckSuitInRoom(furniture);
                if (newSuit != info.idsuit)
                {
                    SendRoomChangeSuit(roomId, newSuit);  //通知服务器房间套装更改
                    FurnitureSetConfig cfg = GetFurnitureSetCfg(newSuit);
                    if (cfg != null)
                    {
                        GameUtility.PopupMessage("组成套装" + LanguageManager.GetText(cfg.name));
                    }

                    info.idsuit = newSuit;
                    m_RoomDic[roomId] = info;
                }
            }
        }
    }

    public void SendCreatRoom(string wallAndDoor, string furniture)
    {
        CSMsgBuildCreateRoom msg = new CSMsgBuildCreateRoom();
        msg.allWalldoor = wallAndDoor;
        msg.allFurniture = furniture;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }

    public void SendRoomChangeSuit(long id, int suit)
    {
        CSMsgRoomChangeSuit msg = new CSMsgRoomChangeSuit();
        msg.idRoom = id;
        msg.idSuit = suit;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }

    public void SendRoomClear(long id)
    {
        CSMsgBuildRoomClear msg = new CSMsgBuildRoomClear();
        msg.idroom = id;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }

    #endregion

    #region 家园地形和区域

    readonly string HouseLandInfo = "HouseLandInfo"; //本地海拔高度
    public CubeManager m_CubeManager;

    EditorBrickEffectManager m_EditorBrickEffectManager;
    public readonly int BaseHeight = 3; //基准层高度

    Dictionary<int, BrickElevationConfig> m_BrickElevationDict = new Dictionary<int, BrickElevationConfig>();
    Dictionary<int, BuildingFireRoomConfig> m_BuildingFireRoomDict = new Dictionary<int, BuildingFireRoomConfig>();
    Dictionary<int, BuildingHomeAreaConfig> m_AreaDict = new Dictionary<int, BuildingHomeAreaConfig>();

    BrickInitDict m_BrickInitializationDict = new BrickInitDict();
    byte[] m_BrickHeightData; //地形海拔高度//
    void InitHomeHeight()
    {
        ConfigHoldUtility<BrickElevationConfig>.LoadXml("Config/brick_elevation", m_BrickElevationDict);
        ConfigHoldUtility<BuildingFireRoomConfig>.LoadXml("Config/building_fire_room", m_BuildingFireRoomDict);
        ConfigHoldUtility<BuildingHomeAreaConfig>.LoadXml("Config/building_home_area", m_AreaDict);
        m_BrickInitializationDict = (ResourceManager.GetInst().Load("Config/brick_initialization") as brick_initialization).m_Dict;
        SetHomeInitData();
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgHouseLandInfo), OnGetHouseLandInfo);
    }

    public BuildingHomeAreaConfig GetAreaCfg(int id)
    {
        if (m_AreaDict.ContainsKey(id))
        {
            return m_AreaDict[id];
        }
        return null;
    }

    public BuildingFireRoomConfig GetBuildingFireRoom(int id)
    {
        if (m_BuildingFireRoomDict.ContainsKey(id))
        {
            return m_BuildingFireRoomDict[id];
        }
        return null;
    }

    public BrickElevationConfig GetBrickElevationCfg(int height)
    {
        if (m_BrickElevationDict.ContainsKey(height))
        {
            return m_BrickElevationDict[height];
        }
        return null;
    }

    void SetHomeInitData()
    {
        HomeSize = (int)Mathf.Sqrt(m_BrickInitializationDict.Count);   //获得家园大小
        m_BrickBoxArry = new BoxCollider[HomeSize * HomeSize];
        m_HomePointData = new byte[HomeSize, HomeSize];
        m_HomeWallDoorData = new long[HomeSize, HomeSize];
        m_HomeWallData = new byte[HomeSize, HomeSize];

        int x, y;
        for (int i = 1; i < m_BrickInitializationDict.Count + 1; i++)
        {
            if (m_BrickInitializationDict[i].is_square != 0)
            {
                x = (i - 1) % HomeSize;
                y = (i - 1) / HomeSize;
                m_HomePointData[x, y] = 3;
                SquarePointList.Add(i - 1);  //存储广场的点//
            }
        }
    }

    void ReGetHomeData()  //重新获取家园大小和家园海拔数组
    {
        Debug.Log("ReGetHomeData");
        HomeSize = (int)Mathf.Sqrt(m_BrickInitializationDict.Count);   //获得家园大小
        string localHouseLand = GameUtility.GetPlayerData(HouseLandInfo);
        if (localHouseLand.Length == 0)
        {
            GetInitHight();  //初始配置高度
        }
        else
        {
            //localHouseLand = GameUtility.GzipDecompress(localHouseLand);
            SetBrickInitializationByString(localHouseLand);
        }
    }

    void OnGetHouseLandInfo(object sender, SCNetMsgEventArgs e)
    {
        SCMsgHouseLandInfo msg = e.mNetMsg as SCMsgHouseLandInfo;
        if (msg.strLandInfo.Equals("0"))
        {
            GetInitHight();
        }
        else
        {
            string info = GameUtility.GzipDecompress(msg.strLandInfo);
            SetBrickInitializationByString(info);
            GameUtility.SavePlayerData(HouseLandInfo, info);  //同步服务器给的地形高度
        }
    }

    void SetBrickInitializationByString(string info)
    {
        m_BrickHeightData = GameUtility.ToByteArray(info);
    }

    public void GetBrickHeightData(string terrain)  //npc家园
    {
        string info = GameUtility.GzipDecompress(terrain);
        HomeSize = (int)Mathf.Sqrt(info.Length);
        m_BrickHeightData = GameUtility.ToByteArray(info);
    }

    public int GetHeightByPos(int x, int z)  //获得指定点的海拔高度
    {
        int id = z * HomeSize + x;
        if (m_BrickHeightData != null && id < m_BrickHeightData.Length && id >= 0)
        {
            int elevation = m_BrickHeightData[id];
            if (elevation > m_BrickElevationDict.Count)
            {
                elevation = m_BrickElevationDict.Count;
            }
            return elevation;
        }
        return 0;
    }

    public int GetHeightByPos(Vector2 pos)  //获得指定点的海拔高度
    {
        int x = (int)pos.x;
        int z = (int)pos.y;
        return GetHeightByPos(x, z);
    }

    public Vector3 GetVector3Pos(Vector2 pos)
    {
        int height = GetHeightByPos(pos);
        return new Vector3(pos.x, height, pos.y);
    }

    public Vector3 GetVector3Pos(int x, int z)
    {
        int height = GetHeightByPos(x, z);
        return new Vector3(x, height, z);
    }


    void GetInitHight()
    {
        m_BrickHeightData = new byte[HomeSize * HomeSize];
        foreach (int id in m_BrickInitializationDict.Keys)
        {
            m_BrickHeightData[id - 1] = (byte)m_BrickInitializationDict[id].elevation;
        }
    }

    int GetBrickKind(int index)  //获得指定的砖块类型//
    {
        if (m_BrickInitializationDict.ContainsKey(index))
        {
            return m_BrickInitializationDict[index].brick_number;
        }
        return 1;
    }

    int GetBrickFixedModel(int index, int height)  //获得固定的砖块类型 小范围更改
    {
        if (m_BrickInitializationDict.ContainsKey(index))
        {
            if (GameUtility.IsStringValid(m_BrickInitializationDict[index].use_brick))
            {
                Dictionary<int, int> heightBrickDic = GameUtility.ParseCommonStringToDict(m_BrickInitializationDict[index].use_brick, ';', ',');
                if (heightBrickDic.ContainsKey(height))
                {
                    return heightBrickDic[height];
                }
            }
        }
        return 0;
    }

    int GetBrickArea(int index)  //获得指定的砖块类型//
    {
        if (m_BrickInitializationDict.ContainsKey(index))
        {
            return m_BrickInitializationDict[index].area;
        }
        return 1;
    }

    public int GetBrickArea(int x, int z)  //获得指定的砖块类型//
    {
        int index = z * HomeSize + x + 1;
        return GetBrickArea(index);
    }

    void SetAllBrick()
    {
        int height = 0;
        for (int z = 0; z < HomeSize; z++)
        {
            for (int x = 0; x < HomeSize; x++)
            {
                height = GetHeightByPos(x, z);
                if (height >= 0) //高度为0的也生成box//
                {
                    SetBrickBox(x, height, z);
                    for (int y = 1; y < height + 1; y++)
                    {
                        if (JudgeIsBrickNeedCreat(x, y, z, height))
                        {
                            SetOneBrickMod(x, y, z);
                        }
                    }
                }
            }
        }
    }

    BoxCollider[] m_BrickBoxArry;
    const int BoxColliderObjNum = 20;
    void SetBrickBox(int x, int y, int z)
    {
        int index = z * HomeSize + x;
        int id = index / (HomeSize * HomeSize / BoxColliderObjNum);
        Transform tf = BoxColliderRoot.transform.Find("BoxColliderObj" + id);
        GameObject obj;
        if (tf == null)
        {
            obj = new GameObject("BoxColliderObj" + id);
            obj.transform.SetParent(BoxColliderRoot.transform);
            GameUtility.SetLayer(obj, "Scene");
        }
        else
        {
            obj = tf.gameObject;
        }

        EventTriggerListener.Get(obj).onClick = OnBrickClick;
        BoxCollider bc = obj.AddComponent<BoxCollider>();
        bc.size = new Vector3(1, y, 1);
        bc.center = new Vector3(x, y * 0.5f, z);
        m_BrickBoxArry[index] = bc;
    }


    void OnBrickClick(GameObject go, PointerEventData data)
    {
        if (m_state == HomeState.None)
        {
            if (m_SelectBuild != null)
            {
                SetSelectBuild(null);
                return;
            }
        }

        if (old_hit_pos != -Vector2.one)
        {
            return;
        }

        int x = (int)(data.pointerPressRaycast.worldPosition.x + 0.5f);
        int z = (int)(data.pointerPressRaycast.worldPosition.z + 0.5f);

        int index = z * 100 + x;
        if (mCleaningBrick.Contains(index))
        {

            GameUtility.PopupMessage("该位置砖块正在被清理！");
            return;
        }

        int y = GetHeightByPos(x, z);

        if (m_cleaningTreasure != 0 && GetTreasureCfg(m_TreasureDic[m_cleaningTreasure].cfgId).type == 4) //障碍转
        {
            if (m_TreasureDic[m_cleaningTreasure].x == x && m_TreasureDic[m_cleaningTreasure].y == z)
            {
                GameUtility.PopupMessage("该位置砖块正在被清理！");
                return;
            }
        }

        if (data.button == PointerEventData.InputButton.Left)
        {
            if (m_state == HomeState.BrickEditor)
            {
                if (!bBrickAdd)
                {
                    if (y > 0)
                    {
                        RemoveBrickHeight(x, z);
                    }
                }
                else
                {
                    if (y >= 0)
                    {
                        AddBrickHeight(x, z);
                    }
                }
            }

            if (m_state == HomeState.None || m_state == HomeState.FurnitureCreat)
            {
                if (y > 0)
                {
                    RemoveBrickHeight(x, z);
                }
            }
        }
    }

    void ChangeBrickBox(int x, int y, int z)
    {
        if (HomeRoot == null)
        {
            return;
        }

        BoxCollider bc = m_BrickBoxArry[z * HomeSize + x];
        bc.size = new Vector3(1, y, 1);
        bc.center = new Vector3(x, y * 0.5f, z);
        if (HomeRoot.activeInHierarchy)
        {
            GameUtility.ReScanPath();  //从新寻路//
        }
    }

    void SetOneBrickMod(int x, int y, int z)
    {
        int index = z * HomeSize + x + 1;
        int model_id = 0;

        int fixedModel = GetBrickFixedModel(index, y);
        if (fixedModel != 0)   //使用固定的砖块类型//            
        {
            model_id = fixedModel;
        }
        else
        {
            int kind = GetBrickKind(index);
            model_id = m_BrickElevationDict[y].GetBrickModel(kind);
        }
        foreach (int id in m_TreasureDic.Keys) //宝藏需要更换砖块
        {
            BuildingHomeTreasureConfig treasureCfg = GetTreasureCfg(m_TreasureDic[id].cfgId);
            if (treasureCfg == null)
            {
                singleton.GetInst().ShowMessage(ErrorOwner.designer, "宝藏" + m_TreasureDic[id].cfgId + "不存在");
                continue;
            }
            if (GameUtility.IsRectAllInRect(new Vector2(m_TreasureDic[id].x, m_TreasureDic[id].y), Vector2.one * treasureCfg.size_x, new Vector2(x, z), Vector2.one))
            {
                if (y >= m_TreasureDic[id].height && y < m_TreasureDic[id].height + treasureCfg.size_z)
                {
                    if (treasureCfg.brick_model > 0)
                    {
                        model_id = treasureCfg.brick_model;
                        continue;
                    }
                }
            }
        }
        if (model_id != 0)
        {
            m_CubeManager.AddMod(model_id, x, y, z, GetBrickLink(x, y, z));
        }
    }

    void SetUnOpenArea()
    {
        string name = string.Empty;
        Transform tf;
        for (int i = 0; i < BrickRoot.transform.childCount; i++)
        {
            tf = BrickRoot.transform.GetChild(i);
            name = tf.name;
            int area = int.Parse(name.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries)[1]);  //获得区域
            if (area > OpenArea)
            {
                Material mat = tf.GetComponent<MeshRenderer>().material;
                mat.SetFloat("_GrayScale", 0.75f);
            }
        }
    }

    public IEnumerator SetOpenArea()
    {
        InputManager.GetInst().SwitchInup(false);
        if (HomeRoot == null)
        {
            InputManager.GetInst().SwitchInup(true);
            yield break;
        }

        yield return new WaitForSeconds(3.0f);

        BuildingHomeAreaConfig bhc = GetAreaCfg(OpenArea);
        if (bhc != null)
        {
            string effect_url = bhc.unlock_effect;
            Vector2 pos2 = XMLPARSE_METHOD.ConvertToVector2(bhc.unlock_effect_position);
            int build = 0;
            BuildingHomeAreaConfig next_bhc = GetAreaCfg(OpenArea + 1);
            if (next_bhc != null)
            {
                build = next_bhc.unlock_building;
            }

            BuildInfo m_info = new BuildInfo();
            foreach (BuildInfo info in m_BuildDic.Values)
            {
                if (info.buildId == build)
                {
                    m_info = info;
                    break;
                }
            }

            int height = GetHeightByPos(pos2);
            Vector3 pos = new Vector3(pos2.x, height, pos2.y);
            Vector3 camera_pos = SetPosOnCentre(pos, HomeCamera.transform);
            HomeCamera.transform.DOMove(camera_pos, 1.5f);
            yield return new WaitForSeconds(1.5f);

            EffectManager.GetInst().PlayEffect(effect_url, pos);
            yield return new WaitForSeconds(2.8f);

            Transform tf;
            for (int i = 0; i < BrickRoot.transform.childCount; i++)
            {
                tf = BrickRoot.transform.GetChild(i);
                int area = int.Parse(tf.name.Split('#')[1]);  //获得区域
                if (area == OpenArea)
                {
                    Material mat = tf.GetComponent<MeshRenderer>().material;
                    mat.SetFloat("_GrayScale", 1);
                }
            }
            yield return new WaitForSeconds(0.2f);
            if (m_info.buildId > 0)
            {
                SetBuild(m_info);
                SetHomePoint(m_info.pos, m_info.size_x, m_info.size_y, 1);
            }
            else
            {
                InputManager.GetInst().SwitchInup(true);
            }
        }
    }

    ulong GetBrickLink(int x, int y, int z)
    {
        ulong bit = 0;
        int idx = -1;
        for (int tz = z - 1; tz <= z + 1; tz++)
        {
            for (int tx = x - 1; tx <= x + 1; tx++)
            {
                idx++;
                if (idx == 4 || tx < 0 || tz < 0 || tx >= HomeSize || tz >= HomeSize)
                {
                    continue;
                }

                int height = GetHeightByPos(tx, tz);
                if (height >= y)
                {
                    bit |= ((ulong)1 << idx);
                }
                else if (height == y - 1)
                {
                    bit |= ((ulong)1 << (idx + 9));
                }
            }
        }

        if (y < GetHeightByPos(x, z))   //上面有
        {
            bit |= (1 << 4);
        }
        return bit;
    }

    //检查是否需要创建（上下左右前后都有砖块或者是最底下一层上前后左右都有砖块就不需要创建）
    bool JudgeIsBrickNeedCreat(int x, int y, int z, int height)
    {
        if (y < height) //说明上下都有//
        {
            if (x - 1 >= 0 && GetHeightByPos(x - 1, z) >= y) //左边有
            {
                if (x + 1 < HomeSize && GetHeightByPos(x + 1, z) >= y) //右边有
                {
                    if (z - 1 >= 0 && GetHeightByPos(x, z - 1) >= y) //前面有
                    {
                        if (z + 1 < HomeSize && GetHeightByPos(x, z + 1) >= y) //后面有
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    bool CheckCanEditorBrick(int x, int height, int z, bool needTip = true)  //检测是否可以改变地形//
    {
        if (height <= BaseHeight)
        {
            return false;
        }

        if (HasAnyThingOnBrick(x, z)) //广场和有建筑摆放的地方无法
        {
            if (needTip)
            {
                //GameUtility.PopupMessage("广场上无法改变地形！");
            }
            return false;
        }

        if (!IsBrickInOpenArea(x, z))
        {
            if (needTip)
            {
                GameUtility.PopupMessage("该区域未开放！");
            }
            return false;
        }

        if (!HasBrickAroundExitBaseHeight(x, z))
        {
            if (needTip)
            {
                GameUtility.PopupMessage("必须先清理最外层的砖！");
            }
            return false;
        }

        if (m_BrickElevationDict.ContainsKey(height))
        {
            int need_level = m_BrickElevationDict[height].req_level;
            if (MainBuildLevel < need_level)
            {
                if (needTip)
                {
                    GameUtility.PopupMessage("主城功能等级不够,无法操作该高度地形！");
                }
                return false;
            }
            return true;
        }

        if (needTip)
        {
            GameUtility.PopupMessage("已经到达最大高度！");
        }
        return false;
    }


    bool CheckIsLevelEnough(int height)
    {
        if (m_BrickElevationDict.ContainsKey(height))
        {
            int need_level = m_BrickElevationDict[height].req_level;
            if (MainBuildLevel < need_level)
            {
                return false;
            }
            return true;
        }
        return false;
    }

    public bool HasAnyThingOnBrick(int x, int z)  //检测改变要改变的地砖上是否有建筑物
    {
        if (m_HomePointData[x, z] == 1 || m_HomePointData[x, z] == 3)
        {
            return true;
        }
        return false;
    }

    bool IsBrickInOpenArea(int index)
    {
        int area = GetBrickArea(index);
        if (area > OpenArea)
        {
            return false;
        }
        return true;
    }

    bool IsBrickInOpenArea(int x, int z)
    {
        int index = z * HomeSize + x + 1;
        return IsBrickInOpenArea(index);
    }

    bool IsBuildInOpenArea(int x, int y, int sizeX, int sizeY)
    {
        for (int i = x; i < x + sizeX; i++)
        {
            for (int j = y; j < y + sizeY; j++)
            {
                if (!IsBrickInOpenArea(i, j))
                {
                    return false;
                }
            }
        }
        return true;
    }

    bool HasBrickAroundExitBaseHeight(int x, int z)  //操作的高度周围是否存在基准层//
    {
        if (GetHeightByPos(x - 1, z) != BaseHeight)
        {
            if (GetHeightByPos(x + 1, z) != BaseHeight)
            {
                if (GetHeightByPos(x, z - 1) != BaseHeight)
                {
                    if (GetHeightByPos(x, z + 1) != BaseHeight)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public void AddBrickHeight(int x, int z)  //添加砖块
    {
        if (CommonDataManager.GetInst().GetNowResourceNum("brick") == 0)
        {
            GameUtility.PopupMessage("砖块不足无法添加高低");
            return;
        }

        int height = GetHeightByPos(x, z) + 1;
        if (CheckCanEditorBrick(x, height, z))
        {
            ChangeBrick(x, height, z);
            CheckTreasureByAdd(x, z);
            AddBrickMod(x, height, z);
            AddBrickEditorEffectInAdd(x, height, z);
        }
    }

    readonly int maxCleanBrickNum = 10;
    HashSet<int> mCleaningBrick = new HashSet<int>();
    void RemoveBrickHeight(int x, int z) //减少砖块
    {
        int height = GetHeightByPos(x, z);
        if (CheckCanEditorBrick(x, height, z))
        {
            int index = (x + z * HomeManager.HomeSize) * 100 + height;
            if (mHinderBrick.Contains(index))  //障碍转//
            {
                SendTreasureClean(index);
                return;
            }

            if (mCleaningBrick.Count == maxCleanBrickNum)
            {
                GameUtility.PopupMessage(string.Format("最多只能同时清理{0}块砖！", maxCleanBrickNum));
                return;
            }

            Vector3 pos = new Vector3(x, height, z);

            if (m_BrickElevationDict.ContainsKey(height))
            {
                GameObject m_CountDown = UIManager.GetInst().ShowUI_Multiple<UI_CountDown3D>("UI_CountDown3D");
                m_CountDown.GetComponent<UI_CountDown3D>().SetUp(m_BrickElevationDict[height].time, pos, CleaningBrickFinish);
                mCleaningBrick.Add(x + z * 100);
            }
        }
    }

    void CleaningBrickFinish(object data) //状态更改为清理完毕
    {
        Vector3 pos = (Vector3)data;
        int x = (int)pos.x;
        int height = (int)pos.y;
        int z = (int)pos.z;

        ChangeBrick(x, height - 1, z);
        if (HomeRoot != null)
        {
            CheckTreasureByRemove(x, z);
            RemoveBrickMod(x, height, z);
            if (m_state == HomeState.BrickEditor)
            {
                if (bBrickAdd)
                {
                    RemoveBrickEditorEffectInAdd(x, height, z);
                }
                else
                {
                    RemoveBrickEditorEffectInRemove(x, height, z);
                }
            }
        }
        int index = z * 100 + x;
        mCleaningBrick.Remove(index);

    }

    void PlayChangeBrickEffect(int x, int height, int z)
    {
        string effect_url = string.Empty;
        if (bBrickAdd)
        {
            effect_url = "effect_raid_element_down";
            EffectManager.GetInst().PlayEffect(effect_url, new Vector3(x, height - 1, z));
        }
        else
        {
            effect_url = m_BrickElevationDict[height + 1].brick_clean_effect;
            EffectManager.GetInst().PlayEffect(effect_url, new Vector3(x, height, z));
        }
    }

    void ChangeBrick(int x, int y, int z)
    {
        int index = z * HomeSize + x;
        m_BrickHeightData[index] = (byte)y;

        CSMsgBuildLandInfoModify msg = new CSMsgBuildLandInfoModify();
        msg.strModifyInfo = index + "&" + y;
        NetworkManager.GetInst().SendMsgToServer(msg);

        string info = GameUtility.ToHexString(m_BrickHeightData);
        //info = GameUtility.GzipCompress(info);  //gc有点大//
        GameUtility.SavePlayerData(HouseLandInfo, info);  //存储本地海拔//     

        ChangeBrickBox(x, y, z);  //改变box高度//

        if (HomeRoot.activeInHierarchy)
        {
            PlayChangeBrickEffect(x, y, z);
        }

    }

    void AddBrickMod(int x, int y, int z)
    {
        SetOneBrickMod(x, y, z);
        //检测是否有可以删除的点//该点的下，左右，前后//
        if (y - 1 >= 1)
        {
            if (!JudgeIsBrickNeedCreat(x, y - 1, z, GetHeightByPos(x, z)))
            {
                m_CubeManager.RemoveMod(x, y - 1, z);
            }
        }

        if (x - 1 >= 0)
        {
            if (!JudgeIsBrickNeedCreat(x - 1, y, z, GetHeightByPos(x - 1, z)))
            {
                m_CubeManager.RemoveMod(x - 1, y, z);
            }
        }

        if (x + 1 < HomeSize)
        {
            if (!JudgeIsBrickNeedCreat(x + 1, y, z, GetHeightByPos(x + 1, z)))
            {
                m_CubeManager.RemoveMod(x + 1, y, z);
            }
        }

        if (z - 1 >= 0)
        {
            if (!JudgeIsBrickNeedCreat(x, y, z - 1, GetHeightByPos(x, z - 1)))
            {
                m_CubeManager.RemoveMod(x, y, z - 1);
            }
        }

        if (z + 1 < HomeSize)
        {
            if (!JudgeIsBrickNeedCreat(x, y, z + 1, GetHeightByPos(x, z + 1)))
            {
                m_CubeManager.RemoveMod(x, y, z + 1);
            }
        }
        ReplaceBrick(x, y, z);
    }


    void RemoveBrickMod(int x, int y, int z)
    {
        m_CubeManager.RemoveMod(x, y, z);
        //检测是否有需要添加的点//该点的下，左右，前后//
        int height;
        if (y - 1 >= 1)
        {
            if (JudgeIsBrickNeedCreat(x, y - 1, z, GetHeightByPos(x, z)))
            {
                SetOneBrickMod(x, y - 1, z);
            }
        }

        if (x - 1 >= 0)
        {
            height = GetHeightByPos(x - 1, z);
            if (height > y)
            {
                if (JudgeIsBrickNeedCreat(x - 1, y, z, height))
                {
                    SetOneBrickMod(x - 1, y, z);
                }
            }
        }

        if (x + 1 < HomeSize)
        {
            height = GetHeightByPos(x + 1, z);
            if (height > y)
            {
                if (JudgeIsBrickNeedCreat(x + 1, y, z, height))
                {
                    SetOneBrickMod(x + 1, y, z);
                }
            }
        }

        if (z - 1 >= 0)
        {
            height = GetHeightByPos(x, z - 1);
            if (height > y)
            {
                if (JudgeIsBrickNeedCreat(x, y, z - 1, height))
                {
                    SetOneBrickMod(x, y, z - 1);
                }
            }
        }

        if (z + 1 < HomeSize)
        {
            height = GetHeightByPos(x, z + 1);
            if (height > y)
            {
                if (JudgeIsBrickNeedCreat(x, y, z + 1, height))
                {
                    SetOneBrickMod(x, y, z + 1);
                }
            }
        }
        ReplaceBrick(x, y, z);
    }

    void ReplaceBrick(int x, int y, int z)
    {
        if (x - 1 >= 0)
        {
            if (GetHeightByPos(x - 1, z) >= y)
            {
                SetOneBrickMod(x - 1, y, z);
            }
        }

        if (x + 1 < HomeSize)
        {
            if (GetHeightByPos(x + 1, z) >= y)
            {
                SetOneBrickMod(x + 1, y, z);
            }
        }

        if (z - 1 >= 0)
        {
            if (GetHeightByPos(x, z - 1) >= y)
            {
                SetOneBrickMod(x, y, z - 1);
            }
        }

        if (z + 1 < HomeSize)
        {
            if (GetHeightByPos(x, z + 1) >= y)
            {
                SetOneBrickMod(x, y, z + 1);
            }
        }
    }

    public void SetBrickOccluderOn()
    {
        Highlighter hl = BrickRoot.GetComponent<Highlighter>();
        if (hl == null)
        {
            hl = BrickRoot.AddComponent<Highlighter>();
        }
        hl.ReinitMaterials();
        hl.OccluderOn();
    }

    void ShowEditorBrickEffect()
    {
        if (bBrickAdd)
        {
            m_EditorBrickEffectManager.GetEffectObj("effect_brick_add");
        }
        else
        {
            m_EditorBrickEffectManager.GetEffectObj("effect_brick_delete");
        }
        int height = 0;
        for (int i = 0; i < HomeSize; i++)
        {
            for (int j = 0; j < HomeSize; j++)
            {
                if (bBrickAdd)
                {
                    height = GetHeightByPos(j, i) + 1;
                }
                else
                {
                    height = GetHeightByPos(j, i);
                }

                if (CheckCanEditorBrick(j, height, i, false))
                {
                    if (bBrickAdd)
                    {
                        m_EditorBrickEffectManager.AddEffect(j, height - 1, i);
                    }
                    else
                    {
                        m_EditorBrickEffectManager.AddEffect(j, height, i);
                    }
                }
            }
        }
    }

    void AddBrickEditorEffectInAdd(int x, int y, int z)
    {
        m_EditorBrickEffectManager.RemoveEffect(x, y - 1, z);
        if (CheckCanEditorBrick(x, y + 1, z, false))
        {
            m_EditorBrickEffectManager.AddEffect(x, y, z);
        }

        EditorBrickAroundEffect(x, z, true);

    }

    void RemoveBrickEditorEffectInRemove(int x, int y, int z)
    {
        m_EditorBrickEffectManager.RemoveEffect(x, y, z);
        if (CheckCanEditorBrick(x, y - 1, z, false))
        {
            m_EditorBrickEffectManager.AddEffect(x, y - 1, z);
        }

        EditorBrickAroundEffect(x, z, false);
    }

    void RemoveBrickEditorEffectInAdd(int x, int y, int z)
    {
        m_EditorBrickEffectManager.RemoveEffect(x, y, z);
        if (CheckCanEditorBrick(x, y, z, false))
        {
            m_EditorBrickEffectManager.AddEffect(x, y - 1, z);
        }

        EditorBrickAroundEffect(x, z, true);
    }

    void EditorBrickAroundEffect(int x, int z, bool bAdd)
    {
        int height;
        //还影响四周的砖块
        if (x - 1 >= 0)
        {
            height = GetHeightByPos(x - 1, z);
            m_EditorBrickEffectManager.RemoveEffect(x - 1, height, z);
            if (bAdd)
            {
                if (CheckCanEditorBrick(x - 1, height + 1, z, false))
                {
                    m_EditorBrickEffectManager.AddEffect(x - 1, height, z);
                }
            }
            else
            {
                if (CheckCanEditorBrick(x - 1, height, z, false))
                {
                    m_EditorBrickEffectManager.AddEffect(x - 1, height, z);
                }
            }
        }

        if (x + 1 < HomeSize)
        {
            height = GetHeightByPos(x + 1, z);
            m_EditorBrickEffectManager.RemoveEffect(x + 1, height, z);

            if (bAdd)
            {
                if (CheckCanEditorBrick(x + 1, height + 1, z, false))
                {
                    m_EditorBrickEffectManager.AddEffect(x + 1, height, z);
                }
            }
            else
            {
                if (CheckCanEditorBrick(x + 1, height, z, false))
                {
                    m_EditorBrickEffectManager.AddEffect(x + 1, height, z);
                }
            }
        }

        if (z - 1 >= 0)
        {
            height = GetHeightByPos(x, z - 1);
            m_EditorBrickEffectManager.RemoveEffect(x, height, z - 1);
            if (bAdd)
            {
                if (CheckCanEditorBrick(x, height + 1, z - 1, false))
                {
                    m_EditorBrickEffectManager.AddEffect(x, height, z - 1);
                }
            }
            else
            {
                if (CheckCanEditorBrick(x, height, z - 1, false))
                {
                    m_EditorBrickEffectManager.AddEffect(x, height, z - 1);
                }
            }
        }

        if (z + 1 < HomeSize)
        {
            height = GetHeightByPos(x, z + 1);
            m_EditorBrickEffectManager.RemoveEffect(x, height, z + 1);
            if (bAdd)
            {
                if (CheckCanEditorBrick(x, height + 1, z + 1, false))
                {
                    m_EditorBrickEffectManager.AddEffect(x, height, z + 1);
                }
            }
            else
            {
                if (CheckCanEditorBrick(x, height, z + 1, false))
                {
                    m_EditorBrickEffectManager.AddEffect(x, height, z + 1);
                }
            }
        }
    }

    void CleanEditorBrickEffect()
    {
        m_EditorBrickEffectManager.Reset();
    }

    #endregion

    #region 宝藏

    public Dictionary<int, BuildingHomeTreasureConfig> m_HomeTreasureCfg = new Dictionary<int, BuildingHomeTreasureConfig>();
    public Dictionary<int, Treasure> m_TreasureDic = new Dictionary<int, Treasure>();
    public List<int> mHinderBrick = new List<int>();
    void InitTreasure()
    {
        ConfigHoldUtility<BuildingHomeTreasureConfig>.LoadXml("Config/building_home_treasure", m_HomeTreasureCfg);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgHouseLandTreasure), OnTreasure);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgBuildTreasureReward), OnGetTreasure);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgHouseLandTreasureCleaning), OnGetTreasureCleaning);
    }

    int m_cleaningTreasure = 0;             //清理中宝藏
    int m_cleaningTreasureRealRestTime = 0;     //清理中剩余时间
    float m_cleaningTreasureRestTime = 0;

    public BuildingHomeTreasureConfig GetTreasureCfg(int id)
    {
        if (m_HomeTreasureCfg.ContainsKey(id))
        {
            return m_HomeTreasureCfg[id];
        }
        return null;
    }

    void OnTreasure(object sender, SCNetMsgEventArgs e)
    {
        SCMsgHouseLandTreasure msg = e.mNetMsg as SCMsgHouseLandTreasure; //strTreasureInfo格式：idPos(三维坐标),idTreasureCfg;... （x + z * 100） * 100 + y
        if (GameUtility.IsStringValid(msg.strTreasureInfo, ","))
        {
            string[] treasure_info = msg.strTreasureInfo.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < treasure_info.Length; i++)
            {
                string[] info = treasure_info[i].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                int id = int.Parse(info[0]);
                if (!m_TreasureDic.ContainsKey(id))
                {
                    Treasure treasure = new Treasure(id, int.Parse(info[1]));
                    m_TreasureDic.Add(id, treasure);

                    if (HomeRoot != null) //家园状态下新增加
                    {
                        CreatTreasure(id);
                    }
                }
            }
        }

        if (GameUtility.IsStringValid(msg.strClearList))
        {
            string[] clear_list = msg.strClearList.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < clear_list.Length; i++)
            {
                int id = int.Parse(clear_list[i]);
                if (m_TreasureDic.ContainsKey(id))
                {
                    Treasure treasure = m_TreasureDic[id];
                    treasure.isCleaned = true;
                    m_TreasureDic[id] = treasure;
                }
            }
        }
        //帮服务器重置数据//
        if (msg.nRemainTime <= 0)
        {
            if (m_TreasureDic.ContainsKey(m_cleaningTreasure))
            {
                Treasure treasure = m_TreasureDic[m_cleaningTreasure];
                treasure.isCleaned = true;
                m_TreasureDic[m_cleaningTreasure] = treasure;
            }
            m_cleaningTreasure = 0;
        }
        else
        {
            m_cleaningTreasure = msg.idCleaningPos;
            m_cleaningTreasureRealRestTime = msg.nRemainTime;
            m_cleaningTreasureRestTime = m_cleaningTreasureRealRestTime + Time.realtimeSinceStartup;
        }
    }

    void OnGetTreasure(object sender, SCNetMsgEventArgs e)
    {
        SCMsgBuildTreasureReward msg = e.mNetMsg as SCMsgBuildTreasureReward;
        Transform m_ObjectTreasure = TreasureRoot.transform.Find(msg.idPos.ToString());
        if (m_ObjectTreasure != null)
        {
            GameObject.Destroy(m_ObjectTreasure.gameObject);
        }
        if (m_TreasureDic.ContainsKey(msg.idPos))
        {
            m_TreasureDic.Remove(msg.idPos);
        }
    }

    void OnGetTreasureCleaning(object sender, SCNetMsgEventArgs e)
    {
        SCMsgHouseLandTreasureCleaning msg = e.mNetMsg as SCMsgHouseLandTreasureCleaning;
        m_cleaningTreasure = msg.idCleaningPos;
        m_cleaningTreasureRealRestTime = msg.nRemainTime;
        m_cleaningTreasureRestTime = m_cleaningTreasureRealRestTime + Time.realtimeSinceStartup;

        Transform m_ObjectTreasure = TreasureRoot.transform.Find(m_cleaningTreasure.ToString());
        if (m_ObjectTreasure != null)
        {
            BuildingHomeTreasureConfig treasureCfg = GetTreasureCfg(m_TreasureDic[m_cleaningTreasure].cfgId);
            Vector3 offset = new Vector3(treasureCfg.size_x * 0.5f - 0.5f, 0, treasureCfg.size_x * 0.5f - 0.5f);
            SetCleaningCountDown(m_ObjectTreasure.gameObject, offset);
        }
    }

    public void SendTreasureClean(int id)  //清理宝藏
    {
        CSMsgBuildTreasureClean msg = new CSMsgBuildTreasureClean();
        msg.idPos = id;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }

    public void SendTreasureGet(int id)   //领取宝藏
    {
        CSMsgBuildTreasureReward msg = new CSMsgBuildTreasureReward();
        msg.idPos = id;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }

    void SetAllTreasure()
    {
        foreach (int id in m_TreasureDic.Keys)
        {
            CreatTreasure(id);
        }
    }

    void CreatTreasure(int id)  //生成宝藏
    {
        BuildingHomeTreasureConfig treasureCfg;
        Treasure m_Treasure;
        m_Treasure = m_TreasureDic[id];
        treasureCfg = GetTreasureCfg(m_Treasure.cfgId);
        if (treasureCfg == null)
        {
            return;
        }

        GameObject m_ObjectTreasure;
        if (treasureCfg.type == 4)//障碍转（特殊方法实现,做一个空模型做辅助)
        {
            m_ObjectTreasure = new GameObject();
            if (!mHinderBrick.Contains(m_Treasure.id))
            {
                mHinderBrick.Add(m_Treasure.id);
            }
        }
        else
        {
            m_ObjectTreasure = ModelResourceManager.GetInst().GenerateObject(treasureCfg.model);
        }

        if (m_ObjectTreasure == null)
        {
            return;
        }
        m_ObjectTreasure.transform.SetParent(TreasureRoot.transform);
        m_ObjectTreasure.name = id.ToString();
        BoxCollider bc = m_ObjectTreasure.AddComponent<BoxCollider>();
        bc.size = new Vector3(treasureCfg.size_x, treasureCfg.size_z, treasureCfg.size_y);
        bc.center = new Vector3(0, treasureCfg.size_z * 0.5f, 0);
        EventTriggerListener mListener = m_ObjectTreasure.AddComponent<EventTriggerListener>();
        mListener.onClick = OnClickTreasure;
        GameUtility.SetLayer(m_ObjectTreasure, "InBuildObj");
        Vector3 offset = new Vector3(treasureCfg.size_x * 0.5f - 0.5f, 0, treasureCfg.size_x * 0.5f - 0.5f);
        int height = 0;
        if (treasureCfg.type == 2)//水下宝藏
        {
            height = GetMinHeight(m_Treasure.x, m_Treasure.y, treasureCfg.size_x);
            m_ObjectTreasure.transform.localPosition = new Vector3(m_Treasure.x, m_Treasure.height, m_Treasure.y) + offset;
            if (height >= m_Treasure.height)
            {
                m_ObjectTreasure.SetActive(true);
            }
            else
            {
                m_ObjectTreasure.SetActive(false);
            }
        }
        else
        {       //在有性能必要是添加水上的宝藏可见性,现在处理成全部可见//

            height = GetMaxHeight(m_Treasure.x, m_Treasure.y, treasureCfg.size_x);
            m_ObjectTreasure.transform.localPosition = new Vector3(m_Treasure.x, m_Treasure.height - 1, m_Treasure.y) + offset;
            if (height >= m_Treasure.height)
            {
                bc.enabled = false;
            }
            else
            {
                bc.enabled = true;
            }

            int treasureMaxHeight = m_Treasure.height + treasureCfg.size_z - 1;
            int minHeight = GetMinHeight(m_Treasure.x, m_Treasure.y, treasureCfg.size_x);
            if (minHeight < treasureMaxHeight)
            {
                m_ObjectTreasure.SetActive(true);
            }
            else
            {
                m_ObjectTreasure.SetActive(false);
            }

            if (treasureCfg.type == 4) //障碍转
            {
                m_ObjectTreasure.SetActive(true);
            }
        }

        if (treasureCfg.clean_time > 0)
        {
            if (m_Treasure.isCleaned)
            {
                SetTreasureState(m_ObjectTreasure, Color.white);
                mListener.SetTag(0);
            }
            else
            {
                SetTreasureState(m_ObjectTreasure, Color.cyan);
                mListener.SetTag(treasureCfg.clean_time);
            }
        }
        else
        {
            mListener.SetTag(0);
        }


        if (m_cleaningTreasure == id)
        {
            if (m_cleaningTreasureRealRestTime > 0)
            {
                SetCleaningCountDown(m_ObjectTreasure, offset);
            }
        }
    }

    void CleaningTreasureFinish(object data) //状态更改为清理完毕
    {
        GameObject m_ObjectTreasure = (GameObject)data;
        int id = int.Parse(m_ObjectTreasure.name);
        if (m_TreasureDic.ContainsKey(id))
        {
            Treasure treasure = m_TreasureDic[id];
            treasure.isCleaned = true;
            m_TreasureDic[id] = treasure;
        }
        SetTreasureState(m_ObjectTreasure, Color.white);
        EventTriggerListener.Get(m_ObjectTreasure).SetTag(0);
        m_cleaningTreasure = 0;
        m_cleaningTreasureRealRestTime = 0;
        m_cleaningTreasureRestTime = 0;

        BuildingHomeTreasureConfig treasureCfg = GetTreasureCfg(m_TreasureDic[id].cfgId);
        if (treasureCfg.type == 4)  //障碍砖特殊处理
        {
            int x = m_TreasureDic[id].x;
            int z = m_TreasureDic[id].y;
            int height = m_TreasureDic[id].height;

            SendTreasureGet(id);
            ChangeBrick(x, height - 1, z);
            if (HomeRoot != null)
            {
                RemoveBrickMod(x, height, z);
                if (m_state == HomeState.BrickEditor)
                {
                    if (bBrickAdd)
                    {
                        RemoveBrickEditorEffectInAdd(x, height, z);
                    }
                    else
                    {
                        RemoveBrickEditorEffectInRemove(x, height, z);
                    }
                }
            }
            mHinderBrick.Remove(id);
        }

    }

    void SetCleaningCountDown(GameObject m_ObjectTreasure, Vector3 offset)
    {
        GameObject m_CountDown = UIManager.GetInst().ShowUI_Multiple<UI_CountDown3D>("UI_CountDown3D");
        m_CountDown.GetComponent<UI_CountDown3D>().SetUp(m_cleaningTreasureRealRestTime, m_cleaningTreasureRestTime, m_ObjectTreasure.transform, CleaningTreasureFinish, m_ObjectTreasure, offset);
    }

    void OnClickTreasure(GameObject go, PointerEventData data)
    {
        int id = int.Parse(go.name);
        if (id == m_cleaningTreasure)
        {
            GameUtility.PopupMessage("正在清理中！");
            return;
        }

        int time = (int)EventTriggerListener.Get(go).GetTag();
        if (time > 0)
        {
            UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox").SetConfirmAndCancel("提示", "是否清洗宝藏？", ConfirmCleanTreasure, null, id);
        }
        else
        {
            if (CommonDataManager.GetInst().IsBagFull())
            {
                GameUtility.PopupMessage("背包已经满了,请先清理！");
                return;
            }
            SendTreasureGet(id);
        }
    }


    void ConfirmCleanTreasure(object data)
    {
        int id = (int)data;
        SendTreasureClean(id);
    }

    void CheckTreasureByRemove(int x, int z) //清砖时检测宝藏
    {
        Treasure m_Treasure = GetTreasureByIndex(x, z);
        BuildingHomeTreasureConfig treasureCfg = GetTreasureCfg(m_Treasure.cfgId);
        if (m_Treasure.id != 0)
        {
            if (treasureCfg.type == 2) //水下宝藏
            {
                //特效位置不做变化//
            }
            else
            {
                Transform m_ObjectTreasure = TreasureRoot.transform.Find(m_Treasure.id.ToString());
                int height = GetMaxHeight(m_Treasure.x, m_Treasure.y, treasureCfg.size_x);
                if (height < m_Treasure.height)
                {
                    m_ObjectTreasure.GetComponent<BoxCollider>().enabled = true;
                }

                int treasureMaxHeight = m_Treasure.height + treasureCfg.size_z - 1;
                int minHeight = GetMinHeight(m_Treasure.x, m_Treasure.y, treasureCfg.size_x);
                if (minHeight < treasureMaxHeight)
                {
                    m_ObjectTreasure.SetActive(true);
                }
                else
                {
                    m_ObjectTreasure.SetActive(false);
                }
            }
        }
    }

    void CheckTreasureByAdd(int x, int z) //添加时检测宝藏
    {
        Treasure m_Treasure = GetTreasureByIndex(x, z);
        BuildingHomeTreasureConfig treasureCfg = GetTreasureCfg(m_Treasure.cfgId);
        if (m_Treasure.id != 0)
        {
            Transform m_ObjectTreasure = TreasureRoot.transform.Find(m_Treasure.id.ToString());
            if (treasureCfg.type == 2) //水下宝藏
            {
                int min_heigh = GetMinHeight(m_Treasure.x, m_Treasure.y, treasureCfg.size_x);
                if (min_heigh >= m_Treasure.height)
                {
                    m_ObjectTreasure.SetActive(true);
                }
            }
            else
            {
                int treasureMaxHeight = m_Treasure.height + treasureCfg.size_z - 1;
                int minHeight = GetMinHeight(m_Treasure.x, m_Treasure.y, treasureCfg.size_x);
                if (minHeight < treasureMaxHeight)
                {
                    m_ObjectTreasure.SetActive(true);
                }
                else
                {
                    m_ObjectTreasure.SetActive(false);
                }
            }

        }
    }

    int GetMaxHeight(int x, int y, int size)
    {
        int max_height = 0;
        for (int i = y; i < y + size; i++)
        {
            for (int j = x; j < x + size; j++)
            {
                if (GetHeightByPos(j, i) > max_height)
                {
                    max_height = GetHeightByPos(j, i);
                }
            }
        }
        return max_height;
    }

    int GetMinHeight(int x, int y, int size)
    {
        int min_height = 100; //随便定的值
        for (int i = y; i < y + size; i++)
        {
            for (int j = x; j < x + size; j++)
            {
                if (GetHeightByPos(j, i) < min_height)
                {
                    min_height = GetHeightByPos(j, i);
                }
            }
        }
        return min_height;
    }

    Treasure GetTreasureByIndex(int x, int z)
    {
        BuildingHomeTreasureConfig treasureCfg;
        foreach (int id in m_TreasureDic.Keys)
        {
            treasureCfg = GetTreasureCfg(m_TreasureDic[id].cfgId);
            if (GameUtility.IsRectAllInRect(new Vector2(m_TreasureDic[id].x, m_TreasureDic[id].y), Vector2.one * treasureCfg.size_x, new Vector2(x, z), Vector2.one))
            {
                return m_TreasureDic[id];
            }
        }
        return new Treasure();
    }

    void SetTreasureState(GameObject obj, Color color)
    {
        MeshRenderer[] m_all_mr = obj.GetComponentsInChildren<MeshRenderer>();
        if (m_all_mr.Length > 0)
        {
            for (int i = 0; i < m_all_mr.Length; i++)
            {
                m_all_mr[i].material.color = color;
            }
        }
        else
        {
            SkinnedMeshRenderer[] m_all_Skinnedmr = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
            for (int i = 0; i < m_all_Skinnedmr.Length; i++)
            {
                m_all_Skinnedmr[i].material.color = color;
            }
        }
    }

    #endregion

    #region 制作 (包含科技)


    public Dictionary<int, BuildingMakeFormulaHold> m_BuildingMakeFormula = new Dictionary<int, BuildingMakeFormulaHold>();
    public Dictionary<int, BuildingMakeFormulaClassHold> m_BuildingMakeFormulaClassDict = new Dictionary<int, BuildingMakeFormulaClassHold>();
    public Dictionary<int, BuildingTechnologyConfig> m_BuildingTechnologyDict = new Dictionary<int, BuildingTechnologyConfig>();

    public Dictionary<int, int> mFinishTechnology = new Dictionary<int, int>(); //整理记录当前系列 最高等级

    void InitMake()
    {
        ConfigHoldUtility<BuildingMakeFormulaHold>.LoadXml("Config/building_make_formula", m_BuildingMakeFormula);
        ConfigHoldUtility<BuildingMakeFormulaClassHold>.LoadXml("Config/building_make_formula_class", m_BuildingMakeFormulaClassDict);
        ConfigHoldUtility<BuildingTechnologyConfig>.LoadXml("Config/building_technology", m_BuildingTechnologyDict);

        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgHouseBenchProduce), OnGetMakeFormula);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgBuildBench), OnGetMakeState);

        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgSciTech), OnGetAllTechnology);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgSciTechAdd), OnGetOneTechnology);
    }


    public BuildingMakeFormulaHold GetFormulaCfg(int id)
    {
        if (m_BuildingMakeFormula.ContainsKey(id))
        {
            return m_BuildingMakeFormula[id];
        }
        singleton.GetInst().ShowMessage(ErrorOwner.designer, "配方" + id + "不存在");
        return null;
    }

    public BuildingMakeFormulaClassHold GetFormulaClassCfg(int id)
    {
        if (m_BuildingMakeFormulaClassDict.ContainsKey(id))
        {
            return m_BuildingMakeFormulaClassDict[id];
        }
        return null;
    }


    public BuildingTechnologyConfig GetTechnologyCfg(int id)
    {
        if (m_BuildingTechnologyDict.ContainsKey(id))
        {
            return m_BuildingTechnologyDict[id];
        }
        return null;
    }

    public List<int> GetFormulaFromServer()  //该建筑的配方（服务器给我的）
    {
        List<int> formula_list = new List<int>();
        int buildId = GetSelectBuild().mBuildInfo.buildId;
        int level = GetSelectBuild().mBuildInfo.level;

        if (level > 0)
        {
            string formula_list_extra = PlayerController.GetInst().GetPropertyValue("formula_list");
            formula_list_extra += "|" + PlayerController.GetInst().GetPropertyValue("ex_formula_list");

            string[] formula = formula_list_extra.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            int id = 0;
            for (int i = 0; i < formula.Length; i++)
            {
                int.TryParse(formula[i], out id);
                if (id != 0)
                {
                    BuildingMakeFormulaHold bmh = GetFormulaCfg(id);
                    int furniture_id = bmh.furniture_id;
                    if (furniture_id / 100 == buildId && buildId * 100 + level >= furniture_id)
                    {
                        formula_list.Add(id);
                    }
                }
            }
        }
        return formula_list;
    }

    public SortedList<int, List<int>> GetMyFormulaList()
    {
        SortedList<int, List<int>> m_formula = new SortedList<int, List<int>>();
        //根据建筑id和等级收集  //先每次都遍历，以后再优化//
        List<int> formula_list = GetFormulaFromServer();

        for (int i = 0; i < formula_list.Count; i++)
        {
            int id = formula_list[i];
            BuildingMakeFormulaHold formula = GetFormulaCfg(id);
            if (formula == null)
            {
                singleton.GetInst().ShowMessage(ErrorOwner.designer, "配方id不存在！");
            }
            else
            {
                int kind = formula.class_id;
                if (m_formula.ContainsKey(kind))
                {
                    m_formula[kind].Add(id);
                }
                else
                {
                    List<int> list = new List<int>();
                    list.Add(id);
                    m_formula.Add(kind, list);
                }
            }
        }
        return m_formula;
    }


    public List<int> GetTechnologyList()  //检测科技的配方//
    {
        List<int> technology_list = new List<int>();
        int buildId = GetSelectBuild().mBuildInfo.buildId;
        int level = GetSelectBuild().mBuildInfo.level;

        if (level > 0)
        {
            foreach (int id in m_BuildingTechnologyDict.Keys)
            {
                int furniture_id = m_BuildingTechnologyDict[id].worktable_id;  //最小家具id
                if (furniture_id / 100 == buildId && buildId * 100 + level >= furniture_id)
                {
                    technology_list.Add(id);
                }
            }
        }
        return technology_list;
    }


    public SortedList<int, List<int>> GetShowTechnologyList()
    {
        SortedList<int, List<int>> m_technology = new SortedList<int, List<int>>();
        List<int> technology_list = GetTechnologyList();

        List<int> showList = new List<int>();
        for (int i = 0; i < technology_list.Count; i++)
        {
            int id = technology_list[i];
            int series = id / 100;
            int now_level = GetTechnologyLevel(series);

            int showId = series * 100 + now_level + 1;
            if (technology_list.Contains(showId))
            {
                if (!showList.Contains(showId))
                {
                    showList.Add(showId);
                }
            }
            else
            {
                showId -= 1;
                if (technology_list.Contains(showId))
                {
                    if (!showList.Contains(showId))
                    {
                        showList.Add(showId);
                    }
                }
            }
        }

        for (int i = 0; i < showList.Count; i++)
        {
            BuildingTechnologyConfig technologyCfg = GetTechnologyCfg(showList[i]);
            if (technologyCfg == null)
            {
                singleton.GetInst().ShowMessage(ErrorOwner.designer, "科技id不存在！");
            }
            else
            {
                int kind = technologyCfg.class_id;
                if (m_technology.ContainsKey(kind))
                {
                    m_technology[kind].Add(showList[i]);
                }
                else
                {
                    List<int> list = new List<int>();
                    list.Add(showList[i]);
                    m_technology.Add(kind, list);
                }
            }
        }
        return m_technology;
    }

    public int GetTechnologyLevel(int series)   //检测比这个科技小的科技是否都完成了
    {
        if (mFinishTechnology.ContainsKey(series))
        {
            return mFinishTechnology[series];
        }
        else
        {
            return 0;
        }
    }

    public void SendMakeFormula(int cfg_id)
    {
        CSMsgHouseBenchProduce msg = new CSMsgHouseBenchProduce();
        msg.id = GetSelectBuild().mBuildInfo.id;
        msg.cfg_id = cfg_id;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }

    public void SendMakeCancel()
    {
        CSMsgBenchCancelProduce msg = new CSMsgBenchCancelProduce();
        msg.id = GetSelectBuild().mBuildInfo.id;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }

    public void SendBenchQuick()
    {
        CSMsgQuickBench msg = new CSMsgQuickBench();
        msg.idrealbuild = GetSelectBuild().mBuildInfo.id;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }

    public void SendMakeTechnology(int cfg_id)
    {
        CSMsgResearchSciTech msg = new CSMsgResearchSciTech();
        msg.idrealbuild = GetSelectBuild().mBuildInfo.id;
        msg.sciId = cfg_id;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }

    void OnGetMakeFormula(object sender, SCNetMsgEventArgs e)
    {
        SCMsgHouseBenchProduce msg = e.mNetMsg as SCMsgHouseBenchProduce;
        AfterMakeSuc(msg.id);
    }

    void OnGetAllTechnology(object sender, SCNetMsgEventArgs e)
    {
        SCMsgSciTech msg = e.mNetMsg as SCMsgSciTech;
        mFinishTechnology.Clear();
        if (GameUtility.IsStringValid(msg.sciStr))
        {
            string[] temp = msg.sciStr.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < temp.Length; i++)
            {
                string[] info = temp[i].Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                int series = int.Parse(info[0]);
                int level = int.Parse(info[1]);
                mFinishTechnology.Add(series, level);
            }
        }
    }

    void OnGetOneTechnology(object sender, SCNetMsgEventArgs e)
    {
        SCMsgSciTechAdd msg = e.mNetMsg as SCMsgSciTechAdd;
        int series = msg.sciStr / 100;
        int level = msg.sciStr % 100;

        if (mFinishTechnology.ContainsKey(series))
        {
            mFinishTechnology[series] = level;
        }
        else
        {
            mFinishTechnology.Add(series, level);
        }

        //如果制造界面打开
        AfterMakeSuc(msg.id);

    }

    void AfterMakeSuc(long id)
    {
        UI_HomeBuildingMake uhm = UIManager.GetInst().GetUIBehaviour<UI_HomeBuildingMake>();
        if (uhm != null)
        {
            uhm.OnClickClose();
        }
            (m_BuildBehaviourDic[id] as BuildBenchBehaviour).PlayMakeFinishEffect();
    }


    Dictionary<long, SCMsgBuildBench> BenchInfoDic = new Dictionary<long, SCMsgBuildBench>();

    void OnGetMakeState(object sender, SCNetMsgEventArgs e)
    {
        SCMsgBuildBench msg = e.mNetMsg as SCMsgBuildBench;
        if (BenchInfoDic.ContainsKey(msg.id))
        {
            BenchInfoDic[msg.id] = msg;
        }
        else
        {
            BenchInfoDic.Add(msg.id, msg);
        }

        if (HomeRoot != null)
        {
            UI_HomeBuildingMake uhm = UIManager.GetInst().GetUIBehaviour<UI_HomeBuildingMake>();
            if (uhm != null)
            {
                uhm.OnClickClose();
            }
            BuildBenchBehaviour bb = m_BuildBehaviourDic[msg.id] as BuildBenchBehaviour;
            if (bb != null)
            {
                bb.SetMakeInfo(msg);
            }
        }
    }

    void ShowMakeTips()
    {
        foreach (long id in BenchInfoDic.Keys)
        {
            if (m_BuildBehaviourDic.ContainsKey(id))
            {
                BuildBenchBehaviour bb = m_BuildBehaviourDic[id] as BuildBenchBehaviour;
                if (bb != null)
                {
                    bb.SetMakeInfo(BenchInfoDic[id]);
                }
            }
        }
    }

    #endregion

    #region 广播台

    bool bHotelRefreshTime = false;
    float hotelRestTime;                    //刷新倒计时
    List<long> HotelNpcList = new List<long>();

    void InitHotel()
    {
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgBuildInnInfo), OnGetInnInfo);
    }

    public void AddHotelNpc(long id)
    {
        HotelNpcList.Add(id);
    }

    public void DeleteHotelNpc(long id)
    {
        if (HotelNpcList.Contains(id))
        {
            HotelNpcList.Remove(id);
        }
    }

    public long GetNextHotelNpcId(long id)
    {
        int index = HotelNpcList.IndexOf(id);
        if (index + 1 < HotelNpcList.Count)
        {
            return HotelNpcList[index + 1];
        }
        return HotelNpcList[0];
    }

    void OnGetInnInfo(object sender, SCNetMsgEventArgs e)
    {
        SCMsgBuildInnInfo msg = e.mNetMsg as SCMsgBuildInnInfo;

        if (msg.nRemianTime > 0)
        {
            bHotelRefreshTime = true;
            hotelRestTime = msg.nRemianTime + Time.realtimeSinceStartup;
        }
        else
        {
            bHotelRefreshTime = false;
        }
    }

    public void SendHotelGetHero(long npcId)
    {
        CSMsgHotelGetHero msg = new CSMsgHotelGetHero();
        msg.npcId = npcId;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }

    public void SendHotelRefresh(int type) // 0:时间刷新 1：钻石刷新
    {
        CSMsgBuildHotelRefresh msg = new CSMsgBuildHotelRefresh();
        msg.bySuper = type;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }

    public void SendHotelDisband(long npcId)
    {
        CSMsgBuildHotelLeave msg = new CSMsgBuildHotelLeave();
        msg.npcId = npcId;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }

    public int GetHotelRefreshCost()
    {
        string super_broadcast_price = GlobalParams.GetString("super_broadcast_price");
        string[] cost = super_broadcast_price.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        int hotelRefreshTime = PlayerController.GetInst().GetPropertyInt("day_inn_refresh_times");
        if (hotelRefreshTime >= cost.Length)
        {
            return int.Parse(cost[cost.Length - 1]);
        }
        return int.Parse(cost[hotelRefreshTime]);
    }

    public void HotelRefreshTime()
    {
        if (bHotelRefreshTime)
        {
            if (hotelRestTime - Time.realtimeSinceStartup <= 0)
            {
                SendHotelRefresh(0);
                bHotelRefreshTime = false;
            }
        }
    }

    #endregion

    #region 收获类建筑

    public Dictionary<int, BuildProduceHold> m_BuildingProduce = new Dictionary<int, BuildProduceHold>();
    Dictionary<long, float> m_ProduceTime = new Dictionary<long, float>();
    void InitGain()
    {
        ConfigHoldUtility<BuildProduceHold>.LoadXml("Config/building_produce", m_BuildingProduce);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgBuildProduce), OnGetBuildProduce);
    }

    public BuildProduceHold GetBuildingProduceCfg(int id)
    {
        if (m_BuildingProduce.ContainsKey(id))
        {
            return m_BuildingProduce[id];
        }
        return null;
    }

    void OnGetBuildProduce(object sender, SCNetMsgEventArgs e)
    {
        SCMsgBuildProduce msg = e.mNetMsg as SCMsgBuildProduce;
        float time = Time.realtimeSinceStartup - msg.pass_time;
        if (m_ProduceTime.ContainsKey(msg.id))
        {
            m_ProduceTime[msg.id] = time;

            if (HomeRoot != null && GetState() == HomeState.None)
            {
                if (m_BuildBehaviourDic.ContainsKey(msg.id))
                {
                    BuildProduceBehaviour bb = m_BuildBehaviourDic[msg.id] as BuildProduceBehaviour;
                    if (bb != null)
                    {
                        bb.CheckOutput();
                        if (msg.count > 0)
                        {
                            bb.ShowResAdd(msg.count);
                        }
                    }
                }
            }
        }
        else
        {
            if (m_BuildDic.ContainsKey(msg.id))
            {
                m_ProduceTime.Add(msg.id, time);
            }
            else
            {
                Debuger.Log("没有这个建筑！");
            }
        }
    }

    public void SendResourceGain(BuildProduceBehaviour bb)
    {
        if (bb.GetNowOutPut() <= 0)
        {
            GameUtility.PopupMessage("不要心急,还没有任何产出！");
            return;
        }

        string attr_name = CommonDataManager.GetInst().GetResourcesCfg(bb.ResType).attr;
        if (PlayerController.GetInst().GetPropertyLong(attr_name) == PlayerController.GetInst().GetPropertyLong(attr_name + "_capacity"))
        {
            GameUtility.PopupMessage("请先升级仓库！");
        }
        else
        {
            CSMsgHouseProduceCollect msg = new CSMsgHouseProduceCollect();
            msg.id = bb.mBuildInfo.id;
            NetworkManager.GetInst().SendMsgToServer(msg);
        }
    }

    public float GetOutPutTime(long id)
    {
        if (m_ProduceTime.ContainsKey(id))
        {
            return m_ProduceTime[id];
        }
        return -1;
    }

    #endregion

    #region 压力治疗

    Dictionary<int, BuildingCureConfig> m_BuildCureCfg = new Dictionary<int, BuildingCureConfig>(); //表里的建筑数据信息
    Dictionary<long, SCMsgBuildCure> CureInfoDic = new Dictionary<long, SCMsgBuildCure>();


    void InitCurePressure()
    {
        ConfigHoldUtility<BuildingCureConfig>.LoadXml("Config/building_cure", m_BuildCureCfg);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgBuildCure), OnGetBuildCure);
    }

    public BuildingCureConfig GetCurePressureCfg(int id)
    {
        if (m_BuildCureCfg.ContainsKey(id))
        {
            return m_BuildCureCfg[id];
        }
        return null;
    }

    void OnGetBuildCure(object sender, SCNetMsgEventArgs e)
    {
        SCMsgBuildCure msg = e.mNetMsg as SCMsgBuildCure;
        if (CureInfoDic.ContainsKey(msg.id))
        {
            CureInfoDic[msg.id] = msg;
        }
        else
        {
            CureInfoDic.Add(msg.id, msg);
        }
    }


    public SCMsgBuildCure GetBuildCureInfo(long id)
    {
        if (CureInfoDic.ContainsKey(id))
        {
            if (CureInfoDic.ContainsKey(id))
            {
                if (m_BuildDic[id].type == EBuildType.eCure)
                {
                    return CureInfoDic[id];
                }
                else
                {
                    BenchInfoDic.Remove(id);
                }
            }
        }
        return null;
    }

    public void SetCureBuildTime(long id, float rest_time)
    {
        if (HomeRoot != null)
        {
            if (m_BuildBehaviourDic.ContainsKey(id))
            {
                if (m_BuildBehaviourDic[id] is BuildCureBehaviour)
                {
                    (m_BuildBehaviourDic[id] as BuildCureBehaviour).SetCureTime(rest_time);
                }
            }
        }
    }

    #endregion

    #region 入住

    Dictionary<int, BuildingHabitancyConfig> m_BuildHabitancyDict = new Dictionary<int, BuildingHabitancyConfig>(); //表里的建筑数据信息
    void InitCheckIn()
    {
        ConfigHoldUtility<BuildingHabitancyConfig>.LoadXml("Config/building_habitancy", m_BuildHabitancyDict);
    }

    public BuildingHabitancyConfig GetBuildHabitancyCfg(int id)
    {
        if (m_BuildHabitancyDict.ContainsKey(id))
        {
            return m_BuildHabitancyDict[id];
        }
        return null;
    }

    #endregion

    #region 家园npc模型

    void CreatTestModel()
    {
        PhysicsRaycaster pr = HomeCamera.GetComponent<PhysicsRaycaster>();
        if (pr == null)
        {
            pr = HomeCamera.gameObject.AddComponent<PhysicsRaycaster>();
        }
        pr.eventMask = LayerMask.GetMask("Character");
        string test_model = GlobalParams.GetString("test_model");
        string[] temp = test_model.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (temp.Length >= 0)
        {
            for (int i = 0; i < temp.Length; i++)
            {
                SetTestModel(int.Parse(temp[i]), i);
            }
        }
        else
        {
            singleton.GetInst().ShowMessage("global_value表中的test_model字段格式错误！");
        }
    }

    void SetTestModel(int id, int index)
    {
        Vector3[] test_pos = XMLPARSE_METHOD.ConvertToVector3Array(GlobalParams.GetString("test_hero_pos"));
        CharacterConfig characterCfg = CharacterManager.GetInst().GetCharacterCfg(id);
        if (characterCfg != null)
        {
            GameObject test_model = CharacterManager.GetInst().GenerateModel(characterCfg);
            if (test_model != null)
            {
                test_model.transform.SetParent(HomeRoot.transform);
                test_model.transform.localPosition = test_pos[index];
                test_model.transform.localEulerAngles = Vector3.up * 180;
                test_model.AddComponent<TestHeroBehaviour>();
            }
        }
    }

    Dictionary<long, GameObject> PetObjDict = new Dictionary<long, GameObject>();
    void SetPetModelAll()
    {
        List<Pet> m_petlist = PetManager.GetInst().GetMyPetListState(PetState.Normal);
        for (int i = 0; i < m_petlist.Count; i++)
        {
            SetPetModelOne(m_petlist[i]);
        }
    }

    public void SetPetModelOne(Pet pet)
    {
        if (HomeRoot == null)
        {
            return;
        }

        GameObject pet_obj;
        if (PetObjDict.ContainsKey(pet.ID))
        {
            pet_obj = PetObjDict[pet.ID];
        }
        else
        {
            CharacterConfig characterCfg = CharacterManager.GetInst().GetCharacterCfg(pet.CharacterID);
            pet_obj = CharacterManager.GetInst().GenerateModel(characterCfg);
        }

        if (pet_obj != null)
        {

            GameUtility.SetLayer(pet_obj, "Character");
            pet_obj.name = pet.ID.ToString();
            if (!PetObjDict.ContainsKey(pet.ID))
            {
                if (pet.GetPropertyInt("cur_state") == (int)PetState.Normal)
                {
                    pet_obj.transform.SetParent(PetRoot.transform);
                    Vector3 pos = RandomPetFirstPoint();
                    pet_obj.transform.localPosition = pos;
                    pet_obj.transform.localEulerAngles = Vector3.up * 180;
                    ObjPath np = pet_obj.AddComponent<ObjPath>();
                    np.SetPetPath();
                }

                if (pet.GetPropertyInt("cur_state") == (int)PetState.Curing)
                {
                    SetPetCure(pet, pet_obj);
                }
                PetObjDict.Add(pet.ID, pet_obj);
            }
            else
            {

                if (pet.GetPropertyInt("cur_state") == (int)PetState.Normal)
                {
                    pet_obj.transform.SetParent(PetRoot.transform);
                    pet_obj.transform.localPosition = new Vector3((int)pet_obj.transform.localPosition.x, (int)pet_obj.transform.localPosition.y, (int)pet_obj.transform.localPosition.z);
                    pet_obj.transform.localEulerAngles = Vector3.up * 180;
                    SetObjMove(true, pet_obj);
                }

                if (pet.GetPropertyInt("cur_state") == (int)PetState.Curing)
                {
                    SetPetCure(pet, pet_obj);
                }

            }
        }
    }


    void SetPetCure(Pet pet, GameObject obj)
    {
        SetObjMove(false, obj);
        EBuildType type;
        GameObject bed = FindBuildModel(pet.GetPropertyLong("belong_build"), out type);
        obj.transform.SetParent(bed.transform);
        if (type == EBuildType.eCure)
        {
            obj.transform.localPosition = new Vector3(0.68f, 0, 0f);
            obj.transform.localEulerAngles = new Vector3(324f, 178f, 280f);
        }
        if (type == EBuildType.eSpecificity)
        {
            obj.transform.localPosition = new Vector3(0.7f, 0.5f, -0.1f);
            obj.transform.localEulerAngles = new Vector3(351f, 178f, 280f);
        }

    }

    public GameObject FindBuildModel(long mId, out EBuildType type)
    {
        type = EBuildType.eBegin;
        foreach (long id in m_BuildDic.Keys)
        {
            if (mId == id)
            {
                type = m_BuildDic[id].type;
                return m_BuildBehaviourDic[id].GetModelRoot();
            }
        }
        return null;
    }

    public void DeletePetObj(long id)
    {
        if (PetObjDict.ContainsKey(id))
        {
            GameObject.Destroy(PetObjDict[id]);
            PetObjDict.Remove(id);
        }
    }

    GameObject GetPetObj(long id)
    {
        if (PetObjDict.ContainsKey(id))
        {
            return PetObjDict[id];
        }
        return null;
    }

    Dictionary<long, GameObject> NpcObjDict = new Dictionary<long, GameObject>();
    void SetNpcModelAll()
    {
        Dictionary<long, NpcInfo> npcDict = NpcManager.GetInst().GetNpcDict();

        var NpcEnumerator = npcDict.GetEnumerator();
        try
        {
            while (NpcEnumerator.MoveNext())
            {
                SetNpcModelOne(NpcEnumerator.Current.Value);
            }
        }
        finally
        {
            NpcEnumerator.Dispose();
        }
    }

    public GameObject GetNpcObj(long id)
    {
        if (NpcObjDict.ContainsKey(id))
        {
            return NpcObjDict[id];
        }
        return null;
    }
    public GameObject GetNpcObjByType(int type)
    {
        foreach (NpcInfo info in NpcManager.GetInst().GetNpcDict().Values)
        {
            NpcConfig nc = NpcManager.GetInst().GetNpcCfg(info.idConfig);
            if (nc.type == 3)
            {
                return GetNpcObj(info.id);
            }
        }
        return null;
    }

    public void SetNpcModelOne(NpcInfo info)
    {
        if (NpcObjDict.ContainsKey(info.id))
        {
            return;
        }

        if (HomeRoot == null)
        {
            return;
        }

        NpcConfig nc = NpcManager.GetInst().GetNpcCfg(info.idConfig);
        if (nc != null)
        {
            GameObject npc = ModelResourceManager.GetInst().GenerateObject(nc.model);

            npc.transform.SetParent(NpcRoot.transform);
            ObjPath np = npc.AddComponent<ObjPath>();
            npc.transform.localPosition = GetNpcBornPos();
            npc.transform.localEulerAngles = new Vector3(0, UnityEngine.Random.Range(0, 360), 0);
            np.SetNpcPath(GetNpcPos(info.id, nc.type));

            npc.name = info.id.ToString();
            BoxCollider bc = npc.AddComponent<BoxCollider>();
            bc.size *= 1.5f;
            bc.center += new Vector3(0, bc.size.y * 0.5f, 0);
            EventTriggerListener listener = npc.AddComponent<EventTriggerListener>();
            listener.onClick = OnClickNpc;

            //头顶icon
            GameObject m_Icon = UIManager.GetInst().ShowUI_Multiple<UI_NpcIcon>("UI_NpcIcon");
            m_Icon.name = "UI_NpcIcon";
            m_Icon.transform.SetParent(npc.transform);
            m_Icon.transform.localPosition = Vector3.up * 2.0f;
            if (nc.type == 4)  //任务npc
            {
                TaskManager.GetInst().SetTaskNpcIcon(info.idConfig, m_Icon);
            }
            else
            {
                if (NpcManager.GetInst().IsNewNpc(info.id))
                {
                    m_Icon.GetComponent<UI_NpcIcon>().SetIcon("Npc#xin");
                }
                else
                {
                    m_Icon.GetComponent<UI_NpcIcon>().SetIcon(nc.head_icon);
                }
            }

            GameUtility.SetLayer(npc, "Character");

//             if (nc.type == 3)
//             {
//                 GuideManager.GetInst().CheckHomeNpcGuide(npc);
//             }
            NpcObjDict.Add(info.id, npc);
        }
    }

    public void CheckAllTaskNpcIcon()
    {
        Dictionary<long, NpcInfo> npcDict = NpcManager.GetInst().GetNpcDict();
        var NpcEnumerator = npcDict.GetEnumerator();
        try
        {
            while (NpcEnumerator.MoveNext())
            {
                NpcConfig nc = NpcManager.GetInst().GetNpcCfg(NpcEnumerator.Current.Value.idConfig);
                if (nc != null)
                {
                    if (nc.type == 4)
                    {
                        GameObject m_Icon = GetNpcObj(NpcEnumerator.Current.Key).transform.Find("UI_NpcIcon").gameObject;
                        TaskManager.GetInst().SetTaskNpcIcon(nc.id, m_Icon);
                    }
                }
            }
        }
        finally
        {
            NpcEnumerator.Dispose();
        }
    }

    public void DeleteNpcObj(long id, float delayTime = 0)
    {
        if (NpcObjDict.ContainsKey(id))
        {
            GameObject.Destroy(NpcObjDict[id], delayTime);
            NpcObjDict.Remove(id);
        }
        DeleteHotelNpc(id);
        RemoveSquarePoint(id);
        NpcManager.GetInst().RemoveNewNpc(id);
    }

    void OnClickNpc(GameObject go, PointerEventData data)
    {
        if (m_state == HomeState.None)
        {
            long id = long.Parse(go.name);
            NpcManager.GetInst().NpcTrigger(id);
            ResetBuildingState();
            //GuideManager.GetInst().RemoveNpcGuide(go);
        }
    }

    public void SetObjMove(bool isMove, GameObject obj)
    {
        if (obj != null)
        {
            ObjPath np = obj.GetComponent<ObjPath>();
            if (np != null)
            {
                if (isMove)
                {
                    np.Startgo();
                }
                else
                {
                    np.OnDisable();
                }
            }
        }
    }

    public void SetAllObjMove(bool isMove)
    {
        var PetObjEnumerator = PetObjDict.GetEnumerator();
        try
        {
            while (PetObjEnumerator.MoveNext())
            {
                SetObjMove(isMove, PetObjEnumerator.Current.Value);
            }
        }
        finally
        {
            PetObjEnumerator.Dispose();
        }


        var NpcObjEnumerator = NpcObjDict.GetEnumerator();
        try
        {
            while (NpcObjEnumerator.MoveNext())
            {
                SetObjMove(isMove, NpcObjEnumerator.Current.Value);
            }
        }
        finally
        {
            NpcObjEnumerator.Dispose();
        }
    }

    Vector3 cameraPosOri = Vector3.zero;
    Vector3 cameraRotationOri = Vector3.zero;
    float cameraFovOri = 0;
    Vector3 showObjPosOri;
    Vector3 showObjAnglesOri;
    Vector3[] showObjPos = { new Vector3(22.9f, 4, 22.9f), new Vector3(24.1f, 4f, 22.9f) };
    Vector3[] showObjAngles = { new Vector3(0, 183f, 0), new Vector3(0, 194f, 0) };
    GameObject showObjMod;

    public void ResetHomeCamera()
    {
        if (GameStateManager.GetInst().GameState == GAMESTATE.HOME)
        {
            UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", true);
            ResetCamera();
        }
    }

    void ResetCamera()
    {
        if (cameraPosOri == Vector3.zero)
        {
            return;
        }
        Camera.main.transform.position = cameraPosOri;
        Camera.main.transform.localEulerAngles = cameraRotationOri;
        Camera.main.fieldOfView = cameraFovOri;
        Camera.main.cullingMask |= 1 << 9;  //打开Character 
        cameraPosOri = Vector3.zero;
        ResetShowMod();
        ChangeStateUI(true);
        SetState(HomeState.None);
    }

    public void ResetShowMod()
    {
        if (showObjMod != null)
        {
            GameUtility.SetLayer(showObjMod, 9);
            showObjMod.transform.position = showObjPosOri;
            showObjMod.transform.localEulerAngles = showObjAnglesOri;
            SetObjMove(true, showObjMod);
            Transform UI_NpcIcon = showObjMod.transform.Find("UI_NpcIcon");
            if (UI_NpcIcon != null)
            {
                UI_NpcIcon.SetActive(true);
            }
            showObjMod = null;
        }
    }

    void SetCloseShotCamera()  //设置近景相机
    {
        Camera.main.transform.position = new Vector3(22.9f, 6.1f, 17.0f);
        Camera.main.transform.localEulerAngles = new Vector3(15.7f, 12.3f, 0);
        Camera.main.fieldOfView = 26f;
        Camera.main.cullingMask = ~(1 << 9);  //关闭Character 
        SetState(HomeState.ShotCamera);
    }

    public GameObject ChangeCameraForPet(long id)  //英雄界面模型近景//
    {
        showObjMod = GetPetObj(id);
        if (showObjMod == null)
        {
            return null;
        }
        SetCloseShotCamera();
        showObjPosOri = showObjMod.transform.position;
        showObjAnglesOri = showObjMod.transform.localEulerAngles;
        showObjMod.transform.localEulerAngles = showObjAngles[0];
        showObjMod.transform.position = showObjPos[0];
        GameUtility.SetLayer(showObjMod, 31);

        SetObjMove(false, showObjMod);

        ChangeStateUI(false);
        return showObjMod;
    }

    public GameObject ChangeCameraForNpc(long id)
    {
        showObjMod = GetNpcObj(id);
        if (showObjMod == null)
        {
            return null;
        }
        SetCloseShotCamera();

        showObjPosOri = showObjMod.transform.position;
        showObjAnglesOri = showObjMod.transform.localEulerAngles;
        showObjMod.transform.localEulerAngles = showObjAngles[1];
        showObjMod.transform.localPosition = showObjPos[1];
        GameUtility.SetLayer(showObjMod, 31);

        SetObjMove(false, showObjMod);
        showObjMod.transform.Find("UI_NpcIcon").gameObject.SetActive(false);

        ChangeStateUI(false);
        return showObjMod;
    }

    public void SaveCameraData()
    {
        cameraPosOri = Camera.main.transform.position;
        cameraRotationOri = Camera.main.transform.localEulerAngles;
        cameraFovOri = Camera.main.fieldOfView;
    }

    void ChangeStateUI(bool isshow)
    {
        foreach (long id in m_BuildBehaviourDic.Keys)
        {
            m_BuildBehaviourDic[id].SetStateIsShow(isshow);
        }
    }

    public Vector3 RandomPetFirstPoint()  //起始点
    {
        while (true)
        {
            int i = UnityEngine.Random.Range(0, HomeSize);
            int j = UnityEngine.Random.Range(0, HomeSize);
            if (m_HomePointData[j, i] != 1)
            {
                int height = GetHeightByPos(j, i);
                if (height >= BaseHeight)
                {
                    return new Vector3(j, height, i);
                }
            }
        }
    }

    public Vector3 RandomPetPoint(Vector2 m_point)
    {
        HashSet<int> hashset = new HashSet<int>();
        List<int> check_point = new List<int>();
        List<int> temp = new List<int>();
        int index = (int)m_point.x + (int)m_point.y * 100;
        hashset.Add(index);
        check_point.Add(index);
        int pointIndex;
        while (check_point.Count > 0)
        {
            temp.Clear();
            for (int i = 0; i < check_point.Count; i++)
            {
                int x = (int)check_point[i] % 100;
                int y = (int)check_point[i] / 100;
                //上
                if (y + 1 < HomeSize)
                {
                    if ((m_HomePointData[x, y + 1] != 1) && GetHeightByPos(x, y + 1) >= BaseHeight)
                    {
                        if (Mathf.Abs(GetHeightByPos(x, y + 1) - GetHeightByPos(x, y)) < 2)
                        {
                            pointIndex = x + (y + 1) * 100;
                            if (!hashset.Contains(pointIndex))
                            {
                                hashset.Add(pointIndex);
                                temp.Add(pointIndex);
                            }
                        }
                    }
                }
                //下
                if (y - 1 >= 0)
                {
                    if ((m_HomePointData[x, y - 1] != 1) && GetHeightByPos(x, y - 1) >= BaseHeight)
                    {
                        if (Mathf.Abs(GetHeightByPos(x, y - 1) - GetHeightByPos(x, y)) < 2)
                        {
                            pointIndex = x + (y - 1) * 100;
                            if (!hashset.Contains(pointIndex))
                            {
                                hashset.Add(pointIndex);
                                temp.Add(pointIndex);
                            }
                        }
                    }
                }
                //左
                if (x - 1 >= 0)
                {
                    if ((m_HomePointData[x - 1, y] != 1) && GetHeightByPos(x - 1, y) >= BaseHeight)
                    {
                        if (Mathf.Abs(GetHeightByPos(x - 1, y) - GetHeightByPos(x, y)) < 2)
                        {
                            pointIndex = x - 1 + y * 100;
                            if (!hashset.Contains(pointIndex))
                            {
                                hashset.Add(pointIndex);
                                temp.Add(pointIndex);
                            }
                        }
                    }
                }
                //右
                if (x + 1 < HomeSize)
                {
                    if ((m_HomePointData[x + 1, y] != 1) && GetHeightByPos(x + 1, y) >= BaseHeight)
                    {
                        if (Mathf.Abs(GetHeightByPos(x + 1, y) - GetHeightByPos(x, y)) < 2)
                        {
                            pointIndex = x + 1 + y * 100;
                            if (!hashset.Contains(pointIndex))
                            {
                                hashset.Add(pointIndex);
                                temp.Add(pointIndex);
                            }
                        }
                    }
                }
            }
            if (hashset.Count >= 150) //找150个点就差不多了不然客户端会卡顿//
            {
                break;
            }
            check_point = new List<int>(temp);
        }

        int pos = 0;
        if (hashset.Count > 0)
        {
            int[] mArray = new int[hashset.Count];
            hashset.CopyTo(mArray);
            pos = mArray[UnityEngine.Random.Range(0, mArray.Length)];
        }
        int posX = (int)pos % 100;
        int posZ = (int)pos / 100;

        return GetVector3Pos(new Vector2(posX, posZ));
    }

    List<int> SquarePointList = new List<int>();

    Dictionary<int, long> RecruitSquarePoint = new Dictionary<int, long>();
    Dictionary<long, Vector3> NpcSquarePoint = new Dictionary<long, Vector3>();

    public void GetRecruitSquarePoint()  //fuck
    {
        RecruitSquarePoint.Clear();
        int max_y = 0;
        for (int i = 0; i < SquarePointList.Count; i++)
        {
            int y = SquarePointList[i] / HomeSize;
            if (max_y < y)
            {
                max_y = y;
            }
        }
        List<int> temp = new List<int>();
        for (int i = 0; i < SquarePointList.Count; i++)
        {
            int y = SquarePointList[i] / HomeSize;
            if (max_y == y)
            {
                temp.Add(SquarePointList[i]);
            }
        }


        int index = temp[temp.Count / 2];  //获得队首

        int X = index % HomeSize;
        int Y = index / HomeSize;

        for (int i = 0; i < 100; i++)
        {
            int new_index = Y * HomeSize + X;
            if (SquarePointList.Contains(new_index))
            {
                RecruitSquarePoint.Add(new_index, 0);
            }
            else
            {
                break;
            }
            int offset = UnityEngine.Random.Range(1, 3);
            Y -= offset;
        }
    }

    void RemoveSquarePoint(long id)
    {
        if (NpcSquarePoint.ContainsKey(id))
        {
            NpcSquarePoint.Remove(id);
        }

        foreach (int m_index in RecruitSquarePoint.Keys)
        {
            if (RecruitSquarePoint[m_index] == id)
            {
                RecruitSquarePoint[m_index] = 0;
                break;
            }
        }
    }

    public Vector3 GetNpcBornPos()  //npc 出生时的位置
    {
        float center = HomeSize * 0.5f;
        float offset = 2.0f;

        int x = (int)UnityEngine.Random.Range(center - offset, center + offset);
        int y = 2;
        int height = GetHeightByPos(x, y);
        return new Vector3(x, height, y);
    }

    public Vector3 GetNpcPos(long id, int type)
    {
        int x, y;
        int index;

        if (type == 3) //招募npc排队
        {
            foreach (int m_index in RecruitSquarePoint.Keys)
            {
                if (RecruitSquarePoint[m_index] == 0)
                {
                    index = m_index;
                    x = index % HomeSize;
                    y = index / HomeSize;
                    int height = GetHeightByPos(x, y);
                    Vector3 pos = new Vector3(x, height, y);
                    RecruitSquarePoint[m_index] = id;
                    return pos;
                }
            }
            return -Vector3.one;
        }
        else
        {
            if (NpcSquarePoint.ContainsKey(id))
            {
                return NpcSquarePoint[id];
            }
            while (true)
            {
                index = SquarePointList[UnityEngine.Random.Range(0, SquarePointList.Count)];
                x = index % HomeSize;
                y = index / HomeSize;
                int height = GetHeightByPos(x, y);
                Vector3 pos = new Vector3(x, height, y);
                int noX = RecruitSquarePoint.Keys.First() % HomeSize;
                if (!NpcSquarePoint.Values.Contains(pos) && x != noX)
                {
                    NpcSquarePoint.Add(id, pos);
                    return pos;
                }
            }

        }
    }

    public Vector3 GetAnimalPos()
    {
        float center = HomeSize * 0.5f;
        float offset = 8.0f;

        while (true)
        {
            int x = (int)UnityEngine.Random.Range(center - offset, center + offset);
            int y = (int)UnityEngine.Random.Range(center - offset, center + offset);
            int height = GetHeightByPos(x, y);
            if (m_HomePointData[x, y] != 1)
            {
                return new Vector3(x, height, y);
            }
        }
    }

    public void ResetNpcMessager()  //由于外网网络可能导致突然响应多次npc点击所以在每次npc反馈时清空当前状态
    {
        if (HomeRoot != null)
        {
            UIManager.GetInst().CloseAllNormalWindow();
            ResetCamera();
        }
    }

    public void ChangeNpcNewIcon(long id, NpcConfig nc)
    {
        GameObject npcObj = GetNpcObj(id);
        if (npcObj != null)
        {
            Transform UI_NpcIcon = npcObj.transform.Find("UI_NpcIcon");
            if (UI_NpcIcon != null)
            {
                UI_NpcIcon.GetComponent<UI_NpcIcon>().SetIcon(nc.head_icon);
            }
        }
        NpcManager.GetInst().RemoveNewNpc(id);
    }


    public IEnumerator GotoHomeEntrance()
    {
        InputManager.GetInst().SwitchInup(false);
        if (HomeRoot == null)
        {
            InputManager.GetInst().SwitchInup(true);
            yield break;
        }

        int index = SquarePointList[SquarePointList.Count / 2];
        int x = index % HomeSize;
        int y = index / HomeSize;

        int height = GetHeightByPos(x, y);
        Vector3 pos = new Vector3(x, height, y);
        Vector3 camera_pos = SetPosOnCentre(pos, HomeCamera.transform);
        HomeCamera.transform.DOMove(camera_pos, 1.0f);
        yield return new WaitForSeconds(1.0f);

        InputManager.GetInst().SwitchInup(true);
    }

    #endregion

    #region 操作输入

    public void Update()
    {
        if (Camera.main == null)
        {
            return;
        }
        if (WorldMapManager.GetInst().IsShow)
        {
            return;
        }

        if (UIManager.GetInst().HasNormalUIOpen())
        {
            return;
        }

        if (m_state == HomeState.ShotCamera)
        {
            return;
        }

        InputManager.GetInst().UpdateGlobalInput();


#if UNITY_STANDALONE
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            Camera.main.transform.RotateAround(GetScreenCenterPos(), Vector3.up, -1f);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            Camera.main.transform.RotateAround(GetScreenCenterPos(), Vector3.up, 1f);
        }
#endif
        //世外桃源用//
        if (HomeRoot != null && isTestWorld)
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                GameObject.Destroy(HomeRoot);
                ResourceManager.GetInst().UnloadAllAB();
                LoadHome();
            }
        }
    }

    float MinCameraY = 10f;
    float MaxCameraY = 35f;
    float MinRotationX = 25f;
    float MaxRotationX = 60f;

    Vector2 m_vStartMousePos;
    Vector3 m_vStartCameraPos;
    Vector3 m_vStartCameraRot; //角度
    float oldDistance;
    float zoomSpeed;

    public void OnMouseScroll()
    {
        if (m_state == HomeState.Move)
        {
            return;
        }
        StopCameraYAutoTweener();
        HomeCameraHeight(Input.GetAxis("Mouse ScrollWheel") * 20);
    }

    public void OnMouseScrollTest()
    {
        var end = Camera.main.transform.position + Camera.main.transform.forward * Input.GetAxis("Mouse ScrollWheel") * 20;
        Camera.main.transform.position = end;
    }

    Coroutine ShowLongPress;
    UI_CountDown2D m_LongPressCountDown;
    public void OnClickDown()
    {
        m_vStartMousePos = Input.mousePosition;
        m_vStartCameraPos = Camera.main.transform.position;

        if (m_state == HomeState.BrickEditor)
        {
            ShowLongPress = AppMain.GetInst().StartCoroutine(ShowLongPressCountDown(InputManager.GetInst().LongPressSpace));
        }
    }

    bool bTestDarg = true;
    public void OnClickDownTest()
    {
        bTestDarg = true;
        if (InputManager.GetInst().IsPointerOverAnyThing())
        {
            bTestDarg = false;
            return;
        }

        m_vStartMousePos = Input.mousePosition;
        m_vStartCameraPos = Camera.main.transform.position;
        m_vStartCameraRot = Camera.main.transform.eulerAngles;
    }

    IEnumerator ShowLongPressCountDown(float time)
    {
        float delayTime = time * 0.2f;
        yield return new WaitForSeconds(delayTime);
        GameObject m_CountDown = UIManager.GetInst().ShowUI_Multiple<UI_CountDown2D>("UI_CountDown2D");
        m_CountDown.transform.SetParent(HomeRoot.transform);
        m_LongPressCountDown = m_CountDown.GetComponent<UI_CountDown2D>();
        m_LongPressCountDown.SetUp(time - delayTime);
    }

    void StopShowLongPressCountDown()
    {
        if (m_LongPressCountDown != null)
        {
            m_LongPressCountDown.Cancel();
        }
        if (ShowLongPress != null)
        {
            AppMain.GetInst().StopCoroutine(ShowLongPress);
        }
    }

    void EnterDarg()
    {
        if (m_state == HomeState.Move)
        {
            return;
        }
        StopCameraYAutoTweener();
        beginDragY = GetScreenCenterHeight();
    }

    void OnDarg()
    {
        if (m_state == HomeState.Move)
        {
            return;
        }
        StopShowLongPressCountDown();
        UpdateCameraMove();
    }

    void OnDargTest() //室外桃园拖动
    {
        if (!bTestDarg)
        {
            return;
        }

        if (Input.GetMouseButton(0)) //左键
        {
            Vector3 deltaX = (Input.mousePosition.x - m_vStartMousePos.x) * moveRatio * Camera.main.transform.right;
            Vector3 up = Quaternion.AngleAxis(90f, Vector3.up) * Camera.main.transform.right;
            Vector3 deltaZ = (Input.mousePosition.y - m_vStartMousePos.y) * moveRatio * up;
            Vector3 cameraPos = m_vStartCameraPos - (deltaX - deltaZ);
            Camera.main.transform.position = cameraPos;
        }
        if (Input.GetMouseButton(1)) //右键
        {
            Camera.main.transform.eulerAngles = m_vStartCameraRot + new Vector3(-(Input.mousePosition.y - m_vStartMousePos.y) / 5f, (Input.mousePosition.x - m_vStartMousePos.x) / 5f, 0f);
        }
    }

    float inertiaRatio = 0.001f;
    void OnDargOver()
    {
        if (m_state == HomeState.Move)
        {
            return;
        }

        var end = Camera.main.transform.position;

        end -= (Input.mousePosition.x - m_vStartMousePos.x) * inertiaRatio * Camera.main.transform.right;
        Vector3 up = Quaternion.AngleAxis(90f, Vector3.up) * Camera.main.transform.right;
        end += (Input.mousePosition.y - m_vStartMousePos.y) * inertiaRatio * up;

        if (IsInHomeSize(GetScreenCenterPos()))
        {
            float height = GetScreenCenterHeight();
            float diff = (beginDragY - height) * 0.8f;

            if (diff < 0 && Camera.main.transform.position.y > MaxCameraY)
            {
                diff = 0;
            }
            if (diff > 0 && Camera.main.transform.position.y < MinCameraY)
            {
                diff = 0;
            }
            end += Camera.main.transform.forward * diff;
        }

        if (IsCameraLimit(end))
        {
            CameraYAutoTweener = Camera.main.transform.DOLocalMove(end, 1.0f).SetEase(Ease.OutCubic);
        }
    }

    Tweener CameraYAutoTweener;
    public void StopCameraYAutoTweener()
    {
        if (CameraYAutoTweener != null)
        {
            CameraYAutoTweener.Kill();
        }
    }

    void OnClickUp()
    {
        if (m_state == HomeState.BrickEditor)
        {
            OnLongPressDargOver();
        }
    }

    void EnterLongPress()
    {
        UIManager.GetInst().GetUIBehaviour<UI_HomeMain>().ShowLongPressIcon(true);
        OnLongPressDarg();
    }

    Dictionary<int, GameObject> PreviewBrick = new Dictionary<int, GameObject>(); //长按拖动预览
    Vector2 old_hit_pos = -Vector2.one;
    void OnLongPressDarg()
    {
        RaycastHit hit = new RaycastHit();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int mask;
        mask = 1 << LayerMask.NameToLayer("BlockObj") | 1 << LayerMask.NameToLayer("InBuildObj");  //忽略宝藏和建筑
        if (Physics.Raycast(ray, out hit, Camera.main.farClipPlane, mask))
        {
        }
        else
        {
            mask = 1 << LayerMask.NameToLayer("Scene");
            if (Physics.Raycast(ray, out hit, Camera.main.farClipPlane, mask))
            {
                int x = (int)(hit.point.x + 0.5f);
                int z = (int)(hit.point.z + 0.5f);

                if (new Vector2(x, z) != old_hit_pos)
                {
                    int y = GetHeightByPos(x, z);
                    int index = x + z * HomeSize;
                    GameObject cube = null;
                    if (!PreviewBrick.ContainsKey(index))
                    {
                        if (bBrickAdd)
                        {
                            if (CheckCanEditorBrick(x, y + 1, z))
                            {
                                cube = EffectManager.GetInst().GetEffectObj("effect_set_brick_1");
                                cube.transform.position = new Vector3(x, y, z);
                                cube.transform.SetParent(HomeRoot.transform);
                                PreviewBrick.Add(index, cube);
                            }
                        }
                        else
                        {
                            if (PreviewBrick.Count + mCleaningBrick.Count < maxCleanBrickNum)
                            {
                                if (CheckCanEditorBrick(x, y, z))
                                {
                                    cube = EffectManager.GetInst().GetEffectObj("effect_raid_click_roadbed_002");
                                    GameUtility.GetTransform(cube, "fangkuang").gameObject.SetActive(false);
                                    cube.transform.position = new Vector3(x, y - 0.5f, z);
                                    cube.transform.SetParent(HomeRoot.transform);
                                    PreviewBrick.Add(index, cube);
                                }
                            }
                        }
                    }
                    old_hit_pos = new Vector2(x, z);
                }
            }
        }
        UIManager.GetInst().GetUIBehaviour<UI_HomeMain>().SetLongPressIconPosition();
    }

    void OnLongPressDargOver()
    {
        foreach (int index in PreviewBrick.Keys)
        {
            int x = index % HomeSize;
            int z = index / HomeSize;
            int y = GetHeightByPos(x, z);
            if (PreviewBrick[index].activeSelf)
            {
                if (bBrickAdd)
                {
                    if (y >= 0)
                    {
                        AddBrickHeight(x, z);
                    }
                }
                else
                {
                    if (y > 0)
                    {
                        RemoveBrickHeight(x, z);
                    }
                }
            }
            GameObject.Destroy(PreviewBrick[index]);
        }
        PreviewBrick.Clear();
        old_hit_pos = -Vector2.one;
        UIManager.GetInst().GetUIBehaviour<UI_HomeMain>().ShowLongPressIcon(false);
        StopShowLongPressCountDown();
    }

    Vector2 oldVec;
    Vector3 centerPos;
    void OnMultiTouchMove()
    {
        StopShowLongPressCountDown();
        if (m_state == HomeState.Move)
        {
            return;
        }

        StopCameraYAutoTweener();
        Touch finger0 = Input.GetTouch(0);
        Touch finger1 = Input.GetTouch(1);
        if (finger0.phase == TouchPhase.Began || finger1.phase == TouchPhase.Began)
        {
            oldDistance = Vector2.Distance(finger0.position, finger1.position);
            oldVec = finger1.position - finger0.position;
            centerPos = GetScreenCenterPos();
        }

        if (finger0.phase == TouchPhase.Moved || finger1.phase == TouchPhase.Moved)
        {
            Vector2 curVec = finger1.position - finger0.position;
            float angle = Vector2.Angle(oldVec, curVec);
            angle *= Mathf.Sign(Vector3.Cross(oldVec, curVec).z);
            HomeCamera.transform.RotateAround(centerPos, Vector3.up, angle * 0.7f);
            oldVec = curVec;

            float newDistance = Vector2.Distance(finger0.position, finger1.position);
            HomeCameraHeight(GameUtility.EnlargeSpeedByDistance(oldDistance, newDistance, zoomSpeed));
            oldDistance = newDistance;

        }
    }

    public void InputBinding()
    {
        InputManager.GetInst().UpdateInputReset(); //先清空代理
        InputManager.GetInst().SetbIgnoreUI(false);

        InputManager.GetInst().onClickDown = OnClickDown;
        InputManager.GetInst().onDarg = OnDarg;
        InputManager.GetInst().enterDarg = EnterDarg;
        InputManager.GetInst().onDargOver = OnDargOver;
        InputManager.GetInst().onMouseScroll = OnMouseScroll;
        InputManager.GetInst().onMultiTouchMove = OnMultiTouchMove;
        InputManager.GetInst().onClickUp = OnClickUp;
    }


    public void InputBindingForTest()  //室外桃园
    {
        InputManager.GetInst().UpdateInputReset(); //先清空代理
        InputManager.GetInst().SetbIgnoreUI(false);

        InputManager.GetInst().onClickDown = OnClickDownTest;
        InputManager.GetInst().onDarg = OnDargTest;
        InputManager.GetInst().onMouseScroll = OnMouseScrollTest;
    }

    void HomeCameraHeight(float speed)
    {
        Vector3 end = Vector3.zero;
        float rotationX = HomeCamera.transform.localEulerAngles.x;
        if (m_state != HomeState.BrickEditor)
        {
            if (HomeCamera.transform.position.y < (MaxCameraY + MinCameraY) * 0.4f)
            {
                HelpGameObject.transform.position = HomeCamera.transform.position;
                HelpGameObject.transform.localEulerAngles = HomeCamera.transform.localEulerAngles;
                rotationX += -speed * 1.5f;
                rotationX = Mathf.Clamp(rotationX, MinRotationX, MaxRotationX);
                Vector3 pos = GetScreenCenterPos();
                HelpGameObject.transform.localEulerAngles = new Vector3(rotationX, HomeCamera.transform.localEulerAngles.y, HomeCamera.transform.localEulerAngles.z);
                end = SetPosOnCentre(pos, HelpGameObject.transform) + HomeCamera.transform.forward * speed;
            }
            else
            {
                end = HomeCamera.transform.position + HomeCamera.transform.forward * speed;
            }
        }
        else
        {
            end = HomeCamera.transform.position + HomeCamera.transform.forward * speed;
        }

        if (end.y >= MaxCameraY || end.y <= MinCameraY)
        {
            return;
        }

        if (IsCameraLimit(end))
        {
            HomeCamera.transform.localEulerAngles = new Vector3(rotationX, HomeCamera.transform.localEulerAngles.y, HomeCamera.transform.localEulerAngles.z);
            HomeCamera.transform.position = end;
        }
    }

    float moveRatio;
    float beginDragY = 0;
    public void UpdateCameraMove()
    {
        Vector3 deltaX = (Input.mousePosition.x - m_vStartMousePos.x) * moveRatio * Camera.main.transform.right;
        Vector3 up = Quaternion.AngleAxis(90f, Vector3.up) * Camera.main.transform.right;
        Vector3 deltaZ = (Input.mousePosition.y - m_vStartMousePos.y) * moveRatio * up;
        Vector3 cameraPos = m_vStartCameraPos - (deltaX - deltaZ);

        if (IsCameraLimit(cameraPos))
        {
            Camera.main.transform.position = cameraPos;
        }
    }

    bool IsCameraLimit(Vector3 pos)
    {
        if (Vector3.Distance(pos, new Vector3(HomeSize * 0.5f - 0.5f, BaseHeight, HomeSize * 0.5f - 0.5f)) > 50)
        {
            return false;
        }
        return true;
    }

    #endregion

    #region Utility

    public Vector3 GetScreenCenterPos()
    {
        Vector3 pos = -Vector3.one;
        RaycastHit hit = new RaycastHit();
        Ray ray = Camera.main.ViewportPointToRay(Vector2.one * 0.5f);
        int mask;
        mask = 1 << LayerMask.NameToLayer("Water");
        if (Physics.Raycast(ray, out hit, Camera.main.farClipPlane, mask))
        {
            pos = hit.point;
        }
        return pos;
    }

    public Vector3 SetPosOnCentre(Vector3 pos, Transform tf)
    {
        float ratio = (pos.y - tf.position.y) / tf.forward.y;  //固定y坐标不变//
        Vector3 trans = pos - ratio * tf.forward;                   //计算出固定y坐标下摄像机的x,y坐标。
        return new Vector3(trans.x, tf.position.y, trans.z);
    }

    int GetScreenCenterHeight() //取屏幕中心点的5*5矩形
    {
        int size = 3;
        Vector3 pos = GetScreenCenterPos();
        int x = (int)pos.x;
        int z = (int)pos.z;
        int max_height = 0;
        int height = 0;
        for (int i = z - size; i <= z + size; i++)
        {
            for (int j = x - size; j <= x + size; j++)
            {
                if (IsInHomeSize(new Vector3(j, 0, i)))
                {
                    height = GetHeightByPos(j, i);
                    if (height > max_height)
                    {
                        max_height = height;
                    }
                }
            }
        }
        return max_height;
    }

    bool IsInHomeSize(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x <= HomeSize - 1 && pos.z >= 0 && pos.z <= HomeSize - 1)
        {
            return true;
        }
        return false;
    }

    public int GetMaxBuildHieght(int x, int y, int sizeX, int sizeY)
    {
        int max_height = 0;
        int height = 0;
        for (int i = x; i < x + sizeX; i++)
        {
            for (int j = y; j < y + sizeY; j++)
            {
                height = GetHeightByPos(i, j);
                if (height > max_height)
                {
                    max_height = height;
                }
            }
        }
        return max_height;
    }

    void ChangeCameraRotation(Vector3 angel)
    {
        Vector3 pos = GetScreenCenterPos();
        HomeCamera.transform.rotation = Quaternion.Euler(angel);
        HomeCamera.transform.position = SetPosOnCentre(pos, HomeCamera.transform);
    }

    #endregion

    #region 家园场景

    public static int HomeSize; //家园场景大小//
    public GameObject HomeRoot;
    public Camera HomeCamera;

    GameObject BrickRoot;
    GameObject BoxColliderRoot;
    GameObject FoundationRoot;
    GameObject BuildRoot;
    GameObject TreasureRoot;
    GameObject EffectRoot;
    GameObject PetRoot;
    GameObject NpcRoot;
    GameObject HelpGameObject;

    public void SetHomeRoot(string name)
    {
        HomeRoot = GameObject.Find(name);

        BrickRoot = new GameObject("BrickRoot");
        BrickRoot.transform.SetParent(HomeRoot.transform);
        m_CubeManager = BrickRoot.AddComponent<CubeManager>();

        BoxColliderRoot = new GameObject("BoxColliderRoot");
        BoxColliderRoot.transform.SetParent(HomeRoot.transform);

        TreasureRoot = new GameObject("TreasureRoot");
        TreasureRoot.transform.SetParent(HomeRoot.transform);

        FoundationRoot = new GameObject("FoundationRoot");
        FoundationRoot.transform.SetParent(HomeRoot.transform);

        BuildRoot = new GameObject("BuildRoot");
        BuildRoot.transform.SetParent(HomeRoot.transform);

        EffectRoot = new GameObject("EffectRoot");
        EffectRoot.transform.SetParent(HomeRoot.transform);
        m_EditorBrickEffectManager = EffectRoot.AddComponent<EditorBrickEffectManager>();

        PetRoot = new GameObject("PetRoot");
        PetRoot.transform.SetParent(HomeRoot.transform);

        NpcRoot = new GameObject("NpcRoot");
        NpcRoot.transform.SetParent(HomeRoot.transform);

        Transform water = HomeRoot.transform.Find("planebase");
        if (water != null)
        {
            water.gameObject.AddComponent<BoxCollider>();
            water.DOLocalRotate(Vector3.up * 180, 300).SetLoops(-1, LoopType.Restart);
        }

        HelpGameObject = new GameObject("help");
        HelpGameObject.transform.SetParent(HomeRoot.transform);

    }

    public void SetAmbient()
    {
        Transform ambient = HomeRoot.transform.Find("AmbientLight");
        if (ambient != null)
        {
            RenderSettings.ambientLight = new Color32((byte)ambient.position.x, (byte)ambient.position.y, (byte)ambient.position.z, 255);
        }
    }

    public void SetHomeCamera()
    {
        GameUtility.ResetCameraAspect();
        HomeCamera = Camera.main;
        HomeCamera.transform.position = GlobalParams.GetVector3("village_init_camera_position");
        HomeCamera.fieldOfView = GlobalParams.GetFloat("village_field_of_view");
        HomeCamera.transform.rotation = Quaternion.Euler(GlobalParams.GetVector3("village_camera_rot"));
        HomeCamera.nearClipPlane = 0.8f;
        Vector2 cameraHeight = GlobalParams.GetVector2("village_camera_hight");
        MinCameraY = cameraHeight.x;
        MaxCameraY = cameraHeight.y;

        SetHomeCameraEventMask(InitLayer);
        HighlightingBase hb = HomeCamera.gameObject.AddComponent<HighlightingMobile>(); //描边
        hb.SetParameterForHome();
    }

    string[] InitLayer = new string[] { "Character", "BlockObj", "Scene", "InBuildObj" };  //人物，建筑物或者宝藏，地砖
    void SetHomeCameraEventMask(params string[] layer)
    {
        PhysicsRaycaster pr = HomeCamera.GetComponent<PhysicsRaycaster>();
        if (pr == null)
        {
            pr = HomeCamera.gameObject.AddComponent<PhysicsRaycaster>();
        }
        pr.eventMask = LayerMask.GetMask(layer);
    }

    //以下是时间和天气//
    Vector3[] ShadowLightRotate;
    Color32[] ShadowLightColor;
    float[] ShadowLightIntensity;
    Vector3[] FillLightRotate;
    Color32[] FillLightColor;
    float[] FillLightIntensity;
    Vector3[] FilllightCharacterRotation;
    Color32[] FilllightCharacterColor;
    float[] FilllightCharacterIntensity;
    float gradualChangeTime; //变换时间
    float gradualStopTime;   //停留时间
    Color32[] AmbientLight;
    Light shadowlight;
    Light filllight;
    Light characterfilllight;

    Material matBase;
    Material matTile;
    Color32[] BasewaterColor;
    Color32[] TilewaterColor;
    Color32[] TileReflectionColor;
    Color32[] TileSpecularColor;
    Vector4[] TileDistortparams;
    Transform Rain;

    Sequence EnvironmentSequence;
    DayTime nowTime;
    bool bNowRain = false;  //当前是否下雨//
    bool bNextRain = false; //下一个时段是否下雨//

    void GetLightParameter()  //获取家园各种时间天气需要改变的参数一万个参数//
    {
        Rain = HomeCamera.transform.Find("rain");
        //获得各种灯光参数               
        ShadowLightRotate = XMLPARSE_METHOD.ConvertToVector3Array(GlobalParams.GetString("shadowlight_rotation"));
        ShadowLightColor = XMLPARSE_METHOD.ConvertToColor32Array(GlobalParams.GetString("shadowlight_color"));
        ShadowLightIntensity = XMLPARSE_METHOD.ConvertToFloatArray(GlobalParams.GetString("shadowlight_intensity"));

        FillLightRotate = XMLPARSE_METHOD.ConvertToVector3Array(GlobalParams.GetString("filllight_rotation"));
        FillLightColor = XMLPARSE_METHOD.ConvertToColor32Array(GlobalParams.GetString("filllight_color"));
        FillLightIntensity = XMLPARSE_METHOD.ConvertToFloatArray(GlobalParams.GetString("filllight_intensity"));
        //角色灯光//
        FilllightCharacterRotation = XMLPARSE_METHOD.ConvertToVector3Array(GlobalParams.GetString("filllightcharacter_rotation"));
        FilllightCharacterColor = XMLPARSE_METHOD.ConvertToColor32Array(GlobalParams.GetString("filllightcharacter_color"));
        FilllightCharacterIntensity = XMLPARSE_METHOD.ConvertToFloatArray(GlobalParams.GetString("filllightcharacter_intensity"));

        gradualChangeTime = GlobalParams.GetFloat("transition_time");
        gradualStopTime = GlobalParams.GetFloat("keep_time");

        AmbientLight = XMLPARSE_METHOD.ConvertToColor32Array(GlobalParams.GetString("ambientlight"));

        shadowlight = HomeRoot.transform.Find("shadowlight").GetComponent<Light>();
        filllight = HomeRoot.transform.Find("filllight").GetComponent<Light>();
        characterfilllight = HomeRoot.transform.Find("filllightcharacter").GetComponent<Light>();

        matBase = HomeRoot.transform.Find("shui/Water4Example (Simple)/basewater").GetComponent<MeshRenderer>().sharedMaterial;
        matTile = HomeRoot.transform.Find("shui/Water4Example (Simple)/Tile").GetComponent<MeshRenderer>().sharedMaterial;

        BasewaterColor = XMLPARSE_METHOD.ConvertToColor32Array(GlobalParams.GetString("basewater_color"));
        TilewaterColor = XMLPARSE_METHOD.ConvertToColor32Array(GlobalParams.GetString("Tile_basecolor"));
        TileReflectionColor = XMLPARSE_METHOD.ConvertToColor32Array(GlobalParams.GetString("Tile_reflectioncolor"));
        TileSpecularColor = XMLPARSE_METHOD.ConvertToColor32Array(GlobalParams.GetString("Tile_specularcolor"));
        TileDistortparams = XMLPARSE_METHOD.ConvertToVector4Array(GlobalParams.GetString("Tile_distortparams"));
    }

    void BeginLight(DayTime dayTime)
    {
        GetLightParameter();
        if (dayTime == DayTime.Night)
        {
            LightToNext(dayTime, false);
        }
        else
        {
            LightToNext(dayTime, IsRain());
        }
    }

    void LightToNext(DayTime dayTime, bool bNowRain)   //设定一个起始一个时间 ///0黄昏 1夜晚 2白天 3下雨
    {
        int timeIndex;
        this.bNowRain = bNowRain;
        EnterTime(dayTime);

        if (bNowRain)
        {
            timeIndex = 3; //雨天
            Rain.SetActive(true);
        }
        else
        {
            timeIndex = (int)dayTime;
            Rain.SetActive(false);
        }
        //设置初始值
        RenderSettings.ambientLight = AmbientLight[timeIndex]; //环境光//
                                                               //灯光//
        SetInitLight(shadowlight, ShadowLightRotate, ShadowLightColor, ShadowLightIntensity, timeIndex);
        SetInitLight(filllight, FillLightRotate, FillLightColor, FillLightIntensity, timeIndex);
        SetInitLight(characterfilllight, FilllightCharacterRotation, FilllightCharacterColor, FilllightCharacterIntensity, timeIndex);
        //水//
        matBase.SetColor("_Color", BasewaterColor[timeIndex]);
        matTile.SetColor("_BaseColor", TilewaterColor[timeIndex]);
        matTile.SetColor("_ReflectionColor", TileReflectionColor[timeIndex]);
        matTile.SetColor("_SpecularColor", TileSpecularColor[timeIndex]);
        matTile.SetVector("_DistortParams", TileDistortparams[timeIndex]);

        //确定下一个时段是否下雨
        if (dayTime == DayTime.Dusk)  //黄昏
        {
            bNextRain = false;
        }
        else
        {
            bNextRain = IsRain();
        }

        int nextIndex;
        if (bNextRain)
        {
            nextIndex = 3; //雨天
        }
        else
        {
            nextIndex = GetNewTime();
        }

        float stopTime;
        if (nextIndex == 3 || dayTime == DayTime.Night)
        {
            stopTime = gradualStopTime * 0.4f;
        }
        else
        {
            stopTime = gradualStopTime;
        }

        EnvironmentSequence = DOTween.Sequence();  //一个时间过度的动画//
        EnvironmentSequence
        .AppendInterval(stopTime)  //追加一个时间间隔
        .Append(DOTween.To(() => RenderSettings.ambientLight, x => RenderSettings.ambientLight = x, AmbientLight[nextIndex], gradualChangeTime))
        .Join(matBase.DOColor(BasewaterColor[nextIndex], "_Color", gradualChangeTime))
        .Join(matTile.DOColor(TilewaterColor[nextIndex], "_BaseColor", gradualChangeTime))
        .Join(matTile.DOColor(TileReflectionColor[nextIndex], "_ReflectionColor", gradualChangeTime))
        .Join(matTile.DOColor(TileSpecularColor[nextIndex], "_SpecularColor", gradualChangeTime))
        .Join(matTile.DOVector(TileDistortparams[nextIndex], "_DistortParams", gradualChangeTime));

        JoinLightTween(shadowlight, ShadowLightRotate, ShadowLightColor, ShadowLightIntensity, nextIndex);
        JoinLightTween(filllight, FillLightRotate, FillLightColor, FillLightIntensity, nextIndex);
        JoinLightTween(characterfilllight, FilllightCharacterRotation, FilllightCharacterColor, FilllightCharacterIntensity, nextIndex);

        EnvironmentSequence.AppendCallback(OnNextTime);
    }
    void OnNextTime()
    {
        LightToNext((DayTime)GetNewTime(), bNextRain);
    }

    int GetNewTime()
    {
        int timeIndex = (int)nowTime;
        int nextIndex = timeIndex + 1;
        if (nextIndex >= (int)DayTime.NO_LIGHT)
        {
            nextIndex = 0;
        }
        return nextIndex;
    }

    bool IsRain()  //50% 概率下雨
    {
        return GameUtility.RandomChance(30);
    }

    bool IsRabbit() //白天 黄昏 晴天是否出现兔子
    {
        return GameUtility.RandomChance(30);
    }

    void ShowRabbit()
    {
        if (IsRabbit())
        {
            if (HomeRoot == null)
            {
                return;
            }
            //Debug.Log("兔子出现了");
            GameObject rabbitObj = ModelResourceManager.GetInst().GenerateObject(4);
            if (rabbitObj != null)
            {
                rabbitObj.transform.SetParent(HomeRoot.transform);
                rabbitObj.transform.localPosition = GetAnimalPos();
                rabbitObj.transform.localEulerAngles = new Vector3(0, UnityEngine.Random.Range(0, 360), 0);
                ObjPath np = rabbitObj.AddComponent<ObjPath>();
                np.SetAnimalPath();
            }
        }
    }

    bool IsEagle() //白天 黄昏 雨天是否出现老鹰
    {
        return GameUtility.RandomChance(30);
    }

    void ShowEagle()
    {
        if (IsEagle())
        {
            if (HomeRoot == null)
            {
                return;
            }
            //Debug.Log("老鹰出现了");
            GameObject eagleObj = ModelResourceManager.GetInst().GenerateObject(54);
            if (eagleObj != null)
            {
                eagleObj.transform.SetParent(HomeRoot.transform);
                Vector3 pos = GetEagleInitialPos();
                eagleObj.transform.localPosition = pos;
                Vector3 homeCenter = new Vector3(HomeSize * 0.5f - 0.5f, 8, HomeSize * 0.5f - 0.5f);
                Vector3 target = new Vector3(homeCenter.x * 2 - pos.x, homeCenter.y * 2 - pos.y, homeCenter.z * 2 - pos.z);
                eagleObj.transform.LookAt(target);
                eagleObj.transform.DOMove(target, 25.0f).OnComplete(() =>
                {
                    GameObject.Destroy(eagleObj);
                });
            }
        }
    }

    Vector3 GetEagleInitialPos()
    {
        float offset = 10;
        float height = 10;
        Vector3 pos = Vector3.zero;
        int random = UnityEngine.Random.Range(0, 4);
        float value = UnityEngine.Random.Range(-offset, HomeSize + offset);
        switch (random)
        {
            case 0:
                pos = new Vector3(-offset, height, value);
                break;
            case 1:
                pos = new Vector3(value, height, -offset);
                break;
            case 2:
                pos = new Vector3(HomeSize + offset, height, value);
                break;
            case 3:
                pos = new Vector3(value, height, HomeSize + offset);
                break;
        }
        return pos;
    }

    void EnterTime(DayTime dayTime)
    {
        nowTime = dayTime;
        if (nowTime == DayTime.Day)
        {
            if (bNowRain)
            {
                //Debug.Log("进入白天雨天");
            }
            else
            {

                //Debug.Log("进入白天晴天");
            }
            AppMain.GetInst().StartCoroutine(ShowAnimal());

        }
        else if (nowTime == DayTime.Dusk)
        {
            if (bNowRain)
            {
                //Debug.Log("进入黄昏雨天"); 
            }
            else
            {
                //Debug.Log("进入黄昏晴天");
            }
            AppMain.GetInst().StartCoroutine(ShowAnimal());
        }
        else if (nowTime == DayTime.Night)
        {
            //Debug.Log("进入黑夜");
        }
    }

    IEnumerator ShowAnimal()
    {
        for (int i = 0; i < 3; i++)
        {
            ShowEagle();
            ShowRabbit();
            float waitTime = UnityEngine.Random.Range(10f, 20f);
            yield return new WaitForSeconds(waitTime);
        }
    }

    void SetInitLight(Light light, Vector3[] Rotation, Color32[] Color, float[] Intensity, int timeIndex)
    {
        if (light == null || Rotation.Length < timeIndex || Color.Length < timeIndex || Intensity.Length < timeIndex)
        {
            return;
        }
        //初始值//
        light.transform.rotation = Quaternion.Euler(Rotation[timeIndex]);
        light.color = Color[timeIndex];
        light.intensity = Intensity[timeIndex];
    }

    void JoinLightTween(Light light, Vector3[] Rotation, Color32[] Color, float[] Intensity, int timeIndex)
    {
        EnvironmentSequence
        .Join(light.DOColor(Color[timeIndex], gradualChangeTime))
        .Join(light.transform.DORotate(Rotation[timeIndex], gradualChangeTime))
        .Join(light.DOIntensity(Intensity[timeIndex], gradualChangeTime));
    }

    #endregion

    List<int> m_UnNewFurnitureList = new List<int>();
    public void AddUnNewFurniture(int id)
    {
        if (!m_UnNewFurnitureList.Contains(id))
        {
            m_UnNewFurnitureList.Add(id);
        }
    }

    public void SaveUnNewFurnitureList()
    {
        string ret = "";
        for (int i = 0; i < m_UnNewFurnitureList.Count; i++)
        {
            ret += m_UnNewFurnitureList[i];
            if (i < m_UnNewFurnitureList.Count - 1)
            {
                ret += ",";
            }
        }
        GameUtility.SavePlayerData(PlayerController.GetInst().PlayerID + "UnNewFurniture", ret);
    }
    public void LoadUnNewFurnitureList()
    {
        string tmp = GameUtility.GetPlayerData(PlayerController.GetInst().PlayerID + "UnNewFurniture");
        m_UnNewFurnitureList = GameUtility.ToList<int>(tmp, ',', (x) => int.Parse(x));
    }

    public bool IsNewFurniture(int id)
    {
        return !m_UnNewFurnitureList.Contains(id);
    }
    public void UnNewFurnitureList(int tab)
    {
        foreach (int id in GetBuildFurnitureList(tab + 1, MainBuildLevel))
        {
            AddUnNewFurniture(id);
        }
        SaveUnNewFurnitureList();
    }

    public int CheckNewFurnitureList(int tab = -1)
    {
        List<int> list = new List<int>();
        for (int i = 0; i < (int)UI_FurnitureList.FURNITURE_TAB.MAX; i++)
        {
            if (tab > -1 && i != tab)
                continue;
            foreach (int id in GetBuildFurnitureList(i + 1, MainBuildLevel))
            {
                if (!m_UnNewFurnitureList.Contains(id))
                {
                    list.Add(id);
                }
            }
        }
        return list.Count;
    }
}

//脚本objpath//
public enum ObjType
{
    PET = 0,    //自己的宠物 在家园内随意走动//
    NPC = 1,    //npc 从屏幕最下方走到广场,只走一次//
    ANIMAL = 2, //动物在家园中心区域走动//
}

