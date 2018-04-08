using UnityEngine;
using System.Collections;
namespace Message
{
        class SCMsgNpcVillageEnter : SCMsgBaseAck
        {
                public int idCfg;
                public SCMsgNpcVillageEnter()
                {
                        SetTag("sNpcVillageInit");
                }
        }
        class SCMsgPlayerVillageEnter : SCMsgBaseAck
        {
                public string landInfo;                 //地形信息
                public string buildInfo;                //cfg_id&level&pos|     需要解压
                public string villagerInfo;             //idCharactar&nLevel|idCharactar&nLevel|
                public SCMsgPlayerVillageEnter()
                {
                        SetTag("sPlayerVillageInit");
                }
        }
        class SCMsgVillageState : SCMsgBaseAck
        {
                public int state;
                public int remainTime;
                public SCMsgVillageState()
                {
                        SetTag("sVillageInfo");
                }
        }
        class SCMsgVillageTeam : SCMsgBaseAck
        {
                public string info;
                public SCMsgVillageTeam()
                {
                        SetTag("sVillageTeamMember");
                }
        }
        class CSMsgVillageChoose : CSMsgBaseReq
        {
                public long idHero;
                public int idVillager;
                public int idCfg;
                public CSMsgVillageChoose()
                {
                        SetTag("cVillagerChoose");
                }
        }
        class SCMsgVillageBuilding : SCMsgBaseAck
        {
                public long idBuildLite;
                public int idCfg;
                public int nPos;
                public int nLevel;
                public int idAvatar;
                public SCMsgVillageBuilding()
                {
                        SetTag("sBuildLite");
                }
        }
        class SCMsgVillageLeave : SCMsgBaseAck
        {
                public int result;      //0失败1成功2逃离
                public SCMsgVillageLeave()
                {
                        SetTag("sLeaveVillage");
                }
        }

        /// <summary>
        /// 迷宫掉落
        /// </summary>
        class SCMsgVillageAward : SCMsgBaseAck
        {
                public string info;// {nType&nTypeId&nCount|nType&nTypeId&nCount|}
                public SCMsgVillageAward()
                {
                        SetTag("sVillageAward");
                }
        }
        class CSMsgVillageLeave : CSMsgBaseReq
        {
                public CSMsgVillageLeave()
                {
                        SetTag("cVillageLeave");
                }
        }

        class CSMsgVillageAwardAdd : CSMsgBaseReq
        {
                public string info;     //nType&nTypeId&nCount|nType&nTypeId&nCount|
                public CSMsgVillageAwardAdd()
                {
                        SetTag("cVillageAwardAdd");
                }
        }

        class CSMsgVillageInvade : CSMsgBaseReq
        {
                public CSMsgVillageInvade()
                {
                        SetTag("cVillageInvade");
                }
        }

        class CSMsgVillageMatching : CSMsgBaseReq
        {
                public string petlist;
                public CSMsgVillageMatching()
                {
                        SetTag("cVillageMatching");
                }
        }

        class CSMsgVillageTrigger : CSMsgBaseReq
        {
                public int idVillager;
                public CSMsgVillageTrigger()
                {
                        SetTag("cVillageTrigger");
                }
        }
}
