using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

class TestHeroBehaviour : MonoBehaviour, IDragHandler
{

        void Awake()
        {
                GameUtility.SetLayer(gameObject, "Character");      
                BoxCollider box = gameObject.AddComponent<BoxCollider>();
                box.size = new Vector3(1, 2, 1);
                box.center = new Vector3(0, 1, 0);
        }

        void OnEnable()
        {
                GameUtility.ObjPlayAnim(gameObject, CommonString.idle_001Str, true);
        }

        float rotate_offset = -1.5f;
        public void OnDrag(PointerEventData eventData)
        {
                gameObject.transform.Rotate(Vector3.up, eventData.delta.x * rotate_offset);               
        }
}