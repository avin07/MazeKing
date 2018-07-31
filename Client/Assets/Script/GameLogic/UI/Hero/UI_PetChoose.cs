using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

public class UI_PetChoose : UI_ScrollRectHelp
{

        List<long> show_pet_list = new List<long>();

        GameObject choose;
        GameObject m_petbtn;
        int career_sys = 0;  //0表示全部 
        int max_choose_limit = 0;  //最大选择数量//
        Transform Cost;
        PetChoosetype m_type = PetChoosetype.LevelUp;
        void Awake()
        {
                choose = GetGameObject("choose");
                m_petbtn = GetGameObject("pet");
                m_petbtn.SetActive(false);
                Cost = transform.Find("GameObject/Cost");
        }


        public override void OnShow(float time)
        {
                base.OnShow(time);
                InitKind();
        }

        void InitKind()
        {
                GameObject careersys = GetGameObject("careersys0");
                RectTransform rt = careersys.transform as RectTransform;
                for (int i = 0; i < CharacterManager.GetInst().GetCareerSysNum() + 1; i++)
                {
                        GameObject temp = GetGameObject("careersys" + i);
                        if (temp == null)
                        {
                        temp = CloneElement(careersys, "careersys" + i);
                        (temp.transform as RectTransform).anchoredPosition = new Vector2(rt.anchoredPosition.x + (rt.sizeDelta.x + 20.0f) * i, rt.anchoredPosition.y);
                        ResourceManager.GetInst().LoadIconSpriteSyn(CharacterManager.GetInst().GetCareerSysIcon(i), temp.transform);
                        }
                        EventTriggerListener listener = temp.GetComponent<EventTriggerListener>();
                        if (listener == null)
                        {
                                listener = temp.AddComponent<EventTriggerListener>();
                        }
                        listener.onClick = OnCareerSys;
                        listener.SetTag(i);
                }
                choose.transform.SetAsLastSibling();
        }

        private void OnCareerSys(GameObject go, PointerEventData data)
        {
                int careersys = (int)EventTriggerListener.Get(go).GetTag();
                (choose.transform as RectTransform).anchoredPosition = (go.transform as RectTransform).anchoredPosition;
                career_sys = careersys;
                CreatPet();
        }

        public void RefreshMyPetList(List<long> pet_list, PetChoosetype type, int limit_num) //显示的宠物，类型，已经选中的宠物，限制的数量//
        {
                show_pet_list = pet_list;
                m_type = type;
                max_choose_limit = limit_num;
                CreatPet();
        }

        BuildInfo m_build_info;
        public void RefreshMyPetList(List<long> pet_list, PetChoosetype type, int limit_num, BuildInfo build_info)  //为了治疗压力
        {
               m_build_info = build_info;
               RefreshMyPetList(pet_list, type, limit_num);
        }


        void CreatPet()
        {
                Cost.SetActive(false);
                SetChildActive(m_petbtn.transform.parent,false);
                List<Pet> m_petlist = PetManager.GetInst().GetMyPetByCareerAndRaceSort(show_pet_list, career_sys, 0);
                GameObject pet_btn = null;
                for (int i = 0; i < m_petlist.Count; i++)
                {
                        pet_btn = GetGameObject(m_petbtn.transform.parent.gameObject, "pet" + i);
                        if (pet_btn == null)
                        {
                                pet_btn = CloneElement(m_petbtn, "pet" + i);
                        }
                        EventTriggerListener listener = EventTriggerListener.Get(pet_btn);
                        listener.onClick = OnSelectPet;
                        listener.onDrag = OnDarg;
                        listener.onBeginDrag = OnBeginDrag;
                        listener.onEndDrag = OnEndDrag;
                        pet_btn.SetActive(true);
                        Pet pet = m_petlist[i];
                        listener.SetTag(pet);
                        UpdatePet(pet, pet_btn);
                }
                //yield return new WaitForEndOfFrame();
                GetGameObject(sr.gameObject, "content").GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        }

        void UpdatePet(Pet pet, GameObject go)
        {
                //贴状态
                SetStar(pet, GetGameObject(go, "star_group").transform);
/*                SetPressure(pet.FighterProp.Pressure, go);*/
                GetText(go, "level").text = pet.GetPropertyString("level");
                GetImage(go, "gou").gameObject.SetActive(false);
                ResourceManager.GetInst().LoadIconSpriteSyn(HeroManager.GetInst().GetCareerIcon(pet.GetPropertyInt("career")), GetImage(go, "careersys_icon").transform);
                ResourceManager.GetInst().LoadIconSpriteSyn(ModelResourceManager.GetInst().GetIconRes(pet.GetPropertyInt("model_id")), GetImage(go, "head").transform);
                if (PetManager.GetInst().ChoosePetList.Contains(pet.ID))
                {
                        SetGou(go);
                }     
        }

