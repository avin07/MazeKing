using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
public class UI_Pet : UI_ScrollRectHelp
{
        public enum PET_TAB
        {
            PET = 0,          //宠物
            HANDBOOK = 1,     //图鉴
            MAX = 2,
        }

        PET_TAB m_tab = PET_TAB.PET;
        GameObject[] Groups = new GameObject[(int)PET_TAB.MAX];  //存储,防止经常查询//
        GameObject choose;
        GameObject m_petbtn;
        int career_sys = 0;  //0表示全部 
        void Awake()
        {
            for (int i = 0; i < (int)PET_TAB.MAX; i++)
            {
                GameObject group = GetGameObject("Group" + (int)i);
                Groups[i] = group;
                if (i == 0)
                {
                    Groups[i].SetActive(true);
                }
                else
                {
                    Groups[i].SetActive(false);
                }
                GameObject btn = GetGameObject("Tab" + (int)i);
                EventTriggerListener listener = btn.AddComponent<EventTriggerListener>();
                listener.onClick = OnTab;
                listener.SetTag((PET_TAB)i);
            }
            choose = GetGameObject("choose");
            m_petbtn = GetGameObject(Groups[0], "pet");
            m_petbtn.SetActive(false);

        }

                
        public void OnClickClose()
        {
            UIManager.GetInst().CloseUI(this.name);
            UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", true);
        }

        public override void OnShow(float time)
        {
            UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", false);
            base.OnShow(time);
            InitKind();
            RefreshMyPetList();
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

        void RefreshMyPetList()
        {
            CreatPet();
        }

        void CreatPet()
        {
            SetChildActive(m_petbtn.transform.parent, false);
            List<Pet> m_petlist = PetManager.GetInst().GetMyPetByCareerAndRaceSort(career_sys, 0);  //种族概念暂时废弃//
            sr = GetGameObject(Groups[0], "ScrollRect").GetComponent<ScrollRect>();
            GetText("num").text = "<color=#FDCE95>" + PetManager.GetInst().GetMyAllPetNum() + "</color><color=#7D7D7D>/" + CommonDataManager.GetInst().GetHeroBagLimit() + "</color>";
            GameObject pet_btn = null;
            for (int i = 0; i < m_petlist.Count; i++)
            {
                pet_btn = GetGameObject(Groups[0], "pet" + i);
                if (pet_btn == null)
                {
                    pet_btn = CloneElement(m_petbtn, "pet" + i);
                }
                EventTriggerListener listener = EventTriggerListener.Get(pet_btn);
                listener.onClick = OnPetClick;
                listener.onDrag = OnDarg;
                listener.onBeginDrag = OnBeginDrag;
                listener.onEndDrag = OnEndDrag;
                pet_btn.SetActive(true);
                Pet pet = m_petlist[i];
                listener.SetTag(pet);
                UpdatePet(pet, pet_btn);
            }
            GetGameObject(sr.gameObject, "content").GetComponent<RectTransform>().anchoredPosition = Vector2.zero;


        }

        void UpdatePet(Pet pet, GameObject go)
        {
            //贴状态
            PetState state = (PetState)pet.GetPropertyInt("cur_state");
            for (int i = 0; i < 3; i++)
            {
                GameObject flag = GetGameObject(go, "flag" + i);
                if (flag != null)
                {
                    if (i == (int)state)
                    {
                        flag.SetActive(true);
                    }
                    else
                    {
                        flag.SetActive(false);
                    }                   
                }
            }
            if (state == PetState.Death) //死亡
            {
                UIUtility.SetImageGray(true, GetImage(go, "head").transform);
                go.GetComponent<Button>().interactable = false;
            }
            else
            {
                UIUtility.SetImageGray(false, GetImage(go, "head").transform);
                go.GetComponent<Button>().interactable = true;
            }


            SetStar(pet, GetGameObject(go, "star_group").transform);
/*            SetPressure(pet.FighterProp.Pressure, go);*/
            GetText(go, "level").text = pet.GetPropertyString("level");
            GetText(go, "maxlevel").text = "/" + pet.GetPropertyString("max_level");
            GetText(go, "name").text = pet.GetPropertyString("name");
            UIUtility.SetImageGray(!pet.CheckCanCure(), GetGameObject(go, "specificity").transform);
            UIUtility.SetImageGray(!pet.CanUseBetterEquip(), GetGameObject(go, "equip").transform);
            UIUtility.SetImageGray(!pet.CanAdvanced(), GetGameObject(go, "up").transform);
            ResourceManager.GetInst().LoadIconSpriteSyn(HeroManager.GetInst().GetCareerIcon(pet.GetPropertyInt("career")), GetImage(go, "careersys_icon").transform);
            ResourceManager.GetInst().LoadIconSpriteSyn(ModelResourceManager.GetInst().GetIconRes(pet.GetPropertyInt("model_id")), GetImage(go, "head").transform);

        }


        //IEnumerator CreatPiece(GameObject root)
        //{

        //    yield return null;  //为了保证start能被运行完
        //    for (int i = 0; i < MAX_NUM; i++)
        //    {
        //        GameObject go = GetGameObject(Content0, "pet" + i);
        //        if (go != null)
        //        {
        //            go.SetActive(false);
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }

