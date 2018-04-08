using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class RaidConditionRoomConfig : ConfigBase
{
        public int type;
        public string desc;
        public int item;
        public int hp_per;
        
        public RaidConditionRoomConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }
}
