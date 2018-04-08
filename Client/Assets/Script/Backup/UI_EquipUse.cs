//using UnityEngine;
//using System.Collections;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using System.Collections.Generic;
//using System;

//public class UI_EquipUse : UI_ScrollRectHelp
//{

//    public GameObject now;
//    public GameObject late;
//    public GameObject bag;
//    public GameObject gou;
//    Equip m_now_equip = null;
//    Equip m_select_equip = null;
//    Pet m_pet;
//    ItemType.EQUIP_PART part;

//    public void Refresh(int m_part,long pet_id)
//    {
//        gou.SetActive(false);
//        m_pet = PetManager.GetInst().GetPet(pet_id);
//        part = (ItemType.EQUIP_PART)m_part;
//        m_now_equip = m_pet.GetMyEquipPart(part);
//        RefreshNow(m_now_equip);
//        RefreshLate(m_select_equip);
//        RefreshBag();
//        GetButton(bag, "puton").interactable = false; 
//    }

//    void RefreshNow(Equip equip)
//    {
//        if (equip == null)
//        {
//            GetButton(bag, "takeoff").interactable = false; 
//            if (part == ItemType.EQUIP_PART.OFF_HAND)  //处理双手武器//fuck//
//            {
//                Equip main_hand = m_pet.GetMyEquipPart(ItemType.EQUIP_PART.MAIN_HAND);
//                if (main_hand != null)
//                {
//                    EquipHoldConfig ec = EquipManager.GetInst().GetEquipCfg(main_hand.equip_configid);
//                    if (ec.is_two_hands == 1)
//                    {
//                        GetImage(now, "icon").gameObject.SetActive(true);
//                        ResourceManager.GetInst().LoadIconSpriteSyn(ec.icon, GetImage(now, "icon").transform);
//                        GetText(now, "des").text = LanguageManager.GetText(ec.desc);
//                        GetImage(now, "icon").color = new Color(1, 1, 1, 0.35f);
//                        return;
//                    }
//                }
//            }
//            GetImage(now, "icon").gameObject.SetActive(false);
//            GetText(now, "des").text = "未穿戴装备";
//        }
//        else
//        {
//            GetButton(bag, "takeoff").interactable = true; 
//            EquipHoldConfig ec = EquipManager.GetInst().GetEquipCfg(equip.equip_configid);
//            GetImage(now, "icon").gameObject.SetActive(true);
//            ResourceManager.GetInst().LoadIconSpriteSyn(ec.icon, GetImage(now, "icon").transform);
//            GetText(now, "des").text = LanguageManager.GetText(ec.desc);
//            GetImage(now, "icon").color = new Color(1, 1, 1, 1);
//        }
//    }


//    void RefreshLate(Equip equip)
//    {
//        if (equip == null)
//        {
//            GetImage(late, "icon").gameObject.SetActive(false);
//            GetText(late, "des").text = "未选中装备";
//        }
//        else
//        {
//            EquipHoldConfig ec = EquipManager.GetInst().GetEquipCfg(equip.equip_configid);
//            GetImage(late, "icon").gameObject.SetActive(true);
//            ResourceManager.GetInst().LoadIconSpriteSyn(ec.icon, GetImage(late, "icon").transform);
//            GetText(late, "des").text = LanguageManager.GetText(ec.desc) + "\nneed lv" + ec.need_level;
//        }
//    }

