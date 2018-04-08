//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using System.Xml;

//public class JigsawTileUpgradeConfig : ConfigBase
//{
//        public int normal_req_quantity;
//        public int normal_req_gold;
//        public int rare_req_quantity;
//        public int rare_req_gold;
//        public int epic_req_quantity;
//        public int epic_req_gold;
//        public int legend_req_quantity;
//        public int legend_req_gold;
//        public int set_req_quantity;
//        public int set_req_gold;

//        public JigsawTileUpgradeConfig(XmlNode child) : base(child)
//        {
//        }

//        public override void InitSelf(XmlNode child)
//        {
//                SetupFields(child);
//        }


//        public int GetReqGoldByQuality(int quality)
//        {
//                switch (quality)
//                {
//                        case 1: 
//                                return normal_req_gold;
//                        case 2:
//                                return rare_req_gold;
//                        case 3:
//                                return epic_req_gold;
//                        case 4:
//                                return set_req_gold;
//                        case 5:
//                                return legend_req_gold;
//                        default:
//                                return normal_req_gold;
//                }
//        }

//        public int GetReqQuantityByQuality(int quality)
//        {
//                switch (quality)
//                {
//                        case 1:
//                                return normal_req_quantity;
//                        case 2:
//                                return rare_req_quantity;
//                        case 3:
//                                return epic_req_quantity;
//                        case 4:
//                                return set_req_quantity;
//                        case 5:
//                                return legend_req_quantity;
//                        default:
//                                return normal_req_quantity;
//                }
//        }
//}
