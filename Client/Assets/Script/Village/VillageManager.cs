using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Message;

public class VillageManager : SingletonObject<VillageManager>
{
        #region Config
        Dictionary<int, VillageConfig> m_VillageDict = new Dictionary<int, VillageConfig>();
        Dictionary<int, VillageTargetConfig> m_VillageTargetDict = new Dictionary<int, VillageTargetConfig>();
        Dictionary<int, VillageBuildingConfig> m_VillageBuildingDict = new Dictionary<int, VillageBuildingConfig>();
        Dictionary<int, VillageElementConfig> m_VillageElementDict = new Dictionary<int, VillageElementConfig>();
        Dictionary<int, VillageElementOptionConfig> m_VillageElementOptionDict = new Dictionary<int, VillageElementOptionConfig>();
        void InitConfig()
        {
                ConfigHoldUtility<VillageConfig>.LoadXml("Config/npc_village", m_VillageDict);
                ConfigHoldUtility<VillageTargetConfig>.LoadXml("Config/npc_village_target", m_VillageTargetDict);
                ConfigHoldUtility<VillageBuildingConfig>.LoadXml("Config/npc_village_building", m_VillageBuildingDict);
                ConfigHoldUtility<VillageElementConfig>.LoadXml("Config/charactar_element", m_VillageElementDict);
                ConfigHoldUtility<VillageElementOptionConfig>.LoadXml("Config/charactar_element_option", m_VillageElementOptionDict);
        }
        public VillageConfig GetVillageCfg(int id)
        {
                if (m_VillageDict.ContainsKey(id))
                {
                        return m_VillageDict[id];
                }
                return null;
        }
        VillageBuildingConfig GetVillageBuildingCfg(int id)
        {
                if (m_VillageBuildingDict.ContainsKey(id))
                {
                        return m_VillageBuildingDict[id];
                }
                return null;
        }
        VillageElementConfig GetVillageElementCfg(int id)
        {
                if (m_VillageElementDict.ContainsKey(id))
                {
                        return m_VillageElementDict[id];
                }
                return null;
        }
        public VillageElementOptionConfig GetVillageElementOptionCfg(int id)
        {
                if (m_VillageElementOptionDict.ContainsKey(id))
                {
                        return m_VillageElementOptionDict[id];
                }
                return null;
        }
        VillageTargetConfig GetVillageTargetCfg(int id)
        {
                if (m_VillageTargetDict.ContainsKey(id))
                {
                        return m_VillageTargetDict[id];
                }
                return null;
        }
        #endregion

        enum VILLAGE_STATE
        {
                NONE,
                PREVIEW,
                INVADE,
                MAX
        };
        string SceneName = "80001";

        int m_nVillageID = 1;
        bool m_bLoading = false;
        float m_fCameraDist;
        string m_sTeamInfo = "";
        VILLAGE_STATE m_VillageState = VILLAGE_STATE.NONE;
        int m_nMode = 1;        //0无 1预览 2开战
        VillageConfig m_VillageCfg;

        bool IsNpcVillage = true;
        GameObject m_ActionUnitSelectEffect;
        GameObject ActionUnitSelectEffect
        {
                get
                {
                        if (m_ActionUnitSelectEffect == null)
                        {
                                m_ActionUnitSelectEffect = EffectManager.GetInst().GetEffectObj("effect_battle_teammate_circle_001");
                        }
                        return m_ActionUnitSelectEffect;
                }
        }
        HeroUnit m_MainHero;
        public HeroUnit MainHero
        {
                get
                {
                        return m_MainHero;
                }
                set
                {
                        m_MainHero = value;
                        if (m_MainHero != null)
                        {
                                ActionUnitSelectEffect.transform.SetParent(MainHero.transform);
                                ActionUnitSelectEffect.transform.localPosition = Vector3.up * 0.2f;
                        }
                }
        }
        GameObject m_SceneRoot;
        UI_VillageMain m_UIMain;

        Dictionary<int, VillagerBehaviour> m_VillagerDict = new Dictionary<int, VillagerBehaviour>();

        public bool IsLoadUnfinish = false;

        Vector3 GetPos(int pos)
        {
                return new Vector3(pos / 10000, pos % 100, pos % 10000 / 100);
        }


        #region 消息收发