//    void RefreshBag()
//    {
//        GameObject m_equip = GetGameObject(sr.gameObject, "equip0");
//        List<Equip> m_equip_list = EquipManager.GetInst().GetEquipInBagByPartBySort(part, m_pet);
//        m_equip.SetActive(false);
//        for (int i = 0; i < m_equip_list.Count; i++)
//        {
//            GameObject temp = GetGameObject(sr.gameObject, "equip" + i);
//            if (temp == null)
//            {
//                temp = GameObject.Instantiate(m_equip) as GameObject;
//                temp.name = "equip" + i;
//                temp.transform.SetParent(m_equip.transform.parent);
//                temp.transform.localScale = Vector3.one;
//            }
//            EquipHoldConfig ec = EquipManager.GetInst().GetEquipCfg(m_equip_list[i].equip_configid);
//            ResourceManager.GetInst().LoadIconSpriteSyn(ec.icon, temp.transform);
//            EventTriggerListener.Get(temp).onClick = OnEquipClick;
//            EventTriggerListener.Get(temp).onDrag = OnDarg;
//            EventTriggerListener.Get(temp).onBeginDrag = OnBeginDrag;
//            EventTriggerListener.Get(temp).onEndDrag = OnEndDrag;
//            EventTriggerListener.Get(temp).SetTag(m_equip_list[i]);
//            temp.SetActive(true);
//        }
//        StartCoroutine(WaitForGird());
//    }

//    public IEnumerator WaitForGird()
//    {
//        yield return new WaitForEndOfFrame();
//        GetGameObject(sr.gameObject, "content").GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
//    }

//    void OnEquipClick(GameObject go, PointerEventData data)
//    {
//        if (!can_click)
//        {
//            return;
//        }
//        m_select_equip = EventTriggerListener.Get(go).GetTag() as Equip;
//        gou.SetActive(true);
//        gou.transform.position = GetGameObject(go,"pos").transform.position;
//        gou.transform.SetParent(go.transform);
//        RefreshLate(m_select_equip);
//        EquipHoldConfig ec = EquipManager.GetInst().GetEquipCfg(m_select_equip.equip_configid);
//        if (ec.need_level > m_pet.GetPropertyInt("level"))
//        {
//            GetButton(bag, "puton").interactable = false; 
//        }
//        else
//        {
//            GetButton(bag, "puton").interactable = true; 
//        }

//    }

//    public void OnClickClose()
//    {
//        UIManager.GetInst().CloseUI(this.name);
//        //UIManager.GetInst().SetUIActiveState<UI_PetEat>("UI_PetEat", true);
//    }

//    public void PutOn(GameObject go)
//    {
//        if (go.GetComponent<Button>().interactable)
//        {
//            if (m_now_equip != null)
//            {
//                UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox").SetConfirmAndCancel("提示", "更换该装备会使原装备消失，是否继续", ConfirmPutOn, null, null);
//            }
//            else
//            {
//                if (part == ItemType.EQUIP_PART.OFF_HAND)  //处理双手武器//fuck//
//                {
//                    Equip main_hand = m_pet.GetMyEquipPart(ItemType.EQUIP_PART.MAIN_HAND);
//                    if (main_hand != null)
//                    {
//                        EquipHoldConfig ec = EquipManager.GetInst().GetEquipCfg(main_hand.equip_configid);
//                        if (ec.is_two_hands == 1)
//                        {
//                            UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox").SetConfirmAndCancel("提示", "更换该装备会使原来的双手武器消失，是否继续", ConfirmPutOn, null, null);
//                            return;
//                        }
//                    }
//                }
//                ConfirmPutOn(null);
//            }
//        }

//    }

//    public void TakeOff(GameObject go)
//    {
//        if (go.GetComponent<Button>().interactable)
//        {
//            if (CommonDataManager.GetInst().IsBagFull())
//            {
//                GameUtility.PopupMessage("背包已经满了,请先清理！");
//                return;
//            }

//            EquipHoldConfig ec = EquipManager.GetInst().GetEquipCfg(m_now_equip.equip_configid);
//            string []info = ec.unload_cost.Split(',');
//            string name = CommonDataManager.GetInst().GetResourcesCfg(int.Parse(info[0])).name;
//            UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox").SetConfirmAndCancel("提示", "脱装备需要消耗" + info[1] + name + "是否拖下", ConfirmTakeOff, null, null);

//        }

//    }

//    void ConfirmPutOn(object data)
//    {
//        EquipManager.GetInst().SendPutOn(m_pet.ID, m_select_equip.id, (int)part);
//    }


//    void ConfirmTakeOff(object data)
//    {
//        EquipManager.GetInst().SendTakeOff(m_pet.ID, m_now_equip.id);
//    }
//}

