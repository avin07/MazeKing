using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
public class Experience : ConfigBase
{
        public long need_exp;
        public long total_exp;

        public int reset_skill_cost_diamond;

        public Experience(XmlNode child)
            : base(child)
        {
        }
        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }
}
