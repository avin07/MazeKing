using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class PressureJudgeConfig : ConfigBase
{
        public string mark;
        public int type;
        public int range_type;
        public int weight;
        public string talk;
        public int buff;
        public string name;
        public int pressure;
        public int heal_multiplier_per;
        public PressureJudgeConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
