 using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour 
{
        Camera cam;
        public Vector2 range = new Vector2(0.3f, 0.3f);
        Vector3 m_OriPos;
        float m_fTime;
        float m_fMaxTime;

        public float MaxTime
        {
                get { return m_fMaxTime; }
                set
                {
                        m_fMaxTime = value;
                }
        }

	// Use this for initialization
	void Start () 
        {
                cam = this.GetComponent<Camera>();
                m_OriPos = cam.transform.position;
                m_fTime = Time.realtimeSinceStartup;
                Component.Destroy(this, m_fMaxTime);
	}
	
	// Update is called once per frame
        void Update()
        {
                if (cam != null)
                {
                        Vector3 newPos = m_OriPos;
                        newPos.x += UnityEngine.Random.Range(-range.x, range.x);
                        newPos.y += UnityEngine.Random.Range(-range.y, range.y);
                        cam.transform.position = newPos;
                }
        }
}
