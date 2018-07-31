using UnityEngine;
using System.Collections;

public class SimpleButtonAnim : MonoBehaviour 
{
        public float OffsetY;
        public float Speed;
        float StartY;
        float EndY;

        bool m_bUp = true;
	// Use this for initialization
	void Start () 
        {
                StartY = this.transform.localPosition.y;
	}
	
	// Update is called once per frame
	void Update () 
        {
                if (m_bUp)
                {
                        if (transform.localPosition.y < StartY + OffsetY)
                        {
                                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + Time.deltaTime * Speed, transform.localPosition.z);
                        }
                        else
                        {
                                m_bUp = !m_bUp;
                        }
                }
                else
                {
                        if (transform.localPosition.y > StartY)
                        {
                                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y - Time.deltaTime * Speed, transform.localPosition.z);
                        }
                        else
                        {
                                m_bUp = !m_bUp;
                        }
                }
	}
}
