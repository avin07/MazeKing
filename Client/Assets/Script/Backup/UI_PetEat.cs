//using UnityEngine;
//using System;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using System.Collections.Generic;
//using System.Collections;

//public class UI_PetEat : UIBehaviour
//{

//        public GameObject left;
//        private long select_pet_id = 0;
//        static public readonly int MAX_CARD_NUM = 8;
//        Sprite staroff;
//        List<long> m_FoodPetList = new List<long>(); //选中的材料宠物//
//        private Pet m_pet
//        {
//                get
//                {
//                        return PetManager.GetInst().GetPet(select_pet_id);
//                }
//        }


//        public enum PET_EAT_TAB
//        {
//                LEVELUP = 0,    //升级界面
//                TRANSFORM = 1,  //转职的界面
//                SPECIFICITY = 2,     //特性
//                EQUIP = 3,     //装备
//                MAX = 4,
//        }

//        PET_EAT_TAB m_tab = PET_EAT_TAB.LEVELUP;
//        GameObject[] Groups = new GameObject[(int)PET_EAT_TAB.MAX];  //存储,防止经常查询//
//        string help = "";
//        Sprite jia;
//        Image slide0;
//        Image slide1;
//        void Awake()
//        {
//                ModelCamera = GameObject.Find("UIModleCamera");
//                for (int i = 0; i < (int)PET_EAT_TAB.MAX; i++)
//                {
//                        Toggle tl = GetToggle(gameObject, "Tab" + i);
//                        EventTriggerListener.Get(tl.gameObject).onClick = OnTab;
//                        EventTriggerListener.Get(tl.gameObject).SetTag(i);
//                        GameObject group = GetGameObject("Group" + (int)i);
//                        Groups[i] = group;
//                }
//                staroff = GetImage(left, "head").sprite;
//                for (int i = 0; i < MAX_CARD_NUM; i++)
//                {
//                        m_FoodPetList.Add(0);
//                }
//                Button card = GetButton(Groups[0], "card0");
//                help = GetText(card.gameObject, "Text").text;
//                jia = GetImage(card.gameObject, "Image").sprite;
//                slide0 = GetImage(Groups[0].gameObject, "slide0");  //原本的经验条
//                slide1 = GetImage(Groups[0].gameObject, "slide1");

//                InitEquip();

//        }

//        public void GetFoodList(List<long> food_list)
//        {
//                for (int i = 0; i < MAX_CARD_NUM; i++)
//                {
//                        if (i < food_list.Count)
//                        {
//                                m_FoodPetList[i] = food_list[i];
//                        }
//                        else
//                        {
//                                m_FoodPetList[i] = 0;
//                        }
//                }
//        }

//        public void ClearFoodList()
//        {
//                for (int i = 0; i < MAX_CARD_NUM; i++)
//                {
//                        m_FoodPetList[i] = 0;
//                }
//        }


//        public void RefreshMyPetEat(long m_pet_id)
//        {
//                select_pet_id = m_pet_id;

//                RefreshLeft();
//                RefreshTab();
//                RefreshLevelUp();
//        }

//        public void RefreshMyPetLevelSucess()
//        {
//                ClearFoodList();
//                RefreshLeft();
//                RefreshTab();
//                RefreshLevelUp();
//        }

//        public void RefreshGroup(PET_EAT_TAB tab)
//        {
//                m_tab = tab;
//                for (int i = 0; i < (int)PET_EAT_TAB.MAX; i++)
//                {
//                        if ((int)i == (int)tab)
//                        {
//                                Groups[i].SetActive(true);
//                        }
//                        else
//                        {
//                                Groups[i].SetActive(false);
//                        }

//                }
//                HightLightTab(tab);
//                switch (tab)
//                {
//                        case PET_EAT_TAB.LEVELUP:
//                                RefreshLevelUp();
//                                break;
//                        case PET_EAT_TAB.TRANSFORM:
//                                RefreshTransform();
//                                break;
//                        case PET_EAT_TAB.SPECIFICITY:
//                                RefreshSpecificityGroup();
//                                break;
//                        case PET_EAT_TAB.EQUIP:
//                                RefreshEquip();
//                                break;
//                        default:
//                                break;
//                }
//        }


//        public void RefreshLeft()
//        {
//                GetText(left, "name").text = m_pet.GetPropertyString("name");
//                GetText(left, "level").text = m_pet.GetPropertyString("level");
//                ResourceManager.GetInst().LoadCharactorIcon(ModelResourceManager.GetInst().GetIconRes(m_pet.GetPropertyInt("model_id")), GetImage(left, "head").transform);
//                SetStar(m_pet.GetPropertyInt("star"), m_pet.GetPropertyInt("max_star"), left);
//                RefreshModel();


