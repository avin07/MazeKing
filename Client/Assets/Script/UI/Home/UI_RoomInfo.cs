//using UnityEngine;
//using System.Collections;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using System.Collections.Generic;
//using System;
//using System.Linq;

//public class UI_RoomInfo : UIBehaviour  //作为一个公用部件 直接实例化后挂载在所在的ui窗口上
//{
//        Transform roomInfo;
//        Transform furnitureTf;

//        void Awake()
//        {
//                roomInfo = transform.Find("roominfo");
//                furnitureTf = roomInfo.Find("furniturelist/content/furniture");

//                EventTriggerListener.Get(transform.Find("mask")).onClick = OnClickClose;
//        }

//        public void OnClickClose(GameObject go, PointerEventData data)
//        {
//                UIManager.GetInst().CloseUI(this.name);
//                UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", true);
//        }

//        public void Refresh(BuildInfo info)
//        {
//                UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", false);
//                int suitId = HomeManager.GetInst().GetMySuit(info.id);
//                RefreshInfo(suitId, info.strfitment);
//        }

//        void RefreshInfo(int suitId, string furnitures)
//        {
//                FindComponent<Text>(roomInfo, "name").text = HomeManager.GetInst().GetRoomName(suitId);
//                Button suitBtn = FindComponent<Button>(roomInfo, "suitbtn");
//                if (suitId == 0)
//                {
//                        FindComponent<Text>(roomInfo, "suitdes").text = "快去凑齐家具套装吧！";
//                        suitBtn.gameObject.SetActive(false);
//                        suitBtn.onClick.RemoveAllListeners();
//                }
//                else
//                {
//                        FurnitureSetConfig fsc = HomeManager.GetInst().GetFurnitureSetCfg(suitId);
//                        FindComponent<Text>(roomInfo, "suitdes").text = LanguageManager.GetText(fsc.describe);
//                        suitBtn.gameObject.SetActive(true);
//                        suitBtn.onClick.AddListener(() => OnClickActiveSuit(suitId));
//                }

//                Dictionary<int, int> furnitureDic = HomeManager.GetInst().GetRoomFurniture(furnitures);

//                SetChildActive(furnitureTf.parent, false);

//                Transform furnitureTemp;
//                BuildingFurnitureConfig bfc;
//                int i = 0;
                
//                foreach (int id in furnitureDic.Keys)
//                {
//                        bfc = HomeManager.GetInst().GetFurnitureCfg(id);
//                        furnitureTemp = GetChildByIndex(furnitureTf.parent, i);
//                        if (furnitureTemp == null)
//                        {
//                                furnitureTemp = CloneElement(furnitureTf.gameObject).transform;
//                        }
//                        furnitureTemp.SetActive(true);
//                        FindComponent<Text>(furnitureTemp, "num").text = furnitureDic[id].ToString();
//                        FindComponent<Text>(furnitureTemp, "name").text = LanguageManager.GetText(bfc.name);
//                        FindComponent<Text>(furnitureTemp, "add").text = bfc.add_reputation.ToString();
//                        FindComponent<Text>(furnitureTemp, "des").text = LanguageManager.GetText(bfc.desc);
//                        ResourceManager.GetInst().LoadIconSpriteSyn(bfc.icon, FindComponent<Image>(furnitureTemp, "icon"));

//                        Button btn = FindComponent<Button>(furnitureTemp, "suitbtn");
//                        int furnitureId = id;

//                        if (bfc.mSuit.Count > 0)
//                        {
//                                btn.gameObject.SetActive(true);
//                                btn.onClick.AddListener(() => OnClickSuit(furnitureId));
//                        }
//                        else
//                        {
//                                btn.gameObject.SetActive(false);
//                                btn.onClick.RemoveAllListeners();
//                        }

//                        i++;
//                }              
 
//        }

//        void OnClickSuit(int furnitureId)
//        {
//                BuildingFurnitureConfig furnitureCfg = HomeManager.GetInst().GetFurnitureCfg(furnitureId);

//                UIManager.GetInst().ShowUI<UI_FurnitureSuitList>("UI_FurnitureSuitList").Refresh(furnitureCfg.mSuit, furnitureId, null);
//        }


//        void OnClickActiveSuit(int suitId)
//        {
//                UIManager.GetInst().ShowUI<UI_FurnitureSuitList>("UI_FurnitureSuitList").Refresh(suitId);
//        }


//}



