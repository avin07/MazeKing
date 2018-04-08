using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
public class CareerConfigHold : ConfigBase
{
        public string name;
        public string icon;
        public string desc;
        public CareerConfigHold(XmlNode child)
            : base(child)
        {
                XMLPARSE_METHOD.GetNodeInnerText(child, "name", ref name, "");
                XMLPARSE_METHOD.GetNodeInnerText(child, "icon", ref icon, "");
                XMLPARSE_METHOD.GetNodeInnerText(child, "desc", ref desc, "");
        }
}
