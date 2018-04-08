using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class RaidTalkPoolConfig : ConfigBase
{
        public string text;

        public RaidTalkPoolConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
