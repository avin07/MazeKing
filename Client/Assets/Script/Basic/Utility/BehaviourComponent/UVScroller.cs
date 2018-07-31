using UnityEngine;

class UVScroller : MonoBehaviour
{
    public float scrollSpeed = 0.1f;

    void Awake()
    {
        if (GetComponent<Renderer>().sharedMaterial.shader.isSupported)
        {
            if (Camera.main != null && Camera.main.isActiveAndEnabled)
            {
                Camera.main.depthTextureMode |= DepthTextureMode.Depth;
            }

        }
    }

    void Update()
    {

        float offset = Time.time * scrollSpeed;
        if (GetComponent<Renderer>() == null)
        {
            return;
        }
        if (GetComponent<Renderer>().sharedMaterial == null)
        {
            return;
        }
        if (GetComponent<Renderer>().sharedMaterial.HasProperty("_BumpMap"))
        {
            GetComponent<Renderer>().sharedMaterial.SetTextureOffset("_BumpMap", new Vector2(offset / -7f, offset));
        }
        GetComponent<Renderer>().sharedMaterial.SetTextureOffset("_MainTex", new Vector2(offset / 10f, offset));
    }
}

