using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class BuildingHomeTreasureConfig : ConfigBase
{
        public int brick_model;
        public int model;
        public int size_x;
        public int size_y;
        public int size_z;
        public int type;
        public int clean_time;
        //public string hint_effect;

        public BuildingHomeTreasureConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
