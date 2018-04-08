using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using System.Linq;

public class UI_SpecificityCure : UIBehaviour
{
        Transform bedGroup;
        Transform goodGroup;
        Transform badGroup;
        Transform costGroup;

        void Awake()
        {
                IsFullScreen = true;

                bedGroup = transform.Find("bedgroup");
                goodGroup = transform.Find("goodgroup");
                badGroup = transform.Find("badgroup");
                costGroup = transform.Find("costgroup");

                HeroContent = transform.Find("herolist/scrollrect/content");
                HeroChooseState = transform.Find("herolist/gou");
                FindComponent<Button>("bg/close").onClick.AddListener(OnClose);
        }

        void OnClose()
        {
                UIManager.GetInst().CloseUI(this.name);
        }

        public void Refresh()
        {
                RefreshMyHeroList();
                RefreshInit();
        }

        #region 英雄列表

        readonly int hero_init_num = 11;    //一屏能放的pet数
        Pet m_select_hero = null;
        Transform HeroContent;
        Transform HeroChooseState;

        public void RefreshMyHeroList()
        {
                CreatHero();
        }

        List<Pet> m_herolist = new List<Pet>();
        void CreatHero()
        {
                HeroChooseState.SetActive(false);
                HeroChooseState.SetParent(transform);
                SetChildActive(HeroContent, false);
                m_herolist = PetManager.GetInst().GetMyPetByCareerAndRaceSort(0, 0);  //种族概念暂时废弃//

                int count = m_herolist.Count;
                RectTransform hero_rtf = null;
                if (count < hero_init_num)  //tony要求添加的空白槽位
                {
                        count = hero_init_num;
                }
                else
                {
                        count += 1;
                }

                RectTransform select_hero_rft = null;
                for (int i = 0; i < count; i++)
                {
                        hero_rtf = GetChildByIndex(HeroContent, i);
                        if (hero_rtf == null)
                        {
                                hero_rtf = CloneElement(GetChildByIndex(HeroContent, 0).gameObject).transform as RectTransform;
                        }
                        hero_rtf.name = i.ToString();
                        hero_rtf.SetActive(true);
                        Pet pet = null;
                        if (i < m_herolist.Count)
                        {
                                pet = m_herolist[i];
                                Button btn = hero_rtf.GetComponent<Button>();
                                btn.onClick.RemoveAllListeners();
                                btn.onClick.AddListener(() => OnHeroClick(btn.gameObject, pet));
                                if (pet == m_select_hero)
                                {
                                        select_hero_rft = hero_rtf;
                                }
                        }
                        UpdateHero(pet, hero_rtf);
                }

                (HeroContent as RectTransform).anchoredPosition = new Vector2(400, 0); //打开菜单有个弹出的效果//
                //ChooseHero(select_hero_rft);
        }

        public void ShowPetNotNormalTime(Pet pet)
        {
                long id = pet.GetPropertyLong("belong_build");

                Transform bed = bedGroup.Find(id.ToString());
                if (bed != null)
                {
                        if (pet.GetShowTime() > 0)
                        {
                                FindComponent<Text>(bed, "des").text = UIUtility.GetTimeString3((int)pet.GetShowTime());
                        }
                        else
                        {
                                FindComponent<Text>(bed, "des").text = "";
                        }
                }
        }

        void UpdateHero(Pet pet, Transform tf)
        {
                if (pet == null)
                {
                        tf.Find("group").SetActive(false);
                }
                else
                {
                        tf.Find("group").SetActive(true);
                        //显示状态
                        PetState state = (PetState)pet.GetPropertyInt("cur_state");
                        for (int i = 0; i < (int)PetState.Max; i++)
                        {
                                Transform flag = tf.Find("group/flag" + i);
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
                                UIUtility.SetImageGray(true, tf.Find("group/icon"));
                        }
                        else
                        {
                                UIUtility.SetImageGray(false, tf.Find("group/icon"));
                        }
                        string career_url = CharacterManager.GetInst().GetCareerIcon(pet.GetPropertyInt("career"));
                        ResourceManager.GetInst().LoadIconSpriteSyn(career_url, tf.Find("group/career_icon"));
                        Transform start_group = tf.Find("group/star_group");
                        SetStar(pet, start_group);
                        FindComponent<Text>(tf, "group/level").text = "Lv." + pet.GetPropertyString("level");
                        ResourceManager.GetInst().LoadIconSpriteSyn(ModelResourceManager.GetInst().GetIconRes(pet.GetPropertyInt("model_id")), tf.Find("group/icon"));
                }
        }

