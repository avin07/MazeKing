using UnityEngine;
using System.Collections;

public enum EBuildState
{
        eWork = 0,
        eUpgrage = 1,  //建造升级中
        eClear = 2,
}

namespace Message
{

#region 家园操作

        class CSMsgHouseBuildCreate : CSMsgBaseReq  //新建建筑
        {
                //配置ID 原点坐标 朝向,原点坐标nBottomPos(x*100+y) 朝向nFace
                public int bulid_id;
                public int pos;
                public int face;
                public CSMsgHouseBuildCreate()
                {
                        SetTag("cHouseBuildCreate");
                }

        }
                
        class CSMsgHouseBuildMove : CSMsgBaseReq  //移动建筑//
        {
                //id实例 destPos face
                public long id;
                public int pos;
                public int face;
                public long idRoom;
                public CSMsgHouseBuildMove()
                {
                        SetTag("cHouseBuildMove");
                }
        }

        class CSMsgHouseBuildClear : CSMsgBaseReq  //删除建筑
        {
                public long id;
                public CSMsgHouseBuildClear()
                {
                        SetTag("cHouseBuildClear");
                }
        }

        class CSMsgHouseBuildUpgrade : CSMsgBaseReq  //建筑升级
        {
                public long id;
                public CSMsgHouseBuildUpgrade()
                {
                        SetTag("cHouseBuildUpgrade");
                }
        }

        class CSMsgBuildQuickUpdrage : CSMsgBaseReq  //建筑快速升级升级
        {
                public long id;
                public CSMsgBuildQuickUpdrage()
                {
                        SetTag("cQuickUpdrage");
                }
        }

        class CSMsgHouseBuildUpgradeCancel : CSMsgBaseReq  //升级取消
        {
                public long id;
                public CSMsgHouseBuildUpgradeCancel()
                {
                        SetTag("cHouseBuildUpgradeCancel");
                }
        }

        class CSMsgBuildStateReq : CSMsgBaseReq  //客户端时间到了询问服务器
        {
                public long id;
                public CSMsgBuildStateReq()
                {
                        SetTag("cBuildStateChange");
                }
        }

        public class SCMsgHouseBuildInfo : SCMsgBaseAck
        {
                public long id;                                 //实例
                public int pos;
                public int face;
                public int bulid_id;
                public int level;                               //建筑等级
                public int state;                               //当前状态 EBuildState
                public int pass_time;                           //当前状态经过的时间
                public long belong_room;
                public SCMsgHouseBuildInfo()
                {
                    SetTag("sHouseBuildInfo");
                }
        }

        public class SCMsgHouseBuildRoomInfo : SCMsgBaseAck
        {
                public long idroom;                             //实例
                public int idsuit;                              //激活的套装//
                public string allbuild;                         //里面的家具(idRealBuild|idRealBuild...)
                public string alldoor_wall;                     //建造中家具 idRealBuild|idRealBuild...
                public SCMsgHouseBuildRoomInfo()
                {
                    SetTag("sRoom");
                }
        }

        class CSMsgBuildCreateRoom : CSMsgBaseReq 
        {
                public string allFurniture;
                public string allWalldoor;
                public CSMsgBuildCreateRoom()
                {
                        SetTag("cBuildCreateRoom");
                }
        }

        class CSMsgRoomChangeSuit : CSMsgBaseReq 
        {
                public long idRoom;
                public int idSuit;
                public CSMsgRoomChangeSuit()
                {
                        SetTag("cRoomChangeSuit");
                }
        }

        class CSMsgBuildRoomClear : CSMsgBaseReq
        {
                public long idroom;
                public CSMsgBuildRoomClear()
                {
                        SetTag("cBuildRoomClear");
                }
        }

        class SCMsgHouseBuildInfoEnd : SCMsgBaseAck
        {
                public SCMsgHouseBuildInfoEnd()
                {
                        SetTag("sHouseBuildInfoEnd");
                }
        }

        class SCMsgHouseBuildClear : SCMsgBaseAck
        {
                public long id;
                public SCMsgHouseBuildClear()
                {
                        SetTag("sHouseBuildClear");
                }
        }
    
        class SCMsgNeedObstaclePos : SCMsgBaseAck  //服务器需要的点数
        {
                public int count;
                public SCMsgNeedObstaclePos()
                {
                        SetTag("sNeedObstaclePos");
                }
        }

        class CSMsgCreateObstacle : CSMsgBaseReq 
        {
                public int x;
                public int y;
                public CSMsgCreateObstacle()
                {
                        SetTag("cCreateObstacle");
                }
        }

#endregion

#region 制作

