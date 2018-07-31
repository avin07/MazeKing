using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// lcc
/// UGUI 策划需求的scrollbar 
/// </summary>

public class ScheduleBar : MonoBehaviour
{
    public enum Direction
    {
        LeftToRight = 0,
        TopToBottom = 1,
        BottomToTop = 2,
    }

    public RectTransform bg;

    public RectTransform point;

    [SerializeField]
    private Direction m_Direction = Direction.TopToBottom;
    public Direction direction
    {
        get
        {
            return m_Direction;
        }
        set
        {
            m_Direction = value;
            UpdateScheduleBar();
            SetPointPosion(this.value);
        }
    }

    [SerializeField]
    private float m_Value = 0;
    public float value
    {
        get
        {
            return m_Value;
        }
        set
        {
            m_Value = value;
            SetPointPosion(m_Value);
        }
    }

    void SetPointPosion(float m_value)//0到1
    {
        if (m_Direction == Direction.BottomToTop)
        {
            point.anchoredPosition = new Vector3(0, -(1 - m_value) * bg.sizeDelta.y, 0);
        }
        else
        {
            point.anchoredPosition = new Vector3(0, -m_value * bg.sizeDelta.y, 0);
        }

    }


    void UpdateScheduleBar()
    {
        if (m_Direction == Direction.LeftToRight)
        {
            bg.eulerAngles = new Vector3(0, 0, 90);
        }
        else
        {
            bg.eulerAngles = new Vector3(0, 0, 0);
        }
    }
}
