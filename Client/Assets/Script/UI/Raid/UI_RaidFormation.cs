using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

public class UI_RaidFormation : UIBehaviour
{
        public GameObject content;
        public GameObject m_Pet0;
        public GameObject m_Item0;
        public GameObject m_AdvItem0;

        public Text m_CaptainSkill;
        public Text m_TeamCount;

        int m_nMaxTeamCount = 0;
        
        GameObject m_FormationScene;

        int m_CfgId = 0;
        int m_nFloor = 1;
        UIBehaviour m_PrevUI;

        List<Transform> m_FormationPosList = new List<Transform>();

        enum RF_MODE
        {
                RAID,
                DEFEND,
        };
        RF_MODE m_Mode = RF_MODE.RAID;    //1副本 2村庄 3DIY 4防守设置

        public void OnClickReturn()
        {
                UIManager.GetInst().CloseUI(this.name);
                if (m_PrevUI != null)
                {
                        m_PrevUI.gameObject.SetActive(true);
                }
                else
                {
                        UIManager.GetInst().ShowUI<UI_WorldMap>("UI_WorldMap");
                }
        }
        List<long> m_ExistFormation = new List<long>();
        Dictionary<GameObject, Pet> m_PetElemDict = new Dictionary<GameObject, Pet>();
        Dictionary<int, FormationUnitBehav> m_UnitDict = new Dictionary<int, FormationUnitBehav>();

        void CommonSetup()
        {
                if (m_PrevUI != null)
                {
                        m_PrevUI.gameObject.SetActive(false);
                }

                m_TeamCount.text = "0/" + m_nMaxTeamCount;
                m_CaptainSkill.text = "";
                GetGameObject("btnenter").SetActive(true);
                GetGameObject("btnset").SetActive(false);

                RefreshMyPetList();
                InitBag();
        }

        public void Setup(int raidId, int floor, UIBehaviour prev)//副本
        {
                m_Mode = RF_MODE.RAID;
                m_PrevUI = prev;

                m_CfgId = raidId;
                m_nFloor = floor;
                m_nMaxTeamCount = GlobalParams.GetInt("init_max_team_member") + PlayerController.GetInst().GetPropertyInt("team_member_limit");
                Debug.Log("m_nMaxTeamCount = " + m_nMaxTeamCount);

                RaidInfoHold raidInfo = RaidConfigManager.GetInst().GetRaidInfoCfg(m_CfgId);
                if (raidInfo != null)
                {
                        m_ExistFormation = UserManager.GetInst().GetFormation(raidInfo.formation_id);
                }

                CommonSetup();
        }


        void DefendOver()
        {
                if (m_Mode==RF_MODE.DEFEND)
                {
                        HomeManager.GetInst().SetHomeActive(true);
                        UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", false);
                }
        }

        public override void OnShow(float time)
        {
                base.OnShow(time);
                m_FormationScene = SceneManager.GetInst().LoadGameObject("Scene/Scene_formation_field");
#if UNITY_ANDROID
                AntialiasingAsPostEffect aape = m_FormationScene.GetComponentInChildren<AntialiasingAsPostEffect>();
                if (aape != null)
                {
                        GameObject.Destroy(aape);
                }
                SSAOEffect ssao = m_FormationScene.GetComponentInChildren<SSAOEffect>();
                if (ssao != null)
                {
                        GameObject.Destroy(ssao);
                }
#endif

                Transform t = GameUtility.GetTransform(m_FormationScene, "AmbientLight");
                if (t != null)
                {
                        RenderSettings.ambientLight = new Color(t.position.x / 255f, t.position.y / 255f, t.position.z / 255f);
                }
                m_FormationPosList.Clear();
                for (int i = 1; i <= 6; i++)
                {
                        m_FormationPosList.Add(GameUtility.GetTransform(m_FormationScene, "pos_" + i));
                }
        }
        public override void OnClose(float time)
        {
                base.OnClose(time);
                GameObject.Destroy(m_FormationScene);
                if (m_PrevUI != null)
                {
                        m_PrevUI.gameObject.SetActive(true);
                }
                m_FormationPosList.Clear();
//                 if (m_ExistFormation != null)
//                 {
//                         m_ExistFormation.Clear();
//                 }
                m_PetElemDict.Clear();
                m_UnitDict.Clear();
                m_ItemDict.Clear();
                m_AdvItemDict.Clear();
                switch (m_Mode)
                {
                        case RF_MODE.RAID:
                                break;
                        case RF_MODE.DEFEND:
                                DefendOver();
                                break;
                }
        }

