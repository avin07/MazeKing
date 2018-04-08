using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class EventTriggerListener : UnityEngine.EventSystems.EventTrigger
{

        /// <summary>
        /// 以后scrollrect中统一不能使用该方法进行事件绑定，sr的拖动事件会被该脚本中的拖动事件覆盖
        /// 可以使用  btn.onClick.AddListener(() => ()); 
        /// 该事件系统适用于ugui 和 3d物体 ，3d 物体需要在摄像机上添加PhysicsRaycaster
        /// </summary>
        /// <param name="go"></param>
        /// <param name="eventData"></param>

        public Action<GameObject, PointerEventData> onClick;
        public Action<GameObject, PointerEventData> onDown;
        public Action<GameObject, PointerEventData> onEnter;
        public Action<GameObject, PointerEventData> onExit;
        public Action<GameObject, PointerEventData> onUp;
        public Action<GameObject, PointerEventData> onDrag;
        public Action<GameObject, PointerEventData> onBeginDrag;
        public Action<GameObject, PointerEventData> onEndDrag;
        public Action<GameObject, PointerEventData> onDrop;
        public Action<GameObject, BaseEventData> onSelect;
        public Action<GameObject, BaseEventData> onUpdateSelect;
        public Action<GameObject, BaseEventData> onDeselect;
        public Action<GameObject, AxisEventData> onMove;

        static public EventTriggerListener Get(GameObject go)
        {
                EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
                if (listener == null) listener = go.AddComponent<EventTriggerListener>();
                return listener;
        }

        static public EventTriggerListener Get(Transform tf)
        {
                EventTriggerListener listener = tf.gameObject.GetComponent<EventTriggerListener>();
                if (listener == null) listener = tf.gameObject.AddComponent<EventTriggerListener>();
                return listener;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
                if (eventData.dragging)
                {
                        return;
                }
                if (onClick != null)
                {
                        onClick(gameObject, eventData);
                }
        }
        public override void OnPointerDown(PointerEventData eventData)
        {
                if (onDown != null) onDown(gameObject, eventData);
        }
        public override void OnPointerEnter(PointerEventData eventData)
        {
                if (onEnter != null) onEnter(gameObject, eventData);
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
                if (onExit != null) onExit(gameObject, eventData);
        }
        public override void OnPointerUp(PointerEventData eventData)
        {
                if (onUp != null) onUp(gameObject, eventData);
        }
        public override void OnDrag(PointerEventData eventData)
        {
                if (onDrag != null) onDrag(gameObject, eventData);
        }
        public override void OnBeginDrag(PointerEventData eventData)
        {
                if (onBeginDrag != null) onBeginDrag(gameObject, eventData);
        }
        public override void OnEndDrag(PointerEventData eventData)
        {
                if (onEndDrag != null) onEndDrag(gameObject, eventData);
        }

        public override void OnDrop(PointerEventData eventData)
        {
                if (onDrop != null) onDrop(gameObject, eventData);
        }

        public override void OnSelect(BaseEventData eventData)
        {
                if (onSelect != null) onSelect(gameObject, eventData);
        }
        public override void OnUpdateSelected(BaseEventData eventData)
        {
                if (onUpdateSelect != null) onUpdateSelect(gameObject, eventData);
        }
        public override void OnDeselect(BaseEventData eventData)
        {
                if (onDeselect != null) onDeselect(gameObject, eventData);
        }

        public override void OnMove(AxisEventData eventData)
        {
                if (onMove != null) onMove(gameObject, eventData);
        }
       
        private object Tag;
        public void SetTag(object m_tag)
        {
                Tag = m_tag;
        }

        public object GetTag()
        {
                return Tag;           
        }

}