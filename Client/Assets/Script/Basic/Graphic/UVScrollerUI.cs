using UnityEngine;
using UnityEngine.UI;

class UVScrollerUI : MonoBehaviour
{
    public float scrollSpeed = 0.1f;
    void Update()
    {
        if (GetComponent<Image>().material.shader.isSupported)
        {
            if (Camera.main != null)
            {
                Camera.main.depthTextureMode |= DepthTextureMode.Depth;
            }
        }

        float offset = Time.time * scrollSpeed;
        if (GetComponent<Image>().material.HasProperty("_BumpMap"))
        {
            GetComponent<Image>().material.SetTextureOffset("_BumpMap", new Vector2(offset / -7f, offset));
        }
        GetComponent<Image>().material.SetTextureOffset("_MainTex", new Vector2(offset / 10f, offset));
    }
}