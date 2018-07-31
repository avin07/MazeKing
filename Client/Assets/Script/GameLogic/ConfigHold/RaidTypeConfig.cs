using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

[System.Serializable]
public class RaidTypeConfig : ConfigBase
{
        public int door_type;
        public int is_perception;
        public string mark;
        public string condition;
        public string room_type_icon;

        public RaidTypeConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
