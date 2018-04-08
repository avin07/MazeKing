using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

class UIManager : SingletonObject<UIManager>
{
        Font mFont0;          //普通字体
        public Font MicrosoftFont
        {
                get
                {
                        if (mFont0 == null)
                        {
                                mFont0 = Resources.Load("Font/Font0", typeof(Font)) as Font;                                
                                //Debuger.Log(m_Font.name);
                        }
                        return mFont0;
                }
        }

         
        Font mFont1;       // 字母和数字
        public Font LithographFont
        {
                get
                {
                        if (mFont1 == null)
                        {
                                mFont1 = Resources.Load("Font/Lithograph Bold", typeof(Font)) as Font;
                                //Debuger.Log(m_Font.name);
                        }
                        return mFont1;
                }
        }

        GameObject mUIRoot;
        public GameObject UIRoot
        {
                get
                {
                        if (mUIRoot == null)
                        {
                                mUIRoot = GameObject.Find("UIRoot");
                                if (mUIRoot == null)
                                {
                                        mUIRoot = new GameObject("UIRoot");
                                }
                        }
                        return mUIRoot;
                }
        }

        Dictionary<string, UIBehaviour> m_UIBehaviourDict = new Dictionary<string, UIBehaviour>();      //实例化出来的界面

        public Object LoadUI(string name)
        {
                Object uiObj = null;
                uiObj = ResourceManager.GetInst().Load("UI/" + name, AssetResidentType.Temporary);                       
                return uiObj;
        }

        public T ShowUI<T>(float time = 0.2f) where T : UIBehaviour  //单例界面使用
        {
                string name = typeof(T).Name;
                //Debug.Log("ShowUI " + name);
                return ShowUI<T>(name, time);
        }

        public T ShowUI<T>(string name,float time = 0.2f) where T : UIBehaviour  //单例界面使用
        {                
                if (!m_UIBehaviourDict.ContainsKey(name))
                {
                        GameObject obj = GameObject.Instantiate(LoadUI(name)) as GameObject;
                        obj.name = name;
                        obj.transform.SetParent(UIRoot.transform);

                        T uis = obj.GetComponent<T>();
                        if (uis == null)
                        {
                                uis = obj.AddComponent<T>();  //没有就客户端主动去绑定脚本（去掉在ui工程下也要建立同名脚本的操作）
                        }
                        uis.OnShow(time);

                        m_UIBehaviourDict.Add(name, uis);                        
                        return uis;
                }
                else
                {
                        if (!m_UIBehaviourDict[name].gameObject.activeSelf)
                        {
                            m_UIBehaviourDict[name].gameObject.SetActive(true);
                        }
                        return (T)m_UIBehaviourDict[name];
                }
        }

        public void CloseAllUI()
        {
                List<string> NeedRemove = new List<string>();
                foreach (string name in m_UIBehaviourDict.Keys)
                {
                        if (name == "UI_SceneLoading")
                                continue;

                        NeedRemove.Add(name);
                }
                for (int i = 0; i < NeedRemove.Count; i++)
                {
                        CloseUI(NeedRemove[i]);
                }
        }

        public GameObject ShowUI_Multiple<T>(string name) where T : UIBehaviour
        {
                GameObject obj = GameObject.Instantiate(LoadUI(name)) as GameObject;
                T uis = obj.GetComponent<T>();
                if (uis == null)
                {
                        uis = obj.AddComponent<T>();  //没有就客户端主动去绑定脚本（去掉在ui工程下也要建立同名脚本的操作）
                }
                uis.OnShow(0);       
                return obj;
        }

        public bool IsUIVisible(string name)
        {
                if (m_UIBehaviourDict.ContainsKey(name))
                {
                    if (m_UIBehaviourDict[name].gameObject.activeSelf)
                    {
                        return true;
                    }
                }
                return false;
        }

        public bool HasAnyUIOpen()
        {
                if (m_UIBehaviourDict.Count > 0)
                {
                        return true;
                }
                return false;
        }
        public bool HasNotMainUIOpen()
        {
                var UIBehaviourDict_Enumerator = m_UIBehaviourDict.GetEnumerator();
                try
                {
                        while (UIBehaviourDict_Enumerator.MoveNext())
                        {
                                UIBehaviour uis = UIBehaviourDict_Enumerator.Current.Value;
                                if (uis != null)
                                {
                                        if (uis.UILevel > UIBehaviour.UI_LEVEL.MAIN && uis.IsVisible())
                                        {
                                                return true;
                                        }
                                }
                        }
                }
                finally
                {
                        UIBehaviourDict_Enumerator.Dispose();
                }
                return false;
        }

