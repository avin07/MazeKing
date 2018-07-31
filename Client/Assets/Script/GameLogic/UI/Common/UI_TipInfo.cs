using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_TipInfo : UIBehaviour
{
    /// <summary>
    /// 通用界面暂时先为技能信息服务，以后都用代理来实现//
    /// </summary>
    public Text m_name;
    public Text m_text;

    public void SetName(string text)
    {
        m_name.text = text;
    }

    public void SetText(string text)
    {
        m_text.text = text;
    }


    void Update()
    {
        if (InputManager.GetInst().GetInputUp(true))
        {
            if (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.name.Contains("skill"))
            {

            }
            else
            {
                /*                    HeroPubManager.GetInst().SetCameraVague(false);*/
                UIManager.GetInst().CloseUI(this.name, 0.0f); //立即关闭
            }

        }
    }


}

