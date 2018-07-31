using UnityEngine;
using System.Collections;
namespace Message
{
        class SCMsgEquip : SCMsgBaseAck
        {
                //id&idOwner&nEquipType&byPlace&nBagPos&unCreateTime}; // idOwner:所属者ID nEquipType:装备配置id；byPlace：位置；nBagPos：背包坐标；unCreateTime：创建
                public string info;

                public SCMsgEquip()
                {
                    SetTag("sEquip");
                }
        }
        class SCMsgEquipDel : SCMsgBaseAck
        {
                public long idEquip;
                public long idOwner;
                public SCMsgEquipDel()
                {
                    SetTag("sEquipDel");
                }
        }

        class CSMsgEquipMount : CSMsgBaseReq
        {
            //{idPet} {idEquip} {byPlace}
            public long idPet;
            public long idEquip;
            public int byPlace;

            public CSMsgEquipMount()
            {
                SetTag("cEquipMount");
            }
        }

        class CSMsgEquipUnmount : CSMsgBaseReq
        {
            public long idPet;
            public long idEquip;
            public CSMsgEquipUnmount()
            {
                SetTag("cEquipUnmount");
            }
        }

}