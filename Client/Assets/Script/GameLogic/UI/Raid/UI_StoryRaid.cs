using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class UI_StoryRaid : UIBehaviour
{

        public Transform left;
        public Transform right;
        Transform select;

        int raid_id = 0;
        int floor = 0;

        void Awake()
        {
                select = right.Find("choose");
        }

        void ChooseDifficulty(GameObject go, PointerEventData data)
        {
                floor = (int)EventTriggerListener.Get(go).GetTag() + 1;
                (select as RectTransform).anchoredPosition = (go.transform as RectTransform).anchoredPosition;
                int raidBaseId = raid_id - raid_id % 10;
                RefreshLeft(raidBaseId + floor);
        }

        void ChooseDifficultyTip(GameObject go, PointerEventData data)
        {
                GameUtility.PopupMessage("请先通过前面的难度！");
        }

        public void Refresh(int raid)
        {
                raid_id = raid;
                int index = raid_id % 10 - 1;
                RefreshRight();
                ChooseDifficulty(GetGameObject(right.gameObject, "btn" + index), null);
        }


        public override void OnClickClose(GameObject go)
        {
                base.OnClickClose(go);
                UIManager.GetInst().ShowUI<UI_WorldMap>("UI_WorldMap");
        }

        void RefreshRight()
        {
                int diffity = raid_id % 10;
                int baseRaid = raid_id - diffity;
                int maxRaidId = RaidConfigManager.GetInst().GetRaidMaxDifficulty(baseRaid);

                Transform item;

                for (int i = 0; i < 4; i++)
                {
                        GameObject btn = right.Find("btn" + i).gameObject;
                        if (i < maxRaidId - baseRaid)
                        {
                                int raidId = baseRaid + i + 1;
                                int raidTaskId = RaidConfigManager.GetInst().GetRaidInfoCfg(raidId).raid_task_id;
                                string []reward = RaidConfigManager.GetInst().GetTaskConfig(raidTaskId).reward_item_list.Split(new char[]{';'},StringSplitOptions.RemoveEmptyEntries);

                                for (int j = 0; j < 2; j++)
                                {
                                        item = btn.transform.Find("item" + j);
                                        if (j < reward.Length)
                                        {

                                                int num;
                                                string name;
                                                int id;
                                                string des;
                                                Thing_Type type;

                                                Image quality_image = FindComponent<Image>(item, "quality");
                                                Image icon_image = FindComponent<Image>(item, "icon");
                                                Text num_down = FindComponent<Text>(item, "num_down");

                                                CommonDataManager.GetInst().SetThingIcon(reward[j], icon_image.transform, quality_image.transform, out name, out num, out id, out des, out type);
                                                num_down.text = num.ToString();
                                                item.SetActive(true);
                                        }
                                        else
                                        {
                                                item.SetActive(false);
                                        }
                                }

                                if (i < diffity)
                                {
                                        EventTriggerListener.Get(btn).SetTag(i);
                                        EventTriggerListener.Get(btn).onClick = ChooseDifficulty;
                                        btn.transform.Find("lock").SetActive(false);
                                }
                                else
                                {
                                        EventTriggerListener.Get(btn).SetTag(null);
                                        EventTriggerListener.Get(btn).onClick = ChooseDifficultyTip;
                                        btn.transform.Find("lock").SetActive(true);
                                }
                                btn.SetActive(true);
                        }
                        else
                        {
                                btn.SetActive(false);
                        }                        
                }
        }



        void RefreshLeft(int id) //raidmap 主键
        {              
                RaidMapHold rmh = RaidConfigManager.GetInst().GetRaidMapCfg(id * 1000 + 1);
                FindComponent<Text>(left, "level").text = RaidConfigManager.GetInst().GetRaidInfoCfg(id).raid_level.ToString();
                FindComponent<Text>(left, "des").text = LanguageManager.GetText(rmh.desc);
                FindComponent<Text>("bg/name").text = LanguageManager.GetText(rmh.name);
                FindComponent<Text>(left, "tip").text = LanguageManager.GetText(rmh.adventure_keyword);            
        }


        public void OnClickGo()  //点击进去按键，弹出选人的界面//
        {
                UI_RaidFormation uis = UIManager.GetInst().ShowUI<UI_RaidFormation>("UI_RaidFormation");
                uis.Setup(raid_id, floor, this);
        }
}

