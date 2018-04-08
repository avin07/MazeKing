using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_CampSkill : UIBehaviour
{
        public GameObject m_SkillGroup;
        public List<Toggle> m_SkillToggleList = new List<Toggle>();// m_Skill0, m_Skill1, m_Skill2;
        public Image m_PetIcon;
        //int m_SkillCfgId0, m_SkillCfgId1;
        int m_SelectSkillCfgId;
        GameObject m_TipGroup;
        int[] m_SkillIds = new int[3];
        
        public int SelectCampSkillCfgId
        {
                get
                {
                        return m_SelectSkillCfgId;
                }
        }
        void Awake()
        {
                m_TipGroup = GetGameObject("tipgroup");
        }

        public void OnClickSkill(GameObject go)
        {
                for (int i = 0; i < m_SkillToggleList.Count; i++)
                {
                        if (go == m_SkillToggleList[i].gameObject)
                        {
                                m_SelectSkillCfgId = m_SkillIds[i];
                                SetTipGroup();
                                CampManager.GetInst().SelectSkill();
                        }
                }
        }
        void SetTipGroup()
        {
                RaidCampSkillConfig  cfg = CampManager.GetInst().GetCampSkillCfg(m_SelectSkillCfgId);
                if (cfg != null)
                {
                        m_TipGroup.SetActive(true);
                        GetText(m_TipGroup, "skillname").text = SkillManager.GetInst().GetSkillName(cfg.id);
                        GetText(m_TipGroup, "skilldesc").text = SkillManager.GetInst().GetSkillDesc(cfg.id);
                        GetText(m_TipGroup, "skillcd").text = cfg.cost_point.ToString();
                }
        }

        public void SetHero(HeroUnit unit)
        {
                m_SkillGroup.SetActive(true);
                m_SelectSkillCfgId = 0;
                m_TipGroup.SetActive(false);
                string icon = ModelResourceManager.GetInst().GetIconRes(unit.hero.CharacterCfg.modelid);
                ResourceManager.GetInst().LoadIconSpriteSyn(icon, m_PetIcon.transform);
                List<SkillLearnConfigHold> campSkills = unit.hero.GetCampSkill();

                for (int i = 0; i < m_SkillToggleList.Count; i++)
                {
                        m_SkillToggleList[i].gameObject.SetActive(false);
                        m_SkillIds[i] = 0;
                }
                if (campSkills == null || campSkills.Count == 0)
                {
                        return;
                }

                for (int i = 0; i < campSkills.Count; i++)
                {
                        Image im = GetImage(m_SkillToggleList[i].gameObject, "skill_icon");
                        m_SkillIds[i] = campSkills[i].id;
                        m_SkillToggleList[i].gameObject.SetActive(true);
                        ResourceManager.GetInst().LoadIconSpriteSyn(campSkills[i].icon, im.transform);
                        m_SkillToggleList[i].isOn = false;
                }

                //EventSystem.current.SetSelectedGameObject(m_SkillToggleList[0].gameObject);
                //m_SelectSkillCfgId = m_SkillIds[0];
                //SetTipGroup();
        }
        public void OnClickBack()
        {
                CloseSkill();
                CampManager.GetInst().GotoSelectActor();
        }
        public void CloseSkill()
        {
                m_SkillGroup.SetActive(false);
                m_TipGroup.SetActive(false);
        }
        public void OnClickEndCamp()
        {
                OnClickClose(null);
                
                CampManager.GetInst().ExitCamp();
        }
        public void SetSP(int sp)
        {
                GetText("SkillPoint").text = sp.ToString();
        }
}
