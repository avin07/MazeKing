using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class BuildRoomConfig : ConfigBase
{
        public string room_formula;
        public int popularity_by_blank;
        public int popularity_by_complete;
        public string wall_door_position;
        public string room_limit;

        public BuildRoomConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
