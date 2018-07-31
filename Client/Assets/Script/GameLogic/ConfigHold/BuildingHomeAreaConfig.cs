using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class BuildingHomeAreaConfig : ConfigBase
{
        public int unlock_building;
        public string unlock_effect_position;
        public string unlock_effect;

        public BuildingHomeAreaConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
