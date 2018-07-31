using UnityEngine;
using System.Collections;
using System.Collections.Generic;

class CombineStruct
{
        //public int MAX_COMBINE_COUNT = 400;      //������Ϊ�����嶥�����٣��������instance���Ƚϴ�ͨ�����ǿ�������<65536
        const int MAX_VERTICES = 65535;
        public Transform parentTrans;
        public Material mat;
        public int area;
        public List<int> changeIdxs = new List<int>();
        public Dictionary<int, GameObject> combinedObjs = new Dictionary<int, GameObject>();
        public List<Dictionary<int, CombineInstance>> combineList = new List<Dictionary<int, CombineInstance>>();
        public Dictionary<int, int> m_CombineVerticesCount = new Dictionary<int, int>();

        public bool IsGenerateMeshCollider;
        public bool IsCastShadow;
        public bool IsReceiveShadow;

        public CombineStruct(Transform parent, Material _mat, int _area, bool bCastShadow, bool bReceiveShadow, bool bMeshCollider)
        {
                parentTrans = parent;
                mat = _mat;
                area = _area;
                IsCastShadow = bCastShadow;
                IsReceiveShadow = bReceiveShadow;
                IsGenerateMeshCollider = bMeshCollider;
        }

        bool IsCombinelistFull(int idx, int count)
        {
                if (m_CombineVerticesCount.ContainsKey(idx))
                {
                        if (m_CombineVerticesCount[idx] + count >= MAX_VERTICES)
                        {
                                return true;
                        }
                }
                return false;
        }

        public void AddInstance(CombineInstance instance, int meshID)
        {
                int idx = 0;
                for (idx = 0; idx < combineList.Count; idx++)
                {
                        if (IsCombinelistFull(idx, instance.mesh.vertexCount))
                        {
                                continue;
                        }
                        if (!combineList[idx].ContainsKey(meshID))
                        {
                                combineList[idx].Add(meshID, instance);
                                m_CombineVerticesCount[idx] += instance.mesh.vertexCount;
                                break;
                        }                        
                }
                if (idx >= combineList.Count)
                {
                        combineList.Add(new Dictionary<int, CombineInstance>());
                        combineList[idx].Add(meshID, instance);
                        m_CombineVerticesCount.Add(idx, instance.mesh.vertexCount);
                }

                if (!changeIdxs.Contains(idx))
                {
                        changeIdxs.Add(idx);
                }
        }

        public bool RemoveInstance(int toRemoveId)
        {
                bool ret = false;
                for (int idx = 0; idx < combineList.Count; idx++)
                {
                        if (combineList[idx].ContainsKey(toRemoveId))
                        {
                                combineList[idx].Remove(toRemoveId);
                                if (!changeIdxs.Contains(idx))
                                {
                                        changeIdxs.Add(idx);
                                }
                                ret = true;
                        }
                }
                return ret;
        }

        GameObject GenerateCombineObj(Material mat, int area, int index)
        {
                GameObject go = new GameObject("Combined_" + mat.name + "#" + area);
                go.transform.parent = parentTrans;
                go.transform.localScale = Vector3.one;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localPosition = Vector3.zero;
                go.AddComponent<MeshFilter>();
                MeshRenderer mr = go.AddComponent<MeshRenderer>();
                mr.material.shader = mat.shader;
                mr.material.CopyPropertiesFromMaterial(mat);
                mr.castShadows = IsCastShadow;
                mr.receiveShadows = IsReceiveShadow;
                return go;
        }

        public void CombineAll()
        {
                if (changeIdxs.Count > 0)
                {
                        int idx;
                        for (int i = 0; i < changeIdxs.Count; i++)
                        {
                                idx = changeIdxs[i];
                                if (combineList[idx].Count > 0)
                                {
                                        CombineSingleMaterial(idx);
                                }
                                else if (combinedObjs.ContainsKey(idx))
                                {
                                        GameObject.Destroy(combinedObjs[idx]);
                                        combinedObjs.Remove(idx);
                                }
                        }
                        changeIdxs.Clear();
                }
        }

