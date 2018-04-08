using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class RaidSupporterConfig : ConfigBase
{
        public int supporter_id;
        public int monster_id;
        public int level;
        public string save_speak;

        public RaidSupporterConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
