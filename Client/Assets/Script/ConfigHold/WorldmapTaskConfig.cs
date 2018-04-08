using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class WorldmapTaskConfig : ConfigBase
{
        //public int type;
        public string name;
        public string icon;
        public string issue_task_id;
        public string submit_task_id;
        public string common_dialog;

        public WorldmapTaskConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
