using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum LINK_DIRECTION
{
        NORTH,
        WEST,
        SOUTH,
        EAST,
        MAX
}
public enum ROOM_FLAG
{
        LOCKED,
        DETECTED,
        COMPLETED,
        ACTIVE,
        BROKEN,
};

public enum ROOM_TYPE
{
        NONE = 0,
        Normal = 1,  //普通（正分）
        Treasure = 2,           //宝物（正分）
        Curse = 3,              //诅咒（负分）
        Locked = 4,             //上锁（正分）
        Hide = 5,               //隐藏（正分）
        Shop = 6,               //商店（正分）
        Destiny = 7,            //命运（正分）
        Error = 8,              //error（负分）
        Bedroom = 9,            //卧室（正分）
        Sacrifice = 10,         //牺牲（正分）
        Enterance = 11,        //入口
        NegNormal = 12,         //普通（负分） 
        NegTreasure = 13,    //宝物（负分）
        Story = 14,                     //剧情
        Elite = 15,                     //精英
        Boss = 16,                      //boss
        Challenge = 17,         //挑战
        Gamble = 18,            //赌博
        Demon_Angle = 19,  //恶魔/天使
        Exit = 20,                      //出口
        WILD = 21,                //野外
        DARK = 22,               //黑暗
        Voracity = 23,           //贪婪
        Adventure = 24,       //冒险
        Choose = 25,            //选择
        Roadblock = 26,       ///路障
        Trap = 27,                 //陷阱
        DropTrap = 28,         //掉落陷阱
        END,
}

public class RaidRoomBehav : MonoBehaviour
{
        public int pieceCfgId;
        public int index;       //y * m_nMapCount + x;

        public int idOffset
        {
                get
                {
                        return index * 10000;
                }
        }

        public RaidInfoRandomConfig pieceInfoCfg;
        public int RealSizeX
        {
                get
                {
                        if (pieceInfoCfg != null)
                        {
                                if (m_nRotateDirect == 1 || m_nRotateDirect == 3)
                                {
                                        return pieceInfoCfg.sizey;
                                }
                                else
                                {
                                        return pieceInfoCfg.sizex;
                                }
                        }
                        return 0;
                }
        }
        public int RealSizeY
        {
                get
                {
                        if (pieceInfoCfg != null)
                        {
                                if (m_nRotateDirect == 1 || m_nRotateDirect == 3)
                                {
                                        return pieceInfoCfg.sizex;
                                }
                                else
                                {
                                        return pieceInfoCfg.sizey;
                                }
                        }
                        return 0;
                }
        }

        public int m_nRoomFlag;
        public int RoomFlag
        {
                get
                {
                        return m_nRoomFlag;
                }
                set
                {
                        m_nRoomFlag = value;
                        IsDetected =  IsFlagOn(ROOM_FLAG.DETECTED);
                        IsActive = IsFlagOn(ROOM_FLAG.ACTIVE);
                        IsComplete = IsFlagOn(ROOM_FLAG.COMPLETED);
                        IsLocked = IsFlagOn(ROOM_FLAG.LOCKED);
                        IsBroken = IsFlagOn(ROOM_FLAG.BROKEN);
                        /*                        Debug.Log("Room " + index + " Active=" + IsActive + " Complete = " + IsComplete + " Lock=" + IsLocked + " Detected=" + IsDetected);*/
                }
        }

        public void SetFlag(ROOM_FLAG flag, bool bEnable = true)
        {
                if (bEnable)
                {
                        RoomFlag |= GameUtility.GetFlagValInt((int)flag);
                }
                else
                {
                        RoomFlag = GameUtility.ClearFlagBit(RoomFlag, (int)flag);
                }

        }
        public bool IsFlagOn(ROOM_FLAG flag)
        {
                return GameUtility.IsFlagOn(RoomFlag, (int)flag);
        }
        public bool IsHide
        {
                get
                {
                        if (IsDetected)
                        {
                                return false;
                        }
                        return pieceType == (int)ROOM_TYPE.Hide;
                }
        }
        public bool IsDetected;
        public bool IsActive;
        public bool IsComplete;
        public bool IsLocked;
        public bool IsBroken;
        /// <summary>
        /// 相邻方向的分片index字典。如果为空或者不想通，则没有。
        /// </summary>

        public List<string> m_LinkStatus = new List<string>();
        Dictionary<int, Dictionary<LINK_DIRECTION, int>> m_LinkDict = new Dictionary<int, Dictionary<LINK_DIRECTION, int>>();

        public Dictionary<int, Dictionary<LINK_DIRECTION, int>> GetLinkDict()
        {
                return m_LinkDict;
        }

        public int GetLinkRoomIndex(LINK_DIRECTION direct)
        {
                return GetLinkRoomIndex(this.index, direct);
        }
        public int GetLinkRoomIndex(int subIndex, LINK_DIRECTION direct)
        {
                if (m_LinkDict.ContainsKey(subIndex))
                {
                        if (m_LinkDict[subIndex].ContainsKey(direct))
                        {
                                return m_LinkDict[subIndex][direct];
                        }
                }
                return -1;
        }
        public int GetLinkIndexByDoor(RaidNodeBehav door)
        {
                foreach (var param in m_DoorDict)
                {
                        if (param.Value.doorId == door.id)
                        {
                                return param.Key;
                        }
                }
                return -1;
        }

