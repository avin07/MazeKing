using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;


class TcpPipe
{
        private Queue m_queMsg = new Queue();
        protected TcpClient m_tcp;
        private Thread m_threadReceive;
        Thread m_threadSend;
        Queue m_queMsgSend = new Queue();
        Queue _queMsgSend = new Queue();
        private string m_strIP;
        private int m_nPort;

        public string ip { get { return m_strIP; } }
        public int port { get { return m_nPort; } }

        //public bool m_bLogin;//新登陆流程判断

        private bool AbortFlag = false;
        public bool Connected
        {
                get
                {
                        if (m_tcp != null && m_tcp.Client != null)
                        {
                                return m_tcp.Client.Connected;
                        }
                        else
                        {
                                return false;
                        }
                }
        }

        public TcpPipe(string strIP, int nPort)
        {
                m_strIP = strIP;
                m_nPort = nPort;
        }

        public string CheckMsg()
        {
                lock (m_queMsg.SyncRoot)
                {
                        if (m_queMsg.Count > 0)
                        {
                                return (string)m_queMsg.Dequeue();
                        }
                        return null;
                }
        }
        public string PeekMsg()
        {
                if (m_queMsg.Count > 0)
                {
                        return (string)m_queMsg.Peek();
                }
                return null;
        }

        public bool Connect()
        {
                try
                {
                        m_tcp = new TcpClient();
                        m_tcp.Connect(m_strIP, m_nPort);
                        m_threadReceive = new Thread(new ThreadStart(this.ThreadProcReceive));

                        Debuger.Log("TcpPipeConnect = " + m_threadReceive.ManagedThreadId + " IP=" + m_strIP + "  Port=" + m_nPort);
                        AbortFlag = false;
                        m_threadReceive.Start();
                        Debuger.LogWarning("TcpPipe Connect");

                        m_threadSend = new Thread(new ThreadStart(ThreadSend));
                        m_threadSend.Start();

                        return true;
                }
                catch (SocketException se)
                {
                        Debuger.Log(se);
                        Thread.Sleep(5);
                }
                catch (System.Security.SecurityException se2)
                {
                        Debuger.Log(se2);
                        Thread.Sleep(1);
                        //Security.PrefetchSocketPolicy(NetworkManager.IP, 843);
                        Thread.Sleep(5);
                        return ReConnect();
                }
                catch (Exception e)
                {
                        System.Diagnostics.Debug.WriteLine(e.ToString());
                        Thread.Sleep(5);
                }
                return false;
        }

        bool ReConnect()
        {
                if (m_threadReceive != null)
                        m_threadReceive.Abort();

                m_tcp = new TcpClient();
                m_tcp.Connect(m_strIP, m_nPort);
                m_threadReceive = new Thread(new ThreadStart(this.ThreadProcReceive));

                Debuger.Log("m_threadReceive.Connect = " + m_threadReceive.ManagedThreadId + " Time = " + Time.time);
                AbortFlag = false;
                m_threadReceive.Start();
                Debuger.Log("m_threadReceive.Start();###################################################ReConnect");

                return true;
        }

        public virtual void Disconnect()
        {
                //暂时恢复abort函数（代验证）
                //m_threadReceive.Abort();
                //Debuger.LogError("网络_______________________________Disconnect   stack="+StackTraceUtility.ExtractStackTrace());
                Debuger.LogError("网络_______________________________Disconnect   ");
                AbortFlag = true;
                m_tcp.Close();
        }

        ~TcpPipe()
        {
                this.Disconnect();
        }

        bool bRecord = false;
        System.DateTime lastRecordTime;
        public void StartRecord()
        {
#if UNITY_STANDALONE_WIN
                bRecord = true;
                lastRecordTime = System.DateTime.Now;
#endif
        }
        void Record(string strMsg)
        {
                int msec = (int)(System.DateTime.Now - lastRecordTime).TotalMilliseconds;
                lastRecordTime = System.DateTime.Now;
        }

