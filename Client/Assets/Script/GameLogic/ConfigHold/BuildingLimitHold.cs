using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class BuildingLimitHold : ConfigBase
{
        public int level;
        public int max_number;

        public BuildingLimitHold(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
