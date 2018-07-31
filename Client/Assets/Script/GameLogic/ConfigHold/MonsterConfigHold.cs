using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class MonsterConfig : ConfigBase
{
        public int charactar_id;
        public int init_attack;
        public int init_speed;
        public int init_critical_rate_per;
        public int init_critical_damage_per;
        public int init_defence;
        public int init_life;
        public int attack_growth;
        public int defence_growth;
        public int life_growth;
        public int attack_factor;
        public int speed_factor;
        public int defence_factor;
        public int life_factor;
        public int speciality_factor;
        public int attack_per_factor;
        public int speed_per_factor;
        public int critical_rate_per_factor;
        public int accurate_per_factor;
        public int penetrate_per_factor;
        public int damage_increase_per_factor;
        public int defence_per_factor;
        public int dodge_per_factor;
        public int debuff_resistance_per_factor;
        public int damage_reduce_per_factor;
        public int parry_per_factor;
        public int rebound_per_factor;
        public int tough_per_factor;
        public int life_per_factor;
        public int life_steal_per_factor;
        public int critical_damage_per_factor;
        public int heal_increase_per_factor;
        public int recover_increase_per_factor;
        public int cooldown_reduce_per_factor;
        public int trigger_rate_per_factor;
        public string skill_list;

        public MonsterConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
