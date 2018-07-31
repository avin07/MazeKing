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
        
        /// 1��ս���ڻغ�ǰ�����Ч��dothot��
        /// 2��ս�����ⶼ����������Ч�����ԣ�
        /// 3��ս���ڽ����һ�غ���Ч����ʱը����
        /// </summary>
        public int effective_type;
        /// <summary>
        /// 1��ս���ڽ�ɫ�ж�ǰʧЧ
        /// 2��ս���ڽ�ɫ�ж���ʧЧ
        /// 3��ս�����뿪��ǰ��ʧЧ
        /// 4��ս�������n��ս������ս��������ʧЧ
        /// 5��ս����ֱ���´���ӪǰʧЧ
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
