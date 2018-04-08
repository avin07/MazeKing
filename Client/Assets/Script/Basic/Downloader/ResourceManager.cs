//#define USE_ASYN_LOAD
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine.UI;

public enum AssetResidentType
{
    Never,      //不存储
    Temporary, //切换场景时清理
    Always,  // 永远存在
}

public class ResourceManager : SingletonObject<ResourceManager>
{
    public static string LOCAL_PATH = "";
    public static string LOCAL_ADD_PATH = Application.persistentDataPath + CommonString.divideStr;

    public struct AssetBundleStruct
    {
        public AssetBundle ab;
        public AssetResidentType residentType;

    }


    Dictionary<string, AssetBundleStruct> m_ABDict = new Dictionary<string, AssetBundleStruct>();

    public void UnloadAllAB()
    {
        UIManager.GetInst().CloseAllUI();
        List<string> NeedRemove = new List<string>();
        foreach (string url in m_ABDict.Keys)
        {
            if (m_ABDict[url].residentType < AssetResidentType.Always)
            {
                if (m_ABDict[url].ab != null)
                {
                    m_ABDict[url].ab.Unload(true);
                }
                NeedRemove.Add(url);
            }
        }
        for (int i = 0; i < NeedRemove.Count; i++)
        {
            m_ABDict.Remove(NeedRemove[i]);
        }

        AudioManager.GetInst().ReleaseAll();
        ModelResourceManager.GetInst().DestroySharedMats();
        GameUtility.StopAllDoTween();   //关闭所有tween
        Resources.UnloadUnusedAssets(); //卸载不使用的assets
    }


    public AssetBundle LoadAB(string relate_path, AssetResidentType residentType = AssetResidentType.Never)
    {
        if (m_ABDict.ContainsKey(relate_path))
        {
            return m_ABDict[relate_path].ab;
        }

        string cached_path = LOCAL_PATH + relate_path + ".unity3d";
        if (File.Exists(cached_path))
        {
            return DownLoadAB(cached_path, relate_path, residentType);
        }
        else
        {
            cached_path = LOCAL_ADD_PATH + relate_path + ".unity3d";
            if (File.Exists(cached_path))
            {
                return DownLoadAB(cached_path, relate_path, residentType);
            }
        }
        singleton.GetInst().ShowMessage(ErrorOwner.designer, relate_path + "不存在！");
        return null;
    }

    AssetBundle DownLoadAB(string url, string relate_path, AssetResidentType residentType)
    {
        AssetBundle ab = AssetBundle.LoadFromFile(url);
        if (!m_ABDict.ContainsKey(relate_path) && residentType > AssetResidentType.Never)
        {
            AssetBundleStruct asb;
            asb.residentType = residentType;
            asb.ab = ab;
            m_ABDict.Add(relate_path, asb);
        }
        Debug.Log("DownloadAb " + url);
        return ab;
    }

    public Object Load(string path, AssetResidentType residentType = AssetResidentType.Never, string name = "")
    {
        Object obj_ret = null;
        AssetBundle ab = LoadAB(path, residentType);
        if (ab != null)
        {

            if (name.Length > 0)
            {
                obj_ret = ab.LoadAsset(name);
            }
            else
            {
                obj_ret = ab.mainAsset;
            }
            if (residentType == AssetResidentType.Never)
            {
                //ab.Unload(false);     //unity2017需要延迟unload资源。
            }
        }

        if (obj_ret == null)
        {
            obj_ret = Resources.Load(path);
        }
        if (obj_ret == null)
        {
            Debuger.LogWarning("MxResMgr:Load failed: " + path);
        }
        return obj_ret;
    }

    /// <summary>
    /// 读取Icon
    /// </summary>
    /// <param name="spriteName"></param>
    /// <param name="ts"></param>
    public void LoadIconSpriteSyn(string spriteName, Transform ts) //icon统一使用ab方式
    {
        if (string.IsNullOrEmpty(spriteName))
        {
            if (ts != null)
            {
                Image im = ts.GetComponent<Image>();
                if (im != null)
                {
                    im.enabled = false;
                }
            }
            return;
        }

        string atlasName = "";
        Sprite sprite = null;
        if (spriteName.Contains(CommonString.poundStr))
        {
            atlasName = spriteName.Split('#')[0];
            string url = "Sprite/Icon/" + atlasName;
            Object obj = Load(url, AssetResidentType.Temporary, spriteName);
            sprite = obj as Sprite;
            if (sprite == null)
            {
                singleton.GetInst().ShowMessage(ErrorOwner.designer, "图标" + spriteName + "不存在");
                Debug.Log(url);
            }
        }
        else
        {
            singleton.GetInst().ShowMessage(ErrorOwner.designer, "图标" + spriteName + "表里名字格式不正确！");
        }

        if (ts != null)
        {
            Image im = ts.GetComponent<Image>();
            if (im != null)
            {
                if (sprite == null)
                {
                    im.enabled = false;
                }
                else
                {
                    im.enabled = true;
                }

                im.sprite = sprite;

            }
            else
            {
                if (ts.GetComponent<SpriteRenderer>() != null)
                {
                    ts.GetComponent<SpriteRenderer>().sprite = sprite;
                }
            }
        }
    }