        /// <summary>
        /// 是否随机分片
        /// </summary>
        public bool IsRandomPiece
        {
                get
                {
                        if (pieceInfoCfg != null)
                        {
                                return pieceInfoCfg.is_random == 1;
                        }
                        return false;
                }
        }
        public bool IsRoadPiece
        {
                get
                {
                        return pieceType == (int)ROOM_TYPE.Normal;
                }
        }
        public int pieceType
        {
                get
                {
                        if (pieceInfoCfg != null)
                        {
                                return pieceInfoCfg.type;
                        }
                        return 0;
                }
        }
        public int floorHeight
        {
                get
                {
                        if (pieceInfoCfg != null)
                        {
                                return pieceInfoCfg.floor_height;
                        }
                        return 0;
                }
        }

        public int index_X;
        public int index_Y;
        public List<int> subIndexList = new List<int>();
        Dictionary<int, int> linkStateDict = new Dictionary<int, int>();

        public int posX
        {
                get
                {
                        return (int)this.transform.position.x;
                }
        }
        public int posY
        {
                get
                {
                        return (int)this.transform.position.z;
                }
        }
        public List<RaidNodeBehav> nodeList = new List<RaidNodeBehav>();
        /// <summary>
        /// 房间内交互元素列表
        /// </summary>
        public List<RaidNodeBehav> interactiveList = new List<RaidNodeBehav>();

        public GameObject nodeRoot;
        public GameObject interactiveRoot;
        public GameObject insideRoot;
        public GameObject nonBlockRoot;
        public GameObject blockRoot;
        Dictionary<int, BoxCollider> m_ColliderDict = new Dictionary<int, BoxCollider>();
        public void AddBoxCollider(int x, int z, bool bLog = false)
        {
                int height = 0;
                int id = x * 1000 + z;
                if (floorDict.ContainsKey(id))
                {
                        height = floorDict[id].Count;
                }
                bool bBlock = FloorY + height != 0;
                BoxCollider collider;
                if (!m_ColliderDict.ContainsKey(id))
                {
                        if (bBlock)
                        {
                                collider = blockRoot.AddComponent<BoxCollider>();
                        }
                        else
                        {
                                collider = nonBlockRoot.AddComponent<BoxCollider>();
                        }
                        m_ColliderDict.Add(id, collider);
                }
                else
                {
                        collider = m_ColliderDict[id];
                }
                collider.size = new Vector3(1f, height, 1f);
                collider.center = new Vector3(x - this.posX, FloorY + height / 2f, z - this.posY);
        }

        Dictionary<LINK_DIRECTION, GameObject> m_DirectionPartObjs = new Dictionary<LINK_DIRECTION, GameObject>();
        Dictionary<LINK_DIRECTION, bool> m_DirectionPartVisible = new Dictionary<LINK_DIRECTION, bool>();

        public void InitRoots()
        {
                nodeRoot = new GameObject("Room_Node");
                nodeRoot.transform.SetParent(this.gameObject.transform);
                nodeRoot.transform.localPosition = Vector3.zero;
                nodeRoot.transform.localRotation = Quaternion.identity;

                interactiveRoot = new GameObject("ElemRoot");
                interactiveRoot.transform.SetParent(this.gameObject.transform);
                interactiveRoot.transform.localPosition = Vector3.zero;
                interactiveRoot.transform.localRotation = Quaternion.identity;

                insideRoot = new GameObject("Room_Inside");
                insideRoot.transform.SetParent(this.gameObject.transform);
                insideRoot.transform.localPosition = Vector3.zero;
                insideRoot.transform.localRotation = Quaternion.identity;

                nonBlockRoot = new GameObject("CollisionNonBlock_Root");
                nonBlockRoot.transform.SetParent(this.transform);
                nonBlockRoot.transform.localPosition = Vector3.zero;
                nonBlockRoot.transform.localRotation = Quaternion.identity;
                GameUtility.SetLayer(nonBlockRoot, "NonBlockObj");

                blockRoot = new GameObject("CollisionBlock_Root");
                blockRoot.transform.SetParent(this.transform);
                blockRoot.transform.localPosition = Vector3.zero;
                blockRoot.transform.localRotation = Quaternion.identity;
                GameUtility.SetLayer(blockRoot, "BlockObj");

                for (LINK_DIRECTION direct = LINK_DIRECTION.NORTH; direct < LINK_DIRECTION.MAX; direct++)
                {
                        GameObject obj = new GameObject("ROOM_" + direct);
                        obj.transform.SetParent(this.gameObject.transform);
                        obj.transform.localPosition = Vector3.zero;
                        obj.transform.localRotation = Quaternion.identity;
                        m_DirectionPartObjs.Add(direct, obj);
                }
                InitLinks();
        }

        public void SetLinkState(Dictionary<int, int> dict)
        {
                linkStateDict = dict;
                subIndexList = new List<int>(dict.Keys);
        }
        public bool AddLinkState(int subIndex, int linkIndex, LINK_DIRECTION direct)
        {
                if (m_LinkDict.ContainsKey(subIndex))
                {
                        if (!m_LinkDict[subIndex].ContainsKey(direct))
                        {
                                m_LinkDict[subIndex].Add(direct, linkIndex);
                                m_LinkStatus.Add(subIndex + "--" + direct + "--" + linkIndex);
                                return true;
                        }
                }
                return false;
        }

