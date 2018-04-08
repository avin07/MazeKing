using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class BuildFunctionHold : ConfigBase
{
        public string icon;

        public BuildFunctionHold(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
