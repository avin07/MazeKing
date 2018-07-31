using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Message;

class LoginManager : SingletonObject<LoginManager>
{
    public string IP;
    public int Port;
    public string m_sAccount;
    private string m_sPassword;
    public string m_sName;

    public void Init()
    {
#if UNITY_STANDALONE
        XmlDocument xml = new XmlDocument();
        XmlReaderSettings settings = new XmlReaderSettings();
        settings.IgnoreComments = true;
        XmlReader reader = XmlReader.Create("AccountConfig.xml", settings);
        xml.Load(reader);

        XmlNode root = xml["dataroot"];
        XMLPARSE_METHOD.GetNodeInnerText(root, "IP", ref IP, "");
        XMLPARSE_METHOD.GetNodeInnerInt(root, "Port", ref Port, 7777);

        XMLPARSE_METHOD.GetNodeInnerText(root, "Account", ref m_sAccount, "");
        XMLPARSE_METHOD.GetNodeInnerText(root, "Password", ref m_sPassword, "");
        XMLPARSE_METHOD.GetNodeInnerText(root, "Name", ref m_sName, "");

        string path = "";
        XMLPARSE_METHOD.GetNodeInnerText(root, "ResourcePath", ref path, "");
        if (!string.IsNullOrEmpty(path))    //pc版本本地加载//
        {
            ResourceManager.LOCAL_PATH = path;
            Debug.Log("LocalPath " + path);
        }
        ResourceManager.LOCAL_PATH = Application.dataPath + "/../assetbundles/";
#elif UNITY_ANDROID || UNITY_IOS
                Port =7777;
                m_sPassword = "123456";
                ResourceManager.LOCAL_PATH = Application.streamingAssetsPath + "/";
#endif
    }
}
