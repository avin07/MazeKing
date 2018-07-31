using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class BuildingFireRoomConfig : ConfigBase
{
        public int unlock_area;
        public string unlock_effect;

        public BuildingFireRoomConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
