// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.EventSystems;
// using System.Collections;
// using System.Collections.Generic;

//public class UI_CombatSkill : UIBehaviour
//{
//        List<int> m_SkillList = new List<int>();
//        public List<GameObject> m_BtnList = new List<GameObject>();
//        int m_nActiveSkillCount = 0;
//        int m_nSkillIdx = -1;

//        GameObject m_SkillGroup;
//        Toggle m_BtnAuto;
//        FighterBehav m_BelongFighter;
//        public int SkillIndex
//        {
//                get { return m_nSkillIdx; }
//        }

//        GameObject m_AutoUIEffect;

//        void Awake()
//        {
//                int idx = 0;
//                foreach (GameObject skillBtn in m_BtnList)
//                {
//                        EventTriggerListener listener = EventTriggerListener.Get(skillBtn);
//                        listener.onClick = OnClickButton;
//                        listener.onDown = OnDown;
//                        listener.onUp = OnUp;
//                        listener.SetTag(idx);
//                        idx++;
//                        //skillBtn.SetActive(false);
//                }
                
//                m_SkillGroup = GetGameObject("SkillGroup");
//                ShowSkillGroup(false);
//                m_BtnAuto = GetToggle("BtnAuto");
//                EventTriggerListener.Get(m_BtnAuto.gameObject).onClick = OnClickBtnAuto;
//        }

//        void Start()
//        {
//                if (!PlayerController.GetInst().IsGuideFinish(3))
//                {
//                        m_BtnAuto.gameObject.SetActive(false);
//                        GetGameObject("BtnSkip").SetActive(false);
//                }
//        }

//        public void ShowSkillGroup(bool bVisible)
//        {
//                m_SkillGroup.SetActive(bVisible);
//                if (bVisible == false)
//                {
//                        UIManager.GetInst().CloseUI("UI_SkillTips");
//                }

//        }
//        public void SetBtnAuto(bool bOn)
//        {
//                m_BtnAuto.isOn = bOn;

//                GetGameObject("BtnSkip").SetActive(!bOn);
//                UIUtility.SetUIEffect(this.name, m_BtnAuto.gameObject, m_BtnAuto.isOn, "effect_battle_automatic_button");
//        }
//        void OnClickBtnAuto(GameObject go, PointerEventData data)
//        {                
//                CombatManager.GetInst().SaveAutoState(m_BtnAuto.isOn);
//                GetGameObject("BtnSkip").SetActive(!m_BtnAuto.isOn);
//                UIUtility.SetUIEffect(this.name, m_BtnAuto.gameObject, m_BtnAuto.isOn, "effect_battle_automatic_button");
//        }

//        public int GetSelectSkillID()
//        {
//                if (m_SkillList.Count > m_nSkillIdx)
//                {
//                        return m_SkillList[m_nSkillIdx];
//                }
//                return 0;
//        }

//        bool IsActiveSkill(int idx)
//        {
//                return idx < m_nActiveSkillCount;
//        }

//        bool IsSkillAvail(FighterBehav behav, SkillConfig cfg)
//        {
//                if (cfg == null)
//                        return false;

//                if (behav.IsSilence() && cfg.is_silence == 1)
//                {
//                        return false;
//                }
                
//                if (behav.GetSkillCD(cfg.id) > 0)
//                {
//                        return false;
//                }
//                return true;
//        }
//        public void SetupSkillIcons(FighterBehav behav)
//        {
//                m_BelongFighter = behav;
//                foreach (GameObject skillBtn in m_BtnList)
//                {
//                        skillBtn.SetActive(false);
//                        UIUtility.CleanUIEffect(this.name, skillBtn.name);
//                }
//                m_SkillList = new List<int>(behav.FighterProp.GetActiveSkillList());
//                m_nActiveSkillCount = m_SkillList.Count;        //计算主动技能数目
//                m_SkillList.AddRange(behav.FighterProp.CharacterCfg.passive_skill);
//                int idx = 0;
//                foreach (int skillId in m_SkillList)
//                {
//                        if (idx < m_BtnList.Count)
//                        {
//                                GameObject skillBtn = m_BtnList[idx];
//                                skillBtn.SetActive(true);

//                                Image im = GetImage(skillBtn, "skillicon");
//                                im.enabled = true;
//                                ResourceManager.GetInst().LoadIconSpriteSyn(SkillManager.GetInst().GetSkillIconUrl(skillId), im.transform);

