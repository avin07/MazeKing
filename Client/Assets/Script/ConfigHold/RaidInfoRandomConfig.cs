using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

[System.Serializable]
public class RaidInfoRandomConfig : ConfigBase
{
        public int sizex;
        public int sizey;
        public int type;
        public int is_random;
        public int floor_height;      //µÿ√Ê∫£∞Œ
        public int door_state;
        public RaidInfoRandomConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
