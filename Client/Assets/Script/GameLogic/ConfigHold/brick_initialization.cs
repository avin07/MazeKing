using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;


[System.Serializable]
public class BrickInitDict : SerializeDict<int, BrickInitializationConfig> { }

[System.Serializable]
public class brick_initialization : CommonScriptableObject
{
        public BrickInitDict m_Dict = new BrickInitDict();
        public override void LoadAll(string configName)
        {
                ConfigHoldUtility<BrickInitializationConfig>.PreLoadXml("Config/" + configName, m_Dict);
        }
}

[System.Serializable]
public class BrickInitializationConfig : ConfigBase
{
        public int elevation;
        public int area;
        public string use_brick;  //¸ß¶È,Ìæ»»µÄ×©¿é
        public int brick_number;
        public int is_square;

        public BrickInitializationConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
