//using UnityEngine;
//using System.Collections;
//namespace Message
//{
//        class CSMsgAreaMapEnter : CSMsgBaseReq
//        {
//                public int idMap;
//                public CSMsgAreaMapEnter()
//                {
//                        SetTag("cMapEnter");
//                }
//        }
//        class SCMsgAreaMapEnter : SCMsgBaseAck
//        {
//                public long id; //实例id
//                public int idMap;//地图配置id
//                public int nDistrictId; //建筑物id
//                public string guardlist;
//                public SCMsgAreaMapEnter()
//                {
//                        SetTag("sMapEnter");
//                }
//        }

//        class CSMsgAreaMapExplore : CSMsgBaseReq
//        {
//                public long id; //实例id
//                public int nDistrictId;
//                public CSMsgAreaMapExplore()
//                {
//                        SetTag("cMapExplore");
//                }
//        }
//        class SCMsgAreaMapExplore : SCMsgBaseAck
//        {
//                public long id;//实例id
//                public int idMap;       //地图配置id
//                public int nDistrictId;
//                public int ret; 
//                public SCMsgAreaMapExplore()
//                {
//                        SetTag("sMapExplore");
//                }
//        }

//        class SCMsgAreaGuardOpen : SCMsgBaseAck
//        {
//                public int idGuard;
//                public SCMsgAreaGuardOpen()
//                {
//                        SetTag("sMapGuardOpen");
//                }
//        }

//}