        void Update()
        {
                UpdateUnitMove();
        }

        #region Pet
        void RefreshMyPetList()
        {
                CreatePet();
        }
        void CreatePet()
        {                                
                SetChildActive(content.transform,false);
                List<Pet> petlist = PetManager.GetInst().GetMyPetListSort();
                int exist_num = 0;
                for (int i = 0; i < petlist.Count; i++)
                {
                        GameObject pet_btn = GetGameObject(content, "pet" + i);
                        if (pet_btn == null)
                        {
                                pet_btn = CloneElement(m_Pet0, "pet" + i);
                        }
                        pet_btn.SetActive(true);
                        Pet pet = petlist[i];
                        m_PetElemDict.Add(pet_btn, pet);
                        UpdatePet(pet, pet_btn);

                        if (m_ExistFormation != null)
                        {
                                int idx = m_ExistFormation.IndexOf(pet.ID);
                                if (idx >= 0 && idx < 6)
                                {
                                        exist_num++;
                                        InitPetModel(idx, pet, pet_btn);
                                }
                        }
                        m_TeamCount.text = exist_num + CommonString.divideStr + m_nMaxTeamCount;
                }
                m_Pet0.SetActive(false);
                CheckIconState();
                content.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        }
        void UpdatePet(Pet pet, GameObject go)
        {
                SetStar(pet, GetGameObject(go, "star_group").transform);
                SetPressure(pet.FighterProp.Pressure, go);
                ResourceManager.GetInst().LoadIconSpriteSyn(ModelResourceManager.GetInst().GetIconRes(pet.GetPropertyInt("model_id")), GetImage(go, "head").transform);
                GetGameObject(go, "gou").SetActive(false);
                GetText(go, "level").text = "LV." + pet.GetPropertyString("level");
                ResourceManager.GetInst().LoadIconSpriteSyn(CharacterManager.GetInst().GetCareerIcon(pet.GetPropertyInt("career")), GetImage(go, "career_icon").transform);
        }

        void CheckIconState()
        {
                bool bGray = m_UnitDict.Count >= m_nMaxTeamCount;
                foreach (GameObject go in m_PetElemDict.Keys)
                {
                        if (!GetGameObject(go, "gou").activeSelf)
                        {
                                UIUtility.SetImageGray(bGray, GetImage(go, "head").transform);
                                //全灰和星级灰态有冲突
                                //UIUtility.SetGroupGray(bGray, go);
                        }
                }
        }

        void RemovePetFromFormation(FormationUnitBehav fub, int idx)
        {
                GetGameObject(fub.PetBtnObj, "gou").SetActive(false);
                GameObject.Destroy(fub.gameObject);
                if (m_UnitDict.ContainsKey(idx))
                {
                        m_UnitDict.Remove(idx);
                }
                m_TeamCount.text = m_UnitDict.Count + CommonString.divideStr + m_nMaxTeamCount;
                CheckIconState();

        }
        public void OnPetClick(GameObject go)
        {
                if (m_PetElemDict.ContainsKey(go))
                {
                        Pet pet = m_PetElemDict[go];

                        foreach (int idx in m_UnitDict.Keys)
                        {
                                FormationUnitBehav fub = m_UnitDict[idx];
                                if (fub.m_Pet.ID == pet.ID)
                                {
                                        RemovePetFromFormation(fub, idx);
                                        return;
                                }
                        }

                        if (m_UnitDict.Count >= m_nMaxTeamCount)
                        {
                                GameUtility.PopupMessage(LanguageManager.GetText(""));
                        }
                        else
                        {
                                for (int idx = 0; idx < m_FormationPosList.Count; idx++)
                                {
                                        if (!m_UnitDict.ContainsKey(idx))
                                        {
                                                InitPetModel(idx, pet, go);
                                                m_TeamCount.text = m_UnitDict.Count + CommonString.divideStr + m_nMaxTeamCount;
                                                CheckIconState();
                                                break;
                                        }
                                }
                        }
                }
        }
        void CheckCaptainSkill()
        {
                m_CaptainSkill.text = "";

                if (m_UnitDict.ContainsKey(0))
                {
                        Pet pet = m_UnitDict[0].m_Pet;
                        SkillLearnConfigHold cfg = pet.GetCaptainSkill();
                        if (cfg != null)
                        {
                                m_CaptainSkill.text = LanguageManager.GetText(cfg.desc);
                        }
                }
        }

