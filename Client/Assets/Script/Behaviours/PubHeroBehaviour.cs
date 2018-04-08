using UnityEngine;
using System.Collections;

public class PubHeroBehaviour : MonoBehaviour
{
    public int desk;
    public int charactar_id;
    public int index;

    void OnMouseUpAsButton()
    {
        if (!HeroPubManager.GetInst().CanChoose())
        {
            return;
        }

        HeroPubManager.GetInst().SelectCircleObj.transform.SetParent(this.gameObject.transform);
        HeroPubManager.GetInst().SelectCircleObj.transform.localPosition = Vector3.zero;
        HeroPubManager.GetInst().SetHeroInfo(desk, charactar_id, index,this.gameObject);

        UI_HeroPub phb = HeroPubManager.GetInst().GetMyHeroPub();
        if (phb != null)
        {
            phb.SetBgActive(true);
            phb.UpdateHeroInfo(charactar_id, desk);
        }

    }

    public void Disappear(float delaytime,float maxtime)
    {
        ModelDisappear md = gameObject.AddComponent<ModelDisappear>();
        md.DelayTime = delaytime;
        md.MaxTime = maxtime;
    }

}
