using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using System.Linq; 

public class UI_Task : UIBehaviour
{
        Transform mTask;
        Transform mItem;
        Transform mResItem;

        Transform right;
        Text des;                               //任务描述
        Transform empty;
        Sprite[] taskSprite = new Sprite[2];

        void Awake()
        {
                mTask = FindComponent<Transform>("bg/ScrollRect/content/task");

                right = FindComponent<Transform>("bg/right");
                mItem = FindComponent<Transform>(right,"itemlist/item");
                mResItem = FindComponent<Transform>(right, "reslist/item");
                des = FindComponent<Text>(right,"des");
                empty = FindComponent<Transform>("bg/empty");
                FindComponent<Button>("bg/close").onClick.AddListener(OnClose);
                taskSprite[0] = mTask.GetComponent<Image>().sprite;
                taskSprite[1] = FindComponent<Image>("bg/select").sprite;
        }

        void OnClose()
        {
                UIManager.GetInst().CloseUI(this.name);
                UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", true);
                TaskManager.GetInst().ClearNewTaskList();
        }

        public void Refresh()
        {
                Dictionary<int, string> mTaskDic = TaskManager.GetInst().GetAllTaskDic();
                RefreshLeft(mTaskDic);
                if (mTaskDic.Count > 0)
                {
                        right.SetActive(true);
                        empty.SetActive(false);
                        ClickTask(mTaskDic.Keys.First(),0); //默认打开第一个任务
                }
                else
                {
                        right.SetActive(false);
                        empty.SetActive(true);
                }
        }


        void RefreshLeft(Dictionary<int, string> mTaskDic)
        {
                SetChildActive(mTask.parent, false);
                int i = 0;
                Transform taskTf;
                TaskConfig tc;
                foreach (int id in mTaskDic.Keys)
                {
                        tc = TaskManager.GetInst().GetTaskCfg(id);
                        taskTf = GetChildByIndex(mTask.parent, i);
                        if (taskTf == null)
                        {
                                taskTf = CloneElement(mTask.gameObject).transform;
                        }
                        taskTf.SetActive(true);
                        FindComponent<Text>(taskTf, "name").text = LanguageManager.GetText(tc.name);
                        int taskId = id;
                        int index = i;
                        taskTf.GetComponent<Button>().onClick.AddListener(() => ClickTask(taskId,index));
                        GetGameObject(taskTf.gameObject, "newlabel").SetActive(TaskManager.GetInst().IsNewTask(id));
                        i++;
                }
        }


        void RefreshRight(int taskId)
        {
                TaskConfig tc = TaskManager.GetInst().GetTaskCfg(taskId);
                des.text = LanguageManager.GetText(tc.description);

                List<string> scheduleDes = TaskManager.GetInst().GetTaskScheduleDes(taskId);
                for (int i = 0; i < 2; i++)
                {
                        FindComponent<Text>(right, "target" + i).text = "";
                }
                for (int i = 0; i < scheduleDes.Count; i++)
                {
                        FindComponent<Text>(right, "target" + i).text = scheduleDes[i];
                }
                RefreshReward(tc);
        }

        void ClickTask(int taskId,int index)
        {
                SetBtnHighlight(index);
                RefreshRight(taskId);
        }

        void SetBtnHighlight(int index)
        {
                for (int i = 0; i < mTask.parent.childCount; i++)
                {
                        if (i == index)
                        {
                                mTask.parent.GetChild(i).GetComponent<Image>().sprite = taskSprite[1];
                        }
                        else
                        {
                                mTask.parent.GetChild(i).GetComponent<Image>().sprite = taskSprite[0];
                        }
                }
        }

        void RefreshReward(TaskConfig tc)
        {

                RefreshResReward(tc.resource_reward);

                if (!tc.regular_reward.Equals("0")) //固定奖励
                {
                        FindComponent<Transform>(right, "tip").SetActive(false);
                        RefreshReward(tc.regular_reward);
                }
                else
                {
                        FindComponent<Transform>(right, "tip").SetActive(true);
                        RefreshReward(tc.selectable_reward);
                }

                if (!tc.area_goal_point_reward.Equals("0"))  //存在dp值奖励
                {
                        string[] temp = tc.area_goal_point_reward.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        WorldMapAreaHold cfg = WorldMapManager.GetInst().GetWorldMapAreaCfg(int.Parse(temp[0]));
                        FindComponent<Text>(right, "dptip").text = "[" + LanguageManager.GetText(cfg.area_name) + "]" + " 区域探索度 " + int.Parse(temp[1]);
                }
                else
                {
                        FindComponent<Text>(right, "dptip").text = string.Empty;
                }

                //资源类奖励//
        }


        void RefreshReward(string reward)
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


        void RefreshResReward(string reward)
        {
                if (!GameUtility.IsStringValid(reward))
                {
                        mResItem.parent.SetActive(false);
                        return;
                }

                mResItem.parent.SetActive(true);

                string[] rewardStr = reward.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                SetChildActive(mResItem.parent, false);

                int num;
                string name;
                int id;
                string des;
                Thing_Type type;
                Transform item;

                for (int i = 0; i < rewardStr.Length; i++)
                {
                        item = GetChildByIndex(mResItem.parent, i);
                        if (item == null)
                        {
                                item = CloneElement(mResItem.gameObject).transform;
                        }
                        item.SetActive(true);

                        Image icon_image = FindComponent<Image>(item, "icon");
                        Text num_down = FindComponent<Text>(item, "num");

                        CommonDataManager.GetInst().SetThingIcon(rewardStr[i], icon_image.transform, null, out name, out num, out id, out des, out type);
                        num_down.text = num.ToString();
                }
        }

}