        public void SendMsg(string strMsg)
        {
                //#if UNITY_EDITOR
                if (/*!strMsg.Contains("cRaidMapMove") && */!strMsg.Contains("cRaidDispelMask") && !strMsg.Contains("cHT"))
                {
                        Debuger.LogError("SendMsg ========= " + strMsg);
                }
                //#endif
                
                if (m_tcp == null)
                {
                        Debuger.Log("tcp==null!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                        return;
                }
                if (m_tcp.Client == null)
                {
                        Debuger.Log("m_tcp.Client == null!!!!!!!!!!!!!!!!!!!!!!!!!");
                        return;
                }

                if (!this.Connected)
                {
                        NotifyDisconnect();
                        return;
                }

                if (bRecord)
                {
                        Record(strMsg);
                }

                if (this.Connected)
                {
                        lock (m_queMsgSend.SyncRoot)
                        {
                                m_queMsgSend.Enqueue(strMsg);
                        }
                }
        }

        public void DisconnectNotify()
        {
                Debuger.Log("TcpPipe not connected!");
        }

        public virtual void NotifyDisconnect()
        {
                DisconnectNotify();
        }

        void ThreadSend()
        {
                if (!this.Connected)
                {
                        Debuger.Log("TcpPipe not connected!");
                        return;
                }

                while (this.Connected && !AbortFlag)
                {
                        if (m_queMsgSend.Count <= 0)
                                Thread.Sleep(10);

                        try
                        {
                                lock (m_queMsgSend.SyncRoot)
                                {
                                        while (m_queMsgSend.Count > 0)
                                        {
                                                _queMsgSend.Enqueue(m_queMsgSend.Dequeue());
                                        }
                                }

                                while (_queMsgSend.Count > 0)
                                {
                                        byte[] bufencode = this.encode((string)_queMsgSend.Dequeue());
                                        byte[] buffer = new byte[(bufencode.Length + sizeof(ushort))];
                                        byte[] arrSize = BitConverter.GetBytes((ushort)bufencode.Length);
                                        arrSize.CopyTo(buffer, 0);
                                        Array.Copy(bufencode, 0, buffer, sizeof(ushort), bufencode.Length);
                                        m_tcp.GetStream().Write(buffer, 0, buffer.Length);
                                }
                        }
                        catch (Exception)
                        {
                        }
                }
        }

        private void ThreadProcReceive()
        {

                if (!this.Connected)
                {
                        Debuger.Log("TcpPipe not connected!");
                        return;
                }

                while (this.Connected && !AbortFlag)
                {
                        try
                        {
                                string[] msgArray = ReadStrings();
                                if (AbortFlag)
                                {
                                        break;
                                }

                                if (msgArray.Length == 0)
                                {
                                        Thread.Sleep(10);
                                }

                                lock (m_queMsg.SyncRoot)
                                {
                                        foreach (string s in msgArray)
                                        {
                                                m_queMsg.Enqueue(s);
                                        }
                                }
                        }
                        catch (Exception)
                        {
                        }
                }
        }


        protected virtual string[] ReadStrings()
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
                usMsgLen /= 2;//真实消息大小

                byte[] abMsgBuf = new byte[usMsgLen];//msg body
                usRecLen = 0;
                while (usRecLen < usMsgLen)
                {
                        int n = m_tcp.GetStream().Read(abMsgBuf, usRecLen, usMsgLen - usRecLen);
                        usRecLen += (ushort)n;
                }

                string[] msgArray;
                string str = bCompressed ? this.decode(abMsgBuf) : StrRevise.Decode(abMsgBuf);
                msgArray = str.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries);
                //Debuger.LogError("======1111" + str);

