using UnityEngine;
using System.Collections;
using System.Xml;

public class ItemConfig : ConfigBase
{
        public string name;
        public string icon;
        public string desc;
        public string type_icon;

        public int type;
        public int quality;
        public int stackable;
        public int value;
        public int is_use;
        public int use_place;
        public int camp_skill_point;
        public int priority;

        public ItemConfig(XmlNode child) : base(child)
        {                
        }
        
        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }
}
