using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class RaidDecorationConfig : ConfigBase
{
        public int model;

        public RaidDecorationConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
