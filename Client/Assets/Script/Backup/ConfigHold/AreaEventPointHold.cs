//using UnityEngine;
//using System.Collections;
//using System.Xml;
//public class AreaEventPoint : ConfigBase
//{
//        public string name;
        
//        public string desc;
//        /// <summary>
//        /// 从属的区域地图
//        /// </summary>
//        public int belong_mapid;
        
//        /// <summary>
//        /// 从属的岗哨
//        /// </summary>
//        public int belong_outpost;

//        /// <summary>
//        /// 1,空地
//        /// 2,未知地
//        /// 3,岗哨
//        /// 4,城镇
//        /// 5,采集点
//        /// 6,迷宫
//        /// </summary>
//        public int type;
        
//        /// <summary>
//        /// type=1时，无效
//        /// type=2时，无效
//        /// type=3时，解锁条件（1=队伍等级），数值
//        /// type=4时，城镇id
//        /// type=5时，采集资源类型
//        /// type=6时，迷宫id
//        /// </summary>
//        public string para;

//        public AreaEventPoint(XmlNode child)
//                : base(child)
//        {

//                XMLPARSE_METHOD.GetNodeInnerInt(child, "map_id", ref belong_mapid, 0);
//                XMLPARSE_METHOD.GetNodeInnerInt(child, "father_outpost_id", ref  belong_outpost, 0);
//                XMLPARSE_METHOD.GetNodeInnerInt(child, "type", ref type, 0);
                
//                XMLPARSE_METHOD.GetNodeInnerText(child, "para", ref para, "");
//                XMLPARSE_METHOD.GetNodeInnerText(child, "name", ref name, "");
//                XMLPARSE_METHOD.GetNodeInnerText(child, "description", ref desc, "");
//        }
//}
