using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CombineChildrenEx))]
public class EditorBrickEffectManager : MonoBehaviour
{
        CombineChildrenEx m_CC;
        GameObject m_EffectObj;

        void Awake()
        {
                m_CC = GetComponent<CombineChildrenEx>();
                if (m_CC == null)
                {
                        m_CC = this.gameObject.AddComponent<CombineChildrenEx>();
                }
        }

        HashSet<int> m_MeshDict = new HashSet<int>();   //记录每个位置的MeshID
   
        int GetMeshID(int x, int y, int z,int index)  //用坐标当主键，默认预留10种不同材质//
        {
                return (y * 10000 + x * 100 + z) * 10 + index;
        }

        public void GetEffectObj(string url)
        {
                m_EffectObj = EffectManager.GetInst().GetEffectObj(url);
                m_EffectObj.transform.SetParent(transform);
                m_EffectObj.SetActive(false);              
        }

        public void Reset()
        {
                GameUtility.DestroyChild(transform);
                m_MeshDict.Clear();
                m_CC.Clean();
        }

        public void AddEffect(int x, int y, int z)
        {
                int meshId = 0 ;
                if (m_EffectObj != null)
                {
                        MeshFilter[] filters = m_EffectObj.GetComponentsInChildren<MeshFilter>(true);
                        MeshFilter filter;
                        for (int i = 0; i < filters.Length; i++)
                        {
                                filter = filters[i];
                                meshId = GetMeshID(x, y, z, i);
                                Renderer curRenderer = filter.GetComponent<Renderer>();
                                if (curRenderer != null && curRenderer.enabled && filter.mesh != null)
                                {
                                        filter.transform.localPosition = new Vector3(x, y + 0.1f, z);
                                        m_CC.AddMesh(filter.mesh, filter.transform.localToWorldMatrix, curRenderer.sharedMaterial, 0, meshId, false, false);
                                        m_MeshDict.Add(meshId);
                                }
                        }
                }
        }

        public void RemoveEffect(int x, int y, int z)
        {
                for (int i = 0; i < 10; i++)
                {
                        int meshId = GetMeshID(x, y, z,i);
                        if (m_MeshDict.Contains(meshId))
                        {
                                m_CC.RemoveMesh(meshId);
                                m_MeshDict.Remove(meshId);
                        }
                }
        }



}
