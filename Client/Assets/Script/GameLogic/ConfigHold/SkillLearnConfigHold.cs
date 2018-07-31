using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
public class SkillLearnConfigHold : ConfigBase
{
        public int type;  //1被动 2主动 3 超级 4 冒险 5扎营 6队长
        public int is_base_skill;
        public string icon;
        public string name;
        public string desc;
        public int quality;
        public List<int> max_level = new List<int>();  //每个星级的技能等级上线
        public SkillLearnConfigHold(XmlNode child) : base(child)
        {
        }


        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }
}

public enum SKILL_TYPE
{
        PASSIVE = 1,  // 被动技能
        ACTIVE = 2,  // 主动技能
        SUPER = 3,  // 超级技能
        ADVENTURE = 4,// 冒险技能
        CAMP = 5,//扎营技能
        CAPTAIN = 6,       //队长技能
}
