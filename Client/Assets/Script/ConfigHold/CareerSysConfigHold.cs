using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
public class CareerFamilyConfig : ConfigBase
{
        public string name;
        public int factor_vs_1;
        public int factor_vs_2;
        public int factor_vs_3;
        public int factor_vs_4;
        public int factor_vs_5;
        public int factor_vs_6;
        public string favor_sequence;
        public string icon;

        public CareerFamilyConfig(XmlNode child)
                : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }
}