        void OnHeroClick(GameObject go, Pet pet)
        {
                if (pet != null)
                {
                        //if (m_select_hero != pet)
                        {
                                m_select_hero = pet;
                                ChooseHero(go.transform);
                        }
                }
        }

        void ChooseHero(Transform tf)
        {
                if (m_select_hero == null)
                {
                        return;
                }

                PetState state = (PetState)m_select_hero.GetPropertyInt("cur_state");
                if (state != PetState.Normal)
                {
                        GameUtility.PopupMessage("该宠物正在被治疗");
                        return;
                }

                buildRealId = GetIdleBed();

                if (buildRealId == 0)
                {
                        GameUtility.PopupMessage("没有空闲床位！");
                        return;
                }

                HeroChooseState.SetActive(true);
                HeroChooseState.SetParent(tf);
                HeroChooseState.localPosition = Vector3.zero;
                HeroChooseState.localScale = Vector3.one;

                RefreshHeroChoose();
        }

        void SetSelectPetInBed()
        {
                Transform bed = bedGroup.Find(buildRealId.ToString());
                UpdateBedHero(m_select_hero, bed);
                FindComponent<Text>(bed, "group/time").text = "";
                bed.Find("group/des").SetActive(false);
                bed.Find("cancel").SetActive(false);
        }

        long GetIdleBed()
        {
                for (int i = 0; i < builds.Count; i++)
                {
                        if (PetManager.GetInst().GetPetsInBuild(builds[i]).Count <= 0)
                        {
                                return builds[i];
                        }
                }

                return 0;
        }

        List<long> builds = new List<long>();
        void RefreshInit()
        {
                goodGroup.SetActive(false);
                badGroup.SetActive(false);
                costGroup.SetActive(false);
                BuildInfo m_info = HomeManager.GetInst().GetSelectBuild().mBuildInfo;
                builds = HomeManager.GetInst().GetBuildTypeInHome(m_info.buildId);

                if(builds.Count > 3)
                {
                         singleton.GetInst().ShowMessage(ErrorOwner.designer, "万宇说治疗怪癖床位超过三个他吃翔");
                }

                for (int i = 0; i < 3; i++)
                {
                        RectTransform mBed = GetChildByIndex(bedGroup, i);
                        if (mBed == null)
                        {
                                mBed = CloneElement(GetChildByIndex(bedGroup, 0).gameObject).transform as RectTransform;
                        }

                        if (i < builds.Count)
                        {

                                BuildInfo mInfo = HomeManager.GetInst().GetBuildInfo(builds[i]);
                                mBed.name = mInfo.id.ToString();
                                if (mInfo.eState != EBuildState.eUpgrage)
                                {
                                        List<Pet> m_pets = PetManager.GetInst().GetPetsInBuild(mInfo.id);
                                        if (m_pets.Count > 0)  //有宠物在治疗
                                        {
                                                UpdateBedHero(m_pets[0],mBed);
                                        }
                                        else                   //空床位
                                        {
                                                UpdateBedNoPet(FindComponent<Image>("bed").sprite,"从下方列表中选择一名需要治疗的英雄",mBed);
                                        }
                                }
                                else
                                {
                                        //升级中建筑无法治疗
                                        UpdateBedNoPet(FindComponent<Image>("bed").sprite,"建筑升级中无法使用",mBed);
                                }
                        }
                        else
                        {
                                 UpdateBedNoPet(FindComponent<Image>("suo").sprite,"建造更多的病床来解锁",mBed);
                        }
                }
        }

        void UpdateBedHero(Pet pet, Transform tf)
        {           
                tf.Find("group").SetActive(true);
                tf.Find("icon").SetActive(false);
                tf.Find("group/des").SetActive(true);
                FindComponent<Text>(tf, "des").text = "";
                FindComponent<Text>(tf, "group/name").text = pet.GetPropertyString("name");
                if (pet.GetShowTime() > 0)
                {
                        FindComponent<Text>(tf, "group/time").text = UIUtility.GetTimeString3((int)pet.GetShowTime());
                }
                else
                {
                        FindComponent<Text>(tf, "group/time").text = "";
                }
                string career_url = CharacterManager.GetInst().GetCareerIcon(pet.GetPropertyInt("career"));
                ResourceManager.GetInst().LoadIconSpriteSyn(career_url, tf.Find("group/career_icon"));
                Transform start_group = tf.Find("group/star_group");
                SetStar(pet, start_group);
                FindComponent<Text>(tf, "group/level").text = "Lv." + pet.GetPropertyString("level");
                ResourceManager.GetInst().LoadIconSpriteSyn(ModelResourceManager.GetInst().GetIconRes(pet.GetPropertyInt("model_id")), tf.Find("group/icon"));    
          
                long id = pet.ID;
                Button cancel = FindComponent<Button>(tf,"cancel");
                cancel.gameObject.SetActive(true);
                cancel.onClick.RemoveAllListeners();
                cancel.onClick.AddListener(()=> CancelPurify(id));
        }

