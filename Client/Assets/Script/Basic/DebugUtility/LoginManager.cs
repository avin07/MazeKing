using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
#if UNITY_EDITOR
using UnityEditor;
#endif
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
#endif
#if UNITY_ANDROID || UNITY_IPHONE
                Port =7777;
                m_sPassword = "123456";
                ResourceManager.LOCAL_PATH = Application.streamingAssetsPath + CommonString.divideStr;
#endif

        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgAccountLogin), OnAccountLogin);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgUserInfo), OnLoadUser);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRandomSeed), OnTestRandom);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgContinueUnFinishBattle), OnUnfinishBattle);
    }

    void OnAccountLogin(object sender, SCNetMsgEventArgs e)
    {
        SCMsgAccountLogin msg = e.mNetMsg as SCMsgAccountLogin;
        switch (msg.errorCode)
        {
            case (int)ERROR_CODE.SUCCESS:
                if (msg.idActor > 0)
                {
                    TryLoadUser(msg.idActor);
                }
                break;
            case (int)ERROR_CODE.NO_ACCOUNT:
                break;
            case (int)ERROR_CODE.PASSWORD_ERR:
                Debuger.LogError("OnAccountLogin PASSWORD_ERROR");
                break;
            default:
                Debuger.LogError("OnAccountLogin errorcode = " + msg.errorCode);
                break;
        }
    }

    public void TryLogin()
    {
        CSMsgAccountLogin msg = new CSMsgAccountLogin();
        msg.sAccount = m_sAccount;
        msg.sName = m_sName;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }

    public void TryLoadUser(long idActor)
    {
        CSMsgLoadUser msg = new CSMsgLoadUser();
        msg.idActor = idActor;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }

    void OnLoadUser(object sender, SCNetMsgEventArgs e)
    {
        SCMsgUserInfo msg = e.mNetMsg as SCMsgUserInfo;
        PlayerController.GetInst().LoadDataFromMsg(msg);
        //PetPieceManager.GetInst().SendPieceQue();
    }

    void OnTestRandom(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRandomSeed msg = e.mNetMsg as SCMsgRandomSeed;
        RandomCreater.seed = msg.seed;
    }

    void OnUnfinishBattle(object sender, SCNetMsgEventArgs e)
    {
        CombatManager.GetInst().HasUnfinishBattle = true;
    }

}
