using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System;

[System.Serializable]
public class RaidElemConfigDict : SerializeDict<int, RaidElemConfig>
{
}

[System.Serializable]
public class raid_common_element : CommonScriptableObject
{
        public RaidElemConfigDict m_Dict = new RaidElemConfigDict();
        public override void LoadAll(string configName)
        {
                ConfigHoldUtility<RaidElemConfig>.PreLoadXml("Config/" + configName, m_Dict);
        }

}

[System.Serializable]
public class RaidElemConfig : ConfigBase
{
        public int type;
        public string name;
        public string desc;
        public string operation_desc;
        public string mark;

        public List<int> model;
        public string minimap_icon;
        public string operation_icon;
        public float operation_time;
        public float icon_height;
        public string player_operation_action;
        public string operation_action;
        public string operation_effect;
        public string result_action;
        public string result_sound;
        public string result_effect;
        
        public Vector3 size;

        public int is_direct_result;
        public int is_ask_executor;
        public string need_tool_id;
        public int is_stop;
        public int is_result_stop;
        public int result_number;
        public int right_adventure_skill;
        public int raid_task_id;
        public string before_aside;
        public string finish_aside;
        public int is_open_door_trigger;
        public List<int> option_client;       //选择元素的选项id序列
        public int room_finish_disappear;       //是否完成房间后消失
        public int brightness_limit;    //光照小于该值时不可操作
        public List<int> unlock_building;
        public int mainModel
        {
                get
                {
                        if (model.Count > 0)
                        {
                                return model[0];
                        }
                        return 0;
                }
        }

        public RaidElemConfig(XmlNode child)
                : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

        #region FUNCTION

        public bool CanShow()
        {
                return type != (int)RAID_ELEMENT_TYPE.HIDE_TRAP &&
                        //type != (int)RAID_ELEMENT_TYPE.RANGE_TRIGGER &&
                            type != (int)RAID_ELEMENT_TYPE.ENTRANCE &&
                            type != (int)RAID_ELEMENT_TYPE.STORY &&
                            type != (int)RAID_ELEMENT_TYPE.MONSTER &&
                            type != (int)RAID_ELEMENT_TYPE.EDITOR_SHOW;
        }
        public bool IsDoor()
        {
                return type == (int)RAID_ELEMENT_TYPE.DOOR
                        || type == (int)RAID_ELEMENT_TYPE.DOOR_LOCKED
                        || type == (int)RAID_ELEMENT_TYPE.DOOR_SPECIAL;
        }
        public bool IsCharacter()
        {
                return type == (int)RAID_ELEMENT_TYPE.MONSTER ||
                                type == (int)RAID_ELEMENT_TYPE.STORY_NPC ||
                                type == (int)RAID_ELEMENT_TYPE.TRADE_NPC ||
                                type == (int)RAID_ELEMENT_TYPE.OPTION_NPC ||
                                type == (int)RAID_ELEMENT_TYPE.NEEDHELP_NPC ||
                                type == (int)RAID_ELEMENT_TYPE.BRANCH_NPC ||
                                type == (int)RAID_ELEMENT_TYPE.SUPPORTER;
        }
        #endregion

}

public enum RAID_FLOOR_TYPE
{
        NONE,
        NORMAL,
        DIG,
        DEPRESSED,
        WATER,
        RANDOM_ELEM,
}
public enum RAID_ELEMENT_TYPE
{
        NONE,
        DOOR_SPECIAL,           //1,特殊门
        DOOR,                           // 2,门
        DOOR_LOCKED,            // 3,带锁的门
        BARRIER,                        // 4,路障
        TREASURE,                       // 5,普通宝箱
        TREASURE_LOCKED,        // 6,带锁宝箱
        TREASURE_MONSTER,       // 7,宝箱怪物
        COLLECTION,                     // 8,采集物
        SKELETON,                       // 9,尸骨
        TRAP,                           // 10,陷阱
        RANDOM_EVENT,           // 11,随机事件
        TELEPORT,                       // 12,传送通道元素
        EXIT,                                   // 13,出口
        ENTRANCE,                       //14,入口
        MONSTER,                        //15守房怪
        CLUE,                                   //密码线索元素
        DECORATION,                     //17装饰性元素
        HIDE_TRAP,                      //暗的陷阱
        STORY,                          //过场动画
        STORY_NPC,              //剧情NPC
        TRADE_NPC,              //交易NPC
        DICE,                           //骰子
        UNLOCK_BUILDING,              //用来解锁建筑的元素
        BEDROOM,                //卧室
        EDITOR_SHOW,            //编辑器需要显示的类型
        OPTION_NPC,              //有选项框的NPC
        NEEDHELP_NPC,       //需求帮助的NPC
        BRANCH_NPC,             //支线任务NPC
        ALCHEMY,                        //炼金炉
        TRIGGER,                        //机关
        SUPPORTER,              //支援者
};
