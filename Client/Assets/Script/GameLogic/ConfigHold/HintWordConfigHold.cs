using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class HintWordConfig : ConfigBase
{
        public string text;
        public HintWordConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }
}
