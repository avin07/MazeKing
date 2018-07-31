using UnityEngine;
using System.Collections;

class UIBillboard : MonoBehaviour
{

    Quaternion qua;
    void Update()
    {
        if (Camera.main != null)
        {
            qua.eulerAngles = new Vector3(Camera.main.transform.rotation.eulerAngles.x, Camera.main.transform.rotation.eulerAngles.y, 0f);
            transform.rotation = qua;
        }
    }
}
