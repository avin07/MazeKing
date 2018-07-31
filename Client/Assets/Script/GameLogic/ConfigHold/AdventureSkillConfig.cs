using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class AdventureSkillConfig : ConfigBase
{
        /// <summary>
        /// 0һ�� 1��� 2��֪ 3�������� 4�ɼ�
        /// </summary>
        public int type;
        public string result_action;
        public string result_effect;
        public float action_time;
        public float action_accelerate_per;
        public List<int> effect_element_type;
        public AdventureSkillConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }
}

public enum ADV_SKILL_TYPE
{
        NORMAL,         //һ��ͨ����
        BURN,                   //����
        LIGHT,                  //������
        TRANSLATE,          //����
        BOAT,                   //��ľ��
        SENSE,                  //��֪
        LONG_TAKE,      //����ȡ��
        UNARMED,                  //ͽ��ϵ�У����ü��Ӣ�����ϴ�û������������unarmd_trap_effect
}
