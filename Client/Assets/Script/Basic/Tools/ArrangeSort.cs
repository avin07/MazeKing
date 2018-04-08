using UnityEngine;
using System.Collections;

public class ArrangeSort : MonoBehaviour
{
        // Use this for initialization
        void Start()
        {
                SortObjs();
        }
        void SortObjs()
        {
                float maxZ = 0f;
                float minZ = 0f;
                foreach (Transform trans in this.gameObject.GetComponentsInChildren<Transform>(true))
                {
                        maxZ = Mathf.Max(trans.position.z, maxZ);
                        minZ = Mathf.Min(trans.position.z, minZ);
                }
                //maxZ -= minZ;
                Debuger.Log(maxZ + " " + minZ);
                foreach (Transform trans in this.gameObject.GetComponentsInChildren<Transform>(true))
                {
                        Renderer tmpR = trans.gameObject.GetComponent<Renderer>();
                        if (tmpR != null)
                        {

                                tmpR.material.renderQueue = tmpR.material.shader.renderQueue + (int)(1000f - (trans.position.z - minZ) / (maxZ - minZ) * 1000f);

                        }
                }
        }
}
