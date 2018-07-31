using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CombineChildrenEx))]
public class CubeManager : MonoBehaviour
{
        CombineChildrenEx m_CC;

        public bool bLog = false;
        void Awake()
        {
                m_CC = GetComponent<CombineChildrenEx>();
                if (m_CC == null)
                {
                        m_CC = this.gameObject.AddComponent<CombineChildrenEx>();
                }
        }

        Dictionary<int, Mesh> m_MeshDict = new Dictionary<int, Mesh>();   //记录每个位置的MeshID
        Dictionary<int, KeyValuePair<int, int>> m_ShadowMeshList = new Dictionary<int, KeyValuePair<int, int>>();         //投影MeshID, <投影砖id,被投影砖id>
        int AreaIndex(int x, int z)
        {
                return HomeManager.GetInst().GetBrickArea(x,z);
        }
        int GetMeshID(int x, int y, int z)
        {
                return (y * 10000 + x * 100 + z) * 10;
        }

        void AddShadowMesh(int belongMeshId, int x, int y, int z, CUBE_SHADOW_STATE css, int shadowedMeshID)
        {
                int mfId = belongMeshId + (int)css  + 1;
                if (!m_ShadowMeshList.ContainsKey(mfId))
                {
                        MeshFilter shadowMF = ModelResourceManager.GetInst().GetCommonShadowMF(css);
                        if (shadowMF != null)
                        {
                                shadowMF.transform.localPosition = new Vector3(x, y, z);
                                m_CC.AddMesh(shadowMF.mesh, shadowMF.transform.localToWorldMatrix, shadowMF.transform.GetComponent<Renderer>().sharedMaterial, 0, mfId,false, false);
                                //记录该投影mesh对应的投影砖和被投影砖关系
                                m_ShadowMeshList.Add(mfId, new KeyValuePair<int, int>(belongMeshId, shadowedMeshID));
                                if (bLog)
                                {
                                        Debuger.Log("m_ShadowMeshList.Add " + mfId + " pair=" + belongMeshId + "," + shadowedMeshID + " " + css);
                                }
                        }
                }
        }

        public void AddWallShadowMesh(int x, int z, int height, CUBE_SHADOW_STATE state,Transform father)
        {
                int mfId = GetMeshID(x, 1, z) + (int)state + 1;
                MeshFilter shadowMF = ModelResourceManager.GetInst().GetCommonShadowMF(state);
                if (shadowMF != null)
                {
                        shadowMF.transform.position = new Vector3(x, 1 + height, z);
                        m_CC.AddMesh(shadowMF.mesh, shadowMF.transform.localToWorldMatrix, shadowMF.transform.GetComponent<Renderer>().sharedMaterial, 0, mfId, false, false, father);
                }                             
        }

        /// <summary>
        /// 检查被自己影响的阴影
        /// </summary>
        void CheckAffectShadow(int meshId, int x, int y, int z, ulong linkState)
        {
                for (CUBE_SHADOW_STATE css = CUBE_SHADOW_STATE.WS; css <= CUBE_SHADOW_STATE.NE; css++)
                {
                        if (GameUtility.IsFlagOn(linkState, (int)css + 9))
                        {
                                int shadowedMeshID = GetMeshID(x - 1 + (int)css % 3, y - 1, z - 1 + (int)css / 3);
                                AddShadowMesh(meshId, x, y, z, css, shadowedMeshID);
                        }
                }
        }
        void CheckBeAffectedShadow(int shadowedMeshID, int x, int y, int z)
        {
                //如果存在比自己高的砖，则不算阴影（实际上不会）
                if (m_MeshDict.ContainsKey(GetMeshID(x, y + 1, z)))
                {
                        return;
                }
                int idx = 0;
                for (int tz = z - 1; tz <= z + 1; tz++)
                {
                        for (int tx = x - 1; tx <= x + 1; tx++)
                        {
                                if (idx == 4 || tx < 0 || tz < 0 || tx >= HomeManager.HomeSize || tz >= HomeManager.HomeSize)
                                {
                                        idx++;
                                        continue;
                                }

                                int belongMeshId = GetMeshID(tx, y + 1, tz);
                                if (m_MeshDict.ContainsKey(belongMeshId))
                                {
                                        CUBE_SHADOW_STATE css = (CUBE_SHADOW_STATE)((int)CUBE_SHADOW_STATE.NE - idx);
                                        AddShadowMesh(belongMeshId, tx, y + 1, tz, css, shadowedMeshID);
                                }
                                idx++;
                        }
                }
        }


