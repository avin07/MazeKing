using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class BuildingInfoHold : ConfigBase
{
        public string name;
        public string icon;
        public string desc;
        public int cost_time;                   //消耗时间
        public string cost_material;            //消耗材料
        public string clean_cost;               //清理花费    
        public string clean_restore;            //清理返还
        public string model;                    //模型   
        public int add_reputation;              //增加人气
        public string minor_function;

        //给林杰的npc村庄用的
        public int size_x;
        public int size_y;  

        
        public BuildingInfoHold(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
                
        }

}
