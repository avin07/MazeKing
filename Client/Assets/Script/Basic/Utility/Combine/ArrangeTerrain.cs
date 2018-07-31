using UnityEngine;
using System.Collections.Generic;

//[ExecuteInEditMode]
public class ArrangeTerrain : MonoBehaviour
{
        public bool m_bInit = false;
        public int size = 5;
        Dictionary<Texture, Renderer> m_MatDict = new Dictionary<Texture, Renderer>();
        // Use this for initialization
        void Combine()
        {
                if (m_bInit)
                {
                        return;
                }

                if (m_bInit == false)
                {
                        m_bInit = true;
                }

                int minx = int.MaxValue;
                int minz = int.MaxValue;
                int maxx = int.MinValue;
                int maxz = int.MinValue;
                foreach (Renderer renderer in this.gameObject.GetComponentsInChildren<Renderer>(true))
                {
                        if (renderer.transform.position.x < minx)
                        {
                                minx = (int)renderer.transform.position.x;
                        }
                        if (renderer.transform.position.z < minz)
                        {
                                minz = (int)renderer.transform.position.z;
                        }
                        if (renderer.transform.position.x > maxx)
                        {
                                maxx = (int)renderer.transform.position.x;
                        }
                        if (renderer.transform.position.z > maxz)
                        {
                                maxz = (int)renderer.transform.position.z;
                        }
                }
                GameObject[,] roots = new GameObject[size, size];
                for (int x = 0; x < size; x++)
                {
                        for (int z = 0; z < size; z++)
                        {
                                GameObject obj= new GameObject();
                                obj.name = "Root_" + x + "_"+ z;
                                obj.transform.SetParent(this.transform);
                                roots[x, z] = obj;
                                obj.AddComponent<CombineChildren>();
                        }
                }

                int deltax = (maxx - minx) / size;
                int deltaz = (maxz - minz) / size;
                Debuger.Log(minx + " " + deltax + " " + maxx + " " + minz + " " + deltaz + " " + maxz);
                foreach (Transform t in gameObject.GetComponentsInChildren<Transform>(true))
                {
                        if (t.name.Contains("Root_"))
                                continue;

                        int x = 0;
                        int z = 0;
                        for (int i = 0; i < size; i++)
                        {
                                if (t.transform.position.x - minx >= deltax * i)
                                {
                                        x = i;
                                }
                                if (t.transform.position.z - minz >= deltaz * i)
                                {
                                        z = i;
                                }
                        }

                        t.transform.SetParent(roots[x, z].transform);
                }
        }

        // Update is called once per frame
        void Update()
        {
                Combine();
        }
}
