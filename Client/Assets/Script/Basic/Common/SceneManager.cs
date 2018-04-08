using UnityEngine;
using System.Collections;

class SceneManager : SingletonObject<SceneManager>
{
        public void Init()
        {

        }

        int m_nSceneId;
        GameObject m_SceneRoot;
        public GameObject SceneRoot
        {
                get
                {
                        return m_SceneRoot;
                }
                set
                {
                        m_SceneRoot = value;
                }
        }

        public GameObject EnterScene(int sceneId)
        {
                if (m_SceneRoot != null)
                {
                        ExitLastScene();
                }
                
                m_nSceneId = sceneId;
                m_SceneRoot = GameObject.Instantiate(ResourceManager.GetInst().Load("Scene/" + sceneId)) as GameObject;
                Transform ambient = m_SceneRoot.transform.Find("AmbientLight");
                if (ambient != null)
                {
                        RenderSettings.ambientLight = new Color32((byte)ambient.position.x, (byte)ambient.position.y, (byte)ambient.position.z, 255);
                }
                return m_SceneRoot;
        }
        public void ExitLastScene()
        {
                if (m_SceneRoot != null)
                {
                        GameObject.Destroy(m_SceneRoot);
                        m_SceneRoot = null;
                }
        }

        public GameObject LoadGameObject(string modelPath)
        {
                Object obj = ResourceManager.GetInst().Load(modelPath);
                if (obj != null)
                {
                        return GameObject.Instantiate(obj) as GameObject;
                }
                return null;
        }

        public GameObject LoadGameObject(Object obj)
        {
                GameObject go = GameObject.Instantiate(obj) as GameObject;
                go.transform.SetParent(m_SceneRoot.transform);
                return go;
        }
}
