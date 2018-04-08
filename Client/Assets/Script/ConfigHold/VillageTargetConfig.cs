using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class VillageTargetConfig : ConfigBase
{
        public string charactar_element;

        public VillageTargetConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
