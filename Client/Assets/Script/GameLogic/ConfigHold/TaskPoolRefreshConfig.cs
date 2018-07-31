using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class TaskPoolRefreshConfig : ConfigBase
{
        public int refresh_price;

        public TaskPoolRefreshConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
