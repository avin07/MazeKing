using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

[System.Serializable]
public class RaidFloorConfig : ConfigBase
{
        public int type;
        public string mark;
        public List<int> model;
        public int result_model;
        public string minimap_icon;
        public string adventure_skill_id;
        public string result_resource;
        public int is_stop;
        public int is_result_stop;
        public string walk_effect;
        public int step_trap_id;
        public string team_attribute_id;
        public int result_null_weight;
        public int result_drop_weight;
        public int result_drop_id;
        public int result_trap_weight;
        public int result_trap_id;
        public string find_drop_effect;
        public int theme;
        public int is_perception;

        public int mainModel
        {
                get
                {
                        if (model.Count > 0)
                        {
                                return model[0];
                        }
                        return 0;
                }
        }

        public bool IsPerception()
        {
                return is_perception == 1;
        }


        public RaidFloorConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
