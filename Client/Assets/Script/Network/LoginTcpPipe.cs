using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;


class LoginTcpPipe : TcpPipe
{
        //private Queue m_queMsg = new Queue();
        //private TcpClient m_tcp;
        //private Thread m_threadReceive;
        //private string m_strIP;
        //private int m_nPort;
        static internal float WAIT_MSG_TIMEOUT = 30F;//等待服务器的最长时间
        internal float waitTime = 0;                //等待服务器的最长时间
        internal bool active = true;               //非主动断开，才检查是否超时

        public LoginTcpPipe(string strIP, int nPort)
                : base(strIP, nPort)
        {

        }


        ~LoginTcpPipe()
        {
                this.Disconnect();
        }

        public override void Disconnect()
        {
                base.Disconnect();
                active = false;
        }

        protected override string[] ReadStrings()
        {

                byte[] abLenBuf = new byte[2];//msg head
                ushort usRecLen = 0;
                while (usRecLen < sizeof(ushort))
                {
                        int n = m_tcp.GetStream().Read(abLenBuf, usRecLen, sizeof(ushort) - usRecLen);//recv head, 2 byte
                        usRecLen += (ushort)n;
                }

                ushort usMsgLen = BitConverter.ToUInt16(abLenBuf, 0);
                bool bCompressed = (usMsgLen % 2 != 0);//判断是否被quicklz压缩

                byte[] abMsgBuf = new byte[usMsgLen];//msg body
                usRecLen = 0;
                while (usRecLen < usMsgLen)
                {
                        int n = m_tcp.GetStream().Read(abMsgBuf, usRecLen, usMsgLen - usRecLen);
                        usRecLen += (ushort)n;
                }

                string[] msgArray;
                msgArray = new string[] { System.Text.Encoding.UTF8.GetString(Coding.Decode(abMsgBuf)) };

                return msgArray;

        }


        public override byte[] encode(string str)
        {
            return Coding.Encode(System.Text.Encoding.UTF8.GetBytes(str.ToCharArray(), 0, str.Length));

        }


        public override void NotifyDisconnect()
        {
                //modify by yifenfei 覆盖了其他的提示
                //base.NotifyDisconnect();
                //联机平台提示失败时，必定断开链接等待重连
                Disconnect();
                //UIManager.GetInst().GetWindow<MyGUIloginWnd>("MyGUIloginWnd").InputPlatformAccount();
        }
}

