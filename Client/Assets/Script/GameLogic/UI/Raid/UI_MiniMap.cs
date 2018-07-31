using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using DG.Tweening;

public class UI_MiniMap : UIBehaviour
{
        public RectTransform map;
        public GameObject room_bg; //房间
        public GameObject door0;
        public GameObject door1;
        public GameObject icon;
        public GameObject road;      //路
        public GameObject role;
        public Text floor_text;
        public GameObject small;
        public GameObject big;

        int m_nRoomSize;
        readonly float ROOM_SIZE_OFFSET = 1.5f;  //小地图边缘填充一个房间大小的空白，即边长加2
        int m_nMapSize;
        int m_nPieceSize;
        Vector2 m_vStartPoint;
        Dictionary<int, GameObject> m_RoomObjDict = new Dictionary<int, GameObject>();
        Dictionary<int, GameObject> m_RoomIconDict = new Dictionary<int, GameObject>();
        Dictionary<string, GameObject> m_DoorDict = new Dictionary<string, GameObject>();

        string floor_info = "";
        void Awake()
        {
                room_bg.SetActive(false);
                door0.SetActive(false);
                door1.SetActive(false);
                icon.SetActive(false);
                road.SetActive(false);
                floor_info = floor_text.text;
                m_nRoomSize = (int)(room_bg.transform as RectTransform).sizeDelta.x;

                FindComponent<Button>("mini/test").onClick.AddListener(FinishRaid);
        }

        void FinishRaid()
        {
                GameUtility.SendGM("/finishraid");
        }

        public void ShowMiniMap(int mapSize, int pieceSize)
        {
                CreatMiniMap(mapSize, pieceSize);
                InitActiveRoom();
                StartCoroutine(AlphaShowRole());                //ChangeMapState(); //默认显示大地图，有了大地图还要小地图干吗？！！
        }
        IEnumerator AlphaShowRole()
        {
                Image im = role.GetComponent<Image>();
                while (true)
                {
                        im.DOFade(0f, 0.5f);
                        yield return new WaitForSeconds(0.5f);
                        im.DOFade(1f, 0.5f);
                        yield return new WaitForSeconds(0.5f);
                }
        }

        public void SetFloor(int floor)
        {
                if (floor <= 0)
                {
                        floor = 1;
                }
                GetText("floor").text = string.Format(floor_info, floor);
        }

        public void CreatMiniMap(int mapSize, int PartSize)
        {
                m_nMapSize = mapSize;
                m_nPieceSize = PartSize;
                int map_size = (int)(mapSize + 2 * ROOM_SIZE_OFFSET) * m_nRoomSize;
                map.sizeDelta = Vector2.one * map_size;
                map.anchoredPosition = Vector2.zero;

                m_vStartPoint = Vector2.one * (-map_size * 0.5f + (0.5f + ROOM_SIZE_OFFSET) * m_nRoomSize);
                Dictionary<int, RaidRoomBehav> roomDict = RaidManager.GetInst().GetAllRoomData();

                foreach (int roomIndex in roomDict.Keys)
                {
                        RaidRoomBehav roomData = roomDict[roomIndex];
                        if (roomData != null)
                        {
                                CreatRoom(roomData);
                                CreatDoor(roomData);
                        }
                }
        }

        string GetRoomUrl(RaidRoomBehav roomData, ref Vector2 size)
        {
                string url = "Room#";
                url += roomData.IsActive ? "bright" : "dark";

                if (roomData.RealSizeY > m_nPieceSize)
                {
                        size.y = 2;
                        url += "_v";
                }
                if (roomData.RealSizeX > m_nPieceSize)
                {
                        size.x = 2;
                        url += "_h";
                }
                return url;
        }

