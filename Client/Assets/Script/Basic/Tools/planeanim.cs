using UnityEngine;
using System.Collections;

public class planeanim : MonoBehaviour 
{
        public float x;
        public bool bX;
        public bool bY;
        public bool bZ;
        public float maxX;
        public float totalX;
        bool bDir = true;

        public Vector3 pos;
        public Vector3 posoffset;
	// Use this for initialization
	void Start () 
        {
                bDir = true;
                pos = this.transform.position ;
                
                totalX = 0f;
	}
	
	// Update is called once per frame
	void Update () 
        {
                if (bX)
                {
                        transform.RotateAround(pos + posoffset, Vector3.right, bDir ? x : (-x));
                }
                if (bY)
                {
                        transform.RotateAround(pos + posoffset, Vector3.up, bDir ? x : (-x));
                }
                if (bZ)
                {
                        transform.RotateAround(pos + posoffset, Vector3.forward, bDir ? x : (-x));
                }
                totalX += bDir ? x : (-x);
                if (bDir  && totalX > maxX)
                {
                        bDir = !bDir;
                }
                else if (!bDir && totalX < -maxX)
                {
                        bDir = !bDir;
                }
	}
}
