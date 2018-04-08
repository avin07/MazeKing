using UnityEngine;
using System.Collections;


public enum EVISIT_REWARD_TYPE
{
    随机恶癖清除 = 1,
    固定一条or多条选一条美德获取 = 2,
    单人经验获取 = 3,
    单人技能升级 = 4,
    单人压力清除 = 5,
    全局压力清除 = 6,
    本回合食物回复 = 7,
    指定item或equip获取 = 8,
    item或equip价值抽取 = 9,
    驿站强力英雄获取 = 10,
    奖励副本获取 = 11,
}


public enum EVISIT_PAY_TYPE
{
    恶癖 = 1,
    item或equip = 2,
    压力值 = 3,
    出门 = 4,
}


namespace Message
{

        class SCMsgNpcInfo : SCMsgBaseAck   //NPC
        {
                public long id;
                public int idConfig;
                public long build_id;
                public int nLiveDay;
                public int nPlace;
                public SCMsgNpcInfo()
                {
                        SetTag("sNpcInfo");
                }
        }


        class SCMsgNpcVisitInfo : SCMsgBaseAck   //访客
        {
                public long id;
                public string strGoodsList;
                public SCMsgNpcVisitInfo()
                {
                        SetTag("sNpcVisitInfo");
                }
        }

        class SCMsgNpcGoodsInfo : SCMsgBaseAck //商人
        {
                public long id;
                public int idConfig;
                public string strGoodsList; // id：实例ID；idConfig: 配置ID； strGoodsList: type&type_id&count|type&type_id&count|...
                public SCMsgNpcGoodsInfo()
                {
                        SetTag("sNpcGoodsInfo");
                }
        }
   
        class SCMsgNpcDisappear : SCMsgBaseAck //npc消失
        {
                public long id;
                public SCMsgNpcDisappear()
                {
                        SetTag("sNpcDisappear");
                }
        }

        class SCMsgRecruitPet : SCMsgBaseAck //招募成功
        {
                public long idnpc;
                public SCMsgRecruitPet()
                {
                        SetTag("sRecruitPet");
                }
        }

        class SCMsgRecruitInfo : SCMsgBaseAck //招募界面反馈
        {
                public long idnpc;
                public SCMsgRecruitInfo()
                {
                        SetTag("sNpcRecruitInfo");
                }
        }

        class SCMsgNpcTaskInfo : SCMsgBaseAck //任务npc反馈
        {
                public long idNpc;     
                public int idNpcCfg;
                public SCMsgNpcTaskInfo()
                {
                        SetTag("sNpcTaskInfo");
                }
        } 

        class CSMsgNpcTrade : CSMsgBaseReq
        {
                public long id;
                public string strSellList;     //strSellList:玩家卖出商品列表（格式同strGoodsList
                public string strBuyList;       //strBuyList:玩家买入商品列表(格式同上)
                public CSMsgNpcTrade()
                {
                        SetTag("cNpcTrade");
                }
        }


        class CSMsgNpcVisit : CSMsgBaseReq
        {
                public long id;
                public string target;
                public CSMsgNpcVisit()
                {
                        SetTag("cNpcVisit");
                }
        }

        class CSMsgNpcTrigger : CSMsgBaseReq  //触发npc
        {
                public long id;
                public CSMsgNpcTrigger()
                {
                        SetTag("cNpcTrigger");
                }
        }


   
}