    public void LoadIconSpriteSyn(string spriteName, Image im) //icon统一使用ab方式
    {
        if (string.IsNullOrEmpty(spriteName))
        {
            if (im != null)
            {
                im.enabled = false;
            }
            return;
        }

        string atlasName = "";
        Sprite sprite = null;
        if (spriteName.Contains(CommonString.poundStr))
        {
            atlasName = spriteName.Split('#')[0];
            string url = "Sprite/Icon/" + atlasName;
            Object obj = Load(url, AssetResidentType.Temporary, spriteName);
            sprite = obj as Sprite;
            if (sprite == null)
            {
                singleton.GetInst().ShowMessage(ErrorOwner.designer, "图标" + spriteName + "不存在");
                //Debug.Log(url);
            }
        }
        else
        {
            singleton.GetInst().ShowMessage(ErrorOwner.designer, "图标" + spriteName + "表里名字格式不正确！");
        }
        if (sprite == null)
        {
            im.enabled = false;
        }
        else
        {
            im.enabled = true;
        }
        im.sprite = sprite;
    }

    public static XmlDocument LoadAssetXmlDocument(string path)
    {
        XmlDocument xml = new XmlDocument();
        string fullpath = Application.dataPath + "/Data/" + path + ".xml";
        Debuger.Log(fullpath);
        if (File.Exists(fullpath))
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            XmlReader reader = XmlReader.Create(fullpath, settings);
            xml.Load(reader);
        }
        return xml;
    }
    /// <summary>
    /// 读取Xml文件
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static XmlDocument LoadXmlDocument(string path)
    {
        XmlDocument xml = new XmlDocument();
        //#if UNITY_EDITOR
        if (File.Exists(Application.dataPath + "/../" + path + ".xml"))
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            XmlReader reader = XmlReader.Create(Application.dataPath + "/../" + path + ".xml", settings);
            xml.Load(reader);
        }
        else
        {
            Object obj = ResourceManager.GetInst().Load(path);
            xml.LoadXml((obj as TextAsset).text);
        }
        //#else
        //                  AssetBundle ab = ResourceManager.GetInst().LoadAB(path);
        //                  xml.LoadXml((ab.mainAsset as TextAsset).text);
        //#endif
        return xml;
    }