//                string[] target = m_pet.GetPropertyString("target").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
//                if (m_target_id == 0)
//                {
//                        m_target_id = int.Parse(target[0]);
//                }
//        }

//        ModelLoadBehaviour mlb;
//        GameObject ModelCamera;
//        public void RefreshModel()
//        {
//                GameObject obj = GetGameObject(left, "head");
//                obj.GetComponent<Image>().color = new Color32(0, 0, 0, 0);

//                mlb = ModelCamera.GetComponent<ModelLoadBehaviour>();
//                if (mlb == null)
//                {
//                        mlb = ModelCamera.AddComponent<ModelLoadBehaviour>();
//                }
//                else //有就删除模型//
//                {
//                        mlb.Reset();
//                }

//                CharacterConfig characterCfg = CharacterManager.GetInst().GetCharacterCfg(m_pet.CharacterID);
//                if (characterCfg != null)
//                {
//                        mlb.LoadModel("Models/Character/", characterCfg.modelid, obj);
//                        mlb.myModelOperte = mlb.MyModleOperteForUI;
//                }
//        }


//        void RefreshTab()
//        {
//                GetToggle(gameObject, "Tab" + (int)PET_EAT_TAB.TRANSFORM).interactable = m_pet.GetPropertyInt("level") == m_pet.GetPropertyInt("max_level") ? true : false;
//        }

//        void HightLightTab(PET_EAT_TAB tab)
//        {
//                GetToggle(gameObject, "Tab" + (int)tab).isOn = true;
//        }

//        void RefreshLevelUp()
//        {
//                RefreshList(PET_EAT_TAB.LEVELUP);
//                RefreshLevelUpRight();
//                RefreshLevelupBottom();
//        }

//        void RefreshTransform()
//        {
//                RefreshList(PET_EAT_TAB.TRANSFORM);
//                RefreshTransformKind();
//                RefreshTransformRight();
//                RefreshTransformBottom();
//        }

//        void RefreshLevelUpRight()
//        {
//                int level = m_pet.GetPropertyInt("level");
//                if (level == m_pet.GetPropertyInt("max_level"))
//                {
//                        GetText(Groups[0].gameObject, "level").text = "LV." + level + "/" + level;
//                        GetText(Groups[0].gameObject, "cost").text = "??";
//                        GetText(Groups[0].gameObject, "get").text = "";
//                        slide0.rectTransform.sizeDelta = new Vector2(0, slide0.rectTransform.sizeDelta.y);
//                        slide1.rectTransform.sizeDelta = new Vector2(0, slide1.rectTransform.sizeDelta.y);
//                        return;
//                }
//                long now_exp = m_pet.GetPropertyLong("exp");
//                long get_exp = GetFoodExp();
//                GetText(Groups[0].gameObject, "cost").text = "??";
//                GetText(Groups[0].gameObject, "get").text = get_exp.ToString();

//                float length = GetImage(Groups[0].gameObject, "exp").rectTransform.sizeDelta.x; //自己用长度实现经验条
//                long next_level_need_exp = CharacterManager.GetInst().GetCharacterExp(level + 1).need_exp;
//                float slide0_length = length * (float)now_exp / (float)next_level_need_exp;
//                slide0.rectTransform.sizeDelta = new Vector2(slide0_length, slide0.rectTransform.sizeDelta.y);
//                slide1.rectTransform.anchoredPosition = new Vector2(slide0_length, slide0.rectTransform.anchoredPosition.y);
//                slide1.rectTransform.sizeDelta = new Vector2(0, slide1.rectTransform.sizeDelta.y);
//                GetText(Groups[0].gameObject, "level").text = "LV." + level + "/" + m_pet.GetPropertyInt("max_level");

//                if (get_exp > 0)
//                {
//                        long all_exp = GetFoodExp() + now_exp + CharacterManager.GetInst().GetCharacterExp(m_pet.GetPropertyInt("level")).total_exp;
//                        int level_to = CharacterManager.GetInst().CanLevelUp(all_exp); //能升级到的等级


