using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;


public class NpcConfig : ConfigBase
{
        public int model;
        public string head_icon;
        public string name;
        public string desc;
        public int star;

        public int type;
        public int type_id;
        public int is_clear_immediately;
        public int live_days;   //停留天数
        public int place;
        public string issue_task_id;     //可发布任务//
        public string submit_task_id;    //可交付任务//
        public string common_dialog;
        public int price;

        public NpcConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
