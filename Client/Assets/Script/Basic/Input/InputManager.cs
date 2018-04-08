using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

class InputManager : SingletonObject<InputManager>
{
        public Vector3 GetInputPosition()
        {
#if UNITY_ANDROID || UNITY_IPHONE
                if (Input.touchCount > 0)
                {
                        return Input.GetTouch(0).position;
                }
#endif
                return Input.mousePosition;
        }

        public bool GetInputUp(bool isIgnoreUIclick) //是否忽略UGUI点击
        {
                if (isIgnoreUIclick == false && IsPointerOverUgui())
                {
                        return false;
                }

                bool bInput = false;

                bInput = Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1);

                return bInput;
        }

        public bool GetInputDown(bool isIgnoreUIclick)
        {
                if (isIgnoreUIclick == false && IsPointerOverUgui())
                {
                        return false;
                }
                bool bInput = false;
                bInput = Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1);
              
                return bInput;
        }

        public bool GetInputHold(bool isIgnoreUIclick)
        {
                if (isIgnoreUIclick == false && IsPointerOverUgui())
                {
                        return false;
                }

                bool bInput = false;
                bInput = Input.GetMouseButton(0) || Input.GetMouseButton(1);
              
                return bInput;
        }

        public bool IsTouchDarg()
        {
                bool bInput = false;
                if (Input.touchCount == 1)  //单指
                {
                        Touch touch = Input.GetTouch(0);
                        bInput = touch.phase == TouchPhase.Moved;
                }
                return bInput;
        }


        public bool IsPointerOverUgui()
        {
                PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
                eventDataCurrentPosition.position = Input.mousePosition;
                eventDataCurrentPosition.pressPosition = Input.mousePosition;
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
                if (results.Count > 0)
                {
                        if (results[0].module is GraphicRaycaster) //点击到了ugui上//
                        {
                                return true;
                        }
                }
                return false;
        }

        public bool IsPointerOverAnyThing()
        {
                PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
                eventDataCurrentPosition.position = Input.mousePosition;
                eventDataCurrentPosition.pressPosition = Input.mousePosition;
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
                if (results.Count > 0)
                {
                        return true;
                }
                return false;
        }

        public bool IsMouseInScreen()
        {
                if (Input.mousePosition.x < 0)
                        return false;
                if (Input.mousePosition.x > Screen.width)
                        return false;
                if (Input.mousePosition.y < 0)
                        return false;
                if (Input.mousePosition.y > Screen.height)
                        return false;
                return true;
        }

        EventSystem eventSystem;
        public void Init()
        {
                eventSystem = EventSystem.current;
        }

        public void SwitchInup(bool isOpen)  //锁住输入
        {
                eventSystem.enabled = isOpen;
        }

        public Action onMouseScroll;        //鼠标滚轮
        public Action onClickDown;          //按下
        public Action enterDarg;            //进入滑动
        public Action enterLongPressDarg;   //进入长按滑动
        public Action enterLongPress;       //进入长按
        public Action onDarg;               //滑动
        public Action onLongPressDarg;      //长按滑动
        public Action onDargOver;           //滑动结束
        public Action onLongPressDargOver;  //长按滑动结束
        public Action onClickUp;            //抬起
        public Action onMultiTouchMove;     //多点滑动

        bool m_bDown = false;
        bool m_bDarg = false;
        bool m_bLongPress = false;
        bool m_bIgnoreUI = false;               //为false时只响应ui
        float DragDelta = 0.2f;
        public float LongPressSpace = 0.5f;
        float m_downStartTime;


        public void UpdateInputReset()    //全局输入重置//
        {
                onMouseScroll = null;
                onClickDown = null;
                enterDarg = null;
                enterLongPressDarg = null;
                enterLongPress = null;
                onDarg = null;
                onLongPressDarg = null;
                onDargOver = null;
                onLongPressDargOver = null;
                onClickUp = null;
                onMultiTouchMove = null;

                InputStateReset();
        }

        public void InputStateReset()
        {
                m_bDown = false;
                m_bDarg = false;
                m_bLongPress = false;
        }

        public void SetbIgnoreUI(bool IsIgnoreUI)
        {
                m_bIgnoreUI = IsIgnoreUI;
        }

        public void UpdateGlobalInput()   //全局手势（不依附于unity3d事件系统）
        {

                if (EventSystem.current == null)
                {
                        return;
                }
                if (Input.GetAxis("Mouse ScrollWheel") != 0)  //PC端滚轮
                {
                        if (onMouseScroll != null)
                        {
                                onMouseScroll();
                        }
                }

                if (Input.touchCount <= 1)
                {

                        if (!m_bDown)  //所有后续输入状态都基于第一次成功的down！
                        {
                                if (InputManager.GetInst().GetInputDown(true)) //鼠标按下
                                {
                                        if (!m_bIgnoreUI)
                                        {
                                                if (InputManager.GetInst().IsPointerOverUgui()) //和ui点击重叠时只响应ui点击//
                                                {
                                                        return;
                                                }
                                        }

                                        m_bDown = true;
                                        m_downStartTime = Time.realtimeSinceStartup;
                                        if (onClickDown != null)
                                        {
                                                onClickDown();
                                        }
                                }
                        }
                        else
                        {
                                if (InputManager.GetInst().GetInputHold(true))
                                {
                                        if (!m_bDarg)
                                        {
                                                if (Mathf.Abs(Input.GetAxis("Mouse X")) > DragDelta || Mathf.Abs(Input.GetAxis("Mouse Y")) > DragDelta)
                                                {
                                                        m_bDarg = true;
                                                        if (m_bLongPress && enterLongPressDarg != null)  //在没有注册长按事件的时候使用滑动事件
                                                        {
                                                                //进入长按滑动
                                                                enterLongPressDarg();
                                                        }
                                                        else
                                                        {
                                                                if (enterDarg != null)
                                                                {
                                                                        enterDarg();
                                                                }
                                                        }                                                       
                                                }
                                                else
                                                {
                                                        if (Time.realtimeSinceStartup - m_downStartTime >= LongPressSpace)
                                                        {
                                                                if (!m_bLongPress)  //非滑动状态下才能进入长按模式
                                                                {
                                                                        m_bLongPress = true;
                                                                        if (enterLongPress != null)
                                                                        {
                                                                                enterLongPress();
                                                                        }
                                                                }
                                                        }
                                                }
                                        }                                      
                                }
                                if (m_bDarg)  //滑动
                                {
                                        if (m_bLongPress && onLongPressDarg != null)
                                        {
                                                onLongPressDarg();    
                                        }
                                        else
                                        {
                                                if (onDarg != null)
                                                {
                                                        onDarg();
                                                }
                                        }
                                }
                                if (InputManager.GetInst().GetInputUp(true))
                                {
                                        if (m_bDarg) //拖屏后抬起
                                        {
                                                if (m_bLongPress && onLongPressDargOver != null)
                                                {
                                                        onLongPressDargOver();                                                      
                                                }
                                                else
                                                {
                                                        if (onDargOver != null)
                                                        {
                                                                onDargOver();
                                                        }
                                                }
                                        }
                                        else         //没有拖屏幕就抬起
                                        {
                                                if (onClickUp != null)
                                                {
                                                        onClickUp();
                                                }
                                        }
                                        InputStateReset();
                                }
                        }
                }
                else   //多点触控
                {
                        InputStateReset();
                        if (onMultiTouchMove != null)
                        {
                                onMultiTouchMove();
                        }
                }                         
        }

}
