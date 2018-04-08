using UnityEngine;
using System.Collections;
using System.Xml;

public class EquipHoldConfig : ConfigBase
{
        public string name;
        public string icon;
        public string desc;
        public int type;
        public int quality;
        public int is_two_hands;
        public int place;
        public string need_career;
        public int need_level;
        public string unload_cost;
        public string attributes;  //属性增加
        public int score;
        public int value;
        public string type_icon;
        public EquipHoldConfig(XmlNode child)
                : base(child)
        {
                XMLPARSE_METHOD.GetNodeInnerInt(child, "type", ref type, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "quality", ref quality, 0);
                XMLPARSE_METHOD.GetNodeInnerText(child, "name", ref name, "");
                XMLPARSE_METHOD.GetNodeInnerText(child, "icon", ref icon, "");
                XMLPARSE_METHOD.GetNodeInnerText(child, "desc", ref desc, "");
                XMLPARSE_METHOD.GetNodeInnerInt(child, "is_two_hands", ref is_two_hands, 0);

                string temp_place = "";
                XMLPARSE_METHOD.GetNodeInnerText(child, "place", ref temp_place, "11");
                place = int.Parse(temp_place.Split(',')[0]);

                XMLPARSE_METHOD.GetNodeInnerText(child, "need_career", ref need_career, "-1");
                XMLPARSE_METHOD.GetNodeInnerInt(child, "need_level", ref need_level, 0);
                XMLPARSE_METHOD.GetNodeInnerText(child, "unload_cost", ref unload_cost, "");
                XMLPARSE_METHOD.GetNodeInnerText(child, "attributes", ref attributes, "");
                XMLPARSE_METHOD.GetNodeInnerInt(child, "score", ref score, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "value", ref value, 0);
                XMLPARSE_METHOD.GetNodeInnerText(child, "type_icon", ref type_icon, "");
        }
}

