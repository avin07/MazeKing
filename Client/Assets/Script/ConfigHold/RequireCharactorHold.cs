using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class RequireCharactorHold : ConfigBase
{
        public int recruit_times;

        public RequireCharactorHold(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
