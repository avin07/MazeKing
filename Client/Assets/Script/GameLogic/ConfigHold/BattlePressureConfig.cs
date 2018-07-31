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
        /// 1,������
        ///2,����ɱ
        ///3,����
        ///4,��ɱ
        /// </summary>
        public int type;

        /// <summary>
        /// 1 �Լ�
        /// 2 ����
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
