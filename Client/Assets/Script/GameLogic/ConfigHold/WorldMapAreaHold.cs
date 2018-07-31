using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class WorldMapAreaHold : ConfigBase
{
        public int need_level;
        public int unlock_type;
        public int cost_gold;
        public int teach_raid_id;
        public string dp_unlock_progress;
        public int dp_max;
        public string area_name;

        public WorldMapAreaHold(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
