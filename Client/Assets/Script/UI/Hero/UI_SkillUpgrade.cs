using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class UI_SkillUpgrade : UIBehaviour
{
        public Transform HeroContent;
        GameObject m_InfoGroup;

        int m_nLevelLimit = 1;
        const int MAX_UPGRADE_COUNT = 6;
        const int MAX_SKILL_COUNT = 4;
        void Awake()
        {

                m_InfoGroup = GetGameObject("infogroup");
                m_InfoGroup.SetActive(false);
        }
        public void OnClickClose()
        {
                base.OnClickClose(null);
        }

        public void OnClickReset()
        {
                if (m_SelectHero != null)
                {
                        Experience expCfg = CharacterManager.GetInst().GetCharacterExp(m_SelectHero.GetPropertyInt("level"));
                        if (expCfg != null)
                        {
                                string text = string.Format(LanguageManager.GetText("skill_point_reset_hint"), expCfg.reset_skill_cost_diamond.ToString());
                                GameUtility.ShowConfirmWndEx(text, ConfirmReset, null, null);
                        }
                }
        }

        void ConfirmReset(object data)
        {
                if (m_SelectHero != null)
                {
                        Message.CSMsgSkillReset msg = new Message.CSMsgSkillReset();
                        msg.idPet = m_SelectHero.ID;
                        NetworkManager.GetInst().SendMsgToServer(msg);
                }
        }

        public void OnClickUpgrade(GameObject go)
        {
                GameUtility.ShowConfirmWnd("skill_level_up_hint", ConfirmUpgrade, null, go);
        }

        public void OnClickInfo()
        {
                m_InfoGroup.SetActive(!m_InfoGroup.activeSelf);
        }

        void ConfirmUpgrade(object data)
        {
                GameObject go = (GameObject)data;

                int cost = int.Parse(GetText(go, "cost").text);
                if (cost > PlayerController.GetInst().GetPropertyInt("gold"))
                {
                        GameUtility.PopupMessage(LanguageManager.GetText("gold_error_hint"));
                        return;
                }
                if (m_SelectHero.GetPropertyInt("skill_point") <= 0)
                {
                        GameUtility.PopupMessage(LanguageManager.GetText("skill_point_error_hint"));
                        return;
                }

                Skill_Struct ss = (Skill_Struct)EventTriggerListener.Get(go).GetTag();

                Message.CSMsgSkillLevelUp msg = new Message.CSMsgSkillLevelUp();
                msg.idPet = m_SelectHero.ID;
                msg.idSkill = ss.id;
                NetworkManager.GetInst().SendMsgToServer(msg);

        }

        public void Refresh(BuildInfo bi)
        {
                BuildingBookshelvesConfig hbc = HomeManager.GetInst().GetBookShelvesCfg(bi.buildCfg.id);
                if (hbc != null)
                {
                        m_nLevelLimit = hbc.skill_level_limit;
                }

                GetText(m_InfoGroup, "furnitureLevel").text = (bi.buildCfg.id % 100).ToString();
                GetText(m_InfoGroup, "skillLimit").text = m_nLevelLimit.ToString();
                GetText(m_InfoGroup, "booksCount").text = "0";
                GetText(m_InfoGroup, "costsave").text = PlayerController.GetInst().GetPropertyInt("skill_lower_consume_per") + "%";

                RefreshMyHeroList(null);
        }
        public void RefreshHero()
        {
                ChooseHero(m_SelectHero);
        }

        readonly int hero_init_num = 11;    //一屏能放的pet数
        Pet m_SelectHero = null;


        public void RefreshMyHeroList(Pet selecthero = null)
        {
                CreatHero(selecthero);
        }

        public void RefreshSkillPoint()
        {
                if (m_SelectHero != null)
                {
                        GetText("skillpoint").text = m_SelectHero.GetPropertyString("skill_point");
                }
        }

        List<Pet> m_herolist = new List<Pet>();
        void CreatHero(Pet selecthero)
        {
                m_herolist = PetManager.GetInst().GetMyPetByCareerAndRaceSort(0, 0);  //种族概念暂时废弃//
                if (selecthero == null)
                {
                        if (m_SelectHero == null)
                        {
                                if (m_herolist.Count > 0)
                                {
                                        m_SelectHero = m_herolist[0];
                                }
                        }
                }
                else
                {
                        m_SelectHero = selecthero;
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
                        }
                        UpdateHero(pet, hero_rtf);
                }

                (HeroContent as RectTransform).anchoredPosition = new Vector2(400, 0); //打开菜单有个弹出的效果//
                ChooseHero(m_SelectHero);
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
                        ChooseHero(pet);
                }
        }
        public void ChooseHero(Pet pet)
        {
                if (m_SelectHero != pet)
                {
                        m_SelectHero = pet;
                }

                if (m_SelectHero == null)
                {
                        return;
                }

                string icon = ModelResourceManager.GetInst().GetIconRes(m_SelectHero.m_CharacterCfg.modelid);

                GetText("heroname").text = LanguageManager.GetText(m_SelectHero.GetPropertyString("name"));
                GetText("careerdesc").text = LanguageManager.GetText(m_SelectHero.GetPropertyString("desc"));
                GetText("herolevel").text = "Lv." + pet.GetPropertyString("level");
                GetText("skillpoint").text = m_SelectHero.GetPropertyString("skill_point");

                ResourceManager.GetInst().LoadIconSpriteSyn(icon, GetImage("heroicon").transform);

                CareerConfigHold careerCfg = CharacterManager.GetInst().GetCareerDic(m_SelectHero.m_CharacterCfg.career);
                if (careerCfg != null)
                {
                        GetText("careername").text = LanguageManager.GetText(careerCfg.name);
                        ResourceManager.GetInst().LoadIconSpriteSyn(careerCfg.icon, GetImage("herocareer").transform);
                }

                SetStar(pet, GetGameObject("star_group").transform);

                List<Skill_Struct> skillList = m_SelectHero.GetBattleSkills();

                GameObject upgradeObj = null;
                GameObject lockObj = null;
                GameObject offObj = null;
                GameObject onObj = null;
                GameObject upgradeRoot = null;
                for (int idx = 0; idx < MAX_SKILL_COUNT; idx++)
                {
                        GameObject skillObj = GetGameObject("skill" + idx);
                        if (idx < skillList.Count)
                        {
                                Skill_Struct ss = skillList[idx];
                                GetGameObject(skillObj, "skilllock").SetActive(false);

                                SkillLearnConfigHold cfg = SkillManager.GetInst().GetSkillInfo(ss.config_id);
                                ResourceManager.GetInst().LoadIconSpriteSyn(cfg.icon, GetImage(skillObj, "skillicon").transform);

                                DelayPressButton dpb = skillObj.GetComponent<DelayPressButton>();
                                if (dpb == null)
                                {
                                        dpb = skillObj.AddComponent<DelayPressButton>();
                                        dpb.onPressDelay.AddListener(new UnityAction<GameObject>(OnSkillTips));
                                        dpb.onReleaseBeforeDelay.RemoveAllListeners();
                                }
                                else
                                {
                                        dpb.enabled = true;
                                }
                                EventTriggerListener.Get(skillObj).SetTag(ss);
                                EventTriggerListener.Get(skillObj).onClick = null;

                                for (int i = 1; i <= MAX_UPGRADE_COUNT; i++)
                                {
                                        upgradeRoot = GetGameObject(skillObj, "upgrade" + i);
                                        if (upgradeRoot != null)
                                        {
                                                EventTriggerListener.Get(upgradeRoot).SetTag(ss);
                                                DelayPressButton delayBtn = upgradeRoot.GetComponent<DelayPressButton>();
                                                if (delayBtn == null)
                                                {
                                                        delayBtn = upgradeRoot.AddComponent<DelayPressButton>();
                                                        delayBtn.onPressDelay.AddListener(new UnityAction<GameObject>(OnSkillTips));                                                        
                                                }
                                                delayBtn.enabled = true;
                                                delayBtn.onReleaseBeforeDelay.RemoveAllListeners();

                                                upgradeObj = GetGameObject(upgradeRoot, "upgrade");
                                                lockObj = GetGameObject(upgradeRoot, "upgradelock");
                                                offObj = GetGameObject(upgradeRoot, "off");
                                                onObj = GetGameObject(upgradeRoot, "on");

                                                bool bActive = i < ss.level && i < m_nLevelLimit;
                                                bool canUpgrade = i == ss.level && i < m_nLevelLimit;

                                                onObj.SetActive(bActive);
                                                offObj.SetActive(!bActive);

                                                upgradeObj.SetActive(canUpgrade);
                                                lockObj.SetActive(!bActive && !canUpgrade);
                                                
                                                if (canUpgrade)
                                                {
                                                        delayBtn.onReleaseBeforeDelay.AddListener(new UnityAction<GameObject>(OnClickUpgrade));

                                                        Text costVal = GetText(upgradeObj, "cost");
                                                        int cost = SkillManager.GetInst().GetSkillUpgradeCost(ss.level + 1, cfg.quality - 1);
                                                        cost = (int)(cost * (1f - PlayerController.GetInst().GetPropertyInt("skill_lower_consume_per") / 100f));
                                                        costVal.text = cost.ToString();
                                                }
                                                else if (!bActive)
                                                {
                                                        delayBtn.onReleaseBeforeDelay.AddListener(new UnityAction<GameObject>(OnClickUpgradeLock));
                                                }
                                        }
                                }
                        }
                        else
                        {
                                GetGameObject(skillObj, "skilllock").SetActive(true);

                                for (int i = 1; i <= MAX_UPGRADE_COUNT; i++)
                                {
                                        upgradeRoot = GetGameObject(skillObj, "upgrade" + i);
                                        if (upgradeRoot != null)
                                        {
                                                upgradeObj = GetGameObject(upgradeRoot, "upgrade");
                                                lockObj = GetGameObject(upgradeRoot, "upgradelock");
                                                offObj = GetGameObject(upgradeRoot, "off");
                                                onObj = GetGameObject(upgradeRoot, "on");
                                                onObj.SetActive(false);
                                                offObj.SetActive(true);

                                                upgradeObj.SetActive(false);
                                                lockObj.SetActive(true);

                                                DelayPressButton delayBtn = upgradeRoot.GetComponent<DelayPressButton>();
                                                if (delayBtn != null)
                                                {
                                                        delayBtn.enabled = false;
                                                }                                                
                                        }
                                }

                                DelayPressButton dpb = skillObj.GetComponent<DelayPressButton>();
                                if (dpb != null)
                                {
                                        dpb.enabled = false;
                                }
                                EventTriggerListener.Get(skillObj).onClick = OnClickSkillLock;
                        }
                }
        }

        public void OnClickSkillLock(GameObject go, PointerEventData data)
        {
                GameUtility.PopupMessage(LanguageManager.GetText("no_skill_lock_hint"));
        }
        public void OnSkillTips(GameObject go)
        {
                int level = 1;
                if (go.name.Contains("upgrade"))
                {
                        level = int.Parse(go.name.Replace("upgrade", "")) + 1;
                }
                Skill_Struct ss = (Skill_Struct)EventTriggerListener.Get(go).GetTag();

                UI_SkillTips uis = UIManager.GetInst().GetUIBehaviour<UI_SkillTips>();
                if (uis == null)
                {
                        uis = UIManager.GetInst().ShowUI<UI_SkillTips>();
                }

                Vector3 screenPos = CalcScreenPosition(go.GetComponent<RectTransform>());
                screenPos.x += 40f;
                screenPos.y -= 40f;
                uis.ShowSkillTip(ss.config_id, screenPos, level);

        }
        public void OnClickUpgradeLock(GameObject go)
        {
                GameUtility.PopupMessage(LanguageManager.GetText("skill_lock_hint"));                
        }

        void Update()
        {
                if (InputManager.GetInst().GetInputDown(true))
                {
                        UIManager.GetInst().CloseUI("UI_SkillTips");
                }
        }
}