using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UI_RaidPassword : UIBehaviour
{
        public Image AnswerIcon0;
        public Image QuestionIcon0;

        RaidNodeBehav m_BelongNode;
        int[] m_PasswordList;
        GameObject[] m_PasswordIconList;
        RaidPasswordRoomConfig m_Cfg;

        GameObject m_ClueGroup;
        GameObject m_ConfirmBtn;

        GameObject m_Tip1, m_Tip2, m_Tip3;
        void Awake()
        {
                m_ClueGroup = GetGameObject("clue");
                m_ClueGroup.SetActive(false);
                m_ConfirmBtn = GetGameObject("confirm");
                m_ConfirmBtn.SetActive(false);
                m_Tip1 = GetGameObject("tip1");
                m_Tip2 = GetGameObject("tip2");
                m_Tip3 = GetGameObject("tip3");
        }
        public void Setup(RaidNodeBehav node, string clue)
        {
                m_BelongNode = node;
                if (node != null && node.belongRoom != null)
                {
                        Debug.Log(node.belongRoom.pieceCfgId);
                        m_Cfg = RaidConfigManager.GetInst().GetPasswordRoomCfg(node.belongRoom.pieceCfgId);
                        if (m_Cfg != null)
                        {
                                for (int i = 0; i < m_Cfg.password_option; i++)
                                {
                                        GameObject obj = QuestionIcon0.gameObject;
                                        if (i > 0)
                                        {
                                                obj = CloneElement(QuestionIcon0.gameObject);
                                                obj.name = "option" + i;
                                        }

                                        if (i < m_Cfg.dark_password_icon.Count)
                                        {
                                                EventTriggerListener.Get(obj).onClick = OnClickOption;
                                                EventTriggerListener.Get(obj).SetTag(i);
                                                Image icon = GetImage(obj, "icon");
                                                icon.enabled = true;
                                                ResourceManager.GetInst().LoadIconSpriteSyn(m_Cfg.dark_password_icon[i], icon.transform);
                                        }
                                }
                                m_PasswordList = new int[m_Cfg.password_number];
                                m_PasswordIconList = new GameObject[m_Cfg.password_number];
                                for (int i = 0; i < m_Cfg.password_number; i++)
                                {
                                        m_PasswordList[i] = -1;
                                        GameObject obj = AnswerIcon0.gameObject;
                                        if (i > 0)
                                        {
                                                obj = CloneElement(AnswerIcon0.gameObject);
                                                obj.name = "password" + i;
                                        }

                                        EventTriggerListener.Get(obj).onClick = OnClickPassword;
                                        EventTriggerListener.Get(obj).SetTag(i);
                                        m_PasswordIconList[i] = obj;
                                }

                        }
                }
                SetClueText(clue);

        }

        void SetClueText(string clue)
        {
                string[] infos = clue.Split('&');
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
                                                break;
                                }
                        }
                }
                //GetText("tips").text = text;
        }

        void OnClickPassword(GameObject go, PointerEventData data)
        {
                Toggle toggle = go.GetComponent<Toggle>();
                if (toggle.isOn)
                {
                        toggle.isOn = false;
                        return;
                }
                else
                {
                        int idx = (int)EventTriggerListener.Get(go).GetTag();
                        GetImage(go, "icon").enabled = false;

                        GameObject optionObj = GetGameObject("option" + m_PasswordList[idx]);
                        optionObj.GetComponent<Toggle>().isOn = false;
                        m_PasswordList[idx] = -1;
                }
        }

        void OnClickOption(GameObject go, PointerEventData data)
        {
                int number = (int)EventTriggerListener.Get(go).GetTag();
                Toggle toggle = go.GetComponent<Toggle>();
                if (toggle.isOn)
                {
                        for (int i = 0; i < m_PasswordList.Length; i++)
                        {
                                if (m_PasswordList[i] == -1)
                                {
                                        if (number < m_Cfg.bright_password_icon.Count)
                                        {
                                                Image icon = GetImage(m_PasswordIconList[i], "icon");

                                                GetImage(m_PasswordIconList[i], "icon").enabled = true;
                                                ResourceManager.GetInst().LoadIconSpriteSyn(m_Cfg.bright_password_icon[number], icon.transform);
                                                m_PasswordIconList[i].GetComponent<Toggle>().isOn = true;
                                                m_PasswordList[i] = number;

                                                CheckConfirmBtnAvailable();

                                                return;
                                        }
                                }
                        }
                        toggle.isOn = false;
                }
                else
                {
                        for (int i = 0; i < m_PasswordList.Length; i++)
                        {
                                if (m_PasswordList[i] == number)
                                {
                                        GetImage(m_PasswordIconList[i], "icon").enabled = false;
                                        m_PasswordIconList[i].GetComponent<Toggle>().isOn = false;
                                        m_PasswordList[i] = -1;
                                        return;
                                }
                        }
                }
        }
        void CheckConfirmBtnAvailable()
        {
                bool bAvail = true;
                for (int i = 0; i < m_PasswordList.Length; i++)
                {
                        if (m_PasswordList[i] < 0)
                        {
                                bAvail = false;
                                break;
                        }
                }
                m_ConfirmBtn.SetActive(bAvail);

        }

        public void OnClickClose()
        {
                base.OnClickClose(null);
        }
        public void OnClickConfirm()
        {
                string str = "";
                for (int i = 0; i < m_PasswordList.Length; i++)
                {
                        if (m_PasswordList[i] < 0)
                                return;

                        str += m_PasswordList[i];
                        str += "|";
                }
                RaidManager.GetInst().SendOpenPasswordDoor(m_BelongNode, str);
                OnClickClose();
        }
        public void OnClickTips()
        {
                m_ClueGroup.SetActive(!m_ClueGroup.activeSelf);
        }
}