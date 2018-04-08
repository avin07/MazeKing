// **********************************************************************
// Copyright (c) 2010 All Rights Reserved
// File     : XMLPARSE_METHOD.cs
// Author   : yjk
// Created  : 2010-12-16
// Porpuse  : 
// **********************************************************************
using System;
using System.Xml;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

class XMLPARSE_METHOD
{
        public static void GetAttrValueStr(XmlNode node, string AttrName, ref string output, string defValue)
        {
                if (node != null)
                {
                        if (node.Attributes.GetNamedItem(AttrName) != null)
                        {
                                output = node.Attributes.GetNamedItem(AttrName).Value;
                                return;
                        }
                }
                output = defValue;
        }

        public static void GetAttrValueInt(XmlNode node, string AttrName, ref int output, int defValue)
        {
                if (node != null)
                {
                        if (node.Attributes.GetNamedItem(AttrName) != null)
                        {
                                int.TryParse(node.Attributes.GetNamedItem(AttrName).Value, out output);
                                return;
                        }
                }
                output = defValue;
        }
        public static void GetAttrValueListInt(XmlNode node, string AttrName, ref List<int> list, char splitchar=',')
        {
                if (node != null)
                {
                        if (node.Attributes.GetNamedItem(AttrName) != null)
                        {
                                string tmp = node.Attributes.GetNamedItem(AttrName).Value;
                                string[] tmps = tmp.Split(splitchar);
                                foreach (string val in tmps)
                                {
                                        if (!string.IsNullOrEmpty(val))
                                        {
                                                list.Add(int.Parse(val));
                                        }
                                }
                        }
                }
        }

        public static void GetAttrValueFloat(XmlNode node, string AttrName, ref float output, float defValue)
        {
                if (node != null)
                {
                        if (node.Attributes.GetNamedItem(AttrName) != null)
                        {
                                float.TryParse(node.Attributes.GetNamedItem(AttrName).Value, out output);
                                return;
                        }
                }
                output = defValue;
        }

        public static void GetAttrValueVector3(XmlNode node, string AttrName, ref Vector3 output, Vector3 defValue)
        {
                if (node != null)
                {
                        if (node.Attributes.GetNamedItem(AttrName) != null)
                        {
                                output = ConvertToVector3(node.Attributes.GetNamedItem(AttrName).Value);
                                return;
                        }
                }
                output = defValue;
        }

        public static void GetAttrValueVector2(XmlNode node, string AttrName, ref Vector2 output, Vector2 defValue)
        {
                if (node != null)
                {
                        if (node.Attributes.GetNamedItem(AttrName) != null)
                        {
                                output = ConvertToVector2(node.Attributes.GetNamedItem(AttrName).Value);
                                return;
                        }
                }
                output = defValue;
        }

        public static void GetAttrValueColor(XmlNode node, string AttrName, ref Color output, Color defValue)
        {
                if (node != null)
                {
                        if (node.Attributes.GetNamedItem(AttrName) != null)
                        {
                                output = ConvertToColor(node.Attributes.GetNamedItem(AttrName).Value);
                                return;
                        }
                }
                output = defValue;
        }


        public static void GetAttrValueBool(XmlNode node, string AttrName, ref bool output, bool defValue)
        {
                if (node != null)
                {
                        if (node.Attributes.GetNamedItem(AttrName) != null)
                        {
                                output = ConvertToBool(node.Attributes.GetNamedItem(AttrName).Value);
                                return;
                        }
                }
                output = defValue;
        }

        #region ConvertMethod

        public static bool ConvertToBool(string boolstring)
        {
                if (boolstring == "true")
                {
                        return true;
                }
                return false;
        }

        public static Vector3 ConvertToVector3(string posstr)
        {
                posstr = posstr.Replace("(", "");
                posstr = posstr.Replace(")", "");

                Vector3 ret = Vector3.zero;
                char[] sp = { ',' };
                string[] xyz = posstr.Split(sp);
                if (xyz.Length != 3)
                {
                        //Debuger.LogError("ConvertToVector3" + posstr + " error!");
                }
                else
                {
                        ret = new Vector3(float.Parse(xyz[0]), float.Parse(xyz[1]), float.Parse(xyz[2]));
                }
                return ret;
        }

        public static Vector3[] ConvertToVector3Array(string posstr)
        {
                string[] pos = posstr.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                Vector3[] m_pos = new Vector3[pos.Length];

                for (int i = 0; i < pos.Length; i++)
                {
                        m_pos[i] = ConvertToVector3(pos[i]);
                }
                return m_pos;
        }


