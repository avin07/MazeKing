using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class BuildingMakeFormulaClassHold : ConfigBase
{
        public string name;

        public BuildingMakeFormulaClassHold(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}