#if USE_ASYN_LOAD
    #region 加载管理(统一使用www加载)
        public struct AssetsStruct
        {
                public AssetResidentType ResidentType;
                public Object Assets;
                public AssetsStruct(AssetResidentType m_type, Object m_asssets)
                {
                        ResidentType = m_type;
                        Assets = m_asssets;
                }

        }

        public Dictionary<string, Dictionary<string, AssetsStruct>> m_ResidentAssetBundle = new Dictionary<string, Dictionary<string, AssetsStruct>>();  //常驻内存中的资源//
        public Dictionary<string, List<AssetBundleStruct>> m_AbLib = new Dictionary<string, List<AssetBundleStruct>>();  //资源还没加载完成时又发起了该资源的加载//
        public Dictionary<AssetBundleKind, List<string>> m_KindUrl = new Dictionary<AssetBundleKind, List<string>>();

        bool isConnetNet = false;

        public Object GetAssetFromAssetbundle(string url, string assetName)
        {
                Dictionary<string, AssetsStruct> dict = GetAssetbundle(url);
                if (dict != null)
                {
                        if (!string.IsNullOrEmpty(assetName))
                        {
                                if (dict.ContainsKey(assetName))
                                {
                                        return dict[assetName].Assets;
                                }
                        }
                        else if (dict.Count > 0)
                        {
                                foreach (AssetsStruct obj in dict.Values)
                                {
                                        return obj.Assets;
                                }
                        }
                        else
                        {
                                Debuger.Log("here");
                        }
                }
                else
                {
                        Debuger.Log("here");
                }
                return null;
        }

        public Dictionary<string, AssetsStruct> GetAssetbundle(string url)
        {
                if (m_ResidentAssetBundle.ContainsKey(url))
                {
                        return m_ResidentAssetBundle[url];
                }
                return null;
        }

        //要求代码中始终保证finishCallBack不为null
        public IEnumerator ResourceLoad(AssetBundleStruct m_abs)
        {
                string url = m_abs.url;

                if (m_ABDict.ContainsKey(m_abs.ori_url))  //处理同步加载的冲突//
                {
                        m_ABDict[m_abs.ori_url].Unload(false);
                        m_ABDict.Remove(m_abs.ori_url);
                }


                if (!m_AbLib.ContainsKey(url))   //存储多回调//
                {
                        List<AssetBundleStruct> temp = new List<AssetBundleStruct>();
                        temp.Add(m_abs);
                        m_AbLib.Add(url, temp);
                }
                else
                {
                        m_AbLib[url].Add(m_abs);  //相同加载请求先挂起,等到加载成功后在处理所有回调//
                        yield break;
                }

                //存储类型//
                if (!m_KindUrl.ContainsKey(m_abs.kind))
                {
                        List<string> temp = new List<string>();
                        temp.Add(url);
                        m_KindUrl.Add(m_abs.kind, temp);
                }
                else
                {
                        m_KindUrl[m_abs.kind].Add(url);
                }


                //Debuger.Log("开始加载资源" + url);
                WWW www = new WWW(url);

                while (www != null && !www.isDone)
                {
                        if (m_abs.loadingCallBack != null)
                        {
                                m_abs.loadingCallBack(www);   //用来处理loading界面//
                        }
                        yield return null;
                }
                if (www != null && www.error == null && www.isDone)
                {
                        //Debuger.Log("资源加载完成" + url);

                        if (m_abs.residentType > AssetResidentType.Never)  //常驻内存//
                        {
                                if (!m_ResidentAssetBundle.ContainsKey(url))
                                {
                                        Dictionary<string, AssetsStruct> m_dic = new Dictionary<string, AssetsStruct>();
                                        if (m_abs.kind == AssetBundleKind.Model)//模型类型的资源只取mainasset保存索引
                                        {
                                                Object obj = www.assetBundle.mainAsset;
                                                AssetsStruct ass = new AssetsStruct(m_abs.residentType, obj);
                                                m_dic.Add(obj.name, ass);
                                        }
                                        else
                                        {
                                                Object[] m_object = www.assetBundle.LoadAll();
                                                for (int i = 0; i < m_object.Length; i++)
                                                {
                                                        AssetsStruct ass = new AssetsStruct(m_abs.residentType, m_object[i]);
                                                        m_dic.Add(m_object[i].name, ass);
                                                }
                                        }
                                        m_ResidentAssetBundle.Add(url, m_dic);
                                }
                        }

                        if (m_abs.finishCallBack != null)
                        {
                                m_abs.finishCallBack(www, m_abs);
                        }
                        //完成其他的回调//
                        if (m_AbLib[url].Count > 1)
                        {
                                for (int i = 1; i < m_AbLib[url].Count; i++)
                                {
                                        if (m_AbLib[url][i].finishCallBack != null)
                                        {
                                                m_AbLib[url][i].finishCallBack(www, m_AbLib[url][i]);
                                        }
                                }
                        }

                        if (m_abs.kind == AssetBundleKind.Sence) //释放临时资源
                        {
                                List<string> m_clean_url = new List<string>();
                                foreach (string m_url in m_ResidentAssetBundle.Keys)
                                {
                                        foreach (AssetsStruct m_ass in m_ResidentAssetBundle[m_url].Values)
                                        {
                                                if (m_ass.ResidentType < AssetResidentType.Forever)
                                                {
                                                        m_clean_url.Add(m_url);
                                                        break;
                                                }
                                        }
                                }

                                for (int i = 0; i < m_clean_url.Count; i++)
                                {
                                        m_ResidentAssetBundle.Remove(m_clean_url[i]);
                                }
                                Resources.UnloadUnusedAssets(); //清理内存//
                        }

                        m_AbLib.Remove(url);
                        m_KindUrl[m_abs.kind].Remove(url);

                        www.assetBundle.Unload(false);
                        //Debuger.Log("释放" + url);

                        //保证congfig全部下载完成以后再连接网络//
                        if (!isConnetNet)
                        {
                                if (m_KindUrl.ContainsKey(AssetBundleKind.Config))
                                {
                                        if (m_KindUrl[AssetBundleKind.Config].Count == 0)
                                        {
                                                //联网//
                                                Debuger.Log("配置加载完毕！");
                                                isConnetNet = true;
                                                AppMain.GetInst().FinishLoadConfig();
                                        }
                                }
                        }
                }
                else
                {
                        Debuger.LogError(www.error + ";" + url + "资源加载失败！");
                }

                www.Dispose();
                www = null;
        }

        public delegate void LoadFinishCallBack(WWW www, AssetBundleStruct abs);
        public delegate void LoadingCallBack(WWW www);

        public enum AssetBundleKind
        {
                Config,   //配置
                Icon,     //图标
                Perferb,  //预制体
                Model,    //模型
                Effect,   //特效
                Sence,    //场景
        }

        public enum AssetResidentType
        {
                Never,      //不存储
                Temporary, //切换场景时清理
                Forever,  // 永远存在
        }

    #endregion
    #region ICON 异步方法
        public void LoadIconSpriteAsyn(string spriteName, Transform ts) //icon统一使用ab方式
        {
                string atlasName = "";
                if (spriteName.Contains(CommonString.poundStr))
                {
                        atlasName = spriteName.Split('#')[0];
                }
                else
                {
                        singleton.GetInst().ShowMessage(ErrorOwner.designer, "图标" + spriteName + "表里名字格式不正确！");
                        return;
                }
                string url = "Sprite/Icon/" + atlasName;
                IconSpriteStruct iss = new IconSpriteStruct(ts, spriteName);
                AssetBundleStruct m_abs = new AssetBundleStruct(url, IconLoadCallBack, iss, ResourceManager.AssetBundleKind.Icon);
                m_abs.residentType = AssetResidentType.Temporary;  //测试//
                AppMain.GetInst().StartCoroutine(ResourceManager.GetInst().ResourceLoad(m_abs));
        }

        void IconLoadCallBack(WWW www, AssetBundleStruct abs)
        {
                IconSpriteStruct iss = (IconSpriteStruct)abs.tag;
                Sprite sprite = null;

                if (abs.residentType > AssetResidentType.Never)
                {
                        sprite = GetAssetFromAssetbundle(www.url, iss.spriteName) as Sprite;
                }
                else
                {
                        sprite = www.assetBundle.Load(iss.spriteName) as Sprite;
                }

                if (iss.ts != null)
                {
                        Image im = iss.ts.gameObject.GetComponent<Image>();
                        if (im != null)
                        {
                                im.sprite = sprite;
                                im.enabled = true;
                                return;
                        }

                        if (iss.ts.gameObject.GetComponent<SpriteRenderer>() != null)
                        {
                                iss.ts.gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
                        }
                }
        }

        struct IconSpriteStruct
        {
                public Transform ts;
                public string spriteName;
                public IconSpriteStruct(Transform m_ts, string m_spriteName)
                {
                        ts = m_ts;
                        spriteName = m_spriteName;
                }
        }

    #endregion
    #region 模型加载

        public struct ModelStruct
        {
                public string modelPath;
                public object tag;
                public ModelStruct(string _modelPath, object _tag)
                {
                        modelPath = _modelPath;
                        tag = _tag;
                }
        }

        public void LoadModel(string url, ResourceManager.LoadFinishCallBack callback, object tag)
        {
                ModelStruct mls = new ModelStruct(url, tag);
                AssetBundleStruct m_abs = new AssetBundleStruct(url, callback, mls, ResourceManager.AssetBundleKind.Model);
                m_abs.residentType = AssetResidentType.Temporary;
                AppMain.GetInst().StartCoroutine(ResourceManager.GetInst().ResourceLoad(m_abs));
        }



    #endregion
