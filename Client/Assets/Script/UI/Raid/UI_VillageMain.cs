using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UI_VillageMain : UIBehaviour
{
        public List<GameObject> m_HeroList = new List<GameObject>();

        public Text m_TaskTitle, m_TaskDesc;
        public GameObject m_MainTaskFinish, m_MainTaskUnfinish, m_BranchTaskGroup;

        public GameObject m_BuffIcon;
        public GameObject m_TaskGroup;
        public GameObject m_Task0;
        public GameObject m_PlayerGroup;
        public Button m_BtnBag;

        public GameObject m_PreviewGroup;
        public GameObject m_InvadeGroup;

        public void ShowPreview()
        {
                m_PreviewGroup.SetActive(true);
                m_InvadeGroup.SetActive(false);
        }
        public void ShowInvade()
        {
                m_PreviewGroup.SetActive(false);
                m_InvadeGroup.SetActive(true);
        }
        Dictionary<long, GameObject> m_HeroObjDict = new Dictionary<long, GameObject>();

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
        public void OnClickLeave()
        {
                UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox").SetConfirmAndCancel("", "确定退出村庄吗？", ConfirmLeave, null, null);
        }
        void ConfirmLeave(object data)
        {
                VillageManager.GetInst().ExitVillage();
                UIManager.GetInst().CloseUI(this.name);
        }

        public void UpdateTime(int time, int maxtime)
        {
                if (time < maxtime)
                {
                        GetText("remaintime").text = (maxtime - time).ToString();
                }
                else
                {
                        GetText("remaintime").text = "";
                }

        }

        //         #region TASK
        //         bool m_bTaskFinish = false;
        //         RaidTaskConfig m_TaskCfg;
        //         Dictionary<int, GameObject> m_BranchDict = new Dictionary<int, GameObject>();
        // 
        //         bool UpdateTaskCount(RaidTaskConfig taskCfg, string countstr, Text titleText, Text descText)
        //         {
        //                 bool bFinish = true;
        //                 Dictionary<int, int> dict = GameUtility.ParseCommonStringToDict(countstr, '|', '&');
        // 
        //                 string[] paramArray = new string[taskCfg.target_id.Count];
        //                 for (int i = 0; i < taskCfg.target_id.Count; i++)
        //                 {
        //                         int id = taskCfg.target_id[i];
        //                         int max = -1;
        // 
        //                         if (i < taskCfg.req_quantity.Count)
        //                         {
        //                                 max = taskCfg.req_quantity[i];
        //                         }
        // 
        //                         if (max > 0)
        //                         {
        //                                 if (dict.ContainsKey(id))
        //                                 {
        //                                         if (dict[id] < max)
        //                                         {
        //                                                 bFinish = false;
        //                                                 paramArray[i] = dict[id] + "/" + max;
        //                                         }
        //                                         else
        //                                         {
        //                                                 paramArray[i] = max + "/" + max;
        //                                         }
        //                                 }
        //                                 else
        //                                 {
        //                                         paramArray[i] = "0" + "/" + max;
        // 
        //                                         bFinish = false;
        //                                 }
        //                         }
        //                         else
        //                         {
        //                                 bFinish = false;
        //                                 if (dict.ContainsKey(id))
        //                                 {
        //                                         paramArray[i] = dict[id].ToString();
        //                                 }
        //                                 else
        //                                 {
        //                                         paramArray[i] = "0";
        //                                 }
        //                         }
        //                 }
        // 
        //                 titleText.text = LanguageManager.GetText(taskCfg.name);
        //                 descText.text = string.Format(LanguageManager.GetText(taskCfg.desc), paramArray);
        //                 return bFinish;
        //         }
        //         public void UpdateMainTask(string countstr)
        //         {
        //                 if (m_TaskCfg != null)
        //                 {
        //                         if (UpdateTaskCount(m_TaskCfg, countstr, m_TaskTitle, m_TaskDesc))
        //                         {
        //                                 if (m_MainTaskFinish.gameObject.activeSelf == false)
        //                                 {
        //                                         UIManager.GetInst().ShowUI<UI_RaidTaskFinish>("UI_RaidTaskFinish");
        //                                         m_MainTaskFinish.gameObject.SetActive(true);
        //                                 }
        //                                 m_MainTaskUnfinish.gameObject.SetActive(false);
        //                         }
        //                         else
        //                         {
        //                                 m_MainTaskFinish.gameObject.SetActive(false);
        //                                 m_MainTaskUnfinish.gameObject.SetActive(true);
        //                         }
        //                 }
        //                 else
        //                 {
        //                         m_TaskTitle.text = "";
        //                         m_TaskDesc.text = "";
        //                 }
        //         }
        //         public void UpdateBranchTask(TASK_INFO taskInfo)
        //         {
        //                 RaidTaskConfig taskCfg = RaidConfigManager.GetInst().GetTaskConfig(taskInfo.taskId);
        //                 if (taskCfg != null)
        //                 {
        //                         if (taskInfo.state == 1)
        //                         {
        //                                 if (m_BranchDict.ContainsKey(taskCfg.id))
        //                                 {
        //                                         GameObject.Destroy(m_BranchDict[taskCfg.id]);
        //                                         m_BranchDict.Remove(taskCfg.id);
        //                                 }
        //                         }
        //                         else
        //                         {
        //                                 GameObject taskGroup = null;
        //                                 if (!m_BranchDict.ContainsKey(taskCfg.id))
        //                                 {
        //                                         GameObject cloneElem = CloneElement(m_Task0);
        //                                         cloneElem.transform.SetParent(m_BranchTaskGroup.transform);
        //                                         m_BranchDict.Add(taskCfg.id, cloneElem);
        //                                 }
        //                                 taskGroup = m_BranchDict[taskCfg.id];
        // 
        //                                 UpdateTaskCount(taskCfg, taskInfo.countstr, GetText(taskGroup, "tasktitle"), GetText(taskGroup, "taskdesc"));
        //                         }
        //                 }
        //         }
        //         public void SetRaidTask(int raidId, string countstr)
        //         {
        //                 m_TaskCfg = null;
        //                 m_bTaskFinish = false;
        //                 m_MainTaskFinish.gameObject.SetActive(false);
        //                 m_MainTaskUnfinish.gameObject.SetActive(true);
        //                 m_TaskTitle.text = "";
        //                 m_TaskDesc.text = "";
        // 
        //                 m_TaskCfg = RaidConfigManager.GetInst().GetRaidMainTask();
        //                 UpdateMainTask(countstr);
        //         }
        // 
        //         #endregion

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
                                string icon = ModelResourceManager.GetInst().GetIconRes(cfg.modelid);
                                ResourceManager.GetInst().LoadIconSpriteSyn(icon, GetImage(heroObj, "playericon").transform);
                                m_HeroObjDict.Add(unit.ID, heroObj);

                                SetHeroHp(unit.ID, unit.hero.Hp, unit.hero.MaxHp);
                                SetHeroPressure(unit.ID, unit.hero.Pressure);
                                SetHeroBuff(unit.ID, unit.hero.m_BuffList);
                                SetHeroBehaviour(unit.ID, unit.hero.GoodSpecCount, unit.hero.BadSpecCount);

                                GameObject specGroupObj = GetGameObject(heroObj, "specificty_group");
                                if (idx == 0 && specGroupObj.GetComponent<EventTriggerListener>() == null)
                                {
                                        EventTriggerListener.Get(specGroupObj).onClick = OnClickSpecTips;
                                        EventTriggerListener.Get(specGroupObj).SetTag(GetImage(specGroupObj, "spectips").gameObject);
                                }
                                idx++;
                        }
                }
        }
        public void SetHeroPressure(long id, int pressure)
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
        public void SetHeroHp(long id, int hp, int maxhp)
        {
                if (m_HeroObjDict.ContainsKey(id))
                {
                        GetImage(m_HeroObjDict[id], "hp").fillAmount = hp / (float)maxhp;
                        GetText(m_HeroObjDict[id], "hp_value").text = hp + CommonString.divideStr + maxhp;
                }
        }
        public void SetHeroBuff(long id, List<RaidHero.HeroBuff> buffList)
        {
                if (m_HeroObjDict.ContainsKey(id))
                {
                        Transform buffgroup = m_HeroObjDict[id].transform.Find("buffgroup");
                        foreach (Transform trans in m_HeroObjDict[id].GetComponentsInChildren<Transform>(true))
                        {
                                if (trans.parent == buffgroup)
                                {
                                        GameObject.Destroy(trans.gameObject);
                                }
                        }

                        foreach (RaidHero.HeroBuff buff in buffList)
                        {
                                SkillBuffConfig cfg = SkillManager.GetInst().GetBuff(buff.cfgId);
                                if (cfg != null)
                                {
                                        GameObject buffIcon = CloneElement(m_BuffIcon, "buff_" + buff.cfgId);
                                        ResourceManager.GetInst().LoadIconSpriteSyn(cfg.icon, buffIcon.transform);
                                        buffIcon.transform.SetParent(buffgroup);
                                        buffIcon.transform.localScale = Vector3.one;
                                }
                        }
                }
        }

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
                int index = int.Parse(go.name.Replace("player", ""));
                VillageManager.GetInst().SwitchCaptain(index);
        }

        public void OnClickInvade()
        {
                VillageManager.GetInst().SendInvade();
        }
        public void OnClickExit()
        {
                VillageManager.GetInst().ExitVillage();
                UIManager.GetInst().CloseUI(this.name);
        }
        #endregion
}
