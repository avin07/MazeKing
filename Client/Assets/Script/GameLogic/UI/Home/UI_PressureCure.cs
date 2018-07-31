//using UnityEngine;
//using System.Collections;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using System.Collections.Generic;
//using System;
//using System.Linq;

//public class UI_PressureCure : UIBehaviour
//{

//        Transform bedGroup;
//        void Awake()
//        {
//                IsFullScreen = true;
//                bedGroup = transform.Find("bedgroup");
//                HeroContent = transform.Find("herolist/scrollrect/content");
//                HeroChooseState = transform.Find("herolist/gou");
//                FindComponent<Button>("bg/close").onClick.AddListener(OnClose);
//        }

//        void OnClose()
//        {
//                UIManager.GetInst().CloseUI(this.name);
//        }

//        public void Refresh()
//        {
//                RefreshMyHeroList();
//                RefreshInit();
//        }

//        #region 英雄列表

//        readonly int hero_init_num = 11;    //一屏能放的pet数
//        Pet m_select_hero = null;
//        Transform HeroContent;
//        Transform HeroChooseState;

//        public void RefreshMyHeroList()
//        {
//                CreatHero();
//        }

//        List<Pet> m_herolist = new List<Pet>();
//        void CreatHero()
//        {
//                HeroChooseState.SetActive(false);
//                HeroChooseState.SetParent(transform);
//                SetChildActive(HeroContent, false);
//                m_herolist = PetManager.GetInst().GetPressurePets();  //只显示有压力的宠物

//                int count = m_herolist.Count;
//                RectTransform hero_rtf = null;
//                if (count < hero_init_num)  //tony要求添加的空白槽位
//                {
//                        count = hero_init_num;
//                }
//                else
//                {
//                        count += 1;
//                }

//                RectTransform select_hero_rft = null;
//                for (int i = 0; i < count; i++)
//                {
//                        hero_rtf = GetChildByIndex(HeroContent, i);
//                        if (hero_rtf == null)
//                        {
//                                hero_rtf = CloneElement(GetChildByIndex(HeroContent, 0).gameObject).transform as RectTransform;
//                        }
//                        hero_rtf.name = i.ToString();
//                        hero_rtf.SetActive(true);
//                        Pet pet = null;
//                        if (i < m_herolist.Count)
//                        {
//                                pet = m_herolist[i];
//                                Button btn = hero_rtf.GetComponent<Button>();
//                                btn.onClick.RemoveAllListeners();
//                                btn.onClick.AddListener(() => OnHeroClick(btn.gameObject, pet));
//                                if (pet == m_select_hero)
//                                {
//                                        select_hero_rft = hero_rtf;
//                                }
//                        }
//                        UpdateHero(pet, hero_rtf);
//                }

//                (HeroContent as RectTransform).anchoredPosition = new Vector2(400, 0); //打开菜单有个弹出的效果//
//                //ChooseHero(select_hero_rft);
//        }

//        public void ShowPetNotNormalTime(Pet pet)
//        {
//                long id = pet.GetPropertyLong("belong_build");

//                Transform bed = bedGroup.Find(id.ToString());
//                if (bed != null)
//                {
//                        if (pet.GetShowTime() > 0)
//                        {
//                                FindComponent<Text>(bed, "des").text = UIUtility.GetTimeString3((int)pet.GetShowTime());
//                        }
//                        else
//                        {
//                                FindComponent<Text>(bed, "des").text = "";
//                        }
//                }
//        }

//        void UpdateHero(Pet pet, Transform tf)
//        {
//                if (pet == null)
//                {
//                        tf.Find("group").SetActive(false);
//                }
//                else
//                {
//                        tf.Find("group").SetActive(true);
//                        //显示状态
//                        PetState state = (PetState)pet.GetPropertyInt("cur_state");
//                        for (int i = 0; i < (int)PetState.Max; i++)
//                        {
//                                Transform flag = tf.Find("group/flag" + i);
//                                if (flag != null)
//                                {
//                                        if (i == (int)state)
//                                        {
//                                                flag.SetActive(true);
//                                        }
//                                        else
//                                        {
//                                                flag.SetActive(false);
//                                        }
//                                }
//                        }
//                        if (state == PetState.Death) //死亡
//                        {
//                                UIUtility.SetImageGray(true, tf.Find("group/icon"));
//                        }
//                        else
//                        {
//                                UIUtility.SetImageGray(false, tf.Find("group/icon"));
//                        }
//                        string career_url = HeroManager.GetInst().GetCareerIcon(pet.GetPropertyInt("career"));
//                        ResourceManager.GetInst().LoadIconSpriteSyn(career_url, tf.Find("group/career_icon"));
//                        Transform start_group = tf.Find("group/star_group");
//                        SetStar(pet, start_group);
//                        FindComponent<Text>(tf, "group/level").text = "Lv." + pet.GetPropertyString("level");
//                        ResourceManager.GetInst().LoadIconSpriteSyn(ModelResourceManager.GetInst().GetIconRes(pet.GetPropertyInt("model_id")), tf.Find("group/icon"));
//                }
//        }

