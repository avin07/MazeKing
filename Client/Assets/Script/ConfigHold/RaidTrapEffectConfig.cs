using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

[System.Serializable]
public class RaidTrapEffectConfig : ConfigBase
{
        public string effect;
        public int hp;
        public int is_team_attributes;
        public int is_monomer;
        public int buff_id;
        public int drop_id;
        //public int pressure;
        public int hint_word;
        public int is_stop;
        public int is_shock_screen;
        public int effect_type;
        public string sound_effect;
        public RaidTrapEffectConfig(XmlNode child)
                : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
