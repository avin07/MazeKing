using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class BuildingHabitancyConfig : ConfigBase
{
        public int live_limit;

        public BuildingHabitancyConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
