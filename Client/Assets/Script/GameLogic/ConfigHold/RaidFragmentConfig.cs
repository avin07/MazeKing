using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class RaidFragmentConfig : ConfigBase
{
        public List<int> floorList;
        public List<int> elemList;

        public RaidFragmentConfig(XmlNode child) : base(child)
        {
                floorList = new List<int>();
                elemList = new List<int>();
                for (int i = 1; i <= 13; i++)
                {
                        int tmpFloorId = 0;
                        XMLPARSE_METHOD.GetNodeInnerInt(child, "roadbed_change_" + i, ref tmpFloorId, 0);
                        floorList.Add(tmpFloorId);

                        int tmpElemId = 0;
                        XMLPARSE_METHOD.GetNodeInnerInt(child, "element_change_" + i, ref tmpElemId, 0);
                        elemList.Add(tmpElemId);
                }
        }
}
