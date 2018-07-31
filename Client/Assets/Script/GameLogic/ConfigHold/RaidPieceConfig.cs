using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class RaidPieceConfig : ConfigBase
{
        public int special_door;
        public int special_door_type;

        public RaidPieceConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
