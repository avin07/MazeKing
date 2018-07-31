using UnityEngine;
using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

public class MonsterTeamConfig : ConfigBase
{
        public string monsterlist;
        public MonsterTeamConfig(XmlNode child)
                : base(child)
        {
                XMLPARSE_METHOD.GetNodeInnerText(child, "monster_list", ref monsterlist, "");
        }
}