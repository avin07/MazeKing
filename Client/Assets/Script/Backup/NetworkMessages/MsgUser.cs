using UnityEngine;
using System.Collections;
namespace Message
{
        class SCMsgUserInfo : SCMsgBaseAck
        {
                public long id;
                public string name;
                public int house_level;  //家园等级  
                public long gold;
                public long diamond;
                public string three_star_list; //idHero1|idHero2|
                public string four_star_list;
                public string five_star_list;
                public int bag_capacity;      //道具和装备的总背包上限
                public int maze_capacity;
                public string formula_list; //额外配方
                public string ex_formula_list; //额外配方
                public int house_exp;
                public int vitality;     //体力(当前可使用的回合数)
                public int gharry_lvl;   //马车等级
                public int food;      //食物库存数量

                public int gold_capacity;                     // 金币上限           
                public int stone_capacity;                    // 石头上限          
                public int wood_capacity;                     // 木头上限          
                public int crystal_capacity;                  // 水晶上限          
                public int hide_capacity;                     // 兽皮上限    
                public int pet_capactity;                     // 人物仓库上限              
                public int stone;                             // 石头              
                public int wood;                              // 木头              
                public int crystal;                           // 水晶              
                public int hide;                              // 兽皮
                public int brick;                             // 砖
                public int prosperity;                        // 繁荣度

                public int score;                             // 自制迷宫积分
                public int guide_id;                          // 当前引导id
                public string raidGuide;                      // 迷宫引导
                public string strAchieveRewardList;           // 全局的完成成就id
                public string open_area;                      // 家园开放区域 （<=）
                public int nScene;                            // 0家园 1副本 2村庄
                public int boardTaskRefreshTimes;             // 公告板任务每日的刷新次数
                public int day_inn_refresh_times;             // inn英雄刷新次数  
                public long lastOfflineTimeStamp;             // 上一次下线时间戳

                public int produce_augmentation_per;          //生产增幅
                public int cure_augmentation_per;             //治疗增幅
                public int cure_lower_consume_per;            //治疗降价
                public int bench_lower_consume_per;           //制造降价
                public int team_member_limit;                           //上阵人数上限增加
                public int skill_lower_consume_per;             //技能打折：
                public int npc_lower_price;                     //商人打折：
                public int npc_item_count;                      //家园商人栏位

                public SCMsgUserInfo()
                {
                        SetTag("sUser");
                }
        }

        class SCUserAttrUpdate : SCMsgBaseAck
        {
                public long ID;
                public string Name;
                public string Value;

                public SCUserAttrUpdate()
                {
                        SetTag("sUserAttr");
                }
        }

        class SCFormationUpdate : SCMsgBaseAck
        {
                public long id;
                public int type;
                public string strPetList;//同步阵型信息       petId|petId|...
                public SCFormationUpdate()
                {
                        SetTag("sFormation");
                }
        }

}