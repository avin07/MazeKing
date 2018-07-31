using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class UI_RaidPasswordTips : UIBehaviour
{
        public Text m_TipText;
        GameObject m_Tip1, m_Tip2, m_Tip3;

        void Awake()
        {
                m_Tip1 = GetGameObject("tip1");
                m_Tip2 = GetGameObject("tip2");
                m_Tip3 = GetGameObject("tip3");
        }

        public void Setup(string tips, RaidRoomBehav pwRoom)
        {
                if (pwRoom == null)
                        return;

                RaidPasswordRoomConfig m_Cfg = RaidConfigManager.GetInst().GetPasswordRoomCfg(pwRoom.pieceCfgId);
                string[] infos = tips.Split('&');
                foreach (string info in infos)
                {
                        if (string.IsNullOrEmpty(info))
                                continue;
                        string[] tmps = info.Split('|');
                        if (tmps.Length == 3)
                        {
                                int type = int.Parse(tmps[0]);
                                int place = int.Parse(tmps[2]) + 1;
                                int value = int.Parse(tmps[1]);
                                switch (type)
                                {
                                        case 1:
                                                {
                                                        GameObject tipObj = CloneElement(m_Tip1);
                                                        Image icon = GetImage(tipObj, "icon");
                                                        icon.enabled = true;
                                                        ResourceManager.GetInst().LoadIconSpriteSyn(m_Cfg.dark_password_icon[value], icon.transform);
                                                }
                                                //text += string.Format(LanguageManager.GetText("password_clue_3"), value.ToString());
                                                //text += "\n";
                                                break;
                                        case 2:
                                                {
                                                        GameObject tipObj = CloneElement(m_Tip2);
                                                        string text = tipObj.GetComponent<Text>().text;
                                                        tipObj.GetComponent<Text>().text = text.Replace("%s", place.ToString());
                                                        Image icon = GetImage(tipObj, "icon");
                                                        icon.enabled = true;
                                                        ResourceManager.GetInst().LoadIconSpriteSyn(m_Cfg.dark_password_icon[value], icon.transform);
                                                }
                                                //                                                 text += string.Format(LanguageManager.GetText("password_clue_1"), place.ToString(), value.ToString());
                                                //                                                 text += "\n";
                                                break;
                                        case 3:
                                                {
                                                        GameObject tipObj = CloneElement(m_Tip3);
                                                        Image icon = GetImage(tipObj, "icon");
                                                        icon.enabled = true;
                                                        ResourceManager.GetInst().LoadIconSpriteSyn(m_Cfg.dark_password_icon[value], icon.transform);
                                                }
                                                //                                                 text += string.Format(LanguageManager.GetText("password_clue_2"), value.ToString());
                                                //                                                 text += "\n";
                                                break;
                                }
                        }
                }
                //m_TipText.text = text;
        }
        public void OnClickClose()
        {
                base.OnClickClose(null);
        }
}