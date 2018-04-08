using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class SkillTriggerConfig : ConfigBase
{       
        /// <summary>
        /// 0,������������
        /// 1,�������ͷ���
        /// 2,�Դ����������߶��Ե��ѷ�
        /// 3,�Դ����������߶��Եĵз�
        /// 4,�Դ����������߶��Եĵз�ǰ��
        /// 5,�Դ����������߶��Եĵз�����
        /// 6,�Դ����������߶��Ե��ѷ�ͬ��
        /// </summary>
        public int target;
        
        /// <summary>
        /// 0,����
        /// 1,����
        /// 2,��ɱ��λ
        /// 3,�ж�ǰ
        /// 4,�ж���
        /// 5,�ܵ��˺�
        /// 6,�ܵ�����
        /// 7,��������
        /// </summary>
        public int trigger_action;
        
        /// <summary>
        /// 0,������������
        /// 1,�������ͷ���
        /// 2,��������
        /// </summary>
        public int trigger_skill_releaser;
        
        public int trigger_skill_id;
        public int trigger_rate;
        public int is_repeat;
        public SkillTriggerConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