        void InitPetModel(int idx, Pet pet, GameObject go)
        {
                GetGameObject(go, "gou").SetActive(true);

                GameObject mod = CharacterManager.GetInst().GenerateModel(pet.CharacterID);

                if (mod != null)
                {
                        FormationUnitBehav fub = mod.AddComponent<FormationUnitBehav>();
                        BoxCollider box = mod.AddComponent<BoxCollider>();
                        box.center = Vector3.up;
                        box.size = new Vector3(1f, 2f, 1f);
                        GameUtility.ObjPlayAnim(mod, CommonString.idle_001Str, true);
                        GameUtility.SetLayer(mod, "Character");
                        Transform posT = m_FormationPosList[idx];
                        if (posT != null)
                        {
                                mod.transform.SetParent(posT);
                                mod.transform.localPosition = Vector3.zero;
                                mod.transform.localRotation = Quaternion.identity;
                        }
                        fub.m_Pet = pet;
                        fub.PetBtnObj = go;
                        m_UnitDict.Add(idx, fub);
                        CheckCaptainSkill();
                }
        }
        #endregion
        #region ITEM

        int m_nMaxAdvBagSize = 0;
        Dictionary<GameObject, DropObject> m_ItemDict = new Dictionary<GameObject, DropObject>();
        Dictionary<DropObject, GameObject> m_AdvItemDict = new Dictionary<DropObject, GameObject>();
        public void InitBag()
        {
                m_nMaxAdvBagSize = PlayerController.GetInst().GetPropertyInt("maze_capacity");
                m_AdvItem0.SetActive(false);
                m_Item0.SetActive(false);
                for (int i = 0; i < m_nMaxAdvBagSize; i++)
                {
                        GameObject itemObj = CloneElement(m_AdvItem0, "adv_item_" + i);
                        ClearAdvItem(itemObj);
                }

                List<int> typeList = GameUtility.ToList<int>(GlobalParams.GetString("raid_carry_item_type"), ',', (x) =>
                                 {
                                         return int.Parse(x);
                                 });
                List<DropObject> itemlist = ItemManager.GetInst().GetMyBagItemByTypeList(typeList); //迷宫道具
                int idx = 0;
                foreach (DropObject di in itemlist)
                {
                        DropObject newdi = new DropObject(di);

                        GameObject itemObj = CloneElement(m_Item0, "item" + idx);
                        string iconname = di.GetIconName();
                        if (iconname != "")
                        {
                                ResourceManager.GetInst().LoadIconSpriteSyn(iconname, GetImage(itemObj, "icon").transform);
                                GetImage(itemObj, "gou").enabled = false;
                                GetText(itemObj, "count").text = di.nOverlap.ToString();
                        }
                        if (!m_ItemDict.ContainsKey(itemObj))
                        {
                                m_ItemDict.Add(itemObj, di);
                        }
                        idx++;
                }
                m_Item0.transform.parent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }

        void ClearAdvItem(GameObject go)
        {
                GetImage(go, "itemicon").enabled = false;
                GetImage(go, "itemiconcover").enabled = false;
                GetText(go, "itemcount").text = "";
        }
        public void OnClickItem(GameObject go)
        {
                if (m_ItemDict.ContainsKey(go))
                {
                        DropObject di = m_ItemDict[go];
                        if (m_AdvItemDict.ContainsKey(di))
                        {
                                ClearAdvItem(m_AdvItemDict[di]);
                                m_AdvItemDict.Remove(di);
                                GetImage(go, "gou").enabled = false;
                        }
                        else
                        {

                                if (AddItemToAdvBag(di))
                                {
                                        GetImage(go, "gou").enabled = true;
                                }
                        }
                }
        }

