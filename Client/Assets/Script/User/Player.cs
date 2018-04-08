using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Message;
using System;

class PlayerController : PropertyBase
{
        private static PlayerController mSingleton;

        public PlayerController()
        {
        }

        public static PlayerController GetInst()
        {
                if (mSingleton == null)
                {
                        mSingleton = SingletonObject<PlayerController>.GetInst();
                }
                return mSingleton;
        }

        long m_PlayerID;
        public long PlayerID
        {
                get
                {
                        return m_PlayerID;
                }
                set
                {
                        m_PlayerID = value;
                }
        }

        public long lastOfflineTimeStamp;

        public List<ulong> m_GuideList = new List<ulong>();


        void InitPropertyHandlers()
        {
                m_PropertyHandlers.Add("diamond", OnDiamondUpdate);
                m_PropertyHandlers.Add("three_star_list", OnThreeGet);
                m_PropertyHandlers.Add("four_star_list", OnFourGet);
                m_PropertyHandlers.Add("five_star_list", OnFiveGet);
                m_PropertyHandlers.Add("house_level", OnHomeLevel);
                m_PropertyHandlers.Add("house_exp", OnHomeExp);
                m_PropertyHandlers.Add("gharry_lvl", OnHorseLevel);
                //m_PropertyHandlers.Add("food", OnFood);
                m_PropertyHandlers.Add("gold", OnGoldUpdate);
                m_PropertyHandlers.Add("gold_capacity", OnGoldUpdate);
                m_PropertyHandlers.Add("stone", OnStoneUpdate);
                m_PropertyHandlers.Add("stone_capacity", OnStoneUpdate);
                m_PropertyHandlers.Add("wood", OnWoodUpdate);
                m_PropertyHandlers.Add("wood_capacity", OnWoodUpdate);
                m_PropertyHandlers.Add("crystal", OnCrystalUpdate);
                m_PropertyHandlers.Add("crystal_capacity", OnCrystalUpdate);
                m_PropertyHandlers.Add("hide", OnHideUpdate);
                m_PropertyHandlers.Add("hide_capacity", OnHideUpdate);
                m_PropertyHandlers.Add("brick", OnBrickUpdate);
                m_PropertyHandlers.Add("score", OnScoreChange);
                m_PropertyHandlers.Add("achieve_reward_list", OnAchieveReward);
                m_PropertyHandlers.Add("open_area", OnOpenArea);
                m_PropertyHandlers.Add("day_task_refresh_times", OnBoardTaskRefreshTimes);
        }