        int m_nRotateDirect = 2;
        float m_fAllRot = 0f;
        public float RotOffset
        {
                get
                {
                        return m_fAllRot;
                }
        }

        public Vector3 GetRotatedPos(Vector3 pos)
        {
                int x = (int)pos.x;
                int y = (int)pos.z;
                CalcRotate(ref x, ref y);
                
                return new Vector3(this.posX + x, pos.y, this.posY + y);
        }

        public void CalcRotate(ref int oriX, ref int oriY)
        {
                int x = oriX;
                int y = oriY;
                switch (m_nRotateDirect)
                {
                        case 0:
                                x = pieceInfoCfg.sizex - oriX - 1;
                                y = pieceInfoCfg.sizey - oriY - 1;
                                m_fAllRot = 180f;
                                break;
                        case 1:
                                x = oriY;
                                y = pieceInfoCfg.sizex - oriX - 1;
                                m_fAllRot = 90f;
                                break;
                        default:
                        case 2:
                                m_fAllRot = 0f;
                                break;
                        case 3:
                                x = pieceInfoCfg.sizey - oriY - 1;
                                y = oriX;
                                m_fAllRot = 270f;
                                break;
                }
                oriX = x;
                oriY = y;
        }

        void InitLinks()
        {
                int linkstate = 0;
                m_LinkDict.Clear();
                foreach (int subIndex in subIndexList)
                {
                        m_LinkDict.Add(subIndex, new Dictionary<LINK_DIRECTION, int>());
                        for (LINK_DIRECTION direct = LINK_DIRECTION.NORTH; direct < LINK_DIRECTION.MAX; direct++)
                        {
                                if (IsLinked(subIndex, direct))
                                {
                                        int linkIndex = RaidManager.GetInst().GetLinkIndex(subIndex, direct);
                                        if (!subIndexList.Contains(linkIndex))
                                        {
                                                linkstate |= GameUtility.GetFlagValInt((int)direct);
                                                m_LinkDict[subIndex].Add(direct, linkIndex);
                                                //       Debug.Log("Room " + this.index + " sub=" + subIndex + " AddLink " + direct + " " + m_LinkDict[subIndex][direct]);
                                        }
                                }
                        }
                }
                m_nRotateDirect = CalcRotateIndex(pieceInfoCfg.door_state, linkstate);
        }

        int CalcRotateIndex(int doorstate, int linkstate)
        {
                if (IsRandomPiece)
                        return 2;

                for (int i = 0; i < 4; i++)
                {
                        if (doorstate == linkstate)
                        {
                                return (i + 2) % 4;
                        }
                        bool state_3 = GameUtility.IsFlagOn(doorstate, 3);
                        doorstate %= GameUtility.GetFlagValInt(3);
                        doorstate *= 2;
                        if (state_3)
                        {
                                doorstate |= GameUtility.GetFlagValInt(0);
                        }
                }
                return 2;
        }

        RaidNodeBehav GetRoomNode(int id)
        {
                return nodeList.Find((x) =>
                {
                        return x.id == id;
                });
        }

        public void CreateNode(int nodeId)
        {
                int oriNodeId = nodeId;
                if (this.idOffset > 0)
                {
                        oriNodeId %= this.idOffset;
                }
                int x = oriNodeId / 100;
                int y = oriNodeId % 100;
                GameObject nodeObj = new GameObject("Node" + nodeId);
                RaidNodeBehav node = nodeObj.AddComponent<RaidNodeBehav>();
                node.id = nodeId;
                node.nodeCfg = null;
                node.oriCfgNodeId = oriNodeId;
                node.groupId = 0;
                node.belongRoom = this;
                node.mapId = this.pieceCfgId;

                node.elemCfg = null;
                node.elemCount = 0;
                node.compOptions = null;
                node.compOptionEffects = null;

                RaidManager.GetInst().AddNodeToDict(node);
                this.nodeList.Add(node);

                if (m_nRotateDirect != 2)
                {
                        CalcRotate(ref x, ref y);
                }
                nodeObj.transform.SetParent(this.nodeRoot.transform);
                nodeObj.transform.localPosition = new Vector3(x, 0f, y);
                nodeObj.transform.rotation = Quaternion.Euler(Vector3.up * m_fAllRot);

                Debug.Log("CreateNode " + nodeId);
        }

