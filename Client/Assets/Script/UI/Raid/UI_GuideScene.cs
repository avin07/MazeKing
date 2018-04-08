using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class UI_GuideScene : UIBehaviour 
{
        public GameObject m_BaseGroup;
        public Canvas m_Canvas;
        public RectTransform m_BaseRT;
        Vector3 m_WorldPos;
        RaidManager.OpFinishHandler m_FinishHandler;
        object m_HandlerData;
        RaidNodeBehav m_BelongNode;
        GuideConfig m_GuideCfg;
        RectTransform arrowRt;
        RectTransform msgRt;

        void Awake()
        {
                arrowRt = GetGameObject("arrow").GetComponent<RectTransform>();
                msgRt = GetGameObject("msg").GetComponent<RectTransform>();
                msgRt.SetActive(false);
        }

        public int GetGuideId()
        {
                if (m_GuideCfg != null)
                {
                        return m_GuideCfg.id;
                }
                return 0;
        }

        GameObject m_BelongNpc;
        public void SetupNpc(GameObject npcObj, GuideConfig cfg)
        {
                m_GuideCfg = cfg;
                m_BelongNpc = npcObj;
                m_WorldPos = npcObj.transform.position + Vector3.up * 3f;
                SetText(cfg);
        }
        void SetText(GuideConfig cfg)
        {
                Text msgText = GetText("Text");
                if (cfg != null)
                {
                        msgText.text = LanguageManager.GetText(cfg.operate_prompt);
                }
                LayoutElement le = msgText.GetComponent<LayoutElement>();
                le.preferredWidth = msgText.text.Length * 20 + 40;
        }
        public void Setup(RaidNodeBehav node,GuideConfig cfg)
        {
                m_GuideCfg = cfg;
                m_BelongNode = node;
                m_BelongNode.GuideUI = this;
                SetText(cfg);
                if (m_BelongNode == null || m_BelongNode.gameObject.activeSelf == false)
                {
                        m_BaseGroup.SetActive(false);
                }

                m_WorldPos = node.GetCenterPosition() + Vector3.up * 3f;
        }
        void Update()
        {
                if (m_BaseGroup == null)
                        return;
                                
                if (Camera.main != null)
                {
                        if (m_BelongNpc != null)
                        {
                                m_WorldPos = m_BelongNpc.transform.position + Vector3.up * 5f;
                        }
                        m_BaseRT.anchoredPosition = UIUtility.WorldToCanvas(m_Canvas, m_WorldPos);
                }

                if (m_BelongNode != null)
                {
                        if (RaidManager.GetInst().IsInFocusRoom(this.m_BelongNode))
                        {
                                m_BaseGroup.SetActive(true);
                        }
                        else
                        {
                                m_BaseGroup.SetActive(true);
                        }
                }
        }
        public void OnClickNode()
        {
                if (m_GuideCfg.next > 0)
                {
                        GuideManager.GetInst().GotoNextGuide(m_GuideCfg.next, m_GuideCfg.id);
                }
                else
                {
                        GuideManager.GetInst().SaveGuide(m_GuideCfg.id);
                }
        }
        void OnDestroy()
        {
                UI_Guide uis = UIManager.GetInst().GetUIBehaviour<UI_Guide>();
                if (uis != null)
                {
                        if (m_GuideCfg != null)
                        {
                                if (m_GuideCfg.next <= 0)
                                {
                                        uis.CloseGuide(m_GuideCfg.id);
                                }
                        }
                }
        }
}
