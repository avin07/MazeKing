using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class RoomLayoutConfig : ConfigBase
{
        public int wall_layout;
        public string room_decoration;
        public string door_position;
        public int door_model;
        public int area;

        public RoomLayoutConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
