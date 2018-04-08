using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using Message;

class singleton : SingletonBehaviour<singleton>
{
        bool m_debug = false;

        IEnumerator Start()
        {
                if (Application.isEditor || LoginManager.GetInst().IP.Contains("192.168.0"))
                {
                        m_debug = true;
                }
                yield return new WaitForSeconds(5.0f);
        }




        #region 过滤关键字初始化
        public void OnLoadMaskWordConfig(UnityEngine.Object textAsset)
        {
                //MaskWord.GetInst().InitKeyWord(textAsset as MaskWordHold);

        }
        #endregion 过滤关键字初始化

        public void ClearLog()
        {
                mDebugLogStr = "";
        }

        int m_nBackupIndex = 0;
        List<string> m_BackupText = new List<string>();
        float timeClick = 3f;
        bool bAudio = false;
        void Update()
        {

                timeClick -= Time.deltaTime;
                if (timeClick <= 0)
                {
                        timeClick = 3f;
                }


                if (Input.GetKeyUp(KeyCode.KeypadMinus))
                {
                        mDebugLogStr = "";
                }

                if (Input.GetKeyUp(KeyCode.S))
                {
                        bAudio = !bAudio;
                        AudioController.Instance.DisableAudio = bAudio;
                }


                if (!string.IsNullOrEmpty(errorLog))
                {
                        Debuger.LogError(errorLog);
                        errorLog = "";
                }

#if UNITY_STANDALONE
                if (UIManager.GetInst().IsUIVisible("UI_Chat"))
                {
                        UI_Chat uis = UIManager.GetInst().GetUIBehaviour<UI_Chat>();

                        if (Input.GetKeyUp(KeyCode.Return))
                        {
                                uis.FocusInputField();
                        }
                        if (m_BackupText.Count > 0)
                        {
                                if (Input.GetKeyUp(KeyCode.UpArrow))
                                {
                                        if (m_nBackupIndex < 0 || m_nBackupIndex >= m_BackupText.Count)
                                        {
                                                m_nBackupIndex = (m_nBackupIndex + m_BackupText.Count) % m_BackupText.Count;
                                        }
                                        uis.SetChatText(m_BackupText[m_nBackupIndex]);
                                        m_nBackupIndex--;
                                }
                                if (Input.GetKeyUp(KeyCode.DownArrow))
                                {
                                        if (m_nBackupIndex < 0 || m_nBackupIndex >= m_BackupText.Count)
                                        {
                                                m_nBackupIndex = (m_nBackupIndex + m_BackupText.Count) % m_BackupText.Count;
                                        }
                                        uis.SetChatText(m_BackupText[m_nBackupIndex]);
                                        m_nBackupIndex++;
                                }
                        }
                }

#endif

        }

        public void AddBackupText(string text)
        {
                if (!m_BackupText.Contains(text))
                {
                        m_BackupText.Add(text);
                        m_nBackupIndex = m_BackupText.Count - 1;
                }
        }


        public static string errorLog = "";

        string mDebugLogStr = "";
        GUIStyle _style;
        GUIStyle mStyle
        {
                get
                {
                        if (_style == null)
                        {
                                _style = new GUIStyle();
                                _style.normal.textColor = Color.white;
                                _style.fontSize = 30;
                                _style.fontStyle = FontStyle.Bold;
                        }
                        return _style;
                }
        }

        public void ShowMessage(string str)
        {
                //#if _DebugVersion

//                 if (mDebugLogStr.Contains(str))
//                         return;

                Debuger.LogError(str);
                mDebugLogStr += str + "\n";
                logMsg.Enqueue(str);
                //#endif
        }

        Queue<string> logMsg = new Queue<string>();


        public void ShowMessage(ErrorOwner mOwner, string str)
        {
                //#if _DebugVersion
                if (mDebugLogStr.Contains(str))
                        return;

                switch (mOwner)
                {
                        case ErrorOwner.artist:
                                str = "美术:" + str;
                                break;
                        case ErrorOwner.designer:
                                str = "策划:" + str;
                                break;
                        case ErrorOwner.exception:
                                str = "错误：" + str;
                                break;
                        case ErrorOwner.gametest:
                                str = "数据测试: " + str;
                                break;
                        case ErrorOwner.server:
                                str = "服务器说: " + str;
                                break;
                        default:
                                break;
                }
                Debuger.LogError(str);
                mDebugLogStr += str + "\n";
                logMsg.Enqueue(str);

        }

#if UNITY_STANDALONE || UNITY_EDITOR
        void OnGUI()
        {
                if (!m_debug)
                        return;

                if (mDebugLogStr != null && mDebugLogStr != "")
                {
                        GUI.depth = -2500;
                        string tmpThreadLog = ThreadDebugLog.GetLog();
                        if (!string.IsNullOrEmpty(tmpThreadLog))
                                mDebugLogStr += tmpThreadLog + "\n";
                        GUI.Label(new Rect(0.0f, 30.0f, Screen.width, Screen.height), mDebugLogStr, mStyle);
                }

        }
#endif

}
enum ErrorOwner
{
        artist = 0,
        designer,
        exception,
        gametest,
        server,
}

class ThreadDebugLog
{
        static string _str;

        public static void ShowMessage(string log)
        {
                lock (_str)
                {
                        _str += log + "\n";
                }

        }

        public static string GetLog()
        {
                string tmpStr = _str;
                if (!string.IsNullOrEmpty(_str))
                {
                        lock (_str)
                        {
                                _str = "";
                        }
                }
                return tmpStr;
        }

}