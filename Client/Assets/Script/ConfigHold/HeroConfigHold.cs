using UnityEngine;
using System.Collections;
using System.Xml;
public class HeroConfigHold : ConfigBase
{
    public int characterId;
    public HeroConfigHold(XmlNode child)
        : base(child)
    {
        XMLPARSE_METHOD.GetNodeInnerInt(child, "charactar_id", ref characterId, 0);
    }

}