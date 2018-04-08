using UnityEngine;
using System.Collections;

public class CameraTouch : MonoBehaviour
{
        void Update()
        {
                UpdateTouch();
        }
        void UpdateTouch()
        {
                if (Input.touchCount > 0)
                {
                        Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                        RaycastHit hit;
                        if (Physics.Raycast(ray.origin, ray.direction * 10, out hit))
                        {
                                foreach (Touch touch in Input.touches)
                                {
                                        if (touch.phase == TouchPhase.Began)
                                        {
                                                hit.collider.SendMessage("OnMouseDown", SendMessageOptions.DontRequireReceiver);
                                        }
                                        else if (touch.phase == TouchPhase.Moved)
                                        {
                                                hit.collider.SendMessage("OnMouseDrag", SendMessageOptions.DontRequireReceiver);
                                        }
                                        else if (touch.phase == TouchPhase.Ended)
                                        {
                                                hit.collider.SendMessage("OnMouseUp", SendMessageOptions.DontRequireReceiver);
                                        }

                                }
                        }
                }
        }

}
