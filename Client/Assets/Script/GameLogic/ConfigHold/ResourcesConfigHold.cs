using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class ResourcesConfigHold : ConfigBase
{
        public string name;
        public string desc;
        public string icon;
        public string attr;
        public string lock_attr;

        public ResourcesConfigHold(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