        public static Vector2 ConvertToVector2(string posstr)
        {
                Vector2 ret = Vector2.zero;
                string[] xy = posstr.Split(',');
                if (xy.Length != 2)
                {
                        Debuger.LogError("ConvertToVector2 " + posstr + " error!");
                }
                else
                {
                        ret = new Vector2(float.Parse(xy[0]), float.Parse(xy[1]));
                }
                return ret;
        }

        public static Rect ConvertToRect(string posstr)
        {
                Rect ret = new Rect(0, 0, 0, 0);
                char[] sp = { ',' };
                string[] xyzw = posstr.Split(sp);
                if (xyzw.Length != 4)
                {
                        Debuger.LogError("Postion error!!!");
                }
                else
                {
                        ret = new Rect(float.Parse(xyzw[0]), float.Parse(xyzw[1]), float.Parse(xyzw[2]), float.Parse(xyzw[3]));
                }
                return ret;
        }

        public static Color ConvertToColor(string colorStr)
        {
                int rgb = int.Parse(colorStr, System.Globalization.NumberStyles.HexNumber);
                int r = rgb / 0x10000;
                int g = (rgb % 0x10000) / 0x100;
                int b = rgb % 0x100;
                //Debuger.Log("r=" + r + " g=" + g + " b=" + b);
                return new Color(r / 255.0f, g / 255.0f, b / 255.0f);
        }

        public static Color32 ConvertToColor32(string colorStr)
        {
                Color32 m_color = new Color32();
                char[] sp = { ',' };
                string[] rgba = colorStr.Split(sp);
                if (rgba.Length != 4)
                {
                        Debuger.LogError("color error!!!");
                }
                else
                {
                        m_color = new Color32(byte.Parse(rgba[0]), byte.Parse(rgba[1]), byte.Parse(rgba[2]), byte.Parse(rgba[3]));
                }
                return m_color;
        }


        public static Color32 [] ConvertToColor32Array(string colorStr)
        {
                string[] color = colorStr.Split(new char[]{';'},StringSplitOptions.RemoveEmptyEntries);

                Color32[] m_color = new Color32[color.Length];

                for (int i = 0; i < color.Length; i++)
                {
                        m_color[i] = ConvertToColor32(color[i]);
                }
                return m_color;
        }

        public static float[] ConvertToFloatArray(string valueStr)
        {
                string[] value = valueStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                float[] m_value = new float[value.Length];

                for (int i = 0; i < value.Length; i++)
                {
                        m_value[i] = float.Parse(value[i]);
                }
                return m_value;
        }


        public static Vector4[] ConvertToVector4Array(string Str)
        {
                string[] vector = Str.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                Vector4[] m_vector = new Vector4[vector.Length];

                for (int i = 0; i < vector.Length; i++)
                {
                        m_vector[i] = ConvertToVector4(vector[i]);
                }
                return m_vector;
        }

        public static Vector4 ConvertToVector4(string str)
        {

                Vector4 ret = Vector4.zero;
                char[] sp = { ',' };
                string[] xyzw = str.Split(sp);
                if (xyzw.Length != 4)
                {
                }
                else
                {
                        ret = new Vector4(float.Parse(xyzw[0]), float.Parse(xyzw[1]), float.Parse(xyzw[2]), float.Parse(xyzw[3]));
                }
                return ret;
        }

        public static XmlNode GetNodeInnerText(XmlNode node, string nodename, ref string output, string defValue)
        {
                output = defValue;
                XmlNode childNode = node[nodename];// node.SelectSingleNode(nodename);
                if (childNode != null)
                {
                        if (!string.IsNullOrEmpty(childNode.InnerText))
                        {
                                output = childNode.InnerText;                                
                        }
                }
                else
                {
                        //Debuger.LogWarning("text node " + nodename + " not find !!!");
                }
                return childNode;
        }

        public static void GetNodeInnerBool(XmlNode node, string nodename, ref bool output, bool defValue)
        {
                output = defValue;
                XmlNode childNode = node[nodename];

                if (childNode != null)
                {
                        bool.TryParse(childNode.InnerText, out output);
                }
                else
                {
                        //Debuger.LogWarning("text node " + nodename + " not find !!!");
                }
        }


        public static void GetNodeInnerFloat(XmlNode node, string nodename, ref float output, float defValue)
        {
                XmlNode childNode = node[nodename];
                if (childNode != null)
                {
                        try
                        {
                                output = float.Parse(childNode.InnerText);
                        }
                        catch (FormatException e)
                        {
                                output = defValue;
                        }
                }
                else
                {
                        output = defValue;
                        //Debuger.LogWarning("float node " + nodename + " not find !!!");
                }
        }

