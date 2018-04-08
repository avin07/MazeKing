using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class DialogConfig : ConfigBase
{
        public string text;
        public string event_picture;
        public string event_choice;
        public string choice_result_dialog;
        public string talker_name;       //说话者名字
        public string npc_head_icon;     //对话头像

        public DialogConfig(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
        }

}
