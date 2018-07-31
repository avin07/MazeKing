//using UnityEngine;
//using System.Collections;
//using UnityEngine.UI;
//using System.Collections.Generic;
//using DG.Tweening;
//using UnityEngine.EventSystems;

//public class UI_BossStatus : UIBehaviour
//{
//        public GameObject m_HpObj;
//        public GameObject m_BuffIcon0;
//        public GameObject m_Skill0;


//        Image m_HpImage;
//	void Awake()
//	{
//                m_HpImage = GetImage(m_HpObj, "hp_white");
//                m_BuffIcon0.SetActive(false);
//                m_Skill0.SetActive(false);
//        }

//        public void ShowPassiveSkill(int passiveSkillId)
//        {
//        }
//        public void ShowActiveSkill(string name)
//        {
//        }
//        public void SetHp(int hp, int maxHp, bool bNeedAnim = false)
//        {
//                GetImage(m_HpObj, "hp_red").fillAmount = (float)hp / maxHp;
//                if (bNeedAnim)
//                {
//                        DOTween.To(() =>
//                        {
//                                return m_HpImage.fillAmount;
//                        }, v =>
//                        {
//                                m_HpImage.fillAmount = v;
//                        }, (float)hp / maxHp, 1f);
//                }
//                else
//                {
//                        m_HpImage.fillAmount = (float)hp / maxHp;
//                }
//                GetText("hpval").text = hp.ToString() + "/" + maxHp.ToString();
//        }

//        public void Talk(string text, float time = 2f)
//        {

//        }

//        #region BUFF
//        public void ShowGetBuff(string buffname, float time = 2f)
//        {
//        }
//        Dictionary<int, GameObject> m_BuffDict = new Dictionary<int, GameObject>();
//        public void AddBuff(FighterBuffBehav buff)
//        {                
//                SkillBuffConfig cfg = SkillManager.GetInst().GetBuff(buff.m_BuffCfg.id);
//                if (!string.IsNullOrEmpty(cfg.icon))
//                {
//                        GameObject obj = CloneElement(m_BuffIcon0, "bufficon" + m_BuffDict.Count);
//                        EventTriggerListener.Get(obj).onClick = OnClickBuff;
//                        EventTriggerListener.Get(obj).SetTag(buff.m_BuffCfg);
//                        GetImage(obj, "icon").enabled = true;
//                        if (buff.LeftRound >= 99)
//                        {
//                                GetText(obj, "count").text = "";
//                        }
//                        else
//                        {
//                                GetText(obj, "count").text = buff.LeftRound.ToString();
//                        }
//                        ResourceManager.GetInst().LoadIconSpriteSyn(cfg.icon, GetImage(obj, "icon").transform);
//                        if (!m_BuffDict.ContainsKey(buff.GetInstanceID()))
//                        {
//                                m_BuffDict.Add(buff.GetInstanceID(), obj);
//                        }
//                }
//        }
//        public void DelBuff(FighterBuffBehav buff)
//        {
//                if (m_BuffDict.ContainsKey(buff.GetInstanceID()))
//                {
//                        GameObject.Destroy(m_BuffDict[buff.GetInstanceID()]);
//                }
//        }
//        public void OnClickBuff(GameObject go, PointerEventData data)
//        {
//                Debug.Log(go.GetComponent<RectTransform>().position + " " + go.GetComponent<RectTransform>().anchoredPosition);
//                UI_SkillTips uis = UIManager.GetInst().GetUIBehaviour<UI_SkillTips>();
//                if (uis == null)
//                {
//                        uis = UIManager.GetInst().ShowUI<UI_SkillTips>();
//                }
//                SkillBuffConfig cfg = EventTriggerListener.Get(go).GetTag() as SkillBuffConfig;
//                uis.SetBuffTip(cfg, CalcScreenPosition(go.GetComponent<RectTransform>()));
//        }
//        public void UpdateBuffCount(FighterBuffBehav buff)
//        {
//                int buffInstId = buff.GetInstanceID();
//                if (m_BuffDict.ContainsKey(buffInstId))
//                {
//                        GetText(m_BuffDict[buffInstId], "count").text = buff.LeftRound >= 99 ? "" : buff.LeftRound.ToString();
//                }
//        }
//        #endregion

//        #region SKILL

//        FighterBehav m_BossBehav;
//        public void SetupSkills(FighterBehav behav)
//        {
//                m_BossBehav = behav;
//                List<int>skillList = new List<int>(behav.FighterProp.GetActiveSkillList());
//                int activeSkillCount = skillList.Count;        //计算主动技能数目
//                skillList.AddRange(behav.FighterProp.CharacterCfg.passive_skill);

//                for(int idx = 0; idx < skillList.Count; idx++)
//                {
//                        int skillId = skillList[idx];
//                        GameObject skillBtn = CloneElement(m_Skill0, "skill" + idx);
//                        EventTriggerListener.Get(skillBtn).onClick = OnClickSkill;
//                        EventTriggerListener.Get(skillBtn).SetTag(skillId);

//                        Image im = GetImage(skillBtn, "skillicon");
//                        im.enabled = true;
//                        ResourceManager.GetInst().LoadIconSpriteSyn(SkillManager.GetInst().GetSkillIconUrl(skillId), im.transform);

//                        if (idx< activeSkillCount)
//                        {
//                                SkillConfig cfg = SkillManager.GetInst().GetActiveSkill(skillId);
//                                if (cfg != null)
//                                {
////                                         int cdval = behav.GetSkillCD(cfg.id);
////                                         GetText(skillBtn, "skillcd").text = cdval > 0 ? cdval.ToString() : "";
////                                         bool bAvail = IsSkillAvail(behav, cfg);
////                                         im.color = bAvail ? Color.white : Color.gray;
//                                }
//                        }
//                        else
//                        {
//                                //GetText(skillBtn, "skillcd").text = "";
//                                im.color = Color.gray;
//                                //UIUtility.SetUIEffect(this.name, skillBtn, true, "effect_battle_passive_skill");
//                        }
//                }
                
//                string icon = ModelResourceManager.GetInst().GetIconRes(m_BossBehav.CharacterCfg.modelid);
//                ResourceManager.GetInst().LoadIconSpriteSyn(icon, GetImage("bossicon").transform);
//                GetText("bosslevel").text = m_BossBehav.FighterProp.Level.ToString();                
//        }

//        public void OnClickSkill(GameObject go, PointerEventData data)
//        {
//                int skillId = (int)EventTriggerListener.Get(go).GetTag();
//                {
//                        UI_SkillTips uis = UIManager.GetInst().GetUIBehaviour<UI_SkillTips>();
//                        if (uis == null)
//                        {
//                                uis = UIManager.GetInst().ShowUI<UI_SkillTips>();
//                        }
//                        uis.ShowSkillTip(skillId, go.transform.position, m_BossBehav.FighterProp);
//                }
//        }

//        #endregion
//}