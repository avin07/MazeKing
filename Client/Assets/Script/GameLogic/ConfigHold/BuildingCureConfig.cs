using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System;

public class BuildingCureConfig : ConfigBase
{
        public Dictionary<string, string> m_BuildingCure = new Dictionary<string, string>(); 
        public BuildingCureConfig(XmlNode child): base(child)
        {
                
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
                m_BuildingCure = XMLPARSE_METHOD.GetXMLContent(child);
        }

        public void GetCurePressureInfo(int pressure_value,out string cost_resource, out int cost_time)
        {
                string cure_pressure_interval = GlobalParams.GetString("cure_pressure_interval"); //左开右闭
                string[] interval = cure_pressure_interval.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                int index = 0;
                for (int i = 0; i < interval.Length; i++)
                {
                        int min = int.Parse(interval[i]);
                        int max = int.Parse(interval[i + 1]);
                        if (pressure_value > min && pressure_value <= max)
                        {
                                index = i + 1;
                                break;
                        }
                }
                cost_resource = m_BuildingCure["cost_resource_" + index];
                cost_time = int.Parse(m_BuildingCure["cost_time_" + index]);

        }
}
