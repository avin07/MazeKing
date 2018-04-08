using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[ExecuteInEditMode]
public class MixMaterialBehav : MonoBehaviour 
{
        static Dictionary<int, Material> g_matdict = new Dictionary<int, Material>();
        public int index = 0;
        Mesh mesh;

        public int index_N;
        public int index_W;
        public int index_S;
        public int index_E;

        public int MatIndex
        {
                get
                {
                        return index * 10000 + index_N * 1000 + index_W * 100 + index_S * 10 + index_E;
                }
        }

        Renderer CheckHit(Vector3 direction)
        {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, direction, out hit, 1f, 1<<LayerMask.NameToLayer("InBuildObj")))
                {
                        return hit.collider.gameObject.GetComponentInChildren<Renderer>();
                }
                return null;
        }

        string GetPropName(Vector3 dir)
        {
                if (dir == Vector3.forward)
                {
                        return "n";
                        //return "Models/model_common_plane_n";
                }
                if (dir == Vector3.left)
                {
                        return "w";
                        //return "Models/model_common_plane_w";
                }
                if (dir == Vector3.back)
                {
                        return "s";
                        //return "Models/model_common_plane_s";
                }
                if (dir == Vector3.right)
                {
                        return "e";
                        //return "Models/model_common_plane_e";
                }
                return "";
        }

        void Start () 
        {
                //this.gameObject.name = "NeedCombine";
                Dictionary<Vector3, Material> dirDict = new Dictionary<Vector3, Material>();
                {
                        Vector3[] dirs = new Vector3[4] { Vector3.forward, Vector3.left, Vector3.back, Vector3.right };

                        foreach (Vector3 dir in dirs)
                        {
                                Renderer tmpR = CheckHit(dir);
                                if (tmpR != null && tmpR.sharedMaterial != null && tmpR.sharedMaterial.mainTexture != null)
                                {
                                        if (tmpR.sharedMaterial.mainTexture.name.Contains("wall"))
                                        {
                                                dirDict.Add(dir, tmpR.sharedMaterial);
                                                tmpR.gameObject.name = "NeedCombine_" + tmpR.sharedMaterial.mainTexture.name;
                                        }
                                }
                        }
                        int idx = 1;
                        foreach (Vector3 dir in dirDict.Keys)
                        {
                                //Object ab = ResourceManager.GetInst().Load(GetPropName(dir), AssetResidentType.Temporary);
                                //GameObject obj = GameObject.Instantiate(ab) as GameObject;
//                                 GameObject obj = ModelResourceManager.GetInst().GenerateCommonPlane(GetPropName(dir));
//                                 //obj.name = "MixPlane_dir";
//                                 obj.transform.SetParent(this.transform);
//                                 obj.transform.position= transform.position + Vector3.up * 0.0003f * idx;
//                                 obj.GetComponentInChildren<Renderer>().sharedMaterial = dirDict[dir];
                                idx++;
                        }
                        //GameObject.Destroy(this);
                }
        }
}
