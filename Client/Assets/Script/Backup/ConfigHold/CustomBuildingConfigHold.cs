//using UnityEngine;
//using System.Collections;
//using System.Xml;
//public class CustomBuildingConfig : ConfigBase
//{
//        public int level;
//        public string desc;
//        public string name;

//        public int type;
//        public int type_small;
//        public Vector3 area;
//        public int model_id;
//        public string event_icon;
//        public int gold_cost;
//        public int time;
//        public int event_type;
//        public string event_action;
//        public int load_time;
//        public string limit_career;
//        public string good_career;
//        public string event_attribute;
//        public int event_range;
//        public int event_time;
//        public string event_reward;
//        public string event_effect;

//        public CustomBuildingConfig(XmlNode child) 
//                : base(child)
//        {
//                XMLPARSE_METHOD.GetNodeInnerInt(child, "level", ref level, 0);
//                XMLPARSE_METHOD.GetNodeInnerText(child, "desc", ref desc, "");
//                XMLPARSE_METHOD.GetNodeInnerText(child, "name", ref name, "");

//                XMLPARSE_METHOD.GetNodeInnerInt(child, "type", ref type, 0);
//                XMLPARSE_METHOD.GetNodeInnerInt(child, "type_small", ref type_small, 0);

//                XMLPARSE_METHOD.GetNodeInnerVec3(child, "area", ref area, Vector3.one);
//                XMLPARSE_METHOD.GetNodeInnerInt(child, "model_id", ref model_id, 0);
//                XMLPARSE_METHOD.GetNodeInnerText(child, "event_icon", ref event_icon, "");
//                XMLPARSE_METHOD.GetNodeInnerInt(child, "gold_cost", ref gold_cost, 0);
//                XMLPARSE_METHOD.GetNodeInnerInt(child, "time", ref time, 0);

//                XMLPARSE_METHOD.GetNodeInnerInt(child, "event_type", ref event_type, 0);
//                XMLPARSE_METHOD.GetNodeInnerInt(child, "load_time", ref load_time, 0);
//                XMLPARSE_METHOD.GetNodeInnerText(child, "event_action", ref event_action, "");
//                XMLPARSE_METHOD.GetNodeInnerText(child, "limit_career", ref limit_career, "");
//                XMLPARSE_METHOD.GetNodeInnerText(child, "good_career", ref good_career, "");
//                XMLPARSE_METHOD.GetNodeInnerText(child, "event_attribute", ref event_attribute, "");
//                XMLPARSE_METHOD.GetNodeInnerInt(child, "event_range", ref event_range, 0);
//                XMLPARSE_METHOD.GetNodeInnerInt(child, "hold_time", ref event_time, 0);
//                XMLPARSE_METHOD.GetNodeInnerText(child, "event_reward", ref event_reward, "");

//                XMLPARSE_METHOD.GetNodeInnerText(child, "event_effect", ref event_effect, "");
//        }
//}
