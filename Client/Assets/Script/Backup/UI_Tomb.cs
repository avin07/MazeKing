//using UnityEngine;
//using System.Collections;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using System.Collections.Generic;
//using System;

//public class UI_Tomb : UIBehaviour
//{
//    public GameObject m_petbtn;
//    public ScrollRect sr;
//    public long build_id;

//    void Awake()
//    {
//        m_petbtn.SetActive(false);
//    }

//    public void Refresh(long id) 
//    {
//        UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", false);
//        CreatPet();
//        build_id = id;
//    }

//    public void Refresh()
//    {
//        CreatPet();
//    }


//    void CreatPet()
//    {
//        SetGameObjectHide("pet", m_petbtn.transform.parent.gameObject);
//        List<Pet> m_petlist = PetManager.GetInst().GetMyPetListState(PetState.Death);
//        GameObject pet_btn = null;
//        for (int i = 0; i < m_petlist.Count; i++)
//        {
//            pet_btn = GetGameObject(m_petbtn.transform.parent.gameObject, "pet" + i);
//            if (pet_btn == null)
//            {
//                pet_btn = CloneElement(m_petbtn, "pet" + i);
//            }
//            pet_btn.SetActive(true);
//            Pet pet = m_petlist[i];
//            UpdatePet(pet, pet_btn);
//        }
//        GetGameObject(sr.gameObject, "content").GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
//    }

//    int need_gold = 0;
//    void UpdatePet(Pet pet, GameObject go)
//    {
//        SetStar(pet, GetGameObject(go,"star_group").transform);
//        SetPressure(pet.FighterProp.Pressure,go);
//        need_gold = CommonDataManager.GetInst().GetReviveCost(pet.GetPropertyInt("level"), pet.GetPropertyInt("star"));
//        GetText(go, "gold").text = need_gold.ToString();
//        GetText(go, "level").text = pet.GetPropertyString("level");
//        GetText(go, "maxlevel").text = "/" + pet.GetPropertyString("max_level");
//        GetText(go, "name").text = pet.GetPropertyString("name");
//        ResourceManager.GetInst().LoadIconSpriteSyn(CharacterManager.GetInst().GetCareerSysIcon(pet.GetPropertyInt("career_sys")), GetImage(go, "careersys_icon").transform);
//        ResourceManager.GetInst().LoadIconSpriteSyn(ModelResourceManager.GetInst().GetIconRes(pet.GetPropertyInt("model_id")), GetImage(go, "head").transform);
//        UIUtility.SetImageGray(true,GetImage(go, "head").transform);
//        EventTriggerListener.Get(GetGameObject(go, "revive")).onClick = OnRevive;
//        EventTriggerListener.Get(GetGameObject(go, "revive")).SetTag(pet.ID);
        

//    }


//    private void OnRevive(GameObject go, PointerEventData data)
//    {
//        long petid = (long)EventTriggerListener.Get(go).GetTag();
//        if (CommonDataManager.GetInst().GetNowResourceNum("gold") >= need_gold)
//        {
//            PetManager.GetInst().SendPetRevive(build_id,petid.ToString());
//        }
//        else
//        {
//            GameUtility.PopupMessage("金钱不足！");
//        }       
//    }

//    public void OnClose()
//    {
//        UIManager.GetInst().CloseUI(this.name);
//        UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", true);
//    }
//}