                return msgArray;
        }

        public virtual byte[] encode(string str)
        {
            return (Coding.Encode(StrRevise.Encode(str)));
        }

        public string decode(byte[] source)
        {
                int level;
                int src = ((source[0] & 2) == 2) ? 9 : 3;
                int size = src == 9 ? source[5] | (source[6] << 8) | (source[7] << 16) | (source[8] << 24) : source[2];
                int dst = 0;
                uint cword_val = 1;
                byte[] destination = new byte[size];
                int[] hashtable = new int[4096];
                byte[] hash_counter = new byte[4096];
                int last_matchstart = size - 6 - 4 - 1;
                int last_hashed = -1;
                int hash;
                uint fetch = 0;

                level = (source[0] >> 2) & 0x3;

                if (level != 1 && level != 3)
                {
                        return StrRevise.Decode(source);
                }

                if ((source[0] & 1) != 1)
                {
                        byte[] d2 = new byte[size];
                        System.Array.Copy(source, src, d2, 0, size);
                        return StrRevise.Decode(d2);
                }

                for (; ; )
                {
                        if (cword_val == 1)
                        {
                                cword_val = (uint)(source[src] | (source[src + 1] << 8) | (source[src + 2] << 16) | (source[src + 3] << 24));
                                src += 4;
                                if (dst <= last_matchstart)
                                {
                                        if (level == 1)
                                                fetch = (uint)(source[src] | (source[src + 1] << 8) | (source[src + 2] << 16));
                                        else
                                                fetch = (uint)(source[src] | (source[src + 1] << 8) | (source[src + 2] << 16) | (source[src + 3] << 24));
                                }
                        }

                        if ((cword_val & 1) == 1)
                        {
                                uint matchlen;
                                uint offset2;

                                cword_val = cword_val >> 1;

                                if (level == 1)
                                {
                                        hash = ((int)fetch >> 4) & 0xfff;
                                        offset2 = (uint)hashtable[hash];

                                        if ((fetch & 0xf) != 0)
                                        {
                                                matchlen = (fetch & 0xf) + 2;
                                                src += 2;
                                        }
                                        else
                                        {
                                                matchlen = source[src + 2];
                                                src += 3;
                                        }
                                }
                                else
                                {
                                        uint offset;
                                        if ((fetch & 3) == 0)
                                        {
                                                offset = (fetch & 0xff) >> 2;
                                                matchlen = 3;
                                                src++;
                                        }
                                        else if ((fetch & 2) == 0)
                                        {
                                                offset = (fetch & 0xffff) >> 2;
                                                matchlen = 3;
                                                src += 2;
                                        }
                                        else if ((fetch & 1) == 0)
                                        {
                                                offset = (fetch & 0xffff) >> 6;
                                                matchlen = ((fetch >> 2) & 15) + 3;
                                                src += 2;
                                        }
                                        else if ((fetch & 127) != 3)
                                        {
                                                offset = (fetch >> 7) & 0x1ffff;
                                                matchlen = ((fetch >> 2) & 0x1f) + 2;
                                                src += 3;
                                        }
                                        else
                                        {
                                                offset = (fetch >> 15);
                                                matchlen = ((fetch >> 7) & 255) + 3;
                                                src += 4;
                                        }
                                        offset2 = (uint)(dst - offset);
                                }

                                destination[dst + 0] = destination[offset2 + 0];
                                destination[dst + 1] = destination[offset2 + 1];
                                destination[dst + 2] = destination[offset2 + 2];

                                for (int i = 3; i < matchlen; i += 1)
                                {
                                        destination[dst + i] = destination[offset2 + i];
                                }

                                dst += (int)matchlen;

                                if (level == 1)
                                {
                                        fetch = (uint)(destination[last_hashed + 1] | (destination[last_hashed + 2] << 8) | (destination[last_hashed + 3] << 16));
                                        while (last_hashed < dst - matchlen)
                                        {
                                                last_hashed++;
                                                hash = (int)(((fetch >> 12) ^ fetch) & (4096 - 1));
                                                hashtable[hash] = last_hashed;
                                                hash_counter[hash] = 1;
                                                fetch = (uint)(fetch >> 8 & 0xffff | (uint)destination[last_hashed + 3] << 16);
                                        }
                                        fetch = (uint)(source[src] | (source[src + 1] << 8) | (source[src + 2] << 16));
                                }
                                else
                                {
                                        fetch = (uint)(source[src] | (source[src + 1] << 8) | (source[src + 2] << 16) | (source[src + 3] << 24));
                                }
                                last_hashed = dst - 1;
                        }
                        else
                        {
                                if (dst <= last_matchstart)
                                {
                                        destination[dst] = source[src];
                                        dst += 1;
                                        src += 1;
                                        cword_val = cword_val >> 1;

                                        if (level == 1)
                                        {
                                                while (last_hashed < dst - 3)
                                                {
                                                        last_hashed++;
                                                        int fetch2 = destination[last_hashed] | (destination[last_hashed + 1] << 8) | (destination[last_hashed + 2] << 16);
                                                        hash = ((fetch2 >> 12) ^ fetch2) & (4096 - 1);
                                                        hashtable[hash] = last_hashed;
                                                        hash_counter[hash] = 1;
                                                }
                                                fetch = (uint)(fetch >> 8 & 0xffff | (uint)source[src + 2] << 16);
                                        }
                                        else
                                        {
                                                fetch = (uint)(fetch >> 8 & 0xffff | (uint)source[src + 2] << 16 | (uint)source[src + 3] << 24);
                                        }
                                }
                                else
                                {
                                        while (dst <= size - 1)
                                        {
                                                if (cword_val == 1)
                                                {
                                                        src += 4;
                                                        cword_val = 0x80000000;
                                                }

                                                destination[dst] = source[src];
                                                dst++;
                                                src++;
                                                cword_val = cword_val >> 1;
                                        }
                                        return StrRevise.Decode(destination);
                                }
                        }
                }
        }

}