        void CancelPurify(long id)
        {
                UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox").SetConfirmAndCancel("取消", "取消治疗特性返还花费的一半", ConfirmCancel, null, id);
        }

        void ConfirmCancel(object data)
        {
                long id = (long)data;
                PetManager.GetInst().SendPetPurifyCancel(id);
        }

        void UpdateBedNoPet(Sprite sp, string des, Transform tf)
        {
                tf.Find("group").SetActive(false);
                tf.Find("cancel").SetActive(false);
                tf.Find("icon").SetActive(true);
                FindComponent<Image>(tf,"icon").sprite = sp;
                FindComponent<Text>(tf,"des").text = des;
        }

        void RefreshHeroChoose()
        {
                SetSelectPetInBed();

                goodGroup.SetActive(true);
                badGroup.SetActive(true);
                costGroup.SetActive(false);
                RefreshSpecificity(Specificity_Type.GOOD_RANDOM, goodGroup.Find("content"));
                RefreshSpecificity(Specificity_Type.BAD_RANDOM, badGroup.Find("content"));
        }

        void RefreshSpecificity(Specificity_Type type, Transform group)
        {
                int max_have = 0;
                if (type == Specificity_Type.GOOD_RANDOM)
                {
                        max_have = m_select_hero.GetPropertyInt("random_helpful_behavior_quantity");
                }
                if (type == Specificity_Type.BAD_RANDOM)
                {
                        max_have = m_select_hero.GetPropertyInt("random_harmful_behavior_quantity");
                }


                List<Specificity_Struct> specificity_list =  m_select_hero.GetMyPetSpecificity(type);
                SetChildActive(group, false);

                for (int i = 0; i < max_have; i++)
                {
                        RectTransform specificity = GetChildByIndex(group, i);
                        if (specificity == null)
                        {
                                specificity = CloneElement(GetChildByIndex(group, 0).gameObject).transform as RectTransform;
                        }
                        specificity.SetActive(true);

                        if (i < specificity_list.Count)
                        {
                                SpecificityHold sfh = PetManager.GetInst().GetSpecificityCfg(specificity_list[i].id);
                                FindComponent<Text>(specificity, "Text").text = LanguageManager.GetText(sfh.name);

                                Button btn = specificity.GetComponent<Button>();
                                btn.onClick.RemoveAllListeners();
                                btn.onClick.AddListener(() => OnClickSpecificity(sfh));
                        }
                        else
                        {
                                FindComponent<Text>(specificity, "Text").text = "";
                                Button btn = specificity.GetComponent<Button>();
                                btn.onClick.RemoveAllListeners();
                        }
                }
        }

        bool bEnough = false;
        void OnClickSpecificity(SpecificityHold sfh)
        {
                costGroup.SetActive(true);
                FindComponent<Text>(costGroup, "Text").text = "去除" + LanguageManager.GetText(sfh.name);

                int num;
                string name;
                int id;
                string des;
                Thing_Type type;

                Image icon_image = FindComponent<Image>(costGroup, "cost_icon");
                Text num_down = FindComponent<Text>(costGroup, "cost");

                CommonDataManager.GetInst().SetThingIcon(sfh.remove_cost, icon_image.transform, null, out name, out num, out id, out des, out type);

                bEnough =CommonDataManager.GetInst().CheckIsThingEnough(sfh.remove_cost);
                if(bEnough)
                {
                        num_down.color = Color.green; 
                }
                else
                {
                        num_down.color = Color.red; 
                }

                num_down.text = num.ToString();
                FindComponent<Text>(costGroup, "time").text = UIUtility.GetTimeString3(sfh.remove_cost_time);

                Button btn = FindComponent<Button>(costGroup, "cure");
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(()=> OnCure(sfh));
        }

        long buildRealId;
        void OnCure(SpecificityHold sfh)
        {
                if (bEnough)
                {
                        PetManager.GetInst().SendPetPurify(buildRealId, m_select_hero.ID, sfh.good_or_bad - 1, sfh.id);
                }
                else
                {
                        GameUtility.PopupMessage("资源不足");
                }
        }

        #endregion


}

