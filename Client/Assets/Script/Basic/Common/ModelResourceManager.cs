using UnityEngine;
using System.Collections;
using System.Collections.Generic;

class ModelResourceManager : SingletonObject<ModelResourceManager>
{
    public const string MODEL_COMMON_SET = "model_common_set";
    Dictionary<int, ModelConfig> m_ModelDict = new Dictionary<int, ModelConfig>();
    Dictionary<int, ModelAccessoriesConfig> m_AccessoriesDict = new Dictionary<int, ModelAccessoriesConfig>();
    public void Init()
    {
        ConfigHoldUtility<ModelConfig>.LoadXml("Config/model_para", m_ModelDict);
        ConfigHoldUtility<ModelAccessoriesConfig>.LoadXml("Config/model_accessories", m_AccessoriesDict);
        //Debuger.Log("ModelConfig Count=" + m_ModelDict.Count);
    }
    public List<ModelConfig> GetAllCommonBricks()
    {
        List<ModelConfig> list = new List<ModelConfig>();
        foreach (ModelConfig cfg in m_ModelDict.Values)
        {
            if (cfg.model_resource.Contains(MODEL_COMMON_SET))
            {
                list.Add(cfg);
            }
        }
        return list;
    }

    public ModelConfig GetModelCfg(int id)
    {
        if (m_ModelDict.ContainsKey(id))
        {
            return m_ModelDict[id];
        }
        return null;
    }
    public ModelAccessoriesConfig GetAccessoryCfg(int id)
    {
        if (m_AccessoriesDict.ContainsKey(id))
        {
            return m_AccessoriesDict[id];
        }
        return null;
    }
    public float GetScale(int modelId)
    {
        if (m_ModelDict.ContainsKey(modelId))
        {
            return m_ModelDict[modelId].scale;
        }
        return 1f;
    }

    public string GetActionId(int id, int actionid)
    {
        if (m_ModelDict.ContainsKey(id))
        {
            if (actionid > 0 && actionid <= m_ModelDict[id].actionids.Count)
            {
                return m_ModelDict[id].actionids[actionid - 1];
            }
        }
        return "";
    }
    public string GetActionHitTime(int id, int actionid)
    {
        if (m_ModelDict.ContainsKey(id))
        {
            if (actionid > 0 && actionid <= m_ModelDict[id].actionTimes.Count)
            {
                return m_ModelDict[id].actionTimes[actionid - 1];
            }
        }
        return "";
    }

    public string GetModelRes(int id)
    {
        if (m_ModelDict.ContainsKey(id))
        {
            return m_ModelDict[id].model_resource;
        }
        return "";
    }


    public string GetIconRes(int id)
    {
        if (m_ModelDict.ContainsKey(id))
        {
            if (!string.IsNullOrEmpty(m_ModelDict[id].icon_resource))
            {
                return m_ModelDict[id].icon_resource;
            }
        }
        return "";
    }

    public void GenerateAccessory(GameObject mainMod, int id)
    {
        ModelAccessoriesConfig cfg = GetAccessoryCfg(id);
        if (cfg != null)
        {
            Object obj = ResourceManager.GetInst().Load("Character/" + cfg.accessories_name, AssetResidentType.Temporary);
            if (obj != null)
            {
                GameObject partMod = GameObject.Instantiate(obj) as GameObject;
                Transform belongT = GameUtility.GetTransform(mainMod, cfg.bone_name);
                if (belongT == null)
                {
                    belongT = mainMod.transform;

                    Debuger.Log("GenerateAccessory Error:" + cfg.bone_name + " is not exist" + " " + mainMod.name);
                }
                partMod.transform.SetParent(belongT);

                //                                 partMod.transform.localScale = Vector3.one;
                partMod.transform.localPosition = Vector3.zero;
                //                                 partMod.transform.localRotation= Quaternion.identity;

                //Debuger.Log("GenerateAccessory " + cfg.accessories_name);
            }
        }
    }
    GameObject GenerateCharacterMod(int modelId)
    {
        string url = ModelResourceManager.GetInst().GetModelRes(modelId);
        if (!string.IsNullOrEmpty(url))
        {
            Object obj = ResourceManager.GetInst().Load(url, AssetResidentType.Temporary);
            if (obj != null)
            {
                GameObject mod = GameObject.Instantiate(obj) as GameObject;
                //foreach (SkinnedMeshRenderer smr in mod.GetComponentsInChildren<SkinnedMeshRenderer>())
                //{
                //        //smr.material.shader = Shader.Find("Bumped Specular");
                //}
                mod.transform.localScale = Vector3.one * GetScale(modelId);
                if (mod.GetComponentInChildren<Animation>() != null)
                {
                    mod.GetComponentInChildren<Animation>().cullingType = AnimationCullingType.AlwaysAnimate;
                }
                return mod;
            }
        }
        return null;
    }


