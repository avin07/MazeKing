#define _DebugVersion
//#define SecurityPrefetch //如果安全验证端口不为843，需要使用此宏
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Message;
using System;

class NetworkManager : SingletonObject<NetworkManager>
{
        #region pause state control
#if SecurityPrefetch
    //安全验证端口
    public static int SecurityPort = 843;

    //安全验证timeout时间
    public static int SecurityTimeout = 4096;
#endif

#if _DebugVersion
        //获取平台id的地址
        public static string PlatformUrl = "http://2012xj.haohaowan.com/xjk?key=DLK&domain=";

#else
    //获取平台id的地址
     public static string PlatformUrl = "http://api-xj.iahgames.com/key?key=DLK&domain=";

#endif
        public static readonly string DefaultClientUrl = "http://10.10.1.120/CurrentVersion/";


        public static string IP = "10.10.1.120";
        public static int Port = 84;
        public static int areaID = 1;
        public static string userName = "";

        private bool mPause = false;
        private bool haveShowNetLost = false;

        bool m_bIsReturnByUser = true;

        public bool IsReturnByUser
        {
                get { return m_bIsReturnByUser; }
                set { m_bIsReturnByUser = value; }
        }

        public void Init(string platform_Url)
        {
                NetworkManager.PlatformUrl = platform_Url;
        }

        /// <summary>
        /// NetworkManager是不是被暂停了
        /// </summary>
        /// <returns></returns>
        public bool IsPause()
        {
                return mPause;
        }

        /// <summary>
        /// 挂起NetworkManager, 只会对网络消息的处理有影响, 网络消息的接收不受影响(注意:是处理而不是接收, 先接收了消息才可以有消息处理);
        /// 对于断开连接的请求还是会立刻响应.
        /// </summary>
        public void Suspend()
        {
                Debuger.LogWarning("Networking Suspend()**************************************************************");
                mPause = true;
        }

        /// <summary>
        /// 对于挂起的NetworkManager, 要使用WakeUp唤醒NetworkManager继续处理网络消息.
        /// </summary>
        public void WakeUp()
        {
                Debuger.LogWarning("Networking WakeUp()**************************************************************");
                mPause = false;
        }

        #endregion


        #region Properties and Fields

        public TcpPipe pipe = null;

        #endregion // Properties and Fields

        #region Normal Methods

        //是否使用安全验证机制
        private void SecurityPrefetch(string ip)
        {
#if SecurityPrefetch
        if (Application.platform == RuntimePlatform.WindowsWebPlayer ||
            Application.platform == RuntimePlatform.OSXWebPlayer)
        {
            Security.PrefetchSocketPolicy(ip, SecurityPort, SecurityTimeout);
        }
#endif
        }

        public bool ConnectToServer(string address, int port)
        {
                if (pipe != null && pipe.Connected)
                {
                        pipe.Disconnect();
                }
                pipe = new TcpPipe(address, port);

                if (!pipe.Connected)//当前没连通
                {
                        if (!pipe.Connect())
                        {
                            Debuger.Log(address + " fatal error not connected #####");
                            return false;
                        }
                }

                return true;
        }

        public void DisconnectToServer()
        {
                if (pipe == null)
                {
                        return;
                }

                if (!pipe.Connected)
                {
                        return;
                }

                pipe.Disconnect();
        }

        int n_MsgCount = 1;
        public void SendMsgToServer<T>(T msgObj) where T : CSMsgBaseReq
        {
                if (!NetworkManager.GetInst().IsReturnByUser)
                {
#if UNITY_EDITOR
                        Debuger.LogWarning("  waiting for login Message!!!!!   " + msgObj.GetTag() + "=============stack =" + StackTraceUtility.ExtractStackTrace());
#else
			Debuger.LogWarning("  waiting for login Message!!!!!");
#endif
                        return;
                }

                if (pipe != null)
                {
                        string msg = NetMsgSerializer.Serialize(msgObj);
                        pipe.SendMsg(msg /* + " " + CalcSerial(msg)*/);//n_MsgCount++
                        //if (n_MsgCount != 0)
                        //{
                        //        n_MsgCount++;
                        //}
                }
        }

        public void InitSerialNum()
        {
                n_MsgCount = 1;
        }

        int CalcSerial(string str)
        {
                int count = 0;
                for (int i = 0; i < str.Length; ++i)
                {
                        char a = str[i];
                        if (a >= 'a' && a <= 'z')
                        {
                                count += a - 'a';
                        }
                        else if (a >= 'A' && a <= 'Z')
                        {
                                count += 'Z' - a;
                        }
                        else if (a >= '0' && a <= '9')
                        {
                                count += a - '0';
                        }
                }

                return (count ^ n_MsgCount) ^ 919929937;
        }

        public void SendMsgToServer(string msg)
        {
                if (!NetworkManager.GetInst().IsReturnByUser)
                {
                        return;
                }
                if (pipe != null)
                {
                        pipe.SendMsg(msg + " " + CalcSerial(msg));//(n_MsgCount++));
                        if (n_MsgCount != 0)
                        {
                                n_MsgCount++;
                        }
                }
        }

