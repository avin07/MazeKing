using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class ModelAccessoriesConfig : ConfigBase
{
        public string accessories_name;
        public string bone_name;

        public ModelAccessoriesConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
