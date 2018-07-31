using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Message;
using UnityEngine.UI;

class TaskManager : SingletonObject<TaskManager>
{        
	public void Init() 
        {
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidBranchTask), OnRaidTaskBranch);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidTaskCount), OnRaidTaskCount);

                InitTask();
	}
        #region 迷宫任务
        void OnRaidTaskCount(object sender, SCNetMsgEventArgs e)
        {
                SCMsgRaidTaskCount msg = e.mNetMsg as SCMsgRaidTaskCount;
                RaidManager.GetInst().UpdateTaskCount(msg.taskCount);
        }

        void OnRaidTaskBranch(object sender, SCNetMsgEventArgs e)
        {
//                SCMsgRaidBranchTask msg = e.mNetMsg as SCMsgRaidBranchTask;
                
//                 RAID_TASK_INFO taskInfo = GetTaskInfo(msg.taskId);
//                 taskInfo.state = msg.nState;
//                 taskInfo.countstr = msg.countStr;
// 
//                 //RaidManager.GetInst().UpdateBranchTask(taskInfo);

//                 UI_RaidTask uis = UIManager.GetInst().GetUIBehaviour<UI_RaidTask>();
//                 if (uis != null)
//                 {
//                         uis.UpdateTaskCount();
//                 }
        }
        Dictionary<int, RAID_TASK_INFO> m_RaidBranchDict = new Dictionary<int, RAID_TASK_INFO>();
        RAID_TASK_INFO GetTaskInfo(int taskId)
        {
                if (!m_RaidBranchDict.ContainsKey(taskId))
                {
                        m_RaidBranchDict.Add(taskId, new RAID_TASK_INFO(taskId));
                }
                return m_RaidBranchDict[taskId];
        }
        public RAID_TASK_INFO GetBranchTask(int taskId)
        {
                if (m_RaidBranchDict.ContainsKey(taskId))
                {
                        return m_RaidBranchDict[taskId];
                }
                return null;
        }

        public string GetCurrRaidTaskReward()
        {
                RaidTaskConfig taskCfg = RaidConfigManager.GetInst().GetRaidMainTask();
                if (taskCfg != null)
                {
                        return taskCfg.reward_item_list;
                }
                return "";
        }
        public void SendTaskSubmit(int taskId)
        {
                CSMsgRaidTaskSubmit msg = new CSMsgRaidTaskSubmit();
                msg.taskId = taskId;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

        #endregion

        #region 新任务系统

        Dictionary<int, string> currentNpcTaskDic = new Dictionary<int, string>();    //已接取的任务（不包含公告板任务）
        HashSet<int> finishTaskSet = new HashSet<int>();                              //已完成的任务(没有公告板任务)
        Dictionary<int, string> currentBoardTaskDic = new Dictionary<int, string>();  //已接取的公告板任务

        Dictionary<int, TaskConfig> m_TaskConfig = new Dictionary<int, TaskConfig>();
        Dictionary<int, TaskPoolRefreshConfig> m_TaskPoolRefreshConfig = new Dictionary<int, TaskPoolRefreshConfig>();

        void InitTask()
        {
                ConfigHoldUtility<TaskConfig>.LoadXml("Config/task", m_TaskConfig);
                ConfigHoldUtility<TaskPoolRefreshConfig>.LoadXml("Config/task_pool_refresh", m_TaskPoolRefreshConfig);  
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgTask), OnTask);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgTaskAdd), OnTaskAdd);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgTaskSchedule), OnTaskSchedule);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgTaskSubmit), OnTaskSubmit);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgTaskDel), OnTaskDel);
        }

        public Dictionary<int, string> GetAllTaskDic()
        {
                Dictionary<int, string> allTaskDic = new Dictionary<int, string>(currentNpcTaskDic);
                foreach (int id in currentBoardTaskDic.Keys)
                {
                        allTaskDic.Add(id, currentBoardTaskDic[id]);
                }
                return allTaskDic;
        }

        public Dictionary<int, string> GetNpcTaskDic()
        {
                return currentNpcTaskDic;
        }

        public Dictionary<int, string> GetBoardTaskDic()
        {
                return currentBoardTaskDic;
        }

        public string GetTaskSchedule(int id)
        {
                if(currentNpcTaskDic.ContainsKey(id))
                {
                        return currentNpcTaskDic[id];
                }
                if (currentBoardTaskDic.ContainsKey(id))
                {
                        return currentBoardTaskDic[id];
                }
                return String.Empty;
        }

        public TaskConfig GetTaskCfg(int id)
        {
                if (m_TaskConfig.ContainsKey(id))
                {
                        return m_TaskConfig[id];
                }
                return null;
        }

        public int GetPoolRefreshCfg(int id)
        {
                if (id > m_TaskPoolRefreshConfig.Count)
                {
                        return m_TaskPoolRefreshConfig[m_TaskPoolRefreshConfig.Count].refresh_price;
                }
                else
                {
                        return m_TaskPoolRefreshConfig[id].refresh_price;
                }
        }

        public List<string> GetTaskScheduleDes(int taskId)
        {
                TaskConfig tc = TaskManager.GetInst().GetTaskCfg(taskId);
                string schedule = GetTaskSchedule(taskId);
                List<string> ScheduleDesList = new List<string>();
                if (tc == null || schedule.Length == 0)
                {
                        return ScheduleDesList;
                }

                string[] finishNum = schedule.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                string[] needNum = tc.target_num.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string[] progressTip = tc.progress_notice.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string des;

                if (needNum.Length == finishNum.Length)
                {
                        for (int i = 0; i < progressTip.Length; i++)
                        {
                                if (i < finishNum.Length)
                                {
                                        if (int.Parse(finishNum[i]) < int.Parse(needNum[i])) //未达成
                                        {
                                                des = String.Format(LanguageManager.GetText(progressTip[i]), "(" + "<color=red>" + finishNum[i] + "</color>/" + needNum[i] + ")");
                                        }
                                        else
                                        {
                                                des = String.Format(LanguageManager.GetText(progressTip[i]), "(完成)");
                                        }
                                        ScheduleDesList.Add(des);
                                }
                        }
                }
                else
                {
                        singleton.GetInst().ShowMessage(ErrorOwner.designer, "任务" + taskId + "条件数目服务器和客户端配置不一致！");
                }
                return ScheduleDesList;
        }

        public bool GetTaskState(int taskId)  //true 为已经达成
        {
                TaskConfig tc = GetTaskCfg(taskId);
                if(tc != null)
                {
                        string schedule = GetTaskSchedule(taskId);
                        string taskCondition = tc.target_num;
                        string[] needNum = taskCondition.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] finishNum = schedule.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);  //nSchedule1&nSchedule2&

                        if (needNum.Length == finishNum.Length)
                        {
                                for (int i = 0; i < needNum.Length; i++)
                                {
                                        if (int.Parse(finishNum[i]) < int.Parse(needNum[i]))
                                        {
                                                return false;
                                        }
                                }
                                return true;
                        }
                }
                return false;
        }

        public string GetTaskAcceptDialog(int taskId)
        {
                TaskConfig tc = GetTaskCfg(taskId);
                if (tc != null)
                {
                        return tc.accept_dialog;
                }
                return String.Empty;
        }

        public string GetTaskSubmitDialog(int taskId)
        {
                TaskConfig tc = GetTaskCfg(taskId);
                if (tc != null)
                {
                        return tc.submit_dialog;
                }
                return String.Empty;
        }

        public void ClickTaskNpcFeedback(int npcCfgId,long npcId)  //点击任务npc的反馈//
        {
                int taskId = 0;
                int taskNpcState = CheckTaskNpcState(npcCfgId, ref taskId);

                if (taskNpcState == 0)
                {
                        return;
                }

                if (GameStateManager.GetInst().GameState == GAMESTATE.HOME)  //除0之外的所有情况在家园中都显示近景
                {
                        HomeManager.GetInst().StopCameraYAutoTweener();
                        UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", false);
                        HomeManager.GetInst().SaveCameraData();
                        HomeManager.GetInst().ChangeCameraForNpc(npcId);
                }

                if (taskNpcState > 0 && taskNpcState < 5)
                {
                        UIManager.GetInst().ShowUI<UI_Dialog>("UI_Dialog").RefreshTask(taskNpcState, taskId, npcId);
                }

                if (taskNpcState == 5)
                {
                        if (GameStateManager.GetInst().GameState == GAMESTATE.HOME || GameStateManager.GetInst().IsGameInRaid())
                        {
                                NpcConfig nc = NpcManager.GetInst().GetNpcCfg(npcCfgId);
                                UIManager.GetInst().ShowUI<UI_Dialog>("UI_Dialog").RefreshTask(nc.common_dialog,npcId);
                        }
                }             
        }

        public void ClickMapTask(int mapTaskId)  //点击任务npc的反馈//
        {
                int taskId = 0;
                int taskState = CheckTaskMapState(mapTaskId, ref taskId);

                if (taskState == 0)
                {
                        return;
                }

                if (taskState > 0 && taskState < 5)
                {
                        UIManager.GetInst().ShowUI<UI_Dialog>("UI_Dialog").RefreshTask(taskState, taskId, 0);
                }

                if (taskState == 5)
                {
                        WorldmapTaskConfig wtc = WorldMapManager.GetInst().GetWorldMapTaskCfg(mapTaskId);
                        UIManager.GetInst().ShowUI<UI_Dialog>("UI_Dialog").RefreshTask(wtc.common_dialog,0);                       
                }
        }

        public void SetTaskNpcIcon(int npcCfgId,GameObject Icon)
        {
                int taskId = 0;
                int taskNpcState = CheckTaskNpcState(npcCfgId, ref taskId);
                UI_NpcIcon uni = Icon.GetComponent<UI_NpcIcon>();
                switch (taskNpcState)
                {
                        case 0: //npc配置不存在//
                                break;
                        case 1: //有任务可以交付//
                                uni.SetIcon("Npc#wenhao");
                                break;
                        case 2: //有任务可以接取//
                                uni.SetIcon("Npc#tanhao");
                                break;
                        case 3: //有任务已接取但没完成//
                                uni.SetIcon("Npc#wenhao",true);
                                break;
                        case 4: //有任务可以接但未达到条件//
                                uni.SetIcon("Npc#tanhao",true);
                                break;
                        case 5: //以上情况都不存在//
                                uni.SetIcon("Npc#tanhao", true);
                                break;
                }
        }


        public void SetMapNpcIcon(int mapId,Image flag)
        {
                int taskId = 0;
                int taskNpcState = CheckTaskMapState(mapId, ref taskId);
                flag.enabled = true;
                switch (taskNpcState)
                {
                        case 0: //npc配置不存在//
                                flag.enabled = false;
                                break;
                        case 1: //有任务可以交付//
                                ResourceManager.GetInst().LoadIconSpriteSyn("Raid#wenhao", flag);
                                break;
                        case 2: //有任务可以接取//
                                ResourceManager.GetInst().LoadIconSpriteSyn("Raid#tanhao", flag);
                                break;
                        case 3: //有任务已接取但没完成//
                                ResourceManager.GetInst().LoadIconSpriteSyn("Raid#wenhao", flag);
                                UIUtility.SetImageGray(true, flag.transform);
                                break;
                        case 4: //有任务可以接但未达到条件//
                                ResourceManager.GetInst().LoadIconSpriteSyn("Raid#tanhao", flag);
                                UIUtility.SetImageGray(true, flag.transform);
                                break;
                        case 5: //以上情况都不存在//
                                flag.enabled = false;
                                break;
                }
        }


        public int CheckTaskNpcState(int npcCfgId, ref int taskId)  
        {
                NpcConfig nc = NpcManager.GetInst().GetNpcCfg(npcCfgId);
                if (nc == null)
                {
                        return 0;
                }

                List<int> issueTaskList = nc.issue_task_id.ToList<int>(',', (s) => int.Parse(s));   //可发布任务
                List<int> submitTaskList = nc.submit_task_id.ToList<int>(',', (s) => int.Parse(s)); //可提交任务
                return CheckTaskState(issueTaskList, submitTaskList, ref taskId);
        }

        public int CheckTaskMapState(int mapTaskId, ref int taskId)
        {
                WorldmapTaskConfig wtc = WorldMapManager.GetInst().GetWorldMapTaskCfg(mapTaskId);
                if (wtc == null)
                {
                        return 0;
                }
                List<int> issueTaskList = wtc.issue_task_id.ToList<int>(',', (s) => int.Parse(s));   //可发布任务
                List<int> submitTaskList = wtc.submit_task_id.ToList<int>(',', (s) => int.Parse(s)); //可提交任务
                return CheckTaskState(issueTaskList, submitTaskList, ref taskId);
        }

        int CheckTaskState(List<int> issueTaskList, List<int> submitTaskList , ref int taskId)
        {

                //1判断是否有任务可以交付//
                TaskConfig tc;
                for (int i = 0; i < submitTaskList.Count; i++)
                {
                        taskId = submitTaskList[i];
                        if (currentNpcTaskDic.ContainsKey(taskId))
                        {
                                if (GetTaskState(taskId)) //可交付
                                {
                                        tc = GetTaskCfg(taskId);
                                        if (tc != null)
                                        {
                                                return 1;
                                        }
                                }
                        }
                }

                //2判断是否有可接取任务//
                for (int i = 0; i < issueTaskList.Count; i++)
                {
                        taskId = issueTaskList[i];
                        if (!finishTaskSet.Contains(taskId) && !currentNpcTaskDic.ContainsKey(taskId))  //该任务没被完成且没有被接取
                        {
                                tc = GetTaskCfg(taskId);
                                if (tc != null)
                                {
                                        if (tc.pre_task != 0 && finishTaskSet.Contains(tc.pre_task) || tc.pre_task == 0) //前置任务完成
                                        {
                                                if (PlayerController.GetInst().GetPropertyInt("house_level") >= tc.home_level_limit) //家园等级符合
                                                {
                                                        return 2;
                                                }
                                        }
                                }
                        }
                }

                //3如果有任务没完成
                for (int i = 0; i < submitTaskList.Count; i++)
                {
                        taskId = submitTaskList[i];
                        if (currentNpcTaskDic.ContainsKey(taskId))
                        {
                                if (!GetTaskState(taskId)) //不可交付
                                {
                                        tc = GetTaskCfg(taskId);
                                        if (tc != null)
                                        {
                                                return 3;
                                        }
                                }
                        }
                }

                //4如果有任务可以接取
                for (int i = 0; i < issueTaskList.Count; i++)
                {
                        taskId = issueTaskList[i];
                        if (!finishTaskSet.Contains(taskId) && !currentNpcTaskDic.ContainsKey(taskId))  //该任务没被完成
                        {
                                tc = GetTaskCfg(taskId);
                                if (tc != null)
                                {
                                        return 4;
                                }
                        }
                }
                return 5;
        }

        void OnTask(object sender, SCNetMsgEventArgs e)
        {
                SCMsgTask msg = e.mNetMsg as SCMsgTask;
                string strCurrentTask = msg.strCurrentTask; //idTaskCfg:nSchedule1&nSchedule2&...|idTaskCfg:nSchedule1&nSchedule2&...|

                if(GameUtility.IsStringValid(strCurrentTask,":"))
                {
                        string[] oneTaskInfo = strCurrentTask.Split(new char []{'|'}, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < oneTaskInfo.Length; i++)
                        {
                                string[] info = oneTaskInfo[i].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                int taskId = int.Parse(info[0]);
                                TaskConfig tc = GetTaskCfg(taskId);
                                if (tc != null)
                                {
                                        if (tc.type == 2) //公告板任务
                                        {
                                                currentBoardTaskDic.Add(taskId, info[1]);
                                        }
                                        else  //npc任务和地图任务
                                        {
                                                currentNpcTaskDic.Add(taskId, info[1]); //保存接取的任务和进度
                                        }
                                        if (tc.target_type == 0) //接到就完成//
                                        {
                                                currentNpcTaskDic[taskId] = "1";
                                        }
                                }
                                else
                                {
                                        Debug.LogError("Task " + taskId + " is not exist in config");
                                }
                        }
                }

                finishTaskSet = msg.strFinishTask.ToHashSet<int>('|', (s) => int.Parse(s));

                if (GameStateManager.GetInst().GameState == GAMESTATE.RAID_PLAYING)
                {
                        RaidManager.GetInst().UpdateTasks();
                }
        }

        void OnTaskAdd(object sender, SCNetMsgEventArgs e)
        {
                SCMsgTaskAdd msg = e.mNetMsg as SCMsgTaskAdd;
                TaskConfig tc = GetTaskCfg(msg.idTaskCfg);
                if (tc == null)
                {
                        return;
                }

                if (tc.type == 2) //公告板任务
                {
                        if (!currentBoardTaskDic.ContainsKey(msg.idTaskCfg))
                        {
                                currentBoardTaskDic.Add(msg.idTaskCfg, String.Empty);
                                AddToNewTaskList(msg.idTaskCfg);
                                //刷新公告版
                        }
                        else
                        {
                                singleton.GetInst().ShowMessage(ErrorOwner.exception, "重复添加任务" + msg.idTaskCfg);
                        }
                        
                }
                else  //npc任务 和 地图任务
                {
                        if (!currentNpcTaskDic.ContainsKey(msg.idTaskCfg))
                        {
                                currentNpcTaskDic.Add(msg.idTaskCfg, String.Empty);
                                AddToNewTaskList(msg.idTaskCfg);
                        }
                        else
                        {
                                singleton.GetInst().ShowMessage(ErrorOwner.exception, "重复添加任务" + msg.idTaskCfg);
                        }
                }

                if (tc.target_type == 0) //接到就完成//
                {
                        currentNpcTaskDic[msg.idTaskCfg] = "1";
                        if (GameStateManager.GetInst().GameState == GAMESTATE.HOME)
                        {
                                HomeManager.GetInst().CheckAllTaskNpcIcon();
                        }
                }
        }

        public bool CheckHasBoardTaskFinish()
        {
                foreach (int id in currentBoardTaskDic.Keys)
                {
                        if (GetTaskState(id))
                        {
                                return true;
                        }
                }
                return false;
        }

        void OnTaskSchedule(object sender, SCNetMsgEventArgs e)
        {
                SCMsgTaskSchedule msg = e.mNetMsg as SCMsgTaskSchedule;
                if (currentNpcTaskDic.ContainsKey(msg.idTaskCfg))
                {
                        currentNpcTaskDic[msg.idTaskCfg] = msg.strSchedule;
                        if (GameStateManager.GetInst().GameState == GAMESTATE.HOME)
                        {
                                if (WorldMapManager.GetInst().IsShow) //更新图标
                                {
                                        WorldMapManager.GetInst().RefreshRaid();
                                        WorldMapManager.GetInst().FuckNewAnimation();
                                }
                                else
                                {
                                        HomeManager.GetInst().CheckAllTaskNpcIcon();
                                }
                        }
                }
                else if (currentBoardTaskDic.ContainsKey(msg.idTaskCfg))
                {
                        currentBoardTaskDic[msg.idTaskCfg] = msg.strSchedule;
                        UI_TaskBoard utb = UIManager.GetInst().GetUIBehaviour<UI_TaskBoard>();
                        if (utb != null)
                        {
                                utb.RefreshTask();
                        }
                        HomeManager.GetInst().CheckBoardBar();
                }
                else
                {
                        singleton.GetInst().ShowMessage(ErrorOwner.gametest, "当前不存在任务" + msg.idTaskCfg + "为何下发进度");
                }

                if (GameStateManager.GetInst().GameState == GAMESTATE.RAID_PLAYING)
                {
                        RaidManager.GetInst().UpdateBranchTask(msg.idTaskCfg);
                }
        }

        void OnTaskSubmit(object sender, SCNetMsgEventArgs e)
        {
                SCMsgTaskSubmit msg = e.mNetMsg as SCMsgTaskSubmit;
                if (currentNpcTaskDic.ContainsKey(msg.idTaskCfg))
                {
                        currentNpcTaskDic.Remove(msg.idTaskCfg);
                        finishTaskSet.Add(msg.idTaskCfg);

                        if (GameStateManager.GetInst().GameState == GAMESTATE.HOME)
                        {
                                if (WorldMapManager.GetInst().IsShow) //更新图标
                                {
                                        WorldMapManager.GetInst().RefreshRaid();
                                        WorldMapManager.GetInst().FuckNewAnimation();
                                }
                                else
                                {
                                        HomeManager.GetInst().CheckAllTaskNpcIcon();
                                }
                        }
                }

                if (currentBoardTaskDic.ContainsKey(msg.idTaskCfg))
                {
                        currentBoardTaskDic.Remove(msg.idTaskCfg);
                        UI_TaskBoard utb = UIManager.GetInst().GetUIBehaviour<UI_TaskBoard>();
                        if (utb != null)
                        {
                                utb.RefreshTask();
                        }
                        HomeManager.GetInst().CheckBoardBar();
                }
        }

        void OnTaskDel(object sender, SCNetMsgEventArgs e) //公告板任务被删除
        {
                SCMsgTaskDel msg = e.mNetMsg as SCMsgTaskDel;
                if (currentBoardTaskDic.ContainsKey(msg.idTaskCfg))
                {
                        currentBoardTaskDic.Remove(msg.idTaskCfg);
                        //npc任务刷新成功
                }
        }

        public void SendTaskAccept(int taskId, long npcId) //接取任务
        {
                CSMsgTaskAdd msg = new CSMsgTaskAdd();
                msg.idNpc = npcId;
                msg.idTaskCfg = taskId;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

        public void SendTaskSubmit(int taskId, long npcId,int nOpt) //提交任务
        {
                CSMsgTaskSubmit msg = new CSMsgTaskSubmit();
                msg.idNpc = npcId;
                msg.idTaskCfg = taskId;
                msg.nOpt = nOpt;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

        public void SendTaskSubmit(int nIndex, object para)
        {
                string []temp = ((string)para).Split('|');
                SendTaskSubmit(int.Parse(temp[0]), long.Parse(temp[1]), nIndex);
        }

        public void SendTaskRefresh(int taskId) //刷新公告板任务
        {
                CSMsgTaskRefresh msg = new CSMsgTaskRefresh();
                msg.idTaskCfg = taskId;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

        int m_nNewTaskCount = 0;
        public int NewTaskCount
        {
                get
                {
                        return m_nNewTaskCount;
                }
                set
                {
                        m_nNewTaskCount = value;
                        if (UIManager.GetInst().GetUIBehaviour<UI_HomeMain>() != null)
                        {
                                UIManager.GetInst().GetUIBehaviour<UI_HomeMain>().RefreshNewDarwNum("taskbtn", NewTaskCount);
                        }
                }
        }

        List<long> m_NewTaskList = new List<long>();
        public bool IsNewTask(long id)
        {
                return m_NewTaskList.Contains(id);
        }
        public void ClearNewTaskList()
        {
                m_NewTaskList.Clear();
                NewTaskCount = 0;
        }
        public void AddToNewTaskList(long id)
        {
                if (!m_NewTaskList.Contains(id))
                {
                        NewTaskCount++;
                        m_NewTaskList.Add(id);
                }
        }

        #endregion
}

public class RAID_TASK_INFO
{
        public int taskId;
        public int state;
        public string countstr;

        public RAID_TASK_INFO(int id)
        {
                taskId = id;
        }
}