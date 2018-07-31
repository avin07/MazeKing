using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class BuildingTechnologyConfig : ConfigBase
{
        public string name;
        public string describe;
        public string icon;
        public string relate_attribute;
        public int worktable_id;
        public string cost;
        public int class_id;

        public BuildingTechnologyConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
