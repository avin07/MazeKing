using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class FurnitureSetConfig : ConfigBase
{
        public string furniture_list;
        public int unlock_main_function;
        public int para;
        public string add_minor_function;
        public string name;
        public string describe;
        public int s_id;
        public int level;


        public FurnitureSetConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
