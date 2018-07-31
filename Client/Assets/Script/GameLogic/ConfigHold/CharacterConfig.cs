using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class CharacterConfig : ConfigBase
{
        public int accessories_id;
        public int career;
        public int career_sys;
        public string desc;
        public int model_id;
        public string name;
        public int race;
        public int weapon_id;

        public CharacterConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
