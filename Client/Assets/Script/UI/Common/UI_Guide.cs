using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

public class UI_Guide : UIBehaviour
{
        RectTransform arrowRt;
        RectTransform msgRt;
        RectTransform targetBoardRt;
        GameObject mWindow;
        Transform mShowBtn;
        UnityAction onGuideAction;
        UnityAction<bool> OnGuideToggle;
        Canvas m_canvas;
        int direction;
        string mBtnname;
        int m_nStartGuideId = 0;
        int guideType;
        GuideConfig m_GuideCfg;
        void Awake()
        {
                this.UILevel = UI_LEVEL.MAIN;
                arrowRt = transform.Find("arrow") as RectTransform ;
                msgRt = transform.Find("msg") as RectTransform;
                targetBoardRt = transform.Find("targetboard") as RectTransform;
                m_canvas = transform.GetComponent<Canvas>();
        }
        public override void OnShow(float time)
        {
                base.OnShow(time);
                Debug.Log("SetUIGuide " );
        }
        public override void OnClose(float time)
        {
                base.OnClose(time);
                Debug.Log("SetUIGuide false");
        }

        public void CloseGuide(int guideId)
        {
                if (m_GuideCfg != null && m_GuideCfg.id == guideId)
                {
                        UIManager.GetInst().CloseUI(this.name);
                }
        }

        public void SetSceneGuide(GuideConfig cfg)
        {
                m_GuideCfg = cfg;
                arrowRt.SetActive(false);
                msgRt.SetActive(false);
                SetTargetboard(cfg.target_prompt);
                mShowBtn = null;
                mWindow = null;
        }

        public void SetUIGuide(GuideConfig cfg, int startId) //下左上右//
        {
                m_GuideCfg = cfg;
                m_nStartGuideId = startId;
                guideType = cfg.type;

                mWindow = UIManager.GetInst().GetUIObj(cfg.display_interface);
                if (mWindow != null || guideType == 4)
                {
                        //msgRt.anchoredPosition = XMLPARSE_METHOD.ConvertToVector2(mGuide.operate_prompt_position);

                        if (guideType == 4)
                        {
                                mShowBtn = WorldMapNode(cfg.point_at);
                        }
                        else
                        {
                                mShowBtn = mWindow.transform.Find(cfg.point_at);
                        }

                        if (mShowBtn == null || mShowBtn.gameObject.activeSelf == false)
                        {
                                Debug.LogError("不存在控件" + cfg.point_at);
                                OnClickClose(null);
                                return;
                        }

                        SetTargetboard(cfg.target_prompt);

                        Text msgText = GetText(msgRt.gameObject, "Text");
                        msgText.text = LanguageManager.GetText(cfg.operate_prompt);

                        LayoutElement le = msgText.GetComponent<LayoutElement>();
                        le.preferredWidth = msgText.text.Length * 20;

                        mBtnname = mShowBtn.name;
                        direction = cfg.arrow_direction;
                        if (guideType == 4)
                        {
                                SetPosition3D();
                        }
                        else
                        {
                                SetPostion(direction);
                        }

                        Button btn = mShowBtn.GetComponent<Button>();
                        if (btn != null)
                        {
                                RemoveGuideClick();
                                onGuideAction = () => GuideClick(cfg);
                                btn.onClick.AddListener(onGuideAction);
                        }
                        else
                        {
                                Toggle toggle = mShowBtn.GetComponent<Toggle>();
                                if (toggle != null)
                                {
                                        OnGuideToggle = (x) => GuideClick(cfg);
                                        toggle.onValueChanged.AddListener(OnGuideToggle);
                                }
                        }
                }
                else
                {
                        gameObject.SetActive(false);
                        if (GameUtility.IsStringValid(cfg.display_interface))
                        {
                                singleton.GetInst().ShowMessage("当前不存在界面" + cfg.display_interface);
                        }
                }
        }

        void SetTargetboard(string str)
        {
                if (!string.IsNullOrEmpty(str))
                {
                        GetText(targetBoardRt.gameObject, "targettext").text = LanguageManager.GetText(str);

                        targetBoardRt.gameObject.SetActive(true);
                        targetBoardRt.sizeDelta = new Vector2((int)Mathf.Max(GetText("targettext").text.Length * 20, 200f) + 70, targetBoardRt.sizeDelta.y);
                        
                }
                else
                {
                        GetGameObject("targetboard").SetActive(false);
                }
        }

