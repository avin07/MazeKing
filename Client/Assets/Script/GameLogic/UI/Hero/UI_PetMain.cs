using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using System.Linq;



/// <summary>
/// 这个界面使用频率大，用到的组件都直接在脚本上挂在
/// </summary>

public class UI_PetMain : UI_ScrollRectHelp
{
        public enum TAB
        {
                LevelUp = 0,      //升级
                Advanced = 1,     //进阶
                Specificity = 2,  //状态
                Equip = 3,        //装备
                Max = 4
        }

        TAB m_tab = TAB.LevelUp;
        GameObject[] Groups = new GameObject[(int)TAB.Max];
        public GameObject left;
        Pet m_pet;
        void Awake()
        {
                ModelCamera = GameObject.Find("UIModleCamera");
                for (int i = 0; i < (int)TAB.Max; i++)
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
                        EventTriggerListener.Get(btn).onClick = OnTab;
                        EventTriggerListener.Get(btn).SetTag((TAB)i);
                }

                for (int i = 0; i < attr_num; i++)
                {
                        attrs_obj[i] = GetGameObject(advanced_attr_group, (i + 1).ToString());
                }
                GameObject obj = GetGameObject(left, "head");
                EventTriggerListener.Get(obj).onDrag = OnMoveModelDarg;
                SetLight();

