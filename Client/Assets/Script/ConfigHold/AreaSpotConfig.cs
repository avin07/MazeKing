using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class AreaSpotConfig : ConfigBase
{
        public int area_id;
        public int spot_id;
        public int need_goal_point;  //显示点位需求的dp点数

        public AreaSpotConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
