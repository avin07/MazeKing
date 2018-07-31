using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//[ExecuteInEditMode]
public class CalcBlend : MonoBehaviour
{
        Dictionary<string, Material> m_ShareDict = new Dictionary<string, Material>();
        void Awake()
        {
                MeshRenderer[] renderers = gameObject.GetComponentsInChildren<MeshRenderer>(true);
                foreach (MeshRenderer renderer in renderers)
                {
                        BoxCollider box = renderer.gameObject.AddComponent<BoxCollider>();
                        box.size = Vector3.one;
                        renderer.gameObject.layer = LayerMask.NameToLayer("InBuildObj");

                        if (renderer.sharedMaterial != null && renderer.sharedMaterial.name.Contains("roadbed"))
                        {
                                renderer.gameObject.AddComponent<MixMaterialBehav>();
                        }
                }
        }
}
