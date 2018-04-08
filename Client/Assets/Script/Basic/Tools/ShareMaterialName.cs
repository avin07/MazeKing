using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ShareMaterialName : MonoBehaviour
{
        public List<string> matlist = new List<string>();
        public void SetMat()
        {
                Material[] mats = new Material[matlist.Count];
                for (int i = 0; i < matlist.Count; i++)
                {
                        mats[i] = ModelResourceManager.GetInst().GetCommonShareMat(matlist[i]);
                }

                MeshRenderer mr = transform.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                        mr.sharedMaterials = mats;
                }
                else
                {
                        SkinnedMeshRenderer smr = transform.GetComponent<SkinnedMeshRenderer>();
                        if (smr != null)
                        {
                                smr.sharedMaterials = mats;
                        }
                }
        }
}
