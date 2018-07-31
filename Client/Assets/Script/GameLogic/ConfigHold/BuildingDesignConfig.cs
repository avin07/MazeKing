using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class BuildingDesignConfig : ConfigBase
{
        public string unlock_furniture_list;   //设计室等级解锁的可建造的建筑
        public int overall_capacity_limit;

        public BuildingDesignConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
