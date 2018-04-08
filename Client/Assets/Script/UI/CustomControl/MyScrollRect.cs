using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// lcc
/// 为了实现策划的需求，滑动时显示滚动条，其他时间不显示
/// </summary>

public class MyScrollRect : ScrollRect
{
        //暂时只支持列表的pivot为（0，1），锚点为左上角的
        private ScheduleBar m_schedulebar;

        public void SetSchedulebarDirection(ScheduleBar.Direction direction)
        {
                m_schedulebar.direction = direction;
        }

        new void Awake()
        {
                if (horizontalScrollbar != null)
                {
                        horizontalScrollbar.gameObject.SetActive(false);
                }
                if (verticalScrollbar != null)
                {
                        verticalScrollbar.gameObject.SetActive(false);
                }

                foreach (ScheduleBar sb in gameObject.GetComponentsInChildren<ScheduleBar>(true))
                {
                        m_schedulebar = sb;
                        break;
                }
                if (m_schedulebar != null)
                {
                        m_schedulebar.gameObject.SetActive(false);
                }
        }


        public override void OnBeginDrag(PointerEventData eventData)
        {
                if (!CanDarg())
                {
                        return;
                }
                base.OnBeginDrag(eventData);
                if (horizontalScrollbar != null)
                {
                        horizontalScrollbar.gameObject.SetActive(true);
                }
                if (verticalScrollbar != null)
                {
                        verticalScrollbar.gameObject.SetActive(true);
                }
                if (m_schedulebar != null)
                {
                        StopCoroutine("ScheduleBarClose");
                        m_schedulebar.gameObject.SetActive(true);
                        m_schedulebar.GetComponent<CanvasGroup>().alpha = 1;
                }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
                base.OnEndDrag(eventData);
                if (horizontalScrollbar != null)
                {
                        horizontalScrollbar.gameObject.SetActive(false);
                }
                if (verticalScrollbar != null)
                {
                        verticalScrollbar.gameObject.SetActive(false);
                }
                if (m_schedulebar != null)
                {
                        StartCoroutine("ScheduleBarClose");
                }
        }

        public override void OnDrag(PointerEventData eventData)
        {
                if (!CanDarg())
                {
                        return;
                }
                base.OnDrag(eventData);
                if (m_schedulebar != null)
                {
                        float m_value = 0;
                        if (m_schedulebar.direction == ScheduleBar.Direction.BottomToTop)
                        {
                                if (content.anchoredPosition.y < 0)
                                {
                                        m_value = Mathf.Abs(content.anchoredPosition.y) / (content.sizeDelta.y - (transform as RectTransform).sizeDelta.y);
                                }
                        }
                        else if (m_schedulebar.direction == ScheduleBar.Direction.TopToBottom)
                        {
                                if (content.anchoredPosition.y > 0)
                                {
                                        m_value = Mathf.Abs(content.anchoredPosition.y) / (content.sizeDelta.y - (transform as RectTransform).sizeDelta.y);
                                }
                        }
                        else
                        {
                                if (content.anchoredPosition.x < 0)
                                {
                                        m_value = Mathf.Abs(content.anchoredPosition.x) / (content.sizeDelta.x - (transform as RectTransform).sizeDelta.x);
                                }
                        }
            
                        m_value = Mathf.Clamp(m_value, 0, 1);
                        m_schedulebar.value = m_value;
                }
        }


        IEnumerator ScheduleBarClose()
        {
                yield return new WaitForSeconds(2.0f);
                float time = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - time <= 1.0f)
                {
                        float delt_alph = Mathf.Lerp(1, 0, (Time.realtimeSinceStartup - time) / 1.0f);
                        m_schedulebar.GetComponent<CanvasGroup>().alpha = delt_alph;
                        yield return null;
                }
                m_schedulebar.GetComponent<CanvasGroup>().alpha = 0;
                m_schedulebar.gameObject.SetActive(false);
        }



        bool CanDarg()
        {
                if(m_schedulebar != null)
                {
                        float content_length = 0;
                        float scroll_rect_length = 0;
                        if(m_schedulebar.direction == ScheduleBar.Direction.LeftToRight )
                        {
                                scroll_rect_length = (transform as RectTransform).sizeDelta.x;
                                content_length = content.sizeDelta.x;
                        }
                        else
                        {
                                scroll_rect_length = (transform as RectTransform).sizeDelta.y;
                                content_length = content.sizeDelta.y;
                        }

                        return content_length > scroll_rect_length ? true : false;

                }
                return true;
        }
}

