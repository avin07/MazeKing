using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class RaidCampSkillConfig : ConfigBase
{
        public string skill_effect;
        public int action_id;
        public int pressure;
        public int heal_multiplier_per;
        public string buff;
        public int cost_point;
        public RaidCampSkillConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
