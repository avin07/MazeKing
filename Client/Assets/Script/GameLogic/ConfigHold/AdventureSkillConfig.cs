using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class AdventureSkillConfig : ConfigBase
{
        /// <summary>
        /// 0一般 1侦查 2感知 3盗贼技艺 4采集
        /// </summary>
        public int type;
        public string result_action;
        public string result_effect;
        public float action_time;
        public float action_accelerate_per;
        public List<int> effect_element_type;
        public AdventureSkillConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }
}

public enum ADV_SKILL_TYPE
{
        NORMAL,         //一般通用类
        BURN,                   //焚烧
        LIGHT,                  //照明弹
        TRANSLATE,          //传送
        BOAT,                   //扎木筏
        SENSE,                  //感知
        LONG_TAKE,      //隔空取物
        UNARMED,                  //徒手系列，不用检测英雄身上带没带，会额外造成unarmd_trap_effect
}
