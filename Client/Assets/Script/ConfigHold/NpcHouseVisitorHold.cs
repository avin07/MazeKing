using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class NpcHouseVisitorHold : ConfigBase
{
        public string photo;
        public string name;
        public string desc;
        public int get_type;
        public string get_parameter;
        public int get_per;
        public int pay_type;
        public string pay_parameter;
        public int is_choose; //0无选人，1有选人

        public NpcHouseVisitorHold(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
