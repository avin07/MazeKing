using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

class UI_UnitStatus : UIBehaviour 
{
        public Canvas m_Canvas;
        public RectTransform m_BaseRT;
        public Transform m_BelongTransform;
        
        Vector3 m_WorldPos;
        
        public Image m_EnemyHp;
        public Image m_MyHp;
        public Image m_EnemyHpBg;
        public Image m_MyHpBg;

        public Image m_PassiveSkillIcon;
        public GameObject m_ActiveSkillObj;
        public Text m_ActiveSkillText;

        Image m_CurrHp;
        Text m_TalkText;
        Text m_BuffName;
        public GameObject m_BuffIcon0;
        List<GameObject> m_BuffIconList = new List<GameObject>();

        List<Image> m_Pressure = new List<Image>();

        public GameObject m_PressureGroup;
        GameObject m_BuffGroup;
        GameObject GetItemObj(int idx)
        {
                GameObject obj = null;
                if (idx < m_BuffIconList.Count)
                {
                        obj = m_BuffIconList[idx];
                }
                else
                {
                        obj = CloneElement(m_BuffIcon0, "bufficon" + idx);
                        m_BuffIconList.Add(obj);
                }
                return obj;
        }

        void Awake()
        {
                m_BuffGroup = GetGameObject("buffgroup");
                m_BuffIcon0.gameObject.SetActive(false);
                m_BuffGroup.SetActive(false);
                for (int i = 0; i < 20; i++)
                {
                        Image im = GetImage("pressure" + i);
                        m_Pressure.Add(im);
                }
                m_PassiveSkillIcon.gameObject.SetActive(false);
                m_ActiveSkillObj.SetActive(false);

                m_TalkText = GetText("talk");
                m_TalkText.enabled = false;
                m_BuffName = GetText("buffname");
                m_BuffName.enabled = false;
        }
        
        public void SetEnemy(bool bEnemy)
        {
                if (bEnemy)
                {
                        m_MyHpBg.gameObject.SetActive(false);
                        m_EnemyHpBg.gameObject.SetActive(true);
                        m_CurrHp = m_EnemyHp;
                }
                else
                {
                        m_MyHpBg.gameObject.SetActive(true);
                        m_EnemyHpBg.gameObject.SetActive(false);
                        m_CurrHp = m_MyHp;
                }
                EnablePressure(bEnemy == false);
        }

        public void SetHp(int hp, int maxHp,bool bNeedAnim = false)
        {
                GetImage(m_CurrHp.gameObject, "hp").fillAmount = (float)hp / maxHp;
                if (bNeedAnim)
                {
                        DOTween.To(() =>
                      {
                              return m_CurrHp.fillAmount;
                      }, v =>
                      {
                              m_CurrHp.fillAmount = v;
                      }, (float)hp / maxHp, 1f);
                }
                else
                {
                        m_CurrHp.fillAmount = (float)hp / maxHp;
                }
        }

        public void SetPressure(int pressure)
        {
                int count = pressure / 10;
                for (int i = 0; i < m_Pressure.Count; i++)
                {
                        m_Pressure[i].enabled = i < count;
                }
        }
        public void EnablePressure(bool bEnable)
        {
                m_PressureGroup.SetActive(bEnable);
        }

