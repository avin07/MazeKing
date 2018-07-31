using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class SkillBuffConfig : ConfigBase
{
        public string name;
        public string mark;
        public string icon;
        public string desc;
        /// <summary>
        
        /// 1，战斗内回合前多次生效（dothot）
        /// 2，战斗内外都立即持续生效（属性）
        /// 3，战斗内仅最后一回合生效（定时炸弹）
        /// </summary>
        public int effective_type;
        /// <summary>
        /// 1，战斗内角色行动前失效
        /// 2，战斗内角色行动后失效
        /// 3，战斗外离开当前层失效
        /// 4，战斗外持续n场战斗，并战斗结束后失效
        /// 5，战斗外直到下次扎营前失效
        /// </summary>
        public int fail_type;
        public int time;
        public int good_or_bad;
        public int show_type;
        public int is_more_battle;
        public string exist_effect;
        public int effective_action;
        public string effective_effect;
        public int multiplier;
        public string attributes;
        public int overlay_number;
        public int overlay_type;
        public int group;        
        public int call_trigger_id;
        public int trigger_number;
        public string add_effect;

        public int is_shield;
        public int target_heal_multiplier_per;
        public int cast_attack_multiplier_per;

        //public int is_escape_fail;
        public int is_dead_fail;
        public int is_sleep_fail;
        public int is_damage_fail;
        public int is_action_fail;

        public SkillBuffConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