                HeroContent = transform.Find("herolist/scrollrect/content");
                HeroChooseState = transform.Find("herolist/gou");
        }

        public void OnClickClose()
        {
                UIManager.GetInst().CloseUI(this.name);
                UIManager.GetInst().SetUIActiveState<UI_HeroMain>("UI_HeroMain", true);
                UIManager.GetInst().ShowUI<UI_HeroMain>("UI_HeroMain").RefreshMyHeroList(m_pet);
                if (mod != null)
                {
                    GameObject.Destroy(mod);
                }
        }

        public override void OnShow(float time)
        {
                PetManager.GetInst().ChoosePetList.Clear();
                base.OnShow(time);
        }


        #region 英雄列表

        readonly int hero_init_num = 11;    //一屏能放的pet数
        Pet m_select_hero = null;

        Transform HeroContent;
        Transform HeroChooseState;

        public void RefreshMyHeroList(Pet selecthero = null)
        {
                CreatHero(selecthero);
        }

        List<Pet> m_herolist = new List<Pet>();
        void CreatHero(Pet selecthero)
        {
                HeroChooseState.SetActive(false);
                HeroChooseState.SetParent(transform);
                SetChildActive(HeroContent, false);
                m_herolist = PetManager.GetInst().GetMyPetByCareerAndRaceSort(0, 0);  //种族概念暂时废弃//
                if (selecthero == null)
                {
                        if (m_select_hero == null)
                        {
                                if (m_herolist.Count > 0)
                                {
                                        m_select_hero = m_herolist[0];
                                }
                        }
                }
                else
                {
                        m_select_hero = selecthero;
                }
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
                ChooseHero(select_hero_rft);
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

                        string career_url = HeroManager.GetInst().GetCareerIcon(pet.GetPropertyInt("career"));
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
                        if (m_select_hero != pet)
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
                PetManager.GetInst().ChoosePetList.Clear();

                HeroChooseState.SetActive(true);
                HeroChooseState.SetParent(tf);
                HeroChooseState.localPosition = Vector3.zero;
                HeroChooseState.localScale = Vector3.one;

                RefreshMain(m_select_hero.ID);
        }

        #endregion


        private void OnTab(GameObject go, PointerEventData data)
        {
                TAB pt = (TAB)EventTriggerListener.Get(go).GetTag();

                if (pt != m_tab)
                {
                        if (pt == TAB.Advanced)
                        {
                                if (target.Equals("0") || target.Length == 0)
                                {
                                        GameUtility.PopupMessage("该英雄无法转生！");
                                        HightLightTab(m_tab);
                                        return;
                                }
                                if (m_pet.GetPropertyInt("level") != m_pet.GetPropertyInt("max_level"))
                                {
                                        GameUtility.PopupMessage("没到达等级上限！");
                                        HightLightTab(m_tab);
                                        return;
                                }

                        }
                        PetManager.GetInst().ChoosePetList.Clear();
                        RefreshGroup(pt);
                }
        }

        void HightLightTab(TAB tab)
        {
                GetToggle(gameObject, "Tab" + (int)tab).isOn = true;
        }

        public void RefreshGroup(TAB tab)
        {
                m_tab = tab;
                for (int i = 0; i < (int)TAB.Max; i++)
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
                HightLightTab(tab);
                switch (tab)
                {
                        case TAB.LevelUp:
                                RefreshLevelUp();
                                break;
                        case TAB.Advanced:
                                RefreshAdvanced();
                                break;
                        case TAB.Specificity:
                                RefreshSpecificity();
                                break;
                        case TAB.Equip:
                                break;
                        default:
                                break;
                }
        }

        public void RefreshMain(long id)
        {
                m_pet = PetManager.GetInst().GetPet(id);
                RefreshLeft();
                RefreshGroup(TAB.LevelUp);
        }

        public void RefreshLeft()
        {
                GetText(left, "name").text = m_pet.GetPropertyString("name");
                GetText(left, "level").text = m_pet.GetPropertyString("level");
                ResourceManager.GetInst().LoadIconSpriteSyn(HeroManager.GetInst().GetCareerIcon(m_pet.GetPropertyInt("career")), GetImage(left, "career").transform);
                SetStar(m_pet, GetGameObject(left, "star_group").transform);
                RefreshModel();
                FindComponent<Text>(left, "des").text = LanguageManager.GetText(m_pet.GetPropertyString("desc"));
                target = m_pet.GetPropertyString("target");
        }


        GameObject ModelCamera;
        GameObject mod;
        float rotate_offset = -1.5f;
        void OnMoveModelDarg(GameObject go, PointerEventData eventdata)
        {
                if (mod != null)
                {
                        mod.transform.Rotate(Vector3.up, eventdata.delta.x * rotate_offset);
                }
        }

         void SetLight()
        {

            GameObject go = new GameObject("light_kebi");
            go.transform.localPosition = Vector3.zero;
            go.transform.localEulerAngles = new Vector3(31.2f, 41.9f, 83.76f);
            go.transform.SetParent(this.transform);

            Light light1 = go.AddComponent<Light>();
            light1.type = LightType.Directional;
            light1.color = new Color(244f / 255f, 241f / 255f, 229f / 255f, 1);
            light1.shadows = LightShadows.Soft;
            light1.intensity = 0.4f;
            light1.cullingMask = 1 << 17;

            GameObject go2 = new GameObject("light_kebi");
            go2.transform.localPosition = Vector3.zero;
            go2.transform.localEulerAngles = new Vector3(309.5f, 233.8f, 293.4f);
            go2.transform.SetParent(this.transform);

            Light light2 = go2.AddComponent<Light>();
            light2.type = LightType.Directional;
            light2.color = new Color(189f / 255f, 191f / 255f, 240f / 255f, 1);
            light2.intensity = 0.2f;
            light2.cullingMask = 1 << 17;
        }


        public void RefreshModel()
        {

                if (mod != null)
                {
                    GameObject.DestroyImmediate(mod);    
                }
                
                CharacterConfig characterCfg = CharacterManager.GetInst().GetCharacterCfg(m_pet.CharacterID);
                if (mod == null)
                {
                    if (characterCfg != null)
                    {

                            mod = CharacterManager.GetInst().GenerateModel(characterCfg);
                            mod.transform.SetParent(ModelCamera.transform);
                            GameUtility.SetLayer(mod, "UIModel");
                            mod.transform.localEulerAngles = new Vector3(17.7f, 180, 0f);
                            mod.transform.localPosition = new Vector3(-2.51f, -0.17f, 24);
                            GameUtility.ObjPlayAnim(mod, "idle_001", true);

                    }
                }

                   
              
        }

        void OnEnable()
        {
                if (ModelCamera != null)
                {
                        ModelCamera.GetComponent<Camera>().enabled = true;
                }

        }


        void OnDisable()
        {
                if (ModelCamera != null)
                {
                    ModelCamera.GetComponent<Camera>().enabled = false;
                }
        }

        #region 强化（就是升级）

        public void RefreshLevelUp()
        {
                RefreshLevelupBottom();
                RefreshLevelUpUp();
        }


        public GameObject exp_bar_group;
        public GameObject level_up_skill_group;
        int level_up_need_gold;
        void RefreshLevelUpUp()
        {

                long now_exp = m_pet.GetPropertyInt("exp");
                int max_level = m_pet.GetPropertyInt("max_level");
                int level = m_pet.GetPropertyInt("level");
                if (PetManager.GetInst().ChoosePetList.Count <= 0) //没有选择宠物
                {
                        if (level == max_level) //当前等级已经是最大等级
                        {
                                GetText(exp_bar_group, "exp").text = "MAX";
                                GetImage(exp_bar_group, "exp0").fillAmount = 0;

                        }
                        else
                        {
                                long next_level_need_exp = CharacterManager.GetInst().GetCharacterExp(level + 1).need_exp;
                                GetText(exp_bar_group, "exp").text = now_exp + " / " + next_level_need_exp;
                                GetImage(exp_bar_group, "exp0").fillAmount = (float)now_exp / (float)next_level_need_exp;
                        }
                        GetText(exp_bar_group, "now_level").text = level.ToString();
                        GetGameObject(exp_bar_group, "Image").SetActive(false);
                        level_up_skill_group.SetActive(false);
                        GetImage(exp_bar_group, "exp1").fillAmount = 0;
                        GetText(exp_bar_group, "next_level").text = "";
                        GetText(levelup_pet_group.transform.parent.gameObject, "need_gold").text = "";
                }
                else
                {

                        long food_exp = PetManager.GetInst().AllGetFoodExp();
                        long all_exp = food_exp + now_exp + CharacterManager.GetInst().GetCharacterExp(m_pet.GetPropertyInt("level")).total_exp;
                        int level_to = CharacterManager.GetInst().CanLevelUp(all_exp);
                        if (level_to >= max_level)  //
                        {
                                level_to = max_level;
                                GetText(exp_bar_group, "exp").text = "MAX";
                                GetImage(exp_bar_group, "exp0").fillAmount = 0;
                                GetImage(exp_bar_group, "exp1").fillAmount = 0;
                        }
                        else
                        {
                                long next_level_need_exp = CharacterManager.GetInst().GetCharacterExp(level_to + 1).need_exp;
                                if (level_to == level)
                                {
                                        GetImage(exp_bar_group, "exp0").fillAmount = (float)now_exp / (float)next_level_need_exp;
                                }
                                else
                                {
                                        GetImage(exp_bar_group, "exp0").fillAmount = 0;
                                }
                                long show_now_exp = CharacterManager.GetInst().GetCharacterExp(level_to + 1).need_exp - (CharacterManager.GetInst().GetCharacterExp(level_to + 1).total_exp - all_exp);
                                GetText(exp_bar_group, "exp").text = show_now_exp + " / " + next_level_need_exp;
                                GetImage(exp_bar_group, "exp1").fillAmount = (float)show_now_exp / (float)next_level_need_exp;
                        }
                        GetText(exp_bar_group, "now_level").text = level.ToString();
                        GetText(exp_bar_group, "next_level").text = level_to.ToString();
                        GetGameObject(exp_bar_group, "Image").SetActive(true);
                        //RefreshSkillLevelUpRate();

                        level_up_need_gold = (int)(food_exp / GlobalParams.GetInt("exp_trans_gold"));
                        GetText(levelup_pet_group.transform.parent.gameObject, "need_gold").text = level_up_need_gold.ToString();

                }


        }

        readonly int max_skill_show_num = 4;
        void RefreshSkillLevelUpRate()
        {
                level_up_skill_group.SetActive(true);
                SetChildActive(level_up_skill_group.transform,false);

                List<Skill_Struct> m_skill = m_pet.GetMySkill();
                int show_num = 0;
                for (int i = 0; i < m_skill.Count; i++)
                {
                        Skill_Struct ss = m_skill[i];
                        SkillLearnConfigHold sfh = SkillManager.GetInst().GetSkillInfo(ss.config_id);
                        if (sfh != null)
                        {
                                if (show_num < max_skill_show_num)
                                {
                                        //if (sfh.type != 4) //冒险技能不升级
                                        //{
                                        GameObject skill = GetGameObject(level_up_skill_group, "skill" + show_num);
                                        skill.SetActive(true);
                                        if (ss.level >= SkillManager.GetInst().GetSkillMaxLevel(ss.config_id, m_pet.GetPropertyInt("star")))
                                        {
                                                GetText(skill, "level").text = "Max";
                                                GetText(skill, "rate").text = "";
                                                GetGameObject(skill, "up").SetActive(false);
                                        }
                                        else
                                        {
                                                GetText(skill, "level").text = "LV." + ss.level;
                                                GetText(skill, "rate").text = PetManager.GetInst().CalSkillUpRate(ss.config_id, ss.level) + "%";
                                                GetGameObject(skill, "up").SetActive(true);
                                        }
                                        ResourceManager.GetInst().LoadIconSpriteSyn(SkillManager.GetInst().GetSkillIconUrl(ss.config_id), GetImage(skill, "icon").transform);
                                        show_num++;
                                        //}
                                }
                        }
                }
        }


        public GameObject levelup_pet_group;
        void RefreshLevelupBottom() //共用数量max_need_num = 5
        {
                for (int i = 0; i < max_need_num; i++)
                {
                        GameObject pet = GetGameObject(levelup_pet_group, "pet" + i);
                        if (i < PetManager.GetInst().ChoosePetList.Count)
                        {
                                Pet m_pet = PetManager.GetInst().GetPet(PetManager.GetInst().ChoosePetList[i]);
                                RefreshPet(m_pet, pet);
                        }
                        else
                        {
                                GetGameObject(pet, "show").SetActive(false);
                                ResourceManager.GetInst().LoadIconSpriteSyn("Bg#jia", GetImage(pet, "icon").transform);
                        }
                        EventTriggerListener.Get(pet).onClick = ChooseLevelupNeed;
                }
                GetText(levelup_pet_group.transform.parent.gameObject, "all_gold").text = CommonDataManager.GetInst().GetNowResourceNum("gold").ToString();
        }

        void ChooseLevelupNeed(GameObject go, PointerEventData data)
        {
                //if (m_pet.GetPropertyInt("level") == m_pet.GetPropertyInt("max_level"))
                //{
                //    GameUtility.PopupMessage("该宠物已达最高等级！");
                //    return;
                //}

                int limit_num = max_need_num; //根据界面定位5
                UIManager.GetInst().ShowUI<UI_PetChoose>("UI_PetChoose").RefreshMyPetList(PetManager.GetInst().GetMyPetLeveUpFood(m_pet.ID), PetChoosetype.LevelUp, limit_num);
                gameObject.SetActive(false);
        }

        public void OnLevel() //升级
        {

                if (PetManager.GetInst().ChoosePetList.Count == 0)
                {
                        GameUtility.PopupMessage("请先选择材料！");
                        return;
                }

                if (level_up_need_gold > CommonDataManager.GetInst().GetNowResourceNum("gold"))
                {
                        GameUtility.PopupMessage("金钱不足！");
                        return;
                }

                if (IsFoodPetHasEquip())
                {
                        if (CommonDataManager.GetInst().IsBagFull())
                        {
                                GameUtility.PopupMessage("背包已满,请先清理！");
                                return;
                        }
                }

                string food = "";
                if (PetManager.GetInst().ChoosePetList.Count > 0)
                {
                        for (int i = 0; i < PetManager.GetInst().ChoosePetList.Count; i++)
                        {
                                food += PetManager.GetInst().ChoosePetList[i] + "|";
                        }
                }


                HashSet<long> food_pet = new HashSet<long>(PetManager.GetInst().ChoosePetList.Cast<long>());
                //HashSet<long> diy_used_pet = DiyRaidManager.GetInst().GetDiyMazeUsedPet();

                //if (food_pet.Overlaps(diy_used_pet))
                //{
                //        UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox").SetConfirmAndCancel("提示", "饲料中有在自建迷宫防守小队中的英雄，是否继续吞噬？", ConfirmLevel, null, food);
                //}
                //else
                {
                        PetManager.GetInst().SendPetLevelUp(m_pet.ID, food);
                }


        }


        void ConfirmLevel(object data)
        {
                string food = (string)data;
                PetManager.GetInst().SendPetLevelUp(m_pet.ID, food);
        }



        #endregion


        #region 进阶(就是升星和转职)

        public GameObject advanced_pet_group;
        public GameObject advanced_attr_group;
        string target = "";
        int m_target_id = 0;
        int target_num = 0;
        const int attr_num = 5;
        GameObject[] attrs_obj = new GameObject[attr_num];
        public void RefreshAdvanced()
        {
                RefreshAdvancedTarget();
        }

        void RefreshAdvancedTarget()
        {
                GameObject old_pet = GetGameObject(advanced_pet_group, "old_pet");
                GameObject one = GetGameObject(advanced_pet_group, "0");
                GameObject two = GetGameObject(advanced_pet_group, "1");
                RefreshPet(m_pet, old_pet);  //原宠物
                string[] target_info = target.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                target_num = target_info.Length;
                if (target_num == 1) //单转职
                {
                        one.SetActive(true);
                        two.SetActive(false);
                        for (int i = 0; i < target_num; i++)
                        {
                                int target_id = int.Parse(target_info[i]);
                                Pet pet = new Pet(target_id);
                                GameObject pet_obj = GetGameObject(one, "pet" + i);
                                RefreshPet(pet, pet_obj);  //转职宠物
                                m_target_id = pet.CharacterID;
                                RefreshAttrGroup(pet);
                                RefreshNeed(pet);
                        }
                }
                else if (target_num == 2)//双转职
                {
                        one.SetActive(false);
                        two.SetActive(true);
                        for (int i = 0; i < target_num; i++)
                        {
                                int target_id = int.Parse(target_info[i]);
                                Pet pet = new Pet(target_id);
                                GameObject pet_obj = GetGameObject(two, "pet" + i);
                                RefreshPet(pet, pet_obj);  //转职宠物
                                UIUtility.SetGroupGray(true, pet_obj);
                                EventTriggerListener.Get(pet_obj).SetTag(pet);
                                EventTriggerListener.Get(pet_obj).onClick = OnTarget;
                        }
                        advanced_attr_group.SetActive(false);
                        advanced_pet_grid_obj.transform.parent.gameObject.SetActive(false);
                }
                else
                {
                    singleton.GetInst().ShowMessage(ErrorOwner.designer, m_pet.CharacterID + "英雄进阶目标配错了！");
                }


        }

        void RefreshPet(Pet pet, GameObject go)
        {
                SetStar(pet, GetGameObject(go, "star_group").transform);
                ResourceManager.GetInst().LoadIconSpriteSyn(HeroManager.GetInst().GetCareerIcon(pet.GetPropertyInt("career")), GetImage(go, "career").transform);
                ResourceManager.GetInst().LoadIconSpriteSyn(ModelResourceManager.GetInst().GetIconRes(pet.GetPropertyInt("model_id")), GetImage(go, "icon").transform);
                UIUtility.SetImageGray(false, GetGameObject(go, "icon").transform);
                GameObject show = GetGameObject(go, "show");
                if (show != null)
                {
                        show.SetActive(true);
                }
                Text lv = GetText(go, "level");
                if (lv != null)
                {
                        lv.text = pet.GetPropertyString("level");
                }
                Text temp = GetText(go, "temp");
                if (temp != null)
                {
                        temp.text = "";
                }
        }

        void OnTarget(GameObject go, PointerEventData data)
        {
                PetManager.GetInst().ChoosePetList.Clear();
                if (target_num == 2) //置灰另外一个
                {
                        for (int i = 0; i < target_num; i++)
                        {
                                GameObject pet_obj = GetGameObject(GetGameObject(advanced_pet_group, "1"), "pet" + i);
                                if (go.name != pet_obj.name)
                                {
                                        UIUtility.SetGroupGray(true, pet_obj);
                                }

                        }
                }
                UIUtility.SetGroupGray(false, go);
                Pet target_pet = EventTriggerListener.Get(go).GetTag() as Pet;
                SetStar(target_pet, GetGameObject(go, "star_group").transform);
                m_target_id = target_pet.CharacterID;
                RefreshAttrGroup(target_pet);
                RefreshNeed(target_pet);
        }

        void RefreshAttrGroup(Pet target_pet)
        {
                advanced_attr_group.SetActive(true);
                //第一条，是否转职//
                int old_career = m_pet.GetPropertyInt("career");
                int target_career = target_pet.GetPropertyInt("career");
                if (old_career == target_career) //不转职
                {
                        GetGameObject(attrs_obj[0], "other").SetActive(false);
                        GetText(attrs_obj[0], "name0").text = "无转职";
                }
                else
                {
                        GetGameObject(attrs_obj[0], "other").SetActive(true);
                        GetText(attrs_obj[0], "name0").text = LanguageManager.GetText(HeroManager.GetInst().GetCareerCfg(old_career).name);
                        GetText(attrs_obj[0], "name1").text = LanguageManager.GetText(HeroManager.GetInst().GetCareerCfg(target_career).name);
                }

                //第二条，进阶描述
                GetText(attrs_obj[1], "des").text = LanguageManager.GetText(target_pet.GetPropertyString("rising_star_desc"));

                //第三条最大等级提升
                GetText(attrs_obj[2], "level0").text = LanguageManager.GetText(m_pet.GetPropertyString("max_level"));
                GetText(attrs_obj[2], "level1").text = LanguageManager.GetText(target_pet.GetPropertyString("max_level"));

                //第四第五技能相关//主动技能和被动过技能都要检测，优先检测替换然后是新增，大于两个属于配置错误，丢弃
                int index = 3;
                string[] skill_list_old = (m_pet.GetPropertyString("skill_list") + "," + m_pet.GetPropertyString("passive_skill")).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string[] skill_list_new = (target_pet.GetPropertyString("skill_list") + "," + target_pet.GetPropertyString("passive_skill")).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < skill_list_new.Length; i++) //技能数量只会增加
                {
                        if (skill_list_old.Length > i)
                        {
                                int old_skill_id = int.Parse(skill_list_old[i]);
                                int new_skill_id = int.Parse(skill_list_new[i]);
                                if (old_skill_id != new_skill_id) //说明技能强化了
                                {
                                        AttrSkillUp(index, old_skill_id, new_skill_id);
                                        index++;
                                        if (index > 4)
                                        {
                                                return;
                                        }
                                }
                        }
                        else         //新技能产生
                        {
                                AttrSkillGet(index, int.Parse(skill_list_new[i]));
                                index++;
                                if (index > 4)
                                {
                                        return;
                                }
                        }
                }

                for (int i = index; i < index + 2; i++)
                {
                        attrs_obj[index].SetActive(false);
                }
        }

        void AttrSkillUp(int index, int old_skill_id, int new_skill_id)  //技能加强
        {
                attrs_obj[index].SetActive(true);
                GetGameObject(attrs_obj[index], "new_skill").SetActive(false);
                GetGameObject(attrs_obj[index], "skill_up").SetActive(true);
                ResourceManager.GetInst().LoadIconSpriteSyn(SkillManager.GetInst().GetSkillIconUrl(old_skill_id), GetGameObject(attrs_obj[index], "icon0").transform);
                ResourceManager.GetInst().LoadIconSpriteSyn(SkillManager.GetInst().GetSkillIconUrl(new_skill_id), GetGameObject(attrs_obj[index], "icon1").transform);
        }

        void AttrSkillGet(int index, int new_skill_id) //技能获得
        {
                attrs_obj[index].SetActive(true);
                GetGameObject(attrs_obj[index], "new_skill").SetActive(true);
                GetGameObject(attrs_obj[index], "skill_up").SetActive(false);
                ResourceManager.GetInst().LoadIconSpriteSyn(SkillManager.GetInst().GetSkillIconUrl(new_skill_id), GetGameObject(attrs_obj[index], "icon").transform);
        }

        public GameObject advanced_pet_grid_obj;
        public GameObject advanced_item_grid_obj;
        public GameObject advanced_item_none;
        readonly int max_need_num = 5;
        int really_need_num = 0;
        int advanced_need_gold = 0;
        void RefreshNeed(Pet target_pet)
        {
                advanced_pet_grid_obj.transform.parent.gameObject.SetActive(true);
                RefreshNeedPet(target_pet);
                RefreshNeedItem(target_pet.GetPropertyString("trans_need_item"));
                advanced_need_gold = target_pet.GetPropertyInt("trans_need_gold");
                GetText(advanced_pet_grid_obj.transform.parent.gameObject, "gold").text = advanced_need_gold.ToString();
        }

        public void RefreshNeedPet()
        {
                RefreshNeedPet(new Pet(m_target_id));
        }

        void RefreshNeedPet(Pet target_pet) //刷新材料
        {
                //pet
                int trans_need_charactar = target_pet.GetPropertyInt("trans_need_charactar");
                really_need_num = target_pet.GetPropertyInt("trans_need_count");
                if (trans_need_charactar == 0)  //同星级转职
                {
                        for (int i = 0; i < max_need_num; i++)
                        {
                                GameObject pet = GetGameObject(advanced_pet_grid_obj, "pet" + i);
                                if (i < really_need_num)
                                {
                                        if (PetManager.GetInst().ChoosePetList.Count > i)
                                        {
                                                Pet m_pet = PetManager.GetInst().GetPet(PetManager.GetInst().ChoosePetList[i]);
                                                RefreshPet(m_pet, pet);
                                        }
                                        else
                                        {
                                                pet.SetActive(true);
                                                GetGameObject(pet, "show").SetActive(false);
                                                ResourceManager.GetInst().LoadIconSpriteSyn("Bg#" + m_pet.GetPropertyString("star") + "star", GetImage(pet, "icon").transform);
                                                EventTriggerListener.Get(pet).onClick = OnChooseAdvancedNeed;
                                                EventTriggerListener.Get(pet).SetTag(really_need_num);
                                        }

                                }
                                else
                                {
                                        pet.SetActive(false);
                                }

                        }
                }
                else //指定职业转职
                {
                        for (int i = 0; i < max_need_num; i++)
                        {
                                GameObject pet = GetGameObject(advanced_pet_grid_obj, "pet" + i);
                                if (i < really_need_num)
                                {
                                        if (PetManager.GetInst().ChoosePetList.Count > i)
                                        {
                                                Pet m_pet = PetManager.GetInst().GetPet(PetManager.GetInst().ChoosePetList[i]);
                                                RefreshPet(m_pet, pet);
                                        }
                                        else
                                        {
                                                pet.SetActive(true);
                                                GetGameObject(pet, "show").SetActive(false);
                                                ResourceManager.GetInst().LoadIconSpriteSyn(ModelResourceManager.GetInst().GetIconRes(CharacterManager.GetInst().GetCharacterModelId(trans_need_charactar)), GetImage(pet, "icon").transform);
                                                UIUtility.SetImageGray(true, GetGameObject(pet, "icon").transform);
                                                EventTriggerListener.Get(pet).onClick = OnChooseAdvancedNeed;
                                                EventTriggerListener.Get(pet).SetTag(really_need_num);
                                        }

                                }
                                else
                                {
                                        pet.SetActive(false);
                                }

                        }
                }

        }



        bool can_advance_item = true;
        void RefreshNeedItem(string item_need)  //物品
        {
                can_advance_item = true;
                if (item_need.Equals("0"))  //不需要物品
                {
                        advanced_item_grid_obj.SetActive(false);
                        advanced_item_none.SetActive(true);
                }
                else
                {
                        advanced_item_grid_obj.SetActive(true);
                        advanced_item_none.SetActive(false);
                        for (int i = 0; i < max_need_num; i++)
                        {
                                GameObject item = GetGameObject(advanced_item_grid_obj, "item" + i);
                                string[] need = item_need.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                if (i < need.Length)
                                {
                                        int num = 0;
                                        string name = "";
                                        int id = 0;
                                        string des = "";
                                        string needitem = "3," + need[i]; //策划约定一定是物品！
                                        Thing_Type type;
                                        CommonDataManager.GetInst().SetThingIcon(needitem, GetGameObject(item, "icon").transform, null,out name, out num, out id, out des,out type);
                                        int has_num = CommonDataManager.GetInst().GetThingNum(needitem, out num);
                                        if (has_num >= num)
                                        {
                                                GetText(item, "num").text = has_num + " / " + num;
                                        }
                                        else
                                        {
                                                can_advance_item = false;
                                                GetText(item, "num").text = "<color=red>" + has_num + "</color>" + " / " + num;
                                        }
                                        item.SetActive(true);
                                }
                                else
                                {
                                        item.SetActive(false);
                                }
                        }
                }


        }

        void OnChooseAdvancedNeed(GameObject go, PointerEventData data)
        {
                int limit_num = (int)EventTriggerListener.Get(go).GetTag();
                UIManager.GetInst().ShowUI<UI_PetChoose>("UI_PetChoose").RefreshMyPetList(PetManager.GetInst().GetMyPetAdvanceFood(m_pet.ID, m_target_id), PetChoosetype.Advanced, limit_num);
                gameObject.SetActive(false);
        }


        public void OnAdvanced() //转职
        {
                if (CommonDataManager.GetInst().GetNowResourceNum("gold") < advanced_need_gold)
                {
                        GameUtility.PopupMessage("金钱不足！");
                        return;
                }

                if (!can_advance_item)
                {
                        GameUtility.PopupMessage("物品不足！");
                        return;
                }

                if (PetManager.GetInst().ChoosePetList.Count < really_need_num)
                {
                        GameUtility.PopupMessage("英雄不足！");
                        return;
                }


                if (IsFoodPetHasEquip())
                {
                        if (CommonDataManager.GetInst().IsBagFull())
                        {
                                GameUtility.PopupMessage("背包已满,请先清理！");
                                return;
                        }
                }

                string food = "";
                if (PetManager.GetInst().ChoosePetList.Count > 0)
                {
                        for (int i = 0; i < PetManager.GetInst().ChoosePetList.Count; i++)
                        {
                                food += PetManager.GetInst().ChoosePetList[i] + "|";
                        }
                }

                PetManager.GetInst().SendPetAdvance(m_pet.ID, food, m_target_id);
                

        }

        void ConfirmAdvanced(object data)
        {
                string food = (string)data;
                PetManager.GetInst().SendPetAdvance(m_pet.ID, food, m_target_id);
        }



        #endregion


        #region 特性

        readonly int max_specificity_num = 7;
        public GameObject specificity_good_obj;
        public GameObject specificity_bad_obj;
        public GameObject specificity_need_group;
        int select_specificity_id;

        public void RefreshSpecificity()
        {
                specificity_need_group.SetActive(false);
                RefreshSpecificity(Specificity_Type.GOOD_RANDOM, specificity_good_obj);
                RefreshSpecificity(Specificity_Type.BAD_RANDOM, specificity_bad_obj);
        }

        public void UpdateSpecificity()  //治病成功刷新
        {
                specificity_need_group.SetActive(false);
                RefreshSpecificity(Specificity_Type.BAD_RANDOM, specificity_bad_obj);
        }

        void RefreshSpecificity(Specificity_Type type, GameObject group)
        {
                List<Specificity_Struct> specificity_list = m_pet.GetMyPetSpecificity(type);
                for (int i = 0; i < max_specificity_num; i++)
                {
                        GameObject column = GetGameObject(group, i.ToString());
                        column.GetComponent<Image>().color = Color.white;
                        GameObject specificity = GetGameObject(column, "specificity");
                        if (i < specificity_list.Count)
                        {
                                SpecificityHold sfh = PetManager.GetInst().GetSpecificityCfg(specificity_list[i].id);

                                GetText(specificity, "name").text = LanguageManager.GetText(sfh.name); 
                                GetText(specificity, "des").text = LanguageManager.GetText(sfh.desc);
                                GetText(specificity, "des").color = Color.white;
                                GetText(specificity, "level").text = (sfh.quality).ToString();


                                EventTriggerListener.Get(column).onDown = ChooseSpecificity;
                                EventTriggerListener.Get(column).SetTag(specificity_list[i].id);
                                specificity.SetActive(true);
                        }
                        else
                        {
                                specificity.SetActive(false);
                        }
                }
        }

        void ChooseSpecificity(GameObject go, PointerEventData data)
        {
                int id = (int)EventTriggerListener.Get(go).GetTag();
                select_specificity_id = id;
                SpecificityHold sfh = PetManager.GetInst().GetSpecificityCfg(id);
                if (sfh.good_or_bad == 1) //有益不需要
                {
                        specificity_need_group.SetActive(false);
                }
                else
                {
                        RefreshNeed(sfh.require);
                }

                ResetSpecificityStyle();
                if (sfh.good_or_bad == 1)
                {
                        go.GetComponent<Image>().color = Color.blue;
                }
                else
                {
                        go.GetComponent<Image>().color = Color.red;
                }
                GetText(go, "des").color = Color.black;
        }

        void ResetSpecificityStyle()
        {
                for (int i = 0; i < max_specificity_num; i++)
                {
                        Image column0 = GetImage(specificity_good_obj, i.ToString());
                        GameObject specificity0 = GetGameObject(column0.gameObject, "specificity");
                        column0.color = Color.white;
                        GetText(specificity0, "des").color = Color.white;

                        Image column1 = GetImage(specificity_bad_obj, i.ToString());
                        GameObject specificity1 = GetGameObject(column1.gameObject, "specificity");
                        column1.color = Color.white;
                        GetText(specificity1, "des").color = Color.white;
                }
        }


        bool can_cure = true;
        void RefreshNeed(string need_info)  //刷新需求材料
        {
                can_cure = true;
                GameObject need_obj = GetGameObject(specificity_need_group, "need0");
                specificity_need_group.SetActive(true);
                string[] need = need_info.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                SetGameObjectHide("need", gameObject);

                int num = 0;
                string name = "";
                int id = 0;
                string des = "";
                Thing_Type type;


                for (int i = 0; i < need.Length; i++)
                {
                        GameObject temp = GetGameObject(gameObject, "need" + i);
                        if (temp == null)
                        {
                                temp = CloneElement(need_obj, "need" + i);
                        }

                        CommonDataManager.GetInst().SetThingIcon(need[i], temp.transform,null, out name, out num, out id, out des,out type);
                        int has_num = CommonDataManager.GetInst().GetThingNum(need[i], out num);
                        if (has_num >= num)
                        {
                                GetText(temp, "num").text = has_num + " / " + num;
                        }
                        else
                        {
                                can_cure = false;
                                GetText(temp, "num").text = "<color=red>" + has_num + "</color>" + " / " + num;
                        }
                        temp.SetActive(true);
                }
        }


        public void SendCure()  //发送治病
        {


                if (!can_cure)
                {
                        GameUtility.PopupMessage("材料不足！");
                }
                else
                {

                }
        }

        #endregion


        #region 装备

        bool IsFoodPetHasEquip()  //材料宠物是否存在装备//
        {
                for (int i = 0; i < PetManager.GetInst().ChoosePetList.Count; i++)
                {
                        if (PetManager.GetInst().ChoosePetList[i] != 0)
                        {
                                Pet pet = PetManager.GetInst().GetPet(PetManager.GetInst().ChoosePetList[i]);
                                if (pet.PetHasEquip()) //只要有宠物有装备就并且包裹容量达到上限就无法操作
                                {
                                        return true;
                                }
                        }
                }
                return false;
        }



        #endregion

}
