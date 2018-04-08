using UnityEngine;
using System.Collections;

public class ImageOffsetMove : MonoBehaviour
{
        Renderer m_MR;
        public Vector2 OffsetSpeed;
        public Vector2 Offset;
        // Use this for initialization
        void Start()
        {
                m_MR = GetComponent<Renderer>();
                Offset = Vector2.zero;
        }

        // Update is called once per frame
        void Update()
        {
                if (m_MR != null)
                {
                        Offset += OffsetSpeed;
                        m_MR.material.mainTextureOffset = Offset;
                }
        }
}
