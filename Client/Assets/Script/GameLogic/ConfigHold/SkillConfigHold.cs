using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public enum SkillTargetGroup
{
        ENEMY,
        TEAMMATE_EXCEPT_SELF,
        TEAMMATE_ALL,
        SELF,
        ENEMY_FRONT,    //敌方前排
        ENEMY_BACK,      //敌方后排
        TEAMMATE_SAMEROW,
};
public enum SkillRangeType
{
        ALL,                                       //全体
        MAIN,                                   //主目标
        MAIN_WITH_RANDOM,       //主目标附带随机
        SAME_ROW,                       //同排全体
}
public enum Skill_Effect_Type
{
        DAMAGE,
        HEAL,
        BUFF,
        REVIVE,
};

public class SkillConfig : ConfigBase
{
        /// <summary>
        /// 0伤害 1治疗 2辅助 3复活
        /// </summary>
        public int effect_type;
        public int init_cool_down;
        /// <summary>
        /// 0，敌方1，不包括自己的友方2，包括自己的友方3，自己
        /// </summary>
        public int target_group;
        /// <summary>
        /// 0 全体 1单体 2单体随机多人
        /// </summary>
        public int range_type;
        /// <summary>
        /// 0，全体 1，原目标 2，原目标附加随机多人 3，随机目标 4，随机目标附加随机多人
        /// </summary>
        public int add_skill_range_type;
        /// <summary>
        /// 0，全体 1，触发器挂载者 2，触发器挂载者阵营附加随机多人 3，触发器释放者 4，触发器释放者阵营附加随机多人 5，触发对象 6，触发对象阵营附加随机多人
        /// </summary>
        public int trigger_skill_range_type;
        public int random_multiple_number;
        public int random_multiple_special_need;
        /// <summary>
        /// 是否有命中顺序，有命中顺序表示目标可以重复， 没有命中顺序则不可重复
        /// </summary>
        public int is_hit_sequence;
        public int hit_sequence_interval_time;
        public int forward_type;
        public int cast_action_id;
        public string cast_effect;
        public string cast_sound;
        public string hit_action;
        public string hit_effect;
        public string hit_sound;
        public int hit_type;
        public int hit_delay;
        public string location_sound;
        public string location_effect;
        public string bullet_effect;
        public string bullet_sound;        
        public float bullet_time;
        public string attack_heal_number;
        public int base_hitrate_per;
        public int base_critical_rate_per;
        public int extra_critical_rate_per;
        public int extra_damage_increase_per;
        public int extra_penetrate_per;
        public int change_position;
        public List<int> target_condition;
        public int disperse;
        public string disperse_effect;        

        public string revive_effect;

        public List<int> buff;
        public int add_buff_number;

        public int speed;
        public int summon;
        public int punch;
        public int vampire;
        public int pressure;
        public int brightness;
        public int add_skill_condition;
        public int add_skill_id;
        public int is_silence;        
        
        public int extra_attributes;
        public string temporary_attribute;        
        public int disperse_number;
        public int change_buff;
        public string script;

        //以下属性有等级
        public List<int> cool_down;

        public List<int> revive;
        public List<int> disperse_rate;
        public List<int> buff_rate;
        public List<int> speed_rate;
        public List<int> summon_rate;
        public List<int> punch_rate;
        public List<int> pressure_rate;
        public List<int> brightness_rate;
        public List<int> add_skill_rate;

        public string attack_multiplier;
        public string heal_multiplier;
        public string heal_multiplier_per;
        public string extra_attributes_multiplier;

        public bool IsAffectEnemy()
        {
                return target_group == (int)SkillTargetGroup.ENEMY || 
                        target_group == (int)SkillTargetGroup.ENEMY_FRONT || 
                        target_group == (int)SkillTargetGroup.ENEMY_BACK;
        }

        public List<int> GetMultiplier(string multiplier_str, int level)
        {
/*                Debuger.Log(multiplier_str + "  " + level);*/
                List<int> retlist = new List<int>();

                string[] infos = multiplier_str.Split(';');
                if (level > 0 && level <= infos.Length)
                {
                        string[] tmps = infos[level - 1].Split(',');
                        foreach (string val in tmps)
                        {
                                if (!string.IsNullOrEmpty(val))
                                {
                                        retlist.Add(int.Parse(val));
                                }
                        }
                }
                return retlist;

        }

//         public List<int> GetMultiplier(string multiplier_str, FighterProperty fighterProp)
//         {
//                 int level = fighterProp.GetActiveSkillLevel(this.id);
//                 //Debuger.Log(level);
//                 return GetMultiplier(multiplier_str, level);
//         }

        public int GetLevelValue(List<int> prop, int level)
        {
                if (level > 0 && level <= prop.Count)
                {
                        return prop[level - 1];
                }

                return 0;
        }

//         public int GetLevelValue(List<int> prop, FighterProperty fighterProp)
//         {
//                 if (fighterProp != null)
//                 {
//                         int level = fighterProp.GetActiveSkillLevel(this.id);
//                         return GetLevelValue(prop, level);
//                 }
//                 return 0;
//         }

        public SkillConfig(XmlNode child)
                : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }
}
