using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class PassiveSkillConfig : ConfigBase
{

        public int type;
        public string attributes;
        public int trigger_id;
        public string attributes_2;
        public string attributes_3;
        public string attributes_4;
        public string attributes_5;

        public PassiveSkillConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
