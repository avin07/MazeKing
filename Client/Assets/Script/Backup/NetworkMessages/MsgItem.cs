using UnityEngine;
using System.Collections;
namespace Message
{
        //背包物品更新
        public class SCMsgItem : SCMsgBaseAck
        {
                public long id;
                public int nType;
                public int idCfg;
                public int nPlace;
                public int nPos;
                public int nOverlap;
                public int nCreateTime;
                public SCMsgItem()
                {
                        SetTag("sItem");
                }
        }


        class SCMsgItemUpdate : SCMsgBaseAck
        {
                public long id;
                public int nOverlap;
                public int nPlace;
                public int nPos;
                public SCMsgItemUpdate()
                {
                        SetTag("sItemUpdate");
                }
        }

        class SCMsgItemDel : SCMsgBaseAck
        {
                public long id;
                public int nPlace;
                public SCMsgItemDel()
                {
                        SetTag("sItemDel");
                }
        }


        class CSMsgItemUse : CSMsgBaseReq
        {
                public long idItem;
                public long idTarget;
                public int nCount;
                public CSMsgItemUse()
                {
                        SetTag("cItemUse");
                }
        }

}