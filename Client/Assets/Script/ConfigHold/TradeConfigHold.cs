using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class TradeConfig : ConfigBase
{
        public int bag_type;
        public string own_trade_type;
        public int target_drop_pool;
        public string sell_special_value_per;
        public string buy_special_value_per;
        public int sell_value_per;
        public int buy_value_per;
        public int number_limited;

        public TradeConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
