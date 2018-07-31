using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;

public class UI_BuildState : UIBehaviour
{

        public RectTransform m_ProgressBar;
        public RectTransform m_TipInfo;
        public Image m_Levelup;
        public Image m_ResTip;
        Text m_Add;

        Vector2 up_offset = 100 * Vector2.up;

        Sprite level_up_icon; //临时实现功能

        void Awake()
        {
                m_ProgressBar.gameObject.SetActive(false);
                m_TipInfo.gameObject.SetActive(false);
                m_Levelup.gameObject.SetActive(false);
                m_ResTip.gameObject.SetActive(false);
                level_up_icon = GetImage(m_ProgressBar.gameObject, "icon").sprite;
                m_Add = FindComponent<Text>("add");
                m_Add.transform.SetActive(false);

                GetText(GetGameObject("bubble"), "text").text = "";
        }

        public void ShowBar(int rest_time, int max_time, string url)
        {
                GameObject obj = GetGameObject("bubble");
                
                if (rest_time > 0)
                {
                        m_ProgressBar.gameObject.SetActive(true);
                        GetText(m_ProgressBar.gameObject, "day").text = UIUtility.GetTimeString3(rest_time);
                        GetImage(m_ProgressBar.gameObject, "bar").fillAmount = (float)rest_time / max_time;
                        if (url.Length > 0)
                        {
                                ResourceManager.GetInst().LoadIconSpriteSyn(url, GetImage(m_ProgressBar.gameObject, "icon").transform);
                        }
                        else
                        {
                                GetImage(m_ProgressBar.gameObject, "icon").sprite = level_up_icon;
                        }
                        obj.SetActive(false);
                }
                else
                {
                        obj.SetActive(!string.IsNullOrEmpty(GetText(obj, "text").text));
                        m_ProgressBar.gameObject.SetActive(false);
                }
        }

        public void ShowLevelup(bool isShow)
        {
                m_Levelup.gameObject.SetActive(isShow);
        }

        public void ShowInfoTip(bool isShow,string url = "" ,string text = "" , bool bGray = false)
        {
                m_TipInfo.gameObject.SetActive(isShow);
                if(url.Length > 0)
                {
                        Transform icon = m_TipInfo.Find("Image");
                        ResourceManager.GetInst().LoadIconSpriteSyn(url, icon);
                        //m_TipInfo.Find("Image").GetComponent<Image>().SetNativeSize();
                        UIUtility.SetImageGray(bGray, icon.transform);
                }
                m_TipInfo.Find("Image/info").GetComponent<Text>().text = text;              
        }

        public Transform GetBarIcon()
        {
                return GetImage(m_ProgressBar.gameObject, "icon").transform;
        }

        public void ShowResTip(bool isShow, int res_type)
        {
                if (isShow)
                {
                        string url = CommonDataManager.GetInst().GetResourcesCfg(res_type).icon;
                        string attr_name = CommonDataManager.GetInst().GetResourcesCfg(res_type).attr;
                        ResourceManager.GetInst().LoadIconSpriteSyn(url, m_ResTip.transform);
                        if (PlayerController.GetInst().GetPropertyLong(attr_name) == PlayerController.GetInst().GetPropertyLong(attr_name + "_capacity"))
                        {
                                m_ResTip.color = Color.red;
                        }
                        else
                        {
                                m_ResTip.color = Color.white;
                        }
                        m_ResTip.SetNativeSize();
                }
                m_ResTip.gameObject.SetActive(isShow);
        }

        float effect_time = 1.5f;
        public void ShowResAdd(int value)
        {
                m_Add.gameObject.SetActive(true);
                m_Add.text = "+" + value.ToString();
                m_Add.rectTransform.anchoredPosition = m_ResTip.rectTransform.anchoredPosition + up_offset * 0.2f;
                m_Add.rectTransform.DOAnchorPos(m_ResTip.rectTransform.anchoredPosition + up_offset * 0.6f, effect_time);
                m_Add.DOFade(0.6f, effect_time).OnComplete(EffectFinish);
        }

        void EffectFinish()
        {
                m_Add.DOFade(1.0f, 0);
                m_Add.transform.SetActive(false);
        }
        public void ShowBubbleText(string text)
        {
                GameObject obj = GetGameObject("bubble");
                obj.SetActive(true);
                GetText(obj, "text").text = text;
        }
        //Vector2 pos;
        //public void SetPosition(Vector3 position)
        //{
        //        if (Camera.main != null)
        //        {
        //                RectTransformUtility.ScreenPointToLocalPointInRectangle(m_canvas.transform as RectTransform, Camera.main.WorldToScreenPoint(position), m_canvas.worldCamera, out pos);
        //                m_ProgressBar.anchoredPosition = pos + up_offset;
        //                m_TipInfo.anchoredPosition = pos + up_offset;
        //                m_Levelup.rectTransform.anchoredPosition = pos + down_offset;
        //                m_ResTip.rectTransform.anchoredPosition = pos + up_offset * 0.6f;
        //        }

        //}

}
