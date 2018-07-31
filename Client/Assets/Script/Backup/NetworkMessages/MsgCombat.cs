 using UnityEngine;
using System.Collections;
using Message;
namespace Message
{        
        class SCMsgBattle : SCMsgBaseAck
        {
                public int idBattle;
                public int nSeed;
                public string battleScene;
                public int idNode;
                public string strCmdList;       //nRound&idFighter&idSkill&idTarget|nRound&idFighter&idSkill&idTarget|...
                public int nBright;
                public SCMsgBattle()
                {
                        SetTag("sBattle");
                }
        }

        public class SCMsgBattleFighter : SCMsgBaseAck
        {                
                public long idFighter;
                public int idBattle;
                public int group;     //2 ATK  1 DEF
                public int hp;
                public int pressure;
                public int characterId;
                public int nLevel;
                public int atkVal;
                public int defVal;
                public int maxHp;
                public int maxPressure;
                public int speed;
                public int criticalRate;
                public int criticalDamage;
                public int accurate;
                public int penetrate;
                public int damageIncrease;
                public int dodge;
                public int debuffResistance;
                public int damageReduce;
                public int parry;
                public int rebound;
                public int tough;
                public int hpSteal;
                public int healIncrease;
                public int recoverIncrease;
                public int triggerRate;
                public int CDReduce;
                public int lDebuffAccurate;
                public int nTorture;
                public string active_skillInfo;        //idskill1&level1|idskill2&level2|...
                public string passive_skillInfo;       //idskill1&level1|idskill2&level2|...
                public string bufflist;                 //buff1&level&time|buff2&level&time|...  
                
                public SCMsgBattleFighter()
                {
                        SetTag("sBattleFighter");
                }
        }

        class SCMsgBattleStart : SCMsgBaseAck
        {
                public SCMsgBattleStart()
                {
                        SetTag("sBattleStart");
                }
        }
        class CSMsgBattleRound : CSMsgBaseReq
        {
                public int idBattle;
                public int round;
                public long fighterId;
                public int skillId;
                public long targetId;
                public CSMsgBattleRound()
                {
                        SetTag("cBattleRound");
                }
        }

        class CSMsgBattleOver : CSMsgBaseReq
        {
                public int idBattle;
                public CSMsgBattleOver()
                {
                        SetTag("cBattleOver");
                }
        }


        // idState表示对应技能触发的状态：
        public enum SKILL_STATE_TYPE
        {
                DODGE = 1, // 闪避
                CRITICAL = 2, // 暴击
                PARRY = 4, // 格挡
                DELAY = 8, // 提拉行动条
                DISPERSE = 16, // 驱散
                BUFF = 32, // buff
        }

        /// <summary>
        /// 
        /// </summary>
        public class SCMsgBattleRoundInfo : SCMsgBaseAck
        {
                public int nRound;
                public string strRandomList;  // random1|random2|...
                public SCMsgBattleRoundInfo()
                {
                        SetTag("sBattleInfo");
                }
        }
}