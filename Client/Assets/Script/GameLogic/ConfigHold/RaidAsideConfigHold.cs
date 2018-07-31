using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class RaidAsideConfigHold : ConfigBase
{
        public int raid;
        public int cutscene;
        public string enter_aside;
        public string complete_aside;

        public RaidAsideConfigHold(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
