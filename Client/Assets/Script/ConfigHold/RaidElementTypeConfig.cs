using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class RaidElementTypeConfig : ConfigBase
{
        public int priority;
        public string icon;

        public RaidElementTypeConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
