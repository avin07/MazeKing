using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class BuildingBookshelvesConfig : ConfigBase
{
        public int skill_level_limit;

        public BuildingBookshelvesConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
