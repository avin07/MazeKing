using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class UI_HeroDialog : UIBehaviour
{
        Canvas m_Canvas;
        RectTransform m_BaseRT;
        public Transform m_BelongTransform;
        Text m_Text;
	void Awake()
	{
                m_Canvas = GetComponent<Canvas>();
                m_BaseRT = GetImage("bg").gameObject.GetComponent<RectTransform>();
                m_Text = GetText("text");
        }

        public void SetText(string text, Transform belongTrans)
        {
                m_Text.text = text;
                m_BelongTransform = belongTrans;
        }
        void Update()
        {
                if (m_BaseRT != null && m_BelongTransform != null)
                {
                        m_BaseRT.anchoredPosition = UIUtility.WorldToCanvas(m_Canvas, m_BelongTransform.position + Vector3.up * 2f);
                }
        }
}