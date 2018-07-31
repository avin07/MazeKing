using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class TaskConfig : ConfigBase
{
        public int pre_task;
        public string accept_dialog;      //������ʱ�Ի�
        public int home_level_limit;
        public string name;
        public string description;
        public int type;
        public int target_type;
        public string target;
        public string target_num;
        public string regular_reward;   //�̶�
        public string selectable_reward;//��ѡ
        public string resource_reward;  //��Դ
        public string area_goal_point_reward; //����dp����

        public string submit_dialog;       //�ύʱ�Ի�
        public string progress_notice;
        public string uncompleted_dialog;  //δ���ʱ�ĶԻ�
        public string unaccepted_dialog;   //δ��ȡ�Ҳ��ɽ�ȡʱ�ĶԻ�
        public string head_icon;       
    

        public TaskConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