        bool AddItemToAdvBag(DropObject di)
        {
                if (m_AdvItemDict.Count >= m_nMaxAdvBagSize)
                {
                        return false;
                }

                for (int i = 0; i < m_nMaxAdvBagSize; i++)
                {
                        GameObject itemObj = GetGameObject("adv_item_" + i);
                        if (itemObj != null && GetImage(itemObj, "itemiconcover").enabled == false)
                        {
                                GetImage(itemObj, "itemicon").enabled = true;
                                ResourceManager.GetInst().LoadIconSpriteSyn(di.GetIconName(), GetImage(itemObj, "itemicon").transform);
                                GetImage(itemObj, "itemiconcover").enabled = true;
                                GetText(itemObj, "itemcount").text = di.nOverlap.ToString();
                                if (!m_AdvItemDict.ContainsKey(di))
                                {
                                        m_AdvItemDict.Add(di, itemObj);
                                }
                                break;
                        }
                }

                return true;
        }
        public void OnClickAdvItem(GameObject go)
        {
                foreach (var param in m_AdvItemDict)
                {
                        if (param.Value == go)
                        {
                                ClearAdvItem(param.Value);
                                m_AdvItemDict.Remove(param.Key);

                                foreach (GameObject tmpObj in m_ItemDict.Keys)
                                {
                                        if (m_ItemDict[tmpObj] == param.Key)
                                        {
                                                GetImage(tmpObj, "gou").enabled = false;
                                        }
                                }
                                break;
                        }
                }
        }

        string GetItemList()
        {
                string itemlist = "";
                Dictionary<int, int> dict = new Dictionary<int, int>();

                foreach (DropObject dt in m_AdvItemDict.Keys)
                {
                        if (!dict.ContainsKey(dt.idCfg))
                        {
                                dict.Add(dt.idCfg, dt.nOverlap);
                        }
                        else
                        {
                                dict[dt.idCfg] += dt.nOverlap;
                        }
                }
                foreach (var param in dict)
                {
                        itemlist += param.Key + CommonString.ampersandStr + param.Value + CommonString.pipeStr;
                }
                if (String.IsNullOrEmpty(itemlist))
                {
                        return CommonString.zeroStr;
                }
                return itemlist;
        }
        
        #endregion

        #region PET_MODEL_DRAG

        int m_nMovingUnitIdx = -1;
        public void StartDragUnit(FormationUnitBehav unitBehav)
        {
                if (m_nMovingUnitIdx < 0)
                {
                        foreach (int idx in m_UnitDict.Keys)
                        {
                                if (m_UnitDict[idx] == unitBehav)
                                {
                                        m_fTime = Time.realtimeSinceStartup;
                                        m_nMovingUnitIdx = idx;
                                }
                        }
                }
        }

        void DropUnit()
        {
                if (m_nMovingUnitIdx >= 0)
                {
                        for (int i = 0; i < 6; i++)
                        {

                                if (Vector3.Distance(m_FormationPosList[i].position, m_UnitDict[m_nMovingUnitIdx].transform.position) < 1f)
                                {
                                        if (m_UnitDict.ContainsKey(i))
                                        {
                                                FormationUnitBehav behav = m_UnitDict[m_nMovingUnitIdx];
                                                m_UnitDict[m_nMovingUnitIdx] = m_UnitDict[i];
                                                m_UnitDict[m_nMovingUnitIdx].transform.position = m_FormationPosList[m_nMovingUnitIdx].position;

                                                m_UnitDict[i] = behav;
                                                m_UnitDict[i].transform.position = m_FormationPosList[i].position;
                                        }
                                        else
                                        {
                                                m_UnitDict.Add(i, m_UnitDict[m_nMovingUnitIdx]);                                                
                                                m_UnitDict[i].transform.position = m_FormationPosList[i].position;

                                                m_UnitDict.Remove(m_nMovingUnitIdx);
                                        }

                                        m_nMovingUnitIdx = -1;
                                        return;
                                }
                        }
                }
                if (m_nMovingUnitIdx >= 0)
                {
                        m_UnitDict[m_nMovingUnitIdx].transform.position = m_FormationPosList[m_nMovingUnitIdx].position;
                        m_nMovingUnitIdx = -1;
                }
        }
        float m_fTime = 0f;
        