//                        if (CharacterManager.GetInst().GetCharacterExp(level_to + 1) != null)
//                        {
//                                long next_level_to_need = CharacterManager.GetInst().GetCharacterExp(level_to + 1).need_exp; //
//                                long next_level_ower = all_exp - CharacterManager.GetInst().GetCharacterExp(level_to).total_exp;
//                                float length0 = 0;
//                                float length1 = 0;
//                                if (level_to == level)
//                                {
//                                        length0 = length * (float)next_level_ower / (float)next_level_to_need - slide0_length;
//                                }
//                                else
//                                {
//                                        length0 = length - slide0_length;
//                                }
//                                length1 = length * (float)next_level_ower / (float)next_level_to_need;

//                                StartCoroutine(ExpAnim(length0, length1, level_to, level));
//                        }
//                        else
//                        {
//                                StartCoroutine(ExpAnim(length, 0, level_to, level));
//                        }


//                }

//        }

//        IEnumerator ExpAnim(float length0, float length1, int level_to, int level)
//        {
//                float time = Time.realtimeSinceStartup;

//                while (Time.realtimeSinceStartup - time <= 1.5f)
//                {
//                        float x = Mathf.Lerp(0, length0, (Time.realtimeSinceStartup - time) / 1.5f);
//                        slide1.rectTransform.sizeDelta = new Vector2(x, slide1.rectTransform.sizeDelta.y);
//                        yield return null;
//                }

//                if (level_to > level && level_to != m_pet.GetPropertyInt("max_level"))
//                {
//                        slide0.rectTransform.sizeDelta = new Vector2(0, slide0.rectTransform.sizeDelta.y);
//                        slide1.rectTransform.anchoredPosition = Vector2.zero;
//                        GetText(Groups[0].gameObject, "level").text = "LV." + level_to + "/" + m_pet.GetPropertyInt("max_level");
//                        time = Time.realtimeSinceStartup;
//                        while (Time.realtimeSinceStartup - time <= 1.5f)
//                        {
//                                float x = Mathf.Lerp(0, length1, (Time.realtimeSinceStartup - time) / 1.5f);
//                                slide1.rectTransform.sizeDelta = new Vector2(x, slide1.rectTransform.sizeDelta.y);
//                                yield return null;
//                        }
//                }

//                if (level_to == m_pet.GetPropertyInt("max_level"))
//                {
//                        GetText(Groups[0].gameObject, "level").text = "LV." + level_to + "/" + m_pet.GetPropertyInt("max_level");
//                        slide0.rectTransform.sizeDelta = new Vector2(0, slide0.rectTransform.sizeDelta.y);
//                        slide1.rectTransform.sizeDelta = new Vector2(0, slide1.rectTransform.sizeDelta.y);
//                }

//        }

//        long GetFoodExp()
//        {
//                long exp = 0;
//                for (int i = 0; i < m_FoodPetList.Count; i++)
//                {
//                        if (m_FoodPetList[i] != 0)
//                        {
//                                Pet pet = PetManager.GetInst().GetPet(m_FoodPetList[i]);
//                                exp += PetManager.GetInst().PetFoodExp(pet);
//                        }
//                }
//                long can_get_max_exp = CharacterManager.GetInst().GetCharacterExp(m_pet.GetPropertyInt("max_level")).total_exp - m_pet.GetPropertyLong("exp") - CharacterManager.GetInst().GetCharacterExp(m_pet.GetPropertyInt("level")).total_exp;
//                if (can_get_max_exp < 0)
//                {
//                        can_get_max_exp = 0;
//                }
//                return exp > can_get_max_exp ? can_get_max_exp : exp;

//        }


//        void RefreshLevelupBottom()
//        {
//                Dictionary<int, int> m_Skill = new Dictionary<int, int>();// m_pet.GetMySkill();
//                int count = 0;
//                GameObject skill0 = GetGameObject(Groups[0], "skill0");
//                skill0.SetActive(false);
//                foreach (int key in m_Skill.Keys)
//                {
//                        if (SkillManager.GetInst().GetSkill(key) != null)
//                        {
//                                GameObject temp = GetGameObject(Groups[0], "skill" + count);
//                                if (temp == null)
//                                {
//                                        temp = GameObject.Instantiate(skill0) as GameObject;
//                                        temp.transform.SetParent(skill0.transform.parent);
//                                        temp.name = "skill" + count;
//                                        temp.transform.localScale = Vector3.one;
//                                }
//                                ResourceManager.GetInst().LoadIconSprite(SkillManager.GetInst().GetSkillIconUrl(key), GetImage(temp, "icon").transform);
//                                int skill_level = m_Skill[key];
//                                if (m_Skill[key] >= SkillManager.GetInst().GetSkillMaxLevel(key, m_pet.GetPropertyInt("star")))
//                                {
//                                        GetText(temp, "Text").text = skill_level + "Max";
//                                }
//                                else
//                                {
//                                        GetText(temp, "Text").text = "LV." + skill_level + "->LV." + (skill_level + 1) + "  " + "<color=red>" + CalSkillUpRate(key, m_Skill[key], m_FoodPetList) + "%↑" + "</color>";
//                                }
//                                temp.SetActive(true);
//                                count++;
//                        }


