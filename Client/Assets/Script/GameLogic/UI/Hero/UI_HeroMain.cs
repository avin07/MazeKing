using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Text;
using System;
using DG.Tweening;

public class UI_HeroMain : UIBehaviour
{

        public Transform HeroContent;
        public Transform HeroChooseState;
        public Transform EquipContent;
        public Sprite ExitsSprite;

        #region 基础
        public enum HERO_TAB
        {
                BASE = 0,         //基本
                SPECIFICITY = 1,  //特性
                ACHIEVE = 2,      //成就
                EQUIP = 3,
                MAX = 4,
        }

        HERO_TAB m_tab = HERO_TAB.BASE;
        Transform[] Groups = new Transform[(int)HERO_TAB.MAX];  //存储,防止经常查询//
        Transform[] Tabs = new Transform[(int)HERO_TAB.MAX];    //存储,防止经常查询//
        Transform Bg;
        Transform Page;
        Transform HeroDarg;

        void Init()
        {
                EventTriggerListener.Get(gameObject).onClick = OnClickTipClose;
                InitCommon();
                InitBase();
                InitSpecificity();
                InitAchievement();
                InitEquip();

                RefreshMyHeroList();
        }

        void OnClickTipClose(GameObject go, PointerEventData data)
        {
                ClickSkillTip();
                ClickEquipTip();
                ClickSpecificityTip();
        }

        private IEnumerator WaitForInit(float time)
        {
                yield return null; //等待一帧后ugui的世界坐标才能取得正确值。。。
                //base.OnShow(time);
                Init();
        }

        public void OnClickClose()
        {
                UIManager.GetInst().CloseUI(this.name);
                HomeManager.GetInst().ResetHomeCamera();
                PetManager.GetInst().ClearNewHeroList();
        }


        public override void OnShow(float time)
        {
                HomeManager.GetInst().SaveCameraData();
                UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", false);
                base.OnShow(time);
                AppMain.GetInst().StartCoroutine(WaitForInit(time));
        }

        private void OnTab(GameObject go, PointerEventData data)
        {
                HERO_TAB pt = (HERO_TAB)EventTriggerListener.Get(go).GetTag();
                if (pt != m_tab)
                {
                        Page.DOKill(true);  //简陋翻页效果
                        Page.SetActive(true);
                        Page.DORotate(Vector3.up * (-80), 0.3f).SetEase(Ease.InOutQuad).OnComplete(ResetPage);

                        m_tab = pt;
                        RefreshGroup();
                }
        }

        void ResetPage()
        {
                Page.SetActive(false);
                Page.transform.localEulerAngles = Vector3.zero;
        }

        public void RefreshGroup()
        {
                HightLightTab();
                switch (m_tab)
                {
                        case HERO_TAB.BASE:
                                RefreshBase();
                                break;
                        case HERO_TAB.SPECIFICITY:
                                RefreshSpecificity();
                                break;
                        case HERO_TAB.ACHIEVE:
                                RefreshAchievement();
                                break;
                        case HERO_TAB.EQUIP:
                                now_equip_part = ItemType.EQUIP_PART.MAX;
                                RefreshEquipGroup();
                                break;
                        default:
                                break;
                }
        }

        void InitCommon()
        {
                for (int i = 0; i < (int)HERO_TAB.MAX; i++)
                {
                        Groups[i] = transform.Find("Group" + i);
                        Tabs[i] = transform.Find("TabGroup/Tab" + i);

                        EventTriggerListener listener = Tabs[i].gameObject.AddComponent<EventTriggerListener>();
                        listener.onClick = OnTab;
                        listener.SetTag(i);
                }
                Bg = transform.Find("TabGroup/bg");
                HeroDarg = transform.Find("mod_drag");
                EventTriggerListener.Get(HeroDarg).onDrag = OnModelDarg;
                Page = transform.Find("page");
                Page.SetActive(false);
        }

        void HightLightTab()
        {
                int index = (int)m_tab;
                for (int i = 0; i < (int)HERO_TAB.MAX; i++)
                {
                        if (i == index)
                        {
                                Groups[i].SetActive(true);
                                Tabs[i].GetComponent<Image>().enabled = false;
                                Tabs[i].Find("on").GetComponent<Image>().enabled = true; 
                        }
                        else
                        {
                                Groups[i].SetActive(false);
                                Tabs[i].GetComponent<Image>().enabled = true;
                                Tabs[i].Find("on").GetComponent<Image>().enabled = false; 
                        }
                       
                }
                Bg.SetAsLastSibling();
                Tabs[index].SetAsLastSibling();
        }

        #endregion

        #region 英雄列表

