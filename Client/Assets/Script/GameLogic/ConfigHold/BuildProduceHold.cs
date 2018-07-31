using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class BuildProduceHold : ConfigBase
{
        public int output_time;
        public string output;
        public int reserve;

        public BuildProduceHold(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
