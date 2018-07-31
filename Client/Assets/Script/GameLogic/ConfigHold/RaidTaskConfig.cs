using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

[System.Serializable]
public class RaidTaskConfig : ConfigBase
{
        public string name;
        public string desc;
        public string detail;
        public int type;
        public List<int> target_id;
        public List<int> req_quantity;
        
        public int reward_home_exp;
        public string reward_item_list;        

        public RaidTaskConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
