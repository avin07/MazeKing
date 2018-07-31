using UnityEngine;
using System.Collections;
namespace Message
{

        public class SCMsgPetInfo : SCMsgBaseAck
        {
                public long id;
                public int idCharacter;
                public int level;
                public long exp;
                public string strHelpfulBehaviorList; //随机正面属性（&|分割）
                public string strHarmfulBehaviorList; //随机负面属性
                public string strBuffList;      //idbuff1&level1&times1|idbuff2&level2&times2|...
                public string strAttr;
                public int cur_state;   //0 正常 1 出门 2 死亡
                public string strAchieveFinishList;   //已完成成就，只包括客户端无法判断的//
                public string strAchieveRewardList;   //已领取的成就//
                public long belongToBuildID;          //归属建筑物
                public int nSkillPoint;
                //atk_add&atk_per_add&def_add&def_per_add&hp&maxhp_add&maxhp_per_add&speed_add&
                //speed_per_add&maxpressure_add&pressure&speciality&critical_rate_per_add&critical_damage_per_add&
                //accurate_per&penetrate_per&damage_increase_per&dodge_per&debuff_resistance_per&
                //damage_reduce_per&parry_per&rebound_per&tough_per&life_steal_per&heal_increase_per&
                //recover_increase_per&cooldown_reduce_per&trigger_rate_per

                public SCMsgPetInfo()
                {
                        SetTag("sPet");
                }
        }


        class SCPetAttrUpdate : SCMsgBaseAck
        {
                public long ID;
                public string Name;
                public string Value;

                public SCPetAttrUpdate()
                {
                        SetTag("sPetAttr");
                }
        }


        class SCPetDelete : SCMsgBaseAck
        {
            public long petid;      //宠物实例id
            public SCPetDelete()
            {
                SetTag("sPetDel");
            }
        }

        #region 伙伴升级

        class CSMsgPetEat : CSMsgBaseReq
        {
                public long petid;      //宠物实例id
                public string foodinfo; //宠物实例|宠物实例|
                public CSMsgPetEat()
                {
                        SetTag("cPetEat");
                }
        }

        class SCPetEat : SCMsgBaseAck
        {
                public int error_type;  //ERROR_CODE

                public SCPetEat()
                {
                        SetTag("sPetEat");
                }
        }

        #endregion

        #region 伙伴转职

        class CSMsgPetTransfer : CSMsgBaseReq
        {
                public long petid;      //宠物实例id
                public int career_id;   //目标职业id
                public string foodinfo; //宠物实例|宠物实例|
                public CSMsgPetTransfer()
                {
                        SetTag("cPetTransfer");
                }
        }

        class SCPetTransfer : SCMsgBaseAck
        {
                public long pet_id;
                public int career_id;
                public int error_type;  //ERROR_CODE

                public SCPetTransfer()
                {
                        SetTag("sPetTransfer");
                }
        }

        #endregion

        #region 宠物合成

        class SCPetMsgCompose : SCMsgBaseAck
        {
                public int career_id;
                public int error_type;  //ERROR_CODE
                public SCPetMsgCompose()
                {
                        SetTag("sPetCompose");
                }
        }

        class CSMsgPetCompose : CSMsgBaseReq
        {
                public long piece_id;      //伙伴碎片实例ID
                public CSMsgPetCompose()
                {
                        SetTag("cPetCompose");
                }
        }

        class CSMsgPetPieceQuery : CSMsgBaseReq //请求碎片
        {
                public CSMsgPetPieceQuery()
                {
                        SetTag("cPetPieceQuery");
                }
        }

        class SCPetMsgPetPieceSync : SCMsgBaseAck
        {
                public string info; //{id&idConfig&overlap|id&idConfig&overlap|}
                public SCPetMsgPetPieceSync()
                {
                        SetTag("sPetPieceSync");
                }
        }

        class SCPetMsgPetPiece : SCMsgBaseAck
        {
                public string info; //{id&idConfig&overlap};
                public SCPetMsgPetPiece()
                {
                        SetTag("sPetPiece");
                }
        }

        #endregion

        #region 英雄酒馆

        class CSPetMsgPubQuery : CSMsgBaseReq //请求酒馆信息
        {
                public CSPetMsgPubQuery()
                {
                        SetTag("cPubQuery");
                }
        }

        class CSPetMsgPubHire : CSMsgBaseReq
        {
                public int desk;                 //桌号
                public int idCharacter;          //英雄id
                public int index;               //位置号
                public CSPetMsgPubHire()
                {
                        SetTag("cPubHire");
                }
        }