        void InitNode(RaidNodeStruct nodeConfig, bool bNewRoom)
        {
                int x = nodeConfig.id / 100;
                int y = nodeConfig.id % 100;
                if (x >= pieceInfoCfg.sizex || y >= pieceInfoCfg.sizey)
                {
                        return;
                }
                if (m_nRotateDirect != 2)
                {
                        CalcRotate(ref x, ref y);
                }

                int subIndex = GetSubIndex(x, y);
                LINK_DIRECTION direct = GetNodeDirect(x, y);
                int linkIndex = GetLinkRoomIndex(subIndex, direct);

                if (bNewRoom == false)
                {
                        if ((x > 0 && x < RealSizeX - 1) || (y > 0 && y < RealSizeY - 1))
                        {
                                if (RaidManager.GetInst().CheckDoorVisible(this, linkIndex) == false)
                                {
                                        //Debug.Log("CheckDoorVisible " + this + " " + linkIndex + " false");
                                        return;
                                }
                        }
                }

                int posX = (int)this.transform.position.x + x;
                int posZ = (int)this.transform.position.z + y;
                AddToMesh(posX, posZ, new List<int>(nodeConfig.floorlist));

                int nodeId = this.idOffset + nodeConfig.id;
                Message.SCMsgRaidMapInitNodeInfo msg = RaidManager.GetInst().GetInitNode(nodeId);
                RaidElemConfig elemCfg = RaidConfigManager.GetInst().GetElemCfg(msg != null ? msg.ElemId : nodeConfig.elem_id);

                if (elemCfg != null)
                {
                        GameObject nodeObj = new GameObject("Node" + nodeId);
                        RaidNodeBehav node = nodeObj.AddComponent<RaidNodeBehav>();
                        node.id = nodeId;
                        node.nodeCfg = nodeConfig;
                        node.oriCfgNodeId = nodeConfig.id;
                        node.groupId = nodeConfig.group_id;
                        node.belongRoom = this;
                        node.mapId = this.pieceCfgId;

                        Message.SCMsgRaidOptionNodeData opData = RaidManager.GetInst().GetOptionNodeData(nodeId);

                        node.floorList = new List<int>(nodeConfig.floorlist);
                        node.elemCfg = RaidConfigManager.GetInst().GetElemCfg(msg != null ? msg.ElemId : nodeConfig.elem_id);
                        node.elemCount = msg != null ? msg.ElemCount : 0;
                        node.compOptions = opData != null ? GameUtility.ToList<int>(opData.optionList, '|', (s) => (int.Parse(s))) : null;
                        node.compOptionEffects = opData != null ? GameUtility.ToList<int>(opData.effectList, '|', (s) => (int.Parse(s))) : null;

                        if (node.elemCfg != null)
                        {
                                if (this.IsComplete && node.elemCfg.room_finish_disappear == 1)
                                {
                                        node.elemCfg = null;
                                        node.elemCount = 0;
                                }
                                if (elemCfg.type == (int)RAID_ELEMENT_TYPE.UNLOCK_BUILDING)
                                {
                                        if (node.CanUnlockBuild() == false)
                                        {
                                                node.elemCount = node.elemCfg.result_number;
                                        }
                                }
                        }

                        RaidManager.GetInst().AddNodeToDict(node);
                        this.nodeList.Add(node);

                        if (node.elemCfg != null)
                        {
                                if (node.elemCfg.IsDoor())
                                {
                                        node.gameObject.name = "DoorNode_" + node.id + "_" + direct.ToString() + "_" + subIndex + "_" + linkIndex;
                                        //this.AddDoor(subIndex, direct, node);
                                }

                                if (node.IsInteractive() || node.IsCharacterType() || node.IsExit())
                                {
                                        this.interactiveList.Add(node);
                                        nodeObj.transform.SetParent(this.interactiveRoot.transform);
                                }
                                else
                                {
                                        nodeObj.transform.SetParent(this.nodeRoot.transform);
                                }
                        }
                        nodeObj.transform.localPosition = new Vector3(x, 0f, y);
                        nodeObj.transform.rotation = Quaternion.Euler(Vector3.up * m_fAllRot);
                }
                else
                {
                        AddBoxCollider(posX, posZ);
                }
        }

        Dictionary<int, List<int>> floorDict = new Dictionary<int, List<int>>();
        public void AddToMesh(int posX, int posZ, List<int> floorlist)
        {
                int posID = posX * 1000 + posZ;
                if (floorlist != null)
                {
                        for (int height = 0; height < floorlist.Count; height++)
                        {
                                if (floorlist[height] > 0)
                                {
                                        //改三维坐标已存在记录
                                        if (BrickSceneManager.GetInst().SetMeshArray(posX, posZ, height) == false)
                                        {
                                                floorlist[height] = 0;  //置空
                                        }
                                }
                        }
                }
                if (!floorDict.ContainsKey(posID))
                {
                        floorDict.Add(posID, floorlist);
                }
        }

        public void InitNodelist(bool bNewRoom = false)
        {
                RaidNodeConfig nodeListCfg = RaidConfigManager.GetInst().GetRaidNodesConfig(this.pieceCfgId);
                if (nodeListCfg != null)
                {
                        foreach (RaidNodeStruct nodeConfig in nodeListCfg.list)
                        {
                                InitNode(nodeConfig, bNewRoom);
                        }
                        this.SortInteractiveList();
                        foreach (RaidNodeBehav node in this.interactiveList)
                        {
                                if (node.IsDoor())
                                {
                                        if (AddDoor(node, nodeListCfg) == false)
                                        {
                                                node.elemCfg = null;
                                                node.ResetElemObj();
                                        }
                                }
                        }
                }
        }

        public float FloorY
        {
                get
                {
                        return floorHeight - 1f;    //某种历史原因，高度0，砖块实际是在地下，所以默认减1
                }
        }
        void InitFloorMeshes()
        {
                foreach (int posID in floorDict.Keys)
                {
                        int posX = posID / 1000;
                        int posY = posID % 1000;

                        for (int i = 0; i < floorDict[posID].Count; i++)
                        {
                                if (floorDict[posID][i] <= 0)
                                        continue;

                                AddMesh(posX, posY, i, floorDict[posID][i]);
                        }
                }
        }

