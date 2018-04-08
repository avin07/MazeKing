using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class BuildingMakeFormulaHold : ConfigBase
{
        public string require;
        public string output;
        public int class_id;
        public int furniture_id;
        public int is_int_have;
        public string name;
        public int make_time;

        public BuildingMakeFormulaHold(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
