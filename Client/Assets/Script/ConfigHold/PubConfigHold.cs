using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class PubConfigHold : ConfigBase
{
        public int reload_number;
        public int type;
        public int need_money;

        public PubConfigHold(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
