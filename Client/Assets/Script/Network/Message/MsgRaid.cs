using UnityEngine;
using System.Collections;
namespace Message
{
        class SCMsgRaidNodeChange : SCMsgBaseAck  //技能变更
        {
                public int roomIndex;
                public int nodeCfgId;
                public int elemInfo;      //这里元素id = 元素配置id * 10 + 元素剩余处理次数
                public int skillCfgId;
                public long heroId;

                public int NodeId
                {
                        get
                        {
                                return roomIndex * 10000 + nodeCfgId;
                        }
                }

                public int ElemId
                {
                        get
                        {
                                return elemInfo / 10;
                        }
                }
                public int ElemCount
                {
                        get
                        {
                                return elemInfo % 10;
                        }
                }
                public SCMsgRaidNodeChange()
                {
                        SetTag("sRaidMapChangeInfo");
                }
        }

        public class SCMsgRaidMapInitNodeInfo : SCMsgBaseAck  //初始化变更
        {
                public int roomIndex;
                public int nodeCfgId;
                public int elemInfo;      //这里元素id = 元素配置id * 10 + 元素剩余处理次数

                public int NodeId
                {
                        get
                        {
                                return roomIndex * 10000 + nodeCfgId;
                        }
                }
                public int ElemId
                {
                        get
                        {
                                return elemInfo / 10;
                        }
                }
                public int ElemCount
                {
                        get
                        {
                                return elemInfo % 10;
                        }
                }
                //idRaid x  y ElementId; RoadId; nFogId; bIsMask
                public SCMsgRaidMapInitNodeInfo()
                {
                        SetTag("sRaidMapInitChangeInfo");
                }
        }

        class SCMsgRaidPerceptionNodeInfo : SCMsgBaseAck //感知变更
        {
                public int x;
                public int y;
                public int elemInfo;      //这里元素id = 元素配置id * 10 + 元素剩余处理次数

                public int ElemId
                {
                        get
                        {
                                return elemInfo / 10;
                        }
                }
                public int ElemCount
                {
                        get
                        {
                                return elemInfo % 10;
                        }
                }
                public SCMsgRaidPerceptionNodeInfo()
                {
                        SetTag("sRaidMapPerceptionChangeInfo");
                }
        }

        class CSMsgRaidPerceptionNode : CSMsgBaseReq
        {
                public int x;
                public int y;
                public CSMsgRaidPerceptionNode()
                {
                        SetTag("cRaidPerceptionNode");
                }
        }

        class SCMsgRaidEnd : SCMsgBaseAck
        {
                public SCMsgRaidEnd()
                {
                        SetTag("sRaidMapEnd");
                }
        }

        class CSMsgRaidMove : CSMsgBaseReq
        {
                public int x;
                public int y;
                public CSMsgRaidMove()
                {
                        SetTag("cRaidMapMove");
                }
        }

        class CSToNextRaidMap : CSMsgBaseReq
        {
                public CSToNextRaidMap()
                {
                        SetTag("cToNextRaidMap");
                }
        }

        class SCMsgRaidTriggerTrap : SCMsgBaseAck
        {
                public int idTrap;
                public long idHero;
                public SCMsgRaidTriggerTrap()
                {
                        SetTag("sRaidMapTriggerTrap");
                }
        }

        class CSMsgRaidOperateElem : CSMsgBaseReq
        {
                public int nodeId;
                public int toolId;
                public int advSkillId;
                public long idHero;
                public CSMsgRaidOperateElem()
                {
                        SetTag("cRaidMapUseTool");
                }
        }

        //sRaidMaxJumpFloorInfo idRiad* 1000 + idMaxFloor&idRiad* 1000 + idMaxFloor&idRiad* 1000 + idMaxFloor。。。

        class SCRaidMaxJumpFloorInfo : SCMsgBaseAck
        {
                public string raid_floor;
                public SCRaidMaxJumpFloorInfo()
                {
                        SetTag("sRaidMaxJumpFloorInfo");
                }
        }


