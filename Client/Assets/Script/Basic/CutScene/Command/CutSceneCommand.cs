using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class CutSceneCommand
{
        /// <summary>
        /// 索引，用于跳命令
        /// </summary>
        public int index;
        /// <summary>
        /// 下一条命令索引
        /// </summary>
        public int next_index;
        /// <summary>
        /// Camera: MoveTo, Follow, Rotate,
        /// Unit: Create, MoveTo, RotateTo, PlayAnim, 
        /// Dialog: Aside（旁白），Speak
        /// Effect：Play
        /// </summary>
        public string cmd_name;

        public bool isWait;

        public CutSceneCommand(XmlNode node)
        {
                cmd_name = node.Name;

                XMLPARSE_METHOD.GetAttrValueInt(node, "index", ref index, 0);
                XMLPARSE_METHOD.GetAttrValueInt(node, "next_index", ref next_index, 0);
                XMLPARSE_METHOD.GetAttrValueBool(node, "wait", ref isWait, true);
        }
        public virtual void Exec()
        {

        }
}

public class CutSceneCmd_TimeScale : CutSceneCommand
{
        public float scale;
        public CutSceneCmd_TimeScale(XmlNode node)
                : base(node)
        {
                XMLPARSE_METHOD.GetAttrValueFloat(node, "scale", ref scale, 1f);
        }
        public override void Exec()
        {
                base.Exec();
                Time.timeScale = scale;
                CutSceneManager.GetInst().EndWaiting();
                Debuger.Log("timescale=" + scale);
        }
}
public class CutSceneCmd_Wait : CutSceneCommand
{
        public float time;
        public CutSceneCmd_Wait(XmlNode node)
                : base(node)
        {
                XMLPARSE_METHOD.GetAttrValueFloat(node, "time", ref time, 0f);
        }
        public override void Exec()
        {
                base.Exec();
                CutSceneManager.GetInst().SetWaitTime(time);
        }
}
public class CutSceneCmd_Quit : CutSceneCommand
{
        public int result;
        public CutSceneCmd_Quit(XmlNode node)
                : base(node)
        {
                XMLPARSE_METHOD.GetAttrValueInt(node, "result", ref result, 0);
        }
        public override void Exec()
        {
                base.Exec();
                CutSceneManager.GetInst().Quit(result);
        }
}

public class CutSceneCmd_Texture : CutSceneCommand
{
        public string name;
        public Vector2 pos_start;
        public Vector2 pos_end;
        public float show_time;
        public float exist_time;
        public CutSceneCmd_Texture(XmlNode node)
                : base(node)
        {
                XMLPARSE_METHOD.GetAttrValueStr(node, "name", ref name, "");
                XMLPARSE_METHOD.GetAttrValueVector2(node, "pos_start", ref pos_start, Vector2.zero);
                XMLPARSE_METHOD.GetAttrValueVector2(node, "pos_end", ref pos_end, Vector2.zero);
                XMLPARSE_METHOD.GetAttrValueFloat(node, "show_time", ref show_time, 0f);
                XMLPARSE_METHOD.GetAttrValueFloat(node, "exist_time", ref exist_time, 0f);
        }
        
        public override void Exec()
        {
                base.Exec();
                UI_CutSceneDialog uis = UIManager.GetInst().GetUIBehaviour<UI_CutSceneDialog>();
                if (uis == null)
                {
                        uis = UIManager.GetInst().ShowUI<UI_CutSceneDialog>("UI_CutSceneDialog");
                }
                uis.ShowImage(name, pos_start, pos_end, show_time, exist_time);
                if (this.isWait)
                {
                        CutSceneManager.GetInst().SetWaitTime(show_time+exist_time);
                }
                else
                {
                        CutSceneManager.GetInst().EndWaiting();
                }
        }
}