        #region NetWorkUpdateEvent
        internal delegate void NetWorkUpdateEvent();
        public enum NetWorkUpdateType
        {
                CheckLogin_Disconnect = 0,
        }

        public void UpdateCheckEvent(NetWorkUpdateType type, bool add)
        {
                if (type == NetWorkUpdateType.CheckLogin_Disconnect)
                {
                        //Debuger.LogError("UpdateCheckEvent = " + type.ToString() + " add " + add);
                        mbCheckLoginDisconnect = add;
                }
        }

        //protected NetWorkUpdateEvent mCheckLoginDisconnectEvent;
        protected bool mbCheckLoginDisconnect = false;
        protected void UpdateCheckDisconnect()
        {
                //Debuger.LogError("Text here === ");

                if (pipe is LoginTcpPipe)
                {
                        //非主动断开的连接倒计时
                        LoginTcpPipe loginPipe = pipe as LoginTcpPipe;
                        if (loginPipe != null && loginPipe.active)
                        {
                                loginPipe.waitTime += Time.deltaTime;
                                //Debuger.Log("================================= Time  Out   =====================  waitTime" + loginPipe.waitTime);

                                //大于超时时间，断开连接
                                if (loginPipe.waitTime > LoginTcpPipe.WAIT_MSG_TIMEOUT)
                                {
                                        Debuger.Log("================================= Time  Out   =====================  waitTime" + loginPipe.waitTime);
                                        pipe.NotifyDisconnect();
                                        pipe.Disconnect();
                                        pipe = null;
                                }
                        }

                }
        }
        #endregion
        Queue<string> m_PauseMsgQueue = new Queue<string>();
        HashSet<string> m_SkipPauseMsgList = new HashSet<string>();   //即时网络挂起也不挂起的消息列表
        /// <summary>
        /// 将某个消息头加入忽略挂起的列表。
        /// 慎用，会导致消息顺序和服务端不一致。仅在消息顺序不影响的情况下使用。
        /// </summary>
        /// <param name="msg"></param>
        public void AddSkipPauseMsg(string msg)
        {
                if (!m_SkipPauseMsgList.Contains(msg))
                {
                        m_SkipPauseMsgList.Add(msg);
                }
        }
        public void RemoveSkipMsg(string msg)
        {
                if (m_SkipPauseMsgList.Contains(msg))
                {
                        m_SkipPauseMsgList.Remove(msg);
                }
        }
        public void Update()
        {
                if (pipe == null)
                {
                        return;
                }
                if (!IsPause() && !pipe.Connected && !haveShowNetLost)
                {
                        if (m_bIsReturnByUser)
                        {
                                m_bIsReturnByUser = false;
                        }
                        else
                        {
                                haveShowNetLost = true;
                                //singleton.GetInst().ShowMessage("服务器已关闭！");
                                UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox").SetConfirmAndCancel("", "您掉线了", null, null, null);
                                Debuger.LogError("NetworkManager ======================  Update  ==============  pipe.NotifyDisconnect");
                                pipe.NotifyDisconnect();
                                return;
                        }
                }

                if (pipe.Connected)
                {
                        if (IsPause())
                        {
                                string strMsg = pipe.CheckMsg();
                                while (!string.IsNullOrEmpty(strMsg))
                                {
                                        string[] tmps = strMsg.Split(' ');

                                        if (tmps.Length > 0 && m_SkipPauseMsgList.Contains(tmps[0]))
                                        {
                                                MsgMgr.Instance().ProcessMsg(strMsg);   //即时释放消息
                                        }
                                        else
                                        {
                                                m_PauseMsgQueue.Enqueue(strMsg);        //将消息压进挂起队列
                                        }
                                        strMsg = pipe.CheckMsg();
                                }
                        }
                        else
                        {
                                string strMsg = null;
                                while (m_PauseMsgQueue.Count > 0)   //先释放挂起队列里的消息
                                {
                                        strMsg = m_PauseMsgQueue.Dequeue();
                                        MsgMgr.Instance().ProcessMsg(strMsg);
                                        if (IsPause())
                                        {
                                                break;
                                        }
                                }
                                if (IsPause() == false)
                                {
                                        strMsg = pipe.CheckMsg();
                                        while (strMsg != null)
                                        {
                                                MsgMgr.Instance().ProcessMsg(strMsg);
                                                if (IsPause())
                                                {
                                                        break;
                                                }
                                                strMsg = pipe.CheckMsg();
                                        }
                                }
                        }
                }
        }

        string m_KeyInfo = "";

        public string GetLoginKey()
        {
                return m_KeyInfo;
        }

        public bool ConnectLogin(string Keyinfo)
        {
                m_KeyInfo = Keyinfo;
                return ConnectServerWidthKey(Keyinfo);
        }

