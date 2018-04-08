using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class BrickSceneManager : SingletonBehaviour<BrickSceneManager>
{
        int m_nSceneSize = 100;
        public int SceneSize
        {

                get
                {
                        return m_nSceneSize;
                }
                set
                {
                        m_nSceneSize = value;
                        Debug.Log("m_nSceneSize=" + value);
                }
        }
        byte[] m_BrickHeightData; //地形海拔高度//
        Dictionary<int, Mesh> m_MeshDict = new Dictionary<int, Mesh>();   //记录每个位置的MeshID
        Dictionary<int, ModelConfig> m_MeshCfgDict = new Dictionary<int, ModelConfig>();   //记录每个位置的MeshID
        Dictionary<int, KeyValuePair<int, int>> m_ShadowMeshList = new Dictionary<int, KeyValuePair<int, int>>();         //投影MeshID, <投影砖id,被投影砖id>
        CombineChildrenEx m_CC;
        Vector3 m_vOffset;
        int OffsetX
        {
                get
                {
                        return (int)m_vOffset.x;
                }
        }
        int OffsetHeight
        {
                get
                {
                        return (int)m_vOffset.y;
                }
        }
        int OffsetY
        {
                get
                {
                        return (int)m_vOffset.z;
                }
        }

        List<List<int>> m_ModelList;

        int[,] m_MeshArray;
        //y,xxx,zzz,s（s是shadow）
        int GetMeshID(int x, int y, int z)
        {
                return (y * 1000000 + x * 1000 + z) * 10;
        }

        int GetMeshY(int meshId)
        {
                return meshId / 10 / 1000000;
        }
        int GetMeshX(int meshId)
        {
                return (meshId / 10 % 1000000) / 1000;
        }
        int GetMeshZ(int meshId)
        {
                return meshId / 10 % 1000;
        }
        public override void Awake()
        {
                base.Awake();

                m_CC = this.gameObject.AddComponent<CombineChildrenEx>();
        }

        public int SetupScene(string brickInfo, List<List<int>> modelList, Vector3 offset)
        {
                string info = GameUtility.GzipDecompress(brickInfo);
                m_BrickHeightData = GameUtility.ToByteArray(info);
                m_ModelList = modelList;
                //SetupAllBricks();
                m_vOffset = offset;
                InitMeshArray((int)Mathf.Sqrt(info.Length));
                return SceneSize;
        }

        public void InitMeshArray(int size)
        {
                SceneSize = size;

                m_MeshArray = new int[size, size];
                for (int x = 0; x < size; x++)
                {
                        for (int y = 0; y < size; y++)
                        {
                                m_MeshArray[x, y] = 0;
                        }
                }
        }

        public void ClearMeshArray(int x, int y, int height)
        {
                x -= OffsetX;
                y -= OffsetY;
                if (x >= 0 && y >= 0 && x < SceneSize && y < SceneSize)
                {
                        if (IsMeshExist(x, y, height))
                        {
                                int bit = 1 << height;
                                int nMark = 0;
                                nMark = (~nMark) ^ bit;
                                m_MeshArray[x, y] &= nMark;

                                if (x == 32 && y == 13)
                                {
                                        Debug.Log("ClearMeshArray " + height);
                                }
                        }
                }
        }
        
        public bool SetMeshArray(int x, int y, int height)
        {
                x -= OffsetX;
                y -= OffsetY;
                
                if (x >= 0 && y >= 0 && x < SceneSize && y < SceneSize)
                {
                        if (!IsMeshExist(x, y, height))
                        {
                                m_MeshArray[x, y] |= 1 << height;
                                return true;
                        }
                }
                return false;
        }

        public bool IsMeshExist(int x, int y, int height)
        {
                if (x >= 0 && x < SceneSize && y >= 0 && y < SceneSize)
                {
                        if (GameUtility.IsFlagOn(m_MeshArray[x, y], height))
                        {
                                return true;
                        }
                }
                return false;
        }
        public bool IsMeshExist(int x, int y)
        {
                if (x >= 0 && x < SceneSize && y >= 0 && y < SceneSize)
                {
                        return m_MeshArray[x, y] > 0;
                }
                return false;
        }

        public string GetMeshLinkState(int x, int y, int height)
        {
                string ret = "";
                if (m_MeshArray != null)
                {
                        if (y < SceneSize - 1)
                        {
                                if (!GameUtility.IsFlagOn(m_MeshArray[x, y + 1], height))
                                {
                                        ret += "n";
                                }
                        }
                        if (x > 0)
                        {
                                if (!GameUtility.IsFlagOn(m_MeshArray[x - 1, y], height))
                                {
                                        ret += "w";
                                }
                        }
                        if (y > 0)
                        {
                                if (!GameUtility.IsFlagOn(m_MeshArray[x, y - 1], height))
                                {
                                        ret += "s";
                                }
                        }
                        if (x < SceneSize - 1)
                        {
                                if (!GameUtility.IsFlagOn(m_MeshArray[x + 1, y], height))
                                {
                                        ret += "e";
                                }
                        }
                        if (!GameUtility.IsFlagOn(m_MeshArray[x, y], height + 1))
                        {
                                ret += "_t";
                        }
                }
                return ret;
        }

        public void InitSceneMesh()
        {
                if (m_BrickHeightData != null)
                {
                        for (int z = 0; z < SceneSize; z++)
                        {
                                for (int x = 0; x < SceneSize; x++)
                                {
                                        if (!IsMeshExist(x, z))
                                        {
                                                int height = GetHeightByPos(x, z);
                                                for (int y = 0; y < height; y++)
                                                {
                                                        m_MeshArray[x, z] |= 1 << y;
                                                }
                                        }
                                        else
                                        {
                                                int id = z * SceneSize + x;
                                                m_BrickHeightData[id] = 0;
                                        }
                                }
                        }
                }
        }
        public void SetupAllBricks()
        {
                InitSceneMesh();
                int height = 0;
                for (int z = 0; z < SceneSize; z++)
                {
                        for (int x = 0; x < SceneSize; x++)
                        {
                                height = GetHeightByPos(x, z);
                                for (int y = 0; y < height; y++)
                                {
                                        SetSceneBrick(x, y, z);
                                }
                        }
                }
        }
        public int GetHeightByPos(int x, int z)  //获得指定点的海拔高度
        {
                if (m_BrickHeightData != null)
                {
                        int id = z * SceneSize + x;
                        if (id >= 0 && id < m_BrickHeightData.Length)
                        {
                                return m_BrickHeightData[id];
                        }
                }
                return 0;
        }
        public ulong GetBrickLink(int x, int y, int z)
        {
                ulong bit = 0;
                int idx = -1;
                for (int tz = z - 1; tz <= z + 1; tz++)
                {
                        for (int tx = x - 1; tx <= x + 1; tx++)
                        {
                                idx++;
                                if (idx == 4 || tx < 0 || tz < 0 || tx >= SceneSize || tz >= SceneSize)
                                {
                                        continue;
                                }

                                //int height = GetHeightByPos(tx, tz);

                                if (IsMeshExist(tx, tz, y))
                                {
                                        bit |= ((ulong)1 << idx);
                                }
                                else if (IsMeshExist(tx, tz, y - 1))
                                {
                                        bit |= ((ulong)1 << (idx + 9));
                                }
                        }
                }

                if (IsMeshExist(x, z, y + 1))   //上面有
                {
                        bit |= (1 << 4);
                }
                return bit;
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
                if (!IsMeshExist(x, z, y))
                {
                        return;
                }
                        if (IsMeshExist(x, z, y + 1))
                {
                        return;
                }
//                 //如果存在比自己高的砖，则不算阴影（实际上不会）
//                 if (m_MeshDict.ContainsKey(GetMeshID(x, y + 1, z)))
//                 {
//                         return;
//                 }
                int idx = 0;
                for (int tz = z - 1; tz <= z + 1; tz++)
                {
                        for (int tx = x - 1; tx <= x + 1; tx++)
                        {
                                if (idx == 4 || tx < 0 || tz < 0 || tx >= SceneSize || tz >= SceneSize)
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
        void AddShadowMesh(int belongMeshId, int x, int y, int z, CUBE_SHADOW_STATE css, int shadowedMeshID)
        {
                int mfId = belongMeshId + (int)css + 1;
                if (!m_ShadowMeshList.ContainsKey(mfId))
                {
                        MeshFilter shadowMF = ModelResourceManager.GetInst().GetCommonShadowMF(css);
                        if (shadowMF != null)
                        {
                                shadowMF.transform.localPosition = m_vOffset + new Vector3(x, y, z);
                                m_CC.AddMesh(shadowMF.mesh, shadowMF.transform.localToWorldMatrix, shadowMF.transform.GetComponent<Renderer>().sharedMaterial, 0, mfId, false, false);
                                //记录该投影mesh对应的投影砖和被投影砖关系
                                m_ShadowMeshList.Add(mfId, new KeyValuePair<int, int>(belongMeshId, shadowedMeshID));
                        }
                }
        }

        public void AddMod(int modelId, int meshId, Vector3 pos, ulong linkState = 0, bool bAddCollider = false)
        {
                if (m_MeshDict.ContainsKey(meshId))
                {
                        RemoveMod(meshId, false);
                }
                string branch = GameUtility.ConvertLinkBit(linkState);
                
                {
                        MeshFilter meshFilter = ModelResourceManager.GetInst().GetCommonMeshFilter(branch);

                        if (meshFilter != null)
                        {
                                ModelConfig modelCfg = ModelResourceManager.GetInst().GetModelCfg(modelId);

                                if (modelCfg != null && modelCfg.model_resource.Contains("model_common_set"))
                                {
                                        Material shareMat = ModelResourceManager.GetInst().GetCommonShareMat(modelCfg.material_resource);
                                        if (shareMat != null)
                                        {
                                                Mesh mesh = meshFilter.sharedMesh;
                                                if (modelCfg.material_index > 0)
                                                {
                                                        mesh = Mesh.Instantiate(meshFilter.mesh) as Mesh;
                                                        GameUtility.SetMeshUVIndex(mesh, modelCfg.material_index - 1, 8);
                                                }
                                                meshFilter.transform.localPosition = m_vOffset + pos;
                                                if (bAddCollider)
                                                {
                                                        Material newMat = new Material(shareMat);
                                                        newMat.shader = Shader.Find("DF/FadeoutDissolve");
                                                        newMat.name += "_Dissolve";
                                                        newMat.SetTexture("_DissolveTex", Resources.Load("DissolveTex") as Texture);
                                                        newMat.SetTexture("_MaskTex", Resources.Load("DissolveMask") as Texture);
                                                        m_CC.AddMesh(mesh, meshFilter.transform.localToWorldMatrix, newMat, 2, meshId, true, true, null, 0, true);
                                                }
                                                else
                                                {
                                                        m_CC.AddMesh(mesh, meshFilter.transform.localToWorldMatrix, shareMat, 1, meshId, true, true, null, 0, false);
                                                }
                                                m_MeshCfgDict.Add(meshId, modelCfg);
                                                m_MeshDict.Add(meshId, mesh);
                                        }
                                        if (pos.y >= 1 && pos.y <= 4)
                                        {
                                                CheckAffectShadow(meshId, (int)pos.x, (int)pos.y, (int)pos.z, linkState);
                                        }

                                        CheckBeAffectedShadow(meshId, (int)pos.x, (int)pos.y, (int)pos.z);
                                }
                        }
                }
        }

        public void RemoveMod(int meshId, bool bRemoveShadow = true)
        {
                if (m_MeshDict.ContainsKey(meshId))
                {
                        m_CC.RemoveMesh(meshId);
                        m_MeshDict.Remove(meshId);
                        m_MeshCfgDict.Remove(meshId);
                        if (bRemoveShadow)
                        {
                                RemoveShadowMesh(meshId);
                        }
                }
        }

        void RemoveShadowMesh(int meshId)
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

        /// <summary>
        /// x y z是场景坐标系，比迷宫坐标系偏移左下。        
        /// </summary>
        /// <param name="x"></param>
        /// <param name="height"></param>
        /// <param name="z"></param>
        /// <param name="cfg"></param>
        void SetSceneBrick(int x, int height, int z)
        {
                if (height >= 0 && height < m_ModelList.Count)
                {
                        int index = z * SceneSize + x + 1;
                        int kind = UnityEngine.Random.Range(0, m_ModelList[height].Count);
                        if (kind >= m_ModelList[height].Count)
                        {
                                Debuger.LogError(kind + " " + m_ModelList[height].Count  +" " + height);
                        }
                        int model_id = m_ModelList[height][kind];
                        if (model_id != 0)
                        {
                                int meshId = GetMeshID(x, height, z);
                                AddMod(model_id, meshId, new Vector3(x,height, z), GetBrickLink(x, height, z));
                        }
                }
        }
        public void SetNodeBrick(int x, int height, int z, int model_id, bool bAddCollider = false)
        {
                x -= OffsetX;
                z -= OffsetY;
                int meshId = GetMeshID(x, height, z);
                AddMod(model_id, meshId, new Vector3(x, height, z), GetBrickLink(x, height, z), bAddCollider);
        }
        Dictionary<int, Mesh> m_TransitionMeshDict = new Dictionary<int, Mesh>();
        void CheckSingleTransition(int meshId, int offset, string direct, int priority)
        {
                if (m_MeshCfgDict.ContainsKey(meshId + offset) && m_MeshCfgDict[meshId + offset].GetPriority() > priority)
                {
                        AddTransitionMesh(meshId, GetMeshX(meshId), GetMeshY(meshId), GetMeshZ(meshId), direct, m_MeshCfgDict[meshId + offset]);
                }
        }
        public void CheckAllTransitions()
        {
                foreach (var param in m_MeshCfgDict)
                {
                        int priority = m_MeshCfgDict[param.Key].GetPriority();
                        if (priority == 0)
                                continue;

                        CheckSingleTransition(param.Key, 10, "n", priority);
                        CheckSingleTransition(param.Key, -10000, "w", priority);
                        CheckSingleTransition(param.Key, -10, "s", priority);
                        CheckSingleTransition(param.Key, 10000, "e", priority);
                }
        }

        int ConvertDirect(string direct)
        {
                switch (direct)
                {
                        case "n":
                                return 0;
                        case "w":
                                return 1;
                        case "s":
                                return 2;
                        case "e":
                                return 3;
                }
                return -1;
        }

        
        void AddTransitionMesh(int belongMeshId, int x, int y, int z, string direct, ModelConfig modelCfg)
        {
                int mfId = belongMeshId + ConvertDirect(direct);
                if (!m_TransitionMeshDict.ContainsKey(mfId))
                {
                        MeshFilter transitionMF = ModelResourceManager.GetInst().GenerateCommonPlane(direct);
                        if (transitionMF != null)
                        {
                                string matname = modelCfg.material_resource + "_transition" /*+ direct*/;
                                Material shareMat = m_CC.GetShareMat(matname);
                                if (shareMat == null)
                                {
                                        Material meshMat = ModelResourceManager.GetInst().GetCommonShareMat(modelCfg.material_resource);

                                        shareMat = new Material(transitionMF.GetComponent<Renderer>().sharedMaterial);
                                        shareMat.name = matname;
                                        shareMat.shader = Shader.Find("DF/Transparent_Cutout/Bumped Specular");
                                        shareMat.SetTexture("_MainTex", meshMat.GetTexture("_MainTex"));
                                        shareMat.SetTexture("_BumpMap", meshMat.GetTexture("_BumpMap"));
                                        shareMat.SetFloat("_Shininess", meshMat.GetFloat("_Shininess"));
                                        shareMat.SetColor("_Color", meshMat.GetColor("_Color"));
                                }
                                Mesh mesh = transitionMF.mesh;
                                if (modelCfg.material_index > 0)
                                {
                                        mesh = Mesh.Instantiate(transitionMF.mesh) as Mesh;
                                        GameUtility.SetMeshUVIndex(mesh, modelCfg.material_index - 1, 8);
                                }

                                transitionMF.transform.localPosition = m_vOffset + new Vector3(x, y, z);
                                m_CC.AddMesh(mesh, transitionMF.transform.localToWorldMatrix, shareMat, 0, mfId, false, true);
                                m_TransitionMeshDict.Add(mfId, mesh);
                        }
                }
        }
        public void CombineAll()
        {
                m_CC.DoCombine();
        }
}
