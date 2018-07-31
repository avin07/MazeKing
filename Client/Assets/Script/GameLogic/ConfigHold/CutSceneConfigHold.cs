using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class CutSceneConfig : ConfigBase
{
        public List<CutSceneCommand> cmdlist;
        public CutSceneConfig(XmlNode node)
        {
                if (node.Attributes != null)
                {
                        int.TryParse(node.Attributes["id"].Value, out id);
                        cmdlist = new List<CutSceneCommand>();
                        foreach (XmlNode child in node.ChildNodes)
                        {
                                switch (child.Name)
                                {
                                        case "camera_moveto":
                                        case "camera_follow":
                                        case "camera_restore":
                                        case "camera_backup":
                                        case "camera_shake":
                                                {
                                                        CutSceneCmd_Camera cmd = new CutSceneCmd_Camera(child);
                                                        cmdlist.Add(cmd);
                                                }
                                                break;
                                        case "unit_create":
                                        case "unit_delete":
                                        case "unit_play":
                                        case "unit_moveto":
                                        case "unit_forward":
                                        case "unit_show":
                                        case "unit_rotate":
                                                {
                                                        CutSceneCmd_Unit cmd = new CutSceneCmd_Unit(child);
                                                        cmdlist.Add(cmd);
                                                }
                                                break;
                                        case "effect_play":
                                        case "effect_stop":
                                                {
                                                        CutSceneCmd_Effect cmd = new CutSceneCmd_Effect(child);
                                                        cmdlist.Add(cmd);
                                                }
                                                break;
                                        case "dialog_aside":
                                        case "dialog_unit":
                                        case "dialog_center":
                                                {
                                                        CutSceneCmd_Dialog cmd = new CutSceneCmd_Dialog(child);
                                                        cmdlist.Add(cmd);
                                                }
                                                break;
                                        case "quit":
                                                {
                                                        CutSceneCmd_Quit cmd = new CutSceneCmd_Quit(child);
                                                        cmdlist.Add(cmd);
                                                }
                                                break;
                                        case "wait":
                                                {
                                                        CutSceneCmd_Wait cmd = new CutSceneCmd_Wait(child);
                                                        cmdlist.Add(cmd);
                                                }
                                                break;
                                        case "timescale":
                                                {
                                                        CutSceneCmd_TimeScale cmd = new CutSceneCmd_TimeScale(child);
                                                        cmdlist.Add(cmd);
                                                }
                                                break;
                                        case "tex_show":
                                                {
                                                        CutSceneCmd_Texture cmd = new CutSceneCmd_Texture(child);
                                                        cmdlist.Add(cmd);
                                                }
                                                break;

                                }
                        }
                }
        }
}