    public GameObject GenerateObject(int modelId)
    {
        ModelConfig cfg = GetModelCfg(modelId);
        if (cfg != null)
        {
            if (cfg.model_resource.Contains("Character/"))
            {
                return GenerateCharacterMod(modelId);
            }
            else
            {
                return GenerateObject(cfg);
            }
        }
        else
        {
            //Debug.LogError(modelId);
        }
        return null;
    }

    public GameObject GetDrawingObj(int modelId)
    {
        ModelConfig cfg = GetModelCfg(modelId);
        Material mat = GetCommonShareMat(cfg.material_resource);

        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        obj.transform.localEulerAngles = new Vector3(90, 0, 0);
        GameObject.DestroyImmediate(obj.GetComponent<MeshCollider>());
        MeshRenderer m_mr = obj.GetComponent<MeshRenderer>();
        m_mr.receiveShadows = false;
        m_mr.castShadows = false;
        m_mr.sharedMaterial = mat;

        int max_pic = (int)(mat.mainTexture.width / mat.mainTexture.height);
        if (cfg.material_index > 0)
        {
            MeshFilter mf = obj.GetComponentInChildren<MeshFilter>();
            if (mf != null)
            {
                GameUtility.SetMeshUVIndex(mf.mesh, cfg.material_index - 1, max_pic);
            }
        }
        return obj;
    }

    GameObject GenerateObject(ModelConfig cfg)
    {
        if (cfg != null)
        {
            return GenerateObject(cfg.model_resource, cfg.scale);
        }
        return null;
    }

    public GameObject GenerateObject(string url, float scale = 1.0f)
    {
        if (!string.IsNullOrEmpty(url))
        {
            Object obj = ResourceManager.GetInst().Load(url);
            if (obj != null)
            {
                GameObject mod = GameObject.Instantiate(obj) as GameObject;

                mod.transform.localScale = Vector3.one * scale;
                ShareMaterialName[] smns = mod.GetComponentsInChildren<ShareMaterialName>();

                for (int i = 0; i < smns.Length; i++)
                {
                    smns[i].SetMat();
                }
                if (mod.GetComponentInChildren<Animation>() != null)
                {
                    mod.GetComponentInChildren<Animation>().cullingType = AnimationCullingType.AlwaysAnimate;
                }
                Light light = mod.GetComponentInChildren<Light>();
                if (light != null && light.type == LightType.Point)
                {
                    light.renderMode = LightRenderMode.ForceVertex;
                }
                return mod;
            }
        }

        return null;
    }


    #region COMMON_OBJECT
    const string DEF_CUBE_NAME = "model_common_";
    Dictionary<string, MeshFilter> m_CommonMeshDict = new Dictionary<string, MeshFilter>();
    Dictionary<string, MeshFilter> m_CommonShadowDict = new Dictionary<string, MeshFilter>();
    Dictionary<string, GameObject> m_CommonCubeDict = new Dictionary<string, GameObject>();
    Dictionary<string, Material> m_SharedMatDict = new Dictionary<string, Material>();

    Dictionary<string, MeshFilter> m_MixPlaneDict = new Dictionary<string, MeshFilter>();
    GameObject m_CommonCube = null;
    GameObject m_CommonShadow = null;
    GameObject m_CommonPlane = null;

    public bool IsCommonCube(int modelId)
    {
        ModelConfig cfg = GetModelCfg(modelId);
        if (cfg != null)
        {
            return cfg.model_resource.Contains(MODEL_COMMON_SET);
        }
        return false;
    }

