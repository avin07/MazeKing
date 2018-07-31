using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class SkillMoveConfig : ConfigBase
{
        public string icon;
        public int type;
        public int distance;
        public int speed;
        public string effect;

        public SkillMoveConfig(XmlNode child)
                : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }
}
