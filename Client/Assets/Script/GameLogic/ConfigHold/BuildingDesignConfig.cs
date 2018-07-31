using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class BuildingDesignConfig : ConfigBase
{
        public string unlock_furniture_list;   //����ҵȼ������Ŀɽ���Ľ���
        public int overall_capacity_limit;

        public BuildingDesignConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