        public void LoadDataFromMsg(SCMsgUserInfo msg)
        {
                m_PlayerID = msg.id;
                lastOfflineTimeStamp = msg.lastOfflineTimeStamp;
                
                m_GuideList = GameUtility.ToList<ulong>(msg.raidGuide, '|', (x) => ulong.Parse(x));
                InitPropertyHandlers();
                ////////////把属性回调逻辑注册放在属性注册前面，保证第一次赋值属性时逻辑也被运行//

                OnUpdateProperty("name", msg.name);
                OnUpdateProperty("gold", msg.gold);
                OnUpdateProperty("diamond", msg.diamond);
                OnUpdateProperty("three_star_list", msg.three_star_list);
                OnUpdateProperty("four_star_list", msg.four_star_list);
                OnUpdateProperty("five_star_list", msg.five_star_list);
                OnUpdateProperty("bag_capacity", msg.bag_capacity);
                OnUpdateProperty("maze_capacity", msg.maze_capacity);
                OnUpdateProperty("formula_list", msg.formula_list);
                OnUpdateProperty("ex_formula_list", msg.ex_formula_list);
                OnUpdateProperty("house_level", msg.house_level);
                OnUpdateProperty("house_exp", msg.house_exp);
                OnUpdateProperty("vitality", msg.vitality);
                OnUpdateProperty("gharry_lvl", msg.gharry_lvl);
                OnUpdateProperty("food", msg.food);

                OnUpdateProperty("gold_capacity", msg.gold_capacity);
                OnUpdateProperty("stone_capacity", msg.stone_capacity);
                OnUpdateProperty("wood_capacity", msg.wood_capacity);
                OnUpdateProperty("crystal_capacity", msg.crystal_capacity);
                OnUpdateProperty("hide_capacity", msg.hide_capacity);
                OnUpdateProperty("pet_capactity", msg.pet_capactity);
                OnUpdateProperty("stone", msg.stone);
                OnUpdateProperty("wood", msg.wood);
                OnUpdateProperty("crystal", msg.crystal);
                OnUpdateProperty("hide", msg.hide);
                OnUpdateProperty("brick", msg.brick);
                OnUpdateProperty("prosperity", msg.prosperity);
                OnUpdateProperty("score", msg.score);
                OnUpdateProperty("achieve_reward_list", msg.strAchieveRewardList);
                OnUpdateProperty("open_area", msg.open_area);
                OnUpdateProperty("day_task_refresh_times", msg.boardTaskRefreshTimes);
                OnUpdateProperty("day_inn_refresh_times", msg.day_inn_refresh_times);

                OnUpdateProperty("produce_augmentation_per", msg.produce_augmentation_per);
                OnUpdateProperty("cure_augmentation_per", msg.cure_augmentation_per);
                OnUpdateProperty("cure_lower_consume_per", msg.cure_lower_consume_per);
                OnUpdateProperty("bench_lower_consume_per", msg.bench_lower_consume_per);
                OnUpdateProperty("team_member_limit", msg.team_member_limit);
                OnUpdateProperty("skill_lower_consume_per", msg.skill_lower_consume_per);
                OnUpdateProperty("npc_lower_price", msg.npc_lower_price);
                OnUpdateProperty("npc_item_count", msg.npc_item_count);

                GameStateManager.GetInst().LoginScene = msg.nScene;
                Debug.Log("LoginScene=" + msg.nScene);
        }

        public bool IsGuideFinish(int guideId)
        {
                int idx = guideId / 64;
                if (idx < m_GuideList.Count)
                {
                        ulong flag = m_GuideList[idx];
                        int pos = guideId % 64;
                        return GameUtility.IsFlagOn(flag, pos);
                }
                return false;
        }
        public void SetRaidGuideOn(int guideId)
        {
                Debug.Log("SetRaidGuideOn " + guideId);
                int idx = guideId / 64;
                int pos = guideId % 64;

                if (idx < m_GuideList.Count)
                {
                        m_GuideList[idx] |= GameUtility.GetFlagVal(pos);
                }
                else
                {
                        int delta = idx - m_GuideList.Count;
                        for (int i = 0; i <= delta; i++)
                        {
                                ulong flag = 0;
                                if (i == delta)
                                {
                                        flag |= GameUtility.GetFlagVal(pos);
                                }
                                m_GuideList.Add(flag);
                        }
                }

                if (guideId == 5)
                {
                        UI_HomeMain uis = UIManager.GetInst().GetUIBehaviour<UI_HomeMain>();
                        if (uis != null)
                        {
                                uis.ShowFightBtn();
                        }
                }
        }
        public string GetRaidGuide()
        {
                if (m_GuideList.Count > 0)
                {
                        return GameUtility.ListToString<ulong>(m_GuideList, '|');
                }
                return "0";
        }

        #region PropertyHandler
        UI_HomeMain m_HomeMainBehaviour
        {
                get
                {
                        return UIManager.GetInst().GetUIBehaviour<UI_HomeMain>();
                }
        }

        void OnResourceUpdate(string name, string oldval, string newval)
        {
                if (m_HomeMainBehaviour != null)
                {
                        m_HomeMainBehaviour.RefreshResource(name, oldval, newval);
                }
        }

        void OnGoldUpdate(string name, string oldval, string newval)
        {
                OnResourceUpdate(name, oldval, newval);
                UI_HeroPub uhp = UIManager.GetInst().GetUIBehaviour<UI_HeroPub>();
                if (uhp != null)
                {
                        uhp.RefreshGold();
                }
        }

        void OnDiamondUpdate(string name, string oldval, string newval)
        {
                OnResourceUpdate(name, oldval, newval);
        }

        void OnWoodUpdate(string name, string oldval, string newval)
        {
                OnResourceUpdate(name, oldval, newval);
        }

