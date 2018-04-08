using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;

class UI_CutSceneDialog : UIBehaviour
{
        Vector2[] m_DialogOffset = new Vector2[4]
        {
                new Vector2(-32,-10),         //left_bottom
                new Vector2(-29,43),          //left_top
                new Vector2(27,38),          //right_top
                new Vector2(32,-20)           //right_bottom
        };
        public List<GameObject> m_UnitDialogs = new List<GameObject>();
        public GameObject m_SelectionGroup;
        public GameObject m_AsideDialog;

        GameObject m_CurrDialog;

        public Text m_CenterText;
        public GameObject m_CenterTextBg;

        void Awake()
        {
                m_SelectionGroup.SetActive(false);
                m_AsideDialog.SetActive(false);
                m_CenterTextBg.SetActive(false);
                ClearDialogs();
        }
        void ClearDialogs()
        {
                foreach (GameObject dialog in m_UnitDialogs)
                {
                        dialog.SetActive(false);
                }
                m_CurrDialog = null;
        }


        bool m_bNeedWait = false;

        public void OnClickNext()
        {
                ClearDialogs();
                m_AsideDialog.SetActive(false);
                m_CenterTextBg.SetActive(false);
                CutSceneManager.GetInst().EndWaiting();
                m_bNeedWait = false;
                UIManager.GetInst().CloseUI(this.name);
/*                Debug.Log("OnClickNext");*/
        }
        public void OnClickSelection(int tag)
        {
                CutSceneManager.GetInst().GotoCmdIndex(tag);
                CutSceneManager.GetInst().EndWaiting();
                m_SelectionGroup.SetActive(false);
                UIManager.GetInst().CloseUI(this.name);
        }

        int GetDialog(bool bLeft, bool bTop)
        {
                if (bLeft)
                {
                        if (bTop)
                        {
                                return 1;
                        }
                        else
                        {
                                return 0;
                        }
                }
                else
                {
                        if (bTop)
                        {
                                return 2;
                        }
                        else
                        {
                                return 3;
                        }
                }
        }

        public void SetUnitDialog(string icon, Vector3 targetPos, CutSceneCmd_Dialog cmd)
        {
                m_CenterTextBg.SetActive(false);
                //ClearDialogs();
                Vector2 pos = Camera.main.WorldToScreenPoint(targetPos + Vector3.up);
                bool bLeft = pos.x < Screen.width / 2;
                bool bTop = pos.y > Screen.height / 2;

                float width = 20 * cmd.line_count + 158f;       //158是UI里调的(包括边距和间距，icon 128）
                float height = 168f + 20f;            //168也是预设的高度

                if (bLeft && pos.x < width)
                {
                        pos.x = width;
                        bLeft = false;
                }
                else if (!bLeft && pos.x > Screen.width - width)
                {
                        pos.x = Screen.width - width;
                        bLeft = true;
                }
                
                if (!bTop && pos.y < height)
                {
                        pos.y = height;
                        bTop = true;
                }
                else if (bTop && pos.y > Screen.height - height)
                {
                        pos.y = Screen.height - height;
                        bTop = false;
                }

                int dialogIdx = GetDialog(bLeft, bTop);
/*                Debuger.Log(pos + " bLeft=" + bLeft + " top=" + bTop + " " + dialogIdx);*/
                m_CurrDialog = m_UnitDialogs[dialogIdx];

                m_CurrDialog.SetActive(true);
                RectTransform rt = m_CurrDialog.GetComponent<RectTransform>();
                rt.anchoredPosition = pos/*+ m_DialogOffset[dialogIdx]*/;
/*                Debuger.Log(rt.anchoredPosition);*/
                Text textComp = GetText(m_CurrDialog, "text");
                LayoutElement le = textComp.GetComponent<LayoutElement>();
                le.preferredWidth = 20 * cmd.line_count;
                
                GetText(m_CurrDialog, "name").text = cmd.target_name;
                GetText(m_CurrDialog, "text").text = cmd.text;

                if (icon != "")
                {
                        ResourceManager.GetInst().LoadIconSpriteSyn(icon, GetImage(m_CurrDialog, "icon").transform);                        
                }
                else
                {
                        GetImage(m_CurrDialog, "icon").enabled = false;
                }
                bool bHasSelection = SetSelections(cmd.option0, cmd.option1, cmd.option2);
                GetImage(m_CurrDialog, "next").gameObject.SetActive(!bHasSelection);
                if (cmd.isWait)
                {
                        m_bNeedWait = true;
                }
        }

        public void SetAsideDialog(CutSceneCmd_Dialog cmd)
        {
                m_CenterTextBg.SetActive(false);
                m_AsideDialog.SetActive(true);
                m_CurrDialog = m_AsideDialog;
                GetText(m_CurrDialog, "text").text = cmd.text;
                bool bHasSelection = SetSelections(cmd.option0, cmd.option1, cmd.option2);
                GetImage(m_AsideDialog, "next").gameObject.SetActive(!bHasSelection);
                m_bNeedWait = true;
        }

        public bool SetSelections(string option0, string option1, string option2)
        {
                if (string.IsNullOrEmpty(option0) && string.IsNullOrEmpty(option1) && string.IsNullOrEmpty(option2))
                {
                        m_SelectionGroup.SetActive(false);
                        return false;
                }
                m_SelectionGroup.SetActive(true);

                GetButton(m_SelectionGroup, "selection0").gameObject.SetActive(option0 != "");
                GetText(m_SelectionGroup, "selection0_text").text = option0;

                GetButton(m_SelectionGroup, "selection1").gameObject.SetActive(option1 != "");
                GetText(m_SelectionGroup, "selection1_text").text = option1;

                GetButton(m_SelectionGroup, "selection2").gameObject.SetActive(option2 != "");
                GetText(m_SelectionGroup, "selection2_text").text = option2;
                return true;
        }
        void Update()
        {
                if (m_SelectionGroup.activeSelf)
                        return;

                if (m_bNeedWait)
                {
                        if (InputManager.GetInst().GetInputUp(true))
                        {
                                OnClickNext();
                        }
                }
        }

        public Image m_OriImage;
        
        public void ShowImage(string name, Vector2 startpos, Vector2 endpos, float show_time, float exist_time)
        {
                m_CenterTextBg.SetActive(false);
                GameObject imageObj = CloneElement(m_OriImage.gameObject, name);
                ResourceManager.GetInst().LoadIconSpriteSyn(name, imageObj.transform);
                imageObj.GetComponent<Image>().SetNativeSize();

                RectTransform trans = imageObj.GetComponent<RectTransform>();
                trans.anchoredPosition = new Vector2(startpos.x, startpos.y);
                DOTween.To(() => { return trans.anchoredPosition; }, v => { trans.anchoredPosition = v; }, endpos, show_time);
                GameObject.Destroy(imageObj, show_time+ exist_time);
        }
        public void ShowCenterText(CutSceneCmd_Dialog cmd)
        {
/*                Debuger.Log(cmd.text + " " + cmd.isWait + " " + cmd.time);*/
                m_CenterTextBg.SetActive(true);
                m_CenterText.text = cmd.text;
                
                if (cmd.time > 0)
                {
                        Invoke("OnClickNext", cmd.time);
                        CutSceneManager.GetInst().SetWaitTime(cmd.time);
                }
                else
                {
                        m_bNeedWait = true;
                }
        }
}
