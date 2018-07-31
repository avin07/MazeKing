using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class RaidSceneConfig : ConfigBase
{
        //public int floor_height;
        public Vector3 scene_offset;
        public string scene_brick;
        public List<List<int>> model_list = new List<List<int>>();
        public string ornament;
        public RaidSceneConfig(XmlNode child) : base(child)
        {
                //XMLPARSE_METHOD.GetNodeInnerInt(child, "floor_height", ref floor_height, 0);
                XMLPARSE_METHOD.GetNodeInnerVec3(child, "scene_offset", ref scene_offset, Vector3.zero);
                XMLPARSE_METHOD.GetNodeInnerText(child, "scene_brick", ref scene_brick, "");
                XMLPARSE_METHOD.GetNodeInnerText(child, "ornament", ref ornament, "");
                for (int i = 1; i <= 12; i++)
                {
                        model_list.Add(XMLPARSE_METHOD.GetNodeInnerIntList(child, "model_list_" + i, ','));
                }
        }

}