        void CreatRoom(RaidRoomBehav roomData)
        {
                //for (int i = 0; i < roomData.subIndexList.Count; i++)
                {
                        int index = roomData.index;
                        if (!m_RoomObjDict.ContainsKey(index))
                        {
                                Vector2 size = new Vector2(1, 1);
                                GameObject roomObj = CloneElement(room_bg, "room" + index);
                                int subIndexX = index % m_nMapSize;
                                int subIndexY = index / m_nMapSize;
                                string url = GetRoomUrl(roomData, ref size);
                                ResourceManager.GetInst().LoadIconSpriteSyn(url, roomObj.transform);
                                roomObj.SetActive(roomData.IsActive);
                                RectTransform rt = roomObj.GetComponent<RectTransform>();
                                rt.anchoredPosition = m_vStartPoint + new Vector2(subIndexX * m_nRoomSize, subIndexY * m_nRoomSize)
                                        + new Vector2(m_nRoomSize * (size.x - 1) / 2f, m_nRoomSize * (size.y - 1) / 2f);
                                rt.sizeDelta = new Vector2(m_nRoomSize * size.x, m_nRoomSize * size.y);

                                if (roomData.IsHide)
                                {
                                        roomObj.SetActive(false);
                                }
                                m_RoomObjDict.Add(index, roomObj);

                                if (!m_RoomIconDict.ContainsKey(index))
                                {
                                        GameObject iconObj = CloneElement(icon, "icon" + index);
                                        RectTransform iconRt = iconObj.GetComponent<RectTransform>();
                                        iconRt.anchoredPosition = (roomObj.transform as RectTransform).anchoredPosition;
                                        m_RoomIconDict.Add(index, iconObj);

                                        if (roomData.IsDetected)
                                        {
                                                RefreshIcon(roomData);
                                        }
                                        else
                                        {
                                                iconObj.SetActive(false);
                                        }
                                }
                        }
                }
        }

        void CreatDoor(RaidRoomBehav roomData)
        {
                GameObject doorObj = null;
                Dictionary<int, Dictionary<LINK_DIRECTION, int>> linkDict = roomData.GetLinkDict();
                if (linkDict == null)
                        return;
                foreach (int subIndex in linkDict.Keys)
                {
                        foreach (LINK_DIRECTION direct in linkDict[subIndex].Keys)
                        {
                                int subIndexX = subIndex % m_nMapSize;
                                int subIndexY = subIndex / m_nMapSize;
                                int linkIndex = linkDict[subIndex][direct];
                                string door_name = GetDoorName(subIndex, linkIndex);
//                                 RaidRoomBehav linkRoom = RaidManager.GetInst().GetRoomData(linkIndex);
//                                 if (linkRoom != null && linkRoom.IsHide)
//                                 {
//                                         continue;
//                                 }

                                Vector2 pos = m_vStartPoint + new Vector2(subIndexX * m_nRoomSize, subIndexY * m_nRoomSize);
                                switch (direct)
                                {
                                        case LINK_DIRECTION.NORTH:
                                                {
                                                        doorObj = CloneElement(door1, "door" + door_name);
                                                        (doorObj.transform as RectTransform).anchoredPosition = pos + new Vector2(0, m_nRoomSize * 0.5f);
                                                }
                                                break;
                                        case LINK_DIRECTION.EAST:
                                                {
                                                        doorObj = CloneElement(door0, "door" + door_name);
                                                        (doorObj.transform as RectTransform).anchoredPosition = pos + new Vector2(m_nRoomSize * 0.5f, 0);
                                                }
                                                break;
                                        case LINK_DIRECTION.WEST:
                                        case LINK_DIRECTION.SOUTH:
                                                {
                                                        string new_name = linkDict[subIndex][direct] + "," + subIndex;
                                                        if (m_DoorDict.ContainsKey(new_name))
                                                        {
                                                                doorObj = m_DoorDict[new_name];
                                                        }
                                                }
                                                break;
                                }
                                if (doorObj != null)
                                {
                                        doorObj.SetActive(false);
                                        if (!m_DoorDict.ContainsKey(door_name))
                                        {
                                                m_DoorDict.Add(door_name, doorObj);
/*                                                Debug.Log("CreateDoor " + door_name + " false");*/
                                        }
                                }
                        }
                }
        }

        public void RefreshIcon(RaidRoomBehav roomData)
        {
                string url;
                if (!roomData.IsActive || !roomData.IsComplete)
                {
                        url = RaidConfigManager.GetInst().GetRoomTypeIcon(roomData.pieceType);
                }
                else
                {
                        url = roomData.GetRoomElemIcon();
//                         if (string.IsNullOrEmpty(url))
//                         {
//                                 url = RaidConfigManager.GetInst().GetRoomTypeIcon(roomData.pieceType);
//                         }
                }
                //Debug.Log("RefreshIcon " + roomData.index + " " + url);
                if (!string.IsNullOrEmpty(url))
                {                        
                        m_RoomIconDict[roomData.index].SetActive(true);
                        ResourceManager.GetInst().LoadIconSpriteSyn(url, m_RoomIconDict[roomData.index].transform);
                }
                else
                {
                        m_RoomIconDict[roomData.index].SetActive(false);
                }
        }

        public void InitActiveRoom()
        {
                foreach (RaidRoomBehav roomData in RaidManager.GetInst().GetRoomDict().Values)
                {
                        if (roomData.IsActive)
                        {
                                ActiveRoom(roomData);
                        }
                }
        }

