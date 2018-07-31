using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class SkillTriggerConfig : ConfigBase
{       
        /// <summary>
        /// 0,触发器挂载者
        /// 1,触发器释放者
        /// 2,对触发器挂载者而言的友方
        /// 3,对触发器挂载者而言的敌方
        /// 4,对触发器挂载者而言的敌方前排
        /// 5,对触发器挂载者而言的敌方后排
        /// 6,对触发器挂载者而言的友方同排
        /// </summary>
        public int target;
        
        /// <summary>
        /// 0,开场
        /// 1,死亡
        /// 2,击杀单位
        /// 3,行动前
        /// 4,行动后
        /// 5,受到伤害
        /// 6,受到治疗
        /// 7,攻击暴击
        /// </summary>
        public int trigger_action;
        
        /// <summary>
        /// 0,触发器挂载者
        /// 1,触发器释放者
        /// 2,触发对象
        /// </summary>
        public int trigger_skill_releaser;
        
        public int trigger_skill_id;
        public int trigger_rate;
        public int is_repeat;
        public SkillTriggerConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