//                }
//        }

//        void RefreshTransformBottom()
//        {
//                CharacterConfig cc = CharacterManager.GetInst().GetCharacterCfg(m_target_id);
//                if (cc == null)
//                {
//                        Debuger.Log("该宠物不存在！");
//                        return;
//                }

//                string skill_list = cc.GetProp("skill_list");
//                string[] target_skill = skill_list.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
//                List<int> new_skill = new List<int>();
//                Dictionary<int, int> m_Skill = new Dictionary<int, int>();// m_pet.GetMySkill();
//                for (int i = 0; i < target_skill.Length; i++)
//                {
//                        int skill_id = int.Parse(target_skill[i]);
//                        if (!m_Skill.ContainsKey(skill_id))
//                        {
//                                new_skill.Add(skill_id);
//                        }
//                }

//                GameObject skill0 = GetGameObject(Groups[1], "skill0");
//                skill0.SetActive(false);
//                int count = 0;
//                for (int i = 0; i < target_skill.Length; i++)
//                {
//                        int skill_id = int.Parse(target_skill[i]);
//                        if (!new_skill.Contains(skill_id))
//                        {
//                                if (SkillManager.GetInst().GetSkill(skill_id) != null)
//                                {
//                                        GameObject temp = GetGameObject(Groups[1], "skill" + count);
//                                        if (temp == null)
//                                        {
//                                                temp = GameObject.Instantiate(skill0) as GameObject;
//                                                temp.transform.SetParent(skill0.transform.parent);
//                                                temp.name = "skill" + count;
//                                                temp.transform.localScale = Vector3.one;
//                                        }
//                                        count++;
//                                        ResourceManager.GetInst().LoadIconSprite(SkillManager.GetInst().GetSkillIconUrl(skill_id), GetImage(temp, "icon").transform);

//                                        int advance = SkillManager.GetInst().GetSkillLevelUpByStar(m_pet.GetPropertyInt("star"), cc.GetPropInt("star"), skill_id);

//                                        GetText(temp, "Text").text = "等级上限提升" + advance;
//                                        temp.SetActive(true);
//                                }



//                        }
//                }

//                for (int i = 0; i < new_skill.Count; i++)
//                {
//                        if (SkillManager.GetInst().GetSkill(new_skill[i]) != null)
//                        {
//                                GameObject temp = GetGameObject(Groups[1], "skill" + count);
//                                if (temp == null)
//                                {
//                                        temp = GameObject.Instantiate(skill0) as GameObject;
//                                        temp.transform.SetParent(skill0.transform.parent);
//                                        temp.name = "skill" + count;
//                                        temp.transform.localScale = Vector3.one;
//                                        temp.transform.localPosition = Vector3.zero;
//                                }
//                                count++;
//                                ResourceManager.GetInst().LoadIconSprite(SkillManager.GetInst().GetSkillIconUrl(new_skill[i]), GetImage(temp, "icon").transform);
//                                GetText(temp, "Text").text = "new";
//                                temp.SetActive(true);
//                        }
//                }

//        }

//        int CalSkillUpRate(int skill_id, int skill_level, List<long> pet_list)
//        {

//                float offer_exp = 0;
//                for (int i = 0; i < pet_list.Count; i++)
//                {
//                        Pet pet = PetManager.GetInst().GetPet(pet_list[i]);
//                        if (pet != null)
//                        {
//                                if (pet.GetMySkill().ContainsKey(skill_id))
//                                {
//                                        int food_level = 1;// pet.GetMySkill()[skill_id];
//                                        offer_exp += SkillManager.GetInst().GetSkillExpOffer(food_level);
//                                }
//                        }
//                }
//                int rate = Mathf.FloorToInt(offer_exp / SkillManager.GetInst().GetSkillUpExpNeed(skill_level) * 100);
//                if (rate > 100)
//                {
//                        rate = 100;
//                }
//                return rate;
//        }

