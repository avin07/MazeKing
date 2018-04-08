//using UnityEngine;
//using System.Collections;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using System;

//public class UI_CreatFurniture : UIBehaviour
//{

//        void Awake()
//        {
//                EventTriggerListener.Get(transform.Find("mask")).onClick = OnClose;
//                material = transform.Find("Image/need0");
//                make_btn = FindComponent<Button>("Image/make");
//                make_btn.onClick.AddListener(OnMake);
//        }


//        void OnClose(GameObject go, PointerEventData data)
//        {
//                AppMain.GetInst().StartCoroutine(WaitForClose());
//        }

//        IEnumerator WaitForClose()
//        {
//                yield return new WaitForSeconds(0.01f);
//                UIManager.GetInst().CloseUI(this.name);
//        }

//        Transform material;
//        Button make_btn;
//        int furniture_id;
//        int furniture_index;
//        public void Refresh(string str)
//        {
//                string []temp = str.Replace("effect", "").Split('_');
//                furniture_id = int.Parse(temp[1]);
//                furniture_index = int.Parse(temp[0]);
//                BuildingFurnitureConfig FurnitureConfig = HomeManager.GetInst().GetFurnitureCfg(furniture_id);
//                FindComponent<Text>("Image/name").text = LanguageManager.GetText(FurnitureConfig.name);
//                FindComponent<Text>("Image/des").text = LanguageManager.GetText(FurnitureConfig.desc);
//                ResourceManager.GetInst().LoadIconSpriteSyn(FurnitureConfig.icon, transform.Find("Image/icon"));
//                FindComponent<Text>("Image/time").text = FurnitureConfig.cost_time + "秒";

//                int can_make_max = 1;
//                string require = FurnitureConfig.cost_material;
//                string[] detail = require.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
//                int num = 0;
//                string name = "";
//                int id = 0;
//                string des = "";
//                Thing_Type type;
//                for (int i = 0; i < detail.Length; i++)
//                {
//                        Transform m_material = transform.Find("Image/need" + i);
//                        if (m_material == null)
//                        {
//                                m_material = CloneElement(material.gameObject, "need" + i).transform;
//                        }
//                        RectTransform rt = m_material as RectTransform;
//                        RectTransform ori_rt = material as RectTransform;
//                        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, ori_rt.anchoredPosition.y - i * (ori_rt.rect.height - 5.0f));
//                        m_material.SetActive(true);
//                        Transform tf = m_material.Find("icon");
//                        CommonDataManager.GetInst().SetThingIcon(detail[i], tf, null,out name, out num, out id, out des, out type);
 
//                        int has_num = CommonDataManager.GetInst().GetThingNum(detail[i], out num);
//                        int max = has_num / num;
//                        if (max < can_make_max)
//                        {
//                                can_make_max = max;
//                        }

//                        if (max < 1)
//                        {
//                                FindComponent<Text>(m_material, "has_num").text = "<color=red>" + has_num.ToString() + "</color>";
//                        }
//                        else
//                        {
//                                FindComponent<Text>(m_material, "has_num").text = has_num.ToString();
//                        }
//                        FindComponent<Text>(m_material, "need_num").text = num.ToString();

//                }

//                int need_level = FurnitureConfig.build_limit_level;

//                if (need_level > HomeManager.GetInst().MainBuildLevel)
//                {
//                        FindComponent<Text>("Image/level").text = "需要设计室等级" + need_level + "级";
//                        FindComponent<Button>("Image/make").interactable = false;
//                }
//                else
//                {
//                        FindComponent<Text>("Image/level").text = "";
//                        if (can_make_max > 0)
//                        {
//                                FindComponent<Button>("Image/make").interactable = true;
//                        }
//                        else
//                        {
//                                FindComponent<Button>("Image/make").interactable = false;
//                        }
//                }

               

//        }

//        void OnMake()
//        {
//                if (make_btn.interactable == false)
//                {
//                        GameUtility.PopupMessage("建造条件不足！");
//                }
//                else
//                {
//                        HomeManager.GetInst().SendBuildFitment(furniture_id, furniture_index);
//                        UIManager.GetInst().CloseUI(this.name);
//                        HomeManager.GetInst().ExitFurnitureEditor();
                        
//                }
//        }




//}

