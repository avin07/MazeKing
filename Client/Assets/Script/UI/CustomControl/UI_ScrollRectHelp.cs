using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

public class UI_ScrollRectHelp : UIBehaviour
{

    public ScrollRect sr;


    protected bool can_click = true;



    public virtual void OnDarg(GameObject go, PointerEventData data)
    {
        sr.OnDrag(data);
    }

    public virtual void OnBeginDrag(GameObject go, PointerEventData data)
    {
        can_click = false;
        sr.OnBeginDrag(data);
    }

    public virtual void OnEndDrag(GameObject go, PointerEventData data)
    {
        sr.OnEndDrag(data);
        can_click = true;
    }

   


}


