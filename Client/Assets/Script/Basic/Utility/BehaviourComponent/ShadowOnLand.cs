using UnityEngine;
using System.Collections;

public class ShadowOnLand : MonoBehaviour
{        
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
                if (transform.parent != null)
                {
                        this.transform.position = this.transform.parent.position;
                }
        }
}