        class SCRaidMapTeam : SCMsgBaseAck
        {
                public string info;
                public SCRaidMapTeam()
                {
                        SetTag("sRaidMapTeamMember");
                }
        }
        class SCRaidLeaderSkill : SCMsgBaseAck
        {
                public string info;// skill|allowusetimes&skill|allowusetimes...
                public SCRaidLeaderSkill()
                {
                        SetTag("sRaidMapLeadSkill");
                }
        }
        class SCRaidCampTimes : SCMsgBaseAck
        {
                public int campTimes;
                public SCRaidCampTimes()
                {
                        SetTag("sRaidMapCampTimes");
                }
        }

        /// <summary>
        /// 迷宫掉落
        /// </summary>
        class SCMsgRaidAward : SCMsgBaseAck
        {
                public string info;// {nType&nTypeId&nCount|nType&nTypeId&nCount|}
                public SCMsgRaidAward()
                {
                        SetTag("sRaidAward");
                }
        }

        class CSMsgRaidAwardDrop : CSMsgBaseReq
        {
                public string info;//nType&nTypeId&nCount|nType&nTypeId&nCount|
                public CSMsgRaidAwardDrop()
                {
                        SetTag("cRaidAwardDrop");
                }
        }

        class CSMsgRaidAwardAdd : CSMsgBaseReq
        {
                public string info;     //nType&nTypeId&nCount|nType&nTypeId&nCount|
                public CSMsgRaidAwardAdd()
                {
                        SetTag("cRaidAwardAdd");
                }
        }
        /// <summary>
        /// 离开副本
        /// // flag: 0:失败 1:成功 2:逃逸       
        /// </summary>
        public class SCMsgRaidMapLeave : SCMsgBaseAck
        {
                public int flag;
                public int exp;
                public int money;
                public string dropReward;     //item|count&item|count&item|count;
                public string taskReward;       //
                public string behavior;         //idPet&idRemoveBehavior&idReplaceBehavior|idPet&idRemoveBehavior&idReplaceBehavior|

                public SCMsgRaidMapLeave()
                {
                        SetTag("sRaidMapLeave");
                }
        }

        class CSMsgRaidMapLeave : CSMsgBaseReq
        {
                public CSMsgRaidMapLeave()
                {
                        SetTag("cLeaveRaid");
                }
        }

        class CSMsgLoadUnfinishRaid : CSMsgBaseReq
        {
                public CSMsgLoadUnfinishRaid()
                {
                        SetTag("cLoadUnFinishedRaid");
                }
        }

        class SCMsgCheckUnfinishRaid : SCMsgBaseAck
        {
                public long idRaid;
                public SCMsgCheckUnfinishRaid()
                {
                        SetTag("sIsHaveUnFinsihedRaid");
                }
        }
        class CSMsgContinueBattle : CSMsgBaseReq
        {
                public CSMsgContinueBattle()
                {
                        SetTag("cBattleContinue");
                }
        }

        class SCMsgContinueUnFinishBattle : SCMsgBaseAck
        {
                public SCMsgContinueUnFinishBattle()
                {
                        SetTag("sBattleUnFinished");
                }
        }

        class SCMsgRaidTaskCount : SCMsgBaseAck
        {
                public string taskCount;    //target_id&count|target_id&count|...
                public SCMsgRaidTaskCount()
                {
                        SetTag("sRaidTaskCount");
                }
        }


        class SCMsgRaidMapInfo : SCMsgBaseAck
        {
                public int size;                    //1 就是配置地图 3 就是3*3的地图
                public string strInfo;          //（idMap&direct&idOwn&isLocked&isSpied&isComplete&isOpen&isBroken|idMap&direct&idOwn|idMap&direct。。。）
                public int enterRoomId;
                public int idRaid;             //raid_id * 1000 + floor
                public SCMsgRaidMapInfo()
                {
                        SetTag("sRandomRaidMapInfo");
                }
        }

