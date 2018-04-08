using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Message;

//暂时只支持简单界面引导
class GuideManager : SingletonObject<GuideManager>
{
        Dictionary<int, GuideConfig> mGuideDict = new Dictionary<int, GuideConfig>();
        Dictionary<string, List<GuideConfig>> m_UIGuideDict = new Dictionary<string, List<GuideConfig>>();
        public void Init()
        {
                ConfigHoldUtility<GuideConfig>.LoadXml("Config/guide", mGuideDict); //资源类型表
                foreach (GuideConfig cfg in mGuideDict.Values)
                {
                        if (!m_UIGuideDict.ContainsKey(cfg.display_interface))
                        {
                                m_UIGuideDict.Add(cfg.display_interface, new List<GuideConfig>());
                        }
                        m_UIGuideDict[cfg.display_interface].Add(cfg);
                }
        }

        public GuideConfig GetGuideCfg(int id)
        {
                if (mGuideDict.ContainsKey(id))
                {
                        return mGuideDict[id];
                }
//                 if (id > 0)
//                 {
//                         singleton.GetInst().ShowMessage(ErrorOwner.designer, "引导" + id + "不存在");
//                 }
                return null;
        }
        bool CheckGuideAvailable(GuideConfig cfg)
        {
                if (cfg.pre_guide > 0 && !PlayerController.GetInst().IsGuideFinish(cfg.pre_guide))
                {
                        return false;
                }

                if (PlayerController.GetInst().GetPropertyInt("house_level") < cfg.home_level)
                {
                        return false;
                }

                if (PlayerController.GetInst().IsGuideFinish(cfg.id))
                {
                        return false;
                }

                return true;
        }

        public void CheckUIGuide(string windowName)
        {
                if (m_UIGuideDict.ContainsKey(windowName))
                {
                        foreach (GuideConfig cfg in m_UIGuideDict[windowName])
                        {
                                if (windowName.Equals(cfg.display_interface))
                                {
                                        if (cfg.pre_step > 0)
                                                continue;

                                        if (CheckGuideAvailable(cfg))
                                        {
                                                StartGuide(cfg, cfg.id);
                                                return;
                                        }
                                }
                        }
                }
        }

        public void CheckUIGuideClose(GameObject uiObj)
        {
                UI_Guide uis = UIManager.GetInst().GetUIBehaviour<UI_Guide>();
                if (uis != null)
                {
                        uis.CloseGuide(uiObj);
                }
        }

        public void GotoNextGuide(int id, int startId)
        {
                GuideConfig mGuide = GetGuideCfg(id);
                if (mGuide != null)
                {
                        if (PlayerController.GetInst().GetPropertyInt("house_level") >= mGuide.home_level)
                        {
                                StartGuide(mGuide, startId);
                        }
                }
        }

        void StartGuide(GuideConfig mGuide, int startId)
        {
                Debug.Log("StartGuide " + mGuide.id + " startId=" + startId);
                AppMain.GetInst().StartCoroutine(WaitForUIGuide(mGuide, startId));
        }

        IEnumerator WaitForUIGuide(GuideConfig mGuide, int startId)  //防止窗口没被打开或者窗口layout还不准确
        {
                yield return new WaitForSeconds(0.1f);
                UIManager.GetInst().ShowUI<UI_Guide>("UI_Guide").SetUIGuide(mGuide, startId);
        }

        public void CheckRaidPieceGuide(int pieceCfgId)
        {
                foreach (GuideConfig cfg in mGuideDict.Values)
                {
                        if (cfg.raid_piece_id == pieceCfgId)
                        {
                                if (CheckGuideAvailable(cfg))
                                {
                                        StartRaidGuide(cfg);
                                        return;
                                }
                        }
                }
        }

        Dictionary<GameObject, UI_GuideScene> m_UIGuideSceneList = new Dictionary<GameObject, UI_GuideScene>();

