using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

[System.Serializable]
public class RaidInfoDict : SerializeDict<int, RaidInfoHold> { }

[System.Serializable]
public class raid_info : CommonScriptableObject
{
        public RaidInfoDict m_Dict = new RaidInfoDict();
        public override void LoadAll(string configName)
        {
                ConfigHoldUtility<RaidInfoHold>.PreLoadXml("Config/" + configName, m_Dict);
        }
}

[System.Serializable]
public class RaidInfoHold : ConfigBase
{
        public int battle_hero_limit;
        public int carry_item_limit;
        public int raid_level;
        public int type;
        //public string info_npc_image;    //��ʾnpcͼƬ
        public string info_dialog;       //��ʾnpc����
        public int cost_vitality;        //ʳ������
        //public string base_icon;         //����
        public string house_icon;        //���Ӽ���
        public int raid_task_id;
        public int formation_id;
        public int camp_number;
        public int area_goal_point;      //�ɻ�õ�dp��
        public List<int> related_task;   //�Թ����������������ʾ�Թ�����׷��

        public RaidInfoHold(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }
}

public enum RAID_TYPE // ��������
{
    NORMAL = 1, // ��ͨ����
    GUIDE = 3, // ��ѧ����
    ENDLESS = 4, // ���޸���
    STAGE = 5, // �׶θ���

    NPC_VILLAGE = 10, // NPC��ׯ   
    EVENT = 20, // �¼�
    TASK = 30,  // ����

}

