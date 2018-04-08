using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class VillageElementOptionConfig : ConfigBase
{
        public string option_name;
        public string option_effect;
        public string building_influence_appear;
        public string building_influence_disappear;
        public int adventure_skill_appear;
        public int adventure_skill_change_effect;
        public string adventure_skill_effect;
        public string item_require;

        public VillageElementOptionConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
