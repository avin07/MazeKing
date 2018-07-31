using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class BattlePositionConfig : ConfigBase
{
        public List<int> player_dot;
        public List<int> monster_dot;

        public BattlePositionConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
