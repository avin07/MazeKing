//using UnityEngine;
//using System.Collections;
//using UnityEngine.UI;
//using System;
//using System.Collections.Generic;
//using UnityEngine.EventSystems;
//using DG.Tweening;

//public class UI_HomeNursing : UI_ScrollRectHelp
//{
//    List<int> m_facility = new List<int>();
//    int index = 0;
//    GameObject sick;
//    GameObject gou;
//    GameObject need;
//    Specificity_Struct m_ss;
//    long pet_id;
//    BuildingBehaviour m_bb;
//    void Awake()
//    {
//        EventTriggerListener.Get(GetGameObject("pre")).onClick = OnChange;
//        EventTriggerListener.Get(GetGameObject("pre")).SetTag(-1);
//        EventTriggerListener.Get(GetGameObject("next")).onClick = OnChange;
//        EventTriggerListener.Get(GetGameObject("next")).SetTag(1);

//        EventTriggerListener.Get(GetGameObject("choose")).onClick = OnChoose;

//        sick = GetGameObject("sick0");
//        sick.SetActive(false);
//        gou = GetGameObject("gou");
//        gou.SetActive(false);
//        need = GetGameObject("need0");
//        need.SetActive(false);
//    }


//    public void Refresh()
//    {
//        m_bb = HomeManager.GetInst().GetNowBuildBehaviour();
//        if (m_bb != null)
//        {
//            int id = m_bb.m_build_info.build_id * 1000 + m_bb.m_build_info.level;
//            m_facility = HomeManager.GetInst().GetBuildingNursingFacilityID(id);
//            RefreshFacility();
//        }
//        else
//        {
//            Debuger.LogError("流程出错！");
//        }
//    }

//    void OnChange(GameObject go, PointerEventData data)
//    {
//        int add = (int)EventTriggerListener.Get(go).GetTag();
//        int max_index = m_facility.Count - 1;
//        index += add;
//        if (index < 0)
//        {
//            index = max_index;
//        }
//        if (index > max_index)
//        {
//            index = 0;
//        }
//        if (tween != null)
//        {
//            DOTween.Kill(tween.target);
//        }
//        RefreshFacility();
//    }

//    List<long> m_pet_list = new List<long>();
//    BuildBehaviorFacilityHold m_BuildBehaviorFacility;
//    Tween tween;
//    void RefreshFacility()
//    {
//        int id = m_facility[index];
//        m_BuildBehaviorFacility = HomeManager.GetInst().GetBuildingNursingFacilityCfg(id);
//        GetText("name").text = m_BuildBehaviorFacility.name;
//        string can_cure_type = m_BuildBehaviorFacility.cure_type;
//        m_pet_list = PetManager.GetInst().GetSelectPetForNursing(can_cure_type);
//        GameObject tip = GetGameObject("tip");
//        Button choose = GetButton("choose");
//        if (m_pet_list.Count > 0)
//        {
//            tip.SetActive(true);
//            choose.interactable = true;
//            GetGameObject(choose.gameObject, "icon").SetActive(false);
//            tween = tip.GetComponent<RectTransform>().DOShakeScale(1f, new Vector3(0.2f, -0.2f, 0f), 4, 0f).SetLoops(-1, LoopType.Yoyo); 
//        }
//        else
//        {
//            if (tween != null)
//            {
//                DOTween.Kill(tween.target);
//            }
//            tip.SetActive(false);
//            choose.interactable = false;
//            GetGameObject(choose.gameObject, "icon").SetActive(false);
//        }
//        Reset();
//    }

//    void OnChoose(GameObject go, PointerEventData data)
//    {
//        if (go.GetComponent<Button>().interactable)
//        {
//            //选择宠物//
//            UIManager.GetInst().ShowUI<UI_PetChoose>("UI_PetChoose").RefreshMyPetList(m_pet_list, UI_PetEat.PET_EAT_TAB.SPECIFICITY, null);
//        }
//    }

//    public void RefreshPet(long id)
//    {
//        if (pet_id != id)
//        {
//            pet_id = id;
//            Pet pet = PetManager.GetInst().GetPet(id);
//            GetGameObject("tip").SetActive(false);
//            GetImage("icon").gameObject.SetActive(true);
//            ResourceManager.GetInst().LoadIconSprite(ModelResourceManager.GetInst().GetIconRes(pet.GetPropertyInt("model_id")), GetImage("icon").transform);
//        }

        
//        RefreshSick(id);
//    }

