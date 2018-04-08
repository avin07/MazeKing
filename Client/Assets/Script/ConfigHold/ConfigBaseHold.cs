using UnityEngine;
using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

[System.Serializable]
public class CommonScriptableObject : ScriptableObject
{
        public virtual void LoadAll(string configName)
        {
        }
}

[System.Serializable]
public class ConfigBase
{
        [SerializeField]
        public int id;
        public ConfigBase()
        {

        }
        public ConfigBase(XmlNode child)
        {
                XMLPARSE_METHOD.GetNodeInnerInt(child, "id", ref id, 0);
        }

        public virtual void InitSelf(XmlNode child)
        {

        }

        /// <summary>
        /// 默认的解析XML方法，如果需要自己解析，则在继承的构造函数里写，否则就重载InitSelf来条用SetupFields
        /// </summary>
        /// <param name="child"></param>
        public void SetupFields(XmlNode child)
        {
                FieldInfo[] properties = this.GetType().GetFields();

                foreach (FieldInfo info in properties)
                {
                        if (info.FieldType == typeof(string))
                        {
                                string tmp = "";
                                XMLPARSE_METHOD.GetNodeInnerText(child, info.Name, ref tmp, "");
                                info.SetValue(this, tmp);
                        }
                        else if (info.FieldType == typeof(int))
                        {
                                int tmp = 0;
                                XMLPARSE_METHOD.GetNodeInnerInt(child, info.Name, ref tmp, 0);
                                info.SetValue(this, tmp);
                        }
                        else if (info.FieldType == typeof(List<int>))
                        {
                                List<int> tmp = XMLPARSE_METHOD.GetNodeInnerIntList(child, info.Name, ',', 1);
                                info.SetValue(this, tmp);
                        }
                        else if (info.FieldType == typeof(List<string>))
                        {
                                List<string> tmp = XMLPARSE_METHOD.GetNodeInnerStrList(child, info.Name, ',', 1);
                                info.SetValue(this, tmp);
                        }
                        else if (info.FieldType == typeof(long))
                        {
                                long tmp = 0;
                                XMLPARSE_METHOD.GetNodeInnerLong(child, info.Name, ref tmp, 0);
                                info.SetValue(this, tmp);
                        }
                        else if (info.FieldType == typeof(float))
                        {
                                float tmp = 0f;
                                XMLPARSE_METHOD.GetNodeInnerFloat(child, info.Name, ref tmp, 0f);
                                info.SetValue(this, tmp);
                        }
                        else if (info.FieldType == typeof(bool))
                        {
                                bool tmp = false;
                                XMLPARSE_METHOD.GetNodeInnerBool(child, info.Name, ref tmp, false);
                                info.SetValue(this, tmp);
                        }
                        else if (info.FieldType == typeof(Vector2))
                        {
                                Vector2 tmp = Vector2.zero;
                                XMLPARSE_METHOD.GetNodeInnerVec2(child, info.Name, ref tmp, Vector2.zero);
                                info.SetValue(this, tmp);
                        }
                        else if (info.FieldType == typeof(Vector3))
                        {
                                Vector3 tmp = Vector3.zero;
                                XMLPARSE_METHOD.GetNodeInnerVec3(child, info.Name, ref tmp, Vector3.zero);
                                info.SetValue(this, tmp);
                        }
                }
        }
}

public class ConfigHoldUtility<S> where S : ConfigBase
{
        public static void LoadKeyValueDict(string path, Dictionary<string, string> dict)
        {
                XmlDocument xml = ResourceManager.LoadXmlDocument(path);
                XmlNode root = xml["dataroot"];

                //dict = new Dictionary<string, string>(root.ChildNodes.Count);

                for (int i = 0; i < root.ChildNodes.Count; i++)
                {
                        XmlNode child = root.ChildNodes.Item(i);
                        string name = "", value = "";
                        XMLPARSE_METHOD.GetNodeInnerText(child, "name", ref name, "");
                        XMLPARSE_METHOD.GetNodeInnerText(child, "value", ref value, "");
                        if (!dict.ContainsKey(name))
                        {
                                dict.Add(name, value);
                        }
                }

        }

        public static void PreLoadXml(string configPath, Dictionary<int, S> m_dic)
        {
                XmlDocument xml = ResourceManager.LoadAssetXmlDocument(configPath);
                LoadXml(xml, m_dic);
        }

        public static void LoadXml(string configPath, Dictionary<int, S> m_dic)
        {
                XmlDocument xml = ResourceManager.LoadXmlDocument(configPath);
                LoadXml(xml, m_dic);
//             AssetBundleStruct m_abs = new AssetBundleStruct(configPath, XmlLoadCallBack, m_dic, ResourceManager.AssetBundleKind.Config);
//             AppMain.GetInst().StartCoroutine(ResourceManager.GetInst().ResourceLoad(m_abs));
        }

        public static void LoadXml(XmlDocument xml,  Dictionary<int, S> dict)
        {
                XmlNode root = xml["dataroot"];
                //dict = new Dictionary<int, S>(root.ChildNodes.Count);
                for (int i = 0; i < root.ChildNodes.Count; i++)
                {
                        XmlNode child = root.ChildNodes.Item(i);
                        S info = (S)Activator.CreateInstance(typeof(S), child);
                        if (info.id > 0)
                        {
                                if (!dict.ContainsKey(info.id))
                                {
                                        info.InitSelf(child);
                                        dict.Add(info.id, info);
                                }
                                else
                                {
                                        Debuger.LogError(info.id);
                                }
                        }
                }
        }

        #region 异步old
        //public static Dictionary<int, S> LoadXmlText(string textasset, string name)
        //{
        //        XmlDocument xml = new XmlDocument();
        //        Dictionary<int, S> dict = new Dictionary<int, S>();
        //        xml.LoadXml(textasset);
        //        XmlNode root = xml["dataroot"];
        //        for (int i = 0; i < root.ChildNodes.Count; i++)
        //        {
        //                XmlNode child = root.ChildNodes.Item(i);
        //                S info = (S)Activator.CreateInstance(typeof(S), child);
        //                if (info.id > 0)
        //                {
        //                        info.InitSelf(child);
        //                        if (!dict.ContainsKey(info.id))
        //                        {
        //                                dict.Add(info.id, info);
        //                        }
        //                        else
        //                        {
        //                                //singleton.GetInst().ShowMessage(ErrorOwner.designer, name + "表中的id = " + info.id + "相同!!!");
        //                        }
        //                }
        //        }
        //        return dict;
        //}

//         public static void XmlLoadCallBack(WWW www, AssetBundleStruct abs)
//         {
//                 if (www != null)
//                 {
//                         string text = (www.assetBundle.mainAsset as TextAsset).text;
//                         Dictionary<int, S> dict = LoadXmlText(text, www.assetBundle.mainAsset.name);
//                         Dictionary<int, S> m_dict = abs.tag as Dictionary<int, S>;
//                         foreach (int key in dict.Keys)
//                         {
//                                 if (!m_dict.ContainsKey(key))
//                                 {
//                                         m_dict.Add(key, dict[key]);
//                                 }
//                                 else
//                                 {
//                                         Debuger.LogError(www.assetBundle.mainAsset.name + " " + key);
//                                 }
//                         }
//                 }
//         }
        #endregion
}