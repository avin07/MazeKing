using UnityEngine;
using System.Collections.Generic;

public class CombineMaterial : MonoBehaviour
{
        public bool m_bInit = false;

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
                Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
                Renderer renderer;
                for (int i = 0; i < renderers.Length; i++)
                {
                        renderer = renderers[i];
                        if (renderer.sharedMaterial != null && renderer.sharedMaterial.mainTexture == null)
                                continue;

                        if (!m_MatDict.ContainsKey(renderer.sharedMaterial.mainTexture))
                        {
                                m_MatDict.Add(renderer.sharedMaterial.mainTexture, renderer);
                                GameObject tmpRoot = new GameObject();
                                tmpRoot.name = "root" + renderer.sharedMaterial.mainTexture.name;
                                tmpRoot.transform.SetParent(renderer.transform.parent);
                                if (tmpRoot.GetComponent<CombineChildren>() == null)
                                {
                                        tmpRoot.AddComponent<CombineChildren>();
                                }
                                renderer.transform.SetParent(tmpRoot.transform);
                                //Debuger.Log("Add Texture " + renderer.sharedMaterial.mainTexture);
                        }
                        else
                        {
                                if (renderer.sharedMaterial != m_MatDict[renderer.sharedMaterial.mainTexture].sharedMaterial)
                                {
                                        renderer.sharedMaterial = m_MatDict[renderer.sharedMaterial.mainTexture].sharedMaterial;
                                }
                                renderer.transform.SetParent(m_MatDict[renderer.sharedMaterial.mainTexture].transform.parent);
                        }
                }
        }

        // Update is called once per frame
        void Update()
        {
                Combine();
        }
}