    public void InitCommonResources()
    {
        if (m_CommonCube == null)
        {
            m_CommonCube = GameObject.Instantiate(ResourceManager.GetInst().Load("Models/model_common_set", AssetResidentType.Always)) as GameObject;
            Object.DontDestroyOnLoad(m_CommonCube);
            //m_CommonCube.hideFlags = HideFlags.HideAndDontSave;
            Transform[] Trans = m_CommonCube.GetComponentsInChildren<Transform>(true);

            for (int i = 0; i < Trans.Length; i++)
            {
                Transform trans = Trans[i];
                if (trans.parent == m_CommonCube.transform)
                {
                    trans.localPosition = Vector3.zero;
                    m_CommonCubeDict.Add(trans.name, trans.gameObject);
                    m_CommonMeshDict.Add(trans.name, trans.gameObject.GetComponentInChildren<MeshFilter>());
                }
            }
            m_CommonCube.SetActive(false);
        }
        if (m_CommonShadow == null)
        {
            m_CommonShadow = GameObject.Instantiate(ResourceManager.GetInst().Load("Models/model_common_shadow", AssetResidentType.Always)) as GameObject;
            Object.DontDestroyOnLoad(m_CommonShadow);
            //m_CommonCube.hideFlags = HideFlags.HideAndDontSave;
            Transform[] Trans = m_CommonShadow.GetComponentsInChildren<Transform>(true);

            for (int i = 0; i < Trans.Length; i++)
            {
                Transform trans = Trans[i];
                if (trans.parent == m_CommonShadow.transform)
                {
                    trans.localPosition = Vector3.zero;
                    m_CommonShadowDict.Add(trans.name, trans.gameObject.GetComponentInChildren<MeshFilter>());
                }
            }
            m_CommonShadow.SetActive(false);
        }
        if (m_CommonPlane == null)
        {
            m_CommonPlane = GameObject.Instantiate(ResourceManager.GetInst().Load("Models/model_common_plane", AssetResidentType.Always)) as GameObject;
            Object.DontDestroyOnLoad(m_CommonPlane);
            //m_CommonPlane.hideFlags = HideFlags.HideAndDontSave;
            Transform[] Trans = m_CommonPlane.GetComponentsInChildren<Transform>(true);

            for (int i = 0; i < Trans.Length; i++)
            {
                Transform trans = Trans[i];
                if (trans.parent == m_CommonPlane.transform)
                {
                    m_MixPlaneDict.Add(trans.name, trans.gameObject.GetComponentInChildren<MeshFilter>());
                }
            }
            m_CommonPlane.SetActive(false);

        }
    }

    public Material GetCommonShareMat(string matname)
    {
        if (matname.Equals("Default-Diffuse"))
        {
            return null;
        }

        Material mat = null;
        if (m_SharedMatDict.ContainsKey(matname))
        {
            return m_SharedMatDict[matname];
        }
        else
        {
            UnityEngine.Object matObj = ResourceManager.GetInst().Load("Materials/" + matname);
            if (matObj != null)
            {
                mat = Material.Instantiate(matObj) as Material;

                if (matname.Contains("water"))
                {
                    if (mat.HasProperty("_Color"))
                    {
                        Debug.Log(matname + " " + mat.GetColor("_Color"));
                    }
                }
                //Debug.Log("m_SharedMatDict.Add   " + matname);
                m_SharedMatDict.Add(matname, mat);
            }
        }
        return mat;
    }
    public Material GetCommonShareMat(int modelId)
    {
        ModelConfig cfg = GetModelCfg(modelId);
        if (cfg != null)
        {
            if (cfg.model_resource.Contains(MODEL_COMMON_SET))
            {
                return GetCommonShareMat(cfg.material_resource);
            }
        }
        return null;
    }
    public GameObject GetCommonCubeObj(string branch = "")
    {
        if (branch != "")
        {
            string name = DEF_CUBE_NAME + branch;
            if (m_CommonCubeDict.ContainsKey(name))
            {
                return GameObject.Instantiate(m_CommonCubeDict[name]) as GameObject;
            }
        }
        return null;

    }

