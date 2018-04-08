using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

public class UI_CheckIn : UIBehaviour
{
        Transform bg;
        BuildBaseBehaviour bb;
        
        void Awake()
        {
                bg = transform.Find("bg");
                FindComponent<Button>("close").onClick.AddListener(OnClickClose);
        }

        public void Refresh()
        {
                UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", false);
                bb = HomeManager.GetInst().GetSelectBuild();
                RefreshUI();
        }

        public void RefreshUI()
        {
                BuildInfo m_info = bb.mBuildInfo;
                List<Pet> m_pets = PetManager.GetInst().GetPetsInBuild(m_info.id);
                BuildingHabitancyConfig bcc = HomeManager.GetInst().GetBuildHabitancyCfg(m_info.buildCfg.id);
                FindComponent<Text>("num").text = m_pets.Count + CommonString.divideStr + bcc.live_limit;
                Pet m_pet;
                RectTransform m_Build;
                for (int i = 0; i < bcc.live_limit; i++)
                {
                        m_Build = GetChildByIndex(bg, i);
                        if (m_Build == null)
                        {
                                m_Build = CloneElement(GetChildByIndex(bg, 0).gameObject).transform as RectTransform;
                        }

                        if (i < m_pets.Count)
                        {
                                m_pet = m_pets[i];
                                m_Build.Find("group").SetActive(true);
                                string url = ModelResourceManager.GetInst().GetIconRes(m_pet.GetPropertyInt("model_id"));
                                ResourceManager.GetInst().LoadIconSpriteSyn(url, m_Build.Find("icon"));
                                m_Build.GetComponent<Button>().onClick.RemoveAllListeners();

                                Button cancel_btn = FindComponent<Button>(m_Build, "group/cancel");
                                cancel_btn.onClick.RemoveAllListeners();
                                cancel_btn.onClick.AddListener(() => CancelCheckIn(m_pet.ID));
                                FindComponent<Text>(m_Build, "group/pet_level").text = "Lv." + m_pet.GetPropertyString("level");
                                m_Build.name = m_pet.ID.ToString();
                        }
                        else
                        {
                                m_Build.Find("group").SetActive(false);
                                ResourceManager.GetInst().LoadIconSpriteSyn("Bg#jia", m_Build.Find("icon"));
                                m_Build.GetComponent<Button>().onClick.RemoveAllListeners();
                                m_Build.GetComponent<Button>().onClick.AddListener(() => ChoosePet(m_info));
                                m_Build.name = "";
                        }
                }
        }

        void ChoosePet(BuildInfo info)
        {
                UIManager.GetInst().ShowUI<UI_PetChoose>("UI_PetChoose").RefreshMyPetList(PetManager.GetInst().GetCanCheckInPets(), PetChoosetype.CheckIn, 1, info);
        }

        void CancelCheckIn(long id)
        {

        }

        void OnClickClose()
        {
                UIManager.GetInst().CloseUI(this.name);
                UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", true);
        }

}
