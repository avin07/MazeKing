using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class KeepRotZero : MonoBehaviour
{
        public float OffsetX = 0f;
        public float OffsetY = 0f;
        public float TileX = 1f;
        public float TileY = 1f;
        float m_fDeltaTime = 0f;

        void LateUpdate()
        {
                this.transform.localPosition = Vector3.zero;
                this.transform.rotation = Quaternion.identity;
                foreach (DissolveInfo info in RaidManager.GetInst().GetDissolveList())
                {
                        if (info.bExit)
                        {
                                info.dissolve += Time.deltaTime;
                                if (info.dissolve >= 1f)
                                {
                                        info.renderer.sharedMaterial.SetFloat("_IsDissolve", 1);
                                        RaidManager.GetInst().RemoveDissolve(GetComponent<Renderer>());
                                        continue;
                                }
                        }
                        else
                        {
                                info.dissolve -= Time.deltaTime;
                        }
                        info.renderer.sharedMaterial.SetFloat("_PlayerPos", this.transform.position.z);
                        info.renderer.sharedMaterial.SetVector("_OffsetVec", new Vector4(OffsetX, OffsetY, TileX, TileY));
                        info.renderer.sharedMaterial.SetFloat("_IsDissolve", Mathf.Clamp(info.dissolve, 0f, 1f));
                }
        }
        void OnTriggerEnter(Collider other)
        {
                if (other.gameObject.GetComponent<MeshRenderer>() != null)
                {
                        Renderer renderer = other.gameObject.GetComponent<MeshRenderer>();
                        
                        if (renderer.sharedMaterial.shader.name.Contains("Dissolve"))
                        {
                                renderer.sharedMaterial.SetFloat("_PlayerPos", this.transform.position.z);
                                RaidManager.GetInst().AddRenderDissolve(renderer);

                        }
                }
        }
        void OnTriggerExit(Collider other)
        {
                if (other.gameObject.GetComponent<MeshRenderer>() != null)
                {
                        Renderer renderer = other.gameObject.GetComponent<MeshRenderer>();

                        if (renderer.sharedMaterial.shader.name.Contains("Dissolve"))
                        {
                                RaidManager.GetInst().SetRenderDissolve(renderer);
                                //Debug.Log("OnTriggerExit " + other.name);
                        }
                }
        }
}

public class DissolveInfo
{
        public Renderer renderer;
        public float dissolve = 1f;
        public bool bExit = false;
        public DissolveInfo(Renderer r)
        {
                renderer = r;
                dissolve = 1f;
                bExit = false;
        }
}