        void OnStoneUpdate(string name, string oldval, string newval)
        {
                OnResourceUpdate(name, oldval, newval);
        }

        void OnHideUpdate(string name, string oldval, string newval)
        {
                OnResourceUpdate(name, oldval, newval);
        }

        void OnCrystalUpdate(string name, string oldval, string newval)
        {
                OnResourceUpdate(name, oldval, newval);
        }

        void OnBrickUpdate(string name, string oldval, string newval)
        {
                OnResourceUpdate(name, oldval, newval);
        }

        void OnThreeGet(string name, string oldval, string newval)
        {
                GetHero(name, oldval, newval, 1);
        }

        void OnFourGet(string name, string oldval, string newval)
        {
                GetHero(name, oldval, newval, 2);
        }

        void OnFiveGet(string name, string oldval, string newval)
        {
                GetHero(name, oldval, newval, 3);
        }

        void GetHero(string name, string oldval, string newval, int desk)
        {
                if (UIManager.GetInst().IsUIVisible("UI_HeroPub"))
                {
                        if (oldval.Length <= 0)
                        {
                                return;
                        }
                        string[] new_info = newval.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        string[] old_info = oldval.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < new_info.Length; i++)
                        {
                                if (new_info[i] != old_info[i])
                                {
                                        //说明需要改变//
                                        if (new_info[i].Equals(CommonString.zeroStr)) //说明招募成功//
                                        {
                                                HeroPubManager.GetInst().GetHero();
                                        }
                                        else
                                        {
                                                HeroPubManager.GetInst().ChangeHero(name, i + 1, int.Parse(new_info[i]), desk);
                                        }
                                }
                        }
                }

        }

        void OnHomeLevel(string name, string oldval, string newval)
        {
                if (GameStateManager.GetInst().GameState == GAMESTATE.HOME)
                {
                        if (m_HomeMainBehaviour != null)
                        {
                                m_HomeMainBehaviour.RefreshLevel();
                        }

                        //HomeManager.GetInst().CheckBar();
                        HomeManager.GetInst().CheckAllTaskNpcIcon();
                        //WorldMapManager.GetInst().RefreshRaid();
                }
        }

        void OnHomeExp(string name, string oldval, string newval)
        {
                if (m_HomeMainBehaviour != null)
                {
                        m_HomeMainBehaviour.RefreshExp();
                }
        }

        void OnHorseLevel(string name, string oldval, string newval)
        {
                WorldMapManager.GetInst().CountUnlockArea();
                if (GameStateManager.GetInst().GameState == GAMESTATE.HOME)
                {
                        WorldMapManager.GetInst().RefreshCanLockAreaPic();
                        WorldMapManager.GetInst().RefreshHorsePoint();
                }
        }


        void OnFood(string name, string oldval, string newval)
        {
                //if (m_HomeMainBehaviour != null)
                //{
                //        m_HomeMainBehaviour.RefreshFoodGroup();
                //}
        }


        void OnScoreChange(string name, string oldval, string newval)
        {

        }

        HashSet<int> GlobalGetAchieve = new HashSet<int>(); //全局领取过的成就
        void OnAchieveReward(string name, string oldval, string newval)
        {
                GlobalGetAchieve.Clear();
                string achieves = newval;
                if (!achieves.Equals(CommonString.zeroStr))
                {
                        GlobalGetAchieve = achieves.ToHashSet<int>('&', (s) => int.Parse(s));
                }

                UI_HeroMain uhm = UIManager.GetInst().GetUIBehaviour<UI_HeroMain>();
                if (uhm != null)
                {
                        uhm.RefreshAchievement();
                }
        }

        public bool HasGlobalAchieve(int achivee_id)
        {
                return GlobalGetAchieve.Contains(achivee_id);
        }
   
        void OnOpenArea(string name, string oldval, string newval)
        {
                AppMain.GetInst().StartCoroutine(HomeManager.GetInst().SetOpenArea());
        }

        void OnBoardTaskRefreshTimes(string name, string oldval, string newval)
        {
                UI_TaskBoard utb = UIManager.GetInst().GetUIBehaviour<UI_TaskBoard>();
                if (utb != null)
                {
                        utb.RefreshTimes();
                }
        }

        #endregion
}