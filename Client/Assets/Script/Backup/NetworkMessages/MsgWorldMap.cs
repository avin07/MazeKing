using UnityEngine;
using System.Collections;
namespace Message
{
        class CSMsgMapAreaUnlock : CSMsgBaseReq  //请求解锁区域（车马点）
        {
                public int areaID;
                public CSMsgMapAreaUnlock()
                {
                        SetTag("cMapAreaUnlock");
                }
        }

        class CSMsgMapRaidEnter : CSMsgBaseReq //进入副本
        {
                public long id;  //迷宫实例id
                public int floor;
                public string strPetList;
                public string items;
                public CSMsgMapRaidEnter()
                {
                        SetTag("cMapRaidEnter");
                }
        }

        class CSMsgRaidRemainTimeQuery : CSMsgBaseReq //请求剩余时间
        {
                public CSMsgRaidRemainTimeQuery()
                {
                        SetTag("cRaidRemainTimeQuery");
                }
        }

        class SCMsgMapInfo : SCMsgBaseAck
        {
                //{strUnlockAreaList} // strUnlockAreaList: idArea1&progress|idArea2&progress|...  初始为0 ；协议发送时机：登录、解锁区域//
                public string strUnlockAreaList;
                public SCMsgMapInfo()
                {
                        SetTag("sMapInfo");
                }
        }

        public class SCMsgMapAllNodes : SCMsgBaseAck
        {
                // strNodeList: id&idRaidConfig&nType&areaSpot&byShow&ntimes&nClearTimes|
                public string strNodeList;
                public SCMsgMapAllNodes()
                {
                        SetTag("sMapAllNodes");
                }
        }

        public class SCMsgMapMapNode : SCMsgBaseAck
        {
                // strNodeList: id&idRaidConfig&nType&areaSpot&byShow&ntimes&nClearTimes
                public string strNode;
                public SCMsgMapMapNode()
                {
                        SetTag("sMapNode");
                }
        }

        class SCMsgRaidRemainTime : SCMsgBaseAck
        {
                public int time; //剩余时间
                public SCMsgRaidRemainTime()
                {
                        SetTag("sRaidRemainTimeQuery");
                }
        }

        class CSMsgMapEvent : CSMsgBaseReq
        {
                public long id;
                public int eventId;
                public CSMsgMapEvent()
                {
                        SetTag("cMapEvent");
                }
        }

        class SCMsgMapNodeDel : SCMsgBaseAck
        {
                public long id;
                public SCMsgMapNodeDel()
                {
                        SetTag("sMapNodeDel");
                }
        }


        class SCMsgMapAreaPoint : SCMsgBaseAck  //区域进度
        {
                public int area;
                public int point;
                public SCMsgMapAreaPoint()
                {
                        SetTag("sMapAreaPoint");
                }
        }
}