        public static void GetNodeInnerInt(XmlNode node, string nodename, ref int output, int defValue)
        {
                XmlNode childNode = node[nodename];
                if (childNode != null)
                {
                        try
                        {
                                output = int.Parse(childNode.InnerText);
                        }
                        catch (FormatException e)
                        {
                                output = defValue;
                        }
                }
                else
                {
                        output = defValue;
                        //Debuger.LogWarning("Int node " + nodename + " not find !!!");
                }
        }


        public static void GetNodeInnerLong(XmlNode node, string nodename, ref long output, long defValue)
        {
                XmlNode childNode = node[nodename];
                if (childNode != null)
                {
                        try
                        {
                                output = long.Parse(childNode.InnerText);
                        }
                        catch (FormatException e)
                        {
                                output = defValue;
                        }
                }
                else
                {
                        output = defValue;
                        //Debuger.LogWarning("Long node " + nodename + " not find !!!");
                }
        }

        public static List<string> GetNodeInnerStrList(XmlNode node, string nodename, char splitchar, int minCount = 1)
        {
                List<string> list = new List<string>();
                XmlNode childNode = node[nodename];
                if (childNode != null)
                {
                        try
                        {
                                string tmp = childNode.InnerText;
                                if (tmp.Contains(splitchar.ToString()))
                                {
                                        string[] tmps = tmp.Split(splitchar);
                                        list = new List<string>(tmps);
                                }
                                else
                                {
                                        list = new List<string>(new string[] { tmp });
                                }
                        }
                        catch (FormatException e)
                        {
                        }
                }
                else
                {
                        //Debuger.LogWarning("Int node " + nodename + " not find !!!");
                }
                return list;
        }

        public static List<int> GetNodeInnerIntList(XmlNode node, string nodename, char splitchar, int minCount = 1)
        {
                List<int> list = new List<int>();
                XmlNode childNode = node[nodename];
                if (childNode != null)
                {
                        try
                        {
                                string tmp = childNode.InnerText;
                                if (tmp.Contains(splitchar.ToString()))
                                {
                                        string[] tmps = tmp.Split(splitchar);
                                        if (tmps.Length >= minCount)
                                        {
                                                foreach(string val in tmps)
                                                {
                                                        if (!string.IsNullOrEmpty(val))
                                                        {
                                                                list.Add(int.Parse(val));
                                                        }
                                                }
                                        }
                                }
                                else 
                                {
                                        if (!string.IsNullOrEmpty(tmp))
                                        {
                                                int val = 0;
                                                int.TryParse(tmp, out val);
                                                list.Add(val);
                                        }
                                }
                        }
                        catch (FormatException e)
                        {
                        }
                }
                else
                {
                        //Debuger.LogWarning("Int node " + nodename + " not find !!!");
                }
                return list;
        }

        public static void GetNodeInnerVec3(XmlNode node, string nodename, ref Vector3 output, Vector3 defValue)
        {
                XmlNode childNode = node[nodename];
                if (childNode != null)
                {
                        try
                        {
                                output = ConvertToVector3(childNode.InnerText);
                        }
                        catch (FormatException e)
                        {
                                output = defValue;
                        }
                }
                else
                {
                        output = defValue;
                        //Debuger.LogWarning("Int node " + nodename + " not find !!!");
                }
        }

        public static void GetNodeInnerVec2(XmlNode node, string nodename, ref Vector2 output, Vector2 defValue)
        {
                XmlNode childNode = node[nodename];
                if (childNode != null)
                {
                        try
                        {
                                output = ConvertToVector2(childNode.InnerText);
                        }
                        catch (FormatException e)
                        {
                                output = defValue;
                        }
                }
                else
                {
                        output = defValue;
                        //Debuger.LogWarning("Int node " + nodename + " not find !!!");
                }
        }


        public static Dictionary<string, string> GetXMLContent(XmlNode node)
        {
                Dictionary<string, string> content = new Dictionary<string, string>(); 
                for (int i = 0; i < node.ChildNodes.Count; i++)
                {
                        XmlNode child = node.ChildNodes.Item(i);
                        if (!content.ContainsKey(child.Name))
                        {
                                content.Add(child.Name, child.InnerText);
                        }            
                }
                return content;
        }

        #endregion //ConvertMethod
}
