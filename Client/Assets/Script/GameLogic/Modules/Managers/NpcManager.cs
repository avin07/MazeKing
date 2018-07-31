using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Message;

public struct NpcInfo
{
        public long id;
        public int idConfig;
        public long buildId;
        public int restTime;
}

class NpcManager : SingletonObject<NpcManager>
{
        Dictionary<int, NpcConfig> m_NpcCfgDict = new Dictionary<int, NpcConfig>();
        Dictionary<int, TradeConfig> m_TradeDict = new Dictionary<int, TradeConfig>();
        Dictionary<int, NpcHouseVisitorHold> m_VisitorCfg = new Dictionary<int, NpcHouseVisitorHold>();
        public Dictionary<int, CallerRecruitConfig> m_CallerRecruitConfig = new Dictionary<int, CallerRecruitConfig>();
        Dictionary<long, NpcInfo> m_NpcDict = new Dictionary<long, NpcInfo>();
        public void Init()
        {
                ConfigHoldUtility<NpcConfig>.LoadXml("Config/npc", m_NpcCfgDict);
                ConfigHoldUtility<TradeConfig>.LoadXml("Config/trade", m_TradeDict);
                ConfigHoldUtility<NpcHouseVisitorHold>.LoadXml("Config/npc_house_visitor", m_VisitorCfg);
                ConfigHoldUtility<CallerRecruitConfig>.LoadXml("Config/caller_recruit", m_CallerRecruitConfig);  //npc星级


                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgNpcInfo), OnNpcInfo); //npc
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgNpcDisappear), OnNpcDisappear); //NPC消失
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRecruitPet), OnNpcHeroGet);
            

                //NPC反馈//
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgNpcGoodsInfo), OnNpcTradeInfo); //交易NPC
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgNpcVisitInfo), OnNpcVisitInfo); //上门NPC
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRecruitInfo), OnNpcRecruitInfo); //招募Npc
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgNpcTaskInfo), OnNpcTaskInfo); //任务npc
           
        }

        public NpcConfig GetNpcCfg(int cfgId)
        {
                if (m_NpcCfgDict.ContainsKey(cfgId))
                {
                        return m_NpcCfgDict[cfgId];
                }
                return null;
        }
        public TradeConfig GetTradeCfg(int cfgId)
        {
                if (m_TradeDict.ContainsKey(cfgId))
                {
                        return m_TradeDict[cfgId];
                }
                return null;
        }

        public NpcHouseVisitorHold GetVisitorCfg(int id)
        {
                if (m_VisitorCfg.ContainsKey(id))
                {
                        return m_VisitorCfg[id];
                }
                return null;
        }

        public CallerRecruitConfig GetCallerRecruitCfg(int id)
        {
                if (m_CallerRecruitConfig.ContainsKey(id))
                {
                        return m_CallerRecruitConfig[id];
                }
                return null;
        }

        public Dictionary<long, NpcInfo> GetNpcDict() 
        {
                return m_NpcDict;
        }

        public NpcInfo GetNpc(long id)
        {
                if (m_NpcDict.ContainsKey(id))
                {
                        return m_NpcDict[id];
                }
                return new NpcInfo();
        }

        void OnNpcTradeInfo(object sender, SCNetMsgEventArgs e)
        {
                HomeManager.GetInst().ResetNpcMessager();

                SCMsgNpcGoodsInfo msg = e.mNetMsg as SCMsgNpcGoodsInfo;
                NpcConfig npcCfg = GetNpcCfg(msg.idConfig);
                if (npcCfg != null)
                {
                        UI_NpcTrade uis = UIManager.GetInst().GetUIBehaviour<UI_NpcTrade>();
                        if (uis == null)
                        {
                                uis = UIManager.GetInst().ShowUI<UI_NpcTrade>("UI_NpcTrade");
                        }
                        uis.SetTrade(msg.id, msg.strGoodsList, npcCfg);
                }
        }

        void OnNpcVisitInfo(object sender, SCNetMsgEventArgs e)
        {
                HomeManager.GetInst().ResetNpcMessager();

                SCMsgNpcVisitInfo msg = e.mNetMsg as SCMsgNpcVisitInfo;
                int idConfig = GetNpc(msg.id).idConfig;
                UIManager.GetInst().ShowUI<UI_NpcVisitor>("UI_NpcVisitor").Refresh(msg.id, idConfig, msg.strGoodsList);
        }

        void OnNpcRecruitInfo(object sender, SCNetMsgEventArgs e)
        {
                HomeManager.GetInst().ResetNpcMessager();

                SCMsgRecruitInfo msg = e.mNetMsg as SCMsgRecruitInfo;
                UIManager.GetInst().ShowUI<UI_NpcRecruit>("UI_NpcRecruit").Refresh(msg.idnpc);

        }

        void OnNpcTaskInfo(object sender, SCNetMsgEventArgs e)
        {
                HomeManager.GetInst().ResetNpcMessager();

                SCMsgNpcTaskInfo msg = e.mNetMsg as SCMsgNpcTaskInfo;
                //切换到近景
                TaskManager.GetInst().ClickTaskNpcFeedback(msg.idNpcCfg, msg.idNpc);
        }


        HashSet<long> NewNpc = new HashSet<long>();

        public bool IsNewNpc(long id)
        {
                if (NewNpc.Contains(id))
                {
                        return true;
                }
                return false;
        }

        public void RemoveNewNpc(long id)
        {
                if (NewNpc.Contains(id))
                {
                        NewNpc.Remove(id);
                }
        }

        void OnNpcInfo(object sender, SCNetMsgEventArgs e)
        {
                SCMsgNpcInfo msg = e.mNetMsg as SCMsgNpcInfo;
                NpcConfig npcCfg = GetNpcCfg(msg.idConfig);
                if (npcCfg.place == 1)
                {
                        return; //迷宫里的不管理
                }

                NpcInfo info;
                info.id = msg.id;
                info.buildId = msg.build_id;
                info.idConfig = msg.idConfig;
                info.restTime = npcCfg.live_days - msg.nLiveDay;

                if (npcCfg.type == 3) //招募类型的npc
                {
                        HomeManager.GetInst().AddHotelNpc(msg.id);
                }

                if (m_NpcDict.ContainsKey(msg.id))
                {
                        m_NpcDict[msg.id] = info;
                }
                else
                {
                        m_NpcDict.Add(msg.id,info);

                        if (GameStateManager.GetInst().GameState > GAMESTATE.OTHER) //记录新的npc
                        {
                                NewNpc.Add(msg.id);
                                GameUtility.AddNotify(NotifyType.NewNpc, NewNpcTip, "有访客在门口",null);
                        }

                        if (GameStateManager.GetInst().GameState == GAMESTATE.HOME)
                        {
                                HomeManager.GetInst().SetNpcModelOne(info);   //创建npc模型
                        }
                }   
        }


        void NewNpcTip(object data)
        {
                AppMain.GetInst().StartCoroutine(HomeManager.GetInst().GotoHomeEntrance());
        }

        void OnNpcDisappear(object sender, SCNetMsgEventArgs e)
        {
                SCMsgNpcDisappear msg = e.mNetMsg as SCMsgNpcDisappear;

                HomeManager.GetInst().DeleteNpcObj(msg.id);
                m_NpcDict.Remove(msg.id);

                UI_NpcTrade uis = UIManager.GetInst().GetUIBehaviour<UI_NpcTrade>();
                if (uis != null && uis.NpcId == msg.id)
                {
                        UIManager.GetInst().CloseUI("UI_NpcTrade");
                }

                UI_NpcRecruit unr = UIManager.GetInst().GetUIBehaviour<UI_NpcRecruit>();
                if (unr != null && unr.npcId == msg.id)
                {
                        unr.OnClose(null,null);
                }
        }

        void OnNpcHeroGet(object sender, SCNetMsgEventArgs e)
        {
                //招募成功
                SCMsgRecruitPet msg = e.mNetMsg as SCMsgRecruitPet;
                long id = msg.idnpc;
                GameObject obj = HomeManager.GetInst().GetNpcObj(id);
                EffectManager.GetInst().PlayEffect("pub_disappear_effect", obj.transform);
                HomeManager.GetInst().DeleteNpcObj(id, 1.0f);
                AppMain.GetInst().StartCoroutine(WaitForClose(2.0f, UIManager.GetInst().GetUIBehaviour<UI_NpcRecruit>()));

        }

        IEnumerator WaitForClose(float time, UI_NpcRecruit unr)
        {
                yield return new WaitForSeconds(time);
                if (unr != null)
                {
                        unr.OnClose(null, null);
                }
        }

        public void SendHomeVisitor(long id, string target) //访客交货
        {
                CSMsgNpcVisit msg = new CSMsgNpcVisit();
                msg.id = id;
                msg.target = target;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

        public void SendTrade(long id, string selllist, string buylist)
        {
                CSMsgNpcTrade msg = new CSMsgNpcTrade();
                msg.id = id;
                msg.strSellList = selllist;
                msg.strBuyList = buylist;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

        public void NpcTrigger(long id)
        {
                CSMsgNpcTrigger msg = new CSMsgNpcTrigger();
                msg.id = id;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

}