        void CombineSingleMaterial(int index)
        {
                if (combineList[index].Count <= 0)
                {
                        return;
                }

                GameObject go = null;
                if (combinedObjs.ContainsKey(index))
                {
                        go = combinedObjs[index];
                        if (go == null)
                        {
                                go = GenerateCombineObj(mat, area, index);
                        }
                }
                else
                {
                        go = GenerateCombineObj(mat, area, index);
                        combinedObjs.Add(index, go);
                }

                CombineInstance[] instArr = new CombineInstance[combineList[index].Count];
                int i = 0;
                var tmpEnum = combineList[index].GetEnumerator();
                try
                {
                        while (tmpEnum.MoveNext())
                        {
                                instArr[i] = tmpEnum.Current.Value;
                                i++;
                        }
                }
                finally
                {
                        tmpEnum.Dispose();
                }
                
                go.GetComponent<MeshFilter>().mesh.CombineMeshes(instArr);
                if (IsGenerateMeshCollider)
                {
                        if (go.GetComponent<MeshCollider>() == null)
                        {
                                go.AddComponent<MeshCollider>();
                        }
                }
                //Resources.UnloadUnusedAssets();
        }
}

[AddComponentMenu("Mesh/Extend Combine Children")]
public class CombineChildrenEx : MonoBehaviour
{
        Dictionary<string, Material> m_ShareMatDict = new Dictionary<string, Material>();       //������ͬ����ͬһ��
        Dictionary<Material, Dictionary<int, CombineStruct>> m_MeshInstanceDict = new Dictionary<Material, Dictionary<int, CombineStruct>>();

        public bool HasMaterial(string name)
        {
                if (m_ShareMatDict.ContainsKey(name))
                {
                        return true;
                }
                return false;
        }
        public Material GetShareMat(string name)
        {
                if (m_ShareMatDict.ContainsKey(name))
                {
                        return m_ShareMatDict[name];
                }
                return null;
        }

        Matrix4x4 myTransform
        {
                get
                {
                        return transform.worldToLocalMatrix;
                }
        }

        public void ManualCombine()
        {
                Component[] filters = GetComponentsInChildren<MeshFilter>(true);
                toremove.Clear();
                foreach (MeshFilter filter in filters)
                {
                        if (filter.gameObject.name.Contains("Combined_"))
                        {
                                continue;
                        }

                        if (filter.gameObject == this.gameObject)
                        {
                                continue;
                        }
                        if (filter.gameObject.CompareTag("Interactive"))
                        {
                                continue;
                        }

                        if (AddFilter(filter, 0,true,true,null))
                        {
                                toremove.Add(filter.transform.gameObject);
                        }
                }
                for (int i = 0; i < toremove.Count; i++)
                {
                        GameObject.Destroy(toremove[i], Time.deltaTime);
                }
        }

        List<GameObject> toremove = new List<GameObject>();
        public void ManualCombine(Transform root, bool bCastShadow, bool bReceiveShadow)
        {
                toremove.Clear();
                Transform child;
                MeshFilter filter;
                for (int i = 0; i < root.childCount; i++)  //ɾ�������ڵ�������������ڵĽڵ�//
                {
                        child = root.GetChild(i);
                        SkinnedMeshRenderer[] skins = child.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                        Animator[] anima = child.GetComponentsInChildren<Animator>(true);
                        ParticleSystem[] ps = child.GetComponentsInChildren<ParticleSystem>(true);
                        if (skins.Length == 0 && anima.Length == 0 && ps.Length == 0)  //SkinnedMeshRenderer���ж����Ĳ��ϲ�,������Ч��Ҳ
                        {
                                toremove.Add(child.gameObject);
                                MeshFilter[] filters = child.GetComponentsInChildren<MeshFilter>(true);
                                for (int j = 0; j < filters.Length; j++)
                                {
                                        filter = filters[j];
                                        AddFilter(filter, 0, bCastShadow, bReceiveShadow, root);
                                }
                        }
                }
                for (int i = 0; i < toremove.Count; i++)
                {
                        GameObject.Destroy(toremove[i],Time.deltaTime);
                }
        }

        public void Update()
        {
                DoCombine();
        }

