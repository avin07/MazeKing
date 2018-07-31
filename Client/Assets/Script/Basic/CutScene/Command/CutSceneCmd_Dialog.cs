using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class CutSceneCmd_Dialog : CutSceneCommand
{
        public string target;
        public string target_name;
        public string text;
        public int line_count;

        public string option0;
        public string option1;
        public string option2;

        public int option0_goto;
        public int option1_goto;
        public int option2_goto;

        public bool bClear;

        public float time;
        public CutSceneCmd_Dialog(XmlNode node)
                : base(node)
        {
                XMLPARSE_METHOD.GetAttrValueStr(node, "target", ref target, "");
                XMLPARSE_METHOD.GetNodeInnerText(node, "name", ref target_name, "");
                XMLPARSE_METHOD.GetNodeInnerText(node, "text", ref text, "");

                XmlNode optionNode0 = XMLPARSE_METHOD.GetNodeInnerText(node, "option0", ref option0, "");
                XMLPARSE_METHOD.GetAttrValueInt(optionNode0, "goto", ref option0_goto, 0);
                XmlNode optionNode1 = XMLPARSE_METHOD.GetNodeInnerText(node, "option1", ref option1, "");
                XMLPARSE_METHOD.GetAttrValueInt(optionNode1, "goto", ref option1_goto, 0);
                XmlNode optionNode2 = XMLPARSE_METHOD.GetNodeInnerText(node, "option2", ref option2, "");
                XMLPARSE_METHOD.GetAttrValueInt(optionNode2, "goto", ref option2_goto, 0);

                XMLPARSE_METHOD.GetAttrValueInt(node, "line_count", ref line_count, 10);
                XMLPARSE_METHOD.GetAttrValueBool(node, "clear", ref bClear, true);

                XMLPARSE_METHOD.GetAttrValueFloat(node, "time", ref time, 0f);
                
        }

        public override void Exec()
        {
                base.Exec();
                string icon = "";
                Vector3 dialogPos = Vector3.zero;
                if (this.target != "")
                {
                        CS_UNIT cs_unit = CutSceneManager.GetInst().GetUnit(target);
                        if (cs_unit != null)
                        {
                                icon = ModelResourceManager.GetInst().GetIconRes(cs_unit.model_id);
                                dialogPos = cs_unit.obj.transform.position;
                        }
                        else if (target.Contains("Node_"))
                        {
                                int nodeId = 0;
                                int.TryParse(target.Replace("Node_", ""), out nodeId);
                                if (nodeId > 0)
                                {
                                        RaidNodeBehav node = RaidManager.GetInst().GetRaidNodeBehav(CutSceneManager.GetInst().CutSceneRoom.idOffset + nodeId);
                                        //Debuger.Log(CutSceneManager.GetInst().IdOffset + nodeId);
                                        if (node != null)
                                        {
                                                if (node.elemCfg != null && node.elemCfg.type == (int)RAID_ELEMENT_TYPE.STORY_NPC)
                                                {
                                                        icon = ModelResourceManager.GetInst().GetIconRes(node.elemCfg.mainModel);
                                                }
                                                dialogPos = node.transform.position;
                                        }
                                }
                        }
                        else if (target.Contains("Team_"))
                        {
//                                 int teamIdx = 0;
//                                 int.TryParse(target.Replace("Team_", ""), out teamIdx);
//                                 HeroUnit unit = RaidTeamManager.GetInst().GetHeroUnit(teamIdx);
//                                 if (unit != null)
//                                 {
//                                         icon = CharacterManager.GetInst().GetCharacterIcon(unit.CharacterID);
//                                         dialogPos = unit.transform.position;
//                                 }
//                                 else
//                                 {
//                                         if (isWait)
//                                         {
//                                                 CutSceneManager.GetInst().EndWaiting();
//                                         }
//                                         return;
//                                 }
                        }
                }

                UI_CutSceneDialog uis = UIManager.GetInst().ShowUI<UI_CutSceneDialog>("UI_CutSceneDialog");
                switch (cmd_name)
                {
                        case "dialog_aside":
                                {
                                        uis.SetAsideDialog(this);
                                }
                                break;
                        case "dialog_unit":
                                {
                                        uis.SetUnitDialog(icon, dialogPos, this);
                                }
                                break;
                        case "dialog_center":
                                {
                                        uis.ShowCenterText(this);
                                }
                                break;
                }
        }
}