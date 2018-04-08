using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

[System.Serializable]
public class RaidMapDict : SerializeDict<int, RaidMapHold> { }

[System.Serializable]
public class raid_map : CommonScriptableObject
{
        public RaidMapDict m_Dict = new RaidMapDict();
        public override void LoadAll(string configName)
        {
                ConfigHoldUtility<RaidMapHold>.PreLoadXml("Config/" + configName, m_Dict);
        }
}

[System.Serializable]
public class RaidMapHold : ConfigBase
{
        public int raid;
        public int floor;
        public int map_id;
        public int is_jump_floor;
        public int raid_task_id;

        public string name;
        public string desc;
        public string adventure_keyword;
        public string monster;
        public string monster_boss;
        public string reward;
        public int scene_resid;
        public int scene_cfgid;
        public int size;
        public RaidMapHold(XmlNode child) : base(child)
        {
                XMLPARSE_METHOD.GetNodeInnerInt(child, "raid", ref raid, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "floor", ref floor, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "map_id", ref map_id, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "is_jump_floor", ref is_jump_floor, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "raid_task_id", ref raid_task_id, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "size", ref size, 0);
                
                XMLPARSE_METHOD.GetNodeInnerText(child, "name", ref name, "");
                XMLPARSE_METHOD.GetNodeInnerText(child, "desc", ref desc, "");
                XMLPARSE_METHOD.GetNodeInnerText(child, "adventure_keyword", ref adventure_keyword, "");
                XMLPARSE_METHOD.GetNodeInnerText(child, "monster", ref monster, "");
                XMLPARSE_METHOD.GetNodeInnerText(child, "monster_boss", ref monster_boss, "");
                XMLPARSE_METHOD.GetNodeInnerText(child, "reward", ref reward, "");
                XMLPARSE_METHOD.GetNodeInnerInt(child, "scene_resource", ref scene_resid, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "scene_config", ref scene_cfgid, 0);
        }
}