        void UpdateUnitMove()
        {
                if (m_nMovingUnitIdx >= 0)
                {
                        if (Time.realtimeSinceStartup - m_fTime > 0.2f)
                        {
                                if (!m_UnitDict.ContainsKey(m_nMovingUnitIdx))
                                {
                                        return;
                                }
                                if (InputManager.GetInst().GetInputHold(true))
                                {
                                        RaycastHit hit = new RaycastHit();
                                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                                        if (Physics.Raycast(ray, out hit, Camera.main.farClipPlane, 1 << LayerMask.NameToLayer("Scene")))
                                        {
                                                Vector3 pos = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                                                m_UnitDict[m_nMovingUnitIdx].transform.position = pos;
                                        }
                                }
                                else
                                {
                                        DropUnit();
                                }
                        }
                        else
                        {
                                if (InputManager.GetInst().GetInputHold(true))
                                {

                                }
                                else
                                {
                                        if (m_UnitDict.ContainsKey(m_nMovingUnitIdx))
                                        {
                                                RemovePetFromFormation(m_UnitDict[m_nMovingUnitIdx], m_nMovingUnitIdx);
                                                m_nMovingUnitIdx = -1;
                                        }
                                }
                        }
                        CheckCaptainSkill();
                }
                else
                {
                        if (InputManager.GetInst().GetInputDown(false))
                        {
                                RaycastHit hit = new RaycastHit();
                                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                                if (Physics.Raycast(ray, out hit, Camera.main.farClipPlane, 1 << LayerMask.NameToLayer("Character")))
                                {
                                        StartDragUnit(hit.collider.GetComponent<FormationUnitBehav>());
                                }
                        }
                }
        }


        #endregion

        public void GotoRaid()
        {
                string petlist = "";
                if (m_UnitDict.Count == 0)
                {
                        GameUtility.PopupMessage("请选择上阵英雄！");
                        return;
                }

                for (int i = 0; i < 6; i++)
                {
                        if (m_UnitDict.ContainsKey(i))
                        {
                                petlist += m_UnitDict[i].m_Pet.ID + CommonString.pipeStr;
                        }
                        else
                        {
                                petlist += "0|";
                        }
                }
                petlist = petlist.Remove(petlist.LastIndexOf('|'));

                switch (m_Mode)
                {
                        case RF_MODE.RAID:
                                {
                                        int food = PlayerController.GetInst().GetPropertyInt("food");
                                        int need_food = RaidConfigManager.GetInst().GetRaidInfoCfg(m_CfgId).cost_vitality;
                                        if (food < need_food)
                                        {
                                                GameUtility.PopupMessage("食物不足！");
                                                return;
                                        }
                                        WorldMapManager.GetInst().GoIntoRaid(m_nFloor, petlist, GetItemList());
                                }
                                break;
                        case RF_MODE.DEFEND:
                                break;
                }

                UIManager.GetInst().CloseUI(this.name);
                if (m_PrevUI != null)
                {
                        UIManager.GetInst().CloseUI(m_PrevUI.name);
                }
                GameStateManager.GetInst().GameState = GAMESTATE.OTHER;
        }


        public void DefendSet()
        {
                //string petlist = "";
                //if (m_UnitDict.Count == 0)
                //{
                //        GameUtility.PopupMessage("请选择防守英雄！");
                //        return;
                //}          

                //for (int i = 0; i < 6; i++)
                //{
                //        if (m_UnitDict.ContainsKey(i))
                //        {
                //                petlist += m_UnitDict[i].m_Pet.ID + "|";
                //        }
                //        else
                //        {
                //                petlist += "0|";
                //        }
                //}

                //petlist = petlist.Remove(petlist.LastIndexOf('|'));
                //UIManager.GetInst().CloseUI(this.name);
                //if (m_PrevUI != null)
                //{
                //        (m_PrevUI as UI_DiyMazeEditor).SetMyDefendList(petlist);
                //        m_PrevUI.gameObject.SetActive(true);
                //}
                //DefendOver();
        }

}
