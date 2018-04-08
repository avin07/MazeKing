
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Message
{
	class MsgMgr : SingletonObject<MsgMgr>
	{
		private Dictionary<string, SCMsgBaseAck> m_NetMsgMap = new Dictionary<string, SCMsgBaseAck>();

		public void ProcessMsg(string strMsg)
		{
//#if UNITY_EDITOR
                        Debuger.LogWarning("RecvMsg ========= " + strMsg + " " + Time.realtimeSinceStartup);
//#endif
			try
			{
#if UNITY_EDITOR
				//Profiler.BeginSample("DispatchMessage");
#endif
				DispatchMessage(strMsg);

#if UNITY_EDITOR
				//Profiler.EndSample();
#endif
			}
			catch (Exception e)
			{
				Debuger.LogError(e.ToString());
			}
		}

		private void DispatchMessage(string strMsg)
		{
			string[] msgTag = strMsg.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
			if (msgTag.Length == 0)
			{
				Debuger.LogWarning("Receive a Empty message from Server(msg = " + strMsg + ")");
				return;
			}
                        
			SCMsgBaseAck msgObj;
			if (m_NetMsgMap.TryGetValue(msgTag[0], out msgObj))
			{
#if UNITY_EDITOR
				//Profiler.BeginSample("DispatchMessage--" + msgTag[0]);
#endif
				try
				{
					SCMsgBaseAck msgInstance = NetMsgSerializer.Deserialize(msgObj, (msgTag.Length == 2) ? msgTag[1] : "");
					msgInstance.RecordMsg(strMsg);
					msgObj.DispatchMessage(new SCNetMsgEventArgs(msgInstance));
				}
				catch (System.Exception ex)
				{
					Debuger.LogError(ex.ToString());
				}
#if UNITY_EDITOR
				//Profiler.EndSample();
#endif

			}
		}

                public bool RegisterMsg(Type msgType)
                {
                        if (msgType == null)
                        {
                                Debuger.LogError("register msg fatal error ##  the type is null");
                                return false;
                        }

                        SCMsgBaseAck msgObj = Activator.CreateInstance(msgType) as SCMsgBaseAck;
                        if (msgObj == null)
                        {
                                Debuger.LogError("register msg fatal error ##  the msg named " + msgType + " is not a message type");
                                return false;
                        }

                        if (!m_NetMsgMap.ContainsKey(msgObj.GetTag()))
                        {
                                m_NetMsgMap.Add(msgObj.GetTag(), msgObj);
                        }
                        return true;
                }

		public bool RemoveMsg(Type msgType)
		{
			if (msgType == null)
			{
				Debuger.LogError("RemoveMsg fatale error!  the name of event error");
				return false;
			}

			SCMsgBaseAck msgObj = Activator.CreateInstance(msgType) as SCMsgBaseAck;
			if (msgObj == null)
			{
				Debuger.LogError("RemoveMsg fatale error!  the msg named " + msgType + " is not a message type");
				return false;
			}

			return m_NetMsgMap.Remove(msgObj.GetTag());
		}

                public bool RegisterMsgHandler(Type msgType, SCNetMsgHandler handler)
                {
                        if (msgType == null || handler == null)
                        {
                                return false;
                        }

                        SCMsgBaseAck msgObj = Activator.CreateInstance(msgType) as SCMsgBaseAck;
                        if (msgObj == null)
                        {
                                Debuger.LogError("ReisterMsgHandler fatale error the msg named " + msgType + " is not a message type");
                                return false;
                        }

                        if (!m_NetMsgMap.ContainsKey(msgObj.GetTag()))
                        {
                                RegisterMsg(msgType);
                        }
                        
                        SCMsgBaseAck msgRegObj = null;
                        if (m_NetMsgMap.TryGetValue(msgObj.GetTag(), out msgRegObj))
                        {
                                msgRegObj.RegisterHandler(handler);
                                return true;
                        }

                        Debuger.LogError("ReisterMsgHandler fatale error Register Pool do not contain message handler " + msgType);
                        return false;
                }

		public void UnreisterMsgHandler(Type msgType, SCNetMsgHandler handler)
		{
			if (msgType == null || handler == null)
			{
				return;
			}

			SCMsgBaseAck msgObj = Activator.CreateInstance(msgType) as SCMsgBaseAck;
			if (msgObj == null)
			{
				return;
			}

			SCMsgBaseAck msgRegObj = null;
			if (m_NetMsgMap.TryGetValue(msgObj.GetTag(), out msgRegObj))
			{
				msgRegObj.UnregisterHandler(handler);
			}
		}
	}
}
