using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_RaidResult : UIBehaviour 
{
        public GameObject m_Group0;
        public GameObject m_Group1;
        public GameObject m_Reward0;
        public GameObject m_Item0;
        public GameObject m_Pet0;

        public Text m_GoldText;
        public List<Sprite> m_ResultImages = new List<Sprite>();
        public Image m_ResultImage;
        List<GameObject> m_ItemObjlist = new List<GameObject>();
        List<GameObject> m_PetObjlist = new List<GameObject>();

        List<GameObject> m_RewardObjlist = new List<GameObject>();

        int m_nExp = 0;

        void Awake()
        {
                m_ItemObjlist.Add(m_Item0);
                m_PetObjlist.Add(m_Pet0);

                m_RewardObjlist.Add(m_Reward0);
                //GetButton(m_Group1, "btnspec").onClick.AddListener(OnClickUnlockSpec);
        }

        GameObject GetRewardObj(int idx)
        {
                GameObject obj = null;
                if (idx < m_RewardObjlist.Count)
                {
                        obj = m_RewardObjlist[idx];
                }
                else
                {
                        obj = CloneElement(m_Reward0, "reward" + idx);
                        m_RewardObjlist.Add(obj);
                }
                return obj;
        }


        GameObject GetPetObj(int idx)
        {
                GameObject obj = null;
                if (idx < m_PetObjlist.Count)
                {
                        obj = m_PetObjlist[idx];
                }
                else
                {
                        obj = CloneElement(m_Pet0, "pet" + idx);
                        m_PetObjlist.Add(obj);
                }
                return obj;
        }

        GameObject GetItemObj(int idx)
        {
                GameObject obj = null;
                if (idx < m_ItemObjlist.Count)
                {
                        obj = m_ItemObjlist[idx];
                }
                else
                {
                        obj = CloneElement(m_Item0, "item" + idx);
                        m_ItemObjlist.Add(obj);
                }
                return obj;
        }
        bool m_bShowGold;
        bool m_bShowItem;
        int m_nResultFlag;
        public void SetRaidResult(SCMsgRaidMapLeave msg)
        {
                m_nResultFlag = msg.flag;
                m_Group0.SetActive(true);
                m_Group1.SetActive(false);
                m_GoldText.text = CommonString.zeroStr;
                if (msg.flag < m_ResultImages.Count)
                {
                        m_ResultImage.sprite = m_ResultImages[msg.flag];
                }
                m_nExp = msg.exp;

                m_bShowGold = false;
                m_bShowItem = false;
                GetButton("btnnext").gameObject.SetActive(false);

                RaidMapHold cfg = RaidConfigManager.GetInst().GetRaidMapCfg(RaidManager.GetInst().RaidID);
                if (cfg != null)
                {
                        GetText("raidname").text = LanguageManager.GetText(cfg.name);
                }

                StartCoroutine(ShowGold(msg.money));
                StartCoroutine(ShowItem(msg.dropReward));
                SetTaskReward(msg.flag == 1);
                SetBehaviour(msg.behavior);
        }

        Dictionary<long, string> m_BehaviorDict = new Dictionary<long, string>();
        void SetBehaviour(string behavior)
        {
//                 GetText("specname").enabled = false;
//                 GetText("specname").text = "";
                string[] infos = behavior.Split('|');
                foreach (string info in infos)
                {
                        if (string.IsNullOrEmpty(info))
                        {
                                continue;
                        }
                        string[] tmps = info.Split('&');
                        if (tmps.Length == 3)
                        {
                                long petId = long.Parse(tmps[0]);
                                string str = "";
                                if (!m_BehaviorDict.ContainsKey(petId))
                                {
                                        m_BehaviorDict.Add(petId, str);
                                }
                                else
                                {
                                        str = m_BehaviorDict[petId];
                                }

                                SpecificityHold cfg0 = PetManager.GetInst().GetSpecificityCfg(int.Parse(tmps[1]));
                                if (cfg0 != null)
                                {
                                        str += LanguageManager.GetText(cfg0.name);
                                }
                                str += "->";
                                SpecificityHold cfg1 = PetManager.GetInst().GetSpecificityCfg(int.Parse(tmps[2]));
                                if (cfg1 != null)
                                {
                                        str += LanguageManager.GetText(cfg1.name) + "\n";
                                }
                                m_BehaviorDict[petId] = str;
                        }
                }
        }

        void SetTaskReward(bool bFinish)
        {
                string reward = TaskManager.GetInst().GetCurrRaidTaskReward();
                Debuger.Log(reward);
                int idx = 0;
                string[] rewards = reward.Split(';');
                foreach (string info in rewards)
                {
                        if (string.IsNullOrEmpty(info))
                                continue;
                        string[] tmps = info.Split(',');
                        if (tmps.Length == 3)
                        {
                                DropObject di = new DropObject();
                                di.nType = int.Parse(tmps[0]);
                                di.idCfg = int.Parse(tmps[1]);
                                di.nOverlap = int.Parse(tmps[2]);

                                GameObject rewardObj = GetRewardObj(idx);
                                string iconname = di.GetIconName();
                                if (!string.IsNullOrEmpty(iconname))
                                {
                                        GetImage(rewardObj, "icon").enabled = true;
                                        GetImage(rewardObj, "icon").color = bFinish ? Color.white : Color.gray;
                                        ResourceManager.GetInst().LoadIconSpriteSyn(iconname, GetImage(rewardObj, "icon").transform);
                                }
                                GetText(rewardObj, "count").text = di.nOverlap.ToString();
                                idx++;
                        }
                }
        }

        IEnumerator ShowGold(int maxGold)
        {
                int gold = 0;
                int speed = Mathf.Max((int)(maxGold / 100f), 5);
                while (gold < maxGold)
                {
                        m_GoldText.text = gold.ToString();
                        gold += speed;
                        yield return null;
                }
                m_GoldText.text = maxGold.ToString();
                m_bShowGold = true;
//                 if (m_bShowItem)
//                 {
//                         GetButton("btnnext").gameObject.SetActive(true);
//                 }
        }

        IEnumerator ShowItem(string info)
        {
                string[] infos = info.Split('|');
                int idx = 0;
                foreach (string itemstr in infos)
                {
                        if (string.IsNullOrEmpty(itemstr))
                                continue;

                        string[] tmps = itemstr.Split('&');
                        if (tmps.Length >= 3)
                        {
                                DropObject di = new DropObject();
                                di.nType = int.Parse(tmps[0]);
                                di.idCfg = int.Parse(tmps[1]);
                                di.nOverlap = int.Parse(tmps[2]);
                                GameObject itemObj = GetItemObj(idx);

                                string iconname = di.GetIconName();
                                if (!string.IsNullOrEmpty(iconname))
                                {
                                        GetImage(itemObj, "icon").enabled = true;
                                        ResourceManager.GetInst().LoadIconSpriteSyn(iconname, GetImage(itemObj, "icon").transform);

                                }
                                GetText(itemObj, "count").text = di.nOverlap.ToString();
                                idx++;
                        }
                        yield return new WaitForSeconds(0.05f);
                }
                m_bShowItem = true;
//                 if (m_bShowGold)
//                 {
//                         GetButton("btnnext").gameObject.SetActive(true);
//                 }
        }
        int m_nExpCount = 0;
        public void OnClickNext()
        {
                m_Group0.SetActive(false);
                m_Group1.SetActive(true);
                GetButton("btnexit").gameObject.SetActive(false);
                GetButton("bg").enabled = false;

                string[] teams = RaidManager.GetInst().TeamInfo.Split('|');
                m_nExpCount = 0;
                int idx = 0;
                foreach (string petId in teams)
                {
                        if (string.IsNullOrEmpty(petId))
                                continue;
                        RaidHero hero = RaidTeamManager.GetInst().GetRaidHero(long.Parse(petId));
                        if (hero != null)
                        {
                                Pet pet = PetManager.GetInst().GetPet(hero.PetID);
                                if (pet != null)
                                {
                                        m_nExpCount++;
                                        GameObject petObj = GetPetObj(idx);
                                        if (m_BehaviorDict.ContainsKey(pet.ID))
                                        {
                                                GetText(petObj, "specname").text = m_BehaviorDict[pet.ID];
                                                GetGameObject(petObj, "unlockspec").gameObject.SetActive(true);
                                                StartCoroutine(ShowBehaviour(petObj));
                                        }
                                        else
                                        {
                                                GetText(petObj, "specname").text = "";
                                                GetText(petObj, "specname").gameObject.SetActive(false);
                                                GetGameObject(petObj, "unlockspec").gameObject.SetActive(true);
                                        }

                                        GetImage(petObj, "peticon").enabled = true;
                                        ResourceManager.GetInst().LoadIconSpriteSyn(CharacterManager.GetInst().GetCharacterIcon(pet.CharacterID), GetImage(petObj, "peticon").transform);

                                        StartCoroutine(ShowExp(petObj, pet));
                                        idx++;
                                }
                        }
                }
        }

        IEnumerator ShowBehaviour(GameObject petObj)
        {
                GetGameObject(petObj, "specname").SetActive(false);
                Image unlockIm = GetImage(petObj, "unlockspec");
                unlockIm.enabled = false;
                UIUtility.SetUIEffect(this.name, unlockIm.gameObject, true, "effect_balance_specificity_get");
                yield return new WaitForSeconds(1f);

                GetGameObject(petObj, "unlockspec").SetActive(false);
                GetGameObject(petObj, "specname").SetActive(true);
        }


        IEnumerator ShowExp(GameObject petObj, Pet pet)
        {
                int level = pet.GetPropertyInt("level");

                GetText(petObj, "level").text ="Lv." + level.ToString();
                GetText(petObj, "expval").text = "Exp+" + m_nExp.ToString();
                
                Image expImage = GetImage(petObj, "exp");
                long exp = pet.GetPropertyLong("exp");
                long endExp = exp + m_nExp;

                int speed = Mathf.Max((int)(m_nExp / 100f), 5);

                while (exp < endExp)
                {
                        Experience expCfg = CharacterManager.GetInst().GetCharacterExp(level + 1);
                        if (expCfg == null)
                        {
                                break;
                        }
                        long maxexp = expCfg.need_exp;
                        if (expCfg != null)
                        {
                                expImage.fillAmount = exp / (float)maxexp;

                                exp += speed;
                                if (exp >= maxexp)
                                {
                                        level++;
                                        GetText(petObj, "level").text = "Lv." + level.ToString();
                                        endExp -= maxexp;
                                        exp -= maxexp;
                                }
                                yield return null;
                        }
                        else
                        {
                                expImage.fillAmount = endExp / (float)maxexp;
                                break;
                        }
                }

                m_nExpCount--;
                if (m_nExpCount <= 0)
                {
                        GetButton("btnexit").gameObject.SetActive(true);
                }
        }

        public void OnClickExit()
        {
                UIManager.GetInst().CloseUI(this.name);
                RaidManager.GetInst().ExitRaid();
                HomeManager.GetInst().LoadHome();
                //UIManager.GetInst().ShowUI<UI_WorldMap>("UI_WorldMap");
//                ShowResultTip();
        }

        void ShowResultTip()
        {
                string raid_name = "";
                RaidMapHold cfg = RaidConfigManager.GetInst().GetRaidMapCfg(RaidManager.GetInst().RaidID);
                if (cfg != null)
                {
                        raid_name = LanguageManager.GetText(cfg.name);
                }
                if (m_nResultFlag == 1)
                {
                        GameUtility.PopupMessage(raid_name + "挑战胜利！");
                }
                else
                {
                        GameUtility.PopupMessage(raid_name + "挑战失败！");
                }
        }



        public void OnClickUnlockSpec()
        {
                GetButton(m_Group1, "btnspec").gameObject.SetActive(false);
        }
}
