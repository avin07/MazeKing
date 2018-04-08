using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class GuideConfig : ConfigBase
{
        public int home_level;                  //需求等级
        public string display_interface;        //窗口
        public string target_prompt;            //目标文字
        public string operate_prompt;           //操作文字
        public string operate_prompt_position;  //操作文字位置
        public int arrow_direction;             //箭头方向
        public string point_at;                 //指向ui
        public int next;                        //下一个引导
        public int pre_step;                      //前一步id
        public int raid_piece_id;
        public int common_element_id;
        public int type;        //2ui 1迷宫元素 3家园招募型NPC 4 世界地图
        public int pre_guide;           //前置引导id，表示这个引导的开启条件
        public int trigger_guide;           //前置引导id，表示这个引导的开启条件
        public GuideConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