        class CSMsgHouseBenchProduce : CSMsgBaseReq //物品制作
        {
                public long id; //建筑id
                public int cfg_id; //配方id
                public CSMsgHouseBenchProduce()
                {
                        SetTag("cHouseBenchProduce");
                }
        }

        class CSMsgBenchCancelProduce : CSMsgBaseReq //取消物品制作
        {
                public long id; //建筑id
                public CSMsgBenchCancelProduce()
                {
                        SetTag("cBenchCancelProduce");
                }
        }

        class SCMsgHouseBenchProduce : SCMsgBaseAck
        {
                public long id;
                public int idFormula; //配方
                public SCMsgHouseBenchProduce()
                {
                        SetTag("sHouseBenchProduceSuc");
                }
        }

        public class SCMsgBuildBench : SCMsgBaseAck
        {
                public long id;         //
                public int m_idFormula;                 //制作中的物品
                public int rest_time;

                public SCMsgBuildBench()
                {
                        SetTag("sBuildBench");
                }
        }

        class CSMsgQuickBench : CSMsgBaseReq  //快速制造
        {
                public long idrealbuild;
                public CSMsgQuickBench()
                {
                        SetTag("cQuickBench");
                }
        }

        //科技在制作中

        class CSMsgResearchSciTech : CSMsgBaseReq  //快速制造
        {
                public long idrealbuild;
                public int sciId;
                public CSMsgResearchSciTech()
                {
                        SetTag("cResearchSciTech");
                }
        }

        public class SCMsgSciTech : SCMsgBaseAck
        {
                public string sciStr; // 系列|等级&//
                public SCMsgSciTech()
                {
                        SetTag("sSciTech");
                }
        }

        public class SCMsgSciTechAdd : SCMsgBaseAck
        {
                public int sciStr;
                public long id;
                public SCMsgSciTechAdd()
                {
                        SetTag("sSciTechAdd");
                }
        }

#endregion

#region 生产类建筑

        class CSMsgHouseProduceCollect : CSMsgBaseReq //请求家园信息
        {
                public long id; //建筑id
                public CSMsgHouseProduceCollect()
                {
                        SetTag("cHouseProduceCollect");
                }
        }

        class SCMsgBuildProduce : SCMsgBaseAck
        {
                public long id;                 //建筑id
                public int pass_time;           //距离上一次收获经过的时间// 
                public int count;               //收获的数量
                public SCMsgBuildProduce()
                {
                        SetTag("sBuildProduce");
                }
        }

#endregion


#region 广播台

        class SCMsgBuildInnInfo : SCMsgBaseAck
        {
                public int nRemianTime;
                public SCMsgBuildInnInfo()
                {
                        SetTag("sInn");
                }
        }

        class CSMsgHotelGetHero : CSMsgBaseReq  //驿站
        {
                public long npcId;
                public CSMsgHotelGetHero()
                {
                        SetTag("cHotelGetHero");
                }
        }

        class CSMsgBuildHotelRefresh : CSMsgBaseReq 
        {
                public long bySuper;  // 0:时间刷新 1：钻石刷新
                public CSMsgBuildHotelRefresh()
                {
                        SetTag("cBuildHotelRefresh");
                }
        }

        class CSMsgBuildHotelLeave : CSMsgBaseReq 
        {
                public long npcId;  
                public CSMsgBuildHotelLeave()
                {
                        SetTag("cBuildHotelLeave");
                }
        }

#endregion

#region 入住以及各种治疗

        class CSMsgPetCheckIn : CSMsgBaseReq 
        {
                public long realBuild;
                public long petId;
                public CSMsgPetCheckIn()
                {
                        SetTag("cBuildPetStay");
                }
        }
               
        class SCMsgBuildStay : SCMsgBaseAck  //入住信息
        {
                public long realBuild;
                public string heroIds; //id|id|…
                public SCMsgBuildStay()
                {
                        SetTag("sBuildStay");
                }
        }


        public class SCMsgBuildCure : SCMsgBaseAck  
        {
                public long id;
                public string petlist; //id|id|…
                public SCMsgBuildCure()
                {
                        SetTag("sBuildCure");
                }
        }



#endregion


#region 属性类


        class SCMsgRoomAttr : SCMsgBaseAck  //属性类
        {
                public long id;
                public SCMsgRoomAttr()
                {
                        SetTag("sRoomAttr");
                }
        }

#endregion
}