        void InitMsg()
        {
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgNpcVillageEnter), OnNpcVillageEnter);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgPlayerVillageEnter), OnPlayerVillageEnter);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgVillageTeam), OnVillageTeam);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgVillageState), OnVillageState);
        }


        void OnNpcVillageEnter(object sender, SCNetMsgEventArgs e)
        {
                SCMsgNpcVillageEnter msg = e.mNetMsg as SCMsgNpcVillageEnter;
                m_nVillageID = msg.idCfg;
                IsNpcVillage = true;
                AppMain.GetInst().StartCoroutine(EnterVillage());
        }
        void OnPlayerVillageEnter(object sender, SCNetMsgEventArgs e)
        {
                SCMsgPlayerVillageEnter msg = e.mNetMsg as SCMsgPlayerVillageEnter;
                m_nVillageID = 0;
                IsNpcVillage = false;
                m_VillageState = VILLAGE_STATE.NONE;
                AppMain.GetInst().StartCoroutine(EnterVillage(msg.landInfo, msg.buildInfo, msg.villagerInfo));
        }

        float m_fTime;
        int m_nRemainTime = 0;
        void OnVillageState(object sender, SCNetMsgEventArgs e)
        {
                SCMsgVillageState msg = e.mNetMsg as SCMsgVillageState;
                
                //非加载状态时直接切换
                if (m_VillageState == VILLAGE_STATE.NONE && m_bLoading)
                {
                        m_VillageState = (VILLAGE_STATE)msg.state;
                }
                else
                {
                        m_VillageState = (VILLAGE_STATE)msg.state;
                        ChangeVillageState();
                }
                m_fTime = Time.realtimeSinceStartup;
                m_nRemainTime = msg.remainTime;
        }

        void OnVillageTeam(object sender, SCNetMsgEventArgs e)
        {
                RaidTeamManager.GetInst().ClearTeam();
                SCMsgVillageTeam msg = e.mNetMsg as SCMsgVillageTeam;
                m_sTeamInfo = msg.info;
        }
        public void SendOption(int option, VillagerBehaviour villager)
        {
                CSMsgVillageChoose msg = new CSMsgVillageChoose();
                msg.idCfg = option;
                msg.idHero = 0;
                msg.idVillager = villager.Index;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

        public void SendLeave()
        {
                NetworkManager.GetInst().SendMsgToServer(new CSMsgVillageLeave());
        }

        public void SendInvade()
        {
                NetworkManager.GetInst().SendMsgToServer(new CSMsgVillageInvade());
        }

        public void SendVillageTrigger(int id)
        {
                CSMsgVillageTrigger msg = new CSMsgVillageTrigger();
                msg.idVillager = id;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

        #endregion
        public void Init()
        {
                InitConfig();
                InitMsg();

                m_fCameraDist = GlobalParams.GetFloat("camera_focus_dist");
        }

        void InitTerrain(string landInfo, bool bServer = false)
        {
                HomeManager.GetInst().GetBrickHeightData(landInfo);
                HomeManager.GetInst().SetHomeRoot(SceneName);
                HomeManager.GetInst().SetAmbient();
                HomeManager.GetInst().SetHomeCamera();
                HomeManager.GetInst().SetNpcBrick(m_VillageCfg);  //生成地形//
                //HomeManager.GetInst().SetHomeLight();
                HomeManager.GetInst().SetState(HomeState.None);
                HomeManager.GetInst().InputBinding();
        }
        void InitPlayerBuildings(string buildingStr)
        {
                string tmpStr = GameUtility.GzipDecompress(buildingStr);
                Debuger.Log(tmpStr);
                string[] infos = tmpStr.Split('|');
                long idx = 0;
                foreach (string info in infos)
                {
                        if (string.IsNullOrEmpty(info))
                                continue;
                        string[] tmps = info.Split('&');
                        if (tmps.Length == 3)
                        {
                                int idCfg = int.Parse(tmps[0]);
                                int level = int.Parse(tmps[1]);
                                int pos = int.Parse(tmps[2]);
                                if (HomeManager.GetInst().GetBuildTypeCfg(idCfg) != null)
                                {
                                        HomeManager.GetInst().SetNpcBuild(new BuildInfo(idx, pos, idCfg, level,0));
                                }
                        }
                }

        }

        void InitNpcBuildings()
        {
                VillageConfig cfg = GetVillageCfg(m_nVillageID);
                if (cfg != null)
                {
                        int idx = 0;

                        string[] infos = cfg.building_position.Split(';');
                        foreach (string info in infos)
                        {
                                if (string.IsNullOrEmpty(info))
                                        continue;
                                string[] tmps = info.Split(',');
                                if (tmps.Length == 3)
                                {
                                        VillageBuildingConfig buildingCfg = GetVillageBuildingCfg(int.Parse(tmps[0]));
                                        if (buildingCfg != null)
                                        {
                                                if (HomeManager.GetInst().GetBuildTypeCfg(buildingCfg.building_type) != null)
                                                {
                                                        int pos = int.Parse(tmps[1]);
                                                        int layout = int.Parse(tmps[2]);
                                                        BuildInfo buildInfo = new BuildInfo(idx, int.Parse(tmps[0]), buildingCfg.building_type, buildingCfg.level,layout); //id,位置，配置id,工作等级
                                                        GameObject obj = HomeManager.GetInst().SetNpcBuild(buildInfo);
                                                        obj.transform.position = GetPos(pos);
                                                }
                                        }
                                }
                        }
                }
        }
        void InitPlayerVillagers(string info)
        {
                Dictionary<int, int> dict = GameUtility.ParseCommonStringToDict(info, '|', '&');
                int idx = 1;
                foreach (var param in dict)
                {
                        CharacterConfig charCfg = CharacterManager.GetInst().GetCharacterCfg(param.Key);
                        if (charCfg != null)
                        {
                                GameObject obj = new GameObject("Villager" + param.Key);

                                VillagerBehaviour vb = obj.AddComponent<VillagerBehaviour>();
                                vb.Index = idx;
                                vb.elemCfg = GetVillageElementCfg(charCfg.character_element);
                                vb.CharacterID = charCfg.id;
                                vb.CharacterCfg = charCfg;
                                //                                 int x = param.Value / 100;
                                //                                 int z = param.Value % 100;

                                vb.transform.SetParent(HomeManager.GetInst().HomeRoot.transform);
                                //                                vb.transform.position = new Vector3(x, HomeManager.GetInst().GetHeightByPos(new Vector2(x, z)), z);
                                vb.SetModel();

                                m_VillagerDict.Add(vb.Index, vb);
                                idx++;
                        }

                }
        }
        void InitNpcVillagers(string villagerInfo = "")
        {
                Dictionary<int, int> dict = null;
                VillageConfig cfg = GetVillageCfg(m_nVillageID);
                if (cfg != null)
                {
                        dict = GameUtility.ParseCommonStringToDict(cfg.charactar_element, ';', ',');
                }
                else if (!string.IsNullOrEmpty(villagerInfo))
                {
                        dict = GameUtility.ParseCommonStringToDict(villagerInfo, '|', '&');
                }
                int idx = 1;    //村民ID按照配置顺序来
                if (dict != null)
                {
                        foreach (var param in dict)
                        {
                                CharacterConfig charCfg = CharacterManager.GetInst().GetCharacterCfg(param.Key);
                                if (charCfg != null)
                                {
                                        GameObject obj = new GameObject("Villager" + param.Key);

                                        VillagerBehaviour vb = obj.AddComponent<VillagerBehaviour>();
                                        vb.Index = idx;
                                        vb.elemCfg = GetVillageElementCfg(charCfg.character_element);
                                        vb.CharacterID = charCfg.id;
                                        vb.CharacterCfg = charCfg;
                                        vb.transform.SetParent(HomeManager.GetInst().HomeRoot.transform);
                                        vb.transform.position = GetPos(param.Value);
                                        vb.SetModel();

                                        m_VillagerDict.Add(vb.Index, vb);

                                        GameUtility.RotateTowards(vb.transform, GetPos(m_VillageCfg.fight_position));
                                        
                                        idx++;
                                }
                        }
                }
        }


        IEnumerator EnterVillage(string landInfo = "", string buildInfo = "", string villagerInfo = "")
        {
                m_bLoading = true;

                WorldMapManager.GetInst().DestroyWorldMap();
                HomeManager.GetInst().DestroyHome();
                ResourceManager.GetInst().UnloadAllAB();

                m_VillagerDict.Clear();

                if (Camera.main != null)
                {
                        Camera.main.enabled = false;
                }

                AssetBundle ab = ResourceManager.GetInst().LoadAB("Scene/" + SceneName);
                //AsyncOperation async = Application.LoadLevelAdditiveAsync(SceneName);
                AsyncOperation async = Application.LoadLevelAsync(SceneName);
                yield return async;
                ab.Unload(false);
                yield return null;
                if (IsNpcVillage)
                {
                        m_VillageCfg = GetVillageCfg(m_nVillageID);
                        if (m_VillageCfg != null)
                        {
                                InitTerrain(m_VillageCfg.village_brick);
                                InitNpcBuildings();
                                InitNpcVillagers();
                        }
                }
                else
                {
                        m_VillageCfg = null;
                        InitTerrain(landInfo, true);
                        InitPlayerBuildings(buildInfo);
                        InitNpcVillagers(villagerInfo);
                }

                m_UIMain = UIManager.GetInst().ShowUI<UI_VillageMain>("UI_VillageMain");
                m_UIMain.ShowPreview();
                ChangeVillageState();
                m_bLoading = false;

                if (CombatManager.GetInst().HasUnfinishBattle)
                {
                        CombatManager.GetInst().HasUnfinishBattle = false;
                        NetworkManager.GetInst().SendMsgToServer(new CSMsgContinueBattle());
                }
                GameStateManager.GetInst().GameState = GAMESTATE.VILLAGE;
        }
        
        public void EnterInvadeMode()
        {
                m_VillageState = VILLAGE_STATE.INVADE;

                List<HeroUnit> herolist = RaidTeamManager.GetInst().InitTeamMember(m_sTeamInfo);
                MainHero = RaidTeamManager.GetInst().GetFirstHero();
                                
                m_UIMain.ShowInvade();
                m_UIMain.SetupHero(herolist);

                int idx = 0;
                foreach (HeroUnit unit in herolist)
                {
                        unit.transform.position = GetHeroPosition(idx, LINK_DIRECTION.SOUTH);
                        idx++;
                }

                Camera.main.gameObject.AddComponent<PhysicsRaycaster>();
                foreach (VillagerBehaviour vb in m_VillagerDict.Values)
                {
                        vb.SetUIVisible(true);
                        GameUtility.RotateTowards(vb.transform, MainHero.transform);
                }
                SwitchCameraToRaid();
        }

        public void RefreshTaskIcon()
        {
                foreach (VillagerBehaviour vb in m_VillagerDict.Values)
                {
                        vb.RefreshTaskIcon();
                }
        }

        VillagerBehaviour mSelectvillager;
        public void SelectVillager(VillagerBehaviour villager)
        {
                mSelectvillager = null;
                if (villager.elemCfg.task_npc_id != 0)
                {
                        mSelectvillager = villager;
                        SendVillageTrigger(villager.Index);
                }
                else
                {
                        UI_VillagerOption uis = UIManager.GetInst().ShowUI<UI_VillagerOption>("UI_VillagerOption");
                        uis.Setup(villager, null);
                }
        }

        public void ShowVillagerOption()
        {
                if (mSelectvillager != null)
                {
                        UI_VillagerOption uis = UIManager.GetInst().ShowUI<UI_VillagerOption>("UI_VillagerOption");
                        uis.Setup(mSelectvillager, null);
                }
        }

        void ChangeVillageState()
        {
                switch (m_VillageState)
                {
                        case VILLAGE_STATE.NONE:
                                {
                                }
                                break;
                        case VILLAGE_STATE.PREVIEW:
                                {
                                }
                                break;
                        case VILLAGE_STATE.INVADE:
                                {
                                        EnterInvadeMode();
                                }
                                break;
                }
        }

        public void ExitVillage()
        {
                VillageManager.GetInst().SendLeave();
                HomeManager.GetInst().DestroyHome();
                //SceneManager.GetInst().ExitLastScene();
                RaidTeamManager.GetInst().ClearTeam();
                RaidTeamManager.GetInst().ClearHeroDict();
                HomeManager.HomeSize = 0;

                ResourceManager.GetInst().UnloadAllAB();

                HomeManager.GetInst().LoadHome();
                //UIManager.GetInst().ShowUI<UI_WorldMap>("UI_WorldMap");
        }
        public void SwitchCaptain(int index, float time = 0f)
        {
                HeroUnit unit = RaidTeamManager.GetInst().GetHeroUnit(index);
                if (unit.hero.PetID <= 0)
                {
                        return;
                }                
                RaidTeamManager.GetInst().SwitchUnit(unit, time);
                MainHero = RaidTeamManager.GetInst().GetFirstHero();
                if (m_UIMain != null)
                {
                        m_UIMain.SetupHero(RaidTeamManager.GetInst().GetHeroList());
                }
        }

        public void Update()
        {
                switch (m_VillageState)
                {
                        case VILLAGE_STATE.PREVIEW:
                                {
                                        InputManager.GetInst().UpdateGlobalInput();
                                }
                                break;
                        case VILLAGE_STATE.INVADE:
                                {
                                        UpdateCameraFollow();
                                }
                                break;
                }
                UpdateTime();
        }

        void UpdateTime()
        {
                if (m_UIMain != null)
                {
                        m_UIMain.UpdateTime((int)(Time.realtimeSinceStartup - m_fTime), (int)m_nRemainTime);
                }
        }

        void UpdateCameraFollow()
        {
                if (Camera.main == null)
                        return;

                if (MainHero != null)
                {
                        Vector3 newpos = MainHero.transform.position + Vector3.up;
                        newpos -= Camera.main.transform.forward * m_fCameraDist;
                        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, newpos, Time.deltaTime * 2f);
                }
        }

        public void EnterCombat()
        {
                if (m_UIMain != null)
                {
                        m_UIMain.GetComponent<Canvas>().enabled = false;
                }

                GameUtility.EnableCameraRaycaster(false);
                GameStateManager.GetInst().GameState = GAMESTATE.RAID_COMBAT;
                RaidTeamManager.GetInst().TeamStop();
        }
        public void ExitCombat()
        {
                if (m_UIMain != null)
                {
                        m_UIMain.GetComponent<Canvas>().enabled = true;
                }
                GameStateManager.GetInst().GameState = GAMESTATE.RAID_PLAYING;
                GameUtility.EnableCameraRaycaster(true);
        }
        public bool GetBattlePoints(int nodeId, ref RaidBattlePointBehav myBp, ref RaidBattlePointBehav enemyBp)
        {
                Vector3 enemyPos = Vector3.zero;
                Vector3 myPos = Vector3.zero;

                if (m_VillageCfg != null)
                {
                        enemyPos = GetPos(m_VillageCfg.fight_position);
                        myPos = GetPos(m_VillageCfg.fight_position);

                        myPos.z -= GlobalParams.GetFloat("battle_position_mid_distance");
                        enemyPos.z += GlobalParams.GetFloat("battle_position_mid_distance");
                }

                Vector3 dir = (enemyPos - myPos);
                Quaternion rotation = Quaternion.LookRotation(dir);
                GameObject myBpObj = new GameObject();
                myBp = myBpObj.AddComponent<RaidBattlePointBehav>();
                myBp.transform.position = myPos;
                myBp.transform.rotation = Quaternion.LookRotation(dir);
                myBp.Init();

                GameObject enemyBpObj = new GameObject();
                enemyBp = enemyBpObj.AddComponent<RaidBattlePointBehav>();
                enemyBp.transform.position = enemyPos;
                enemyBp.transform.rotation = Quaternion.LookRotation(dir * (-1));
                enemyBp.Init();
                return true;
        }
        Vector3 GetHeroPosition(int idx, LINK_DIRECTION direct)
        {
                if (m_VillageCfg != null)
                {
                        Vector3 pos = GetPos(m_VillageCfg.fight_position);
                        switch (direct)
                        {
                                case LINK_DIRECTION.NORTH:
                                        {
                                                pos.x += (1 - idx % 3) * GlobalParams.GetFloat("battle_position_x_distance");
                                                pos.z += GlobalParams.GetFloat("battle_position_mid_distance") + (idx / 3) * GlobalParams.GetFloat("battle_position_y_distance");
                                        }
                                        break;
                                default:
                                case LINK_DIRECTION.SOUTH:
                                        {
                                                pos.x += (idx % 3 - 1) * GlobalParams.GetFloat("battle_position_x_distance");
                                                pos.z -= GlobalParams.GetFloat("battle_position_mid_distance") + (idx / 3) * GlobalParams.GetFloat("battle_position_y_distance");
                                        }
                                        break;
                                case LINK_DIRECTION.WEST:
                                        {
                                                pos.x -= GlobalParams.GetFloat("battle_position_mid_distance") + (idx / 3) * GlobalParams.GetFloat("battle_position_y_distance");
                                                pos.z += (1 - idx % 3) * GlobalParams.GetFloat("battle_position_x_distance");
                                        }
                                        break;
                                case LINK_DIRECTION.EAST:
                                        {
                                                pos.x += GlobalParams.GetFloat("battle_position_mid_distance") + (idx / 3) * GlobalParams.GetFloat("battle_position_y_distance");
                                                pos.z += (idx % 3 - 1) * GlobalParams.GetFloat("battle_position_x_distance");
                                        }
                                        break;

                        }
                        return pos;
                }
                return Vector3.zero;
        }
        public Dictionary<GameObject, int> GetBattleEnemyObjList()
        {
                Dictionary<GameObject, int> list = new Dictionary<GameObject, int>();
                return list;
        }

        public void SwitchCameraToRaid()
        {
                Vector3 rot;
                Camera.main.fieldOfView = GlobalParams.GetFloat("field_of_view");
                rot = GlobalParams.GetVector3("camera_rot");
                m_fCameraDist = GlobalParams.GetFloat("camera_focus_dist");
                Camera.main.transform.rotation = Quaternion.Euler(rot);
        }
}
