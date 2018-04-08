using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class RaidElementOptionConfig : ConfigBase
{
        /// <summary>
        /// Ӱ������ð�ռ���
        /// </summary>
        public int adventure_skill;
        /// <summary>
        /// Ӱ��ɼ��Ե�ð�ռ���
        /// </summary>
        public int adventure_skill_appear;
        /// <summary>
        /// Ӱ��ѡ���ı���ð�ռ���
        /// </summary>
        public int adventure_skill_translate;
        public string adventure_skill_effect;
        public string error_hint;
        public List<int> next_option;
        public int use_article_way;
        public List<int> article_type;
        public string option_effect;
        public string option_name;
        public int choose_once;
        public int option_effect_disappear;
        public int pre_option_effect;
        public int is_back;
        public int is_open_next_door;
        public RaidElementOptionConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