        public bool HasNormalUIOpen()
        {
            var UIBehaviourDict_Enumerator = m_UIBehaviourDict.GetEnumerator();
            try
            {
                    while (UIBehaviourDict_Enumerator.MoveNext())
                    {
                            UIBehaviour uis = UIBehaviourDict_Enumerator.Current.Value;
                            if (uis != null)
                            {
                                    if (uis.UILevel == UIBehaviour.UI_LEVEL.NORMAL && uis.IsVisible())
                                    {
                                            return true;
                                    }
                            }
                    }
            }
            finally
            {
                    UIBehaviourDict_Enumerator.Dispose();
            }
            return false;
        }


        public bool HasTipUIOpen()
        {
                var UIBehaviourDict_Enumerator = m_UIBehaviourDict.GetEnumerator();
                try
                {
                        while (UIBehaviourDict_Enumerator.MoveNext())
                        {
                                UIBehaviour uis = UIBehaviourDict_Enumerator.Current.Value;
                                if (uis != null)
                                {
                                        if (uis.UILevel == UIBehaviour.UI_LEVEL.TIP && uis.IsVisible())
                                        {
                                                return true;
                                        }
                                }
                        }
                }
                finally
                {
                        UIBehaviourDict_Enumerator.Dispose();
                }
                return false;
        }

        public void CloseUI(string name,float time = 0.5f)
        {
                //Debuger.Log("CloseUI " + name);
                if (m_UIBehaviourDict.ContainsKey(name))
                {
                        m_UIBehaviourDict[name].OnClose(time);
                        //AudioManager.GetInst().PlaySE("SE_Cancel");
                }
        }

        public void RemoveUI(string name)
        {
                if (m_UIBehaviourDict.ContainsKey(name))
                {
                        m_UIBehaviourDict.Remove(name);
                }
        }


        public void SetUIActiveState<T>(string name, bool isActive) where T : UIBehaviour
        {
                if (m_UIBehaviourDict.ContainsKey(name))
                {
                        m_UIBehaviourDict[name].gameObject.SetActive(isActive);
                        if (!isActive)
                        {
                                GuideManager.GetInst().CheckUIGuideClose(m_UIBehaviourDict[name].gameObject);                        
                        }
                        else
                        {
                                GuideManager.GetInst().CheckUIGuide(name);
                        }
                }
        }


        public void SetUIRender<T>(string name, bool isRender) where T : UIBehaviour
        {
                if (m_UIBehaviourDict.ContainsKey(name))
                {
                        m_UIBehaviourDict[name].gameObject.GetComponent<Canvas>().enabled = isRender;
                }
        }

        public GameObject GetUIObj(string name)
        {
                if (m_UIBehaviourDict.ContainsKey(name))
                {
                        return m_UIBehaviourDict[name].gameObject;
                }
                return null;
        }

        public T GetUIBehaviour<T>() where T : UIBehaviour
        {
                var UIBehaviourDict_Enumerator = m_UIBehaviourDict.GetEnumerator();
                try
                {
                        while (UIBehaviourDict_Enumerator.MoveNext())
                        {
                                if (UIBehaviourDict_Enumerator.Current.Value is T)
                                {
                                        return (T)UIBehaviourDict_Enumerator.Current.Value;
                                }
                        }
                }
                finally
                {
                        UIBehaviourDict_Enumerator.Dispose();
                }
                return null;

                //string name = typeof(T).Name;
                //if (m_UIBehaviourDict.ContainsKey(name))
                //{
                //        return (T)m_UIBehaviourDict[name];
                //}
                //return null;
        }

        public void CloseAllNormalWindow()
        {
                List<string> NeedRemove = new List<string>();
                foreach (string name in m_UIBehaviourDict.Keys)
                {
                        if (m_UIBehaviourDict[name].UILevel == UIBehaviour.UI_LEVEL.NORMAL)
                        {
                                NeedRemove.Add(name);
                        }
                }
                for (int i = 0; i < NeedRemove.Count; i++)
                {
                        CloseUI(NeedRemove[i]);
                }
        }
}