        class SCMsgRaidSpecialRoom : SCMsgBaseAck
        {
                public int idFragment;
                public int idOwnidx;
                public SCMsgRaidSpecialRoom()
                {
                        SetTag("sBedRoomAccessFrag");
                }
        }

        class CSMsgRaidTeleport : CSMsgBaseReq
        {
                public int ownIdx;

                public int idNode;
                public CSMsgRaidTeleport()
                {
                        SetTag("cRaidMagicTeleport");
                }
        }
        class SCMsgRaidTeleport : SCMsgBaseAck
        {
                public int index;
                public SCMsgRaidTeleport()
                {
                        SetTag("sRaidTeleport");
                }
        }

        //方向类枚举
        public enum ELink
        {
                elink_eswn = 15,

                elink_esw_ = 14,
                elink_es_n = 13,
                elink_e_wn = 11,
                elink__swn = 7,

                elink_es__ = 12,
                elink_e__n = 9,
                elink__sw_ = 6,
                elink___wn = 3,

                elink_e_w_ = 10,
                elink__s_n = 5,

                elink_e___ = 8,
                elink__s__ = 4,
                elink___w_ = 2,
                elink____n = 1,
        }

        class CSMsgRaidStoryOption : CSMsgBaseReq
        {
                public int result;
                public CSMsgRaidStoryOption()
                {
                        SetTag("cStoryOption");
                }
        }
        class CSMsgRaidStory : CSMsgBaseReq
        {
                public int nodeid;
                public CSMsgRaidStory()
                {
                        SetTag("cRaidMapStoryPlot");
                }
        }

        class SCMsgRaidStoryTrigger : SCMsgBaseAck
        {
                public int storyId;
                public string nodestr;

                public int nodeId
                {
                        get
                        {
                                if (!string.IsNullOrEmpty(nodestr))
                                {
                                        string[] tmps = nodestr.Split('|');
                                        if (tmps.Length == 2)
                                        {
                                                int ownIdx = int.Parse(tmps[0]);
                                                int nodeCfgId = int.Parse(tmps[1]);
                                                return ownIdx * 10000 + nodeCfgId;
                                        }
                                }
                                return -1;
                        }
                }
                public SCMsgRaidStoryTrigger()
                {
                        SetTag("sStoryTrigger");
                }
        }

        class CSMsgRaidRoomProc : CSMsgBaseReq
        {
                public int type;
                public string strParam;
                public CSMsgRaidRoomProc()
                {
                        SetTag("cRaidMapRoomProc");
                }
        }

        class SCMsgPressureTorture : SCMsgBaseAck
        {
                public long idHero;
                public int idCfg;
                public SCMsgPressureTorture()
                {
                        SetTag("sPressureTorture");
                }
        }

        class CSMsgRaidCamp : CSMsgBaseReq
        {
                public CSMsgRaidCamp()
                {
                        SetTag("cRaidCamp");
                }
        }
        class CSMsgRaidCampEat : CSMsgBaseReq
        {
                public string strFood;//idItem|idItem
                public CSMsgRaidCampEat()
                {
                        SetTag("cCampEatFood");
                }
        }

        class CSMsgRaidCampFinish : CSMsgBaseReq
        {
                public CSMsgRaidCampFinish()
                {
                        SetTag("cRaidFinishCamp");
                }
        }

        class SCMsgRaidCampState : SCMsgBaseAck
        {
                public int flag;        //1：扎营，0:不扎营
                public SCMsgRaidCampState()
                {
                        SetTag("sCampState");
                }
        }
        class SCMsgRaidCampSP : SCMsgBaseAck
        {
                public int sp;
                public SCMsgRaidCampSP()
                {
                        SetTag("sCampSp");
                }
        }
        class CSMsgRaidCampSkill : CSMsgBaseReq
        {
                public long idSrcHero;//: 释放者伙伴id
                public long idCampSkillCfg;//： 扎营技能实例id
                public long idDestHero;//:被释放着伙伴id （可选参数，群体技能可以不发送此参数）
                public CSMsgRaidCampSkill()
                {
                        SetTag("cRaidUseCampSkill");
                }
        }

