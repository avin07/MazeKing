using UnityEngine;
using System.Collections;


namespace Message
{
        class SCMsgUnlockBuild : SCMsgBaseAck
        {
                public string strBuild; // idbuildcfg|idbuildcfg
                public SCMsgUnlockBuild()
                {
                        SetTag("sUnlockBuild");
                }
        }

        class SCMsgAddUnlockBuild : SCMsgBaseAck
        {
                public int idbuildcfg;
                public SCMsgAddUnlockBuild()
                {
                        SetTag("sAddUnlockBuild");
                }
        }


#region 海拔高度


        class SCMsgHouseLandInfo : SCMsgBaseAck 
        {
                public string strLandInfo; // 转换成数组 Convert.FromBase64String
                public SCMsgHouseLandInfo()
                {
                        SetTag("sHouseLandInfo");
                }
        }


        class CSMsgBuildLandInfoModify : CSMsgBaseReq //改变地貌信息（index 从0开始）
        {
                public string strModifyInfo;// 格式：index1&value1|index2&value2|...
                public CSMsgBuildLandInfoModify()
                {
                        SetTag("cBuildLandInfoModify");
                }       
        }


#endregion


#region 宝藏

        class SCMsgHouseLandTreasure : SCMsgBaseAck
        {
                public string strTreasureInfo;  // strTreasureInfo格式：idPos(三维坐标),idTreasureCfg;... （x + z * homesize） * 100 + y
                public int idCleaningPos;       // 正在清理
                public int nRemainTime;         // 剩余时间
                public string strClearList;     //idPos1|idPos2|  已经清理完成的点
                public SCMsgHouseLandTreasure()
                {
                        SetTag("sHouseLandTreasure");
                }
        }

        class CSMsgBuildTreasureClean : CSMsgBaseReq 
        {
                public int idPos;       // 清理宝藏
                public CSMsgBuildTreasureClean()
                {
                        SetTag("cBuildTreasureClean");
                }
        }

        class CSMsgBuildTreasureReward : CSMsgBaseReq
        {
                public int idPos;       // 领取宝藏
                public CSMsgBuildTreasureReward()
                {
                        SetTag("cBuildTreasureReward");
                }
        }

        class SCMsgBuildTreasureReward : SCMsgBaseAck
        {
                public int idPos;       // 领取宝藏成功
                public SCMsgBuildTreasureReward()
                {
                        SetTag("sBuildTreasureReward");
                }
        }

        class SCMsgHouseLandTreasureCleaning : SCMsgBaseAck
        {
                public int idCleaningPos;       // 正在清理
                public int nRemainTime;         // 剩余时间
                public SCMsgHouseLandTreasureCleaning()
                {
                        SetTag("sHouseLandTreasureCleaning");
                }
        }


#endregion


}