    public MeshFilter GetCommonMeshFilter(string branch = "")
    {
        //                 if (branch == "")
        //                         return null;
        string name = DEF_CUBE_NAME + branch;
        if (m_CommonMeshDict.ContainsKey(name))
        {
            return m_CommonMeshDict[name];
        }
        //                 else if (m_CommonMeshDict.ContainsKey(DEF_CUBE_NAME))
        //                 {
        //                         return m_CommonMeshDict[DEF_CUBE_NAME];
        //                 }
        return null;
    }
    public MeshFilter GetCommonShadowMF(CUBE_SHADOW_STATE linkState)
    {
        //string name = "model_common_plane_";
        string name = string.Empty;
        switch (linkState)
        {
            case CUBE_SHADOW_STATE.N:
                name = "model_common_plane_n";
                break;
            case CUBE_SHADOW_STATE.W:
                name = "model_common_plane_w";
                break;
            case CUBE_SHADOW_STATE.S:
                name = "model_common_plane_s";
                break;
            case CUBE_SHADOW_STATE.E:
                name = "model_common_plane_e";
                break;
            case CUBE_SHADOW_STATE.NW:
                name = "model_common_plane_nw";
                break;
            case CUBE_SHADOW_STATE.WS:
                name = "model_common_plane_ws";
                break;
            case CUBE_SHADOW_STATE.SE:
                name = "model_common_plane_se";
                break;
            case CUBE_SHADOW_STATE.NE:
                name = "model_common_plane_ne";
                break;
        }

        if (m_CommonShadowDict.ContainsKey(name))
        {
            return m_CommonShadowDict[name];
        }
        return null;
    }

    public GameObject GenerateCommonObject(int modelId, string branch = "")
    {
        ModelConfig cfg = GetModelCfg(modelId);

        if (cfg != null && cfg.model_resource.Contains(MODEL_COMMON_SET))
        {
            Material mat = GetCommonShareMat(cfg.material_resource);
            string name = DEF_CUBE_NAME + branch;
            if (!m_CommonCubeDict.ContainsKey(name))
            {
                name = DEF_CUBE_NAME + "_t";
            }
            GameObject obj = null;
            if (m_CommonCubeDict.ContainsKey(name))
            {
                obj = GameObject.Instantiate(m_CommonCubeDict[name]) as GameObject;
            }
            else if (m_CommonCubeDict.ContainsKey(DEF_CUBE_NAME))
            {
                obj = GameObject.Instantiate(m_CommonCubeDict[DEF_CUBE_NAME]) as GameObject;
            }
            if (obj != null)
            {
                Renderer renderer = obj.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = mat;
                }
                if (cfg.material_index > 0)
                {
                    MeshFilter mf = obj.GetComponentInChildren<MeshFilter>();
                    if (mf != null)
                    {
                        GameUtility.SetMeshUVIndex(mf.mesh, cfg.material_index - 1, 8);
                    }
                }
            }
            return obj;
        }
        else
        {
            return ModelResourceManager.GetInst().GenerateObject(modelId);
        }

    }

    public MeshFilter GenerateCommonPlane(string direct)
    {
        string name = "model_common_plane_" + direct;

        if (m_MixPlaneDict.ContainsKey(name))
        {
            return m_MixPlaneDict[name];
        }
        return null;
    }

    public void DestroyCommonCube()
    {
        foreach (GameObject obj in m_CommonCubeDict.Values)
        {
            GameObject.Destroy(obj);
        }
        GameObject.Destroy(m_CommonCube);
        m_CommonCubeDict.Clear();
    }
    public void DestroySharedMats()
    {
        foreach (Material mat in m_SharedMatDict.Values)
        {
            GameObject.Destroy(mat);
        }
        m_SharedMatDict.Clear();
    }
    public void DestroyCommonPlane()
    {
        //                 foreach (GameObject obj in m_MixPlaneDict.Values)
        //                 {
        //                         GameObject.Destroy(obj);
        //                 }
        //                 GameObject.Destroy(m_CommonPlane);
        m_MixPlaneDict.Clear();
    }

    #endregion
    public void OnApplicationQuit()
    {

        //                 ModelResourceManager.GetInst().DestroyCommonCube();
        //                 ModelResourceManager.GetInst().DestroySharedMats();
        //                 ModelResourceManager.GetInst().DestroyCommonPlane();
    }
}

/// <summary>
/// 标记方块的连接状态的值枚举
/// </summary>
public enum CUBE_LINKSTATE
{
    WS,
    S,
    SE,
    W,
    TOP,
    E,
    NW,
    N,
    NE,
};
public enum CUBE_SHADOW_STATE
{
    WS,
    S,
    SE,
    W,
    TOP,             //不存在，仅占位用
    E,
    NW,
    N,
    NE,
};
