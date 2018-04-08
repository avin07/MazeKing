using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Collections;

class GlobalParams
{
        static Dictionary<string, string> m_GlobalDict = new Dictionary<string, string>();

        public static void Init()
        {
               ConfigHoldUtility<ConfigBase>.LoadKeyValueDict("Config/global_value", m_GlobalDict);
//                 AssetBundleStruct m_abs = new AssetBundleStruct("Config/global_value", XmlLoadCallBack, null, ResourceManager.AssetBundleKind.Config);
//                 AppMain.GetInst().StartCoroutine(ResourceManager.GetInst().ResourceLoad(m_abs));

        }
        public static int GetInt(string name)
        {
                int ret = 0;
                int.TryParse(GetString(name), out ret);
                return ret;
        }
        public static float GetFloat(string name)
        {
                float ret = 0f;
                float.TryParse(GetString(name), out ret);
                return ret;
        }
        public static string GetString(string name)
        {
                if (m_GlobalDict.ContainsKey(name))
                {
                        return m_GlobalDict[name];
                }
                return "";
        }

        public static Vector3 GetVector2(string name)
        {
                if (m_GlobalDict.ContainsKey(name))
                {
                        return XMLPARSE_METHOD.ConvertToVector2(m_GlobalDict[name]);
                }
                return Vector3.zero;
        }

        public static Vector3 GetVector3(string name)
        {
                if (m_GlobalDict.ContainsKey(name))
                {
                        return XMLPARSE_METHOD.ConvertToVector3(m_GlobalDict[name]);
                }
                return Vector3.zero;
        }

        public static Color32 GetColor32(string name)
        {
                if (m_GlobalDict.ContainsKey(name))
                {
                        return XMLPARSE_METHOD.ConvertToColor32(m_GlobalDict[name]);
                }
                return new Color32();
        }


//         #region old
// 
//         public static void XmlLoadCallBack(WWW www, AssetBundleStruct abs)
//         {
//                 string text = (www.assetBundle.mainAsset as TextAsset).text;
//                 XmlDocument xml = new XmlDocument();
//                 xml.LoadXml(text);
//                 GetDict(xml, ref m_GlobalDict);
//         }
// 
//         static void GetDict(XmlDocument xml, ref Dictionary<string, string> dict)
//         {
//                 XmlNode root = xml.SelectSingleNode("dataroot");
//                 for (int i = 0; i < root.ChildNodes.Count; i++)
//                 {
//                         XmlNode child = root.ChildNodes.Item(i);
//                         string name = "", value = "";
//                         XMLPARSE_METHOD.GetNodeInnerText(child, "name", ref name, "");
//                         XMLPARSE_METHOD.GetNodeInnerText(child, "value", ref value, "");
//                         if (!dict.ContainsKey(name))
//                         {
//                                 dict.Add(name, value);
//                         }
//                 }
//         }
//         #endregion old
}