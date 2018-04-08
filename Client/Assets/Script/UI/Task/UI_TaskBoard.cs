using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

public class UI_TaskBoard : UIBehaviour  //公告板
{

        Transform mTask;

        void Awake()
        {
                IsFullScreen = true;
                mTask = transform.Find("ScrollRect/content/task");
                FindComponent<Button>("close").onClick.AddListener(OnClose);
        }

        public void Refresh()
        {               
                UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", false);
                RefreshTask();
                RefreshTip();
                RefreshTimes();
        }

        void RefreshTip()
        {
                FindComponent<Text>(transform, "bg/tip1").text = LanguageManager.GetText("task_pool_refresh_tips_1");
                RefreshTimes();
        }

        public void RefreshTimes()
        {
                FindComponent<Text>(transform, "bg/tip2").text = string.Format(LanguageManager.GetText("task_pool_refresh_tips_2"), PlayerController.GetInst().GetPropertyString("day_task_refresh_times"));
        }

        public void RefreshTask()
        {
                Dictionary<int, string> mTaskDic = TaskManager.GetInst().GetBoardTaskDic();
                SetChildActive(mTask.parent,false);
                Transform taskTf;
                TaskConfig tc;
                int index = 0;
                foreach (int id in mTaskDic.Keys)
                {
                        int taskId = id;
                        tc = TaskManager.GetInst().GetTaskCfg(taskId);
                        taskTf = GetChildByIndex(mTask.parent, index);
                        if (taskTf == null)
                        {
                                taskTf = CloneElement(mTask.gameObject).transform;
                        }
                        taskTf.SetActive(true);
                        FindComponent<Text>(taskTf, "name").text = LanguageManager.GetText(tc.name);
                        FindComponent<Text>(taskTf, "des").text = LanguageManager.GetText(tc.description);
                        ResourceManager.GetInst().LoadIconSpriteSyn(tc.head_icon, taskTf.Find("icon"));
                        List<string> scheduleDes = TaskManager.GetInst().GetTaskScheduleDes(taskId);
                        string progress = string.Empty;
                        for (int i = 0; i < scheduleDes.Count; i++)
                        {
                                progress += scheduleDes[i] + "  ";
                        }
                        FindComponent<Text>(taskTf, "progress").text = progress;

                        Transform mItem = taskTf.Find("itemlist/item");
                        RefreshReward(tc, mItem);

                        bool bFinish = TaskManager.GetInst().GetTaskState(taskId);
                        Button getBtn = FindComponent<Button>(taskTf, "getbtn");
                        Button refreshBtn = FindComponent<Button>(taskTf, "refreshbtn");

                        if (bFinish) //可领取
                        {
                                getBtn.gameObject.SetActive(true);
                                refreshBtn.gameObject.SetActive(false);
                                getBtn.onClick.AddListener(() => OnTaskSubmit(taskId));
                                refreshBtn.onClick.RemoveAllListeners();
                        }
                        else
                        {
                                getBtn.gameObject.SetActive(false);
                                refreshBtn.gameObject.SetActive(true);
                                getBtn.onClick.RemoveAllListeners();
                                refreshBtn.onClick.AddListener(() => OnTaskRefresh(taskId));
                        }
                        index++;
                }
        }


        void OnTaskRefresh(int taskId)
        {
                int refreshTimes = PlayerController.GetInst().GetPropertyInt("day_task_refresh_times");
                int cost = TaskManager.GetInst().GetPoolRefreshCfg(refreshTimes + 1);

                UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox").SetConfirmAndCancel("提示", "是否花费" + cost + "钻石更换该任务", ConfirmRefresh, null, taskId + CommonString.pipeStr + cost);               
        }


        void ConfirmRefresh(object data)
        {
                string[] temp = data.ToString().Split('|');
                if (CommonDataManager.GetInst().CheckIsThingEnough("1,2," + temp[1])) //钻石足够
                {
                        TaskManager.GetInst().SendTaskRefresh(int.Parse(temp[0]));
                }
                else
                {
                        GameUtility.PopupMessage("钻石不足！");
                }
        }

        void OnTaskSubmit(int taskId)  //任务提交
        {
                TaskConfig tc = TaskManager.GetInst().GetTaskCfg(taskId);
                UIManager.GetInst().ShowUI<UI_GetReward>("UI_GetReward").SetReward(tc.regular_reward, 0, TaskManager.GetInst().SendTaskSubmit, taskId + "|0");
        }


        void RefreshReward(TaskConfig tc, Transform mItem)
        {
                if (!tc.regular_reward.Equals(CommonString.zeroStr)) //固定奖励
                {
                        RefreshReward(tc.regular_reward,mItem);
                }
        }

        void RefreshReward(string reward, Transform mItem)
        {
                string[] rewardStr = reward.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                SetChildActive(mItem.parent, false);

                int num;
                string name;
                int id;
                string des;
                Thing_Type type;
                Transform item;

                for (int i = 0; i < rewardStr.Length; i++)
                {
                        item = GetChildByIndex(mItem.parent, i);
                        if (item == null)
                        {
                                item = CloneElement(mItem.gameObject).transform;
                        }
                        item.SetActive(true);

                        Image quality_image = FindComponent<Image>(item, "quality");
                        Image icon_image = FindComponent<Image>(item, "icon");
                        Text num_down = FindComponent<Text>(item, "num_down");

                        CommonDataManager.GetInst().SetThingIcon(rewardStr[i], icon_image.transform, quality_image.transform, out name, out num, out id, out des, out type);
                        num_down.text = num.ToString();
                }
        }

        public void OnClose()
        {
                UIManager.GetInst().CloseUI(this.name);
                UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", true);
        }
}
