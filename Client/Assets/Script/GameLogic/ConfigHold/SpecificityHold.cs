using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class SpecificityHold : ConfigBase
{
        public int type;
        public int good_or_bad;
        public int passive_skill;
        public string require;
        public string trigger_speak;
        public string name;
        public string desc;
        public int result_type;
        public int quality;
        public int furniture_id;
        public string remove_cost;
        public int remove_cost_time;

        public SpecificityHold(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}

public enum Specificity_Type  //特性种类
{
    GOOD_RANDOM, //正面
    BAD_RANDOM, //反面
    ALL,

}


public enum Specificity_State  //是否上锁
{
        UNLOCK,         //未上锁
        LOCK,           //上锁
        MAX,

}
