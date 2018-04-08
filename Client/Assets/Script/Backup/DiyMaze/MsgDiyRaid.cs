//using UnityEngine;
//using System.Collections;
//namespace Message
//{
//        public class SCMsgFragment : SCMsgBaseAck  //自制迷宫分片
//        {
//                //{id} {idConfig} {nLevel} {nCount};
//                public long id;
//                public int idConfig;
//                public int nLevel;
//                public int nCount;
//                public SCMsgFragment()
//                {
//                        SetTag("sFragment");
//                }
//        }


//        class CSMsgFragmentUpgrade : CSMsgBaseReq
//        {
//                public long id;
//                public CSMsgFragmentUpgrade()
//                {
//                        SetTag("cFragmentUpgrade");
//                }
//        }


//        class SCMsgDiyMapSaveSuc : SCMsgBaseAck  //自制迷宫存储成功
//        {
//                public int index;
//                public SCMsgDiyMapSaveSuc()
//                {
//                        SetTag("sDiyMapSaveSuc");
//                }
//        }


//        class CSMsgDiyMapSave : CSMsgBaseReq
//        {
//                public int saveIndex;  
//                public int idTerrain;  //图纸id
//                public int idEnterIdx; //入口
//                public int idExitIdx;  //出口
//                public string strFragmentInfo; //idFragmentCfg|0|idFragmentCfg……（没有填0）
//                public string strEnterToExit; //idx|idx|idx|idx|idx
//                public string strPetlist;   
//                public string strSuitEffect; //ideffect| ideffect

//                public CSMsgDiyMapSave()
//                {
//                        SetTag("cDiyMapSave");
//                }
//        }

//        class CSMsgDiyMapEnable : CSMsgBaseReq
//        {
//                public int index;
//                public CSMsgDiyMapEnable()
//                {
//                        SetTag("cDiyMapEnable");
//                }
//        }


//        public class SCMsgDefenseMap : SCMsgBaseAck  //自制迷宫服务器返还
//        {
//                public int index;
//                public int idterrain;
//                public int enter_index;
//                public int exit_index;
//                public string fraginfo;         //（idfragcfg + level 组成id ）index&id| index&id | index&id
//                public string directinfo;       //index&direct| index& direct | index& direct。。。
//                public string petlist;
//                public SCMsgDefenseMap()
//                {
//                        SetTag("sDefenseMap");
//                }
//        }

//        class CSMsgAckDefendDiyMap : CSMsgBaseReq  //测试自定义迷宫
//        {
//                public string petlist;
//                public CSMsgAckDefendDiyMap()
//                {
//                        SetTag("cAckDefendDiyMap");
//                }
//        }


        
//}