//                                if (IsActiveSkill(idx))
//                                {
//                                        SkillConfig cfg = SkillManager.GetInst().GetActiveSkill(skillId);
//                                        if (cfg != null)
//                                        {
//                                                int cdval = behav.GetSkillCD(cfg.id);
//                                                GetText(skillBtn, "skillcd").text = cdval > 0 ? cdval.ToString() : "";
//                                                bool bAvail = IsSkillAvail(behav, cfg);
//                                                im.color = bAvail ? Color.white : Color.gray;
//                                        }
//                                }
//                                else
//                                {
//                                        GetText(skillBtn, "skillcd").text = "";
//                                        im.color = Color.gray;
//                                        UIUtility.SetUIEffect(this.name, skillBtn, true, "effect_battle_passive_skill");
//                                }
//                                idx++;
//                        }
//                }
//        }

//        public void UpdateSkillCD(int skill, int cd)
//        {
//                for (int i = 0; i < m_SkillList.Count; i++)
//                {
//                        if (i < m_BtnList.Count)
//                        {
//                                if (m_SkillList[i] == skill)
//                                {
//                                        GetText(m_BtnList[i], "skillcd").text = cd > 0 ? cd.ToString() : "";
//                                }
//                        }
//                }
//        }

//        public void OnDown(GameObject go, PointerEventData data)
//        {
//                SetTipGroup((int)EventTriggerListener.Get(go).GetTag());

//        }
//        public void OnUp(GameObject go, PointerEventData data)
//        {
//                SetTipGroup(m_nSkillIdx);
//        }

//        public void OnClickButton(GameObject go, PointerEventData data)
//        {
//                if (CombatManager.GetInst().CanInput() == false)
//                {
//                        return;
//                }
//                SelectSkillBtn(go);
//                SetTipGroup(m_nSkillIdx);
//        }

//        void SetTipGroup(int skillIdx)
//        {
//                if (skillIdx < m_SkillList.Count)
//                {
//                        UI_SkillTips uis = UIManager.GetInst().GetUIBehaviour<UI_SkillTips>();
//                        if (uis == null)
//                        {
//                                uis = UIManager.GetInst().ShowUI<UI_SkillTips>();
//                        }

//                        Vector3 screenPos = CalcScreenPosition(m_BtnList[skillIdx].GetComponent<RectTransform>());
//                        screenPos.x += 40f;
//                        screenPos.y += 40f;
//                        uis.ShowSkillTip(m_SkillList[skillIdx], screenPos, m_BelongFighter.FighterProp);
//                }
//        }

//        void SelectSkillBtn(GameObject go)
//        {
//                int idx = (int)EventTriggerListener.Get(go).GetTag();
//                if (idx == m_nSkillIdx)
//                        return;

//                if (IsActiveSkill(idx))
//                {
//                        if (IsSkillAvail(m_BelongFighter, SkillManager.GetInst().GetActiveSkill(m_SkillList[idx])))
//                        {
//                                if (m_nSkillIdx >= 0 && m_nSkillIdx < m_BtnList.Count)
//                                {
//                                        UIUtility.CleanUIEffect(this.name, GetImage(go, "skillicon").gameObject.name);
//                                }
//                                m_nSkillIdx = idx;
//                                UIUtility.SetUIEffect(this.name, GetImage(go, "skillicon").gameObject, true, "effect_battle_choose_skill");
//                                CombatManager.GetInst().SetTeamSelectIcon(GetSelectSkillID());
//                        }
//                }

//        }

//        public void SelectFirstAvailbleSkill()
//        {
//                if (m_BtnList != null && m_BtnList.Count > 0)
//                {
//                        bool m_bSelect = false;

//                        int idx = 0;
//                        foreach (GameObject skillBtn in m_BtnList)
//                        {
//                                Toggle toggle = skillBtn.GetComponent<Toggle>();
//                                toggle.isOn = false;
//                                if (m_bSelect == false && IsActiveSkill(idx))
//                                {
//                                        toggle.isOn = true;
//                                        m_bSelect = true;

//                                        EventSystem.current.SetSelectedGameObject(skillBtn);
//                                        SelectSkillBtn(skillBtn);
//                                        //m_nSkillIdx = (int)EventTriggerListener.Get(skillBtn).GetTag();

//                                        CombatManager.GetInst().SetTeamSelectIcon(GetSelectSkillID());
//                                }
//                                idx++;
//                        }
//                }
//        }
//        public void OnClickSkip()
//        {
//                CombatManager.GetInst().ConfirmInput(null);
//        }

//        void Update()
//        {
//                if (InputManager.GetInst().GetInputUp(false))
//                {
//                        UIManager.GetInst().CloseUI("UI_SkillTips");
//                }
//        }
//}