//        void OnHeroClick(GameObject go, Pet pet)
//        {
//                if (pet != null)
//                {
//                        //if (m_select_hero != pet)
//                        {
//                                m_select_hero = pet;
//                                ChooseHero(go.transform);
//                        }
//                }
//        }

//        void ChooseHero(Transform tf)
//        {
//                if (m_select_hero == null)
//                {
//                        return;
//                }

//                PetState state = (PetState)m_select_hero.GetPropertyInt("cur_state");
//                if (state != PetState.Normal)
//                {
//                        GameUtility.PopupMessage("该宠物正在被治疗");
//                        return;
//                }

//                buildRealId = GetIdleBed();

//                if (buildRealId == 0)
//                {
//                        GameUtility.PopupMessage("没有空闲床位！");
//                        return;
//                }

//                HeroChooseState.SetActive(true);
//                HeroChooseState.SetParent(tf);
//                HeroChooseState.localPosition = Vector3.zero;
//                HeroChooseState.localScale = Vector3.one;

//                RefreshHeroChoose();
//        }

//        void SetSelectPetInBed()
//        {
//                Transform bed = bedGroup.Find(buildRealId.ToString());
//                UpdateChooseHero(m_select_hero, bed);
//        }

//        long GetIdleBed()
//        {
//                for (int i = 0; i < builds.Count; i++)
//                {
//                        if (PetManager.GetInst().GetPetsInBuild(builds[i]).Count <= 0)
//                        {
//                                return builds[i];
//                        }
//                }

//                return 0;
//        }

//        List<long> builds = new List<long>();
//        int furnitureId;
//        void RefreshInit()
//        {
//                BuildInfo m_info = HomeManager.GetInst().GetSelectBuild().mBuildInfo;
//                furnitureId = m_info.buildCfg.id;

//                builds = HomeManager.GetInst().GetBuildTypeInHome(m_info.buildId);

//                if(builds.Count > 9)
//                {
//                         singleton.GetInst().ShowMessage(ErrorOwner.designer, "万宇说治疗怪癖床位超过9个他吃翔");
//                }

//                for (int i = 0; i < 9; i++)
//                {
//                        RectTransform mBed = GetChildByIndex(bedGroup, i);
//                        if (mBed == null)
//                        {
//                                mBed = CloneElement(GetChildByIndex(bedGroup, 0).gameObject).transform as RectTransform;
//                        }

//                        if (i < builds.Count)
//                        {
//                                BuildInfo mInfo = HomeManager.GetInst().GetBuildInfo(builds[i]);
//                                mBed.name = mInfo.id.ToString();
//                                if (mInfo.eState != EBuildState.eUpgrage)
//                                {
//                                        List<Pet> m_pets = PetManager.GetInst().GetPetsInBuild(mInfo.id);
//                                        if (m_pets.Count > 0)  //有宠物在治疗
//                                        {
//                                                UpdateBedHero(m_pets[0],mBed);
//                                        }
//                                        else                   //空床位
//                                        {
//                                                UpdateBedNoPet(FindComponent<Image>("bed").sprite,"从下方列表中选择一名需要治疗的英雄",mBed);
//                                        }
//                                }
//                                else
//                                {
//                                        //升级中建筑无法治疗
//                                        UpdateBedNoPet(FindComponent<Image>("bed").sprite,"建筑升级中无法使用",mBed);
//                                }
//                        }
//                        else
//                        {
//                                 UpdateBedNoPet(FindComponent<Image>("suo").sprite,"建造更多的病床来解锁",mBed);
//                        }
//                }
//        }

//        void UpdateBedHero(Pet pet, Transform tf)
//        {           
//                tf.Find("group").SetActive(true);
//                tf.Find("icon").SetActive(false);
//                tf.Find("pet").SetActive(false);
//                tf.Find("cancel").SetActive(true);

//                if (pet.GetShowTime() > 0)
//                {
//                        FindComponent<Text>(tf, "des").text = UIUtility.GetTimeString3((int)pet.GetShowTime());
//                }
//                else
//                {
//                        FindComponent<Text>(tf, "des").text = "";
//                }

