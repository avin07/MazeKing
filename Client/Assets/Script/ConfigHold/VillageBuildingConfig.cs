using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class VillageBuildingConfig : ConfigBase
{
        public int building_type;
        public int level;
        public int avatar;

        public VillageBuildingConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
