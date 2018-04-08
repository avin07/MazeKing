using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class BrickElevationConfig : ConfigBase
{
        public int req_level;
        public int elevation_group;
        List<int> model_list;
        public string brick_clean_effect;
        public int time;

        public BrickElevationConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                string temp = "";
                XMLPARSE_METHOD.GetNodeInnerText(child, "model_list", ref temp, "");
                XMLPARSE_METHOD.GetNodeInnerInt(child, "req_level", ref req_level, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "elevation_group", ref elevation_group, 0);
                XMLPARSE_METHOD.GetNodeInnerText(child, "brick_clean_effect", ref brick_clean_effect, "");
                XMLPARSE_METHOD.GetNodeInnerInt(child, "time", ref time, 0);
                model_list = temp.ToList<int>(',', (s) => int.Parse(s));
        }

        public int GetBrickModel(int index)
        {
                if (model_list.Count >= index)
                {
                        return model_list[index - 1];
                }
                else
                {
                        return 0;
                }
        }

}