        class SCPetMsgPubQuery : SCMsgBaseAck
        {
                public long time; //剩余刷新时间
                public SCPetMsgPubQuery()
                {
                        SetTag("sPubQuery");
                }
        }

        class SCPetMsgPubHire : SCMsgBaseAck
        {
                public int error_type;  //ERROR_CODE
                public SCPetMsgPubHire()
                {
                        SetTag("sPubHire");
                }
        }

        #endregion


        class CSMsgPetRelive : CSMsgBaseReq
        {
            public long build_id;
            public string petid;      //id|id
            public CSMsgPetRelive()
            {
                SetTag("cHouseBuildReliveHero");
            }
        }

        class CSMsgPetGetAchieve : CSMsgBaseReq //成就获取
        {
                public long petid;
                public int achieve_id;    
                public CSMsgPetGetAchieve()
                {
                        SetTag("cPetAchieveReward");
                }
        }


        class SCMsgPetOutTime : SCMsgBaseAck  //服务器告知英雄外出的时间
        {
                public long pet_id;
                public int time;    //剩余时间
                public SCMsgPetOutTime()
                {
                        SetTag("sPetOut");
                }
        }


        class CSMsgPetBack : CSMsgBaseReq  
        {
                public long pet_id;
                public CSMsgPetBack()
                {
                        SetTag("cPetBack");
                }
        }

#region 压力

        //sBuildCure idRealBuild idPet|idPet|idPet。。
        class CSMsgBuildCurePet : CSMsgBaseReq  
        {
                public long idRealBuild;
                public long pet_id;
                public CSMsgBuildCurePet()
                {
                        SetTag("cBuildCurePet");
                }
        }

        class SCMsgPetCureTime : SCMsgBaseAck  //服务器告知英雄治疗剩余时间
        {
                public long pet_id;
                public int time;         //剩余时间
                public SCMsgPetCureTime()
                {
                        SetTag("sPetCure");
                }
        }

        class CSMsgPetCureCompleted : CSMsgBaseReq   //告知服务器完成治疗
        {
                public long pet_id;
                public CSMsgPetCureCompleted()
                {
                        SetTag("cPetCureCompleted");
                }
        }

        class SCMsgPetCureCompleted : SCMsgBaseAck   
        {
                public long pet_id;
                public SCMsgPetCureCompleted()
                {
                        SetTag("sPetCureCompleted");
                }
        }


        class CSMsgCurePetCancel : CSMsgBaseReq   //取消治疗
        {
                public long pet_id;
                public CSMsgCurePetCancel()
                {
                        SetTag("cBuildCurePetCancel");
                }
        }

        class SCMsgBuildCurePetCancel : SCMsgBaseAck
        {
                public long pet_id;
                public SCMsgBuildCurePetCancel()
                {
                        SetTag("sBuildCurePetCancel");
                }
        }

#endregion 

 #region 特性

        class CSMsgBuildPurify : CSMsgBaseReq    //治疗特性
        {
                public long idBuild;
                public long pet_id;
                public int type;
                public int idBehaviour;
                public CSMsgBuildPurify()
                {
                        SetTag("cBuildPurifyPet");
                }
        }

        class CSMsgBuildPurifyPetCancel : CSMsgBaseReq    //治疗特性取消
        {
                public long idpet;
                public CSMsgBuildPurifyPetCancel()
                {
                        SetTag("cBuildPurifyPetCancel");
                }
        }

        class SCMsgPetBuildPurifyPetCancel : SCMsgBaseAck  //取消成功
        {
                public long pet_id;
                public SCMsgPetBuildPurifyPetCancel()
                {
                        SetTag("sBuildPurifyPetCancel");
                }
        }
        
        class SCMsgPetPurifyTime : SCMsgBaseAck  //服务器告知英雄治疗剩余时间
        {
                public long pet_id;
                public int time;         //剩余时间
                public SCMsgPetPurifyTime()
                {
                        SetTag("sPetPurify");
                }
        }

        class CSMsgPetPurifyCompleted : CSMsgBaseReq   //告知服务器完成治疗
        {
                public long pet_id;
                public CSMsgPetPurifyCompleted()
                {
                        SetTag("cBuildPurifyCompleted");
                }
        }

        class SCMsgPetPurifyCompleted : SCMsgBaseAck   
        {
                public long pet_id;
                public SCMsgPetPurifyCompleted()
                {
                        SetTag("sPetPurifyCompleted");
                }
        }


        class SCMsgBuildPurify : SCMsgBaseAck  //怪癖建筑上的信息
        {
                public long idBuild;
                public long pet_id;
                public int type;
                public int idBehaviour;
                public SCMsgBuildPurify()
                {
                        SetTag("sBuildPurify");
                }
        }

        
#endregion








}