using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class CharactarAchievementConfig : ConfigBase
{
        public int series_id;
        public string name;
        public string illustration;
        public string tag;
        public string describe;
        public string biography;
        public int max_star;
        public int current_star;
        public int is_global;
        public int target_type;
        public string target_para_list;
        public string reward;

        public CharactarAchievementConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}


public enum ACHIEVE_TYPE
{
        LEVEL_UP = 1, // ���� 
        STAR_UP = 2, // ����
        SKILL_ONE_UP = 3, // ���⼼�ܴﵽָ���ȼ�

        TRANSFER = 10, // תְ
        SKILL_SPECIFY_UP = 11, // ����ָ������
        BEHAVIOR_GET = 12, // ָ�����Ի�ȡ
        BEHAVIOR_COUNT = 13, // ����������ȡ
        RAID_COMPLETE_SELF = 14, // �Լ����ָ������
        RAID_COMPLETE = 15, // ��ָ��������ָ������
}

