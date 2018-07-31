using UnityEngine;
using System.Collections;
using System.Collections.Generic;
class CutSceneManager : SingletonObject<CutSceneManager>
{
        public enum CUTSCENE_STATE
        {
                NONE,
                PREPARE,
                PLAYING,
                WAITING,
                END,
        };

        Dictionary<int, CutSceneConfig> m_ConfigDict = new Dictionary<int, CutSceneConfig>();
        
        public void Init()
        {
                ConfigHoldUtility<CutSceneConfig>.LoadXml("Config/cutscene", m_ConfigDict);
        }

        CutSceneConfig m_CurrentCfg;
        CUTSCENE_STATE m_State = CUTSCENE_STATE.NONE;
        public void SetState(CUTSCENE_STATE state)
        {
                m_State = state;
                //Debuger.Log("CutSceneState = " + state);
        }


        CutSceneConfig GetCutSceneConfig(int id)
        {
                if (m_ConfigDict.ContainsKey(id))
                {
                        return m_ConfigDict[id];
                }
                return null;
        }

        int m_nIdx = 0;
        int m_nStoryId = 0;
        int m_nGotoIndex = 0;
        public void GotoCmdIndex(int idx)
        {
                if (m_CurrentCmd != null)
                {
                        if (m_CurrentCmd is CutSceneCmd_Dialog)
                        {
                                CutSceneCmd_Dialog dialogCmd = (CutSceneCmd_Dialog)m_CurrentCmd;
                                switch (idx)
                                {
                                        case 0:
                                                m_nGotoIndex = dialogCmd.option0_goto;
                                                break;
                                        case 1:
                                                m_nGotoIndex = dialogCmd.option1_goto;
                                                break;
                                        case 2:
                                                m_nGotoIndex = dialogCmd.option2_goto;
                                                break;
                                }
                                Debuger.Log("GotoIndex = " + m_nGotoIndex);
                        }
                }
        }

        CutSceneCommand m_CurrentCmd;
        float m_fMaxWaitTime = 0f;
        float m_fTime;
        int m_nNodeId = 0;
        public int NodeId
        {
                get { return m_nNodeId; }
        }
        Quaternion m_CameraRot;
        public Quaternion CameraOriRot
        {
                get { return m_CameraRot; }
        }
        bool m_bLocalCutScene = false;

        public RaidRoomBehav CutSceneRoom;
        public bool EnterCutscene(int nodeId, int id, RaidRoomBehav room, bool bLocal)
        {
                m_CurrentCfg = GetCutSceneConfig(id);
                m_bLocalCutScene = bLocal;
                if (m_CurrentCfg != null)
                {
                        m_CameraRot = Camera.main.transform.rotation;
                        m_nNodeId = nodeId;
                        CutSceneRoom = room;
                        
                        m_nIdx = 0;
                        UIManager.GetInst().ShowUI<UI_CutSceneMask>("UI_CutSceneMask");
                        GameStateManager.GetInst().GameState = GAMESTATE.RAID_CUTSCENE;
                        Prepare();
                        return true;
                }
                else
                {
                        Debuger.LogError("EnterCutScene Fail id=" + id);
                        return false;
                }
        }

        void Prepare()
        {
                m_nIdx = 0;
                m_nResult = 0;
                m_CurrentCmd = null;
                SetState(CUTSCENE_STATE.PLAYING);
        }

        CutSceneCommand GetNextCmd()
        {
                if (m_nGotoIndex > 0)
                {
                        for (int i = 0; i < m_CurrentCfg.cmdlist.Count; i++)
                        {
                                CutSceneCommand cmd = m_CurrentCfg.cmdlist[i];
                                if (cmd.index == m_nGotoIndex)
                                {
                                        m_nIdx = m_CurrentCfg.cmdlist.IndexOf(cmd);
                                        m_nGotoIndex = 0;
                                        return cmd;
                                }
                        }
                }
                if (m_CurrentCmd != null)
                {
                        if (m_CurrentCmd.next_index > 0)
                        {
                                for (int i = 0; i < m_CurrentCfg.cmdlist.Count; i++)
                                {
                                        CutSceneCommand cmd = m_CurrentCfg.cmdlist[i];
                                        if (cmd.index == m_CurrentCmd.next_index)
                                        {
                                                m_nIdx = m_CurrentCfg.cmdlist.IndexOf(cmd);
                                                return cmd;
                                        }
                                }
                        }
                }
                if (m_nIdx < m_CurrentCfg.cmdlist.Count)
                {
                        return m_CurrentCfg.cmdlist[m_nIdx];
                }
                return null;
        }

        public void Update()
        {
                switch (m_State)
                {
                        case CUTSCENE_STATE.NONE:
                                break;
                        case CUTSCENE_STATE.PREPARE:
                                break;
                        case CUTSCENE_STATE.PLAYING:
                                {
                                        ExecCmd(GetNextCmd());
                                }
                                break;
                        case CUTSCENE_STATE.WAITING:
                                if (Time.realtimeSinceStartup - m_fTime >= m_fMaxWaitTime)
                                {
                                        SetState(CUTSCENE_STATE.PLAYING);
                                }
                                break;
                        case CUTSCENE_STATE.END:
                                {
                                        End();
                                }
                                break;
                }

                CutSceneCameraManager.GetInst().Update();
        }

