using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class VillageElementConfig : ConfigBase
{        
        public string head_icon;
        public List<int> type_id;
        public int task_npc_id;

        public VillageElementConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
