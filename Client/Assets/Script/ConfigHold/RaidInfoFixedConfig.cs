using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

[System.Serializable]
public class RaidInfoFixedConfig : ConfigBase
{
        public int sizex;
        public int sizey;
        public int entrance;
        public int exit;
        public Vector3 CameraPos;
        public RaidInfoFixedConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }
}
