using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public enum BATTLE_PRESSURE_TYPE
{
        NONE,
        BE_CRITICAL_HIT,
        BE_KILLED,
        CRITICAL_HIT,
        KILLED,

        MAX
};

public class BattlePressureConfig : ConfigBase
{
        
        /// <summary>
        /// 1,±»±©»÷
        ///2,±»»÷É±
        ///3,±©»÷
        ///4,»÷É±
        /// </summary>
        public int type;

        /// <summary>
        /// 1 ×Ô¼º
        /// 2 ¶ÓÓÑ
        /// </summary>
        public int range_type;
        public int rate;
        public int min_pressure;
        public int max_pressure;

        public BattlePressureConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
