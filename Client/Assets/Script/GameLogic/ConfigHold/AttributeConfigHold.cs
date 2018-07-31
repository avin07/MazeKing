using UnityEngine;
using System.Collections;
using System.Xml;
public class AttributeConfig : ConfigBase
{
        public string name;
        public string mark;
        public AttributeConfig(XmlNode child)
                : base(child)
        {
                XMLPARSE_METHOD.GetNodeInnerText(child, "name", ref name, "");
                XMLPARSE_METHOD.GetNodeInnerText(child, "mark", ref mark, "");
        }
}