//        void RefreshList(PET_EAT_TAB pt)
//        {
//                GameObject group = null;
//                if (pt == PET_EAT_TAB.LEVELUP)
//                {
//                        group = Groups[0];
//                }
//                if (pt == PET_EAT_TAB.TRANSFORM)
//                {
//                        group = Groups[1];
//                }
//                Button card = GetButton(group, "card0");
//                for (int i = 0; i < MAX_CARD_NUM; i++)
//                {
//                        Button temp = GetButton(group, "card" + i);
//                        if (temp != null)
//                        {
//                                if (i > 0)
//                                {
//                                        说明已经初始化过了//
//                                        break;
//                                }
//                        }
//                        else
//                        {
//                                temp = GameObject.Instantiate(card) as Button;
//                                temp.name = "card" + i;
//                                temp.transform.SetParent(card.transform.parent);
//                                temp.transform.localScale = Vector3.one;
//                                temp.transform.localPosition = Vector3.zero;
//                        }
//                        EventTriggerListener.Get(temp.gameObject).SetTag(pt);
//                        EventTriggerListener.Get(temp.gameObject).onClick = OnChoosePet;
//                }

//                for (int i = 0; i < MAX_CARD_NUM; i++)
//                {
//                        Button temp = GetButton(group, "card" + i);
//                        long petid = m_FoodPetList[i];
//                        Pet pet = PetManager.GetInst().GetPet(petid);

//                        if (petid == 0)
//                        {
//                                GetText(temp.gameObject, "Text").text = help;
//                                GetImage(temp.gameObject, "Image").sprite = jia;
//                                GetGameObject(temp.gameObject, "star_group").SetActive(false);
//                        }
//                        else
//                        {
//                                GetText(temp.gameObject, "Text").text = pet.GetPropertyString("name");
//                                ResourceManager.GetInst().LoadIconSprite(ModelResourceManager.GetInst().GetIconRes(pet.GetPropertyInt("model_id")), GetImage(temp.gameObject, "Image").transform);
//                                GetGameObject(temp.gameObject, "star_group").SetActive(true);
//                                SetStar(pet.GetPropertyInt("star"), pet.GetPropertyInt("max_star"), temp.gameObject);
//                        }
//                }
//        }

//        int m_target_id = 0;
//        void RefreshTransformKind()
//        {
//                string[] target = m_pet.GetPropertyString("target").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
//                GameObject KindGroup = GetGameObject(Groups[1], "KindGroup");
//                if (target.Length > 1)
//                {
//                        KindGroup.SetActive(true);
//                        for (int i = 0; i < target.Length; i++)
//                        {
//                                Toggle kind = GetToggle(KindGroup, "kind" + i);
//                                EventTriggerListener.Get(kind.gameObject).onClick = OnKind;
//                                int id = int.Parse(target[i]);
//                                EventTriggerListener.Get(kind.gameObject).SetTag(id);
//                                GetText(kind.gameObject, "name").text = CharacterManager.GetInst().GetCharacterName(id);

//                        }
//                }
//                else
//                {
//                        KindGroup.SetActive(false);
//                }
//        }

//        void RefreshTransformRight()
//        {
//                Image info = GetImage(Groups[1], "info");
//                GetText(info.gameObject, "name").text = CharacterManager.GetInst().GetCharacterName(m_target_id);
//                List<int> need_race = PetManager.GetInst().PetTransFormRace(m_target_id);
//                List<int> need_career = PetManager.GetInst().PetTransFormCareer(m_target_id);
//                string career = "";
//                for (int i = 0; i < need_career.Count; i++)
//                {
//                        if (i == need_career.Count - 1)
//                        {
//                                career += need_career[i];
//                        }
//                        else
//                        {
//                                career += need_career[i] + ",";
//                        }
//                }

//                string race = "";
//                for (int i = 0; i < need_race.Count; i++)
//                {
//                        if (i == need_race.Count - 1)
//                        {
//                                race += need_race[i];
//                        }
//                        else
//                        {
//                                race += need_race[i] + ",";
//                        }
//                }

//                GetText(info.gameObject, "need").text = "需要同星级" + PetManager.GetInst().TransformNeedNum(m_pet.CharacterID) + "个";
//                GetText(info.gameObject, "level1").text = "LV." + m_pet.GetPropertyString("max_level") + "Max->";
//                GetText(info.gameObject, "level2").text = "LV.1/" + CharacterManager.GetInst().GetCharacterCfg(m_target_id).GetProp("max_level");
//        }


