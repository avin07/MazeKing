using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class UI_MessageBox : UIBehaviour
{
        public GameObject [] MsgObjArray;
        List<Vector2> ori_pos = new List<Vector2>();
        List<GameObject> m_ShowMsg = new List<GameObject>();
        Queue<string> m_MsgInfo = new Queue<string>();

        void Awake()
        {
                this.UILevel = UI_LEVEL.TIP;
                for (int i = 0; i < MsgObjArray.Length; i++)
                {
                        ori_pos.Add((MsgObjArray[i].transform as RectTransform).anchoredPosition);
                }

        }

        public void SetText(string text)
        {
                if (m_ShowMsg.Count < MsgObjArray.Length)
                {
                        GameObject now_show_obj = GetFreeOne();
                        if (now_show_obj != null)
                        {
                                m_ShowMsg.Add(now_show_obj);
                                (now_show_obj.transform as RectTransform).anchoredPosition = ori_pos[0];
                                now_show_obj.SetActive(true);
                                GetText(now_show_obj, "Text").text = text;


                                CanvasGroup cg = now_show_obj.GetComponent<CanvasGroup>();
                                cg.DOFade(0.3f, 1.5f).OnComplete(() => OnFinish(cg));
                                now_show_obj.transform.DOScale(1.1f * Vector3.one, 0.3f);

                                for (int i = 0; i < m_ShowMsg.Count; i++)
                                {
                                        if (i != m_ShowMsg.Count - 1)
                                        {
                                                int index = m_ShowMsg.Count - i - 1;
                                                (m_ShowMsg[i].transform as RectTransform).DOAnchorPos(ori_pos[index], 0.5f);
                                                m_ShowMsg[i].transform.DOScale(Vector3.one, 0.5f);
                                        }
                                }
                        }

                }
                else
                {
                        m_MsgInfo.Enqueue(text);
                }

        }

        GameObject GetFreeOne()
        {
                for (int i = 0; i < MsgObjArray.Length; i++)
                {
                        if (!m_ShowMsg.Contains(MsgObjArray[i]))
                        {
                                return MsgObjArray[i];
                        }
                }
                return null; 
        }


        void OnFinish(CanvasGroup cg)
        {
              
                m_ShowMsg.Remove(cg.gameObject);
                cg.gameObject.SetActive(false);
                cg.gameObject.transform.DOKill();
                cg.alpha = 1;
                cg.gameObject.transform.localScale = Vector3.one;

                if (m_ShowMsg.Count < MsgObjArray.Length)
                {
                        if (m_MsgInfo.Count > 0)
                        {
                               SetText(m_MsgInfo.Dequeue());
                        }
                }

                if (m_ShowMsg.Count <= 0)
                {
                        gameObject.SetActive(false);
                }
        }



}

