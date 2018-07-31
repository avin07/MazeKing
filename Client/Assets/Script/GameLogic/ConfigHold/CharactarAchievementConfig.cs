using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class CharactarAchievementConfig : ConfigBase
{
        public int series_id;
        public string name;
        public string illustration;
        public string tag;
        public string describe;
        public string biography;
        public int max_star;
        public int current_star;
        public int is_global;
        public int target_type;
        public string target_para_list;
        public string reward;

        public CharactarAchievementConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}


public enum ACHIEVE_TYPE
{
        LEVEL_UP = 1, // 升级 
        STAR_UP = 2, // 升星
        SKILL_ONE_UP = 3, // 任意技能达到指定等级

        TRANSFER = 10, // 转职
        SKILL_SPECIFY_UP = 11, // 升级指定技能
        BEHAVIOR_GET = 12, // 指定特性获取
        BEHAVIOR_COUNT = 13, // 特性数量获取
        RAID_COMPLETE_SELF = 14, // 自己完成指定副本
        RAID_COMPLETE = 15, // 带指定伙伴完成指定副本
}

