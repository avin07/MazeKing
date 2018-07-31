using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

class UI_BuildCreatConfirm : UIBehaviour
{
        public Image m_bg;
        Vector2 offset = Vector2.up * 100;
        Canvas m_canvas;

        public delegate void Handler(object data);

        public Handler confirmHandler;
        public Handler cancelHandler;
        object m_data;

        Transform yesTransform;
        Transform noTransform;
        RectTransform title;

        void Awake()
        {
                this.UILevel = UI_LEVEL.MAIN;
                m_canvas = transform.GetComponent<Canvas>();
                yesTransform = m_bg.transform.Find("yes");
                noTransform = m_bg.transform.Find("no");
                title = transform.Find("title") as RectTransform;
        }

        Vector2 pos;
        public void SetPosition(Vector3 position)
        {
                if (m_canvas != null)
                {
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_canvas.transform as RectTransform, Camera.main.WorldToScreenPoint(position), m_canvas.worldCamera, out pos);
                        m_bg.rectTransform.anchoredPosition = pos + offset;
                        title.anchoredPosition = m_bg.rectTransform.anchoredPosition + Vector2.up * 55;
                }
        }

        public void SetConfirmAndCancel(string title,Handler confirm, Handler cancel, object data)
        {
                FindComponent<Text>("title").text = title;
                confirmHandler = confirm;
                cancelHandler = cancel;
                m_data = data;
        }

        public void SetForFurnitureCreat()
        {
                yesTransform.SetActive(true);
                noTransform.SetActive(true);
                m_bg.rectTransform.sizeDelta = new Vector2(200, m_bg.rectTransform.sizeDelta.y);
        }

        public void Ok()
        {
                //UIManager.GetInst().CloseUI(this.name);
                if (confirmHandler != null)
                {
                        confirmHandler(m_data);
                }
        }

        public void Cancel()
        {
                //UIManager.GetInst().CloseUI(this.name);
                if (cancelHandler != null)
                {
                        cancelHandler(m_data);
                }
        }

        public void SetOKInteractable(bool bInteractable)
        {
                yesTransform.GetComponent<Button>().interactable = bInteractable;
        }
}