        /// <summary>
        /// // strParam ：战斗类型  					0
        /// 		        压力变更类型  				压力变更值 
        ///                     物品丢失        			byType(大类）&nTypeId(小类)|byType&nTypeId 。。。
        ///                     增加buf        				idbuff
        /// </summary>
        class SCMsgRaidCampEvent : SCMsgBaseAck
        {
                public int eventId;
                public long idHero;
                public string strParam;
                public SCMsgRaidCampEvent()
                {
                        SetTag("sCompleteCampEvent");
                }
        }
        class CSMsgRaidCampEvent : CSMsgBaseReq
        {
                public int x;
                public int y;
                public CSMsgRaidCampEvent()
                {
                        SetTag("cCampEventTrigger");
                }
        }

        class SCMsgRaidCampInfo : SCMsgBaseAck
        {
                public int state;       //1进餐前，2进餐完
                public string m_strCampSkillInfo;      //扎营技能使用情况 格式：idHero|idskill,idskill&idhero|idskill&...
                public int skillPoint;

                public SCMsgRaidCampInfo()
                {
                        SetTag("sRaidCampInfo");
                }
        }

        class CSMsgRaidDoRandomElem : CSMsgBaseReq
        {
                public int x;
                public int y;
                public CSMsgRaidDoRandomElem()
                {
                        SetTag("cRaidMapEnterNodeChange");
                }
        }

        class SCMsgRaidRoomState : SCMsgBaseAck
        {
                public string roomstate;        //roomid|roomid

                public SCMsgRaidRoomState()
                {
                        SetTag("sRaidMapRoomState");
                }
        }

        //cRaidMapOpenDoor idPet idSkillCfg node_index
        class CSMsgRaidOpenDoor : CSMsgBaseReq
        {
                public long idHero;
                public int idTool;
                public long skillCfgId;
                public int nextIndex;

                public CSMsgRaidOpenDoor()
                {
                        SetTag("cRaidMapOpenDoor");
                }
        }

        class CSMsgRaidEnterConditionRoom : CSMsgBaseReq
        {
                public int roomIndex;
                public CSMsgRaidEnterConditionRoom()
                {
                        SetTag("cEnterConditionFrag");
                }
        }
        class CSMsgRaidEnterHideRoom : CSMsgBaseReq
        {
                public int roomIndex;
                public CSMsgRaidEnterHideRoom()
                {
                        SetTag("cEnterHideFrag");
                }
        }
        //         class CSMsgRaidResetDoor : CSMsgBaseReq
        //         {
        //                 public int nodeId;
        //                 public CSMsgRaidResetDoor()
        //                 {
        //                         SetTag("cRaidResetDoor");
        //                 }
        //         }

        /// <summary>
        ///房间选择元素 
        /// </summary>
        class CSMsgRaidElemChoose : CSMsgBaseReq
        {
                public int nodeId;
                public int option;//1 2 3
                public long idHero;
                public string param;    //默认"0"
                public CSMsgRaidElemChoose()
                {
                        SetTag("cRaidEleChoose");
                }
        }

        /// <summary>
        /// 创建单向房间
        /// </summary>
        class SCMsgRaidCreateRoom : SCMsgBaseAck
        {
                public int index;
                public int pieceCfgId;
                public int linkState;
                public SCMsgRaidCreateRoom()
                {
                        SetTag("sCreateSingleRoom");
                }
        }

        class SCMsgRaidOpenDoor : SCMsgBaseAck
        {
                public int nextIdx;
                public SCMsgRaidOpenDoor()
                {
                        SetTag("sRaidMapOpenDoor");
                }
        }

        /// <summary>
        /// 开门失败反馈
        /// </summary>
        class SCMsgRaidOpenDoorFail : SCMsgBaseAck
        {
                public SCMsgRaidOpenDoorFail()
                {
                        SetTag("sRaidMapOpenDoorFail");
                }
        }

