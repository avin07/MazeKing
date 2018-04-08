//using UnityEngine;
//using System.Collections;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using System.Collections.Generic;
//public class UI_RaidPetChoose : UI_ScrollRectHelp
//{

//    public GameObject content;
//    public GameObject lineup_pet;
//    public GameObject m_pet;

//    readonly int MAX_LINEUP_NUM = 5; //最大上阵数，暂时
//    int limit_num;

//    List<long> MyChoosePet = new List<long>();
//    GameObject Front_Windows; //前一个界面
//    int raid_id = 0;
//    int floor = 1;
//    public void OnClickReturn()
//    {
//        UIManager.GetInst().CloseUI(this.name);
//        if (Front_Windows != null)
//        {
//            Front_Windows.SetActive(true);
//        }
//    }

//    public override void OnShow(float time)
//    {
//        base.OnShow(time);
//        RefreshMyPetList();
//    }

//    void RefreshMyPetList()
//    {
//        StartCoroutine(CreatPet());
//    }

//    public void InitLineUp(int raid,int floor,GameObject window)
//    {
//        Front_Windows = window;
//        Front_Windows.SetActive(false);
//        raid_id = raid;
//        this.floor = floor;
//        int raid_max_num = RaidManager.GetInst().GetRaidInfoCfg(raid).battle_hero_limit;
//        limit_num = raid_max_num < MAX_LINEUP_NUM ? raid_max_num : MAX_LINEUP_NUM;
//        for (int i = 0; i < MAX_LINEUP_NUM; i++)
//        {
//            GameObject pet = GetGameObject("lineuppet" + i);
//            if (pet == null)
//            {
//                pet = CloneElement(lineup_pet, "lineuppet" + i);
//                pet.GetComponent<RectTransform>().anchoredPosition += new Vector2((lineup_pet.GetComponent<RectTransform>().rect.width + 10.0f) * i, 0);
//            }
//            if (i < raid_max_num)
//            {
//                pet.GetComponent<Button>().interactable = true;
//                GetGameObject(pet, "head").SetActive(false);
//                GetGameObject(pet, "Image").SetActive(false);
//                EventTriggerListener.Get(pet).onClick = OnClickLineupPet;
//            }
//            else
//            {
//                pet.GetComponent<Button>().interactable = false;
//                GetGameObject(pet, "head").SetActive(false);
//                GetGameObject(pet, "Image").SetActive(true);
//            }
//        }
//    }

//    IEnumerator CreatPet()
//    {

//        yield return null;  //为了保证start能被运行完
//        SetGameObjectHide("pet", content);

//        List<Pet> m_petlist = PetManager.GetInst().GetMyPetListSort();

//        for (int i = 0; i < m_petlist.Count; i++)
//        {
//            GameObject pet_btn = GetGameObject(content, "pet" + i);
//            if (pet_btn == null)
//            {
//                pet_btn = CloneElement(m_pet, "pet" + i);
//            }
//            EventTriggerListener.Get(pet_btn).onClick = OnPetClick;
//            EventTriggerListener.Get(pet_btn).onDarg = OnDarg;
//            EventTriggerListener.Get(pet_btn).onBeginDarg = OnBeginDrag;
//            EventTriggerListener.Get(pet_btn).onEndDarg = OnEndDrag;
//            pet_btn.SetActive(true);
//            Pet pet = m_petlist[i];
//            EventTriggerListener.Get(pet_btn).SetTag(pet.ID);
//            UpdatePet(pet, pet_btn);
//        }
//        m_pet.SetActive(false);
//    }

//    void UpdatePet(Pet pet, GameObject go)
//    {
//        //贴状态
//        SetStar(pet.GetPropertyInt("star"), pet.GetPropertyInt("max_star"), go, GetGameObject(go, "star0"));
//        SetPressure(pet.FighterProp.Pressure, go);
//        ResourceManager.GetInst().LoadIconSpriteSyn(ModelResourceManager.GetInst().GetIconRes(pet.GetPropertyInt("model_id")), GetImage(go, "head").transform);
//        GetGameObject(go, "gou").SetActive(false);
//        GetText(go, "level").text = "LV." + pet.GetPropertyString("level");
//        ResourceManager.GetInst().LoadIconSpriteSyn(CharacterManager.GetInst().GetCareerSysIcon(pet.GetPropertyInt("career_sys")), GetImage(go, "career_icon").transform);
//    }



//    void OnPetClick(GameObject go, PointerEventData data)
//    {
//        if (!can_click)
//        {
//            return;
//        }
//        long pet_id = (long)EventTriggerListener.Get(go).GetTag();
//        if (MyChoosePet.Contains(pet_id))
//        {
//            MyChoosePet.Remove(pet_id);
//            GetGameObject(go, "gou").SetActive(false);
//        }
//        else
//        {
//            if (MyChoosePet.Count == limit_num)
//            {
//                GameUtility.PopupMessage("超过携带英雄上限！");
//            }
//            else
//            {
//                MyChoosePet.Add(pet_id);
//                GetGameObject(go, "gou").SetActive(true);
//            }
//        }
//        RefreshLineupPet();
//    }


//    void OnClickLineupPet(GameObject go, PointerEventData data)
//    {
//        GameObject pet = EventTriggerListener.Get(go).GetTag() as GameObject;
//        if (pet != null)
//        {
//            OnPetClick(pet, null);
//        }
//    }


//    void RefreshLineupPet()
//    {
//        for (int i = 0; i < MAX_LINEUP_NUM; i++)
//        {
//            GameObject pet = GetGameObject("lineuppet" + i);
//            if (i < MyChoosePet.Count)
//            {
//                Pet m_pet = PetManager.GetInst().GetPet(MyChoosePet[i]);
//                ResourceManager.GetInst().LoadIconSpriteSyn(ModelResourceManager.GetInst().GetIconRes(m_pet.GetPropertyInt("model_id")), GetImage(pet, "head").transform);
//                GetGameObject(pet, "head").SetActive(true);

//                List<Pet> m_petlist = PetManager.GetInst().GetMyPetListSort();
//                int index = m_petlist.IndexOf(m_pet);
//                GameObject pet_btn = GetGameObject(content, "pet" + index);
//                EventTriggerListener.Get(pet).SetTag(pet_btn);
//            }
//            else
//            {
//                GetGameObject(pet, "head").SetActive(false);
//                EventTriggerListener.Get(pet).SetTag(null);
//            }
//        }
//    }

//    public void GotoRaid()
//    {
//        string petlist = "";
//        if (MyChoosePet.Count == 0)
//        {
//            GameUtility.PopupMessage("请选择上阵英雄！");
//            return;
//        }

//        int food = PlayerController.GetInst().GetPropertyInt("food");
//        int need_food = RaidManager.GetInst().GetRaidInfoCfg(raid_id).cost_vitality;
//        if (food < need_food)
//        {
//            GameUtility.PopupMessage("食物不足！");
//            return;
//        }

//        for (int i = 0; i < MyChoosePet.Count; i++)
//        {
//            petlist += MyChoosePet[i] + "|";
//        }


//        WorldMapManager.GetInst().GoIntoRaid(raid_id, floor, petlist);

//        UIManager.GetInst().CloseUI(this.name);
//        UIManager.GetInst().CloseUI(Front_Windows.name);
//    }
//}
