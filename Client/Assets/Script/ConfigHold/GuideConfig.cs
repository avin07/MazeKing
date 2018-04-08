using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class GuideConfig : ConfigBase
{
        public int home_level;                  //����ȼ�
        public string display_interface;        //����
        public string target_prompt;            //Ŀ������
        public string operate_prompt;           //��������
        public string operate_prompt_position;  //��������λ��
        public int arrow_direction;             //��ͷ����
        public string point_at;                 //ָ��ui
        public int next;                        //��һ������
        public int pre_step;                      //ǰһ��id
        public int raid_piece_id;
        public int common_element_id;
        public int type;        //2ui 1�Թ�Ԫ�� 3��԰��ļ��NPC 4 �����ͼ
        public int pre_guide;           //ǰ������id����ʾ��������Ŀ�������
        public int trigger_guide;           //ǰ������id����ʾ��������Ŀ�������
        public GuideConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