        public bool IsAlreadyExist(int id)
        {
                foreach (UI_GuideScene uis in m_UIGuideSceneList.Values)
                {
                        if (uis != null)
                        {
                                if (uis.GetGuideId() == id)
                                {
                                        return true;
                                }
                        }
                }
                return false;
        }
        public void RemoveNpcGuide(GameObject npcObj)
        {
                if (m_UIGuideSceneList.ContainsKey(npcObj))
                {
                        m_UIGuideSceneList[npcObj].OnClickNode();
                        GameObject.Destroy(m_UIGuideSceneList[npcObj]);
                        m_UIGuideSceneList.Remove(npcObj);
                }
        }

        void TriggerHomeNpcGuide(GuideConfig cfg, GameObject npcObj)
        {
                if (npcObj == null)
                {
                        return;
                }

                if (cfg.type == 3)
                {
                        if (IsAlreadyExist(cfg.id))
                        {
                                return;
                        }

                        if (CheckGuideAvailable(cfg))
                        {
                                GameObject uiObj = UIManager.GetInst().ShowUI_Multiple<UI_GuideScene>("UI_GuideScene");
                                uiObj.transform.SetParent(npcObj.transform);
                                uiObj.name = "HomeNpcGuide";
                                GameUtility.SetLayer(uiObj, "UI");
                                UI_GuideScene uis = uiObj.GetComponent<UI_GuideScene>();
                                uis.SetupNpc(npcObj, cfg);

                                if (!m_UIGuideSceneList.ContainsKey(npcObj))
                                {
                                        m_UIGuideSceneList.Add(npcObj, uis);
                                }

                                UI_Guide uiguide = UIManager.GetInst().ShowUI<UI_Guide>();
                                uiguide.SetSceneGuide(cfg);
                        }
                }
        }

        public void CheckHomeNpcGuide(GameObject npcObj)
        {
                Debug.Log("CheckHomeNpcGuide " + npcObj.name);
                foreach (GuideConfig cfg in mGuideDict.Values)
                {
                        TriggerHomeNpcGuide(cfg, npcObj);
                }
        }

        void StartRaidGuide(GuideConfig cfg)
        {
                Debug.Log("StartRaidGuide " + cfg.id);
                RaidNodeBehav node = RaidManager.GetInst().GetNodeInFocusRoom(cfg.common_element_id);
                if (node != null && node.GuideUI == null)
                {
                        GameObject uiObj = UIManager.GetInst().ShowUI_Multiple<UI_GuideScene>("UI_GuideScene");
                        uiObj.transform.SetParent(node.transform);
                        uiObj.name = "ElemGuide" + node.id;
                        GameUtility.SetLayer(uiObj, "UI");
                        UI_GuideScene uis = uiObj.GetComponent<UI_GuideScene>();
                        uis.Setup(node, cfg);

                        UI_Guide uiguide = UIManager.GetInst().ShowUI<UI_Guide>();
                        uiguide.SetSceneGuide(cfg);
                }
        }

        void SaveSubGuide(GuideConfig cfg)
        {
                if (cfg != null)
                {
                        if (PlayerController.GetInst().IsGuideFinish(cfg.id))
                                return;

                        PlayerController.GetInst().SetRaidGuideOn(cfg.id);
                        if (cfg.next > 0)
                        {
                                SaveSubGuide(GetGuideCfg(cfg.next));
                        }
                }
        }

        public void SaveGuide(int id)
        {
                Debug.Log("SaveGuide " + id);                
                GuideConfig cfg = GetGuideCfg(id);
                if (cfg != null)
                {
                        PlayerController.GetInst().SetRaidGuideOn(cfg.id);
                        if (cfg.next > 0)
                        {
                                SaveSubGuide(GetGuideCfg(cfg.next));
                        }

                        if (cfg.trigger_guide > 0)
                        {
                                GuideConfig triggerGuide = GetGuideCfg(cfg.trigger_guide);
                                if (triggerGuide != null)
                                {
                                        if (triggerGuide.type == 3)
                                        {                                                
                                                GameObject npcObj = HomeManager.GetInst().GetNpcObjByType(3);
                                                TriggerHomeNpcGuide(triggerGuide, npcObj);
                                        }
                                }
                        }
                }

                CSMsgGuide msg = new CSMsgGuide();
                msg.guideId = 0;
                msg.raidguide = PlayerController.GetInst().GetRaidGuide();                
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

}
