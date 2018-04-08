//using UnityEngine;
//using System.Collections;
//using System.Xml;
//public class WorldMapConfig : ConfigBase
//{
//        public string name;
//        public string desc;
//        public string res;
//        public int bornid;
//        public Vector2 cameraBoundX;
//        public Vector2 cameraBoundY;

//        public WorldMapConfig(XmlNode child)
//                : base(child)
//        {
//                XMLPARSE_METHOD.GetNodeInnerText(child, "name", ref name, "");
//                XMLPARSE_METHOD.GetNodeInnerText(child, "map_resource", ref res, "");
//                XMLPARSE_METHOD.GetNodeInnerText(child, "description", ref desc, "");

//                XMLPARSE_METHOD.GetNodeInnerInt(child, "born_district_id", ref bornid, 0);
//                XMLPARSE_METHOD.GetNodeInnerVec2(child, "camera_bound_x", ref cameraBoundX, Vector2.zero);
//                XMLPARSE_METHOD.GetNodeInnerVec2(child, "camera_bound_y", ref cameraBoundY, Vector2.zero);
//        }
//}