        public void SetupNodelist()
        {
                InitFloorMeshes();
                foreach (RaidNodeBehav node in this.nodeList)
                {
                        //node.InitFloorMesh();
                        node.InitElemObj();
                        node.UpdateBlockState();
                        node.SetNodeVisible(/*node.belongRoom.IsActive*/);
                        node.CheckInteractiveElemIcon();
                }
        }
        public void AddInside(GameObject go)
        {
                go.transform.SetParent(insideRoot.transform);
        }

        public void AddStaticElement(GameObject go, LINK_DIRECTION direct)
        {
                if (m_DirectionPartObjs.ContainsKey(direct))
                {
                        go.transform.SetParent(m_DirectionPartObjs[direct].transform);
                }
        }

        LINK_DIRECTION GetNodeDirect(int x, int y)
        {
                LINK_DIRECTION direct = LINK_DIRECTION.MAX;
                if (y == 0)
                {
                        direct = LINK_DIRECTION.SOUTH;
                }
                else if (y == RealSizeY - 1)
                {
                        direct = LINK_DIRECTION.NORTH;
                }
                else if (x == 0)
                {
                        direct = LINK_DIRECTION.WEST;
                }
                else if (x == RealSizeX - 1)
                {
                        direct = LINK_DIRECTION.EAST;
                }

                return direct;
        }

        public LINK_DIRECTION GetDoorDirection(int linkSubIndex)
        {
                foreach (Dictionary<LINK_DIRECTION, int> dict in m_LinkDict.Values)
                {
                        for (LINK_DIRECTION direct = LINK_DIRECTION.NORTH; direct < LINK_DIRECTION.MAX; direct++)
                        {
                                if (dict.ContainsKey(direct) && dict[direct] == linkSubIndex)
                                {
                                        return direct;
                                }
                        }
                }

                return LINK_DIRECTION.MAX;
        }

        public LINK_DIRECTION GetDoorDirection(RaidNodeBehav doorNode)
        {
                int linkIdx = GetLinkIndexByDoor(doorNode);
                foreach (Dictionary<LINK_DIRECTION, int> dict in m_LinkDict.Values)
                {
                        for (LINK_DIRECTION direct = LINK_DIRECTION.NORTH; direct < LINK_DIRECTION.MAX; direct++)
                        {
                                if (dict.ContainsKey(direct) && dict[direct] == linkIdx)
                                {
                                        return direct;
                                }
                        }
                }
                return LINK_DIRECTION.MAX;
        }

        public RaidDoor GetDoor(int linkSubIndex)
        {
                if (m_DoorDict.ContainsKey(linkSubIndex))
                {
                        return m_DoorDict[linkSubIndex];
                }
                return null;
        }
        public RaidDoor GetDoor(LINK_DIRECTION direct)
        {
                int linkIndex = GetLinkRoomIndex(direct);
                if (m_DoorDict.ContainsKey(linkIndex))
                {
                        return m_DoorDict[linkIndex];
                }
                return null;
        }

        public bool IsLinked(int subIndex, LINK_DIRECTION direct)
        {
                if (linkStateDict.ContainsKey(subIndex))
                {
                        int bit = 1 << (int)direct;
                        return (linkStateDict[subIndex] & bit) != 0;
                }
                return false;
        }

        int GetSubIndexX(int posX)
        {
                if (posX == 0)
                {
                        return this.index_X;
                }
                else
                {
                        return this.index_X + (posX - 1) / (RaidManager.GetInst().PieceSize - 1);
                }
        }
        int GetSubIndexY(int posY)
        {
                if (posY == 0)
                {
                        return this.index_Y;
                }
                else
                {
                        return this.index_Y + (posY - 1) / (RaidManager.GetInst().PieceSize - 1);
                }
        }
        int GetSubIndex(int posX, int posY)
        {
                return GetSubIndexY(posY) * RaidManager.GetInst().MapSize + GetSubIndexX(posX);
        }
        public void ArrangeObj(GameObject tmpObj, RaidNodeBehav node)
        {
                int x = (int)(node.posX - posX);
                int y = (int)(node.posY - posY);

                //                 if (y == 0)
                //                 {
                //                         AddStaticElement(tmpObj, LINK_DIRECTION.SOUTH);
                //                 }
                //                 else if (y == RealSizeY - 1)
                //                 {
                //                         AddStaticElement(tmpObj, LINK_DIRECTION.NORTH);
                //                 }
                //                 else if (x == 0 && y >= 1 && y < RealSizeY - 1)
                //                 {
                //                         AddStaticElement(tmpObj, LINK_DIRECTION.WEST);
                //                 }
                //                 else if (x == RealSizeX - 1 && y >= 1 && y < RealSizeY - 1)
                //                 {
                //                         AddStaticElement(tmpObj, LINK_DIRECTION.EAST);
                //                 }
                //                 else
                {
                        AddInside(tmpObj);
                }
        }

        public string GetRoomElemIcon() //根据元素优先级显示房间图标
        {
                for (int i = 0; i < interactiveList.Count; i++)
                {
                        if (!interactiveList[i].IsElemDone())
                        {
                                if (interactiveList[i].elemCfg != null)
                                {
                                        RaidElementTypeConfig retc = RaidConfigManager.GetInst().RaidElementTypeCfg(interactiveList[i].elemCfg.type);
                                        if (retc != null)
                                        {
                                                if (retc.priority != 0)
                                                {
                                                        return retc.icon;
                                                }
                                        }
                                }
                        }
                }
                return "";
        }

