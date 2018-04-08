using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UI_Dialog : UIBehaviour
{
        Transform chooseGroup;
        int nowDialogIndex;
        List<int> initDialogList = new List<int>();  //初始对话
        Transform tap;
        Text nameText;
        bool canNext = true;
        bool hasChoose = false;

        enum Dialog_Mode
        {
                WorldEvent = 0,   //世界地图点位
                Normal,        //普通对话
                AcceptTask,   //提交任务（带有奖励）
                SubmitTask,   //提交任务（带有奖励）
                
        };
        Dialog_Mode m_Mode = Dialog_Mode.Normal;
        
        void Awake()
        {
                EventTriggerListener.Get(transform.Find("btn")).onClick = OnNext;
                chooseGroup = transform.Find("choose");
                nameText = FindComponent<Text>("name");
                tap = transform.Find("tap");
                tap.DOScale(Vector3.one * 1.2f, 0.3f).SetLoops(-1);
                hasChoose = false;
        }

        public void RefreshEvent(WorldmapEventConfig wec) //世界地图事件点位
        {
                if (wec != null)
                {
                        m_Mode = Dialog_Mode.WorldEvent;
                        NormalDialog(wec.initial_dialog);
                }
        }

        public long npcId;
        public int taskId;
        public void RefreshTask(int taskNpcState, int taskId, long npcId) //npc提交任务
        {
                m_Mode = Dialog_Mode.Normal;
                this.taskId = taskId;
                this.npcId = npcId;
                TaskConfig tc = TaskManager.GetInst().GetTaskCfg(taskId);
                string dialog = String.Empty;
                if (tc != null)
                {
                        switch (taskNpcState)
                        {
                                case 1: //有任务可以交付//
                                        dialog = tc.submit_dialog;
                                        m_Mode = Dialog_Mode.SubmitTask;
                                        break;
                                case 2: //有任务可以接取//
                                        dialog = tc.accept_dialog;
                                        m_Mode = Dialog_Mode.AcceptTask;
                                        break;
                                case 3: //有任务已接取但没完成//
                                        dialog = tc.uncompleted_dialog;
                                        break;
                                case 4: //有任务可以接但未达到条件//
                                        dialog = tc.unaccepted_dialog;
                                        break;
                                default:
                                        return;

                        }
                }
                if (dialog.Length == 0)
                {
                        singleton.GetInst().ShowMessage(ErrorOwner.designer, "检查下任务" + taskId + "中是否有对话为空！");
                }
                NormalDialog(dialog);
        }


        public void RefreshTask(string common_dialog,long npcId)  //默认对话
        {
                this.npcId = npcId;
                m_Mode = Dialog_Mode.Normal;
                NormalDialog(common_dialog);
        }

        void NormalDialog(string dialog)
        {
                nowDialogIndex = 0;
                initDialogList = dialog.ToList<int>(',', (s) => (int.Parse(s)));
                ShowDialog(nowDialogIndex);
        }

        void ShowDialog(int index)
        {
                if (initDialogList.Count <= index)
                {
                        OnClickClose(null);
                        HomeManager.GetInst().ResetHomeCamera();
                        return;
                }
                int id = initDialogList[index];
                DialogConfig edc = CommonDataManager.GetInst().GetDialogCfg(id);
                if (edc != null)
                {
                        Image bg = FindComponent<Image>("bg");
                        ResourceManager.GetInst().LoadIconSpriteSyn(edc.event_picture, bg.transform);
                        bg.SetNativeSize();

                        canNext = true;
                        chooseGroup.SetActive(false);
                        tap.SetActive(true);

                        for (int i = 0; i < 2; i++)
                        {
                                FindComponent<Text>("Text" + i).text = "";
                        }

                        Text content;
                        //头像和名字
                        if (edc.npc_head_icon.Length > 0) //说明有头像和名字 文字排版有区别
                        {
                                content = FindComponent<Text>("Text0");
                        }
                        else
                        {
                                content = FindComponent<Text>("Text1");
                        }
                        ResourceManager.GetInst().LoadIconSpriteSyn(edc.npc_head_icon, transform.Find("head"));
                        nameText.text = LanguageManager.GetText(edc.talker_name);
                       
                        string text = edc.text;
                        isPlaying = true;
                        textTweener = content.DOText(text, 0.06f * text.Length).OnComplete(() => TextFinish(edc.event_choice));

                }
                else
                {
                        singleton.GetInst().ShowMessage(ErrorOwner.designer, "dialog中不存在id" + id);
                }
        }


        void ShowChoice(string event_choice)
        {
                if (event_choice.Length > 0) //有问题选项
                {
                        hasChoose = true;
                        canNext = false;
                        chooseGroup.SetActive(true);
                        string[] events = event_choice.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        GameObject quiz = chooseGroup.GetChild(0).gameObject;
                        SetChildActive(chooseGroup, false);
                        for (int i = 0; i < events.Length; i++)
                        {
                                Transform temp = null;
                                if (i < chooseGroup.childCount)
                                {
                                        temp = chooseGroup.GetChild(i);
                                }
                                else
                                {
                                        temp = CloneElement(quiz).transform;
                                }
                                temp.SetActive(true);

                                DialogConfig quiz_edc = CommonDataManager.GetInst().GetDialogCfg(int.Parse(events[i]));
                                if (quiz_edc != null)
                                {
                                        Button btn = temp.GetComponent<Button>();
                                        btn.onClick.RemoveAllListeners();
                                        string tempStr = i + CommonString.underscoreStr + quiz_edc.choice_result_dialog;
                                        btn.onClick.AddListener(() => OnQuizClick(tempStr));
                                        FindComponent<Text>(temp, "Text").text = quiz_edc.text; //选项
                                }
                        }
                        //tap.DOKill();
                        tap.SetActive(false);
                }
        }

        void TextFinish(string event_choice)
        {
                isPlaying = false;
                ShowChoice(event_choice);
        }

        bool isPlaying = false;  //是否在播放对话//
        Tweener textTweener;
       
        void OnQuizClick(string param)
        {
                string [] tag = param.Split(new char[]{'_'},StringSplitOptions.RemoveEmptyEntries);
                WorldMapManager.GetInst().SendMapEvent(int.Parse(tag[0]));

                initDialogList = tag[1].ToList<int>(',', (s) => (int.Parse(s)));
                nowDialogIndex = 0;
                ShowDialog(nowDialogIndex);

        }

        void OnNext(GameObject go, PointerEventData data)
        {
                if (isPlaying)
                {
                        textTweener.Kill(true);
                        isPlaying = false;
                        return;
                }
                if (canNext)
                {
                        nowDialogIndex++;
                        if (nowDialogIndex >= initDialogList.Count)
                        {
                                if (m_Mode == Dialog_Mode.WorldEvent)
                                {
                                        if (!hasChoose)
                                        {
                                                WorldMapManager.GetInst().SendMapEvent(0);
                                        }
                                }
                                else 
                                {
                                        if (m_Mode == Dialog_Mode.SubmitTask)
                                        {
                                                TaskConfig tc = TaskManager.GetInst().GetTaskCfg(taskId);
                                                string para = taskId + "|" + npcId;
                                                if (!tc.regular_reward.Equals(CommonString.zeroStr))
                                                {
                                                        UIManager.GetInst().ShowUI<UI_GetReward>("UI_GetReward").SetReward(tc.regular_reward, 0, TaskManager.GetInst().SendTaskSubmit, para);
                                                }
                                                else
                                                {
                                                        UIManager.GetInst().ShowUI<UI_GetReward>("UI_GetReward").SetReward(tc.selectable_reward, 1, TaskManager.GetInst().SendTaskSubmit, para);
                                                }
                                        }
                                        else if (m_Mode == Dialog_Mode.AcceptTask)
                                        {
                                                TaskManager.GetInst().SendTaskAccept(taskId, npcId);
                                        }

                                        if (npcId > 0)
                                        {
                                                HomeManager.GetInst().ResetHomeCamera();
                                        }

                                        //退出近景
                                }


                                OnClickClose(null);
                        }
                        else
                        {
                                ShowDialog(nowDialogIndex);
                        }
                }
        }


}
