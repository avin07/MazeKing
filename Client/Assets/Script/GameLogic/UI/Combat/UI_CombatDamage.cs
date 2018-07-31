//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using System.Collections;
//using System.Collections.Generic;
//using DG.Tweening;

//public class UI_CombatDamage : UIBehaviour
//{
//        public Canvas m_Canvas;
//        public RectTransform m_BaseRT;
//        public Transform m_BelongTransform;
//        Vector3 m_WorldPos;

//        public Image m_Num0;
//        public GameObject m_NumberRoot;

//        float m_fTime;
//        float m_fMaxTime;
//        float m_fShowTime;
//        float m_fDelay;
//        float m_fOffsetY;

//        void Awake()
//        {
//                m_Num0.gameObject.SetActive(false);
//        }
//        const float OFFSETY = 0.003f;
//        List<Image> m_NumList = new List<Image>();
//        public void ShowNum(SkillResultData skillResult, float time, Vector3 worldPos)
//        {
//                m_WorldPos = worldPos;

//                if (Camera.main != null)
//                {
//                        m_BaseRT.anchoredPosition = UIUtility.WorldToCanvas(m_Canvas, m_WorldPos);
//                }
//                m_NumList.Clear();

//                string prefix = "";
//                switch (skillResult.type)
//                {
//                        case SkillResultType.Damage:
//                                {
//                                        if (skillResult.isCritical)
//                                        {
//                                                SetSpecIcon("baoji");
//                                        }

//                                        prefix = skillResult.isCritical ? "big_0" : "0";
//                                        m_fOffsetY = OFFSETY;
//                                }
//                                break;
//                        case SkillResultType.Heal:
//                                {
//                                        prefix = "1";
//                                        m_fOffsetY = OFFSETY;
//                                }
//                                break;
//                        case SkillResultType.AddPressure:
//                                {
//                                        prefix = "2";
//                                        m_fOffsetY = -OFFSETY;
//                                }
//                                break;
//                        case SkillResultType.MinusPressure:
//                                {
//                                        prefix = "3";
//                                        m_fOffsetY = OFFSETY;
//                                }
//                                break;
//                        case SkillResultType.Dodge:
//                                {
//                                        SetSpecIcon("shanbi");
//                                }
//                                break;
//                        case SkillResultType.Miss:
//                                {
//                                        SetSpecIcon("weimingzhong");
//                                }
//                                break;
//                }

//                if (skillResult.isParry)
//                {
//                        SetSpecIcon("gedang");
//                }

//                if (skillResult.nValue > 0)
//                {
//                        SetNumImage(skillResult.nValue.ToString(), prefix);
//                }
//                m_fTime = Time.realtimeSinceStartup;
//                m_fMaxTime = time;
//                m_fShowTime = time / 4f;
//        }
//        void SetSpecIcon(string specIcon)
//        {
//                GameObject newObj = CloneElement(m_Num0.gameObject, specIcon);
//                newObj.transform.SetParent(m_NumberRoot.transform);
//                newObj.SetActive(true);
//                Debug.Log("SetSpecIcon " + newObj.name);
//                ResourceManager.GetInst().LoadIconSpriteSyn("Number#" + specIcon, newObj.transform);

//                Image im = newObj.GetComponent<Image>();
//                im.rectTransform.localScale = Vector3.one;
//                m_NumList.Add(im);
//                StartCoroutine(UpdateImage(im));
//        }

//        void SetNumImage(string str, string prefix)
//        {
//                int idx = 0;
//                foreach (char c in str)
//                {
//                        GameObject newObj = CloneElement(m_Num0.gameObject, "num" + idx);
//                        newObj.transform.SetParent(m_NumberRoot.transform);
//                        newObj.SetActive(true);

//                        string path = "Number#" + prefix + c;
//                        ResourceManager.GetInst().LoadIconSpriteSyn(path, newObj.transform);

//                        Image im = newObj.GetComponent<Image>();
//                        im.rectTransform.localScale = Vector3.one;
//                        m_NumList.Add(im);
//                        StartCoroutine(UpdateImage(im));
//                        idx++;
//                }
//                StartCoroutine(UpdateScale());
//        }
//        IEnumerator UpdateScale()
//        {
//                yield return null;
//                m_BaseRT.DOScale(Vector3.one * 1.5f, m_fShowTime / 2);
//                yield return new WaitForSeconds(m_fShowTime / 2);
//                m_BaseRT.DOScale(Vector3.one * 1f, m_fShowTime / 2);
//                yield return new WaitForSeconds(m_fShowTime / 2);
//        }

//        IEnumerator UpdateImage(Image im)
//        {

//                yield return new WaitForSeconds(m_fShowTime);
//                DOTween.To(() =>
//                {
//                        return im.color;
//                }, v =>
//                {
//                        im.color = v;
//                }, new Color(im.color.r, im.color.g, im.color.b, 0f), m_fMaxTime - m_fShowTime);
//                yield return new WaitForSeconds(m_fMaxTime - m_fShowTime);
//                GameObject.Destroy(this.gameObject);
//        }

//         void Update()
//         {
//                if (Camera.main != null)
//                {
//                        m_BaseRT.anchoredPosition = UIUtility.WorldToCanvas(m_Canvas, m_WorldPos);
//                }
//        }
//}