        void End()
        {
                Time.timeScale = 1f;
                if (m_bLocalCutScene == false)
                {
                        Message.CSMsgRaidStoryOption msg = new Message.CSMsgRaidStoryOption();
                        msg.result = m_nResult;
                        NetworkManager.GetInst().SendMsgToServer(msg);
                }
                ClearUnits();
                DestroyAllEffect();
                RaidManager.GetInst().ExitCutscene();
                UIManager.GetInst().CloseUI("UI_CutSceneMask");
                UIManager.GetInst().CloseUI("UI_CutSceneDialog");
                SetState(CUTSCENE_STATE.NONE);
                //Camera.main.transform.rotation = m_CameraRot;
                foreach (HeroUnit unit in RaidTeamManager.GetInst().GetHeroList())
                {
                        if (unit.Mod != null)
                        {
                                unit.transform.position = unit.Mod.transform.position;
                                unit.transform.rotation = unit.Mod.transform.rotation;
                                unit.UpdateCurrentPos();
                                unit.Mod.transform.SetParent(unit.transform);
                                unit.Mod.transform.localPosition = Vector3.zero;
                                unit.Mod.transform.localRotation = Quaternion.identity;
                        }
                }
        }

        void ExecCmd(CutSceneCommand cmd)
        {
                if (cmd == null)
                {
                        SetState(CUTSCENE_STATE.END);
                        return;
                }
                m_CurrentCmd = cmd;
                if (cmd.isWait)
                {
                        m_fMaxWaitTime = float.MaxValue;
                        SetState(CUTSCENE_STATE.WAITING);
                }

                cmd.Exec();
                m_nIdx++;
        }

        public void EndWaiting()
        {
                if (m_State == CUTSCENE_STATE.WAITING)
                {
                        SetState(CUTSCENE_STATE.PLAYING);
                }
        }
        public void SetWaitTime(float maxTime)
        {
                m_fTime = Time.realtimeSinceStartup;
                m_fMaxWaitTime = maxTime;
        }

        public GameObject GetTargetObj(string target)
        {
                GameObject unitObj = null;
                if (m_UnitDict.ContainsKey(target))
                {
                        CS_UNIT unit = m_UnitDict[target];
                        unitObj = unit.obj;
                }
                else if (target.Contains("Node_"))
                {
                        int nodeId = 0;
                        int.TryParse(target.Replace("Node_", ""), out nodeId);
                        if (nodeId > 0)
                        {
                                RaidNodeBehav node = RaidManager.GetInst().GetRaidNodeBehav(CutSceneManager.GetInst().CutSceneRoom.idOffset + nodeId);
                                //Debuger.Log(CutSceneManager.GetInst().IdOffset + nodeId);
                                if (node != null)
                                {
                                        if (node.elemCfg != null)
                                        {
                                                unitObj = node.elemObj;
                                        }
                                }
                        }
                }
                else if (target.Contains("Team_"))
                {
                        int teamIdx = 0;
                        int.TryParse(target.Replace("Team_", ""), out teamIdx);
                        HeroUnit unit = RaidTeamManager.GetInst().GetHeroUnit(teamIdx);
                        if (unit != null)
                        {
                                unitObj = unit.gameObject;
                        }
                }
                if (unitObj == null)
                {
/*                        Debuger.LogError("GetUnit == null target=" + target);*/
                }
                return unitObj;
        }

        int m_nResult = 0;
        public void Quit(int result)
        {
                m_nResult = result;
                SetState(CUTSCENE_STATE.END);
        }

        Dictionary<string, CS_UNIT> m_UnitDict = new Dictionary<string, CS_UNIT>();
        public CS_UNIT GetUnit(string target)
        {
                if (m_UnitDict.ContainsKey(target))
                {
                        return m_UnitDict[target];
                }
                return null;
        }

        public void AddUnit(string name, int modelId, GameObject obj)
        {
                if (!m_UnitDict.ContainsKey(name))
                {
                        m_UnitDict.Add(name, new CS_UNIT(name, modelId, obj));
                }
        }
        public void DeleteUnit(string name)
        {
                if (m_UnitDict.ContainsKey(name))
                {
                        GameObject.Destroy(m_UnitDict[name].obj);
                        m_UnitDict.Remove(name);
                }
        }
        public void ClearUnits()
        {
                foreach (CS_UNIT unit in m_UnitDict.Values)
                {
                        GameObject.Destroy(unit.obj);
                }
                m_UnitDict.Clear();
        }



        Dictionary<string, GameObject> m_EffectDict = new Dictionary<string, GameObject>();
        List<GameObject> m_EffectList = new List<GameObject>();
        public void AddEffect(string effect_id, GameObject effectObj)
        {
                m_EffectList.Add(effectObj);
                if (!string.IsNullOrEmpty(effect_id))
                {
                        if (!m_EffectDict.ContainsKey(effect_id))
                        {
                                m_EffectDict.Add(effect_id, effectObj);
                        }
                }
        }
        public void StopEffect(string effect_id)
        {
                if (!string.IsNullOrEmpty(effect_id))
                {
                        if (m_EffectDict.ContainsKey(effect_id))
                        {
                                GameObject.Destroy(m_EffectDict[effect_id]);
                                m_EffectDict.Remove(effect_id);
                        }
                }
        }
        public void DestroyAllEffect()
        {
                foreach (GameObject effectObj in m_EffectList)
                {
                        if (effectObj != null)
                        {
                                GameObject.Destroy(effectObj);
                        }
                }
                m_EffectDict.Clear();
                m_EffectList.Clear();
        }
}
public class CS_UNIT
{
        public GameObject obj;
        public int model_id;
        public string name;
        public CS_UNIT(string _name, int id, GameObject _obj)
        {
                name = _name;
                model_id = id;
                obj = _obj;
        }
}

