using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class SkillSummonConfig : ConfigBase
{
        public int type;
        public int number;
        public int live_time;
        public int need_time;
        public int range;
        public string range_effect;
        public int charactar_id;

        public SkillSummonConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