        protected int CompareInteractive(RaidNodeBehav behava, RaidNodeBehav behavb)
        {
                int prioritya = 0;
                int priorityb = 0;
                if (behava.elemCfg != null)
                {
                        RaidElementTypeConfig typeCfg = RaidConfigManager.GetInst().RaidElementTypeCfg(behava.elemCfg.type);
                        if (typeCfg != null)
                        {
                                prioritya = typeCfg.priority;
                        }
                }
                if (behavb.elemCfg != null)
                {
                        RaidElementTypeConfig typeCfg = RaidConfigManager.GetInst().RaidElementTypeCfg(behavb.elemCfg.type);
                        if (typeCfg != null)
                        {
                                priorityb = typeCfg.priority;
                        }
                }

                if (prioritya != priorityb)
                {
                        return priorityb - prioritya;
                }

                return (int)(behavb.id - behava.id);

        }

        public void SortInteractiveList()
        {
                interactiveList.Sort(CompareInteractive);
        }

        public void AddInteractiveList(RaidNodeBehav node)
        {
                bool isadd = false;
                if (node.IsInteractive())
                {
                        if (node.elemObj != null)
                        {
                                interactiveList.Add(node);
                                isadd = true;
                        }
                }
                if (node.elemCfg != null && node.elemCfg.IsCharacter() || node.IsExit())
                {
                        if (!interactiveList.Contains(node))
                        {
                                interactiveList.Add(node);
                                isadd = true;
                        }
                }
                if (isadd)
                {
                        SortInteractiveList();
                }
        }

        public void SetFocusRoomColor(bool bFocus)
        {
                foreach (Transform t in this.insideRoot.GetComponentsInChildren<Transform>(true))
                {
                        GameUtility.SetLayer(t.gameObject, bFocus ? "FocusRoom" : "UnFocusRoom");
                }
        }

        public void SetRoomDissolve(LINK_DIRECTION direct, bool bEnable, bool bSetLink = true)
        {
                //Debuger.Log("SetRoomDissolve " + direct + " " + bEnable);
                if (m_DirectionPartObjs.ContainsKey(direct))
                {
                        foreach (Renderer renderer in m_DirectionPartObjs[direct].GetComponentsInChildren<Renderer>())
                        {
                                if (renderer.name.Contains("Combined"))
                                {
                                        if (renderer.sharedMaterial.shader.name == "DF/FadeoutDissolve")
                                        {
                                                renderer.sharedMaterial.SetFloat("_IsDissolve", bEnable ? 0 : 1);
                                        }
                                }
                        }
                }
                if (bSetLink)
                {
                        foreach (Dictionary<LINK_DIRECTION, int> subLink in m_LinkDict.Values)
                        {
                                if (subLink.ContainsKey(LINK_DIRECTION.SOUTH))
                                {
                                        RaidRoomBehav linkRoom = RaidManager.GetInst().GetRoomData(subLink[LINK_DIRECTION.SOUTH]);
                                        if (linkRoom != null)
                                        {
                                                linkRoom.SetRoomDissolve(LINK_DIRECTION.NORTH, bEnable, false);
                                        }
                                }
                        }
                }
        }

        public void ClearEmptyObjs()
        {
                if (insideRoot != null)
                {
                        List<Transform> toremove = new List<Transform>();
                        foreach (Transform trans in insideRoot.GetComponentsInChildren<Transform>(true))
                        {
                                if (trans.name.Contains("Combined"))
                                        continue;
                                if (trans.name.Contains("Node"))
                                        continue;

                                if (trans.parent == insideRoot.transform)
                                {
                                        if (trans.childCount < 1)
                                        {
                                                toremove.Add(trans);
                                        }

                                }
                        }
                        foreach (Transform trans in toremove)
                        {
                                GameObject.Destroy(trans.gameObject);
                        }
                }
        }
        public void ShowPartVisible(LINK_DIRECTION direct, bool bVisible)
        {
                if (m_DirectionPartObjs.ContainsKey(direct))
                {
                        m_DirectionPartObjs[direct].SetActive(bVisible);
                }
        }
        public void SetDirectionVisible(LINK_DIRECTION direct, bool bVisible)
        {
                if (!m_DirectionPartVisible.ContainsKey(direct))
                {
                        return;
                }
                if (m_DirectionPartVisible[direct] == false)
                {
                        return;
                }
                ShowPartVisible(direct, bVisible);
        }

        public bool IsDirectionPartVisible(LINK_DIRECTION direct)
        {
                if (m_DirectionPartVisible.ContainsKey(direct))
                {
                        return m_DirectionPartVisible[direct];
                }
                return true;
        }
        public void SetPartVisible(LINK_DIRECTION direct, bool bVisible)
        {
                if (!m_DirectionPartVisible.ContainsKey(direct))
                {
                        m_DirectionPartVisible.Add(direct, bVisible);
                }
        }