        readonly int hero_init_num = 11;    //一屏能放的pet数
        Pet m_select_hero = null;

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
                FindComponent<Text>("num").text = "<color=#FDCE95>" + PetManager.GetInst().GetMyAllPetNum() + "</color><color=#7D7D7D>/" + CommonDataManager.GetInst().GetHeroBagLimit() + "</color>";
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
                        hero_rtf = GetChildByIndex(HeroContent,i);
                        if (hero_rtf == null)
                        {
                                hero_rtf = CloneElement(GetChildByIndex(HeroContent,0).gameObject).transform as RectTransform;
                        }
                        hero_rtf.name = i.ToString();
                        hero_rtf.SetActive(true);
                        Pet pet = null;
                        if (i < m_herolist.Count)
                        {
                                pet = m_herolist[i];
                                Button btn = hero_rtf.GetComponent<Button>();
                                btn.onClick.RemoveAllListeners();
                                btn.onClick.AddListener(() => OnHeroClick(btn.gameObject,pet));
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

                        GetGameObject(tf.gameObject, "newlabel").SetActive(PetManager.GetInst().IsNewHero(pet.ID));
                }

        }


        void OnHeroClick(GameObject go,Pet pet)
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

        GameObject hero_mod;
        float rotate_offset = -1.5f;
        void ChooseHero(Transform tf)
        {
                if (m_select_hero == null)
                {
                        return;
                }
                HomeManager.GetInst().ResetShowMod();
                hero_mod = HomeManager.GetInst().ChangeCameraForPet(m_select_hero.ID);
                HeroChooseState.SetActive(true);
                HeroChooseState.SetParent(tf);
                HeroChooseState.localPosition = Vector3.zero;
                HeroChooseState.localScale = Vector3.one;
                RefreshGroup();
        }

        void OnModelDarg(GameObject go, PointerEventData eventdata)
        {
                if (hero_mod != null)
                {
                        hero_mod.transform.Rotate(Vector3.up, eventdata.delta.x * rotate_offset);
                }
        }



        #endregion

        #region 信息

        Transform SkillGroup;
        Transform SkillTip;
        readonly string[] attr_name = { "total_max_hp", "total_attack", "total_defence", "total_speed", "critical_rate_per", "critical_damage_per" };

        void InitBase()
        {
                SkillGroup = Groups[(int)HERO_TAB.BASE].Find("skillgroup");
                SkillTip = Groups[(int)HERO_TAB.BASE].Find("skilltipgroup");

                FindComponent<Button>(Groups[(int)HERO_TAB.BASE], "trainbtn").onClick.AddListener(ClickTrain);
        }

        void ClickTrain()
        {

                int state = m_select_hero.GetPropertyInt("cur_state");
                if (state > (int)PetState.Normal)
                {
                        GameUtility.PopupMessage("该英雄无法操作");
                }
                else
                {
                        UIManager.GetInst().ShowUI<UI_PetMain>("UI_PetMain").RefreshMyHeroList(m_select_hero);
                        UIManager.GetInst().SetUIActiveState<UI_HeroMain>("UI_HeroMain", false);
                }
        }
   
        void RefreshBase()
        {
                SkillTip.SetActive(false);
                RefreshAttr();
                RefreshSkill();
        }

        void RefreshAttr()
        {
                Transform group = Groups[(int)HERO_TAB.BASE];

                int max_level = m_select_hero.GetPropertyInt("max_level");
                int level = m_select_hero.GetPropertyInt("level");

                FindComponent<Text>(group, "level").text = level.ToString();
                FindComponent<Text>(group, "max_level").text = max_level.ToString();

                if (level != max_level)
                {
                        long now_exp = m_select_hero.GetPropertyInt("exp");
                        long next_level_need_exp = CharacterManager.GetInst().GetCharacterExp(level + 1).need_exp;
                        FindComponent<Text>(group, "exp_bar/exp_num").text = now_exp + "/" + next_level_need_exp;
                        FindComponent<Image>(group, "exp_bar/exp_value").fillAmount = (float)now_exp / (float)next_level_need_exp;
                }
                else
                {
                        FindComponent<Text>(group, "exp_bar/exp_num").text = "MAX";
                        FindComponent<Image>(group, "exp_bar/exp_value").fillAmount = 1;
                }

                FindComponent<Text>(group, "name").text = m_select_hero.GetPropertyString("name");
                string career_url = HeroManager.GetInst().GetCareerIcon(m_select_hero.GetPropertyInt("career"));
                ResourceManager.GetInst().LoadIconSpriteSyn(career_url, group.Find("career_icon"));
//                 FighterProperty fp = m_select_hero.FighterProp;
//                 Transform attr_group = group.Find("attrgroup");
//                 for (int i = 0; i < attr_name.Length; i++)
//                 {
//                         Text text = FindComponent<Text>(attr_group.GetChild(i), "value");
//                         if (attr_name[i].Contains("per"))
//                         {
//                                 text.text = fp.GetTotalProp(attr_name[i]) +  "%";
//                         }
//                         else
//                         {
//                                 text.text = fp.GetTotalProp(attr_name[i]).ToString();
//                         }
//                 }
        }

        void RefreshSkill()
        {
                SetChildActive(SkillGroup, false);
                List<Skill_Struct> m_skill = m_select_hero.GetMySkill();
                for (int i = 0; i < m_skill.Count; i++)
                {
                        Skill_Struct ss = m_skill[i];
                        SkillLearnConfigHold sfh = SkillManager.GetInst().GetSkillInfo(ss.config_id);
                        if (sfh != null)
                        {
                                Transform m_skill_tf = null;
                                if (i < SkillGroup.childCount)
                                {
                                        m_skill_tf = SkillGroup.GetChild(i);
                                }
                                else
                                {
                                        m_skill_tf = CloneElement(SkillGroup.GetChild(0).gameObject).transform;
                                }
                                m_skill_tf.SetActive(true);
                                ResourceManager.GetInst().LoadIconSpriteSyn(SkillManager.GetInst().GetSkillIconUrl(ss.config_id), m_skill_tf);

                                EventTriggerListener listener = EventTriggerListener.Get(m_skill_tf.Find("btn"));
                                listener.onClick = OnSkill;
                                listener.SetTag(ss);
                                                             
                        }
                }
        }

        private void OnSkill(GameObject go, PointerEventData data)
        {
                Skill_Struct ss = (Skill_Struct)EventTriggerListener.Get(go).GetTag();
                SetTipGroup(ss, go.transform.parent.GetComponent<Image>().sprite);
        }

        void SetTipGroup(Skill_Struct ss,Sprite sprite)
        {
                SkillTip.SetActive(true);
                int skillId = ss.config_id;
                FindComponent<Text>(SkillTip,"skillname").text = SkillManager.GetInst().GetSkillName(skillId);
                FindComponent<Text>(SkillTip, "skilldesc").text = SkillManager.GetInst().GetSkillDesc(skillId);
                FindComponent<Text>(SkillTip, "skilllv").text = "Lv." + ss.level;
                FindComponent<Image>(SkillTip, "icon").sprite = sprite;
                
        }


        void ClickSkillTip()
        {
                if (SkillTip.gameObject.activeInHierarchy)
                {
                        if (EventSystem.current.currentSelectedGameObject == null)
                        {
                                SkillTip.SetActive(false);
                        }

                }
        }

        #endregion

        #region 怪癖

        Transform SpecificityTip;
        readonly int max_specificity_num = 7;

        void InitSpecificity()
        {
                SpecificityTip = Groups[(int)HERO_TAB.SPECIFICITY].Find("specificitytipgroup");
        }

        void RefreshSpecificity() 
        {
                SpecificityTip.SetActive(false);
                RefreshSpecificity(Specificity_Type.GOOD_RANDOM);
                RefreshSpecificity(Specificity_Type.BAD_RANDOM);
        }


        void RefreshSpecificity(Specificity_Type type)
        {
                int max_have = 0;
                Transform root = null;
                List<Specificity_Struct> specificity_list = m_select_hero.GetMyPetSpecificity(type);
                if (type == Specificity_Type.GOOD_RANDOM)
                {
                        max_have = m_select_hero.GetPropertyInt("random_helpful_behavior_quantity");
                        root = Groups[(int)HERO_TAB.SPECIFICITY].Find("good");
                }
                if (type == Specificity_Type.BAD_RANDOM)
                {
                        max_have = m_select_hero.GetPropertyInt("random_harmful_behavior_quantity");
                        root = Groups[(int)HERO_TAB.SPECIFICITY].Find("bad");
                }
                for (int i = 0; i < max_specificity_num; i++)
                {
                        Transform column = root.GetChild(i);
                        EventTriggerListener listener = EventTriggerListener.Get(column);
                        listener.onClick = null;
                        if (i < max_have)
                        {
                                if(i < specificity_list.Count)
                                {
                                          SpecificityHold sfh = PetManager.GetInst().GetSpecificityCfg(specificity_list[i].id);
                                          FindComponent<Text>(column, "name").text = LanguageManager.GetText(sfh.name);
                                          listener.onClick = OnSpecificity;
                                          listener.SetTag(sfh);
                                }
                                else
                                {
                                        FindComponent<Text>(column, "name").text = "";
                                }

                                column.SetActive(true);

                        }
                        else
                        {
                                column.SetActive(false);
                        }
                }
        }


        void ClickSpecificityTip()
        {
                if (SpecificityTip.gameObject.activeInHierarchy)
                {
                        if (EventSystem.current.currentSelectedGameObject == null)
                        {
                                SpecificityTip.SetActive(false);
                        }

                }
        }

        private void OnSpecificity(GameObject go, PointerEventData data)
        {
                SpecificityHold sh = (SpecificityHold)EventTriggerListener.Get(go).GetTag();
                SetSpecificity(sh);
        }

        void SetSpecificity(SpecificityHold sh)
        {
                SpecificityTip.SetActive(true);
                FindComponent<Text>(SpecificityTip, "skillname").text = LanguageManager.GetText(sh.name);
                FindComponent<Text>(SpecificityTip, "skilldesc").text = LanguageManager.GetText(sh.desc);


        }

        #endregion

        #region 成就

        Transform[] AchievementTF; //记录成就Transform//
        int[] AchievementSibling;  //记录成就层级//
        Transform AchievementTip;
        void InitAchievement()
        {
                Transform AchievementGroup = Groups[(int)HERO_TAB.ACHIEVE].Find("achievement");
                int num = AchievementGroup.childCount;
                AchievementTF = new Transform[num];
                AchievementSibling = new int[num];
                for (int i = 0; i < num; i++)
                {
                        AchievementTF[i] = AchievementGroup.Find(i.ToString());
                        AchievementSibling[i] = int.Parse(AchievementGroup.GetChild(i).name);
                }
                AchievementTip = Groups[(int)HERO_TAB.ACHIEVE].Find("achievement_tip");
        }

        public void RefreshAchievement()
        {
                AchievementTip.SetActive(false);

                List<int> m_ShowAchievement = m_select_hero.GetNeedShowAchievements();
                int AchievementID = 0;
                for (int i = 0; i < AchievementTF.Length; i++)
                {
                        if(i < m_ShowAchievement.Count)
                        {
                                AchievementID = m_ShowAchievement[i];
                                DisplayAchievement(AchievementID,AchievementTF[i]);
                                EventTriggerListener listener = EventTriggerListener.Get(AchievementTF[i]);
                                listener.onClick = OnAchievement;
                                listener.SetTag(AchievementID);
                                AchievementTF[i].SetActive(true);
                        }
                        else
                        {
                                AchievementTF[i].SetActive(false);
                        }

                }
        }


        private void OnAchievement(GameObject go, PointerEventData data)
        {
                //ResetAchievementSibling();
                go.transform.SetAsLastSibling(); //调整层级关系

                int achievement_id = (int)EventTriggerListener.Get(go).GetTag();
                RefreshAchievementTip(achievement_id);

        }

        void ResetAchievementSibling()
        {
                for (int i = 0; i < AchievementSibling.Length; i++)
                {
                        AchievementTF[AchievementSibling[i]].SetSiblingIndex(i);
                }
        }

        readonly int achievement_star_max = 3; // 星级上限
        void DisplayAchievement(int id,Transform tf)
        {
                CharactarAchievementConfig ca_cfg = PetManager.GetInst().GetAchievementCfg(id);
                if (ca_cfg != null)
                {
                        FindComponent<Text>(tf,"name").text = LanguageManager.GetText(ca_cfg.name); 
                        FindComponent<Text>(tf,"Tag").text = LanguageManager.GetText(ca_cfg.tag);
                        //星级
                        int max_star = ca_cfg.max_star;  //该成就星级上限
                        int current_star = ca_cfg.current_star;
                        Transform star = null;
                        for (int i = 0; i < achievement_star_max; i++)
                        {
                                star = tf.Find("stargroup").GetChild(i);
                                if (i < max_star)
                                {
                                        star.SetActive(true);
                                        if (i < current_star - 1) //亮的星级表示已经领取过的等级
                                        {
                                                ResourceManager.GetInst().LoadIconSpriteSyn("Achievement#hero_main_xing", star);
                                        }
                                        else
                                        {
                                                ResourceManager.GetInst().LoadIconSpriteSyn("Achievement#hero_main_xingxing", star);
                                        }
                                }
                                else
                                {
                                        star.SetActive(false);
                                }
                        }


                        //该任务系列达成//
                        Transform finish = tf.Find("finish");
                        finish.SetActive(false);
                        CharactarAchievementConfig ca_cfg_next = PetManager.GetInst().GetAchievementCfg(id + 1);
                        if (ca_cfg_next != null)
                        {
                                if (ca_cfg.series_id != ca_cfg_next.series_id) //当前成就是该系列成就中的最后一个
                                {
                                        if (m_select_hero.isAchievementGet(id))
                                        {
                                                for (int i = 0; i < max_star; i++)
                                                {
                                                        star = tf.Find("stargroup").GetChild(i);
                                                        ResourceManager.GetInst().LoadIconSpriteSyn("Achievement#hero_main_xing", star);
                                                }
                                                finish.SetActive(true);
                                        }
                                }
                        }
                        else
                        {
                                if (m_select_hero.isAchievementGet(id))
                                {
                                        for (int i = 0; i < max_star; i++)
                                        {
                                                star = tf.Find("stargroup").GetChild(i);
                                                ResourceManager.GetInst().LoadIconSpriteSyn("Achievement#hero_main_xing", star);
                                        }
                                        finish.SetActive(true);
                                }
                        }

                        if (ca_cfg.is_global == 1)
                        {
                                ResourceManager.GetInst().LoadIconSpriteSyn("Achievement#hero_main_geren", tf);
                        }
                        else
                        {
                                ResourceManager.GetInst().LoadIconSpriteSyn("Achievement#hero_main_putong", tf);
                        }

                        ResourceManager.GetInst().LoadIconSpriteSyn(ca_cfg.illustration, tf.Find("icon"));

                        ACHIEVE_TYPE type = (ACHIEVE_TYPE)ca_cfg.target_type;
                        switch (type)
                        {
                                case ACHIEVE_TYPE.LEVEL_UP:
                                        break;
                        }

                }
        }

        void RefreshAchievementTip(int achievement_id)
        {
                AchievementTip.SetActive(true);
                CharactarAchievementConfig ca_cfg = PetManager.GetInst().GetAchievementCfg(achievement_id);
                if (ca_cfg != null)
                {
                        ResourceManager.GetInst().LoadIconSpriteSyn(ca_cfg.illustration, AchievementTip.Find("icon"));
                        FindComponent<Text>(AchievementTip, "name").text = LanguageManager.GetText(ca_cfg.name);
                        FindComponent<Text>(AchievementTip, "des").text = LanguageManager.GetText(ca_cfg.describe);
                        FindComponent<Text>(AchievementTip, "biography").text = LanguageManager.GetText(ca_cfg.biography);

                        string reward = ca_cfg.reward;
                        int num = 0;
                        string name = "";
                        int id = 0;
                        string des = "";
                        Thing_Type type;
                        CommonDataManager.GetInst().SetThingIcon(reward, AchievementTip.Find("reward_icon"), null,out name, out num, out id, out des, out type);
                        FindComponent<Text>(AchievementTip, "reward_num").text = num.ToString();

                        string target_para_list = ca_cfg.target_para_list;
                        int now_exp = 0;
                        int max_exp = 1;


                        ACHIEVE_TYPE achieve_type = (ACHIEVE_TYPE)ca_cfg.target_type;
                        switch (achieve_type)
                        {
                                case ACHIEVE_TYPE.LEVEL_UP:
                                        now_exp = m_select_hero.GetPropertyInt("level");
                                        max_exp = int.Parse(target_para_list);
                                        break;
                                case ACHIEVE_TYPE.STAR_UP:
                                        now_exp = m_select_hero.GetPropertyInt("star");
                                        max_exp = int.Parse(target_para_list);
                                        break;
                                case ACHIEVE_TYPE.SKILL_ONE_UP:
                                        now_exp = m_select_hero.GetMaxSkillLevel();
                                        max_exp = int.Parse(target_para_list);
                                        break;
                                case ACHIEVE_TYPE.TRANSFER:
                                        if (m_select_hero.GetPropertyInt("career") == int.Parse(target_para_list))
                                        {
                                                now_exp = 1;
                                        }
                                        break;
                                case ACHIEVE_TYPE.SKILL_SPECIFY_UP:
                                        if (!target_para_list.Contains(","))
                                        {
                                                singleton.GetInst().ShowMessage(ErrorOwner.designer, "成就" + achievement_id + "目标参数格式错误");
                                        }
                                        else
                                        {
                                                string[] skill_temp = target_para_list.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                                int skill_id = int.Parse(skill_temp[0]);
                                                now_exp = m_select_hero.GetSkillLevel(skill_id);
                                                max_exp = int.Parse(skill_temp[1]);
                                        }
                                        break;
                                case ACHIEVE_TYPE.BEHAVIOR_GET:
                                        if (m_select_hero.HasSpecificity(int.Parse(target_para_list)))
                                        {
                                                now_exp = 1;
                                        }
                                        break;
                                case ACHIEVE_TYPE.BEHAVIOR_COUNT:
                                        if (!target_para_list.Contains(","))
                                        {
                                                singleton.GetInst().ShowMessage(ErrorOwner.designer, "成就" + achievement_id + "目标参数格式错误");
                                        }
                                        else
                                        {
                                                string[] specificity_temp = target_para_list.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                                Specificity_Type specificity_type = (Specificity_Type)int.Parse(specificity_temp[0]);
                                                now_exp = m_select_hero.GetSpecificityNum(specificity_type);
                                                max_exp = int.Parse(specificity_temp[1]);
                                        }
                                        break;
                                case ACHIEVE_TYPE.RAID_COMPLETE_SELF:
                                case ACHIEVE_TYPE.RAID_COMPLETE:
                                        if (m_select_hero.HasFinishAchieve(achievement_id)) //服务器通知的
                                        {
                                                now_exp = 1;
                                        }
                                        break;
                                default:
                                        break;

                        }


                        Button get = FindComponent<Button>(AchievementTip, "get");
                        if (!m_select_hero.isAchievementGet(achievement_id))
                        {
                                get.gameObject.SetActive(true);
                                get.interactable = now_exp >= max_exp ? true : false;
                                EventTriggerListener listener = EventTriggerListener.Get(get.gameObject);

                                listener.onClick = OnGetAchievement;
                                listener.SetTag(achievement_id);
                                FindComponent<Text>(AchievementTip, "exp_bar/exp_num").text = now_exp + "/" + max_exp;
                                FindComponent<Image>(AchievementTip, "exp_bar/exp_value").fillAmount = (float)now_exp / (float)max_exp;
                        }
                        else
                        {
                                get.gameObject.SetActive(false);
                                FindComponent<Text>(AchievementTip, "exp_bar/exp_num").text = "已完成";
                                FindComponent<Image>(AchievementTip, "exp_bar/exp_value").fillAmount = 1;
                        }


                }
        }


        private void OnGetAchievement(GameObject go, PointerEventData data)
        {
                int achievement_id = (int)EventTriggerListener.Get(go).GetTag();
                PetManager.GetInst().SendGetAchieve(m_select_hero.ID, achievement_id);
        }

        #endregion
        #region 装备

        List<Equip> m_equip_list = new List<Equip>();
        Sprite NoExitsSprite;
        ItemType.EQUIP_PART now_equip_part = ItemType.EQUIP_PART.MAX;

        public void UpdateHeroEquipUI()
        {
                if (m_tab == HERO_TAB.EQUIP)
                {
                        RefreshEquipGroup();
                }
        }

        void RefreshEquipGroup()
        {
                BodyEquipTip.SetActive(false);
                BagEquipTip.SetActive(false); 
                RefreshBagEquip();
                RefreshHeroEquip();
        }

        void RefreshBagEquip()
        {
                float content_x = (EquipContent as RectTransform).anchoredPosition.x;
                m_equip_list = EquipManager.GetInst().GetEquipInBagByPartBySort(now_equip_part);
                SetChildActive(EquipContent, false);
                GameObject original = EquipContent.GetChild(0).gameObject;
                for (int i = 0; i < m_equip_list.Count; i++)
                {
                        Transform temp = null;
                        if (i < EquipContent.childCount)
                        {
                                temp = EquipContent.GetChild(i);
                        }
                        else
                        {
                                temp = CloneElement(original).transform;
                        }

                        Image quality_image = FindComponent<Image>(temp, "quality");
                        Image icon_image = FindComponent<Image>(temp, "icon");
                        Text num_down = FindComponent<Text>(temp, "num_down");

                        Equip equip = m_equip_list[i];
                        EquipHoldConfig ec = EquipManager.GetInst().GetEquipCfg(equip.equip_configid);
                        if (ec != null)
                        {
                                int quality = ec.quality;
                                if (quality > 1)
                                {
                                        ResourceManager.GetInst().LoadIconSpriteSyn("Bg#quality" + quality, quality_image.transform);
                                }
                                else
                                {
                                        quality_image.enabled = false;
                                }
                                ResourceManager.GetInst().LoadIconSpriteSyn(ec.icon, icon_image.transform);
                                num_down.text = "";   //强化
                        }


                        Button btn = temp.GetComponent<Button>();
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => OnBagEquipClick(equip));
                        temp.SetActive(true);
                }
                (EquipContent as RectTransform).anchoredPosition = new Vector2(content_x, -50); //打开菜单有个弹出的效果//
        }

        void OnBagEquipClick(Equip equip)
        {
                ShowBagEquipTip(equip);

        }

        float refer_pos0_x;
        float refer_pos1_x;
        float equip_tip_offset_x = -5.0f;
        Transform BodyEquipTip;
        Transform BagEquipTip;
        Transform PartGroup;
        void InitEquip()
        {

                PartGroup = Groups[(int)HERO_TAB.EQUIP].Find("partgroup");
                BodyEquipTip = Groups[(int)HERO_TAB.EQUIP].Find("body_equip_info");
                BagEquipTip = Groups[(int)HERO_TAB.EQUIP].Find("bag_equip_info");

                for (int i = 0; i < PartGroup.childCount; i++)
                {
                        int index = i + (int)ItemType.EQUIP_PART.MAIN_HAND;
                        Button btn = PartGroup.GetChild(i).GetComponent<Button>();
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => OnHeroEquipClick(index));
                        if (i == 0)
                        {
                                NoExitsSprite = btn.image.sprite;
                        }
                }

                //坐标参照物//
                Transform refer0 = PartGroup.GetChild(0); 
                Transform refer1 = PartGroup.GetChild(1);
                Vector2 pos0 = refer0.TransformToCanvasLocalPosition(Groups[(int)HERO_TAB.EQUIP] , BodyEquipTip);
                Vector2 pos1 = refer1.TransformToCanvasLocalPosition(Groups[(int)HERO_TAB.EQUIP] , BagEquipTip );

                //坐标修正//
                refer_pos0_x = pos0.x - (refer0 as RectTransform).sizeDelta.x * 0.5f + (BodyEquipTip as RectTransform).sizeDelta.x * 0.5f;
                refer_pos1_x = (pos0.x + pos1.x) * 0.5f;

        }


        void RefreshHeroEquip()
        {
                int index = 0;
                for (ItemType.EQUIP_PART i = ItemType.EQUIP_PART.MAIN_HAND; i < ItemType.EQUIP_PART.MAX; i++)
                {
                        Transform equip = PartGroup.GetChild(index);
                     
                        Equip m_equip = m_select_hero.GetMyEquipPart(i);
                        SetHeroEquip(equip, m_equip);
                        index++;
                        if (m_equip == null)
                        {
                                if (i == ItemType.EQUIP_PART.OFF_HAND)  //双手武器
                                {
                                        Equip main_hand = m_select_hero.GetMyEquipPart(ItemType.EQUIP_PART.MAIN_HAND);
                                        if (main_hand != null)
                                        {
                                                EquipHoldConfig ec = EquipManager.GetInst().GetEquipCfg(main_hand.equip_configid);
                                                if (ec.is_two_hands == 1)
                                                {
                                                        Transform equip_another = PartGroup.GetChild(1);
                                                        SetHeroEquip(equip_another, main_hand,true);
                                                }
                                        }
                                }
                        }
                }
        }

        void SetHeroEquip(Transform equip, Equip m_equip, bool is_two_hands = false)
        {
                Image quality_image = FindComponent<Image>(equip, "quality");
                Image icon_image = FindComponent<Image>(equip, "icon");
                Text name = FindComponent<Text>(equip, "name");

                if (m_equip == null)
                {
                        quality_image.enabled = false;
                        icon_image.enabled = false;
                        name.gameObject.SetActive(true);
                        equip.GetComponent<Image>().sprite = NoExitsSprite;
                }
                else
                {
                        EquipHoldConfig ec = EquipManager.GetInst().GetEquipCfg(m_equip.equip_configid);
                        if (ec != null)
                        {
                                int quality = ec.quality;
                                if (quality > 1)
                                {
                                        ResourceManager.GetInst().LoadIconSpriteSyn("Bg#quality" + quality, quality_image.transform);
                                }
                                else
                                {
                                        quality_image.enabled = false;
                                }
                                ResourceManager.GetInst().LoadIconSpriteSyn(ec.icon, icon_image.transform);
                                name.gameObject.SetActive(false);
                                equip.GetComponent<Image>().sprite = ExitsSprite;
                        }
                }

                if (is_two_hands)
                {
                        icon_image.color = new Color(1, 1, 1, 0.5f);
                }
                else
                {
                        icon_image.color = new Color(1, 1, 1, 1);
                }

        }

        void OnHeroEquipClick(int index)
        {
                //now_equip_part = (ItemType.EQUIP_PART)index;
                //RefreshBagEquip();
                ShowBodyEquipTip((ItemType.EQUIP_PART)index);
        }

        void ShowBodyEquipTip(ItemType.EQUIP_PART part)
        {
                Equip m_equip = m_select_hero.GetMyEquipPart(part);
                if (m_equip != null)
                {
                        ShowEquipTip(m_equip, BodyEquipTip);
                        (BodyEquipTip as RectTransform).SetAnchoredPositionX(refer_pos1_x);
                        BodyEquipTip.SetActive(true);
                        ShowEquipTip(m_equip, BodyEquipTip);
                        BagEquipTip.SetActive(false);
                }
        }

        void ShowBagEquipTip(Equip equip)
        {
                BagEquipTip.SetActive(true);
                ShowEquipTip(equip, BagEquipTip);
                EquipHoldConfig ec = EquipManager.GetInst().GetEquipCfg(equip.equip_configid);
                int part = ec.place;
                Equip m_equip = m_select_hero.GetMyEquipPart((ItemType.EQUIP_PART)part);  //穿戴的装备
                if (m_equip != null)
                {
                        (BodyEquipTip as RectTransform).SetAnchoredPositionX(refer_pos0_x);
                        (BagEquipTip as RectTransform).SetAnchoredPositionX((BodyEquipTip as RectTransform).sizeDelta.x + equip_tip_offset_x + (BodyEquipTip as RectTransform).anchoredPosition.x);
                        BodyEquipTip.SetActive(true);
                        ShowEquipTip(m_equip, BodyEquipTip);
                }
                else
                {
                        (BagEquipTip as RectTransform).SetAnchoredPositionX(refer_pos1_x);
                        BodyEquipTip.SetActive(false);
                }

        }

        void ShowEquipTip(Equip equip, Transform root)
        {
                EquipHoldConfig ec = EquipManager.GetInst().GetEquipCfg(equip.equip_configid);
                if (ec != null)
                {
                        Image quality_image = FindComponent<Image>(root, "quality");
                        Image icon_image = FindComponent<Image>(root, "icon");
                        Text name = FindComponent<Text>(root, "name");
                        int quality = ec.quality;
                        if (quality > 1)
                        {
                                ResourceManager.GetInst().LoadIconSpriteSyn("Bg#quality" + quality, root.Find("quality"));
                        }
                        else
                        {
                                quality_image.enabled = false;
                        }
                        ResourceManager.GetInst().LoadIconSpriteSyn(ec.icon, icon_image.transform);
                        name.text = LanguageManager.GetText(ec.name);
                        FindComponent<Text>(root, "part").text = ItemType.part_des[ec.place - (int)ItemType.EQUIP_PART.MAIN_HAND];
                        FindComponent<Text>(root, "is_two_hands").text = ec.is_two_hands > 0 ? "是" : "否";
                        FindComponent<Text>(root, "score").text = ec.score.ToString();
                        FindComponent<Text>(root, "des").text = LanguageManager.GetText(ec.desc);

                        //需求等级
                        int now_level = m_select_hero.GetPropertyInt("level");
                        int need_level = ec.need_level;
                        Text level_text = FindComponent<Text>(root, "level_text");
                        Text need_level_text = FindComponent<Text>(root, "need_level");
                        bool bLevel = false;
                        if(now_level < need_level)
                        {
                                level_text.color = Color.red;
                                need_level_text.color = Color.red; 
                        }
                        else
                        {
                                level_text.color = Color.black;
                                need_level_text.color = Color.black; 
                                bLevel = true;
                        }
                        need_level_text.text = need_level.ToString();

                        //需求职业
                        bool bCareer = false;
                        Transform need_career = root.Find("need_career");
                        SetChildActive(need_career, false);
                        GameObject original_career = need_career.GetChild(0).gameObject;
                        string[] career = ec.need_career.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < career.Length; i++)
                        {
                                int career_id = int.Parse(career[i]);
                                if (career_id == m_select_hero.GetPropertyInt("career") || career_id <= 0) //职业吻合
                                {
                                        bCareer = true;
                                }
                                Transform temp = null;
                                if (i < need_career.childCount)
                                {
                                        temp = need_career.GetChild(i);
                                }
                                else
                                {
                                        temp = CloneElement(original_career).transform;
                                }
                                if (career_id == -1)
                                {
                                        ResourceManager.GetInst().LoadIconSpriteSyn("Career#quanbu", temp);
                                }
                                else
                                {
                                        string career_url = HeroManager.GetInst().GetCareerIcon(career_id);
                                        ResourceManager.GetInst().LoadIconSpriteSyn(career_url, temp);
                                }
                                temp.SetActive(true);                         
                        }
                        if (bCareer)
                        {
                                FindComponent<Text>(root, "career_text").color = new Color32(60,42,22,255);
                        }
                        else
                        {
                                FindComponent<Text>(root, "career_text").color = Color.red;
                        }

                        Transform attr_group = root.Find("attradd");
                        GameObject original_attr = attr_group.GetChild(0).gameObject;
                        SetChildActive(attr_group, false);
                        string[] attr_value = ec.attributes.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < attr_value.Length; i++)
                        {
                                Transform temp = null;
                                if (i < attr_group.childCount)
                                {
                                        temp = attr_group.GetChild(i);
                                }
                                else
                                {
                                        temp = CloneElement(original_attr).transform;
                                }
                                string[] attr = attr_value[i].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                FindComponent<Text>(temp, "name").text = CharacterManager.GetInst().GetPropertyMark(int.Parse(attr[0]));
                                string attr_name = CharacterManager.GetInst().GetPropertyName(int.Parse(attr[0]));
                                if(attr_name.Contains("per"))
                                {
                                        FindComponent<Text>(temp, "value").text = "+" +  attr[1] + "%";
                                }
                                else
                                {
                                        FindComponent<Text>(temp, "value").text = "+" + attr[1];
                                }
                                temp.SetActive(true);
                        }


                        Button btn = FindComponent<Button>(root, "btn");
                        if (bCareer && bLevel)
                        {
                                btn.interactable = true;
                        }
                        else
                        {
                                btn.interactable = false;
                        }
                        if (root == BagEquipTip) //穿戴或替换
                        {
                                btn.onClick.RemoveAllListeners();
                                btn.onClick.AddListener(() => PutOn(equip));
                        }
                        if (root == BodyEquipTip)//拖下
                        {
                                btn.onClick.RemoveAllListeners();
                                btn.onClick.AddListener(() => TakeOff(equip));
                        }
                }              
        }


        public void PutOn(Equip equip)
        {
                EquipHoldConfig ec = EquipManager.GetInst().GetEquipCfg(equip.equip_configid);
                if (BodyEquipTip.gameObject.activeSelf)
                {
                        UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox").SetConfirmAndCancel("提示", "更换该装备会使原装备消失，是否继续？", ConfirmPutOn, null, equip);
                }
                else
                {
                        if (ec.place == (int)ItemType.EQUIP_PART.OFF_HAND)  //处理双手武器//fuck//
                        {
                                Equip main_hand = m_select_hero.GetMyEquipPart(ItemType.EQUIP_PART.MAIN_HAND);
                                if (main_hand != null)
                                {
                                        EquipHoldConfig main_hand_ec = EquipManager.GetInst().GetEquipCfg(main_hand.equip_configid);
                                        if (main_hand_ec.is_two_hands == 1)
                                        {
                                                UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox").SetConfirmAndCancel("提示", "更换该装备会使原来的双手武器消失，是否继续？", ConfirmPutOn, null, equip);
                                                return;
                                        }
                                }
                        }
                        ConfirmPutOn(equip);
                }             
        }

        void ConfirmPutOn(object data)
        {
                Equip equip = (Equip)data;
                EquipHoldConfig ec = EquipManager.GetInst().GetEquipCfg(equip.equip_configid);
                EquipManager.GetInst().SendPutOn(m_select_hero.ID, equip.id, ec.place);
        }

        public void TakeOff(Equip equip)
        {
                if (CommonDataManager.GetInst().IsBagFull())
                {
                        GameUtility.PopupMessage("背包已经满了,请先清理！");
                        return;
                }

                EquipHoldConfig ec = EquipManager.GetInst().GetEquipCfg(equip.equip_configid);
                string[] info = ec.unload_cost.Split(',');
                string name = LanguageManager.GetText( CommonDataManager.GetInst().GetResourcesCfg(int.Parse(info[0])).name);
                UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox").SetConfirmAndCancel("提示", "脱下需要消耗" + info[1] + name + "，是否拖下？", ConfirmTakeOff, null, equip);

        }

        void ConfirmTakeOff(object data)
        {
                Equip equip = (Equip)data;
                EquipManager.GetInst().SendTakeOff(m_select_hero.ID, equip.id);
        }


        void ClickEquipTip()
        {
                if (BagEquipTip.gameObject.activeInHierarchy || BodyEquipTip.gameObject.activeInHierarchy)
                {
                        if (EventSystem.current.currentSelectedGameObject == null)
                        {
                                BodyEquipTip.SetActive(false);
                                BagEquipTip.SetActive(false); 
                        }
                }
        }

        #endregion

}
