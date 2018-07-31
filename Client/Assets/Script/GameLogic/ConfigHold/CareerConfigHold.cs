using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class CareerConfigHold : ConfigBase
{
    public string name;
    public string icon;

    public int race;
    public int color;

    public int growth_atk;
    public int growth_hp;
    public int growth_mdef;
    public int growth_pdef;
    public int growth_spp;

    public int init_atk;
    public int init_hp;
    public int init_mdef;
    public int init_pdef;
    public int init_spd;

    public int max_atk;
    public int max_hp;
    public int max_mdef;
    public int max_pdef;
    public int max_spd;

    public CareerConfigHold(XmlNode child) : base(child)
    {
    }

    public override void InitSelf(XmlNode child)
    {
        SetupFields(child);
    }

}
