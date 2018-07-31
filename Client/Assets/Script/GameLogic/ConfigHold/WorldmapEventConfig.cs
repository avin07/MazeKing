using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class WorldmapEventConfig : ConfigBase
{
        public int event_type;
        public string icon;
        public string initial_dialog;
        public string name;
        public int need_level;

        public WorldmapEventConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