//    public void UpdatePetData()
//    {
//        string can_cure_type = m_BuildBehaviorFacility.cure_type;
//        m_pet_list = PetManager.GetInst().GetSelectPetForNursing(can_cure_type);
//    }

//    List<Specificity_Struct> sicklist = new List<Specificity_Struct>();
//    void RefreshSick(long id)
//    {
//        sicklist = PetManager.GetInst().GetPetSickForNursing(m_BuildBehaviorFacility.cure_type, id);
//        SetGameObjectHide("sick", sr.gameObject);
//        for (int i = 0; i < sicklist.Count; i++)
//        {
//            GameObject temp = GetGameObject(sr.gameObject, "sick" + i);
//            if (temp == null)
//            {
//                temp = CloneElement(sick, "sick" + i);
//            }

//            SpecificityHold sfh = PetManager.GetInst().GetSpecificityCfg(sicklist[i].id);

//            int skill_id = sfh.passive_skill;
//            PassiveSkillConfig cfg = SkillManager.GetInst().GetPassiveSkill(skill_id);
//            if (cfg != null)
//            {
//                GetText(temp, "m_name").text = LanguageManager.GetText(cfg.name);
//            }


//            EventTriggerListener.Get(temp.gameObject).onDown = ChooseSick;
//            EventTriggerListener.Get(temp.gameObject).onDarg = OnDarg;
//            EventTriggerListener.Get(temp.gameObject).onBeginDarg = OnBeginDrag;
//            EventTriggerListener.Get(temp.gameObject).onEndDarg = OnEndDrag;
//            EventTriggerListener.Get(temp.gameObject).SetTag(sicklist[i]);

//            temp.SetActive(true);
//        }

//        StartCoroutine(WaitForGird());
//    }

//    public IEnumerator WaitForGird()
//    {
//        yield return new WaitForEndOfFrame();
//        GetGameObject(sr.gameObject, "content").GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
//        ChooseSick(sick, null);
//    }


//    void ChooseSick(GameObject go, PointerEventData data)
//    {
//        if (!can_click)
//        {
//            return;
//        }
//        if (go.activeSelf)
//        {
//            Specificity_Struct ss = (Specificity_Struct)EventTriggerListener.Get(go).GetTag();
//            m_ss = ss;
//            SpecificityHold sfh = PetManager.GetInst().GetSpecificityCfg(ss.id);
//            gou.SetActive(true);
//            gou.transform.SetParent(go.transform);
//            gou.transform.position = go.transform.position;
//            RefreshNeed(sfh.require);
//        }

//    }

//    bool can_cure = true;
//    void RefreshNeed(string need_info)  //刷新需求材料
//    {
//        string[] need = need_info.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
//        SetGameObjectHide("need", gameObject);

//        int num = 0;
//        string name = "";
//        int id = 0;
//        string des = "";


//        for (int i = 0; i < need.Length; i++)
//        {
//            GameObject temp = GetGameObject(gameObject, "need" + i);
//            if (temp == null)
//            {
//                temp = CloneElement(this.need, "need" + i);
//            }

//            CommonDataManager.GetInst().SetThingIcon(need[i], temp.transform, out name, out num, out id, out des);
//            int has_num = CommonDataManager.GetInst().GetThingNum(need[i], out num);
//            if (has_num >= num)
//            {
//                GetText(temp, "num").text = has_num + " / " + num;
//            }
//            else
//            {
//                can_cure = false;
//                GetText(temp, "num").text = "<color=red>" + has_num + "</color>" + " / " + num;
//            }
//            temp.SetActive(true);
//        }
//    }

//    void Reset()
//    {
//        SetGameObjectHide("sick", sr.gameObject);
//        SetGameObjectHide("need", gameObject);
//        gou.SetActive(false);
//        m_ss.id = 0;
//        pet_id = 0;
//    }


//    public void SendCure()  //发送治病
//    {
//        if (!can_cure)
//        {
//            GameUtility.PopupMessage("材料不足！");
//        }
//        else
//        {
//            if (m_ss.id == 0)
//            {
//                GameUtility.PopupMessage("请选择要治愈的特性！");
//            }
//            else
//            {
//                //CAREER = 0, // 职业
//                //HELPFUL = 1, // 正面
//                //HARMFUL = 2, // 负面

//                int type = 0;
//                if (m_ss.type == Specificity_Type.BAD_RANDOM)
//                {
//                    type = 2;
//                }
//                HomeManager.GetInst().SendCureSick(m_bb.m_build_info.id, index, pet_id, m_ss.id, type);
//            }
//        }
//    }
//}

