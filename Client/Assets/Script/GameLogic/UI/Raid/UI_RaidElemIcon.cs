using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class UI_RaidElemIcon : UIBehaviour 
{
        public Image m_Icon;
        public Button m_Button;
        public GameObject m_BaseGroup;
        public GameObject m_Loading;
        public Image m_LoadingWrong;
        public Image m_LoadingRight;
        Text m_IconName;
        public bool IsCheckDist = true;
        public Canvas m_Canvas;
        public RectTransform m_BaseRT;
        Vector3 m_WorldPos;
        RaidManager.OpFinishHandler m_FinishHandler;
        object m_HandlerData;
        RaidNodeBehav m_BelongNode;

        public Image m_OutRangeIcon;

        void Awake()
        {
                m_Loading.SetActive(false);
                m_IconName = GetText("iconname");
                m_OutRangeIcon = GetImage("icon1");
        }

        public void StartLoading(float time, bool bRight/*, RaidManager.OpFinishHandler handler, object data*/)
        {
                m_Loading.SetActive(true);
                m_LoadingRight.gameObject.SetActive(bRight);
                m_LoadingWrong.gameObject.SetActive(!bRight);
//                 m_FinishHandler = handler;
//                 m_HandlerData = data;
                StartCoroutine(ProcessLoading(time, bRight ? m_LoadingRight : m_LoadingWrong));

                TweenColor tc = m_Button.gameObject.GetComponent<TweenColor>();
                if (tc == null)
                {
                        tc = m_Button.gameObject.AddComponent<TweenColor>();
                }
                tc.from = m_Button.gameObject.GetComponent<Image>().color;
                tc.to = new Color(1f, 193f / 255f, 25f / 255f);
                tc.style = TweenMain.Style.PingPong;
                tc.duration = time / 4f;

                TweenColor tc2 = m_Icon.GetComponent<TweenColor>();
                if (tc2 == null)
                {
                        tc2 = m_Icon.gameObject.AddComponent<TweenColor>();
                }
                tc2.from = m_Icon.GetComponent<Image>().color;
                tc2.from.a = 0f;
                tc2.to = new Color(1f, 193f / 255f, 25f / 255f);
                tc2.style = TweenMain.Style.PingPong;
                tc2.duration = time / 4f;

                if (bRight == false)
                {
                        TweenScale ts = m_Button.GetComponent<TweenScale>();
                        if (ts == null)
                        {
                                ts = m_Button.gameObject.AddComponent<TweenScale>();
                        }
                        ts.to = new Vector3(1.2f, 1.2f, 1.2f);
                        ts.style = TweenMain.Style.PingPong;
                        ts.duration = time / 8f;
                }
        }
        public void StopLoading()
        {
                TweenColor tc = m_Button.gameObject.GetComponent<TweenColor>();
                if (tc != null)
                {
                        tc.from.a = 1f;
                        m_Button.GetComponent<Image>().color = tc.from;
                }

                TweenColor tc2 = m_Icon.gameObject.GetComponent<TweenColor>();
                if (tc2 != null)
                {
                        tc2.from.a = 1f;
                        m_Icon.color = tc2.from;
                }

                GameObject.Destroy(tc);
                GameObject.Destroy(tc2);

                TweenScale ts = m_Button.GetComponent<TweenScale>();
                if (ts != null)
                {
                        m_Button.GetComponent<RectTransform>().localScale = ts.from;
                }
                GameObject.Destroy(ts);
        }

        IEnumerator ProcessLoading(float maxtime, Image loadingIm)
        {
                float time = Time.realtimeSinceStartup;
                loadingIm.fillAmount = 0;
                while (Time.realtimeSinceStartup - time < maxtime && RaidManager.GetInst().GetTargetNode() == this.m_BelongNode)
                {
                        loadingIm.fillAmount = (Time.realtimeSinceStartup - time) / maxtime;
                        yield return null;
                }
                loadingIm.fillAmount = 1;
                StopLoading();
                m_Loading.SetActive(false);
//                 if (RaidManager.GetInst().GetTargetNode() == this.m_BelongNode)
//                 {
//                         if (m_FinishHandler != null)
//                         {
//                                 m_FinishHandler(m_HandlerData);
//                         }
//                 }
        }

        public void Setup(RaidNodeBehav node)
        {
                m_BelongNode = node;
                if (node.elemCfg != null)
                {
                        if (!string.IsNullOrEmpty(node.elemCfg.operation_icon))
                        {
                                ResourceManager.GetInst().LoadIconSpriteSyn(node.elemCfg.operation_icon, m_Icon.transform);
                        }
                        else
                        {
                                m_Icon.gameObject.SetActive(false);
                        }
                        ResourceManager.GetInst().LoadIconSpriteSyn("Operation#shijuwai", m_OutRangeIcon.transform);
                        if (node.CanUnlockBuild())
                        {
                                m_IconName.text = LanguageManager.GetText("unlock_building_element_name");
                        }
                        else
                        {
                                m_IconName.text = LanguageManager.GetText(node.elemCfg.name);
                        }
                }


                if (m_BelongNode == null || m_BelongNode.gameObject.activeSelf == false)
                {
                        m_BaseGroup.SetActive(false);
                }

                m_WorldPos = node.GetCenterPosition() + Vector3.up * m_BelongNode.elemCfg.icon_height;
        }
        void Update()
        {
                if (IsCheckDist == false || m_BaseGroup == null)
                        return;
       
                if (m_BelongNode == null || m_BelongNode.gameObject.activeSelf == false)
                {
                        return;
                }

                if (GameStateManager.GetInst().GameState != GAMESTATE.RAID_PLAYING)
                {
                        m_BaseGroup.SetActive(false);
                        return;
                }
                
                if (Camera.main != null)
                {
                        m_BaseRT.anchoredPosition = UIUtility.WorldToCanvas(m_Canvas, m_WorldPos);
                }

                if (RaidTeamManager.GetInst().GetTeamBright() < m_BelongNode.elemCfg.brightness_limit)
                {
                        m_OutRangeIcon.gameObject.SetActive(false);
                        m_Button.gameObject.SetActive(false);
                        return;
                }

                if (RaidManager.GetInst().IsInFocusRoom(this.m_BelongNode))
                {
                        m_BaseGroup.SetActive(true);
                        m_OutRangeIcon.gameObject.SetActive(false);
                        m_Button.gameObject.SetActive(true);
                }
                else
                {
                        m_BaseGroup.SetActive(true);
                        m_OutRangeIcon.gameObject.SetActive(m_BelongNode.belongRoom.IsActive);
                        m_Button.gameObject.SetActive(false);
                }
        }
        public void OnClickButton()
        {
                return;
                RaidManager.GetInst().SelectTargetNode(m_BelongNode);
        }
}
