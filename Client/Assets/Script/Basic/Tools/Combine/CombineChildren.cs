using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/*
Attach this script as a parent to some game objects. The script will then combine the meshes at startup.
This is useful as a performance optimization since it is faster to render one big mesh than many small meshes. See the docs on graphics performance optimization for more info.

Different materials will cause multiple meshes to be created, thus it is useful to share as many textures/material as you can.
*/

[AddComponentMenu("Mesh/Combine Children")]
public class CombineChildren : MonoBehaviour
{
        public List<Renderer> m_DisableRendererList = new List<Renderer>();
        public List<GameObject> m_CombineMeshObjs = new List<GameObject>();

        public bool IsDeleteDisable = false;
        public void Setup(bool bDeleteDisable)
        {
                IsDeleteDisable = bDeleteDisable;
                Start();
        }

        public void Reset()
        {
                bHandled = false;
                foreach (Renderer renderer in m_DisableRendererList)
                {
                        if (renderer != null)
                        {
                                renderer.gameObject.SetActive(true);
                                renderer.enabled = true;
                        }
                }
                m_DisableRendererList.Clear();
                foreach (GameObject obj in m_CombineMeshObjs)
                {
                        GameObject.Destroy(obj);
                }
                m_CombineMeshObjs.Clear();

                Start();
        }

        /// Usually rendering with triangle strips is faster.
        /// However when combining objects with very low triangle counts, it can be faster to use triangles.
        /// Best is to try out which value is faster in practice.
        public bool generateTriangleStrips = true;

        bool bHandled = false;
        Dictionary<Material, List<MeshCombineUtility.MeshInstance>> m_MeshInstanceDict = new Dictionary<Material, List<MeshCombineUtility.MeshInstance>>();
        /// This option has a far longer preprocessing time at startup but leads to better runtime performance.
        public void Start()
        {
                if (bHandled)
                        return;

                bHandled = true;
                Component[] filters = GetComponentsInChildren(typeof(MeshFilter));

                Matrix4x4 myTransform = transform.worldToLocalMatrix;
                
                Dictionary<string, Material> sharedMatDict = new Dictionary<string, Material>();
                for (int i = 0; i < filters.Length; i++)
                {
                        MeshFilter filter = (MeshFilter)filters[i];

                        if (filter.gameObject.CompareTag(CommonString.interactiveStr))
                                continue;

                        if (filter.gameObject.name.Contains(CommonString.combinedStr))
                        {
                                continue;
                        }

                        if (filter.gameObject == this.gameObject)
                        {
                                Debuger.Log(this.gameObject.name);
                                continue;
                        }

                        Renderer curRenderer = filters[i].GetComponent<Renderer>();
                        MeshCombineUtility.MeshInstance instance = new MeshCombineUtility.MeshInstance();
                        instance.mesh = filter.mesh;
                        instance.layer = filter.gameObject.layer;
                        
                        if (curRenderer != null && curRenderer.enabled && instance.mesh != null)
                        {
                                instance.transform = myTransform * filter.transform.localToWorldMatrix;

                                Material[] materials = curRenderer.sharedMaterials;
                                for (int m = 0; m < materials.Length; m++)
                                {
                                        instance.subMeshIndex = System.Math.Min(m, instance.mesh.subMeshCount - 1);
                                        if (materials[m] != null)
                                        {
                                                string matname = materials[m].name;
                                                if (!sharedMatDict.ContainsKey(matname))
                                                {
                                                        sharedMatDict.Add(matname, materials[m]);
                                                        
                                                }
                                                Material sharedMat = sharedMatDict[matname];

                                                if (!m_MeshInstanceDict.ContainsKey(sharedMat))
                                                {
                                                        m_MeshInstanceDict.Add(sharedMat, new List<MeshCombineUtility.MeshInstance>());
                                                }
                                                m_MeshInstanceDict[sharedMat].Add(instance);
                                        }
                                }

                                curRenderer.enabled = false;
                                m_DisableRendererList.Add(curRenderer);
                        }
                }
                DoCombine();
                if (IsDeleteDisable)
                {
//                         foreach (Renderer renderer in m_DisableRendererList)
//                         {
//                                 if (renderer.transform.parent.name.Contains(("(Clone)")))
//                                 {
//                                         if (renderer.transform.parent.GetComponentsInChildren<Light>() == null)
//                                         {
//                                                 GameObject.Destroy(renderer.transform.parent.gameObject);
//                                         }
//                                 }
//                                 else
//                                 {
//                                         if (renderer.transform.GetComponentsInChildren<Light>() == null)
//                                         {
//                                                 GameObject.Destroy(renderer.transform);
//                                         }
//                                 }
//                         }
//                        m_DisableRendererList.Clear();
                }
        }

        void DoCombine()
        {

                Material mat;
                var m_MeshInstance_Enumerator = m_MeshInstanceDict.GetEnumerator();
                try
                {
                        while (m_MeshInstance_Enumerator.MoveNext())
                        {
                                mat = m_MeshInstance_Enumerator.Current.Key;
                                MeshCombineUtility.MeshInstance[] instances = m_MeshInstanceDict[mat].ToArray();
                                {
                                        GameObject go = new GameObject();
                                        go.transform.parent = transform;
                                        go.transform.localScale = Vector3.one;
                                        go.transform.localRotation = Quaternion.identity;
                                        go.transform.localPosition = Vector3.zero;
                                        MeshFilter filter = go.AddComponent<MeshFilter>();
                                        go.AddComponent(typeof(MeshRenderer));
                                        go.GetComponent<Renderer>().material = mat;
                                        go.name = CommonString.combinedStr + go.GetComponent<Renderer>().sharedMaterial.name;
                                        filter.mesh = MeshCombineUtility.Combine(instances, generateTriangleStrips);
                                        go.layer = instances[0].layer;
                                        m_CombineMeshObjs.Add(go);


                                        if (go.GetComponent<Renderer>().sharedMaterial.name.Contains("water"))
                                        {
                                                UVScroller uvs = go.AddComponent<UVScroller>();
                                                uvs.scrollSpeed = -0.3f;

                                        }
                                }
                        }
                }
                finally
                {
                        m_MeshInstance_Enumerator.Dispose();
                }

        }

        public void RemoveMesh(Mesh mesh)
        {
                List<MeshCombineUtility.MeshInstance> list = new List<MeshCombineUtility.MeshInstance>();
                var m_MeshInstance_Enumerator = m_MeshInstanceDict.GetEnumerator();
                try
                {
                        while (m_MeshInstance_Enumerator.MoveNext())
                        {
                                list = m_MeshInstance_Enumerator.Current.Value;
                                for (int i = 0; i < list.Count; i++)
                                {
                                        if (list[i].mesh == mesh)
                                        {
                                                list.Remove(list[i]);
                                                break;
                                        }
                                }
                        }
                }
                finally
                {
                        m_MeshInstance_Enumerator.Dispose();
                }

                for (int i = 0; i < m_CombineMeshObjs.Count; i++)
                {
                        GameObject.Destroy(m_CombineMeshObjs[i]);
                }

                m_CombineMeshObjs.Clear();
                DoCombine();
        }
}