using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class BuildNodeStruct
{
        public int id;
        public List<int> floorlist = new List<int>();   //砖块模型id表:modelId,modelId,
        public BuildNodeStruct(XmlNode node)
        {
                XMLPARSE_METHOD.GetAttrValueInt(node, "id", ref id, 0);
                XMLPARSE_METHOD.GetAttrValueListInt(node, "floorlist", ref floorlist);
        }
}

public class BuildNodeConfig : ConfigBase
{
        public List<int> buildNodePos = new List<int>();  //每层的位置
        public Dictionary<int, List<int>> m_buildNodeDict = new Dictionary<int, List<int>>(); //每层的砖
        public Dictionary<int, int> m_buildHeight = new Dictionary<int, int>();

        public BuildNodeConfig(XmlNode child)
        {
                XMLPARSE_METHOD.GetAttrValueInt(child, "id", ref id, 0);
                int max_height = 0;
                List<BuildNodeStruct> list = new List<BuildNodeStruct>();
                foreach (XmlNode node in child.ChildNodes)
                {
                        BuildNodeStruct bns = new BuildNodeStruct(node);
                        list.Add(bns);
                        if (bns.floorlist.Count > max_height)
                        {
                                max_height = bns.floorlist.Count;
                        }
                        m_buildHeight.Add(bns.id, bns.floorlist.Count);
                }
                list.Sort(CompareBuildNode);

                for (int i = 0; i < list.Count; i++)
                {
                        buildNodePos.Add(list[i].id);
                        List<int> modellist = list[i].floorlist;
                        for (int j = 0; j < max_height; j++)
                        {
                                if (!m_buildNodeDict.ContainsKey(j))
                                {
                                        m_buildNodeDict.Add(j, new List<int>());
                                }

                                if (j < modellist.Count)
                                {
                                        m_buildNodeDict[j].Add(modellist[j]);
                                }
                                else
                                {
                                        m_buildNodeDict[j].Add(0);
                                }
                        }
                }
        }

        protected int CompareBuildNode(BuildNodeStruct nodea, BuildNodeStruct nodeb)
        {
                if (null == nodea || null == nodeb)
                {
                        return -1;
                }

                int ida = nodea.id;
                int idb = nodeb.id;

                return ida - idb;             
        }

        public override void InitSelf(XmlNode child)
        {
        }
}
