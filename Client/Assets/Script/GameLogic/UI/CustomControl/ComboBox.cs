using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// lcc
/// UGUI 定制控件下拉选项菜单
/// </summary>

class ComboBox : UIBehaviour 
{
    public Image bg;
    public Button item;
    public Button head;
    public GameObject body;
    float offset = 8.0f;
    float item_height;
    public delegate void VoidDelegate(ComboBox cb, PointerEventData eventData);
    public VoidDelegate OnComboBoxCilck;

    int m_value = 0;  //当前的值
    List<string> m_list = new List<string>();  
        
    public void Hide()
    {
        body.SetActive(false);
    }

    public void Show()
    {
        body.SetActive(true);
        item.gameObject.SetActive(false);
    }


    public void GetList(List<string> list)
    {
        HideAllItem();
        m_list.Clear();
        for (int i = 0; i < list.Count; i++)
        {
            m_list.Add(list[i]);
            GameObject temp = GetGameObject(body, item.name + i.ToString());
            if (temp == null)
            {
                temp = GameObject.Instantiate(item.gameObject) as GameObject;
                temp.name = item.name + i.ToString();
                temp.transform.SetParent(GetGameObject(body, "father").transform);
                temp.transform.localScale = Vector3.one;
            }
            temp.SetActive(true);
            GetText(temp, "Text").text = list[i];
            EventTriggerListener.Get(temp).onClick = OnItemClick;
            EventTriggerListener.Get(temp).SetTag(i);
        }
        bg.rectTransform.sizeDelta = new Vector2(bg.rectTransform.sizeDelta.x, item_height * m_list.Count + offset);
        Reset();
    }


    void HideAllItem()
    {
        for (int i = 0; i < m_list.Count; i++)
        {
            GameObject temp = GetGameObject(body, item.name + i.ToString());
            if (temp != null)
            {
                temp.SetActive(false);
            }
        }
    }

    void Awake()
    {
        EventTriggerListener.Get(head.gameObject).onClick = OnShow;
        EventTriggerListener.Get(head.gameObject).onDeselect = OnDeselect;
        item_height = item.gameObject.GetComponent<RectTransform>().sizeDelta.y;
        Hide();
    }

    void OnShow(GameObject go, PointerEventData data)
    {
        Show();
    }

    void OnItemClick(GameObject go, PointerEventData data)
    {

        SetHead((int)EventTriggerListener.Get(go).GetTag());
        if (OnComboBoxCilck != null)
        {
            OnComboBoxCilck(this, data);
        }
    }

    void OnDeselect(GameObject go, BaseEventData eventData)
    {
        StartCoroutine(WaitHide());
    }

    static public ComboBox GetComboBox(GameObject go)
    {
        ComboBox combobox = go.GetComponent<ComboBox>();
        if (combobox == null) combobox = go.AddComponent<ComboBox>();
        return combobox;
    }

    void SetHead(int index)
    {
        m_value = index;
        GetText(head.gameObject, "Text").text = m_list[index];
    }

    IEnumerator WaitHide()
    {
       yield return new WaitForSeconds(0.2f);
       Hide();
    }

    public int GetValue()
    {
        return m_value;
    }

    public void Reset()
    {
        SetHead(0);
    }
}