//        void SetStar(int now_star, int max_star, GameObject go)
//        {
//                foreach (Transform tf in go.GetComponentsInChildren<Transform>(true))
//                {
//                        if (tf.name.Contains("Clone"))
//                        {
//                                GameObject.Destroy(tf.gameObject);
//                        }
//                }
//                for (int i = 1; i < now_star; i++)
//                {
//                        CreatStar(true, go);
//                }

//                for (int j = now_star; j < max_star; j++)
//                {
//                        CreatStar(false, go);
//                }
//        }

//        void CreatStar(bool is_on, GameObject go) //是否是点亮的
//        {
//                Image star = null;
//                Image star0 = GetImage(go, "star0");
//                star = GameObject.Instantiate(star0) as Image;
//                star.transform.SetParent(star0.transform.parent);
//                star.transform.localScale = Vector3.one;
//                star.transform.localPosition = Vector3.zero;
//                if (!is_on)
//                {
//                        star.sprite = staroff;
//                }
//        }


//        private void OnTab(GameObject go, PointerEventData data)
//        {
//                Toggle tl = go.GetComponent<Toggle>();
//                if (tl.interactable)
//                {
//                        PET_EAT_TAB pt = (PET_EAT_TAB)EventTriggerListener.Get(go).GetTag();
//                        if (pt != m_tab)
//                        {
//                                if (pt == PET_EAT_TAB.TRANSFORM)
//                                {
//                                        if (m_target_id == 0)
//                                        {
//                                                UIManager.GetInst().ShowUI<UI_MessageBox>("UI_MessageBox").SetText("该宠物无法转生！");
//                                                HightLightTab(m_tab);
//                                                return;
//                                        }
//                                        if (m_pet.GetPropertyInt("level") != m_pet.GetPropertyInt("max_level"))
//                                        {
//                                                UIManager.GetInst().ShowUI<UI_MessageBox>("UI_MessageBox").SetText("没到达等级上限！");
//                                                HightLightTab(m_tab);
//                                                return;
//                                        }

//                                }

//                                m_tab = pt;
//                                ClearFoodList();
//                                RefreshGroup(pt);
//                        }
//                }
//        }

//        void OnChoosePet(GameObject go, PointerEventData data)
//        {
//                PET_EAT_TAB tab = (PET_EAT_TAB)EventTriggerListener.Get(go).GetTag();
//                if (tab == PET_EAT_TAB.LEVELUP)
//                {
//                        int level = m_pet.GetPropertyInt("level");
//                        if (level == m_pet.GetPropertyInt("max_level"))
//                        {
//                                GameUtility.PopupMessage("该宠物已达最大等级！");
//                                return;
//                        }
//                        UIManager.GetInst().ShowUI<UI_PetChoose>("UI_PetChoose").RefreshMyPetList(PetManager.GetInst().GetMyPetLeveUpFood(select_pet_id), tab, m_FoodPetList);
//                }
//                if (tab == PET_EAT_TAB.TRANSFORM)
//                {
//                        UIManager.GetInst().ShowUI<UI_PetChoose>("UI_PetChoose").RefreshMyPetList(PetManager.GetInst().GetMyPetTransformFood(select_pet_id), tab, m_FoodPetList);
//                }
//                gameObject.SetActive(false);
//        }



//        public void OnClickClose()
//        {
//                UIManager.GetInst().CloseUI(this.name);
//                UIManager.GetInst().ShowUI<UI_Pet>("UI_Pet");
//                mlb.Reset();
//        }

//        bool IsFoodPetHasEquip()  //材料宠物是否存在装备//
//        {
//                for (int i = 0; i < m_FoodPetList.Count; i++)
//                {
//                        if (m_FoodPetList[i] != 0)
//                        {
//                                Pet pet = PetManager.GetInst().GetPet(m_FoodPetList[i]);
//                                if (pet.PetHasEquip()) //只要有宠物有装备就并且包裹容量达到上限就无法操作
//                                {
//                                        return true;
//                                }
//                        }
//                }
//                return false;
//        }


//        public void OnLevelUp()
//        {
//                if (IsFoodPetHasEquip())
//                {
//                        if (GlobalParams.IsEquipBagFull())
//                        {
//                                UIManager.GetInst().ShowUI<UI_MessageBox>("UI_MessageBox").SetText("装备背包已经满了,请先清理！");
//                                return;
//                        }
//                }


//                string food = "";
//                if (m_FoodPetList.Count > 0)
//                {
//                        for (int i = 0; i < m_FoodPetList.Count; i++)
//                        {
//                                if (m_FoodPetList[i] != 0)
//                                {
//                                        food += m_FoodPetList[i] + "|";
//                                }
//                        }
//                }