        class SCMsgRaidDoorBattle : SCMsgBaseAck
        {
                public SCMsgRaidDoorBattle()
                {
                        SetTag("sRaidDoorEvent");
                }
        }

        class CSMsgRaidTaskSubmit : CSMsgBaseReq
        {
                public int taskId;
                public CSMsgRaidTaskSubmit()
                {
                        SetTag("cRaidBranchSubmit");
                }
        }

        /// <summary>
        /// 迷宫支线任务
        /// idConfig: 迷宫任务配置id（raid_task） 
        /// nState: 0为未完成 1为已完成 
        /// strTaskInfo: idTarget&count|idTarget&count|
        /// </summary>
        class SCMsgRaidBranchTask : SCMsgBaseAck
        {
                public int taskId;
                public int nState;
                public string countStr;

                public SCMsgRaidBranchTask()
                {
                        SetTag("sRaidBrach");
                }
        }

        /// <summary>
        /// 
        /// </summary>
        class SCMsgRaidNeedToolTip : SCMsgBaseAck
        {
                public int idToolCfg;
                public SCMsgRaidNeedToolTip()
                {
                        SetTag("sRaidEleNeedTool");
                }
        }

        /// <summary>
        /// 未完成的房间列表
        /// </summary>
        class SCMsgRaidRoomUnFinishList : SCMsgBaseAck
        {
                public string info; //idFragment&complete|idFragment&complete|idFragment&complete|idFragment&complete...
                public SCMsgRaidRoomUnFinishList()
                {
                        SetTag("sRaidMapRoomUnCompleteInfo");
                }
        }

        class SCMsgRaidRoomCompleted : SCMsgBaseAck
        {
                public int index;
                public SCMsgRaidRoomCompleted()
                {
                        SetTag("sRaidRoomCompleted");
                }
        }
        class SCMsgRaidRoomUnCompleted : SCMsgBaseAck
        {
                public int index;
                public SCMsgRaidRoomUnCompleted()
                {
                        SetTag("sRaidRoomUnCompleted");
                }
        }

        class CSMsgRaidTriggerHideTrap : CSMsgBaseReq
        {
                public int nodeId;
                public CSMsgRaidTriggerHideTrap()
                {
                        SetTag("cRaidTriggerCovertTrap");
                }
        }

        class CSMsgRaidAlchemy : CSMsgBaseReq
        {
                public string info;// itemtype|itemtype..配置id 
                public int nodeId;
                public CSMsgRaidAlchemy()
                {
                        SetTag("cRaidSmeltingFurnace");
                }
        }

        class SCMsgRaidAlchemyResult : SCMsgBaseAck
        {
                public string info;//itemtype&itemid&itemcout|itemtype&itemid&itemcout|itemtype&itemid&itemcout...
                public SCMsgRaidAlchemyResult()
                {
                        SetTag("sRaidSmeltingFurnace");
                }
        }

        class SCMsgRaidChooseReturn : SCMsgBaseAck
        {
                public int doorId;
                public SCMsgRaidChooseReturn()
                {
                        SetTag("sEleChooseReturn");
                }
        }

        class SCMsgRaidStealth : SCMsgBaseAck
        {
                public int flag;
                public int nodeId;
                public SCMsgRaidStealth()
                {
                        SetTag("sRaidLurk");
                }
        }

        class SCMsgRaidRoomLockList : SCMsgBaseAck
        {
                public string info;
                public SCMsgRaidRoomLockList()
                {
                        SetTag("sRaidMapRoomLock");
                }
        }
        class SCMsgRaidUnlock : SCMsgBaseAck
        {
                public int roomIndex;
                public SCMsgRaidUnlock()
                {
                        SetTag("sRaidunLock");
                }
        }

        // 密码提示
        class SCMsgRaidOperatePasswdTips : SCMsgBaseAck
        {
                public string info;
                public SCMsgRaidOperatePasswdTips()
                {
                        SetTag("sRaidMapPwd");
                }
        }


