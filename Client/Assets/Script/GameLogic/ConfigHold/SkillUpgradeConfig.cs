using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class SkillUpgradeConfig : ConfigBase
{
        public List<int> quality_cost_gold;

        public SkillUpgradeConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
