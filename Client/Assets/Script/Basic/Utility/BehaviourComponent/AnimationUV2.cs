using UnityEngine;
using System.Collections;

public class AnimationUV2 : MonoBehaviour
{
        public float speed = 0.5f;
        void Start()
        {

        }

        void Update()
        {
                if (GetComponent<Renderer>() != null)
                {
                        Vector2 offset = GetComponent<Renderer>().material.mainTextureOffset;
                        offset.x += speed * Time.deltaTime;
                        if (offset.x > 1)
                        {
                                offset.x -= 1;
                        }
                        if (offset.x < 0)
                        {
                                offset.x += 1;
                        }
                        GetComponent<Renderer>().material.SetTextureOffset("_MainTex", offset);
                }
                //                 renderer.material.mainTextureOffset.x += speed * Time.deltaTime;
                //                 if (renderer.material.mainTextureOffset.x > 1)
                //                 {
                //                         renderer.material.mainTextureOffset.x = 0;
                //                 }
                //                 if (renderer.material.mainTextureOffset.x < 0)
                //                 {
                //                         renderer.material.mainTextureOffset.x = 1;
                //                 }
        }
}