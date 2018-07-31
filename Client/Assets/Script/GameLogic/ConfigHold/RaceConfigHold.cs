using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
public class RaceConfigHold : ConfigBase
{
        public string bg;
        public string name;
        public RaceConfigHold(XmlNode child)
            : base(child)
        {
            XMLPARSE_METHOD.GetNodeInnerText(child, "bg_picture", ref bg, "");
            XMLPARSE_METHOD.GetNodeInnerText(child, "name", ref name, "");
            name = LanguageManager.GetText(name);
        }
}