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
        //public string info_npc_image;    //提示npc图片
        public string info_dialog;       //提示npc话语
        public int cost_vitality;        //食物消耗
        //public string base_icon;         //底座
        public string house_icon;        //房子激活
        public int raid_task_id;
        public int formation_id;
        public int camp_number;
        public int area_goal_point;      //可获得的dp点
        public List<int> related_task;   //迷宫中相关任务，用来显示迷宫任务追踪

        public RaidInfoHold(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }
}

public enum RAID_TYPE // 副本类型
{
    NORMAL = 1, // 普通副本
    GUIDE = 3, // 教学副本
    ENDLESS = 4, // 无限副本
    STAGE = 5, // 阶段副本

    NPC_VILLAGE = 10, // NPC村庄   
    EVENT = 20, // 事件
    TASK = 30,  // 任务

}

