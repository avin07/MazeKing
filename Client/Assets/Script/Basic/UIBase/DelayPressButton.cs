using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class DelayButtonEvent : UnityEvent<GameObject>
{
}
public class DelayPressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
        public bool invokeOnce = false;//是否只调用一次  
        private bool hadInvoke = false;//是否已经调用过  

        public float DelayTime = 0.2f;//按下后超过这个时间则认定为"长按"  
        private bool isPointerDown = false;
        private float recordTime;

        public DelayButtonEvent  onPressDelay = new DelayButtonEvent ();//按住时调用  
        public DelayButtonEvent  onReleaseBeforeDelay = new DelayButtonEvent ();//在松开时调用  

        void Start()
        {

        }

        void Update()
        {
                if (/*invokeOnce && */hadInvoke)
                        return;
                if (isPointerDown)
                {
                        if ((Time.time - recordTime) > DelayTime)
                        {
                                onPressDelay.Invoke(this.gameObject);
                                hadInvoke = true;
                        }
                }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
                isPointerDown = true;
                recordTime = Time.time;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
                isPointerDown = false;

                //未触发长按回调时，调用未长按回调
                if (hadInvoke == false)
                {
                        onReleaseBeforeDelay.Invoke(this.gameObject);
                }
                else
                {
                        hadInvoke = false;
                }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
//                 if (isPointerDown)
//                 {
//                         isPointerDown = false;
// //                        onReleaseBeforeDelay.Invoke(this.gameObject);
//                 }
//                 hadInvoke = false;

        }
}