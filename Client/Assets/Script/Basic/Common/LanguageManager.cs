using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
public class LanguageManager
{
        static Dictionary<string, string> m_LanguageDict = new Dictionary<string, string>();
        public static void Init()
        {
                ConfigHoldUtility<ConfigBase>.LoadKeyValueDict("Config/language", m_LanguageDict);
        }

        public static bool HasText(string key)
        {
                return m_LanguageDict.ContainsKey(key);
        }

        public static string GetText(string key)
        {
                if (string.IsNullOrEmpty(key))
                        return "";

                if (m_LanguageDict.ContainsKey(key))
                {
                        return m_LanguageDict[key];
                }
                return key;
        }
}
