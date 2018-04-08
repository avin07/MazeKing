using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class UI_NeedConfirm : UIBehaviour
{

        Vector2 costPos;
        RectTransform costTf;
        RectTransform backTf;

        void Awake()
        {
                this.UILevel = UI_LEVEL.TIP;
                costTf = transform.Find("Image/cost") as RectTransform;
                backTf = transform.Find("Image/back") as RectTransform;
                costPos = costTf.anchoredPosition;
        }

        public Text m_title;
        public GameObject m_confirm;
        public GameObject m_cancel;

        public delegate void Handler(object data);
        //public delegate void CheckHandler(object data,bool isCheck); 暂时没有这个以后看需求

        public Handler confirmHandler;
        public Handler cancelHandler;
        object m_data;
        bool is_enough = true;

        public void SetConfirmAndCancel(string title, string need_info, string back_info, Handler confirm, Handler cancel, object data )
        {
                SetTitle(title);
                confirmHandler = confirm;
                cancelHandler = cancel;
                m_data = data;

                m_confirm.SetActive(true);
                m_cancel.SetActive(true);
                costTf.SetActive(false);
                backTf.SetActive(false);


                //花费和返还都有
                if (GameUtility.IsStringValid(need_info) && GameUtility.IsStringValid(back_info))
                {
                        RefreshNeed(need_info);
                        RefreshBack(back_info);
                        costTf.SetActive(true);
                        backTf.SetActive(true);

                }

                if (GameUtility.IsStringValid(need_info) && !GameUtility.IsStringValid(back_info))
                {
                        RefreshNeed(need_info);
                        costTf.SetActive(true);
                }

                if (!GameUtility.IsStringValid(need_info) && GameUtility.IsStringValid(back_info))
                {
                        RefreshBack(back_info);
                        backTf.anchoredPosition = costPos;
                        backTf.SetActive(true);
                }
        }


        void RefreshNeed(string need_info)
        {
                GameObject root = costTf.gameObject;
                root.SetActive(true);

                GameObject item = root.transform.GetChild(1).gameObject;
                RectTransform rt = item.transform as RectTransform;
                string[] need = need_info.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                SetGameObjectHide("item", root);

                int num = 0;
                string name = "";
                int id = 0;
                string des = "";
                Thing_Type type;


                for (int i = 0; i < need.Length; i++)
                {
                        GameObject temp = GetGameObject(root, "item" + i);
                        if (temp == null)
                        {
                                temp = CloneElement(item, "item" + i);
                        }
                        (temp.transform as RectTransform).anchoredPosition = new Vector2(rt.anchoredPosition.x + i * (rt.sizeDelta.x + 12f), rt.anchoredPosition.y);
                        CommonDataManager.GetInst().SetThingIcon(need[i], temp.transform, null,out name, out num, out id, out des,out type);
                        int has_num = CommonDataManager.GetInst().GetThingNum(need[i], out num);
                        if (type == Thing_Type.RESOURCE)
                        {
                                if (has_num >= num)
                                {
                                        GetText(temp, "num").text = num.ToString();
                                }
                                else
                                {
                                        GetText(temp, "num").text = "<color=red>" + num + "</color>";
                                        is_enough = false;
                                }

                        }
                        else
                        {
                        if (has_num >= num)
                        {
                                GetText(temp, "num").text = has_num + " / " + num;
                        }
                        else
                        {
                                GetText(temp, "num").text = "<color=red>" + has_num + "</color>" + " / " + num;
                                is_enough = false;
                        }
                        }

                        temp.SetActive(true);
                }
        }


        void RefreshBack(string back_info)
        {
                GameObject root = backTf.gameObject;
                root.SetActive(true);

                GameObject item = root.transform.GetChild(1).gameObject;
                RectTransform rt = item.transform as RectTransform;
                string[] back = back_info.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                SetGameObjectHide("item", root);

                int num = 0;
                string name = "";
                int id = 0;
                string des = "";
                Thing_Type type;


                for (int i = 0; i < back.Length; i++)
                {
                        GameObject temp = GetGameObject(root, "item" + i);
                        if (temp == null)
                        {
                                temp = CloneElement(item, "item" + i);
                        }
                        (temp.transform as RectTransform).anchoredPosition = new Vector2(rt.anchoredPosition.x + i * (rt.sizeDelta.x + 12f), rt.anchoredPosition.y);
                        CommonDataManager.GetInst().SetThingIcon(back[i], temp.transform, null, out name, out num, out id, out des, out type);

                        GetText(temp, "num").text = num.ToString();   

                        temp.SetActive(true);
                }
        }

        void SetTitle(string text)
        {
                m_title.text = text;
        }

        public void ClickConfirm()
        {
                if (confirmHandler != null)
                {
                        if (is_enough)
                        {
                                confirmHandler(m_data);
                        }
                        else
                        {
                                GameUtility.PopupMessage("材料不足！");
                        }

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



}