        public void SetRoomVisible(bool bVisible, LINK_DIRECTION fromDirect = LINK_DIRECTION.MAX)
        {
                foreach (LINK_DIRECTION direct in m_DirectionPartObjs.Keys)
                {
                        ShowPartVisible(direct, true);
                }

                if (bVisible)
                {
                        foreach (RaidNodeBehav node in this.nodeList)
                        {
                                if (this.IsComplete && node.elemCfg != null && node.elemCfg.room_finish_disappear == 1)
                                {
                                        node.SetNodeVisible(false);
                                        continue;
                                }
                                node.SetNodeVisible(true);
                                node.StartPlay();
                        }
                }
                else
                {
                        foreach (RaidNodeBehav node in this.nodeList)
                        {
                                if (node.IsDoor())
                                {
                                        continue;
                                }
                                node.SetNodeVisible(false);
                        }
                }
        }
        public void PlayInteractiveElems()
        {
                foreach (RaidNodeBehav node in this.nodeList)
                {

                        node.Replay();
                }
        }

        public RaidNodeBehav GetNodeByElemId(int elemId)
        {
                foreach (RaidNodeBehav node in this.nodeList)
                {
                        if (node.ElemId == elemId)
                        {
                                return node;
                        }
                }
                return null;
        }
        #region DOOR
        public Dictionary<int, RaidDoor> m_DoorDict = new Dictionary<int, RaidDoor>();  //key：连接分片的subIndex.
        List<int> GetInsteadFloorlist(RaidNodeBehav node, RaidNodeConfig roomCfg)
        {
                LINK_DIRECTION oriDirect = GetNodeDirect(node.oriCfgNodeId / 100, node.oriCfgNodeId % 100);

                int tmpId = -1;
                switch (oriDirect)
                {
                        case LINK_DIRECTION.NORTH:
                        case LINK_DIRECTION.SOUTH:
                                {
                                        tmpId = node.oriCfgNodeId - 100;

                                }
                                break;
                        case LINK_DIRECTION.EAST:
                        case LINK_DIRECTION.WEST:
                                {
                                        tmpId = node.oriCfgNodeId - 1;
                                }
                                break;
                }

                List<int> floorlist = null;
                if (tmpId > -1)
                {
                        RaidNodeStruct tmpCfg = roomCfg.list.Find((x) =>
                        {
                                return x.id == tmpId;
                        });
                        if (tmpCfg != null)
                        {
                                floorlist = tmpCfg.floorlist;
                        }
                }
                return floorlist;
        }

        public bool AddDoor(RaidNodeBehav node, RaidNodeConfig roomCfg)
        {
                List<int> floorlist = GetInsteadFloorlist(node, roomCfg);

                int px = node.posX - this.posX;
                int py = node.posY - this.posY;
                int subIndex = GetSubIndex(px, py);
                LINK_DIRECTION realDirect = GetNodeDirect(px, py);
                int linkSubIndex = GetLinkRoomIndex(subIndex, realDirect);
                if (linkSubIndex > -1)
                {
                        if (!m_DoorDict.ContainsKey(linkSubIndex))
                        {
                                RaidDoor door = new RaidDoor(subIndex, linkSubIndex);
                                door.mainNode = node;
                                door.doorElemCfg = door.mainNode.elemCfg;
                                /*                                Debug.LogWarning(door.doorElemCfg.id);*/
                                if (floorlist != null)
                                {
                                        door.InitDoorMesh(floorlist, m_nRotateDirect);
                                }
                                m_DoorDict.Add(linkSubIndex, door);
                                RaidManager.GetInst().AddDoor(door);
                        }
                        return true;
                }
                else
                {
                        if (floorlist != null && node.elemCfg != null)
                        {
                                Vector3 realSize = node.elemCfg.size;
                                bool bMinus = m_nRotateDirect < 2;
                                if (m_nRotateDirect == 1 || m_nRotateDirect == 3)
                                {
                                        realSize = new Vector3(realSize.z, realSize.y, realSize.x);

                                }
                                for (int x = 0; x < realSize.x; x++)
                                {
                                        for (int z = 0; z < realSize.z; z++)
                                        {
                                                for (int y = 0; y < realSize.y; y++)
                                                {
                                                        int rx = node.posX + x * (bMinus ? -1 : 1);
                                                        int rz = node.posY + z * (bMinus ? -1 : 1);
                                                        int ry = (0 - (int)node.FloorY) + y;
                                                        if (ry < floorlist.Count)
                                                        {
                                                                int modelId = floorlist[ry];
                                                                if (modelId > 0)
                                                                {
                                                                        BrickSceneManager.GetInst().SetMeshArray(rx, rz, ry);
                                                                        int posID = rx * 1000 + rz;
                                                                        if (floorDict.ContainsKey(posID))
                                                                        {
                                                                                if (ry < floorDict[posID].Count)
                                                                                {
                                                                                        floorDict[posID][ry] = floorlist[ry];
                                                                                }
                                                                                else
                                                                                {
                                                                                        floorDict[posID].Add(floorlist[ry]);
                                                                                }
                                                                        }
                                                                }
                                                        }
                                                }
                                        }
                                }
                        }
                        return false;
                }
        }
        public void SetDoor(int linkIdx, RaidDoor door)
        {
                if (linkIdx > -1 && !m_DoorDict.ContainsKey(linkIdx))
                {
                        m_DoorDict.Add(linkIdx, door);
                }
        }

