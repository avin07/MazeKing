using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UI_RaidCampFood : UIBehaviour
{
        public GameObject m_Food0;
        public GameObject m_Item0;
        
        GameObject[] m_FoodObjectList;

        DropObject[] m_FoodItemList;

        GameObject FoodSlotRoot;
        void Awake()
        {
                //m_Food0.SetActive(false);
                FoodSlotRoot = GetGameObject("FoodSlot");
        }

        int m_nSkillPoint = 0;
        int SkillPoint
        {
                get
                {
                        return m_nSkillPoint;
                }
                set
                {
                        m_nSkillPoint = value;

                        GetText("foodvalue").text = m_nSkillPoint.ToString();
                }
        }
        public void Setup()
        {
                int maxCount = 0;
                List<HeroUnit> list = RaidTeamManager.GetInst().GetHeroList();
                foreach (HeroUnit unit in list)
                {
                        if (unit.IsAlive)
                        {
                                maxCount++;
                        }
                }
                SkillPoint = 0;
                m_FoodObjectList = new GameObject[maxCount];
                m_FoodItemList = new DropObject[maxCount];
                for (int i = 0; i < maxCount; i++)
                {
                        GameObject obj = m_Food0.gameObject;
                        if (i > 0)
                        {
                                obj = CloneElement(m_Food0.gameObject);
                                obj.name = "food" + i;
                        }

                        EventTriggerListener.Get(obj).onClick = OnClickFood;
                        EventTriggerListener.Get(obj).SetTag(i);
                        m_FoodObjectList[i] = obj;
                }
        }

        void OnClickFood(GameObject go, PointerEventData data)
        {
                int i = (int)EventTriggerListener.Get(go).GetTag();
                Toggle toggle = go.GetComponent<Toggle>();
                if (m_FoodItemList[i] != null)
                {
                        SkillPoint -= m_FoodItemList[i].CampSP;
                        m_FoodItemList[i] = null;
                        UIUtility.SetupItemElem(go, null);
                        m_FoodItemList[i] = null;
                }
        }

        public void AddFoodItem(DropObject di)
        {
                for (int i = 0; i < m_FoodItemList.Length; i++)
                {
                        if (m_FoodItemList[i] == null)
                        {
                                UIUtility.SetupItemElem(m_FoodObjectList[i], di);

                                m_FoodObjectList[i].GetComponent<Toggle>().isOn = true;
                                m_FoodItemList[i] = di;
                                SkillPoint += di.CampSP;
                                return;
                        }
                }
        }

        void OnClickItem(GameObject go, PointerEventData data)
        {
                DropObject di = (DropObject)EventTriggerListener.Get(go).GetTag();
                Toggle toggle = go.GetComponent<Toggle>();
                if (toggle.isOn)
                {
                }
                else
                {
                        for (int i = 0; i < m_FoodItemList.Length; i++)
                        {
                                if (m_FoodItemList[i] == di)
                                {

                                        return;
                                }
                        }
                }
        }

        public void OnClickBack(GameObject go)
        {
                string str = "";
                if (m_FoodItemList != null)
                {
                        for(int i = 0; i < m_FoodItemList.Length; i++)
                        {
                                if (m_FoodItemList[i] != null)
                                {
                                        str += m_FoodItemList[i].idCfg + "|";
                                }
                        }
                }
                if (!string.IsNullOrEmpty(str))
                {
                        CampManager.GetInst().FinishCampEat(str);
                }
                else
                {
                        CampManager.GetInst().FinishCampEat("0");
                }
                OnClickClose(null);

        }
}
