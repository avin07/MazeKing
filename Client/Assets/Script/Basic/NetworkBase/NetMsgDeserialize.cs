
using UnityEngine;
using System;
using System.Collections;

using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Message
{
        class RemainPropStr
        {
                static string[] m_arrValue = null;
                static int m_nIndex;

                public static bool IsValid()
                {
                        return m_arrValue != null;
                }

                public static void SetStr(string msg)
                {
                        msg = msg.Trim();
                        m_arrValue = msg.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        m_nIndex = 0;
                }

                public static int Length { get { return m_arrValue.Length; } }

                public static void Skip()
                {
                        m_nIndex++;
                }

                public static string Get()
                {
                        if (m_nIndex < Length)
                        {
                                return m_arrValue[m_nIndex++];
                        }
                        return "";
                }
                public static int GetIndex()
                {
                        return m_nIndex;
                }

                public static void InitRemainProps(SCMsgBaseAck inst, Type type)
                {
                        if (RemainPropStr.IsValid())
                        {
                                FieldInfo[] prop = type.GetFields(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public);
                                if (RemainPropStr.Length > 0)
                                {
                                        int remain = RemainPropStr.Length - RemainPropStr.GetIndex();
                                        if (remain != prop.Length)
                                        {
                                                Debuger.LogWarning("Remain Prop num = " + remain + " CurrInst Prop num = " + prop.Length);
                                        }

                                        for (int i = 0; i < prop.Length && i < remain; i++)
                                        {
                                                GOComponent.SetPropertyValue(inst, prop[i], RemainPropStr.Get());
                                        }
                                }
                        }
                }
        }

        class NetMsgSerializer
        {
                public static string Serialize<T>(T instanceObj) where T : INetMessageTab
                {
                        FieldInfo[] prop = typeof(T).GetFields(instanceObj.GetFlags());

                        string tag = instanceObj.GetTag();

                        StringBuilder sb = new StringBuilder(64);
                        sb.Append(tag);

                        for (int i = 0; i < prop.Length; i++)
                        {
                                sb.Append(" " + prop[i].GetValue(instanceObj));
                        }

                        return sb.ToString();
                }

                public static SCMsgBaseAck Deserialize(SCMsgBaseAck msgTemplate, string message)
                {

                        Type msgType = msgTemplate.GetType();
                        SCMsgBaseAck instanceObj = (SCMsgBaseAck)Activator.CreateInstance(msgType);
                        FieldInfo[] prop = msgType.GetFields(instanceObj.GetFlags());
                        message = message.Trim();
                        string[] values = null;

                        values = message.Split(new char[] { ' ' }, prop.Length);

                        int realValLen = values.Length;
                        if (prop.Length != realValLen)
                        {
                                if (string.IsNullOrEmpty(instanceObj.GetTag()))
                                {
                                        Debuger.LogError("Deserialize not tag ----- message=" + message);
                                }
#if UNITY_EDITOR
                                Debuger.LogWarning("NetMsgSerializer Deserialize Warning : (" + instanceObj.GetTag() + ") The properties count(" + prop.Length +
                                        " ) is not equal to the count of net message params(" + realValLen + ") - MSG = " + message);
#endif
                        }

                        instanceObj.Deserialize(prop, values);

                        return instanceObj;
                }

                public static T Deserialize<T>(string message) where T : SCMsgBaseAck
                {
                        SCMsgBaseAck instanceObj = (SCMsgBaseAck)Activator.CreateInstance(typeof(T));
                        return (T)Deserialize(instanceObj, message);
                }
        }
}
