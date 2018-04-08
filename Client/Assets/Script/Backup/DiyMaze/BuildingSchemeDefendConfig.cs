//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using System.Xml;

//public class BuildingSchemeDefendConfig : ConfigBase
//{
//        public int raid_file_number;
//        public string unlock_defendterrain;
//        public int normal_level_limit;
//        public int rare_level_limit;
//        public int epic_level_limit;
//        public int legend_level_limit;
//        public int set_level_limit;

//        public BuildingSchemeDefendConfig(XmlNode child) : base(child)
//        {
//        }

//        public override void InitSelf(XmlNode child)
//        {
//                SetupFields(child);
//        }


//        public int GetLimitByQuality(int quality)
//        {
//                switch (quality)
//                {
//                        case 1:
//                                return normal_level_limit;
//                        case 2:
//                                return rare_level_limit;
//                        case 3:
//                                return epic_level_limit;
//                        case 4:
//                                return set_level_limit;
//                        case 5:
//                                return legend_level_limit;
//                        default:
//                                return normal_level_limit;
//                }
//        }

//}
