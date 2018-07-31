using System;
using UnityEngine;

[Serializable]
public class TimedObjectDestructor : MonoBehaviour
{
    public bool detachChildren;
    public float timeOut = 1f;

    public void Start()
    {
        this.Invoke("DestroyNow", this.timeOut);
    }

    public void ReInvoke()
    {
        CancelInvoke();
        Invoke("DestroyNow", timeOut);
    }

    public void DestroyNow()
    {
        if (this.detachChildren)
        {
            this.transform.DetachChildren();
        }
        GameObject.Destroy(this.gameObject);
    }
}