//                string career_url = HeroManager.GetInst().GetCareerIcon(pet.GetPropertyInt("career"));
//                ResourceManager.GetInst().LoadIconSpriteSyn(career_url, tf.Find("group/career_icon"));
//                Transform start_group = tf.Find("group/star_group");
//                SetStar(pet, start_group);
//                FindComponent<Text>(tf, "group/level").text = "Lv." + pet.GetPropertyString("level");
//                ResourceManager.GetInst().LoadIconSpriteSyn(ModelResourceManager.GetInst().GetIconRes(pet.GetPropertyInt("model_id")), tf.Find("group/icon"));    
          
//                long id = pet.ID;
//                Button cancel = FindComponent<Button>(tf,"cancel");
//                cancel.onClick.RemoveAllListeners();
//                cancel.onClick.AddListener(()=> CancelCure(id));
//        }


//        void UpdateChooseHero(Pet pet, Transform tf)
//        {
//                tf.Find("group").SetActive(true);
//                tf.Find("icon").SetActive(false);
//                tf.Find("pet").SetActive(true);
//                tf.Find("cancel").SetActive(true);

//                FindComponent<Text>(tf, "des").text = string.Empty;
                
//                string career_url = HeroManager.GetInst().GetCareerIcon(pet.GetPropertyInt("career"));
//                ResourceManager.GetInst().LoadIconSpriteSyn(career_url, tf.Find("group/career_icon"));
//                Transform start_group = tf.Find("group/star_group");
//                SetStar(pet, start_group);
//                FindComponent<Text>(tf, "group/level").text = "Lv." + pet.GetPropertyString("level");
//                ResourceManager.GetInst().LoadIconSpriteSyn(ModelResourceManager.GetInst().GetIconRes(pet.GetPropertyInt("model_id")), tf.Find("group/icon"));

//                Transform petTf = tf.Find("pet");
//                Button ok = FindComponent<Button>(petTf, "ok");
//                ok.onClick.RemoveAllListeners();
//                ok.onClick.AddListener(OnCure);

//                int cost_time;
//                string cost_res;

//                BuildingCureConfig bcc = HomeManager.GetInst().GetCurePressureCfg(furnitureId);
///*                bcc.GetCurePressureInfo(m_select_hero.FighterProp.Pressure, out cost_res, out cost_time);*/

///*                cost_time = Mathf.CeilToInt(cost_time * (1 - PlayerController.GetInst().GetPropertyInt("cure_augmentation_per") * 0.01f));*/


//                FindComponent<Text>(petTf, "name").text = m_select_hero.GetPropertyString("name");
///*                FindComponent<Text>(petTf, "time").text = UIUtility.GetTimeString3(cost_time);*/


//                int num;
//                string name;
//                int id;
//                string des;
//                Thing_Type type;

//                Image icon_image = FindComponent<Image>(petTf, "icon");
//                Text num_down = FindComponent<Text>(petTf, "cost");

///*                CommonDataManager.GetInst().SetThingIcon(cost_res, icon_image.transform, null, out name, out num, out id, out des, out type);*/

//                num = Mathf.CeilToInt(num * (1 - PlayerController.GetInst().GetPropertyInt("cure_lower_consume_per") * 0.01f));

//                bEnough = CommonDataManager.GetInst().CheckIsThingEnough(cost_res, string.Empty, PlayerController.GetInst().GetPropertyInt("cure_lower_consume_per"));
//                if (bEnough)
//                {
//                        num_down.color = Color.black;
//                }
//                else
//                {
//                        num_down.color = Color.red;
//                }

//                num_down.text = num.ToString();

//        }

//        void CancelCure(long id)
//        {
//                UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox").SetConfirmAndCancel("取消", "取消治疗压力返还花费的一半", ConfirmCancel, null, id);
//        }

//        void ConfirmCancel(object data)
//        {
//                long id = (long)data;
//                PetManager.GetInst().SendPetCurePressureCancel(id);
//        }

//        void UpdateBedNoPet(Sprite sp, string des, Transform tf)
//        {
//                tf.Find("group").SetActive(false);
//                tf.Find("cancel").SetActive(false);
//                tf.Find("pet").SetActive(false);
//                tf.Find("icon").SetActive(true);

//                FindComponent<Image>(tf,"icon").sprite = sp;
//                FindComponent<Text>(tf,"des").text = des;
//        }

//        void RefreshHeroChoose()
//        {
//                SetSelectPetInBed();

//        }

//        bool bEnough = false;
//        long buildRealId;
//        void OnCure()
//        {
//                if (bEnough)
//                {
//                        PetManager.GetInst().SendPetPressureCure(buildRealId, m_select_hero.ID);
//                }
//                else
//                {
//                        GameUtility.PopupMessage("资源不足");
//                }
//        }

//        #endregion


//}