        //    List<PetPiece> m_piecelist = PetPieceManager.GetInst().GetMyPetByCareerAndRaceSort(career_kind.GetValue(), race_kind.GetValue());
        //    GameObject pet_btn = null;
        //    sr = GetGameObject(Groups[1], "ScrollRect").GetComponent<ScrollRect>();
        //    int count = 0;
        //    for (int i = 0; i < m_piecelist.Count; i++)
        //    {
        //        if (m_piecelist[i].num > 0)
        //        {
        //            count++;
        //            pet_btn = GetGameObject(Content1, "piece" + count);
        //            if (pet_btn == null)
        //            {
        //                pet_btn = GameObject.Instantiate(UIManager.GetInst().LoadUI("PetCard")) as GameObject;
        //                pet_btn.transform.SetParent(Content1.transform);
        //                pet_btn.transform.localScale = Vector3.one;
        //                pet_btn.name = "piece" + count;
        //                EventTriggerListener.Get(pet_btn).onDarg = OnDarg;
        //                EventTriggerListener.Get(pet_btn).onBeginDarg = OnBeginDrag;
        //                EventTriggerListener.Get(pet_btn).onEndDarg = OnEndDrag;
        //            }

        //            PetPiece piece = m_piecelist[i];
        //            EventTriggerListener.Get(pet_btn).SetTag(piece);
        //            UpdatePiece(piece, pet_btn);
        //        }

        //    }
        //}

        //void RefreshMyPieceList()
        //{
        //    StartCoroutine(CreatPiece(Content1));
        
        //}

        //void UpdatePiece(PetPiece pet, GameObject go)
        //{
        //    CharacterConfig cc = CharacterManager.GetInst().GetCharacterCfg(pet.character_id);
        //    SetStar(cc.GetPropInt("star"), cc.GetPropInt("max_star"), go);
        //    GetText(go, "name").text = CharacterManager.GetInst().GetCharacterName(pet.character_id);
        //    GetText(go, "level").text = "";
        //    GetGameObject(go, "equip").SetActive(false);
        //    GetSlider(go, "sick_value").gameObject.SetActive(false);
        //    ResourceManager.GetInst().LoadIconSprite(HeroManager.GetInst().GetCareerIcon(cc.GetPropInt("career")), GetImage(go, "career_icon").transform);
        //    ResourceManager.GetInst().LoadIconSprite(ModelResourceManager.GetInst().GetIconRes(cc.GetPropInt("model_id")), GetImage(go, "head").transform);
        //    int need_num = cc.GetPropInt("compose_count");

        //    GetGameObject(go, "Compose").SetActive(true);
        //    GetText(go, "piece_num").text = pet.num + "/" + need_num;
        //    EventTriggerListener.Get(GetButton(go,"make").gameObject).SetTag(pet);
        //    EventTriggerListener.Get(GetButton(go, "make").gameObject).onClick = OnMake;
        //    Image piece_value = GetImage(go, "piece_value");
        //    float length = piece_value.rectTransform.sizeDelta.x;
        //    float now_length = length * (float)(pet.num / need_num);
        //    if (now_length > length)
        //    {
        //        now_length = length;
        //    }
        //    piece_value.rectTransform.sizeDelta = new Vector2(now_length, piece_value.rectTransform.sizeDelta.y);

        //    int race = cc.GetPropInt("race");
        //    ResourceManager.GetInst().LoadIconSprite(CharacterManager.GetInst().GetRaceBg(race), go.transform);
        //}



        void OnPetClick(GameObject go,PointerEventData data)
        {
            if (!can_click)
            {
                return;
            }
            Pet pet = EventTriggerListener.Get(go).GetTag() as Pet;
            int state = pet.GetPropertyInt("cur_state");
            if (state > (int)PetState.Normal)
            {
                    GameUtility.PopupMessage("该英雄无法操作");
            }
            else
            {
                UIManager.GetInst().ShowUI<UI_PetMain>("UI_PetMain").RefreshMain(pet.ID);
                UIManager.GetInst().CloseUI(this.name);
            }

        }


        private void OnTab(GameObject go, PointerEventData data)
        {
            PET_TAB pt = (PET_TAB)EventTriggerListener.Get(go).GetTag();

            if (pt != m_tab)
            {
                m_tab = pt;
                Reset();
                RefreshGroup(m_tab);
            }
        }

        private void OnCareerSys(GameObject go, PointerEventData data)  
        {
            int careersys = (int)EventTriggerListener.Get(go).GetTag();
            (choose.transform as RectTransform).anchoredPosition = (go.transform as RectTransform).anchoredPosition;
            career_sys = careersys;
            RefreshGroup(m_tab);
        }

        public void RefreshGroup(PET_TAB tab)
        {
            for (int i = 0; i < (int)PET_TAB.MAX; i++)
            {
                if ((int)i == (int)tab)
                {
                    Groups[i].SetActive(true);
                }
                else
                {
                    Groups[i].SetActive(false);
                }
            }
            switch (tab)
            {
                case PET_TAB.PET:
                    RefreshMyPetList();
                    break;
                case PET_TAB.HANDBOOK:
                    break;
                default:
                    break;
            }
        }

        void Reset()
        {
            career_sys = 0;
            (choose.transform as RectTransform).anchoredPosition = (GetGameObject("careersys0").transform as RectTransform).anchoredPosition;
        }


        void OnMake(GameObject go, PointerEventData data)
        {
            PetPiece pet = (PetPiece)EventTriggerListener.Get(go).GetTag();
            PetPieceManager.GetInst().SendPetCompose(pet.id);
        }


        List<string> GetCareerSys()
        {
            List<string> list = new List<string>();
            list.Add("ALL");
            for (int i = 1; i < CharacterManager.GetInst().GetCareerSysNum() + 1; i++)
            {
                list.Add(CharacterManager.GetInst().GetCareerSysName(i));
            }
            return list;
        }

        List<string> GetRace()
        {
            List<string> list = new List<string>();
            list.Add("ALL");
            for (int i = 1; i < CharacterManager.GetInst().GetRaceNum() + 1; i++)
            {
                list.Add(CharacterManager.GetInst().GetRaceName(i));
            }
            return list;
        }
        
}