        bool AddFilter(MeshFilter filter, int area, bool bCastShadow , bool bReceiveShadow , Transform father)
        {
                Renderer curRenderer = filter.GetComponent<Renderer>();
                if (curRenderer != null && curRenderer.enabled && filter.mesh != null)
                {
                        for (int i = 0; i < curRenderer.sharedMaterials.Length; i++)
                        {
                                if (curRenderer.sharedMaterials[i] != null)
                                {
                                        AddMesh(filter.mesh, filter.transform.localToWorldMatrix, curRenderer.sharedMaterials[i], area, filter.mesh.GetInstanceID(), bCastShadow, bReceiveShadow, father, i);
                                }
                        }
                        return true;
                }
                return false;
        }
        public void DoCombine()
        {
                Material mat;
                int area;
                var m_MeshInstance_Enumerator = m_MeshInstanceDict.GetEnumerator();
                try
                {
                        while (m_MeshInstance_Enumerator.MoveNext())
                        {
                                mat = m_MeshInstance_Enumerator.Current.Key;
                                var m_MeshInstanceMat_Enumerator = m_MeshInstanceDict[mat].GetEnumerator();
                                try
                                {
                                        while (m_MeshInstanceMat_Enumerator.MoveNext())
                                        {
                                                area = m_MeshInstanceMat_Enumerator.Current.Key;
                                                m_MeshInstanceDict[mat][area].CombineAll();
                                        }
                                }
                                finally
                                {
                                        m_MeshInstanceMat_Enumerator.Dispose();
                                }
                        }
                }
                finally
                {
                        m_MeshInstance_Enumerator.Dispose();
                }

                //ȥ��gc
                //foreach (Material mat in m_MeshInstanceDict.Keys)
                //{
                //        foreach (int area in m_MeshInstanceDict[mat].Keys)
                //        {
                //                m_MeshInstanceDict[mat][area].CombineAll();
                //        }
                //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="matrix"></param>
        /// <param name="shareMat"></param>
        /// <param name="area"></param>
        /// <param name="meshID"></param>
        public void AddMesh(Mesh mesh, Matrix4x4 matrix, Material shareMat, int area, int meshID, bool bCastShadow = true, bool bReceiveShadow = true, Transform father = null, int subCount = 0, bool bAddCollider = false)
        {
                CombineInstance instance = new CombineInstance();
                instance.mesh = mesh;
                instance.transform = myTransform * matrix;
                instance.subMeshIndex = subCount;
                string matName = shareMat.name;
                if (father == null)
                {
                        father = this.transform;
                }
                if (!m_ShareMatDict.ContainsKey(matName))
                {
                        m_ShareMatDict.Add(matName, shareMat);
                }
                Material sharedMat = m_ShareMatDict[matName];

                if (!m_MeshInstanceDict.ContainsKey(sharedMat))
                {
                        m_MeshInstanceDict.Add(sharedMat, new Dictionary<int, CombineStruct>());
                }
                if (!m_MeshInstanceDict[sharedMat].ContainsKey(area))
                {
                        m_MeshInstanceDict[sharedMat].Add(area, new CombineStruct(father, sharedMat, area, bCastShadow, bReceiveShadow, bAddCollider));
                }
                m_MeshInstanceDict[sharedMat][area].AddInstance(instance, meshID);
        }

        public void Clean()
        {
                m_ShareMatDict.Clear();
                m_MeshInstanceDict.Clear();
        }
        
        public void RemoveMesh(int meshID)
        {
                var m_MeshInstance_Enumerator = m_MeshInstanceDict.GetEnumerator();
                try
                {
                        while (m_MeshInstance_Enumerator.MoveNext())
                        {
                                var m_MeshInstanceMat_Enumerator = m_MeshInstanceDict[m_MeshInstance_Enumerator.Current.Key].GetEnumerator();
                                try
                                {
                                        while (m_MeshInstanceMat_Enumerator.MoveNext())
                                        {
                                                if (m_MeshInstanceMat_Enumerator.Current.Value.RemoveInstance(meshID))
                                                {
                                                        return;
                                                }
                                        }
                                }
                                finally
                                {
                                        m_MeshInstanceMat_Enumerator.Dispose();
                                }
                        }
                }
                finally
                {
                        m_MeshInstance_Enumerator.Dispose();
                }
        }
}