//                if (food.Length > 0)
//                {
//                        PetManager.GetInst().SendPetLevelUp(select_pet_id, food);
//                }
//                else
//                {
//                        UIManager.GetInst().ShowUI<UI_MessageBox>("UI_MessageBox").SetText("请选择升级材料！");
//                }

//        }

//        public void OnLevelUpAuto()
//        {
//                int level = m_pet.GetPropertyInt("level");
//                if (level == m_pet.GetPropertyInt("max_level"))
//                {
//                        GameUtility.PopupMessage("该宠物已达最大等级！");
//                        return;
//                }
//                GetFoodList(PetManager.GetInst().CalPetAutoFoodExp(select_pet_id));
//                RefreshGroup(PET_EAT_TAB.LEVELUP);
//        }

//        public void OnKind(GameObject go, PointerEventData data)
//        {
//                int id = (int)EventTriggerListener.Get(go).GetTag();
//                if (m_target_id == id)
//                {
//                        return;
//                }
//                ClearFoodList();
//                m_target_id = id;
//                RefreshList(PET_EAT_TAB.TRANSFORM);
//                RefreshTransformRight();
//        }

//        public void OnTransform() //转职
//        {
//                if (IsFoodPetHasEquip())
//                {
//                        if (GlobalParams.IsEquipBagFull())
//                        {
//                                UIManager.GetInst().ShowUI<UI_MessageBox>("UI_MessageBox").SetText("装备背包已经满了,请先清理！");
//                                return;
//                        }
//                }
//                string food = "";
//                int count = 0;
//                if (m_FoodPetList.Count > 0)
//                {
//                        for (int i = 0; i < m_FoodPetList.Count; i++)
//                        {
//                                if (m_FoodPetList[i] != 0)
//                                {
//                                        food += m_FoodPetList[i] + "|";
//                                        count++;
//                                }
//                        }
//                }

//                if (count < PetManager.GetInst().TransformNeedNum(m_pet.CharacterID))
//                {
//                        UIManager.GetInst().ShowUI<UI_MessageBox>("UI_MessageBox").SetText("材料不足！");
//                        Debuger.Log("材料不足！");
//                }
//                else if (count > PetManager.GetInst().TransformNeedNum(m_pet.CharacterID))
//                {
//                        UIManager.GetInst().ShowUI<UI_MessageBox>("UI_MessageBox").SetText("材料太多！");
//                        Debuger.Log("材料太多！");
//                }
//                else
//                {
//                        PetManager.GetInst().SendPetAdvance(select_pet_id, food, m_target_id);
//                }

//        }



//        List<Specificity_Struct> m_SpecificityAll = new List<Specificity_Struct>();
//        void RefreshSpecificityGroup()  //刷新
//        {
//                m_SpecificityAll.Clear();
//                SetGameObjectHide("sick", Groups[2]);

//                GameObject sick = GetGameObject(Groups[2], "sick0");
//                RectTransform rt = sick.GetComponent<RectTransform>();

//                for (int i = (int)Specificity_Type.GOOD_RANDOM; i < (int)Specificity_Type.MAX; i++)
//                {
//                        AddMySpecificity(m_pet.GetMyPetSpecificity((Specificity_Type)i));
//                }


//                int count = 0;
//                for (int i = 0; i < m_SpecificityAll.Count; i++)
//                {
//                        SpecificityHold sfh = PetManager.GetInst().GetSpecificityCfg(m_SpecificityAll[i].id);
//                        if (sfh != null)
//                        {
//                                GameObject temp = GetGameObject(Groups[2], "sick" + count);
//                                if (temp == null)
//                                {
//                                        temp = CloneElement(sick, "sick" + count);
//                                        temp.GetComponent<RectTransform>().anchoredPosition = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y - count * (rt.rect.height));
//                                }
//                                temp.SetActive(true);
//                                int skill_id = sfh.passive_skill;
//                                PassiveSkillConfig cfg = SkillManager.GetInst().GetPassiveSkill(skill_id);
//                                if (cfg != null)
//                                {
//                                        GetText(temp, "name").text = LanguageManager.GetText(cfg.name);
//                                        GetText(temp, "Text").text = LanguageManager.GetText(cfg.desc);
//                                }
//                                count++;
//                        }


//                }

//        }

//        int AddMySpecificity(List<Specificity_Struct> m_Specificity)
//        {
//                for (int i = 0; i < m_Specificity.Count; i++)
//                {
//                        m_SpecificityAll.Add(m_Specificity[i]);
//                }
//                return m_Specificity.Count;
//        }

