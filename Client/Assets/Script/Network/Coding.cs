// **********************************************************************
// Copyright (c) 2010 All Rights Reserved
// File     : Coding.cs
// Author   : cjq
// Created  : 2011-03-21
// Porpuse  : �򵥼ӽ���
// **********************************************************************
//#define _AssetEncoding
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
/***********************************************************************/

class Coding
{
        static byte[] m_Key = new byte[10] {  0xf4, 0x56, 0x59, 0x21, 0x69, 0xfc, 0x78, 0x99, 0x92, 0xb3  };

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="src">Դ��</param>
        /// <returns>���ؼ��ܴ�</returns>
        public static byte[] Encode(byte[] src)
        {
                int nLen = src.Length;
                byte[] arrDst = new byte[nLen];

                for (int n = 0; n < nLen; n++)
                {
                        int nIndex = n % 10;
                        arrDst[n] = (byte)(src[n] ^ m_Key[nIndex]);
                }

                return arrDst;
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="src">���ܴ�</param>
        /// <returns>���ؽ��ܴ�</returns>
        public static byte[] Decode(byte[] src)
        {
                int nLen = src.Length;
                byte[] arrDst = new byte[nLen];

                for (int n = 0; n < nLen; n++)
                {
                        int nIndex = n % 10;
                        arrDst[n] = (byte)(src[n] ^ m_Key[nIndex]);
                }

                return arrDst;
        }

        /// <summary>
        /// ////��Դ�ļ��ܴ���
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static byte[] AssetEncode(byte[] src)
        {
#if _AssetEncoding
			int nLen = src.Length;
			for (int n = 5; n < nLen;n++ )
			{
				int nIndex = (int)System.Math.Pow(2, n);
				if(nIndex<nLen)
				{
					src[nIndex] = (byte)(src[nIndex] ^ m_Key[n % 10]);
				}else
				{
					break;
				}
			}
#endif
                return src;

        }


}

