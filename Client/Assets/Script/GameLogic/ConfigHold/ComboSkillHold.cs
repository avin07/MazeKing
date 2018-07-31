using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
public class ComboSkill : ConfigBase
{
        public int req_combo_condition;
        public int combo_type;
        public int combo_ratio;
        public int attack_multiplier;
        public int actionid;
        public string effect;
        public ComboSkill(XmlNode child) : base(child)
        {
                XMLPARSE_METHOD.GetNodeInnerInt(child, "req_combo_condition", ref req_combo_condition, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "attack_multiplier", ref attack_multiplier, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "action_id", ref actionid, 0);
                XMLPARSE_METHOD.GetNodeInnerText(child, "effect", ref effect, "");
                string tmp = "";
                XMLPARSE_METHOD.GetNodeInnerText(child, "add_combo_condition", ref tmp, "");
                if (tmp.Contains(","))
                {
                        string[] tmps = tmp.Split(',');
                        if (tmps.Length == 2)
                        {
                                int.TryParse(tmps[0], out combo_type);
                                int.TryParse(tmps[1], out combo_ratio);
                        }
                }
        }
}