        // 密码匹配
        // sRaidInputtPasswdSuc

        public class SCMsgRaidHero : SCMsgBaseAck
        {
                public long idHero;
                public long idPet;
                public int idCharactar;
                public int nLevel;
                public long lHp;
                public long lMaxHp;
                public int lPressure;
                public int lMaxPressure;
                public int nState;
                public int nTorture;            //是否精神拷问过
                public string skillInfo;          // GetSkillInfo()： idSkill&nLevel|idSkill&nLevel|... ;   
                public string buffInfo;         //GetBuffInfo(): idBuff&nLevel&nTimes|idBuff&nLevel&nTimes|...;
                public string behaviourInfo;//GetBeHaviorInfo(): idBeHavior&idBeHavior&..       //美德恶癖
                public string behavior_obtain;//       迷宫中记录美德恶癖的获得情况
                public SCMsgRaidHero()
                {
                        SetTag("sHero");
                }
        }

        /// <summary>
        /// hp,pressure,behavior_obtain
        /// </summary>
        class SCMsgRaidHeroAttr : SCMsgBaseAck
        {
                public long idHero;
                public string propname;
                public string value;
                public SCMsgRaidHeroAttr()
                {
                        SetTag("sHeroAttr");
                }
        }
        /// <summary>
        /// 删除英雄
        /// </summary>
        class SCMsgRaidHeroLeave : SCMsgBaseAck
        {
                public long idHero;
                public SCMsgRaidHeroLeave()
                {
                        SetTag("sRaidHeroDel");
                }
        }
        /// <summary>
        /// 增加英雄
        /// </summary>
        class SCMsgRaidHeroJoin : SCMsgBaseAck
        {
                public long idHero;
                public SCMsgRaidHeroJoin()
                {
                        SetTag("sRaidHeroAdd");
                }
        }

        /// <summary>
        /// 拯救佣兵
        /// </summary>
        class CSMsgRaidRescueHero : CSMsgBaseReq
        {
                public int nodeId;
                public CSMsgRaidRescueHero()
                {
                        SetTag("cRescueExAdventurer");
                }
        }

        class SCMsgRaidRescueHero : SCMsgBaseAck
        {
                public int id;
                public SCMsgRaidRescueHero()
                {
                        SetTag("sExAdventurerSpeak");
                }
        }

        /// <summary>
        /// 发生丢失掉落道具的恶癖结果
        /// </summary>
        class SCMsgRaidLostDrop : SCMsgBaseAck
        {
                public long idHero;
                public string itemstr;//item_type&item_id&item_count|item_type&item_id&item_count。。。
                public SCMsgRaidLostDrop()
                {
                        SetTag("sRaidLostDrop");
                }
        }
        class SCMsgRaidBehaviourTrigger : SCMsgBaseAck
        {
                public long idHero;
                public int idBehaviour;
                public SCMsgRaidBehaviourTrigger()
                {
                        SetTag("sRaidBehaviour");
                }
        }

        class CSMsgRaidTriggerDice : CSMsgBaseReq
        {
                public int nodeId;
                public CSMsgRaidTriggerDice()
                {
                        SetTag("cTriggergDiceEle");
                }
        }
        class SCMsgRaidDiceResult : SCMsgBaseAck
        {
                public int nodeCfgId;
                public int roomIndex;
                public int ret;

                public int NodeId
                {
                        get
                        {
                                return roomIndex * 10000 + nodeCfgId;
                        }
                }

                public SCMsgRaidDiceResult()
                {
                        SetTag("sTriggergDiceEle");
                }
        }

        class SCMsgRaidSkillEffect : SCMsgBaseAck
        {
                public int idSkill;
                public SCMsgRaidSkillEffect()
                {
                        SetTag("sRaidSkillEffect");
                }
        }
        public class SCMsgRaidOptionNodeData : SCMsgBaseAck
        {
                public int roomIndex;
                public int nodeCfgId;
                public string optionList;       //已选过的选项列表
                public string effectList;       //已触发的选项效果列表

