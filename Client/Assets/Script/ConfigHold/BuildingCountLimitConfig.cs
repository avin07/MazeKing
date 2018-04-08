using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class BuildingCountLimitConfig : ConfigBase
{
        public int overall_capacity_limit;  //全局限制

        public BuildingCountLimitConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