        public RaidDoor GetDoorByOwnIdx(int ownIndex)
        {
                foreach (int linkSubIndex in m_DoorDict.Keys)
                {
                        int linkOwnIdx = RaidManager.GetInst().GetOwnIdx(linkSubIndex);
                        if (linkOwnIdx == ownIndex)
                        {
                                return m_DoorDict[linkSubIndex];
                        }
                }
                return null;
        }


        public void UpdateAllDoorState(int fromIndex = -1)
        {
                foreach (var param in m_DoorDict)
                {
                        RaidDoor door = param.Value;

                        int linkIdx = RaidManager.GetInst().GetOwnIdx(param.Key);
                        if (linkIdx == fromIndex)
                        {
                                door.EnableDoor(this, true);
                        }
                        else
                        {
                                RaidRoomBehav linkRoom = RaidManager.GetInst().GetRoomData(linkIdx);
                                if (linkRoom != null && linkRoom.pieceType == (int)ROOM_TYPE.Hide)
                                {
                                        door.EnableDoor(this, this.IsComplete && linkRoom.IsDetected);
                                }
                                else
                                {
                                        if (this.pieceType == (int)ROOM_TYPE.Hide)
                                        {
                                                door.EnableDoor(this, this.IsComplete && linkRoom.IsDetected);
                                        }
                                        else
                                        {
                                                door.EnableDoor(this, this.IsComplete);
                                        }
                                }
                        }                        
                }
                GameUtility.ReScanPath();
        }

        #endregion

        public Vector3 GetCenterPosition()
        {
                return this.transform.position + new Vector3(this.RealSizeX / 2f - 0.5f, 0f, this.RealSizeY / 2f - 0.5f);
        }

        public void DisappearCompletedElems()
        {
                foreach (RaidNodeBehav node in nodeList)
                {
                        if (node.elemCfg != null && node.elemCfg.room_finish_disappear == 1)
                        {
                                node.elemCfg = null;
                                node.elemCount = 0;
                                node.ResetElemObj();
                        }
                }
        }

        public void Broken()
        {
                foreach (RaidNodeBehav node in nodeList)
                {
                        int x = (node.id - idOffset) / 100;
                        int y = (node.id - idOffset) % 100;
                        if (x < 1 || x > 10 || y < 1 || y > 10)
                        {
                                continue;
                        }
                        node.ShowDisappear(UnityEngine.Random.Range(0.01f, 0.5f));
                }
        }
    
        #region COMBINE
        Dictionary<int, int> m_MeshDict = new Dictionary<int, int>();
        public void AddMesh(int x, int y, int height, int modelId)
        {
                LINK_DIRECTION direct = GetNodeCombineRoot(x, y, height);

                bool bAddCollider = true;
                if (height < 0 - FloorY)
                {
                        bAddCollider = false;
                }
                else
                {
//                         if (direct == LINK_DIRECTION.EAST || direct == LINK_DIRECTION.WEST)
//                         {
//                                 bAddCollider = false;
//                         }
                }
                BrickSceneManager.GetInst().SetNodeBrick(x, height, y, modelId, bAddCollider);
        }

        public void RemoveMesh(int x, int y, int height)
        {
                LINK_DIRECTION direct = GetNodeCombineRoot(x, y, height);
                CombineChildrenEx ccex = GetCombineRoot(GetBelongRoot(direct));
                if (ccex != null)
                {
                        ccex.RemoveMesh(x * 1000 + y * 10 + height);
                }
        }

        GameObject GetBelongRoot(LINK_DIRECTION direct)
        {
//                 if (true || IsLinked(direct))
//                 {
                        if (m_DirectionPartObjs.ContainsKey(direct))
                        {
                                return m_DirectionPartObjs[direct];
                        }
//                }
                return insideRoot;
        }
        LINK_DIRECTION GetNodeCombineRoot(int nodePosX, int nodePosY, int height)
        {
                int x = nodePosX - this.posX;
                int y = nodePosY - this.posY;
                GameObject rootObj = insideRoot;
                if (height > 0)
                {
                        if (y == 0)
                        {
                                return LINK_DIRECTION.SOUTH;
                        }
                        else if (y == RealSizeY - 1)
                        {
                                return LINK_DIRECTION.NORTH;
                        }
                        else if (x == 0 && y >= 1 && y < RealSizeY - 1)
                        {
                                return LINK_DIRECTION.WEST;
                        }
                        else if (x == RealSizeX - 1 && y >= 1 && y < RealSizeY - 1)
                        {
                                return LINK_DIRECTION.EAST;
                        }
                }
                return LINK_DIRECTION.MAX;
        }
        CombineChildrenEx GetCombineRoot(GameObject obj)
        {
                CombineChildrenEx ccex = obj.GetComponent<CombineChildrenEx>();
                if (ccex == null)
                {
                        ccex = obj.AddComponent<CombineChildrenEx>();
                }
                return ccex;
        }
        public void ManualCombine()
        {
                //                return;
                if (this.pieceType == (int)ROOM_TYPE.DropTrap)
                {
                        return;
                }
                GetCombineRoot(insideRoot).ManualCombine();

                foreach (GameObject obj in m_DirectionPartObjs.Values)
                {
                        GetCombineRoot(obj).ManualCombine();
                }
        }

        public void SetCombineMeshCollider()
        {
                return;
        }

        #endregion
}
