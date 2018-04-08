using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
public class CharacterConfig : ConfigBase
{
        public int career;
        public int race;
        public int modelid;
        public int weapon_id;
        public int cap_id;
        public int accessories_id;

        public int attack_growth;
        public int defence_growth;
        public int hp_growth;

        public int init_attack;
        public int init_defence;
        public int init_hp;
        public int init_speed;
        public int init_pressure;
        public int init_critical_damage;
        public int init_critical_rate;
        public string name;

        public int character_element;
        public int body_type;
        public List<int> active_skill;
        public List<int> passive_skill;
        public List<int> adventure_skill;
        public List<int> camp_skill;
        public Dictionary<string, string> m_pet_info = new Dictionary<string, string>(); //方便后续统一接口属性获取//

        public string GetProp(string name)
        {
                if (m_pet_info.ContainsKey(name))
                {
                        return m_pet_info[name];
                }
                return "";
        }

        public int GetPropInt(string name)
        {
            int value = 0;
            int.TryParse(GetProp(name), out value);
            return value;
        }


        public CharacterConfig(XmlNode child)
                : base(child)
        {
                XMLPARSE_METHOD.GetNodeInnerInt(child, "attack_growth", ref attack_growth, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "defence_growth", ref defence_growth, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "life_growth", ref hp_growth, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "init_critical_rate_per", ref init_critical_rate, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "init_critical_damage_per", ref init_critical_damage, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "init_pressure", ref init_pressure, 0);
                
                XMLPARSE_METHOD.GetNodeInnerInt(child, "race", ref race, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "charactar_element", ref character_element, 0);

                XMLPARSE_METHOD.GetNodeInnerInt(child, "init_life", ref init_hp, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "init_attack", ref init_attack, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "init_defence", ref init_defence, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "init_speed", ref init_speed, 1);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "career", ref career, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "model_id", ref modelid, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "weapon_id", ref weapon_id, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "cap_id", ref cap_id, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "accessories_id", ref accessories_id, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "body_type", ref body_type, 0);
                
                XMLPARSE_METHOD.GetNodeInnerText(child, "name", ref name, "");
                active_skill = XMLPARSE_METHOD.GetNodeInnerIntList(child, "skill_list", ',', 0);
                passive_skill = XMLPARSE_METHOD.GetNodeInnerIntList(child, "passive_skill", ',', 0);
                camp_skill = XMLPARSE_METHOD.GetNodeInnerIntList(child, "camp_skill", ',', 0);
                adventure_skill = XMLPARSE_METHOD.GetNodeInnerIntList(child, "adventure_skill", ',', 0);
                m_pet_info = XMLPARSE_METHOD.GetXMLContent(child);
        }
}
