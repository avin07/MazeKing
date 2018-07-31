using UnityEngine;
using System.Collections;

public class ModelDisappear : MonoBehaviour
{
        float m_fStarttime = 0f;
        float m_fMaxtime = 1f;
        float m_fDelayTime = 0f;

        public float MaxTime
        {
                set { m_fMaxtime = value; }
        }

        public float DelayTime
        {
                set { m_fDelayTime = value; }
        }
        // Use this for initialization
        void Start()
        {
                m_fStarttime = Time.realtimeSinceStartup;
        }

        // Update is called once per frame
        void Update()
        {
                if (m_fDelayTime > 0)
                {
                        m_fDelayTime -= Time.realtimeSinceStartup - m_fStarttime;
                        m_fStarttime = Time.realtimeSinceStartup;
                }
                else
                {
                        if (Time.realtimeSinceStartup - m_fStarttime < m_fMaxtime)
                        {
                                float alpha = (Time.realtimeSinceStartup - m_fStarttime) / m_fMaxtime;

                                foreach (Renderer renderer in this.gameObject.GetComponentsInChildren<Renderer>(true))
                                {
                                        renderer.material.SetColor("_Color", new Color(1f, 1f, 1f, 1f - alpha));
                                }
                        }
                        else
                        {
                                Destroy(this.gameObject);
                        }
                }
        }
}
