using UnityEngine;
using System.Collections;
namespace Message
{
        /// <summary>
        /// 
        /// </summary>
        class SCMsgChat  : SCMsgBaseAck
        {
                public byte byChannel;
                public long idSender;
                public string senderName;
                public string strTitle;
                public string strText;

                public SCMsgChat()
                {
                        SetTag("sChat");
                }
        }

        /// <summary>
        /// 聊天
        /// </summary>
        class CSMsgChat : CSMsgBaseReq
        {
                public byte byChannel;
                public long idTarget;
                public string strTargetName;
                public string strText;
                
                public CSMsgChat()
                {
                        SetTag("cChat");
                }
        }
    

        class SCMsgNotProgramError : SCMsgBaseAck 
        {
                public string error_info;

                public SCMsgNotProgramError()
                {
                    SetTag("sNotProgramError");
                }
        }

        class SCMsgTips : SCMsgBaseAck
        {
                public int id;
                public SCMsgTips()
                {
                        SetTag("sTips");
                }
        }
}
