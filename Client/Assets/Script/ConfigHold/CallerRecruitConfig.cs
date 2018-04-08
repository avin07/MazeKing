using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class CallerRecruitConfig : ConfigBase
{
        public int price;

        public CallerRecruitConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
