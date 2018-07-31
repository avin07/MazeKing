using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_CheckBox : UIBehaviour
{


    void Awake()
    {
        this.UILevel = UI_LEVEL.TIP;
    }
    /// <summary>
    /// 确认界面，目前只能显示一个以后有需求再改
    /// </summary>
    public Text m_title;
    public Text m_text;
    public GameObject m_confirm;
    public GameObject m_cancel;
    public GameObject m_ok;

    public delegate void Handler(object data);
    //public delegate void CheckHandler(object data,bool isCheck); 暂时没有这个以后看需求

     public Handler confirmHandler;
     public Handler cancelHandler;
     object m_data;


    public void SetConfirmAndCancel(string title,string text,Handler confirm,Handler cancel,object data)
    {
        SetTitle(title);
        SetText(text);
        confirmHandler = confirm;
        cancelHandler = cancel;
        m_data = data;

        m_confirm.SetActive(true);
        m_cancel.SetActive(true);
        m_ok.SetActive(false);
        
    }

    public void SetOnlyOK(string title, string text, Handler confirm,object data)
    {
        SetTitle(title);
        SetText(text);
        confirmHandler = confirm;
        m_data = data;

        m_confirm.SetActive(false);
        m_cancel.SetActive(false);
        m_ok.SetActive(true);
        
    }


    void SetTitle(string text)
    {
        m_title.text = text;
    }

    void SetText(string text)
    {
        m_text.text = text;
    }

    public void ClickConfirm()
    {
        if (confirmHandler != null)
        {
            confirmHandler(m_data);
        }
        UIManager.GetInst().CloseUI(this.name);
    }

    public void ClickCancle()
    {
        if (cancelHandler != null)
        {
            cancelHandler(m_data);
        }
        UIManager.GetInst().CloseUI(this.name);
    }

    public void ClickOk()
    {
        if (confirmHandler != null)
        {
            confirmHandler(m_data);
        }
        UIManager.GetInst().CloseUI(this.name);
    }




}


