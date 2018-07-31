using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class RequireRecruitHold : ConfigBase
{
        public int type;
        public string present;
        public string add_favor;
        public string desc;
        public string name;
        public string icon;

        public RequireRecruitHold(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
