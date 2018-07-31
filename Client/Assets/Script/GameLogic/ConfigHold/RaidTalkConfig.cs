using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class RaidTalkConfig : ConfigBase
{
        public int effect_type;
        public int range_type;
        public int rate;
        public List<int> talk_pool;
        public int type;

        public RaidTalkConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