//        #region 装备

//        Sprite equip_jia;
//        void InitEquip()
//        {
//                GameObject equip0 = GetGameObject(Groups[3], "equip0");
//                equip_jia = GetImage(equip0, "icon").sprite;
//                int num = (int)ItemType.EQUIP_PART.MAX - (int)ItemType.EQUIP_PART.MAIN_HAND;
//                for (int i = 0; i < num; i++)
//                {
//                        GameObject equip = GetGameObject(Groups[3], "equip" + i);
//                        if (equip == null)
//                        {
//                                equip = GameObject.Instantiate(equip0) as GameObject;
//                                equip.name = "equip" + i;
//                                equip.transform.SetParent(equip0.transform.parent);
//                                equip.transform.localScale = Vector3.one;
//                        }
//                        GetImage(equip, "bg").gameObject.SetActive(true);
//                        GetText(equip, "Text").color = ItemType.quality_color[0];
//                        GetText(equip, "Text").text = "未装备" + ItemType.part_des[i];
//                        EventTriggerListener.Get(GetGameObject(equip, "equip")).SetTag(ItemType.EQUIP_PART.MAIN_HAND + i);
//                        EventTriggerListener.Get(GetGameObject(equip, "equip")).onClick = OnClickEquip;
//                }
//        }

//        void RefreshEquip()
//        {
//                int count = 0;
//                for (ItemType.EQUIP_PART i = ItemType.EQUIP_PART.MAIN_HAND; i < ItemType.EQUIP_PART.MAX; i++)
//                {
//                        GameObject equip = GetGameObject(Groups[3], "equip" + count);
//                        Equip m_equip = m_pet.GetMyEquipPart(i);
//                        if (m_equip == null)
//                        {

//                                if (i == ItemType.EQUIP_PART.OFF_HAND)  //双手武器
//                                {
//                                        Equip main_hand = m_pet.GetMyEquipPart(ItemType.EQUIP_PART.MAIN_HAND);
//                                        if (main_hand != null)
//                                        {
//                                                EquipHoldConfig ec = EquipManager.GetInst().GetEquipCfg(main_hand.equip_configid);
//                                                if (ec.is_two_hands == 1)
//                                                {
//                                                        GameObject equip_add = GetGameObject(Groups[3], "equip" + (count + 1));
//                                                        GetImage(equip, "bg").gameObject.SetActive(false);
//                                                        GetText(equip, "Text").color = ItemType.quality_color[ec.quality];
//                                                        GetText(equip, "Text").text = LanguageManager.GetText(ec.name);
//                                                        ResourceManager.GetInst().LoadIconSprite(ec.icon, GetImage(equip, "icon").transform);
//                                                        GetImage(equip, "icon").color = new Color(1, 1, 1, 0.35f);
//                                                        count++;
//                                                        continue;
//                                                }
//                                        }
//                                }
//                                GetImage(equip, "bg").gameObject.SetActive(true);
//                                GetText(equip, "Text").color = ItemType.quality_color[0];
//                                GetText(equip, "Text").text = "未装备" + ItemType.part_des[count];
//                                GetImage(equip, "icon").sprite = equip_jia;
//                                GetImage(equip, "icon").color = new Color(1, 1, 1, 1);


//                        }
//                        else
//                        {
//                                EquipHoldConfig ec = EquipManager.GetInst().GetEquipCfg(m_equip.equip_configid);
//                                GetImage(equip, "bg").gameObject.SetActive(false);
//                                GetText(equip, "Text").color = ItemType.quality_color[ec.quality];
//                                GetText(equip, "Text").text = LanguageManager.GetText(ec.name);
//                                ResourceManager.GetInst().LoadIconSprite(ec.icon, GetImage(equip, "icon").transform);
//                                GetImage(equip, "icon").color = new Color(1, 1, 1, 1);

//                        }
//                        count++;
//                }
//        }

//        void OnClickEquip(GameObject go, PointerEventData data)
//        {
//                int part = (int)EventTriggerListener.Get(go).GetTag();
//                UIManager.GetInst().ShowUI<UI_EquipUse>("UI_EquipUse").Refresh(part, m_pet.ID);
//                gameObject.SetActive(false);
//        }


//        #endregion


//        void OnEnable()
//        {
//                ModelCamera.camera.enabled = true;
//        }


//        void OnDisable()
//        {
//                ModelCamera.camera.enabled = false;
//        }
//}
