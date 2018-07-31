using UnityEngine;
using System.Collections;
using System.Collections.Generic;
class EffectManager : SingletonObject<EffectManager>
{
    public GameObject GetEffectObj(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }
        Object obj = ResourceManager.GetInst().Load(EFFECT_PATH + path, AssetResidentType.Temporary);
        if (obj != null)
        {
            GameObject Obj = GameObject.Instantiate(obj) as GameObject;

            return Obj;
        }
        return null;
    }

    public GameObject PlayEffect(string name, Transform trans, bool bBind = false)
    {
        GameObject effectObj = GetEffectObj(name);
        if (effectObj != null)
        {
            effectObj.transform.position = trans.position;
            effectObj.transform.rotation = trans.rotation;

            if (bBind)
            {
                effectObj.transform.SetParent(trans);
            }
        }
        return effectObj;
    }


    public GameObject PlayEffect(string name, Vector3 point)
    {
        //Debuger.Log(name + " " + point);
        GameObject effectObj = GetEffectObj(name);
        if (effectObj != null)
        {
            effectObj.transform.position = point;
        }
        return effectObj;
    }

    const string EFFECT_PATH = "EffectLib/";
}