        public bool ServerChannalSwitch(string address, int port)
        {
                DisconnectToServer();
                IsReturnByUser = false;
                if (!string.IsNullOrEmpty(m_KeyInfo.Trim()) && m_KeyInfo.ToLower() != "null" && m_KeyInfo.Contains(","))
                {

                }
                else
                {
                        m_KeyInfo = null;
                        //LoginFake.Inst.OverWriteKey(ref m_KeyInfo);
                        m_KeyInfo += ",1,1," + address + "," + port + ",pkServer";
                }

                string[] infos = m_KeyInfo.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (infos.Length < 6)
                {
                        Debuger.LogError(" fatal error !!! the lenght of m_KeyInfo is less than 6  m_KeyInfo=" + m_KeyInfo);
                        return false;
                }
                else
                {
                        string tmpKey4PKserver = infos[0] + "," + infos[1] + "," + infos[2] + "," + address + "," + port + "," + infos[5];
                        Debuger.Log(" ServerChannalSwitch ====================== address=" + address + "port=" + port);
                        Debuger.Log(" ServerChannalSwitch tmpKey4PKserver=" + tmpKey4PKserver);
                        return ConnectServerWidthKey(tmpKey4PKserver);
                }
        }

        bool ConnectServerWidthKey(string Keyinfo)
        {
                string[] infos = Keyinfo.Trim().Split(',');

                Debuger.LogWarning("Time=" + Time.realtimeSinceStartup + " ConnectLogin Keyinfo=" + Keyinfo);

#if UNITY_STANDALONE_WIN
                Keyinfo = "";
#endif

#if _DebugVersion
                if (string.IsNullOrEmpty(Keyinfo.Trim()))
                {
                        infos = new string[] { "", "1", "1", "10.10.1.120", "84" };//dummy data;
                }
#endif
                if (infos.Length > 1)
                {
                        string key = infos[0];
                        //LoginFake.Inst.OverWriteKey(ref key);
                        if (!string.IsNullOrEmpty(key) && key.ToLower() != "null")
                        {
                                try
                                {
                                        string address = infos[3];
                                        int port = int.Parse(infos[4]);

//                                         if (!ServerCrossoverManager.GetInst().b_pkServerConnected)
//                                                 LoginFake.Inst.OverWriteLoginPort(ref address, ref port);

                                        NetworkManager.IP = address;
                                        NetworkManager.Port = port;
                                        SecurityPrefetch(NetworkManager.IP);

                                        Debuger.LogWarning("============================00000000000000address:" + address + "  port:" + port + " key:" + key);
                                        Debuger.Log("Time=" + Time.realtimeSinceStartup + "SendKeyLogin========address=" + address + "port=" + port + "key=" + key + "infos[1]=" + infos[1] + "infos[2]=" + infos[2]);
                                        NetworkManager.GetInst().SendKeyLogin(address, port, key, infos[1], infos[2]);
                                        NetworkManager.areaID = int.Parse(infos[2]);
                                        if (infos.Length >= 7)
                                                NetworkManager.userName = infos[6];

                                        return true;
                                }
                                catch (Exception e)
                                {
                                        Debuger.LogError(e.ToString());
                                        return false;
                                }
                        }
                }

                return false;
        }

        public class LoginData
        {
                public string address;
                public int port;
                public string key;
                public string server;
                public string area;
                public LoginData(string _address, int _port, string _key, string _server, string _area)
                {
                        address = _address;
                        port = _port;
                        key = _key;
                        server = _server;
                        area = _area;
                }
        }
        public LoginData mLoginDate;
        public void SendKeyLogin(string address, int port, string key, string server, string area)
        {
                mLoginDate = new LoginData(address, port, key, server, area);
                if (pipe != null && pipe.Connected)
                {
                        pipe.Disconnect();
                }
                pipe = new LoginTcpPipe(address, port);
                pipe.Connect();

                UpdateCheckEvent(NetWorkUpdateType.CheckLogin_Disconnect, true);

                if (pipe.Connected)
                {
                        string msg = string.Concat("9001 ", key, " ", server, " " + area);
                        Debuger.Log("============== msg: " + msg + "=========address=" + address + "======addressport=" + port);
                        pipe.SendMsg(msg);
                }
                else
                {
                        Debuger.LogError(" SendKeyLogin ===========   pipe.Connected   is false ");
                        pipe.NotifyDisconnect();
                }
        }


        public void SendPasswordLogin(string address, int port)
        {
                if (pipe != null && pipe.Connected)
                {
                        Debuger.LogError("============SendPasswordLogin");
                        pipe.Disconnect();
                }
                pipe = new LoginTcpPipe(address, port);
                pipe.Connect();
                UpdateCheckEvent(NetWorkUpdateType.CheckLogin_Disconnect, true);

                if (pipe.Connected)
                {
                        Debuger.LogError("============pipe.SendMsg(msg)");
//                         string msg = string.Format("9002 {0} {1} {2}", Account, LoginFake.Inst.GetCareer(), LoginFake.Inst.GetNewName());
//                         pipe.SendMsg(msg);
                }
                else
                {
                        Debuger.LogError("============pipe.NotifyDisconnec");
                        pipe.NotifyDisconnect();
                }
        }

        #endregion // Normal Methods

}
