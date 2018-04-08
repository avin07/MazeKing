using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class TaskConfig : ConfigBase
{
        public int pre_task;
        public string accept_dialog;      //接任务时对话
        public int home_level_limit;
        public string name;
        public string description;
        public int type;
        public int target_type;
        public string target;
        public string target_num;
        public string regular_reward;   //固定
        public string selectable_reward;//可选
        public string resource_reward;  //资源
        public string area_goal_point_reward; //区域dp奖励

        public string submit_dialog;       //提交时对话
        public string progress_notice;
        public string uncompleted_dialog;  //未完成时的对话
        public string unaccepted_dialog;   //未接取且不可接取时的对话
        public string head_icon;       
    

        public TaskConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
