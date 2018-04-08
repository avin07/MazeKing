using UnityEngine;
using System.Collections;

public class FadeoutDissolve : MonoBehaviour
{
        public LayerMask layerMask = 15;
        public Transform target;
        
        public float fadeSpeed = 1.0f;
        public float radius = 0.5f;
        public float mRadius
        {
                get
                {
                        return radius;
                }
                set
                {
                        radius = value;
                        ResetOffsets();
                }
        }
        Vector3[] offsets;

        void ResetOffsets()
        {
                offsets = new Vector3[]
                {
                                        Vector3.zero,
                                        new Vector3(0.0f, radius, 0.0f),
                                        new Vector3(0.0f, -radius, 0.0f),
                                        new Vector3(radius, 0.0f, 0.0f),
                                        new Vector3(-radius, 0.0f, 0.0f)
                };
        }

        public float fadeOutAlpha = 0f;

        Shader _DissolveShader;
        Shader DissolveShader
        {
                get
                {
                        if (_DissolveShader == null)
                        {
                                _DissolveShader = Shader.Find("DF/FadeoutDissolve");
                        }
                        return _DissolveShader;
                }
        }

        Texture DissolveTexture
        {
                get
                {
                        return Resources.Load("DissolveTex") as Texture;
                }
        }
        Texture DissolveMask
        {
                get
                {
                        return Resources.Load("DissolveMask") as Texture;
                }
        }

        ArrayList m_DissolveObjects = new ArrayList();

        class DissolveObjInfo
        {
                public Renderer renderer;
                public Material[] originalMats;
                public Material[] dissolveMats;
                public bool IsExist = true;
                public DissolveObjInfo()
                {
                        //originalMaterials = new Material[];
                        //alphaMaterials = new Material[];
                }
        }

        DissolveObjInfo FindLosInfo(Renderer r)
        {
                foreach (DissolveObjInfo tmp in m_DissolveObjects)
                {
                        if (r == tmp.renderer)
                                return tmp;
                }
                return null;
        }

        IEnumerator Start()
        {
                ResetOffsets();
                if (target == null)
                {
                        target = RaidManager.GetInst().MainHero.transform;
                }
                if (target != null)
                {
                        yield return new WaitForSeconds(2.0f);
                }
        }

        float m_fDeltaTime = 0.0f;

        string GetDissolveShaderName(string shaderName)
        {
                if (shaderName == "DF/Bumped Diffuse Gray")
                {
                        return "DF/FadeoutDissolve";
                }
                else if (shaderName == "DF/Bumped Specular Gray")
                {
                        return "DF/FadeoutDissolve";
                }
                return "";
        }
                void LateUpdate()
        {
                if (target == null)
                        return;

                m_fDeltaTime += Time.deltaTime;
                if (m_fDeltaTime >= 0.5f)
                {
                        m_fDeltaTime = 0f;
                        Vector3 from = this.transform.position;
                        Vector3 to = target.position + Vector3.up;
                        float castDistance = Vector3.Distance(to, from);

                        foreach (DissolveObjInfo tmp in m_DissolveObjects)
                        {
                                tmp.IsExist = false;
                        }

                        foreach (Vector3 offset in offsets)
                        {
                                Vector3 relativeOffset = this.transform.TransformDirection(offset);
                                RaycastHit[] hits = Physics.RaycastAll(from + relativeOffset, to - from, castDistance, layerMask.value);

                                foreach (RaycastHit temp in hits)
                                {
                                        Renderer hitRenderer = temp.collider.GetComponent<Renderer>();
                                        if (hitRenderer == null || !hitRenderer.enabled)
                                        {
                                                Debuger.Log(temp.transform.gameObject.name + "hit################");
                                                continue;
                                        }
                                        else
                                        {
                                                if (temp.transform.name.Contains("Combined_model_raid_tree_002"))
                                                        continue;
                                                Debuger.Log("hit " + temp.transform.gameObject.name);
                                        }

                                        DissolveObjInfo info = FindLosInfo(hitRenderer);

                                        if (info == null)
                                        {
                                                info = new DissolveObjInfo();
                                                info.originalMats = hitRenderer.sharedMaterials;
                                                info.dissolveMats = new Material[info.originalMats.Length];
                                                info.renderer = hitRenderer;

                                                for (int i = 0; i < info.originalMats.Length; i++)
                                                {
                                                        if (info.originalMats[i] == null)
                                                                continue;
                                                        string name = GetDissolveShaderName(info.originalMats[i].shader.name);
                                                        if (!string.IsNullOrEmpty(name))
                                                        {
                                                                Material newMaterial = new Material(info.originalMats[i]);
                                                                newMaterial.shader = DissolveShader;
                                                                newMaterial.SetTexture("_DissolveTex", DissolveTexture);
                                                                newMaterial.SetTexture("_MaskTex", DissolveMask);
                                                                newMaterial.SetFloat("_IsDissolve", 0);
                                                                info.dissolveMats[i] = newMaterial;
                                                        }
                                                        else
                                                        {
                                                                info.dissolveMats[i] = info.originalMats[i];
                                                        }
                                                }
                                                hitRenderer.sharedMaterials = info.dissolveMats;
                                                m_DissolveObjects.Add(info);
                                        }
                                        else
                                        {
                                                info.IsExist = true;
                                        }
                                }
                        }

                        ClearDissolveObjects();
                }
        }

        void ClearDissolveObjects()
        {
                for(int i = 0; i < m_DissolveObjects.Count; i++)
                {
                        DissolveObjInfo info = (DissolveObjInfo)m_DissolveObjects[i];
                        if (info.IsExist == false)
                        {
                                info.renderer.sharedMaterials = info.originalMats;

                                m_DissolveObjects.RemoveAt(i);
                                i--;
                        }
                }
        }
}