                public int NodeId
                {
                        get
                        {
                                return roomIndex * 10000 + nodeCfgId;
                        }
                }

                public SCMsgRaidOptionNodeData()
                {
                        SetTag("sRaidMapData");
                }
        }
        /// <summary>
        /// 掉落陷阱
        /// </summary>
        class SCMsgRaidFallTrap : SCMsgBaseAck
        {
                public SCMsgRaidFallTrap()
                {
                        SetTag("sDropRaidMap");
                }
        }
        class SCMsgRaidLastRoom : SCMsgBaseAck
        {
                public int index;
                public SCMsgRaidLastRoom()
                {
                        SetTag("sRaidLastRoom");
                }
        }

        //废弃sRaidMapCampTimes
        public class SCMsgRaidTeam : SCMsgBaseAck
        {
                public int nBright;
                public SCMsgRaidTeam()
                {
                        SetTag("sRaidTeam");
                }
        }

        class SCMsgRaidTeamAttr : SCMsgBaseAck
        {
                public string name;
                public string value;
                public SCMsgRaidTeamAttr()
                {
                        SetTag("sTeamAttr");
                }
        }

        /// <summary>
        /// 触发侦查
        /// </summary>
        class SCMsgRaidDetectTrigger : SCMsgBaseAck
        {
                public SCMsgRaidDetectTrigger()
                {
                        SetTag("sRaidSpying");
                }
        }
        class SCMsgRaidSingleRoomDetect : SCMsgBaseAck
        {
                public int ownIdx;
                public SCMsgRaidSingleRoomDetect()
                {
                        SetTag("sRaidFragSpied");
                }
        }

        class CSMsgRaidShowClue : CSMsgBaseReq
        {
                public CSMsgRaidShowClue()
                {
                        SetTag("cRaidShowClue");
                }
        }

        /// <summary>
        /// type:1 有什么数字，2是哪一位有什么数字，3是没有什么数字
        /// 
        /// </summary>
        class SCMsgRaidShowClue : SCMsgBaseAck
        {
                public string info;     //type|palce|value&type|palce|value&type|palce|value...
                public SCMsgRaidShowClue()
                {
                        SetTag("sRaidShowClue");
                }
        }
        class CSMsgRaidInputPassword : CSMsgBaseReq
        {
                public string strPwd;   //ret0|ret1|ret2|ret3|...
                public CSMsgRaidInputPassword()
                {
                        SetTag("cRaidInputPasswd");
                }
        }

        class SCMsgRaidInputPasswordError : SCMsgBaseAck
        {
                public int valueCorrectCount;
                public int orderCorrectCount;
                public SCMsgRaidInputPasswordError()
                {
                        SetTag("sRaidInputPasswdError");
                }
        }

        /// <summary>
        /// 获得新线索
        /// </summary>
        class SCMsgRaidNewClue : SCMsgBaseAck
        {
                public string info;// type|palce|value&type|palce|value
                public SCMsgRaidNewClue()
                {
                        SetTag("sRaidNewClue");
                }
        }

        /// <summary>
        /// 密码输入错误后房间损坏，再也不能进去
        /// </summary>
        class SCMsgRaidBroken : SCMsgBaseAck
        {
                public int ownidx;
                public SCMsgRaidBroken()
                {
                        SetTag("sDoorBroken");
                }
        }

        class CSMsgRaidDiscoverNewBuild : CSMsgBaseReq
        {
                public long idhero;
                public int nodeId;
                public CSMsgRaidDiscoverNewBuild()
                {
                        SetTag("cDiscoveNewBuild");
                }
        }
        class SCMsgRaidDiscoverNewBuild : SCMsgBaseAck
        {
                public int idBuild;
                public SCMsgRaidDiscoverNewBuild()
                {
                        SetTag("sDiscoveNewBuild");
                }
        }
}
