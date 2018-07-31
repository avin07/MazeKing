using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class UI_EndlessTower : UI_ScrollRectHelp
{

        public Transform left;
        public Transform content;


        int raid_id = 0;
        int now_step = 0;
        Sprite[] towerSprite = new Sprite[2];

        void Awake()
        {

        }

        public void Refresh(int raid)
        {
                raid_id = raid;
                now_step = WorldMapManager.GetInst().GetRaidNowMaxFloor(raid);
                RefreshRight(raid);
        }


        public override void OnClickClose(GameObject go)
        {
                base.OnClickClose(go);
                UIManager.GetInst().ShowUI<UI_WorldMap>("UI_WorldMap");
        }

        GameObject now_select;
        void RefreshRight(int raid)
        {
                List<int> step = RaidConfigManager.GetInst().MyTowerStep(raid);
                int count = step.Count;
                GameObject btn1 = GetGameObject(content.gameObject, "btn1");
                GameObject btn2 = GetGameObject(content.gameObject, "btn2");
                GameObject line1 = GetGameObject(content.gameObject, "line1");
                GameObject line2 = GetGameObject(content.gameObject, "line2");

                towerSprite[0] = FindComponent<Image>(btn1, "icon").sprite;
                towerSprite[1] = FindComponent<Image>("select").sprite;

                for (int i = 1; i <= count; i++)
                {
                        GameObject btn = GetGameObject(content.gameObject, "btn" + i);
                        GameObject line = GetGameObject(content.gameObject, "line" + i);
                        if (btn == null)
                        {
                                if (i % 2 == 1)
                                {
                                        if (i + 1 <= count) //可以画线1
                                        {
                                                if (line == null)
                                                {
                                                        line = CloneElement(line1, "line" + i);
                                                }
                                        }
                                }
                                else
                                {
                                        if (i + 1 <= count) //可以画线1
                                        {
                                                if (line == null)
                                                {
                                                        line = CloneElement(line2, "line" + i);
                                                }
                                        }
                                }

                                //保证按钮在线上面
                                if (i % 2 == 1)
                                {
                                        btn = CloneElement(btn1, "btn" + i);
                                }
                                else
                                {
                                        btn = CloneElement(btn2, "btn" + i);
                                }


                                btn.GetComponent<RectTransform>().anchoredPosition += new Vector2(0, (btn.GetComponent<RectTransform>().rect.height + 90) * ((i - 1) / 2));
                                if (line != null)
                                {
                                        line.GetComponent<RectTransform>().anchoredPosition += new Vector2(0, (btn.GetComponent<RectTransform>().rect.height + 90) * ((i - 1) / 2));
                                }
                        }
                        if (step[i - 1] > now_step)
                        {
                                GetText(btn, "Text").text = "??";
                                FindComponent<Image>(btn, "icon").sprite = towerSprite[0];
                                //btn.GetComponent<Image>().color = Color.gray;
                                btn.GetComponent<Button>().interactable = false;
                                //if (line != null)
                                //{
                                //        line.GetComponent<Image>().color = Color.gray;
                                //}
                        }
                        else
                        {
                                GetText(btn, "Text").text = step[i - 1].ToString();
                                FindComponent<Image>(btn, "icon").sprite = towerSprite[0];
                                btn.GetComponent<Button>().interactable = true;
                                if (step[i - 1] == now_step)
                                {
                                        //btn.GetComponent<Image>().color = Color.green;
                                        FindComponent<Image>(btn, "icon").sprite = towerSprite[1];
                                        now_select = btn;
                                }
                                //if (line != null)
                                //{
                                //        line.GetComponent<Image>().color = Color.white;
                                //}
                        }
                        EventTriggerListener listener = EventTriggerListener.Get(btn);
                        listener.onClick = OnStepClick;
                        listener.onDrag = OnDarg;
                        listener.onBeginDrag = OnBeginDrag;
                        listener.onEndDrag = OnEndDrag;
                        listener.SetTag(step[i - 1]);  //记录楼层//
                }
                float content_height = GetGameObject(content.gameObject, "btn" + count).GetComponent<RectTransform>().anchoredPosition.y + 80f;
                GameObject tower0 = GetGameObject(content.gameObject, "tower0");
                int tower_num = (int)(content_height / tower0.GetComponent<RectTransform>().rect.height) + 1;
                for (int i = 0; i < tower_num; i++)
                {
                        GameObject tower = GetGameObject(content.gameObject, "tower" + i);
                        if (tower == null)
                        {
                                tower = CloneElement(tower0, "tower" + i);
                                tower.GetComponent<RectTransform>().anchoredPosition += new Vector2(0, tower.GetComponent<RectTransform>().rect.height * i);
                        }
                        tower.transform.SetAsFirstSibling();

                }

                StartCoroutine(WaitForsizeDelta(content_height));
        }

        public IEnumerator WaitForsizeDelta(float height)
        {
                yield return new WaitForEndOfFrame();

                RectTransform rt = content.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
                float mid_y = content.parent.gameObject.GetComponent<RectTransform>().sizeDelta.y / 2;
                float delat_y = now_select.GetComponent<RectTransform>().anchoredPosition.y - mid_y;
                if (delat_y < 0)
                {
                        rt.anchoredPosition = Vector2.zero;
                }
                else
                {
                        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, delat_y * (-1));
                }
                OnStepClick(now_select, null);
        }


        void RefreshLeft(int id,int floor) //raidmap 主键
        {
                RaidMapHold rmh = RaidConfigManager.GetInst().GetRaidMapCfg(id * 1000 + floor);
                FindComponent<Text>(left, "level").text = RaidConfigManager.GetInst().GetRaidInfoCfg(id).raid_level.ToString();
                FindComponent<Text>(left, "des").text = LanguageManager.GetText(rmh.desc);
                FindComponent<Text>("bg/name").text = LanguageManager.GetText(rmh.name);
                FindComponent<Text>(left, "tip").text = LanguageManager.GetText(rmh.adventure_keyword);
        }

        void OnStepClick(GameObject go, PointerEventData data)
        {
                if (!can_click)
                {
                        return;
                }
                if (go.GetComponent<Button>().interactable)
                {
                        //now_select.GetComponent<Image>().color = Color.white;
                        //go.GetComponent<Image>().color = Color.green;

                        FindComponent<Image>(now_select, "icon").sprite = towerSprite[0];
                        FindComponent<Image>(go, "icon").sprite = towerSprite[1];

                        now_select = go;
                        int floor = (int)EventTriggerListener.Get(go).GetTag();
                        RefreshLeft(raid_id,floor);
                }
        }


        public void OnClickGo()  //点击进去按键，弹出选人的界面//
        {
                int floor = (int)EventTriggerListener.Get(now_select).GetTag();
                //UIManager.GetInst().ShowUI<UI_RaidPetChoose>("UI_RaidPetChoose").InitLineUp(raid_id,floor,gameObject);
                UI_RaidFormation uis = UIManager.GetInst().ShowUI<UI_RaidFormation>("UI_RaidFormation");
                uis.Setup(raid_id, floor, this);

        }


}
