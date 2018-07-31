using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class RaidCampEventConfig : ConfigBase
{
        public string picture;
        public string desc;
        public int condition;
        public int range_type;
        public int is_battle;
        public int is_miss_item;
        public int pressure;
        public string buff;
        public int weight;

        public RaidCampEventConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
