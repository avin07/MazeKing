using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class ModelConfig : ConfigBase
{
        public string mark;
        public string model_resource;
        public string material_resource;
        public string icon_resource;
        public List<string> actionids;
        public List<string> actionTimes;
        public float scale;

        public int material_index;
        public int GetPriority()
        {
                int priority = 0;
                if (material_resource.Contains("_"))
                {
                        int.TryParse(material_resource.Substring(material_resource.LastIndexOf("_") + 1), out priority);
                }
                return priority;
        }

        public ModelConfig(XmlNode child)
                : base(child)
        {
                XMLPARSE_METHOD.GetNodeInnerText(child, "model_resource", ref model_resource, "");
                string matres = "";
                XMLPARSE_METHOD.GetNodeInnerText(child, "material_resource", ref matres, "");
                if (matres.Contains("#"))
                {
                        string[] tmps = matres.Split('#');
                        int.TryParse(tmps[1], out material_index);
                        material_resource = tmps[0];
                }
                else
                {
                        material_index = 0;
                        material_resource = matres;
                }

                XMLPARSE_METHOD.GetNodeInnerText(child, "icon_resource", ref icon_resource, "");
                XMLPARSE_METHOD.GetNodeInnerText(child, "mark", ref mark, "");
                string hitTime = "";
                actionTimes = new List<string>();
                XMLPARSE_METHOD.GetNodeInnerText(child, "time_before_hit_1", ref hitTime, "");
                actionTimes.Add(hitTime);
                XMLPARSE_METHOD.GetNodeInnerText(child, "time_before_hit_2", ref hitTime, "");
                actionTimes.Add(hitTime);
                XMLPARSE_METHOD.GetNodeInnerText(child, "time_before_hit_3", ref hitTime, "");
                actionTimes.Add(hitTime);

                actionids = new List<string>();
                string tmpid = "";
                XMLPARSE_METHOD.GetNodeInnerText(child, "action_id_1", ref tmpid, "");
                actionids.Add(tmpid);
                XMLPARSE_METHOD.GetNodeInnerText(child, "action_id_2", ref tmpid, "");
                actionids.Add(tmpid);
                XMLPARSE_METHOD.GetNodeInnerText(child, "action_id_3", ref tmpid, "");
                actionids.Add(tmpid);
                XMLPARSE_METHOD.GetNodeInnerFloat(child, "scale", ref scale, 1f);                
        }
}
