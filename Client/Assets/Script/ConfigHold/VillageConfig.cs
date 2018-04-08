using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class VillageConfig : ConfigBase
{
        public int type;
        public int hero_limit;
        public int formation_id;
        public string building_position;
        public string village_brick;
        public string charactar_element;
        public List<List<int>> model_list = new List<List<int>>();
        public int fight_position;
        public List<int> player_position;
        public string village_name;
        public string village_icon;
        public VillageConfig(XmlNode child) : base(child)
        {
                XMLPARSE_METHOD.GetNodeInnerInt(child, "type", ref type, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "hero_limit", ref hero_limit, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "formation_id", ref formation_id, 0);
                XMLPARSE_METHOD.GetNodeInnerInt(child, "fight_position", ref fight_position, 0);

                player_position = XMLPARSE_METHOD.GetNodeInnerIntList(child, "player_position", ',');

                XMLPARSE_METHOD.GetNodeInnerText(child, "building_position", ref building_position, "");
                XMLPARSE_METHOD.GetNodeInnerText(child, "village_brick", ref village_brick, "");
                XMLPARSE_METHOD.GetNodeInnerText(child, "charactar_element", ref charactar_element, "");
                XMLPARSE_METHOD.GetNodeInnerText(child, "village_name", ref village_name, "");
                XMLPARSE_METHOD.GetNodeInnerText(child, "village_icon", ref village_icon, "");
                for (int i = 1; i <= 12; i++)
                {
                        model_list.Add(XMLPARSE_METHOD.GetNodeInnerIntList(child, "model_list_" + i, ','));
                }
        }
}
