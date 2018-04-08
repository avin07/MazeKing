using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using System.Linq;

public class UI_FurnitureSuitList : UIBehaviour  
{
        Transform root;
        Transform suitInfo;
        Transform furnitureTf;
        Transform suitList;
        Transform suitTf;

        HashSet<int> mActiveSuit = new HashSet<int>();

        void Awake()
        {
                EventTriggerListener.Get(transform.Find("mask")).onClick = OnClickClose;
                root = transform.Find("root");

                suitList = root.Find("suitlist");
                suitTf = suitList.Find("content/suit");

                suitInfo = root.Find("suitinfo");
                furnitureTf = suitInfo.Find("furniturelist/content/furniture");
        }

        public void OnClickClose(GameObject go, PointerEventData data)
        {
                UIManager.GetInst().CloseUI(this.name);
                //UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", true);
        }

        public void Refresh()
        {
                mActiveSuit = HomeManager.GetInst().GetHomeActiveSuits();
                RefreshLeft();
        }


        void RefreshLeft()
        {
                List<int> mSuits = HomeManager.GetInst().GetAllFurnitureSuitId();

                if (mSuits.Count > 1)
                {
                        suitList.SetActive(true);
                        SetChildActive(suitTf.parent, false);
                        Transform suitTemp;
                        FurnitureSetConfig fsc;
                        for (int i = 0; i < mSuits.Count; i++)
                        {
                                fsc = HomeManager.GetInst().GetFurnitureSetCfg(mSuits[i]);
                                suitTemp = GetChildByIndex(suitTf.parent, i);
                                if (suitTemp == null)
                                {
                                        suitTemp = CloneElement(suitTf.gameObject).transform;
                                }
                                suitTemp.SetActive(true);
                                FindComponent<Text>(suitTemp, "name").text = LanguageManager.GetText(fsc.name);
                                int suitId = fsc.id;
                                int index = i;

                                if (mActiveSuit.Contains(mSuits[i]))
                                {
                                        suitTemp.Find("active").SetActive(true);
                                }
                                else
                                {
                                        suitTemp.Find("active").SetActive(false);
                                }

                                suitTemp.GetComponent<Button>().onClick.AddListener(() => ClickSuit(suitId, index));
                        }
                }
                else
                {
                        suitList.SetActive(false);
                }
                ClickSuit(mSuits[0], 0); //默认打开第一个任务
                (suitTf.parent as RectTransform).anchoredPosition = new Vector2((suitTf.parent as RectTransform).anchoredPosition.x,-100);
        }


        void RefreshRight(int suitId)
        {
                FurnitureSetConfig fsc = HomeManager.GetInst().GetFurnitureSetCfg(suitId);
                FindComponent<Text>(suitInfo, "name").text = LanguageManager.GetText(fsc.name);

                Dictionary<int, int> furnitureDic = HomeManager.GetInst().GetSuitFurniture(fsc.furniture_list);

                SetChildActive(furnitureTf.parent, false);

                Transform furnitureTemp;
                BuildingInfoHold bi;
                int i = 0;

                foreach (int id in furnitureDic.Keys)
                {
                        bi = HomeManager.GetInst().GetBuildInfoCfg(id * 100 + 1);
                        furnitureTemp = GetChildByIndex(furnitureTf.parent, i);
                        if (furnitureTemp == null)
                        {
                                furnitureTemp = CloneElement(furnitureTf.gameObject).transform;
                        }
                        furnitureTemp.SetActive(true);
                        FindComponent<Text>(furnitureTemp,"num").text = furnitureDic[id].ToString();
                        Text name = FindComponent<Text>(furnitureTemp, "name");
                        name.text = LanguageManager.GetText(bi.name);
                        ResourceManager.GetInst().LoadIconSpriteSyn(bi.icon, FindComponent<Image>(furnitureTemp, "icon"));
                        i++;
                }
                FindComponent<Text>(root,"des").text = LanguageManager.GetText(fsc.describe);

                if (mActiveSuit.Contains(suitId))
                {
                        suitInfo.Find("active").SetActive(true);
                }
                else
                {
                        suitInfo.Find("active").SetActive(false);
                }
        }

        void ClickSuit(int suitId, int index)
        {
                SetBtnHighlight(index);
                RefreshRight(suitId);
        }

        void SetBtnHighlight(int index)
        {
                for (int i = 0; i < suitTf.parent.childCount; i++)
                {
                        Text name = FindComponent<Text>(suitTf.parent.GetChild(i), "name"); 

                        if (i == index)
                        {
                                name.color = UIUtility.activeColor;
                        }
                        else
                        {
                                name.color = UIUtility.unactiveColor;
                        }
                }
        }

}