        void SetPostion(int arrow_direction)
        {
                if (mShowBtn == null || mWindow == null)
                {
                        return;
                }

                if (mShowBtn.name.Equals(mBtnname)) 
                {
                        arrowRt.SetActive(true);
                }
                else
                {
                        arrowRt.SetActive(false);
                }

                Vector2 pos = mShowBtn.TransformToCanvasLocalPosition(mWindow.transform, arrowRt);

                float width = 0.5f * (arrowRt.sizeDelta.x + (mShowBtn as RectTransform).sizeDelta.x);
                float height = 0.5f * (arrowRt.sizeDelta.y + (mShowBtn as RectTransform).sizeDelta.y);

                switch (arrow_direction)
                {
                        case 0:
                                pos += new Vector2(0, height);
                                break;
                        case 1:
                                pos += new Vector2(width, 0);
                                break;
                        case 2:
                                pos -= new Vector2(0, height);
                                break;
                        case 3:
                                pos -= new Vector2(width, 0);
                                break;
                }
                arrowRt.anchoredPosition = pos;
                arrowRt.localEulerAngles = Vector3.back * 90 * arrow_direction;

                Text msgText = GetText(msgRt.gameObject, "Text");
                LayoutElement le = msgText.GetComponent<LayoutElement>();
                int msgwidth = (int)Mathf.Max(msgText.text.Length * 20, le.minWidth) + 40;
                if (arrowRt.anchoredPosition.x + msgwidth > Screen.width / 2f)
                {
                        msgRt.anchoredPosition = new Vector2(arrowRt.anchoredPosition.x - msgwidth / 2f - arrowRt.sizeDelta.x / 2f, arrowRt.anchoredPosition.y);
                }
                else
                {
                        msgRt.anchoredPosition = new Vector2(arrowRt.anchoredPosition.x + msgwidth / 2f + arrowRt.sizeDelta.x / 2f, arrowRt.anchoredPosition.y);
                }

                msgRt.gameObject.SetActive(false);
                arrowRt.gameObject.SetActive(mShowBtn.gameObject.activeInHierarchy);
        }
        
        void GuideClick(GuideConfig mGuide)  //暂时只有点击按钮
        {
                RemoveGuideClick();
                Debug.Log("GuideClick " + mGuide.next);
                if (mGuide.next > 0)
                {
                        GuideManager.GetInst().GotoNextGuide(mGuide.next, m_nStartGuideId);
                }
                else
                {
                        GuideManager.GetInst().SaveGuide(m_nStartGuideId);
                }
                if (this != null)
                {
                        gameObject.SetActive(false);
                }
                
        }

        public void CloseGuide(GameObject uiObj)
        {
                if (mWindow == uiObj)
                {
                        gameObject.SetActive(false);
                }
        }


        void RemoveGuideClick()
        {
                if (mShowBtn != null)
                {
                        Button btn = mShowBtn.GetComponent<Button>();
                        if (btn != null && onGuideAction != null)
                        {
                                btn.onClick.RemoveListener(onGuideAction);
                        }

                        Toggle toggle = mShowBtn.GetComponent<Toggle>();
                        if (toggle != null && OnGuideToggle!= null)
                        {
                                toggle.onValueChanged.RemoveListener(OnGuideToggle);
                        }
                }
        }

        Vector2 pos;
        public void SetPosition3D()
        {
                if (mShowBtn == null)
                {
                        return;
                }

                if (Camera.main == null)
                {
                        return;
                }

                if (m_canvas != null)
                {

                        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_canvas.transform as RectTransform, Camera.main.WorldToScreenPoint(mShowBtn.transform.position), m_canvas.worldCamera, out pos);
                        arrowRt.anchoredPosition = pos + Vector2.up * 40;
                        Text msgText = GetText(msgRt.gameObject, "Text");
                        LayoutElement le = msgText.GetComponent<LayoutElement>();
                        int msgwidth = (int)Mathf.Max(msgText.text.Length * 20, le.minWidth) + 40;
                        msgRt.anchoredPosition = new Vector2(arrowRt.anchoredPosition.x + msgwidth / 2f + arrowRt.sizeDelta.x / 2f, arrowRt.anchoredPosition.y);
                        msgRt.gameObject.SetActive(false);
                        arrowRt.gameObject.SetActive(mShowBtn.gameObject.activeInHierarchy);
                }
        }

        Transform WorldMapNode(string point_at)
        {
                string[] temp = point_at.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                return WorldMapManager.GetInst().GetNodeButton(int.Parse(temp[0]), int.Parse(temp[1]));
        }

        void Update()
        {
                if (guideType == 4)
                {
                        SetPosition3D();
                }
                else
                {
                        SetPostion(direction);
                }
        }
}


