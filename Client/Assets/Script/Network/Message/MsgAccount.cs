using UnityEngine;
using System.Collections;
namespace Message
{
        public enum ERROR_CODE
        {
                SUCCESS = 0,     // 成功
                NO_ACCOUNT = 1,  // 没有对应账号
                NAME_CLASH = 2,  // 名字冲突
                PASSWORD_ERR = 3,     //密码错误
        };



                
        
        class CSMsgAccountLogin : CSMsgBaseReq
        {
                public string sAccount;
                public string sName;
                public CSMsgAccountLogin()
                {
                        SetTag("cAccount");
                }
        }
        class SCMsgAccountLogin : SCMsgBaseAck
        {
                public int errorCode;
                public long idActor;
                public SCMsgAccountLogin()
                {
                        SetTag("sAccount");
                }
        }

        class CSMsgLoadUser : CSMsgBaseReq
        {
                public long idActor;
                public CSMsgLoadUser()
                {
                        SetTag("cLoadUser");
                }
        }
        class SCMsgLoadUser : SCMsgBaseAck
        {
                public string info;
                public SCMsgLoadUser()
                {
                        SetTag("sLoadUser");
                }
        }

        class SCMsgRandomSeed : SCMsgBaseAck
        {
                public int seed;
                public SCMsgRandomSeed()
                {
                        SetTag("sTestRandomSeed");
                }
        }

        class CSNetMsgMBT : CSMsgBaseReq
        {
                public CSNetMsgMBT()
                {
                        SetTag("cHT");
                }
        }

        class CSMsgGuide : CSMsgBaseReq
        {
                public int guideId;
                public string raidguide;
                public CSMsgGuide()
                {
                        SetTag("cGuide");
                }
        }

}