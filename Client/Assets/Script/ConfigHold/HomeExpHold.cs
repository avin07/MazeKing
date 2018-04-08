using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class HomeExpHold : ConfigBase
{
        public int need_exp;
        public int total_exp;

        public HomeExpHold(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