class StrRevise
{
        /// <summary>
        /// 写入数字
        /// </summary>
        /// <param name="szTmp">二进制目标地址</param>
        /// <param name="nSize">写入索引</param>
        /// <param name="unVal">数字</param>
        /// <param name="bSigned">是否为负数</param>
        static protected void SetNumber(ref byte[] szTmp, ref int nSize, UInt64 unVal, bool bSigned)
        {
                if (unVal <= 0xff)
                {
                        szTmp[nSize++] = (byte)(bSigned ? 0x01 : 0x02);
                        szTmp[nSize++] = (byte)unVal;
                }
                else if (unVal <= 0xffff)
                {
                        szTmp[nSize++] = (byte)(bSigned ? 0x03 : 0x04);
                        szTmp[nSize++] = (byte)unVal; unVal >>= 8;
                        szTmp[nSize++] = (byte)unVal;
                }
                else if (unVal <= 0xffffff)
                {
                        szTmp[nSize++] = (byte)(bSigned ? 0x05 : 0x06);
                        szTmp[nSize++] = (byte)unVal; unVal >>= 8;
                        szTmp[nSize++] = (byte)unVal; unVal >>= 8;
                        szTmp[nSize++] = (byte)unVal;
                }
                else if (unVal <= 0xffffffff)
                {
                        szTmp[nSize++] = (byte)(bSigned ? 0x07 : 0x08);
                        szTmp[nSize++] = (byte)unVal; unVal >>= 8;
                        szTmp[nSize++] = (byte)unVal; unVal >>= 8;
                        szTmp[nSize++] = (byte)unVal; unVal >>= 8;
                        szTmp[nSize++] = (byte)unVal;
                }
                else if (unVal <= 0xffffffffffffffff)
                {
                        szTmp[nSize++] = (byte)(bSigned ? 0x0F : 0x10);
                        szTmp[nSize++] = (byte)unVal; unVal >>= 8;
                        szTmp[nSize++] = (byte)unVal; unVal >>= 8;
                        szTmp[nSize++] = (byte)unVal; unVal >>= 8;
                        szTmp[nSize++] = (byte)unVal; unVal >>= 8;
                        szTmp[nSize++] = (byte)unVal; unVal >>= 8;
                        szTmp[nSize++] = (byte)unVal; unVal >>= 8;
                        szTmp[nSize++] = (byte)unVal; unVal >>= 8;
                        szTmp[nSize++] = (byte)unVal;
                }
        }

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="strSrc">字串源数据</param>
        /// <returns>成功返回byte[],否则返回null</returns>
        static public byte[] Encode(string strSrc)
        {
                int nSize = 0;
                byte[] lpDst = null;
                if (null == strSrc || 0 == strSrc.Length)
                {
                        return null;
                }

                byte[] lpSrc = Encoding.UTF8.GetBytes(strSrc.ToCharArray(), 0, strSrc.Length);
                int nLen = lpSrc.Length;
                byte[] szTmp = new byte[nLen];

                for (int n = 0; n < nLen; ++n)
                {
                        if (' ' == lpSrc[n] || n != 0 && lpSrc[n] >= '0' && lpSrc[n] <= '9')
                        {
                                if (' ' == lpSrc[n])
                                {
                                        ++n;
                                }
                                if (nLen == n || ' ' == lpSrc[n])
                                {
                                        szTmp[nSize++] = (byte)' '; // flag space
                                        --n;
                                        continue;
                                }
                                if ('0' == lpSrc[n] && n + 1 < nLen && (' ' == lpSrc[n + 1] || 0 == lpSrc[n + 1]))
                                { // space + 0
                                        szTmp[nSize++] = 0x0E; // flag
                                        continue;
                                }

                                int m = n;
                                if ('-' == lpSrc[n])
                                { // signed number
                                        m += 1;
                                }

                                bool bNum = true;
                                if (m < nLen && '1' <= lpSrc[m] && '9' >= lpSrc[m])
                                { // maybe a number
                                        for (int j = m + 1; j <= nLen; ++j)
                                        {
                                                if ((j - m) > 19)
                                                { // it's a string
                                                        bNum = false;
                                                        break;
                                                }
                                                else if (j == nLen || 0 == lpSrc[j] || '0' > lpSrc[j] || '9' < lpSrc[j])
                                                {   // okey, i'm sure that is a number
                                                        // now, what we can do is converting to uint64
                                                        UInt64 unVal = UInt64.Parse(Encoding.UTF8.GetString(lpSrc, m, j - m));
                                                        SetNumber(ref szTmp, ref nSize, unVal, m != n);
                                                        n = j - 1;
                                                        break;
                                                }
                                                else if (j == nLen || (j - m) > 19)
                                                { // it's a string
                                                        bNum = false;
                                                        break;
                                                }
                                        }
                                }
                                else
                                {
                                        bNum = false;
                                }

                                if (!bNum)
                                {
                                        szTmp[nSize++] = (byte)' ';
                                        for (; n < nLen; ++n)
                                        { // copy to destination
                                                if (' ' == lpSrc[n])
                                                {
                                                        --n;
                                                        break;
                                                }
                                                szTmp[nSize++] = lpSrc[n];
                                        }
                                }
                                continue;
                        }
                        for (; n < nLen; ++n)
                        { // copy to destination
                                if (' ' == lpSrc[n])
                                {
                                        --n;
                                        break;
                                }
                                szTmp[nSize++] = lpSrc[n];
                        }
                }

                lpDst = new byte[nSize];
                Array.Copy(szTmp, lpDst, nSize);
                return lpDst;
        }

        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="lpSrc">二进制源数据</param>
        /// <returns>返回字串数据</returns>
        static public string Decode(byte[] lpSrc)
        {
                if (null == lpSrc || 0 == lpSrc.Length)
                {
                        return "";
                }

                int nSize = lpSrc.Length;
                StringBuilder strBuilder = new StringBuilder(nSize + nSize);

                int nSrcIndex = 0;

                bool bSkillSpace = false;
                while (nSrcIndex < nSize)
                {
                        byte ucFlag = lpSrc[nSrcIndex++];
                        if (ucFlag == 0x01 || ucFlag == 0x02)
                        {
                                byte num = lpSrc[nSrcIndex++];
                                if (!bSkillSpace)
                                {
                                        strBuilder.Append(' ');
                                }
                                else
                                {
                                        bSkillSpace = false;
                                }
                                if (ucFlag == 0x01)
                                {
                                        strBuilder.Append('-');
                                }
                                strBuilder.Append(num);
                        }
                        else if (ucFlag == 0x03 || ucFlag == 0x04)
                        {
                                ushort num = lpSrc[nSrcIndex++];
                                ushort tmp = lpSrc[nSrcIndex++];
                                tmp <<= 8; num |= tmp;
                                if (!bSkillSpace)
                                {
                                        strBuilder.Append(' ');
                                }
                                else
                                {
                                        bSkillSpace = false;
                                }
                                if (ucFlag == 0x03)
                                {
                                        strBuilder.Append('-');
                                }
                                strBuilder.Append(num);
                        }
                        else if (ucFlag == 0x05 || ucFlag == 0x06)
                        {
                                uint num = lpSrc[nSrcIndex++];
                                uint tmp = lpSrc[nSrcIndex++];
                                tmp <<= 8; num |= tmp; tmp = lpSrc[nSrcIndex++];
                                tmp <<= 16; num |= tmp;
                                if (!bSkillSpace)
                                {
                                        strBuilder.Append(' ');
                                }
                                else
                                {
                                        bSkillSpace = false;
                                }
                                if (ucFlag == 0x05)
                                {
                                        strBuilder.Append('-');
                                }
                                strBuilder.Append(num);
                        }
                        else if (ucFlag == 0x07 || ucFlag == 0x08)
                        {
                                uint num = lpSrc[nSrcIndex++];
                                uint tmp = lpSrc[nSrcIndex++];
                                tmp <<= 8; num |= tmp; tmp = lpSrc[nSrcIndex++];
                                tmp <<= 16; num |= tmp; tmp = lpSrc[nSrcIndex++];
                                tmp <<= 24; num |= tmp;
                                if (!bSkillSpace)
                                {
                                        strBuilder.Append(' ');
                                }
                                else
                                {
                                        bSkillSpace = false;
                                }
                                if (ucFlag == 0x07)
                                {
                                        strBuilder.Append('-');
                                }
                                strBuilder.Append(num);
                        }
                        else if (ucFlag == 0x0F || ucFlag == 0x10)
                        {
                                UInt64 num = lpSrc[nSrcIndex++];
                                UInt64 tmp = lpSrc[nSrcIndex++];
                                tmp <<= 8; num |= tmp; tmp = lpSrc[nSrcIndex++];
                                tmp <<= 16; num |= tmp; tmp = lpSrc[nSrcIndex++];
                                tmp <<= 24; num |= tmp; tmp = lpSrc[nSrcIndex++];
                                tmp <<= 32; num |= tmp; tmp = lpSrc[nSrcIndex++];
                                tmp <<= 40; num |= tmp; tmp = lpSrc[nSrcIndex++];
                                tmp <<= 48; num |= tmp; tmp = lpSrc[nSrcIndex++];
                                tmp <<= 56; num |= tmp;
                                if (!bSkillSpace)
                                {
                                        strBuilder.Append(' ');
                                }
                                else
                                {
                                        bSkillSpace = false;
                                }
                                if (ucFlag == 0x0F)
                                {
                                        strBuilder.Append('-');
                                }
                                strBuilder.Append(num);
                        }
                        else if (ucFlag == 0x0E)
                        {
                                strBuilder.Append(" 0");
                        }
                        else if (ucFlag == 0x17)
                        {
                                bSkillSpace = true;
                                strBuilder.Append(' ');
                        }
                        else
                        {
                                bSkillSpace = false;
                                --nSrcIndex;
                                while (nSrcIndex < nSize)
                                {
                                        if (lpSrc[nSrcIndex] > 0x80)
                                        {
                                                int x = 1;
                                                for (; nSrcIndex + x < nSize; x++)
                                                {
                                                        if (lpSrc[nSrcIndex + x] < 0x80)
                                                        {
                                                                break;
                                                        }
                                                }
                                                char[] c = Encoding.UTF8.GetChars(lpSrc, nSrcIndex, x);
                                                strBuilder.Append(c);
                                                nSrcIndex += x;
                                        }
                                        else
                                        {
                                                strBuilder.Append((char)lpSrc[nSrcIndex++]);
                                        }
                                        if (nSrcIndex == nSize || lpSrc[nSrcIndex] <= 0x09 || lpSrc[nSrcIndex] == 0x0E || lpSrc[nSrcIndex] == 0x0F || lpSrc[nSrcIndex] == 0x10 || lpSrc[nSrcIndex] == 0x17)
                                        {
                                                break;
                                        }
                                }
                        }
                }
                return strBuilder.ToString();
        }
}

