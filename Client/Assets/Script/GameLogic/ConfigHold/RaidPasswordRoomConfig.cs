using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class RaidPasswordRoomConfig : ConfigBase
{
        public int broke_probability;
        public string clue_id;
        public int clue_number;
        public List<string> dark_password_icon;
        public List<string> bright_password_icon;
        
        public int password_number;
        public int password_option;

        public RaidPasswordRoomConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
