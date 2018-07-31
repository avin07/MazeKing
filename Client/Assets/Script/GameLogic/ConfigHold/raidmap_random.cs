using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

[System.Serializable]
public class RaidRandomDict : SerializeDict<int, RaidNodeConfig> { }

[System.Serializable]
public class raidmap_random : CommonScriptableObject
{
        public RaidRandomDict m_Dict = new RaidRandomDict();
        public override void LoadAll(string configName)
        {
                ConfigHoldUtility<RaidNodeConfig>.PreLoadXml("Config/" + configName, m_Dict);
        }
}

[System.Serializable]
public class RaidNodeStruct
{
        public int id;
        public int elem_id;
        public int elem_rot;
        public int elem_posy;
        public int group_id;
        public List<int> floorlist = new List<int>();   //砖块模型id表:modelId,modelId,
        public RaidNodeStruct(XmlNode node)
        {
                XMLPARSE_METHOD.GetAttrValueInt(node, "id", ref id, 0);
                //XMLPARSE_METHOD.GetAttrValueInt(node, "floor_id", ref floor_id, 0);
                XMLPARSE_METHOD.GetAttrValueInt(node, "elem_id", ref elem_id, 0);
                XMLPARSE_METHOD.GetAttrValueInt(node, "elem_rot", ref elem_rot, 0);
                XMLPARSE_METHOD.GetAttrValueInt(node, "elem_posy", ref elem_posy, 0);
                XMLPARSE_METHOD.GetAttrValueInt(node, "group_id", ref group_id, 0);
                XMLPARSE_METHOD.GetAttrValueListInt(node, "floorlist", ref floorlist);
        }
}
[System.Serializable]
public class RaidNodeConfig : ConfigBase
{
        public List<RaidNodeStruct> list = new List<RaidNodeStruct>();
        int CompareNode(RaidNodeStruct node0, RaidNodeStruct node1)
        {
                return node0.id.CompareTo(node1.id);
        }
        public RaidNodeConfig(XmlNode child)
        {
                XMLPARSE_METHOD.GetAttrValueInt(child, "id", ref id, 0);
                foreach(XmlNode node in child.ChildNodes)
                {
                        list.Add(new RaidNodeStruct(node));
                }
                list.Sort(CompareNode);
        }

        public override void InitSelf(XmlNode child)
        {
        }
}
