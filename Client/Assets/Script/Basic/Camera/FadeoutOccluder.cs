using UnityEngine;
using System.Collections;

public class FadeoutOccluder : MonoBehaviour
{
        public LayerMask layerMask = 15;
        public Transform target;

        Transform mCameraTransform;

        public float fadeSpeed = 1.0f;
        public float radius = 0.3f;
        public float fadeOutAlpha = 0f;

        Shader mTransparentShader;
        //Shader mTransparentTreeShader;

        ArrayList fadedOutObjects = new ArrayList();

        class FadeoutLOSInfo
        {
                public Renderer renderer;
                public Material[] originalMaterials;
                public Material[] alphaMaterials;
                public bool needFadeOut = true;
                public FadeoutLOSInfo()
                {
                        //originalMaterials = new Material[];
                        //alphaMaterials = new Material[];
                }
        }

        private bool m_bEnable = true;
        public void EnableFadeoutOccluder(bool bl)
        {
                m_bEnable = bl;
        }

        FadeoutLOSInfo FindLosInfo(Renderer r)
        {
                foreach (FadeoutLOSInfo tmp in fadedOutObjects)
                {
                        if (r == tmp.renderer)
                                return tmp;
                }
                return null;
        }

        IEnumerator Start()
        {
                m_bEnable = false;
                if (target == null)
                {
                        target = RaidManager.GetInst().MainHero.transform;
                }
                if (target != null)
                {
                        mCameraTransform = this.transform;
                        mTransparentShader = Shader.Find("DF/FadeoutDissolve");
                        yield return new WaitForSeconds(2.0f);
                        m_bEnable = true;
                }
        }

        float m_fDeltaTime = 0.0f;

        void LateUpdate()
        {
                if (target == null)
                        return;

                if (m_bEnable == false && fadedOutObjects.Count == 0)
                        return;
                m_fDeltaTime += Time.deltaTime;
                if (m_fDeltaTime >= 0.5f)
                {
                        if (m_bEnable)
                        {
                                m_fDeltaTime = 0.0f;
                                Vector3 from = mCameraTransform.position;
                                Vector3 to = target.position + Vector3.up;
                                float castDistance = Vector3.Distance(to, from);

                                foreach (FadeoutLOSInfo tmp in fadedOutObjects)
                                {
                                        tmp.needFadeOut = false;
                                }

                                Vector3[] offsets = new Vector3[] 
                                {
                                        Vector3.zero,
                                        new Vector3(0.0f, radius, 0.0f),
                                        new Vector3(0.0f, -radius, 0.0f),
                                        new Vector3(radius, 0.0f, 0.0f),
                                        new Vector3(-radius, 0.0f, 0.0f)
                                };

                                foreach (Vector3 offset in offsets)
                                {
                                        Vector3 relativeOffset = mCameraTransform.TransformDirection(offset);
                                        RaycastHit[] hits = Physics.RaycastAll(from + relativeOffset, to - from, castDistance, layerMask.value);

                                        foreach (RaycastHit temp in hits)
                                        {
                                                Renderer hitRenderer = temp.collider.GetComponent<Renderer>();
                                                if (hitRenderer == null || !hitRenderer.enabled)
                                                {
                                                        Debuger.Log(temp.transform.gameObject.name + "hit################");
                                                        continue;
                                                }

                                                FadeoutLOSInfo info = FindLosInfo(hitRenderer);

                                                if (info == null)
                                                {
                                                        info = new FadeoutLOSInfo();
                                                        info.originalMaterials = hitRenderer.sharedMaterials;
                                                        info.alphaMaterials = new Material[info.originalMaterials.Length];
                                                        info.renderer = hitRenderer;

                                                        for (int i = 0; i < info.originalMaterials.Length; i++)
                                                        {
                                                                if (info.originalMaterials[i] == null)
                                                                        continue;

                                                                Material newMaterial = new Material(mTransparentShader);
                                                                newMaterial.mainTexture = info.originalMaterials[i].mainTexture;
                                                                newMaterial.color = info.originalMaterials[i].color;
                                                                newMaterial.color = new Color(newMaterial.color.r, newMaterial.color.g, newMaterial.color.b, 1.0f);
                                                                newMaterial.SetTexture("_DissolveTex", Resources.Load("DissolveTex") as Texture);
                                                                newMaterial.SetTexture("_MaskTex", Resources.Load("DissolveMask") as Texture);
                                                                info.alphaMaterials[i] = newMaterial;
                                                        }
                                                        hitRenderer.sharedMaterials = info.alphaMaterials;
                                                        fadedOutObjects.Add(info);
                                                }
                                                else
                                                {
                                                        info.needFadeOut = true;
                                                }
                                        }
                                }
                        }
                }
                //Fade();
        }

        void Fade()
        {
                float fadeDelta = fadeSpeed * Time.deltaTime;//fadeSpeed*Time.deltaTime;
                float alpha = 1.0f;
                for (int i = 0; i < fadedOutObjects.Count; i++)
                {
                        FadeoutLOSInfo fade = (FadeoutLOSInfo)fadedOutObjects[i];
                        if (fade.needFadeOut && m_bEnable)
                        {
                                foreach (Material tmmp in fade.alphaMaterials)
                                {
                                        if (fade == null || fade.alphaMaterials == null || fade.alphaMaterials.Length <= 0 || tmmp == null)
                                        {
                                                continue;
                                        }

                                        alpha = tmmp.color.a;
                                        alpha -= fadeDelta;
                                        alpha = Mathf.Max(alpha, fadeOutAlpha);
                                        tmmp.color = new Color(tmmp.color.r, tmmp.color.g, tmmp.color.b, alpha);
                                }
                        }
                        else
                        {
                                int totallyFadeIn = 0;
                                foreach (Material tmpM in fade.alphaMaterials)
                                {
                                        if (fade == null || fade.alphaMaterials == null || fade.alphaMaterials.Length <= 0 || tmpM == null)
                                        {

                                        }
                                        else
                                        {
                                                alpha = tmpM.color.a;
                                                alpha += fadeDelta;
                                                alpha = Mathf.Min(alpha, 1.0f);
                                                //tmpM.color.a = alpha;
                                                tmpM.color = new Color(tmpM.color.r, tmpM.color.g, tmpM.color.b, alpha);
                                        }
                                        if (alpha >= 0.99f)
                                                totallyFadeIn++;
                                }

                                if (totallyFadeIn == fade.alphaMaterials.Length)
                                {
                                        if (fade.renderer)
                                                fade.renderer.sharedMaterials = fade.originalMaterials;

                                        foreach (Material newMaterial in fade.alphaMaterials)
                                                Destroy(newMaterial);

                                        fadedOutObjects.RemoveAt(i);
                                        i--;
                                }
                        }
                }
        }
}
