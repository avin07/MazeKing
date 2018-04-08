
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Message
{
        #region ServerToClient

        interface INetMessageTab
        {
                string GetTag();
                void SetTag(string msgTag);

                /// <summary>
                /// 属性范围(设定属性是在类自身范围内, 还是包含继承属性)
                /// </summary>
                /// <returns></returns>
                BindingFlags GetFlags();
        }

        interface INetMessageInitRemainProps
        {
                void InitRemainProps();
        }

        public abstract class SCMsgBaseAck : INetMessageTab
        {
                //public abstract void Deserialize(string data);

                private event SCNetMsgHandler mNetMsgEvent;

                protected string mMsgTag;

                public void RegisterHandler(SCNetMsgHandler handler)
                {
                        mNetMsgEvent += handler;
                }

                public void UnregisterHandler(SCNetMsgHandler handler)
                {
                        mNetMsgEvent -= handler;
                }

                public void DispatchMessage()
                {
                        if (mNetMsgEvent != null)
                        {
                                mNetMsgEvent(this, new SCNetMsgEventArgs(this));
                        }
                }

                public void DispatchMessage(SCNetMsgEventArgs args)
                {
                        if (mNetMsgEvent != null)
                        {
                                mNetMsgEvent(this, args);
                        }
                }

                public string GetTag()
                {
                        return mMsgTag;
                }

                public void SetTag(string msgTag)
                {
                        mMsgTag = msgTag;
                }

                public virtual BindingFlags GetFlags()
                {
                        return BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public;
                }

                public static bool IsInherit(System.Type type, System.Type baseType)
                {
                        System.Type parentType = type.BaseType;
                        while (parentType != null)
                        {
                                if (parentType.Equals(baseType))
                                {
                                        return true;
                                }

                                parentType = parentType.BaseType;
                        }

                        return false;
                }

                public virtual void Deserialize(FieldInfo[] prop, string[] values)
                {
                        int i = 0;
                        try
                        {
                                for (; i < prop.Length && i < values.Length; i++)
                                {
                                        string paramValue = values[i];

                                        //Debuger.LogError("替换前值:----------" + paramValue);
                                        paramValue = DecodeAMP(paramValue);
                                        //paramValue = paramValue.Replace("@amp@"," ");
                                        //Debuger.LogError("替换后值:" + paramValue);

                                        if (IsInherit(prop[i].FieldType, typeof(SCMsgBaseAck)))
                                        {
                                                SCMsgBaseAck msgTemplate = (SCMsgBaseAck)Activator.CreateInstance(prop[i].FieldType);
                                                System.Object propObj = Message.NetMsgSerializer.Deserialize(msgTemplate, paramValue);
                                                prop[i].SetValue(this, propObj);
                                        }
                                        else
                                        {
                                                GOComponent.SetPropertyValue(this, prop[i], paramValue);
                                        }
                                }
                        }
                        catch (System.Exception)
                        {
                                Debuger.LogError(GetTag() + " Deserialize Error: Properpty( " + prop[i].Name + " Pos=" + i + " Type=" + prop[i].FieldType + " value=" + values[i] + ")");
                        }
                }

                public virtual void RecordMsg(string msg)
                {

                }

                public virtual string DecodeAMP(string msg)
                {
                        return msg.Replace("@amp@", " ");
                }

        }

        public class SCNetMsgEventArgs : EventArgs
        {
                public readonly SCMsgBaseAck mNetMsg;

                public SCNetMsgEventArgs(SCMsgBaseAck netmsg)
                {
                        mNetMsg = netmsg;
                }
        }

        public delegate void SCNetMsgHandler(object sender, SCNetMsgEventArgs e);

        #endregion

        //-------------------------------------------------------------------------------

        #region ClientToServer

        abstract class CSMsgBaseReq : INetMessageTab
        {
                //public abstract string Serialize();

                protected string mMsgTag;

                public string GetTag()
                {
                        return mMsgTag;
                }

                public void SetTag(string msgTag)
                {
                        mMsgTag = msgTag;
                }

                public virtual BindingFlags GetFlags()
                {
                        return BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public;
                }
        }

        #endregion
}