        private void OnSelectPet(GameObject go, PointerEventData data)
        {
                if (!can_click)
                {
                        return;
                }
                Pet pet = EventTriggerListener.Get(go).GetTag() as Pet;
                Image gou = GetImage(go, "gou");
                if (!gou.gameObject.activeSelf)
                {
                        if (PetManager.GetInst().ChoosePetList.Count < max_choose_limit)
                        {
                                PetManager.GetInst().ChoosePetList.Add(pet.ID);
                        }
                        else
                        {
                                //超过材料最大选择数//
                                GameUtility.PopupMessage("超过需求上限");
                                return;
                        }

                        if (m_type == PetChoosetype.PressureCure)
                        {
/*                                RefreshCost(pet.FighterProp.Pressure);*/
                        }
                }
                else
                {
                        PetManager.GetInst().ChoosePetList.Remove(pet.ID);
                }
                gou.gameObject.SetActive(!gou.gameObject.activeSelf);
        }

        void SetGou(GameObject go)
        {
                Image gou = GetImage(go, "gou");
                gou.gameObject.SetActive(true);
        }


        public void OnClickCancel()
        {
                PetManager.GetInst().ChoosePetList.Clear();
                RefreshByType();
        }


        public void OnClickOk()
        {
                RefreshByType();
        }


        void RefreshByType()
        {
                bool canclose = true;
                if (m_type == PetChoosetype.Advanced)
                {
                        UIManager.GetInst().GetUIBehaviour<UI_PetMain>().RefreshNeedPet();
                        UIManager.GetInst().SetUIActiveState<UI_PetMain>("UI_PetMain", true);
                }
                if (m_type == PetChoosetype.LevelUp)
                {
                        UIManager.GetInst().GetUIBehaviour<UI_PetMain>().RefreshLevelUp();
                        UIManager.GetInst().SetUIActiveState<UI_PetMain>("UI_PetMain", true);
                }
                if (m_type == PetChoosetype.Visitor)
                {
                        UIManager.GetInst().GetUIBehaviour<UI_NpcVisitor>().RefreshPetChoose();
                        UIManager.GetInst().SetUIActiveState<UI_NpcVisitor>("UI_NpcVisitor", true);
                }
                if (m_type == PetChoosetype.PressureCure)
                {
                        if (PetManager.GetInst().ChoosePetList.Count == 1)
                        {
                                Pet m_pet = PetManager.GetInst().GetPet(PetManager.GetInst().ChoosePetList[0]);
                                if (CommonDataManager.GetInst().CheckIsThingEnough(cost_res, string.Empty, PlayerController.GetInst().GetPropertyInt("cure_lower_consume_per")))
                                {
                                        PetManager.GetInst().SendPetPressureCure(m_build_info.id, m_pet.ID);
                                        PetManager.GetInst().ChoosePetList.Clear();
                                }
                                else
                                {
                                        canclose = false;
                                        GameUtility.PopupMessage("材料不足！");
                                }
                        }
                }
                if (m_type == PetChoosetype.CheckIn)
                {
                        Pet m_pet = PetManager.GetInst().GetPet(PetManager.GetInst().ChoosePetList[0]);
                        PetManager.GetInst().SendPetCheckIn(m_build_info.id, m_pet.ID);
                        PetManager.GetInst().ChoosePetList.Clear();
                }
                if (canclose)
                {
                        UIManager.GetInst().CloseUI(this.name);
                }
        }

        string cost_res;
        void RefreshCost(int pressure)
        {
                Cost.SetActive(true);
                int cost_time;

                int mainFurnitureId = m_build_info.buildCfg.id;
                BuildingCureConfig bcc = HomeManager.GetInst().GetCurePressureCfg(mainFurnitureId);
                bcc.GetCurePressureInfo(pressure, out cost_res, out cost_time);

                cost_time = Mathf.CeilToInt(cost_time * (1 - PlayerController.GetInst().GetPropertyInt("cure_augmentation_per") * 0.01f));

                FindComponent<Text>(Cost, "time").text = UIUtility.GetTimeString3(cost_time);
                int num = 0;
                string name = "";
                int id = 0;
                string des = "";
                Thing_Type type;
                CommonDataManager.GetInst().SetThingIcon(cost_res, Cost.Find("res_icon"),null, out name, out num, out id, out des, out type);

                num = Mathf.CeilToInt(num * (1 - PlayerController.GetInst().GetPropertyInt("cure_lower_consume_per") * 0.01f));
                FindComponent<Text>(Cost, "res").text = num.ToString();

                if (CommonDataManager.GetInst().CheckIsThingEnough(cost_res, string.Empty, PlayerController.GetInst().GetPropertyInt("cure_lower_consume_per"))) 
                {
                        FindComponent<Text>(Cost, "res").color = Color.white;
                }
                else
                {
                        FindComponent<Text>(Cost, "res").color = Color.red;
                }

        }      
}

public enum PetChoosetype
{
        LevelUp,
        Advanced,
        Visitor,
        PressureCure,
        CheckIn,
}
