using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Message;

class PetManager : SingletonObject<PetManager>
{
        Dictionary<long, Pet> m_PetDict = new Dictionary<long, Pet>();
        public void Init()
        {
                ConfigHoldUtility<SpecificityHold>.LoadXml("Config/specificity", m_SpecificityDict); //特性
                ConfigHoldUtility<CharactarAchievementConfig>.LoadXml("Config/charactar_achievement", m_AchievementDict); //成就

                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgPetInfo), OnPetInfo);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCPetAttrUpdate), OnPetAttr);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCPetDelete), OnPetDelete);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCPetEat), OnPetLevelUp);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCPetTransfer), OnPetAdvanced);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgPetOutTime), OnGetPetOutTime);

                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgPetCureTime), OnGetPetCureTime);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgBuildCurePetCancel), OnGetPetCureCancel);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgPetCureCompleted), OnGetPetCureCompleted);

                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgPetPurifyTime), OnGetPetPurifyTime);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgPetPurifyCompleted), OnGetPetPurifyCompleted);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgPetBuildPurifyPetCancel), OnGetPetPurifyCancel);
                
        }


        Dictionary<int, SpecificityHold> m_SpecificityDict = new Dictionary<int, SpecificityHold>();
        public SpecificityHold GetSpecificityCfg(int id)
        {
                if (m_SpecificityDict.ContainsKey(id))
                {
                        return m_SpecificityDict[id];
                }
                return null;
        }


        void OnPetInfo(object sender, SCNetMsgEventArgs e)
        {
                SCMsgPetInfo msg = e.mNetMsg as SCMsgPetInfo;

                if (!m_PetDict.ContainsKey(msg.id))
                {
                        Pet pet = new Pet(msg);
                        m_PetDict.Add(msg.id, pet);

                        if (HomeManager.GetInst().HomeRoot != null)
                        {
                                HomeManager.GetInst().SetPetModelOne(m_PetDict[msg.id]);
                        }


                        UI_HeroMain uhm = UIManager.GetInst().GetUIBehaviour<UI_HeroMain>();
                        if (uhm != null)
                        {
                                uhm.RefreshMyHeroList(pet);
                        }



                        if (GameStateManager.GetInst().GameState > GAMESTATE.OTHER)
                        {
                                AddToNewHeroList(pet.ID);
                        }
                }
        }

        void OnPetAttr(object sender, SCNetMsgEventArgs e)
        {
                SCPetAttrUpdate msg = e.mNetMsg as SCPetAttrUpdate;
                if (m_PetDict.ContainsKey(msg.ID))
                {
                        m_PetDict[msg.ID].OnUpdateProperty(msg.Name, msg.Value);
                }
        }


        void OnPetDelete(object sender, SCNetMsgEventArgs e)
        {
                SCPetDelete msg = e.mNetMsg as SCPetDelete;

                if (m_PetDict.ContainsKey(msg.petid))
                {
                        m_PetDict.Remove(msg.petid);
                        if (HomeManager.GetInst().HomeRoot != null)
                        {
                                HomeManager.GetInst().DeletePetObj(msg.petid);
                        }

                        UI_HeroMain uhm = UIManager.GetInst().GetUIBehaviour<UI_HeroMain>();
                        if (uhm != null)
                        {
                                uhm.RefreshMyHeroList();
                        }

                        UI_PetMain upm = UIManager.GetInst().GetUIBehaviour<UI_PetMain>();
                        if (upm != null)
                        {
                                upm.RefreshMyHeroList();
                        }
                }
                else
                {
                        Debuger.Log("删除的英雄不存在！");
                }
        }

        void OnPetLevelUp(object sender, SCNetMsgEventArgs e)
        {
                SCPetEat msg = e.mNetMsg as SCPetEat;
                if (msg.error_type == 0)
                {
                        PetManager.GetInst().ChoosePetList.Clear();
                        UIManager.GetInst().GetUIBehaviour<UI_PetMain>().RefreshLeft();
                        UIManager.GetInst().GetUIBehaviour<UI_PetMain>().RefreshGroup(UI_PetMain.TAB.LevelUp);
                }
                else
                {
                        Debuger.Log("吃英雄出错，错误类型" + msg.error_type + "!!!");
                }

        }

        void OnPetAdvanced(object sender, SCNetMsgEventArgs e)
        {
                SCPetTransfer msg = e.mNetMsg as SCPetTransfer;
                if (msg.error_type == 0)
                {
                        PetManager.GetInst().ChoosePetList.Clear();
                        UIManager.GetInst().GetUIBehaviour<UI_PetMain>().RefreshLeft();
                        UIManager.GetInst().GetUIBehaviour<UI_PetMain>().RefreshGroup(UI_PetMain.TAB.LevelUp);
                        if (HomeManager.GetInst().HomeRoot != null)
                        {
                                HomeManager.GetInst().DeletePetObj(msg.pet_id);
                                HomeManager.GetInst().SetPetModelOne(m_PetDict[msg.pet_id]);
                        }
              
                }
                else
                {
                        Debuger.Log("转生出错，错误类型" + msg.error_type + "!!!");
                }

        }

        public void SendPetLevelUp(long id, string food_info)
        {
                CSMsgPetEat msg = new CSMsgPetEat();
                msg.petid = id;
                msg.foodinfo = food_info;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

        public void SendPetAdvance(long id, string food_info, int career_id)  //进阶
        {
                CSMsgPetTransfer msg = new CSMsgPetTransfer();
                msg.petid = id;
                msg.foodinfo = food_info;
                msg.career_id = career_id;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////////////



        public Pet GetPet(long id)
        {
                if (m_PetDict.ContainsKey(id))
                {
                        return m_PetDict[id];
                }
                return null;
        }

        public Pet GetPetByCharacterId(int characterid)
        {
                foreach (Pet pet in m_PetDict.Values)
                {
                        if (pet.CharacterID == characterid)
                        {
                                return pet;
                        }
                }
                return null;
        }

        public List<Pet> GetMyPetList()
        {
                return new List<Pet>(m_PetDict.Values);
        }

        public List<Pet> GetMyPetListState(PetState state) 
        {
                List<Pet> m_pets = new List<Pet>();
                List<Pet> m_allpets = GetMyPetList();
                for (int i = 0; i < m_allpets.Count; i++)
                {
                        if (m_allpets[i].GetPropertyInt("cur_state") == (int)state)
                        {
                                m_pets.Add(m_allpets[i]);
                        }
                }
                return m_pets;
        }

        public List<long> GetMyPetListID()
        {
                return new List<long>(m_PetDict.Keys);
        }

        public List<long> GetMyPetListStateID(PetState state,long pet_id) //排除了死亡和出门的
        {
                List<long> m_petsid = new List<long>();
                List<Pet> m_allpets = GetMyPetListState(state);
                for (int i = 0; i < m_allpets.Count; i++)
                {
                        if (m_allpets[i].ID != pet_id)
                        {
                                m_petsid.Add(m_allpets[i].ID);
                        }
                }
                return m_petsid;
        }

        public List<Pet> GetPetsInBuild(long build_id)
        {
                List<Pet> m_pets = new List<Pet>(); 
                foreach (Pet pet in m_PetDict.Values)
                {
                        if (pet.GetPropertyLong("belong_build") == build_id)
                        {
                                m_pets.Add(pet);
                        }
                }
                //m_pets.Sort(ComparePetByTime);
                return m_pets;
        }

        protected int ComparePetByTime(Pet peta, Pet petb) 
        {
                if (null == peta || null == petb)
                {
                        Debuger.LogError("can not find pet in pet sort!!");
                        return -1;
                }

                int timea = (int)peta.GetNotNormalTime();
                int timeb = (int)petb.GetNotNormalTime();
                if (peta != petb)
                {
                        return timea - timeb;
                }

                int characterIDa = peta.CharacterID;
                int characterIDb = petb.CharacterID;
                if (characterIDa != characterIDb)
                {
                        return (characterIDb - characterIDa);
                }
                return (int)(petb.ID - peta.ID);
        }


        public int GetMyAllPetNum()
        {
                return m_PetDict.Count;
        }

        public bool IsHeroBagFull()
        {
                int hero_max = CommonDataManager.GetInst().GetHeroBagLimit();
                if (GetMyAllPetNum() >= hero_max)  //宠物包满了
                {
                        return true;
                }
                return false;
        }

        public int GetPetNumByCharacterID(int CharacterId)
        {
                int num = 0;
                foreach (Pet pet in m_PetDict.Values)
                {
                        if (pet.CharacterID == CharacterId)
                        {
                                if (pet.GetPropertyInt("cur_state") == (int)PetState.Normal)
                                {
                                        num++;
                                }
                        }
                }
                return num;
        }

        public List<Pet> GetMyPetListSort(List<Pet> petlist)  
        {
                petlist.Sort(ComparePetByStar);
                return petlist;
        }

        public List<Pet> GetMyPetListSort()  
        {
                List<Pet> petlist = GetMyPetListState(PetState.Normal);
                petlist.Sort(ComparePetByStar);
                return petlist;
        }

        public List<Pet> GetPressurePets()
        {
                List<Pet> m_list = new List<Pet>();
                List<Pet> petlist = GetMyPetList();
                for (int i = 0; i < petlist.Count; i++)
                {
//                         if (petlist[i].FighterProp.Pressure > 0)
//                         {
//                                 m_list.Add(petlist[i]);
//                         }
                }
                m_list.Sort(ComparePetByPressure);
                return m_list;
        }


        public List<long> GetCanCheckInPets()
        {
                List<long> m_list = new List<long>();
                List<Pet> petlist = GetMyPetListState(PetState.Normal);
                for (int i = 0; i < petlist.Count; i++)
                {
                        if (petlist[i].GetPropertyLong("belong_build") == 0)
                        {
                                m_list.Add(petlist[i].ID);
                        }
                }
                //m_list.Sort(ComparePetByPressure);
                return m_list;
        }

        public List<Pet> GetMyPetByCareerAndRaceSort(List<long> m_pet_list, int career_sys,int race) //按照职业和种族分类分类//
        {
                List<Pet> petlist = new List<Pet>();
                for (int i = 0; i < m_pet_list.Count; i++)
                {
                        Pet pet = GetPet(m_pet_list[i]);
                        if (career_sys <= 0)
                        {
                                if (race <= 0)
                                {
                                        petlist.Add(pet);
                                }
                                else
                                {
                                        if (pet.GetPropertyInt("race") == race)
                                        {
                                                petlist.Add(pet);
                                        }
                                }
                        }
                        else
                        {
                                if (pet.GetPropertyInt("career_sys") == career_sys)
                                {
                                        if (race <= 0)
                                        {
                                                petlist.Add(pet);
                                        }
                                        else
                                        {
                                                if (pet.GetPropertyInt("race") == race)
                                                {
                                                        petlist.Add(pet);
                                                }
                                        }
                                }
                        }
                }
                petlist.Sort(ComparePetByStar);
                return petlist;
        }

        public List<Pet> GetMyPetByCareerAndRaceSort(int career_sys, int race) //按照职业和种族分类分类//
        {
                return GetMyPetByCareerAndRaceSort(GetMyPetListID(), career_sys, race);
        }

        public List<long> GetMyPetLeveUpFood(long pet_id)  //升级材料
        {
                List<long> petlist = GetMyPetListStateID(PetState.Normal,pet_id);
                return petlist;
        }

    public List<long> GetMyPetAdvanceFood(long m_id, int target_id) //进阶宠物材料//
    {
        List<Pet> m_all_food_pet = GetMyPetListState(PetState.Normal);
        List<long> petlist = new List<long>();
        CharacterConfig cc = CharacterManager.GetInst().GetCharacterCfg(target_id);
        Pet pet = GetPet(m_id);
        //int trans_need_charactar = cc.GetPropInt("trans_need_charactar");
        //                 if (trans_need_charactar == 0)//同星级转职
        //                 {
        //                         for (int i = 0; i < m_all_food_pet.Count;i++ )
        //                         {
        //                                 if (m_all_food_pet[i].GetPropertyInt("star") == pet.GetPropertyInt("star"))
        //                                 {
        //                                         if (m_all_food_pet[i].ID != m_id)
        //                                         {
        //                                                 petlist.Add(m_all_food_pet[i].ID);
        //                                         }
        //                                 }
        //                         }
        //                 }
        //                else //指定职业转职
//        {
//             for (int i = 0; i < m_all_food_pet.Count; i++)
//             {
//                 if (m_all_food_pet[i].CharacterID == trans_need_charactar)
//                 {
//                     if (m_all_food_pet[i].ID != m_id)
//                     {
//                         petlist.Add(m_all_food_pet[i].ID);
//                     }
//                 }
//             }
   //     }
        return petlist;
    }

        protected int ComparePetByStar(Pet peta, Pet petb) // 按星级>按等级 //排序
        {
                if (null == peta || null == petb)
                {
                        Debuger.LogError("can not find pet in pet sort!!");
                        return -1;
                }

                int mob_starsa = peta.GetPropertyInt("star");
                int mob_starsb = petb.GetPropertyInt("star");
                if (mob_starsa != mob_starsb)
                {
                        return mob_starsb - mob_starsa;
                }


                int levela = peta.GetPropertyInt("level");
                int levelb = petb.GetPropertyInt("level");
                if (levela != levelb)
                {
                        return levelb - levela;
                }
                long sexpa = peta.GetPropertyLong("exp");
                long sexpb = petb.GetPropertyLong("exp");
                if (sexpa != sexpb)
                {
                        return (int)(sexpb - sexpa);
                }

                int characterIDa = peta.CharacterID;
                int characterIDb = petb.CharacterID;
                if (characterIDa != characterIDb)
                {
                        return (characterIDb - characterIDa);
                }
                return (int)(petb.ID - peta.ID);
        }

        protected int ComparePetByPressure(Pet peta, Pet petb) // 按压力大小
        {
                if (null == peta || null == petb)
                {
                        Debuger.LogError("can not find pet in pet sort!!");
                        return -1;
                }

//                 int pressure_a = peta.FighterProp.Pressure;
//                 int pressure_b = petb.FighterProp.Pressure;
//                 if (pressure_a != pressure_b)
//                 {
//                         return pressure_b - pressure_a;
//                 }

                int characterIDa = peta.CharacterID;
                int characterIDb = petb.CharacterID;
                if (characterIDa != characterIDb)
                {
                        return (characterIDb - characterIDa);
                }
                return (int)(petb.ID - peta.ID);
        }


        long PetFoodExp(Pet pet) //该宠物能提供的经验值
        {
                return pet.GetPropertyLong("exp") + CharacterManager.GetInst().GetCharacterExp(pet.GetPropertyInt("level")).total_exp + pet.GetPropertyLong("base_exp");
        }


        public long AllGetFoodExp()
        {
                long exp = 0;
                for (int i = 0; i < ChoosePetList.Count; i++)
                {
                        Pet pet = PetManager.GetInst().GetPet(ChoosePetList[i]);
                        exp += PetManager.GetInst().PetFoodExp(pet);           
                }
                return exp;
        }

        public int CalSkillUpRate(int skill_id, int skill_level)
        {
                //                 float offer_exp = 0;
                //                 for (int i = 0; i < ChoosePetList.Count; i++)
                //                 {
                //                         Pet pet = GetPet(ChoosePetList[i]);
                //                         if (pet != null)
                //                         {
                //                                 List<Skill_Struct> m_skill = pet.GetMySkill();
                //                                 for (int j = 0; j < m_skill.Count; j++)
                //                                 {
                //                                         if (m_skill[j].config_id == skill_id)
                //                                         {
                //                                                 int food_level = m_skill[j].level;
                //                                                 offer_exp += SkillManager.GetInst().GetSkillExpOffer(food_level);
                //                                                 break;
                //                                         }
                //                                 }      
                //                          }
                //                 }
                //                 int rate = Mathf.FloorToInt(offer_exp / SkillManager.GetInst().GetSkillUpExpNeed(skill_level) * 100);
                //                 if (rate > 100)
                //                 {
                //                         rate = 100;
                //                 }
                //                return rate;
                return 100;
        }

        public List<long> ChoosePetList = new List<long>();  //用在材料选择中选中的宠物


        #region 上门行为

        public List<Pet> GetPetsForVisitorByPay(int visitor_id)  //通过上门行为付出筛选出可用的宠物
        {
                NpcHouseVisitorHold nv = NpcManager.GetInst().GetVisitorCfg(visitor_id);
                if (nv != null)
                {
                        List<Pet> m_allpets = GetMyPetListState(PetState.Normal);
                        List<Pet> m_pets = new List<Pet>();
                        EVISIT_PAY_TYPE pay_type = (EVISIT_PAY_TYPE)nv.pay_type;
                        string pay_parameter = nv.pay_parameter;
                        switch (pay_type)
                        {
                                case EVISIT_PAY_TYPE.恶癖:
                                        int count = pay_parameter.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Length;
                                        for (int i = 0; i < m_allpets.Count; i++)
                                        {
                                                Pet pet = m_allpets[i];
                                                if (pet.GetMyPetSpecificity(Specificity_Type.BAD_RANDOM).Count + count <= pet.GetPropertyInt("random_harmful_behavior_quantity"))
                                                {
                                                        m_pets.Add(pet);
                                                }
                                        }
                                        break;
                                case EVISIT_PAY_TYPE.item或equip:
                                        m_pets = m_allpets;
                                        break;
                                case EVISIT_PAY_TYPE.出门:
                                        m_pets = m_allpets;
                                        break;
                                case EVISIT_PAY_TYPE.压力值:
                                        int get_pressure = 0;
                                        int.TryParse(pay_parameter, out get_pressure);
                                        for (int i = 0; i < m_allpets.Count; i++)
                                        {
                                                Pet pet = m_allpets[i];
//                                                 if (!pet.FighterProp.GetPressureToDead(get_pressure))
//                                                 {
//                                                         m_pets.Add(pet);
//                                                 }
                                        }
                                        break;
                                default:
                                        break;

                        }
                        return m_pets;
                }
                return new List<Pet>();
        }


        public List<Pet> GetPetsForVisitorByGet(List<Pet> petlist_pay, int visitor_id)  //筛选出可获得好处的宠物
        {
                NpcHouseVisitorHold nv = NpcManager.GetInst().GetVisitorCfg(visitor_id);
                if (nv != null)
                {
                        List<Pet> m_pets = new List<Pet>();
                        EVISIT_REWARD_TYPE get_type = (EVISIT_REWARD_TYPE)nv.get_type;
                        string get_parameter = nv.get_parameter;
                        switch (get_type)
                        {
                                case EVISIT_REWARD_TYPE.固定一条or多条选一条美德获取:
                                        for (int i = 0; i < petlist_pay.Count; i++)
                                        {
                                                Pet pet = petlist_pay[i];
                                                if (pet.GetMyPetSpecificity(Specificity_Type.GOOD_RANDOM).Count + 1 <= pet.GetPropertyInt("random_helpful_behavior_quantity"))
                                                {
                                                        m_pets.Add(pet);
                                                }
                                        }
                                        break;
                                case EVISIT_REWARD_TYPE.随机恶癖清除:
                                        for (int i = 0; i < petlist_pay.Count; i++)
                                        {
                                                Pet pet = petlist_pay[i];
                                                if (pet.GetMyPetSpecificity(Specificity_Type.BAD_RANDOM).Count > 0)
                                                {
                                                        m_pets.Add(pet);
                                                }
                                        }
                                        break;
                                case EVISIT_REWARD_TYPE.单人经验获取:
                                        for (int i = 0; i < petlist_pay.Count; i++)
                                        {
                                                Pet pet = petlist_pay[i];
                                                if (pet.GetPropertyInt("level") != pet.GetPropertyInt("max_level"))
                                                {
                                                        m_pets.Add(pet);
                                                }
                                        }
                                        break;
                                case EVISIT_REWARD_TYPE.单人技能升级:
                                        for (int i = 0; i < petlist_pay.Count; i++)
                                        {
                                                Pet pet = petlist_pay[i];
                                                List<Skill_Struct> m_skill = pet.GetMySkill();
                                                for (int j = 0; j < m_skill.Count; j++)
                                                {
                                                        Skill_Struct ss = m_skill[j];
                                                        SkillLearnConfigHold sfh = SkillManager.GetInst().GetSkillInfo(ss.config_id);
                                                        if (sfh != null)
                                                        {
                                                        //if (sfh.type != 4) //冒险技能不升级
                                                        //{
                                                                if (ss.level < SkillManager.GetInst().GetSkillMaxLevel(ss.config_id, pet.GetPropertyInt("star")))
                                                                {
                                                                        m_pets.Add(pet);
                                                                        break;
                                                                }
                                                        //}                                
                                                        }
                                                }                        
                                        }
                                        break;
                                case EVISIT_REWARD_TYPE.单人压力清除:
                                        for (int i = 0; i < petlist_pay.Count; i++)
                                        {
                                                Pet pet = petlist_pay[i];
//                                                 if (pet.FighterProp.Pressure > 0)
//                                                 {
//                                                         m_pets.Add(pet);
//                                                 }
                                        }
                                        break;
                                case EVISIT_REWARD_TYPE.全局压力清除:
                                case EVISIT_REWARD_TYPE.本回合食物回复:
                                case EVISIT_REWARD_TYPE.item或equip价值抽取:
                                case EVISIT_REWARD_TYPE.指定item或equip获取:
                                case EVISIT_REWARD_TYPE.驿站强力英雄获取:
                                case EVISIT_REWARD_TYPE.奖励副本获取:
                                default:
                                        break;
                                }
                                return m_pets;
                        }
                return new List<Pet>();
        }

        #endregion

        public void SendPetRevive(long build_id,string id)
        {
                CSMsgPetRelive msg = new CSMsgPetRelive();
                msg.build_id = build_id;
                msg.petid = id;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

        #region 成就

        Dictionary<int, CharactarAchievementConfig> m_AchievementDict = new Dictionary<int, CharactarAchievementConfig>();
        public CharactarAchievementConfig GetAchievementCfg(int id)
        {
                if (m_AchievementDict.ContainsKey(id))
                {
                        return m_AchievementDict[id];
                }
                return null;
        }

        public Dictionary<int, CharactarAchievementConfig> GetAchievementDict()
        {
                return m_AchievementDict;
        }


        public void SendGetAchieve(long petid, int achieveid)
        {
                CSMsgPetGetAchieve msg = new CSMsgPetGetAchieve();
                msg.petid = petid;
                msg.achieve_id = achieveid;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

        #endregion


        void OnGetPetOutTime(object sender, SCNetMsgEventArgs e)
        {
                SCMsgPetOutTime msg = e.mNetMsg as SCMsgPetOutTime;
                if (m_PetDict.ContainsKey(msg.pet_id))
                {
                        Pet pet = m_PetDict[msg.pet_id];
                        pet.SetNotNormalTime(msg.time);
                        pet.SetProperty("cur_state", (int)PetState.Out);   
                }
        }

        public void SendPetBack(long petid)
        {
                CSMsgPetBack msg = new CSMsgPetBack();
                msg.pet_id = petid;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

        public void SendPetPressureCure(long idRealBuild, long petid) //压力治疗
        {
                CSMsgBuildCurePet msg = new CSMsgBuildCurePet();
                msg.idRealBuild = idRealBuild;
                msg.pet_id = petid;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

        public void SendPetCurePressureFinish(long petid)                    //告知服务器治疗完毕
        {
                CSMsgPetCureCompleted msg = new CSMsgPetCureCompleted();
                msg.pet_id = petid;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

        public void SendPetCurePressureCancel(long petid)                   //取消治疗
        {
                CSMsgCurePetCancel msg = new CSMsgCurePetCancel();
                msg.pet_id = petid;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

        void OnGetPetCureTime(object sender, SCNetMsgEventArgs e)
        {
                SCMsgPetCureTime msg = e.mNetMsg as SCMsgPetCureTime;
                if (m_PetDict.ContainsKey(msg.pet_id))
                {
                        Pet pet = m_PetDict[msg.pet_id];
                        pet.SetNotNormalTime(msg.time);
                        pet.SetProperty("cur_state", (int)PetState.Curing);

                        HomeManager.GetInst().SetCureBuildTime(pet.GetPropertyLong("belong_build"), pet.GetNotNormalTime());
                }

//                  UI_PressureCure upc = UIManager.GetInst().GetUIBehaviour<UI_PressureCure>();
//                  if (upc != null)
//                  {
//                          upc.Refresh();
//                  }
        }

        void OnGetPetCureCompleted(object sender, SCNetMsgEventArgs e)
        {
//                 UI_PressureCure upc = UIManager.GetInst().GetUIBehaviour<UI_PressureCure>();
//                 if (upc != null)
//                 {
//                         upc.Refresh();
//                 }
        }

        void OnGetPetCureCancel(object sender, SCNetMsgEventArgs e)
        {
//                 UI_PressureCure upc = UIManager.GetInst().GetUIBehaviour<UI_PressureCure>();
//                 if (upc != null)
//                 {
//                         upc.Refresh();
//                 }
        }


        public void SendPetPurify(long idRealBuild, long petid,int type, int behaviourId) //特性
        {
                CSMsgBuildPurify msg = new CSMsgBuildPurify();
                msg.idBuild = idRealBuild;
                msg.pet_id = petid;
                msg.type = type;
                msg.idBehaviour = behaviourId;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

        public void SendPetPurifyFinish(long petid)                    //告知服务器治疗完毕
        {
                CSMsgPetPurifyCompleted msg = new CSMsgPetPurifyCompleted();
                msg.pet_id = petid;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

        public void SendPetPurifyCancel(long petid)                   //取消治疗
        {
                CSMsgBuildPurifyPetCancel msg = new CSMsgBuildPurifyPetCancel();
                msg.idpet = petid;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

        void OnGetPetPurifyTime(object sender, SCNetMsgEventArgs e)
        {
                SCMsgPetPurifyTime msg = e.mNetMsg as SCMsgPetPurifyTime;
                if (m_PetDict.ContainsKey(msg.pet_id))
                {
                        Pet pet = m_PetDict[msg.pet_id];
                        pet.SetNotNormalTime(msg.time);
                        pet.SetProperty("cur_state", (int)PetState.Curing);
                        HomeManager.GetInst().SetCureBuildTime(pet.GetPropertyLong("belong_build"), pet.GetNotNormalTime());
                }

                UI_SpecificityCure usc = UIManager.GetInst().GetUIBehaviour<UI_SpecificityCure>();
                if (usc != null)
                {
                        usc.Refresh();
                }
        }

        void OnGetPetPurifyCompleted(object sender, SCNetMsgEventArgs e)
        {
                UI_SpecificityCure usc = UIManager.GetInst().GetUIBehaviour<UI_SpecificityCure>();
                if (usc != null)
                {
                        usc.Refresh();
                }
        }

        void OnGetPetPurifyCancel(object sender, SCNetMsgEventArgs e)
        {
                UI_SpecificityCure usc = UIManager.GetInst().GetUIBehaviour<UI_SpecificityCure>();
                if (usc != null)
                {
                        usc.Refresh();
                }
        }


        public void SendPetCheckIn(long idRealBuild, long petid) //入住
        {
                CSMsgPetCheckIn msg = new CSMsgPetCheckIn();
                msg.realBuild = idRealBuild;
                msg.petId = petid;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }      

        public void Update()
        {
                var PetDic_Enumerator = m_PetDict.GetEnumerator();
                try
                {
                        while (PetDic_Enumerator.MoveNext())
                        {
                                Pet pet = PetDic_Enumerator.Current.Value;
                                if (pet.GetPropertyInt("cur_state") == (int)PetState.Out)  //出门状态
                                {
                                        pet.SetShowTime(pet.GetNotNormalTime() - Time.realtimeSinceStartup);

                                        if (pet.GetShowTime() <= 0)
                                        {
                                                SendPetBack(pet.ID);
                                                pet.SetProperty("cur_state", (int)PetState.Normal); 
                                        }
                                }

                                if (pet.GetPropertyInt("cur_state") == (int)PetState.Curing)  //治疗状态
                                {
                                        pet.SetShowTime(pet.GetNotNormalTime() - Time.realtimeSinceStartup);

                                        if (HomeManager.GetInst().GetBuildInfo(pet.GetPropertyLong("belong_build")).type == EBuildType.eCure)
                                        {
//                                                 UI_PressureCure upc = UIManager.GetInst().GetUIBehaviour<UI_PressureCure>();
//                                                 if (upc != null)
//                                                 {
//                                                         upc.ShowPetNotNormalTime(pet);
//                                                 }
// 
                                                if (pet.GetShowTime() <= 0)
                                                {
                                                        SendPetCurePressureFinish(pet.ID);
                                                        pet.SetProperty("cur_state", (int)PetState.Normal);
                                                }
                                        }
                                        else
                                        {
                                                UI_SpecificityCure usc = UIManager.GetInst().GetUIBehaviour<UI_SpecificityCure>();
                                                if (usc != null)
                                                {
                                                        usc.ShowPetNotNormalTime(pet);
                                                }

                                                if (pet.GetShowTime() <= 0)
                                                {
                                                        SendPetPurifyFinish(pet.ID);
                                                        pet.SetProperty("cur_state", (int)PetState.Normal);
                                                }
                                        }
                                                


                                }
                        }
                }
                finally
                {
                        PetDic_Enumerator.Dispose();
                }                               
        }

        int m_nNewHeroCount = 0;
        public int NewHeroCount
        {
                get
                {
                        return m_nNewHeroCount;
                }
                set
                {
                        m_nNewHeroCount = value;
                        if (UIManager.GetInst().GetUIBehaviour<UI_HomeMain>() != null)
                        {
                                UIManager.GetInst().GetUIBehaviour<UI_HomeMain>().RefreshNewDarwNum("newherobtn", NewHeroCount);
                        }
                }
        }

        List<long> m_NewHeroList = new List<long>();
        public bool IsNewHero(long id)
        {
                return m_NewHeroList.Contains(id);
        }
        public void ClearNewHeroList()
        {
                m_NewHeroList.Clear();
                NewHeroCount = 0;
        }
        public void AddToNewHeroList(long id)
        {
                if (!m_NewHeroList.Contains(id))
                {
                        NewHeroCount++;
                        m_NewHeroList.Add(id);
                }
        }

}
