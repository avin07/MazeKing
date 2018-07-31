using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UI_RaidMain : UIBehaviour
{
        public GameObject m_SkillItem0;
        public Sprite m_BigSkillSprite;
        
        public List<GameObject> m_HeroList = new List<GameObject>();

        public Text m_TaskTitle, m_TaskDesc;
        public GameObject m_MainTaskFinish, m_MainTaskUnfinish, m_BranchTaskGroup;

        public GameObject m_BuffIcon;
        public GameObject m_SkillGroup;
        public GameObject m_TaskGroup;
        public GameObject m_Task0;
        public GameObject m_SubTask0;
        public GameObject m_PlayerGroup;
        public Button m_BtnBag, m_BtnCamp;

        public Text m_CampCount;
        Dictionary<long, GameObject> m_HeroObjDict = new Dictionary<long, GameObject>();
        Dictionary<string, AdventureSkillConfig> m_SkillDict = new Dictionary<string, AdventureSkillConfig>();
        //GameObject m_DirectBtnGroup;
        Dictionary<int, GameObject> m_DirectionBtnDict = new Dictionary<int, GameObject>();
        GameObject m_TorchTipGroup;
        List<string> m_TorchTipText = new List<string>();

        void Awake()
        {
                m_SubTask0.SetActive(false);
                m_BranchTaskGroup.SetActive(false);
                m_nCurrentBright = 0;
                m_TorchTipGroup = GetGameObject("torchtips");
                for (int i = 0; i < 7; i++)
                {
                        m_TorchTipText.Add(GetText(m_TorchTipGroup, "tips" + i).text);
                }
                m_TorchTipGroup.SetActive(false);
        }

        public void OnClickMyBag(GameObject go)
        {
                if (UIManager.GetInst().GetUIBehaviour<UI_RaidBag>() != null)
                {
                        UIManager.GetInst().CloseUI("UI_RaidBag");
                }
                else
                {
                        UIManager.GetInst().ShowUI<UI_RaidBag>("UI_RaidBag");                        
                }
        }
        public void OnClickComp()
        {
                if (!UIManager.GetInst().IsUIVisible("UI_RaidTaskFinish"))
                {
                        UIManager.GetInst().ShowUI<UI_RaidTaskFinish>("UI_RaidTaskFinish");
                }
        }
        public void OnClickLeave()
        {
                if (GetGameObject("taskfinish").activeSelf)
                {
                        RaidManager.GetInst().SendLeave();
                }
                else
                {
                        UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox").SetConfirmAndCancel("", "确定退出迷宫吗？", ConfirmLeave, null, null);
                }
        }
        void ConfirmLeave(object data)
        {
                RaidManager.GetInst().SendLeave();
        }

        public void OnClickCameraDebug()
        {                
        }

        #region TASK

        Dictionary<int, GameObject> m_BranchDict = new Dictionary<int, GameObject>();
        public void UpdateBranchTask(int taskId)
        {
                Debug.Log("UpdateBranchTask " + taskId);
                TaskConfig taskCfg = TaskManager.GetInst().GetTaskCfg(taskId);
                //RaidTaskConfig taskCfg = RaidConfigManager.GetInst().GetTaskConfig(taskInfo.taskId);
                if (taskCfg != null)
                {
                        m_BranchTaskGroup.SetActive(true);
                        GameObject taskGroup = null;
                        if (!m_BranchDict.ContainsKey(taskCfg.id))
                        {
                                GameObject cloneElem = CloneElement(m_SubTask0);
                                cloneElem.transform.SetParent(m_BranchTaskGroup.transform);
                                cloneElem.transform.localScale = Vector3.one;
                                m_BranchDict.Add(taskCfg.id, cloneElem);
                        }
                        taskGroup = m_BranchDict[taskCfg.id];
                        List<string> list = TaskManager.GetInst().GetTaskScheduleDes(taskCfg.id);
                        if (list != null)
                        {
                                string text = "";
                                for (int i = 0; i < list.Count; i++)
                                {
                                        text += list[i];
                                }
                                //UpdateTaskCount(taskCfg, taskInfo, GetText(taskGroup, "tasktitle"), GetText(taskGroup, "taskdesc"));                        
                                taskGroup.GetComponent<Text>().text = text;
                        }
                }
        }

        bool m_bTaskFinish = false;

        bool UpdateTaskCount(RaidTaskConfig taskCfg, string countstr, Text titleText, Text descText)
        {
                bool bFinish = true;
                Dictionary<int, int> dict = GameUtility.ParseCommonStringToDict(countstr, '|', '&');

                string[] paramArray = new string[taskCfg.target_id.Count];
                for (int i = 0; i < taskCfg.target_id.Count; i++)
                {
                        int id = taskCfg.target_id[i];
                        int max = -1;

                        if (i < taskCfg.req_quantity.Count)
                        {
                                max = taskCfg.req_quantity[i];
                        }

                        if (max > 0)
                        {
                                if (dict.ContainsKey(id))
                                {
                                        if (dict[id] < max)
                                        {
                                                bFinish = false;
                                                paramArray[i] = dict[id] + "/" + max;
                                        }
                                        else
                                        {
                                                paramArray[i] = max + "/" + max;
                                        }
                                }
                                else
                                {
                                        paramArray[i] = "0" + "/" + max;

                                        bFinish = false;
                                }
                        }
                        else
                        {
                                bFinish = false;
                                if (dict.ContainsKey(id))
                                {
                                        paramArray[i] = dict[id].ToString();
                                }
                                else
                                {
                                        paramArray[i] = "0";
                                }
                        }
                }

                //titleText.text = LanguageManager.GetText(taskCfg.name);
                descText.text = string.Format(LanguageManager.GetText(taskCfg.desc), paramArray);
                return bFinish;
        }

        public void UpdateMainTask(string countstr)
        {
                if (m_TaskCfg != null)
                {
                        m_TaskGroup.SetActive(true);
                        if (UpdateTaskCount(m_TaskCfg, countstr, m_TaskTitle, m_TaskDesc))
                        {
                                GetGameObject("taskfinish").SetActive(true);
                                m_MainTaskFinish.gameObject.SetActive(true);
                                 m_MainTaskUnfinish.gameObject.SetActive(false);
                        }
                        else
                        {
                                GetImage("taskfinish").gameObject.SetActive(false);
                                m_MainTaskFinish.gameObject.SetActive(false);
                                m_MainTaskUnfinish.gameObject.SetActive(true);
                        }
                }
                else
                {
                        m_TaskGroup.SetActive(false);
                        
                        m_TaskDesc.text = "";
                }
        }

        RaidTaskConfig m_TaskCfg;
        public void SetRaidTask(int raidId, string countstr)
        {
                m_TaskCfg = null;
                m_bTaskFinish = false;

                m_MainTaskFinish.gameObject.SetActive(false);
                m_MainTaskUnfinish.gameObject.SetActive(true);
                
                m_TaskDesc.text = "";

                m_TaskCfg = RaidConfigManager.GetInst().GetRaidMainTask();
                UpdateMainTask(countstr);
        }



        #endregion

        #region SKILL

        public void SetupSkills(Dictionary<int, int> list)
        {
                m_SkillDict.Clear();
                int idx = 0;
                m_SkillItem0.SetActive(false);
                foreach (int skillId in list.Keys)
                {
                        AdventureSkillConfig cfg = SkillManager.GetInst().GetAdvSkillCfg(skillId);
                        if (cfg == null)
                                continue;

                        GameObject obj = idx == 0 ? m_SkillItem0 : CloneElement(m_SkillItem0, "BtnSkill" + idx);
                        obj.SetActive(true);
                        if (!m_SkillDict.ContainsKey(obj.name))
                        {
                                m_SkillDict.Add(obj.name, cfg);
                        }
                        Image im = GetImage(obj, "icon");
                        if (im != null)
                        {
                                ResourceManager.GetInst().LoadIconSpriteSyn(SkillManager.GetInst().GetSkillIconUrl(skillId), im.transform);
                        }
                        GetText(obj, "name").text = SkillManager.GetInst().GetSkillName(skillId);
                        idx++;
                }
        }

        #endregion


        public void EnableCampMode(bool bEnable)
        {
                //m_SkillGroup.SetActive(!bEnable);
                m_TaskGroup.SetActive(m_TaskCfg != null ? !bEnable : false);
                m_BtnBag.gameObject.SetActive(!bEnable);
                m_BtnCamp.gameObject.SetActive(!bEnable);
        }

        public void EnableCombatMode(bool bEnable)
        {
                GetGameObject("rightgroup").SetActive(!bEnable);
                m_PlayerGroup.SetActive(!bEnable);
        }

        #region HERO
        public void SetupHero(List<HeroUnit> heroDict)
        {
                foreach (GameObject obj in m_HeroList)
                {
                        obj.SetActive(false);
                }
                m_HeroObjDict.Clear();
                int idx = 0;
                foreach (HeroUnit unit in heroDict)
                {
                        if (idx < m_HeroList.Count)
                        {
                                GameObject heroObj = m_HeroList[idx];
                                heroObj.SetActive(true);
                                CharacterConfig cfg = CharacterManager.GetInst().GetCharacterCfg(unit.CharacterID);
                                string icon = ModelResourceManager.GetInst().GetIconRes(cfg.model_id);

                                Image playerIcon = GetImage(heroObj, "playericon");
                                playerIcon.enabled = true;
                                ResourceManager.GetInst().LoadIconSpriteSyn(icon, playerIcon.transform);
                                UIUtility.SetImageGray(!unit.IsAlive, playerIcon.transform);
                                m_HeroObjDict.Add(unit.ID, heroObj);

//                                SetHeroHp(unit.ID, unit.hero.Hp, unit.hero.MaxHp);
   //                             SetPressure(unit.ID, unit.hero.Pressure);
      //                          SetHeroBuff(unit.ID, unit.hero.m_BuffList);
          //                      SetHeroBehaviour(unit.ID, unit.hero.GoodSpecCount, unit.hero.BadSpecCount);

                                GameObject specGroupObj = GetGameObject(heroObj, "specificty_group");
                                if (idx == 0 && specGroupObj.GetComponent<EventTriggerListener>() == null)
                                {
                                        EventTriggerListener.Get(specGroupObj).onClick = OnClickSpecTips;
                                        EventTriggerListener.Get(specGroupObj).SetTag(GetImage(specGroupObj, "spectips").gameObject);
                                }
                                idx++;
//                                 if (unit.hero.GoodSpecCount + unit.hero.BadSpecCount > 0)
//                                 {
//                                         specGroupObj.SetActive(true);
//                                 }
//                                 else
                                {
                                        specGroupObj.SetActive(false);
                                }
                                
                        }
                }
        }
        public void SetPressure(long id, int pressure)
        {
                if (m_HeroObjDict.ContainsKey(id))
                {
                        int nowCount = pressure / 10;
                        for (int i = 0; i < 20; i++)
                        {
                                GetImage(m_HeroObjDict[id], "pressure" + i).enabled = i < nowCount;
                        }
                        GetText(m_HeroObjDict[id], "pressure_value").text = pressure + "/200";
                }
        }
        public void ShowPressureEffect(long id)
        {
                if (m_HeroObjDict.ContainsKey(id))
                {
                        Image im = GetImage(m_HeroObjDict[id], "pressurebg");
                        UIUtility.SetUIEffect(this.name, im.gameObject, true, "effect_UI_pressure_result");
                }
        }

        public void SetHeroHp(long id, int hp, int maxhp, bool bNeedAnim = false)
        {
                if (m_HeroObjDict.ContainsKey(id))
                {
                        float amount = hp / (float)maxhp;
                        GetImage(m_HeroObjDict[id], "hp2").fillAmount = amount;
                        Image im = GetImage(m_HeroObjDict[id], "hp");
                        if (bNeedAnim)
                        {
                                DOTween.To(() =>
                              {
                                      return im.fillAmount;
                              }, v =>
                              {
                                      im.fillAmount = v;
                              }, amount, 1f);
                        }
                        else
                        {
                                im.fillAmount = amount;
                        }
                        GetText(m_HeroObjDict[id], "hp_value").text = hp + "/" + maxhp;
                }
        }
//         IEnumerator ProcessHpChange(Image)
//         {
//         }

        //public void SetHeroBuff(long id, List<RaidHero.HeroBuff> buffList)
        //{
        //        if (m_HeroObjDict.ContainsKey(id))
        //        {
        //                Transform buffgroup = m_HeroObjDict[id].transform.Find("buffgroup");
        //                foreach (Transform trans in m_HeroObjDict[id].GetComponentsInChildren<Transform>(true))
        //                {
        //                        if (trans.parent == buffgroup)
        //                        {
        //                                GameObject.Destroy(trans.gameObject);
        //                        }
        //                }

        //                foreach (RaidHero.HeroBuff buff in buffList)
        //                {
        //                        SkillBuffConfig buffCfg = SkillManager.GetInst().GetBuff(buff.cfgId);
        //                        if (buffCfg != null)
        //                        {
        //                                GameObject buffIcon = CloneElement(m_BuffIcon, "buff_" + buff.cfgId);
        //                                ResourceManager.GetInst().LoadIconSpriteSyn(buffCfg.icon, buffIcon.transform);
        //                                buffIcon.transform.SetParent(buffgroup);
        //                                buffIcon.transform.localScale = Vector3.one;
        //                                EventTriggerListener.Get(buffIcon).onClick = OnClickBuff;
        //                                EventTriggerListener.Get(buffIcon).SetTag(buffCfg);
        //                        }
        //                }
        //        }
        //}
        //public void OnClickBuff(GameObject go, PointerEventData data)
        //{
        //        UI_SkillTips uis = UIManager.GetInst().GetUIBehaviour<UI_SkillTips>();
        //        if (uis == null)
        //        {
        //                uis = UIManager.GetInst().ShowUI<UI_SkillTips>(0);
        //        }
        //        SkillBuffConfig cfg = EventTriggerListener.Get(go).GetTag() as SkillBuffConfig;
        //        Vector3 screenPos = CalcScreenPosition(go.GetComponent<RectTransform>());
        //        screenPos.x += 30f;
        //        uis.SetBuffTip(cfg, screenPos);
        //}

        public void SetBehaviourShow(long id, int goodCount, int badCount, bool bGood, GameObject unit)
        {
                StartCoroutine(ProcessBehaviourFlyToHero(id, goodCount, badCount, bGood, unit));
        }
        IEnumerator ProcessBehaviourFlyToHero(long id, int goodCount, int badCount, bool bGood, GameObject unit)
        {
                if (m_HeroObjDict.ContainsKey(id))
                {
                        yield return new WaitForSeconds(1f);
                        GameObject obj = GetGameObject(m_HeroObjDict[id], "specificty_group");
                        if (obj != null)
                        {
                                GameObject flyObj = null;
                                if (bGood)
                                {
                                        flyObj = CloneElement(GetGameObject(obj, "specificty_good"), "flyobj");
                                        flyObj.GetComponent<Image>().enabled = true;
                                }
                                else
                                {
                                        flyObj = CloneElement(GetGameObject(obj, "specificty_bad"), "flyobj");
                                        flyObj.GetComponent<Image>().enabled = true;
                                }
                                flyObj.transform.SetParent(obj.transform.parent);
                                flyObj.SetActive(true);
                                if (flyObj != null)
                                {
                                        Canvas canvas = GetComponent<Canvas>();
                                        Vector2 startpos = UIUtility.ScenePositionToUIPosition(flyObj.GetComponent<RectTransform>(), unit.transform.position + Vector3.up * 1.708f, Camera.main);
                                        Vector2 endpos = flyObj.GetComponent<RectTransform>().anchoredPosition;

                                        Debuger.Log(startpos + " " + endpos);
                                        RectTransform trans = flyObj.GetComponent<RectTransform>();
                                        trans.anchoredPosition = new Vector2(startpos.x, startpos.y);
                                        DOTween.To(() =>
                                        {
                                                return trans.anchoredPosition;
                                        }, v =>
                                        {
                                                trans.anchoredPosition = v;
                                        }, endpos, 1f);
                                        GameObject.Destroy(flyObj, 1f);
                                }
                        }
                        yield return new WaitForSeconds(1f);
                        SetHeroBehaviour(id, goodCount, badCount);
                        obj.SetActive(true);
                }
        }


        public void SetHeroBehaviour(long id, int goodCount, int badCount)
        {
                //Debuger.Log("SetHeroBehaviour  " + id + " " + goodCount + " " + badCount); 
                if (m_HeroObjDict.ContainsKey(id))
                {
                        GameObject obj = GetGameObject(m_HeroObjDict[id], "specificty_group");
                        if (obj != null)
                        {
                                GetImage(obj, "specificty_none").enabled = goodCount <= 0 && badCount <= 0;
                                GetImage(obj, "specificty_bad").enabled = goodCount <= 0 && badCount > 0;
                                GetImage(obj, "specificty_good").enabled = goodCount > 0 && badCount <= 0;
                                GetImage(obj, "specificty_good_bad").enabled = goodCount > 0 && badCount > 0;

                                if (GetGameObject(obj, "spectips") != null)
                                {
                                        GetText(obj, "goodcount").text = goodCount.ToString();
                                        GetText(obj, "badcount").text = badCount.ToString();
                                }

                        }
                }
        }
        void OnClickSpecTips(GameObject go, PointerEventData data)
        {
                GameObject tipGroup = (GameObject)EventTriggerListener.Get(go).GetTag();
                tipGroup.SetActive(!tipGroup.activeSelf);
        }

        public void OnClickPlayerIcon(GameObject go)
        {
                if (UIManager.GetInst().IsUIVisible("UI_RaidSkillSelect") == false)
                {
                        int index = int.Parse(go.name.Replace("player", ""));
                        RaidManager.GetInst().SwitchCaptain(index);
                }
        }
        public GameObject GetMainHeroObj()
        {
                if (m_HeroList.Count > 0)
                {
                        return m_HeroList[0];
                }
                return null;
        }


        #endregion
        int m_nCurrentBright = 0;
        public void SetBright(int val)
        {
                val = Mathf.Clamp(val, 0, 200);

                if (val > m_nCurrentBright)
                {
                        PlayBrightEffect();
                }

                m_nCurrentBright = val;

                bool isDayTime = val > 100;
                GetGameObject("BrightIcon_Day").SetActive(isDayTime);
                GetGameObject("BrightIcon_Night").SetActive(!isDayTime);

                if (!isDayTime)
                {
                        Image im = GetImage("brightness");

                        GetGameObject("torch_on").SetActive(val > 0);
                        GetGameObject("torch_off").SetActive(val <= 0);
                        DOTween.To(() =>
                        {
                                return im.fillAmount;
                        }, v =>
                        {
                                im.fillAmount = v;
                        }, Mathf.Max(0, val ) / 100f, 0.5f);
                }
        }
        public void PlayBrightEffect()
        {
                UIUtility.SetUIEffect(this.name, GetGameObject("torch"), true, "effect_UI_add_brightness");
        }
        public void PlayDiceEffect()
        {
                UIUtility.SetUIEffect(this.name, GetGameObject("uieffect"), true, "effect_raid_destiny_room_refresh");
        }

        void Update()
        {
                if (InputManager.GetInst().GetInputUp(false))
                {
                        UIManager.GetInst().CloseUI("UI_SkillTips");
                        m_TorchTipGroup.SetActive(false);

                        UIManager.GetInst().CloseUI("UI_NewFurniture");
                }
        }
        int[] m_BrightNums = new int[7]
        {8,8,4,10,15,4,15};
        public void OnClickTorch()
        {
                m_TorchTipGroup.SetActive(!m_TorchTipGroup.activeSelf);
                int lvl = RaidTeamManager.GetInst().GetTeamBrightLevel();
                for (int i = 0; i < 7; i++)
                {
                        int ret = lvl * m_BrightNums[i];
                        GetText(m_TorchTipGroup, "tips"+i).text = m_TorchTipText[i].Replace("%s", ret.ToString()) + "%";
                }
        }
}
