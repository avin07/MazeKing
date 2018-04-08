﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class UI_VillagerOption : UIBehaviour
{
        public Text m_TextDesc;
        public Button m_Option0;
        
        List<Button> m_BtnList = new List<Button>();
        VillagerBehaviour m_Villager;
        HeroUnit m_Unit;
        void Awake()
        {
                this.IsFullScreen = true;
                m_BtnList.Add(m_Option0);
        }

        public void OnClickOption(GameObject go)
        {
                int option = int.Parse(go.name.Replace("option", ""));
                VillageElementOptionConfig cfg = VillageManager.GetInst().GetVillageElementOptionCfg(option);
                if (cfg != null)
                {
//                         if (cfg.next_option != null && cfg.next_option.Count > 0)
//                         {
//                                 SetupOptions(cfg.next_option);
//                         }
//                         else
                        {
                                VillageManager.GetInst().SendOption(cfg.id, m_Villager);
                                OnClickClose(null);
                        }
                }
        }
        bool CheckOptionVisible(VillageElementOptionConfig cfg)
        {
//                 //该选项只能选一次，如果已经选择过，则不能再选
//                 if (cfg.choose_once == 1)
//                 {
//                         if (m_Villager.compOptions != null && m_Villager.compOptions.Contains(cfg.id))
//                         {
//                                 return false;
//                         }
//                 }
// 
//                 if (m_Villager.compOptionEffects != null)
//                 {
//                         //该选项如果有某个选项效果出现过，则不能再选
//                         if (cfg.option_effect_disappear > 0)
//                         {
//                                 if (m_Villager.compOptionEffects.Contains(cfg.option_effect_disappear))
//                                 {
//                                         return false;
//                                 }
//                         }
//                 }
//                 //该选项如果有前置效果，则看有没有出现过来判断能不能选
//                 if (cfg.pre_option_effect > 0)
//                 {
//                         if (m_Villager.compOptionEffects != null)
//                         {
//                                 return m_Villager.compOptionEffects.Contains(cfg.pre_option_effect);
//                         }
//                         return false;
//                 }
                
                return true;
        }

        bool CheckOptionAvailable(VillageElementOptionConfig cfg)
        {
                bool bAvail = true;
                if (bAvail)
                {
                        //该选项如果有需求技能可见，则看当前英雄是否有该技能来判断能不能选
                        if (cfg.adventure_skill_appear > 0)
                        {
                                bAvail = false;
                                foreach (int skillId in m_Unit.hero.GetSkills().Keys)
                                {
                                        if (skillId == cfg.adventure_skill_appear)
                                        {
                                                bAvail = true;
                                                break;
                                        }
                                }

                        }
                }
                return bAvail;
        }
        string GetOptionText(VillageElementOptionConfig cfg)
        {
                return LanguageManager.GetText(cfg.option_name);
        }

        void SetupOptions(List<int> options)
        {
                foreach (Button btn in m_BtnList)
                {
                        btn.gameObject.SetActive(false);
                }
                int idx = 0;
                for (int i = 0; i < options.Count; i++)
                {
                        VillageElementOptionConfig cfg = VillageManager.GetInst().GetVillageElementOptionCfg(options[i]);
                        if (cfg != null)
                        {
                                if (CheckOptionVisible(cfg) == false)
                                        continue;

                                GameObject optionObj = null;
                                if (idx < m_BtnList.Count)
                                {                                        
                                        optionObj = m_BtnList[idx].gameObject;
                                }
                                else
                                {
                                        optionObj = CloneElement(m_Option0.gameObject);
                                        m_BtnList.Add(optionObj.GetComponent<Button>());
                                }

                                optionObj.SetActive(true);
                                optionObj.name = "option" + cfg.id;
                                m_BtnList[idx].interactable = CheckOptionAvailable(cfg);
                                GetText(optionObj, "Text").text = GetOptionText(cfg);
                                idx++;
                        }
                }
        }

        public void Setup(VillagerBehaviour villager, HeroUnit unit)
        {
                if (villager != null && villager.elemCfg != null)
                {
                        //m_TextDesc.text = LanguageManager.GetText(villager.elemCfg.desc);
                        m_Villager = villager;
                        m_Unit = unit;
                        List<string> optionRetList = new List<string>();
                        List<string> optionTextList = new List<string>();
                        SetupOptions(villager.elemCfg.type_id);
                }
                else
                {
                        OnClickClose(null);
                }
        }
}