        public void ActiveRoom(RaidRoomBehav roomData)
        {
                foreach (int subIndex in roomData.subIndexList)
                {
                        if (m_RoomObjDict.ContainsKey(subIndex))
                        {
                                m_RoomObjDict[subIndex].SetActive(true);
                                Vector2 size = Vector2.one;
                                ResourceManager.GetInst().LoadIconSpriteSyn(GetRoomUrl(roomData, ref size), m_RoomObjDict[subIndex].transform);

                                RefreshIcon(roomData);
                                //ActiveDoor(subIndex);
                        }
                }
                foreach (RaidDoor door in roomData.m_DoorDict.Values)
                {
                        if (roomData.subIndexList.Contains(door.idx0))
                        {
                                ActiveDoor(door.idx0, door.idx1);
                        }
                        else if (roomData.subIndexList.Contains(door.idx1))
                        {
                                ActiveDoor(door.idx1, door.idx0);
                        }
                }
        }
        string GetDoorName(int idx0, int idx1)
        {
                if (idx0 < idx1)
                {
                        return idx0 + "," + idx1;
                }
                else
                {
                        return idx1 + "," + idx0;
                }
        }
        void ActiveDoor(int roomSubIdx, int linkSubIdx)
        {
                string door_name = GetDoorName(roomSubIdx, linkSubIdx);
                if (m_DoorDict.ContainsKey(door_name))
                {
                        int ownIdx = RaidManager.GetInst().GetOwnIdx(linkSubIdx);
                        RaidRoomBehav linkRoom = RaidManager.GetInst().GetRoomData(ownIdx);
                        if (linkRoom != null)
                        {
                                //隐藏房间未被侦查过的不显示门和地图
                                if (linkRoom.IsHide)
                                {
                                        return;
                                }
                                if (m_RoomObjDict.ContainsKey(ownIdx))
                                {
                                        m_RoomObjDict[ownIdx].SetActive(true);
                                        m_DoorDict[door_name].SetActive(true);
/*                                        Debug.Log("ActiveDoor " + door_name);*/
                                }
                        }
                }
        }

        Vector2 GetRoomInsidePosition(int x, int y) //()1,1)为中心点
        {
                Vector2 size = (road.transform as RectTransform).sizeDelta;
                return new Vector2((x - 1) * size.x, (y - 1) * size.y);
        }

        bool need_move = false;
        Vector2 old_pos = -Vector2.one;
        public void SetHeroPosition(RaidRoomBehav currRoom)
        {
                if (currRoom == null)
                {
                        return;
                }
                if (!m_RoomObjDict.ContainsKey(currRoom.index))
                {
                        return;
                }

                Vector2 pos = Vector2.zero;
                GameObject room = null;

                room = m_RoomObjDict[currRoom.index];
                pos = (room.transform as RectTransform).anchoredPosition + GetRoomInsidePosition(1, 1/*m_y, m_x*/);

                Vector2 size = room.GetComponent<RectTransform>().sizeDelta + new Vector2(6f, 6f);
                role.GetComponent<RectTransform>().DOSizeDelta(size, 0.5f);
                if (is_small)
                {
                        if (old_pos != pos)
                        {
                                if (need_move)
                                {
                                        map.DOAnchorPos(-pos, 0.5f);
                                }
                                else
                                {
                                        map.anchoredPosition = -pos;
                                        need_move = true;
                                }
                        }
                }
                else
                {
                        if (old_pos != pos)
                        {
                                (role.transform as RectTransform).DOAnchorPos(pos, 0.5f);
                        }
                }
                old_pos = pos;
        }


        bool is_small = true;
        public void ChangeMapState()
        {
                DOTween.KillAll();
                is_small = !is_small;
                if (is_small)
                {
                        role.transform.SetParent(map.parent.transform);
                        role.transform.localScale = Vector3.one;
                        (role.transform as RectTransform).anchoredPosition = Vector2.zero;
                        map.anchoredPosition = -old_pos;
                        map.localScale = Vector2.one;
                        small.SetActive(true);
                        big.SetActive(false);
                }
                else
                {
                        role.transform.SetParent(map);
                        role.transform.localScale = Vector3.one;
                        (role.transform as RectTransform).anchoredPosition = old_pos;
                        map.anchoredPosition = Vector2.zero;
                        map.localScale = Vector2.one * 0.4f;
                        small.SetActive(false);
                        big.SetActive(true);
                }
        }

        public void PlayDetect()
        {
                UIUtility.SetUIEffect(this.name, GetGameObject("uieffect"), true, "effect_raid_adventure_skill_spy");
        }
}