        Dictionary<long, GameObject> m_IconDict = new Dictionary<long, GameObject>();
        /// <summary>
        /// HeroUnit里显示skillIcon
        /// </summary>
        public void AddSkill(int skillCfgId)
        {
                m_BuffGroup.SetActive(true);
                SkillLearnConfigHold cfg = SkillManager.GetInst().GetSkillInfo(skillCfgId);
                if (cfg != null)
                {
                        GameObject obj = CloneElement(m_BuffIcon0, "bufficon" + m_IconDict.Count);
                        GetImage(obj, "icon").enabled = true;
                        GetText(obj, "count").text = "";
                        ResourceManager.GetInst().LoadIconSpriteSyn(cfg.icon, GetImage(obj, "icon").transform);
                        if (!m_IconDict.ContainsKey(skillCfgId))
                        {
                                m_IconDict.Add(skillCfgId, obj);
                        }
                }
        }

//         /// <summary>
//         /// Fighter里这里显示bufficon
//         /// </summary>
//         /// <param name="buff"></param>
//         public void AddBuff(FighterBuffBehav buff)
//         {
//                 m_BuffGroup.SetActive(true);
//                 SkillBuffConfig cfg = SkillManager.GetInst().GetBuff(buff.m_BuffCfg.id);
//                 if (!string.IsNullOrEmpty(cfg.icon))
//                 {
//                         GameObject obj = CloneElement(m_BuffIcon0, "bufficon" + m_IconDict.Count);
//                         EventTriggerListener.Get(obj).onClick = OnClickBuff;
//                         EventTriggerListener.Get(obj).SetTag(buff.m_BuffCfg);
//                         GetImage(obj, "icon").enabled = true;
//                         if (buff.LeftRound >= 99)
//                         {
//                                 GetText(obj, "count").text = "";
//                         }
//                         else
//                         {
//                                 GetText(obj, "count").text = buff.LeftRound.ToString();
//                         }
//                         ResourceManager.GetInst().LoadIconSpriteSyn(cfg.icon, GetImage(obj, "icon").transform);                        
//                         if (!m_IconDict.ContainsKey(buff.GetInstanceID()))
//                         {
//                                 m_IconDict.Add(buff.GetInstanceID(), obj);
//                         }
//                 }
//                public void DelBuff(FighterBuffBehav buff)
//         {
//                 if (m_IconDict.ContainsKey(buff.GetInstanceID()))
//                 {
//                         GameObject.Destroy(m_IconDict[buff.GetInstanceID()]);
//                 }
        // public void OnClickBuff(GameObject go, PointerEventData data)
        //{
        //        UI_SkillTips uis = UIManager.GetInst().GetUIBehaviour<UI_SkillTips>();
        //        if (uis == null)
        //        {
        //                uis = UIManager.GetInst().ShowUI<UI_SkillTips>();
        //        }
        //        SkillBuffConfig cfg = EventTriggerListener.Get(go).GetTag() as SkillBuffConfig;
        //        Vector3 screenPos = CalcScreenPosition(go.GetComponent<RectTransform>());
        //        screenPos.y += 30f;
        //        uis.SetBuffTip(cfg,screenPos );
        //}

        //public void UpdateBuffCount(FighterBuffBehav buff)
        //{
        //        if (m_IconDict.ContainsKey(buff.GetInstanceID()))
        //        {
        //                if (buff.LeftRound >= 99)
        //                {
        //                        GetText(m_IconDict[buff.GetInstanceID()], "count").text = "";
        //                }
        //                else
        //                {
        //                        GetText(m_IconDict[buff.GetInstanceID()], "count").text = buff.LeftRound.ToString();
        //                }

        //        }
        //}

        void Update()
        {
                if (m_BaseRT != null && m_BelongTransform != null)
                {
                        m_WorldPos = m_BelongTransform.position + Vector3.up * 2.5f;
                        m_BaseRT.anchoredPosition = UIUtility.WorldToCanvas(m_Canvas, m_WorldPos);
                }
        }

        public void HideHpBar()
        {
                m_EnemyHpBg.gameObject.SetActive(false);
                m_MyHpBg.gameObject.SetActive(false);
                m_PressureGroup.SetActive(false);
        }

        public void ShowActiveSkill(string name)
        {
                m_ActiveSkillText.text = name;
                StartCoroutine(ProcessShowActiveSkill());
        }
        IEnumerator ProcessShowActiveSkill()
        {
                m_ActiveSkillObj.SetActive(true);
                yield return new WaitForSeconds(2f);
                m_ActiveSkillObj.SetActive(false);
        }
        public void ShowPassiveSkill(int passiveSkillId)
        {
                ResourceManager.GetInst().LoadIconSpriteSyn(SkillManager.GetInst().GetSkillIconUrl(passiveSkillId), m_PassiveSkillIcon.transform);

                StartCoroutine(ProcessShowPassiveSkill());
        }
        IEnumerator ProcessShowPassiveSkill()
        {
                m_PassiveSkillIcon.gameObject.SetActive(true);
                yield return new WaitForSeconds(2f);
                m_PassiveSkillIcon.gameObject.SetActive(false);
        }

        bool m_bTalking = false;
        public void Talk(string text, float time = 2f)
        {
                if (m_bTalking == false)
                {
                        StartCoroutine(ShowTalk(text, time));
                }
        }
        IEnumerator ShowTalk(string text, float time)
        {
                m_bTalking = true;
                m_TalkText.enabled = true;
                m_TalkText.text = text;
                yield return new WaitForSeconds(time);
                m_TalkText.enabled = false;
                m_TalkText.text = "";
                m_bTalking = false;
        }
        public void ShowGetBuff(string buffname, float time = 2f)
        {
                StartCoroutine(ShowBuffName(buffname, time));
        }
        IEnumerator ShowBuffName(string text, float time)
        {
                m_BuffName.enabled = true;
                m_BuffName.text = text;
                yield return new WaitForSeconds(time);
                m_BuffName.enabled = false;
                m_BuffName.text = "";
        }
}
