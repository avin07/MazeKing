using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class ResourceUpdater : SingletonObject<ResourceUpdater>
{
        public static readonly string VERSION_FILE = "version.txt";
        public static readonly string LOCAL_RES_PATH = Application.streamingAssetsPath + "/";

        public static string SERVER_RES_IP = "192.168.0.104";

#if UNITY_ANDROID 
        public static string SERVER_RES_URL = "/Android/assetbundles_zip/";
#elif UNITY_STANDALONE
        public static string SERVER_RES_URL = "/Windows/assetbundles_zip/";
#elif UNITY_IPHONE
        public static string SERVER_RES_URL = "/IOS/assetbundles_zip/";
#endif

        //由于移动平台的原始数据只读，所以增量更新在新的可读写的路径,android打包时写入选择sdcard!//
        public static readonly string LOCAL_ADD_RES_URL = Application.persistentDataPath + "/";

        private Dictionary<string, string> LocalResVersion;
        private Dictionary<string, string> ServerResVersion;
        private List<string> NeedDownFiles;
        private bool NeedUpdateLocalVersionFile = false;

        void DownloadStreaming()
        {
                if (NeedDownFiles.Count <= 0)
                {
                        StringBuilder versions = new StringBuilder();
                        foreach (var item in LocalResVersion)
                        {
                                versions.Append(item.Key).Append(",").Append(item.Value).Append("\n");
                        }

                        FileStream stream = new FileStream(LOCAL_ADD_RES_URL + VERSION_FILE, FileMode.Create);  //写入增量地址//
                        byte[] data = Encoding.UTF8.GetBytes(versions.ToString());
                        stream.Write(data, 0, data.Length);
                        stream.Flush();
                        stream.Close();

                        UpdateServerRes();
                        return;
                }

                string file = NeedDownFiles[0];
                NeedDownFiles.RemoveAt(0);
                {
#if UNITY_STANDALONE
                        string localFile = "file:///" + LOCAL_RES_PATH + file;
#elif UNITY_IPHONE
                        string localFile = "file:///" + LOCAL_RES_PATH + file;
#elif UNITY_ANDROID
                        string localFile = LOCAL_RES_PATH + file;
#endif
                        localFile = localFile.Replace("\\", "/");
                        //Debuger.Log("DownloadStreaming: " + localFile);

                        AppMain.GetInst().StartCoroutine(DownLoad(localFile, delegate (WWW w)
                        {
                                //将下载的资源替换本地就的资源
                                UpdateLocalRes(file, w.bytes);
                                DownloadStreaming();
                        }));
                }
        }
        UI_ResourceLoading m_UILoading;
        int m_nLocalCount = 0;
        public void StartUpdateRes()  //开始资源的对比更新//
        {                
                //初始化
                LocalResVersion = new Dictionary<string, string>();
                ServerResVersion = new Dictionary<string, string>();
                NeedDownFiles = new List<string>();

                //加载本地version配置
                string local_version_url = "";
                bool bLoadStreaming = false;
                if (File.Exists(LOCAL_ADD_RES_URL + VERSION_FILE))
                {
                        local_version_url = "file:///" + LOCAL_ADD_RES_URL + VERSION_FILE;
                }
                else
                {
                        bLoadStreaming = true;
#if UNITY_STANDALONE
                        local_version_url = "file:///" + LOCAL_RES_PATH + VERSION_FILE;
#elif UNITY_IPHONE 
                        local_version_url = "file:///" + LOCAL_RES_PATH + VERSION_FILE;
#elif UNITY_ANDROID
                        local_version_url = "jar:file://" + LOCAL_RES_PATH + VERSION_FILE;
#endif
        }

        Debuger.Log("local_version_url: " + local_version_url);
                m_UILoading = UIManager.GetInst().ShowUI<UI_ResourceLoading>("UI_ResourceLoading");
                AppMain.GetInst().StartCoroutine(DownLoad(local_version_url, delegate(WWW localVersion)
                {
                        //保存本地的version
                        ParseVersionFile(localVersion.text, LocalResVersion);
                        Debuger.LogError("LocalCount = " + LocalResVersion.Count);

                        if (LocalResVersion.Count > 0 && bLoadStreaming)
                        {
                                NeedDownFiles = new List<string>(LocalResVersion.Keys);
                                m_UILoading.SetProgressMax(NeedDownFiles.Count, true);
                                DownloadStreaming();
                        }
                        else
                        {
                                UpdateServerRes();
                        }
                }));
        }

        void UpdateServerRes()
        {
                //加载服务端version配置
                string url = "http://" + SERVER_RES_IP + SERVER_RES_URL + VERSION_FILE;
                AppMain.GetInst().StartCoroutine(DownLoad(url, delegate (WWW serverVersion)
                {
                        Debuger.Log(serverVersion.url);
                        //保存服务端version
                        ParseVersionFile(serverVersion.text, ServerResVersion);
                        Debuger.LogError("ServerCount = " + ServerResVersion.Count);
                        //计算出需要重新加载的资源
                        CompareVersion();
                        //加载需要更新的资源
                        DownLoadRes();
                }));
        }

        //依次加载需要更新的资源
        private void DownLoadRes()
        {
                if (NeedDownFiles.Count == 0)
                {
                        UpdateLocalVersionFile();
                        return;
                }

                string file = NeedDownFiles[0];
                NeedDownFiles.RemoveAt(0);
                string update_file_url = "http://" + SERVER_RES_IP + SERVER_RES_URL + file;
                update_file_url = update_file_url.Replace("\\", "/");
                Debuger.Log("update_file_url: " + update_file_url);

                AppMain.GetInst().StartCoroutine(DownLoad(update_file_url, delegate(WWW w)
                {
                        //将下载的资源替换本地就的资源
                        UpdateLocalRes(file, w.bytes);
                        DownLoadRes();
                }));
        }

        private void UpdateLocalRes(string fileName, byte[] data)  //把需要下载的新增到可读写的地址//
        {
                //Debuger.LogError(fileName + " bytes=" + data.Length);
                if (m_UILoading != null)
                {
                        m_UILoading.AddProgress(1, fileName);
                }

                fileName = fileName.Replace("\\", "/");
                string filePath = LOCAL_ADD_RES_URL + fileName;
                filePath = filePath.Replace("\\", "/");
                string directoryName = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryName))
                {
                        Debuger.Log(directoryName);
                        Directory.CreateDirectory(directoryName);
                }
                FileStream stream = new FileStream(filePath, FileMode.Create);
                stream.Write(data, 0, data.Length);
                stream.Flush();
                stream.Close();
                if (data.Length > 0)
                {
                        Debuger.Log("更新" + fileName + "成功！");
                }
                else
                {
                        Debuger.LogError("更新" + fileName + "失败！");
                }

                if (fileName.Contains(".zip"))
                {
                        GameUtility.DecompressFileLZMA(LOCAL_ADD_RES_URL + fileName, LOCAL_ADD_RES_URL + fileName.Replace(".zip", ""));
                        File.Delete(LOCAL_ADD_RES_URL + fileName);
                }
        }

        //更新本地的version配置
        private void UpdateLocalVersionFile()
        {
                if (NeedUpdateLocalVersionFile)
                {
                        StringBuilder versions = new StringBuilder();
                        foreach (var item in ServerResVersion)
                        {
                                versions.Append(item.Key).Append(",").Append(item.Value).Append("\n");
                        }

                        FileStream stream = new FileStream(LOCAL_ADD_RES_URL + VERSION_FILE, FileMode.Create);  //写入增量地址//
                        byte[] data = Encoding.UTF8.GetBytes(versions.ToString());
                        stream.Write(data, 0, data.Length);
                        stream.Flush();
                        stream.Close();
                }
                Debuger.Log("资源检查更新完毕!");
                AppMain.GetInst().InitModules(); //保证资源最新再进行config加载//
        }

        private void CompareVersion()
        {
                UnityEngine.Profiling.Profiler.BeginSample("CompareVersion");
                foreach (var version in ServerResVersion)
                {
                        string fileName = version.Key;
                        string serverMd5 = version.Value;
                        //新增的资源
                        if (!LocalResVersion.ContainsKey(fileName))
                        {
                                NeedDownFiles.Add(fileName);                                
                        }
                        else
                        {
                                //需要替换的资源
                                string localMd5;
                                LocalResVersion.TryGetValue(fileName, out localMd5);
                                if (!serverMd5.Equals(localMd5))
                                {
                                        NeedDownFiles.Add(fileName);                                        
                                }
                        }
                }
                //本次有更新，同时更新本地的version.txt
                NeedUpdateLocalVersionFile = NeedDownFiles.Count > 0;
                m_UILoading.SetProgressMax(NeedDownFiles.Count, false);

                UnityEngine.Profiling.Profiler.EndSample();
        }

        private void ParseVersionFile(string content, Dictionary<string, string> dict)
        {
                if (content == null || content.Length == 0)
                {
                        Debuger.LogError("ParseVersionFile");
                        return;
                }
                string[] items = content.Split(new char[] { '\n' });
                foreach (string item in items)
                {                        
                        string[] info = item.Split(new char[] { ',' });
                        if (info != null && info.Length == 2)
                        {
                                dict.Add(info[0], info[1]);
                        }
                }
                Debuger.LogError("ParseVersionFile " + dict.Count);
        }

        private IEnumerator DownLoad(string url, HandleFinishDownload finishFun)
        {
                WWW www = new WWW(url);
                yield return www;
                if (www == null)
                {
                        Debuger.LogError("资源服务器不存在" + url + "!");
                }
                else
                {
                        if (finishFun != null)
                        {
                                finishFun(www);
                        }
                }

                www.Dispose();
                www = null;
        }

        public delegate void HandleFinishDownload(WWW www);
}