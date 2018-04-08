using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class BattlePlaceConfig : ConfigBase
{
        public string battle_point_name;
        public int grid_number;

        public BattlePlaceConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