        public ulong GetWallLink(int x, int y, int z, Dictionary<int, int> WallDict, int build_size)
        {
                ulong bit = 0;
                int idx = -1;
                int height;
                int index;
                for (int tz = z - 1; tz <= z + 1; tz++)
                {
                        for (int tx = x - 1; tx <= x + 1; tx++)
                        {
                                idx++;

                                if (idx == 4 || tx < 0 || tz < 0 || tx >= build_size || tz >= build_size)
                                {
                                        continue;
                                }

                                index = tz + 100 * tx;
                                if (WallDict.ContainsKey(index))
                                {
                                        height = WallDict[index];
                                }
                                else
                                {
                                        height = 0;
                                }

                                if (height >= y)
                                {
                                        bit |= ((ulong)1 << idx);
                                }
                                else if (height == y - 1)
                                {
                                        bit |= ((ulong)1 << (idx + 9));
                                }
                        }
                }

                index = z + 100 * x;
                if (WallDict.ContainsKey(index))
                {
                        height = WallDict[index];
                }
                else
                {
                        height = 0;
                }
                if (y < height)   //上面有
                {
                        bit |= (1 << 4);
                }

                return bit;
        }

        public void Reset()
        {
                m_MeshDict.Clear();
                m_ShadowMeshList.Clear();
                m_CC.Clean();
        }

        public void AddMod(int modelId, int x, int y, int z,ulong linkState = 0)
        {
                int meshId = GetMeshID(x, y, z);
                //Debuger.Log("AddMod " + meshId);

                if (m_MeshDict.ContainsKey(meshId))
                {
                        RemoveMod(x, y, z, false);
                        //return;
                }
                Material shareMat = ModelResourceManager.GetInst().GetCommonShareMat(modelId);
                if (shareMat != null)
                {
                        string branch = GameUtility.ConvertLinkBit(linkState);
                        MeshFilter meshFilter = ModelResourceManager.GetInst().GetCommonMeshFilter(branch);
                        if (meshFilter != null)
                        {
                                meshFilter.transform.localPosition = new Vector3(x, y, z);
                                ModelConfig cfg = ModelResourceManager.GetInst().GetModelCfg(modelId);

                                if (cfg.material_index > 0)
                                {
                                        Mesh mesh = Mesh.Instantiate(meshFilter.mesh) as Mesh;
                                        GameUtility.SetMeshUVIndex(mesh, cfg.material_index - 1, 8);
                                        m_CC.AddMesh(mesh, meshFilter.transform.localToWorldMatrix, shareMat, AreaIndex(x, z), meshId, y >= 3, y >= 2);

                                        m_MeshDict.Add(meshId, mesh);
                                }
                                else
                                {
                                        m_CC.AddMesh(meshFilter.mesh, meshFilter.transform.localToWorldMatrix, shareMat, AreaIndex(x, z), meshId, y >= 3, y >= 2);
                                        m_MeshDict.Add(meshId, meshFilter.mesh);
                                }
                                if (y >= 3)
                                {
                                        CheckAffectShadow(meshId, x, y, z, linkState);
                                }
                                if (y >= 2)
                                {
                                        CheckBeAffectedShadow(meshId, x, y, z);
                                }
                        }
                        else
                        {
                                //Debuger.Log(branch);
                        }
                }
        }

        public void RemoveMod(int x, int y, int z, bool bRemoveShadow = true)
        {
                int meshId = GetMeshID(x, y, z);
                //Debuger.Log("RemoveMod " + meshId);
                if (m_MeshDict.ContainsKey(meshId))
                {
                        m_CC.RemoveMesh(meshId);
                        m_MeshDict.Remove(meshId);

                        if (bRemoveShadow)
                        {
                                //清除自己投影出去的mesh
                                for (int i = 1; i < 10; i++)
                                {
                                        if (m_ShadowMeshList.ContainsKey(meshId + i))
                                        {
                                                m_CC.RemoveMesh(meshId + i);
                                                m_ShadowMeshList.Remove(meshId + i);
                                        }
                                }

                                //清除自己是被投影砖时的投影mesh
                                List<int> toremove = new List<int>();
                                var iter = m_ShadowMeshList.GetEnumerator();
                                while (iter.MoveNext())
                                {
                                        if (iter.Current.Value.Value == meshId)
                                        {
                                                toremove.Add(iter.Current.Key);
                                        }
                                }
                                for (int i = 0; i < toremove.Count; i++)
                                {
                                        m_CC.RemoveMesh(toremove[i]);
                                        m_ShadowMeshList.Remove(toremove[i]);
                                }
                        }
                }
        }

        public void ManualCombine(Transform root, bool bCastShadow , bool bReceiveShadow)
        {
                m_CC.ManualCombine(root, bCastShadow, bReceiveShadow);
        }

}
