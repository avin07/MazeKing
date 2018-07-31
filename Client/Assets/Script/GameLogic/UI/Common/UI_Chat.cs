using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_Chat : UIBehaviour
{
        public GameObject m_ChatGroup;
        InputField m_InputField;
        Button m_BtnSend;
        void Awake()
        {
                m_BtnSend = GetButton("send");
                m_InputField = GetInputField("InputField");                
#if UNITY_STANDALONE
                m_ChatGroup.SetActive(true);
#else
                m_ChatGroup.SetActive(false);
#endif
        }

        public void OnClickSend(GameObject go)
        {

                switch (m_InputField.text)
                {
                        case "高画质":
                                QualitySettings.SetQualityLevel((int)QualityLevel.Fantastic);
                                break;
                        case "中画质":
                                QualitySettings.SetQualityLevel((int)QualityLevel.Good);
                                break;
                        case "低画质":
                                QualitySettings.SetQualityLevel((int)QualityLevel.Fast);
                                break;
                        default:
                                {
                                        Message.CSMsgChat msg = new Message.CSMsgChat();
                                        msg.idTarget = 0;
                                        msg.byChannel = 0;
                                        msg.strTargetName = "0";
                                        msg.strText = m_InputField.text;
                                        NetworkManager.GetInst().SendMsgToServer(msg);
                                }
                                break;
                }

                if (!string.IsNullOrEmpty(m_InputField.text))
                {
                        singleton.GetInst().AddBackupText(m_InputField.text);
                }

                m_InputField.text = "";
        }

//         public void OnInputField()
//         {
//                 OnClickSend(null);
//         }

        public override void OnShow(float time)
        {
                base.OnShow(time);
                m_InputField.Select();
        }
        public void FocusInputField()
        {
                m_InputField.ActivateInputField();
        }
        public void SetChatText(string text)
        {
                m_InputField.text = text;
                Debuger.Log("SetChatText " + text);
        }

        public void OnToggleChat()
        {
                m_ChatGroup.SetActive(!m_ChatGroup.activeSelf);
        }
}
