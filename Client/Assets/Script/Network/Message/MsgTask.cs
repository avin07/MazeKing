using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using System.Collections;
namespace Message
{
        class SCMsgTask : SCMsgBaseAck
        {
                public string strCurrentTask; //idTaskCfg:nSchedule1&nSchedule2&...|idTaskCfg:nSchedule1&nSchedule2&...|
                public string strFinishTask; //idTaskCfg1|idTaskCfg2|...

                public SCMsgTask()
                {
                        SetTag("sTask");
                }
        }

        class CSMsgTaskAdd : CSMsgBaseReq
        {
                public long idNpc;
                public int idTaskCfg;

                public CSMsgTaskAdd()
                {
                        SetTag("cTaskAdd");
                }
        }

        class SCMsgTaskAdd : SCMsgBaseAck
        {
                public int idTaskCfg;
                public SCMsgTaskAdd()
                {
                        SetTag("sTaskAdd");
                }
        }

        class SCMsgTaskSchedule : SCMsgBaseAck
        {
                public int idTaskCfg;
                public string strSchedule; //nSchedule1&nSchedule2&...
                public SCMsgTaskSchedule()
                {
                        SetTag("sTaskSchedule");
                }
        }

        class CSMsgTaskSubmit : CSMsgBaseReq
        {
                public long idNpc;
                public int idTaskCfg;
                public int nOpt;
                public CSMsgTaskSubmit()
                {
                        SetTag("cTaskSubmit");
                }
        }

        class SCMsgTaskSubmit : SCMsgBaseAck
        {
                public int idTaskCfg;
                public SCMsgTaskSubmit()
                {
                        SetTag("sTaskSubmit");
                }
        }


        class SCMsgTaskDel : SCMsgBaseAck  //当前仅有公告板任务刷新时才会有
        {
                public int idTaskCfg;
                public SCMsgTaskDel()
                {
                        SetTag("sTaskDel");
                }
        }


        class CSMsgTaskRefresh : CSMsgBaseReq //公告板刷新任务
        {
                public int idTaskCfg;
                public CSMsgTaskRefresh()
                {
                        SetTag("cTaskRefresh");
                }
        }
}