#endif
}

#if USE_ASYN_LOAD

public struct AssetBundleStruct
{
        public string url;
        public bool is_local;                                       //是否是本地资源,false为网络资源
        public UnityEngine.ThreadPriority threadPriority;           //AssetBundle解压缩线程的优先级
        public object tag;                                          //用户自定义的参数//
        public ResourceManager.LoadFinishCallBack finishCallBack;   //完成后的回调
        public ResourceManager.LoadingCallBack loadingCallBack;     //加载时的回调,将来用于loading进度条//
        public ResourceManager.AssetResidentType residentType;                                       //是否常驻内存//
        public ResourceManager.AssetBundleKind kind;                //资源类型//
        public string ori_url;

        public AssetBundleStruct(string m_url, ResourceManager.LoadFinishCallBack m_finishCallBack, object m_tag, ResourceManager.AssetBundleKind m_kind)
        {

                url = m_url;
                ori_url = m_url;
                is_local = true;
                threadPriority = ThreadPriority.Normal;
                tag = m_tag;

                finishCallBack = m_finishCallBack;
                loadingCallBack = null;
                residentType = ResourceManager.AssetResidentType.Never;
                kind = m_kind;

                if (is_local)
                {
#if UNITY_STANDALONE_WIN
                        //url = GetUrlForMobile();
                        url = "file:///" + ResourceManager.LOCAL_PATH + url + ".unity3d";
                        //Debuger.Log(url);
#elif UNITY_ANDROID
            url = GetUrlForMobile();
#elif UNITY_IPHONE
                url = GetUrlForMobile();
#endif
                }
        }

        public string GetUrlForMobile()
        {
                if (File.Exists(ResourceManager.LOCAL_ADD_PATH + url + ".unity3d"))
                {
                        url = "file:///" + ResourceManager.LOCAL_ADD_PATH + url + ".unity3d";
                }
                else
                {
                        url = ResourceManager.LOCAL_PATH + url + ".unity3d";
                }
                return url;
        }
}
#endif