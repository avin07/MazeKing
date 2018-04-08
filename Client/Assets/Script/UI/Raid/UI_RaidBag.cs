using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UI_RaidBag : UIBehaviour 
{
        int MAX_COL_NUM = 12;
        int MAX_ROW_NUM = 2;
        int m_nMaxBagSize = 0;

        public GameObject m_Item0;
        public List<GameObject> m_ItemObjectlist = new List<GameObject>();
        GameObject m_ItemGroup;
        Dictionary<GameObject, DropObject> m_ItemDict = new Dictionary<GameObject, DropObject>();
        GameObject m_SelectItem;
        GameObject m_ItemDescGroup;
        Toggle m_ToggleExpand;
        public List<DropObject> GetItemList()
        {
                return new List<DropObject>(m_ItemDict.Values);
        }

        void Awake()
        {
                m_Item0.name = "itemtmp";
                m_Item0.SetActive(false);
                for (int i = 0; i < MAX_COL_NUM * MAX_ROW_NUM; i++)
                {
                        m_ItemObjectlist.Add(CloneElement(m_Item0, "item" + i));
                }
                //m_ItemObjectlist.Add(m_Item0);
                m_ItemDescGroup = GetGameObject("itemdescgroup");
                m_ItemDescGroup.SetActive(false);
                m_ItemGroup = GetGameObject("itemgroup");
                m_ToggleExpand = GetToggle("showbag");
        }


        GameObject GetAvailItemBox()
        {
                for (int i = 0; i < m_ItemObjectlist.Count; i++)
                {
                        if (!m_ItemDict.ContainsKey(m_ItemObjectlist[i]))
                        {
                                return m_ItemObjectlist[i];
                        }
                }
                return GetItem(m_ItemObjectlist.Count);
        }

        GameObject GetItem(int idx)
        {
                GameObject obj = null;
                if (idx < m_ItemObjectlist.Count)
                {
                        obj = m_ItemObjectlist[idx];
                        obj.SetActive(true);
                }
                else
                {
                }
                return obj;
        }

        public override void OnShow(float time)
        {
                base.OnShow(time);
                m_nMaxBagSize = PlayerController.GetInst().GetPropertyInt("maze_capacity");
                RefreshBag();
        }
        public void AddItem(DropObject di)
        {
                UIUtility.SetupItemElem(GetAvailItemBox(), di);
        }
        public void UpdateItem(DropObject di)
        {
                foreach (var param in m_ItemDict)
                {
                        if (param.Value.id == di.id)
                        {
                                UIUtility.SetupItemElem(param.Key, di);
                                return;
                        }
                }
        }
        public void RemoveItem(DropObject di)
        {
                foreach (var param in m_ItemDict)
                {
                        if (param.Value.id == di.id)
                        {
                                UIUtility.SetupItemElem(param.Key, null);
                                m_ItemDict.Remove(param.Key);
                                return;
                        }
                }
        }
        public void RefreshBag()
        {
                m_ItemDescGroup.SetActive(false);

                m_ItemDict.Clear();
                List<DropObject> itemlist = ItemManager.GetInst().GetObjectList((int)ItemType.BAG_PLACE.RAID);

                for (int i = 0; i < m_ItemObjectlist.Count; i++)
                {
                        GameObject itemObj = m_ItemObjectlist[i];
                        itemObj.SetActive(i < m_nMaxBagSize);

                        if (i < itemlist.Count)
                        {
                                DropObject di = new DropObject(itemlist[i]);
                                if (!m_ItemDict.ContainsKey(itemObj))
                                {
                                        m_ItemDict.Add(itemObj, di);
                                }
                                else
                                {
                                        m_ItemDict[itemObj] = di;
                                }
                                UIUtility.SetupItemElem(itemObj, di);
                        }
                        else
                        {
                                UIUtility.SetupItemElem(itemObj, null);
                        }
                }
        }

        public void AddDropItem(ref DropObject di)
        {
                //资源不处理
                if (di.nType != (int)Thing_Type.RESOURCE)
                {
                        int maxOverlap = 1;
                        if (di.nType == (int)Thing_Type.ITEM)
                        {//只有道具才有堆叠上限
                                ItemConfig itemCfg = ItemManager.GetInst().GetItemCfg(di.idCfg);
                                if (itemCfg != null)
                                {
                                        maxOverlap = itemCfg.stackable;

                                        foreach (var param in m_ItemDict)
                                        {
                                                DropObject oriDi = param.Value;

                                                if (di.nOverlap <= 0)
                                                        continue;
                                                if (oriDi.TempId != di.TempId)
                                                        continue;
                                                if (oriDi.nOverlap >= maxOverlap)
                                                        continue;

                                                int delta = maxOverlap - oriDi.nOverlap;
                                                if (di.nOverlap <= delta)
                                                {
                                                        oriDi.nOverlap += di.nOverlap;
                                                        di.nOverlap = 0;
                                                }
                                                else
                                                {
                                                        di.nOverlap -= delta;
                                                        oriDi.nOverlap = maxOverlap;
                                                }
                                                GetText(param.Key, "itemcount").text = oriDi.nOverlap.ToString();
                                        }
                                }
                        }
                        if (m_ItemDict.Count >= m_nMaxBagSize)
                        {
                                return;
                        }

                        int idx = 0;
                        while (di.nOverlap > 0 && idx < m_ItemObjectlist.Count)
                        {
                                if (m_ItemDict.Count >= m_nMaxBagSize)
                                {
                                        break;
                                }
                                GameObject itemObj = m_ItemObjectlist[idx];
                                idx++;
                                if (!m_ItemDict.ContainsKey(itemObj))
                                {
                                        if (di.nOverlap <= maxOverlap)
                                        {
                                                DropObject newDi = new DropObject(di);
                                                UIUtility.SetupItemElem(itemObj, newDi);
                                                m_ItemDict.Add(itemObj, newDi);

                                                di.nOverlap = 0;
                                        }
                                        else
                                        {
                                                DropObject newDi = new DropObject(di);
                                                newDi.nOverlap = maxOverlap;
                                                UIUtility.SetupItemElem(itemObj, newDi);
                                                m_ItemDict.Add(itemObj, newDi);

                                                di.nOverlap -= maxOverlap;
                                        }
                                }
                        }
                }
        }
        public void OnClickItem(GameObject go)
        {
                if (UIManager.GetInst().IsUIVisible("UI_RaidAlchemy"))
                {
                        if (m_ItemDict.ContainsKey(go))
                        {
                                DropObject di = m_ItemDict[go];
                                if (di.nOverlap > 0)
                                {
                                        UI_RaidAlchemy uis = UIManager.GetInst().GetUIBehaviour<UI_RaidAlchemy>();
                                        if (uis.AddItem(di))
                                        {
                                                di.nOverlap--;
                                                GetText(go, "itemcount").text = di.nOverlap.ToString();
                                        }
                                }
                        }
                }
                else if (UIManager.GetInst().IsUIVisible("UI_DropBag"))
                {
                        if (m_ItemDict.ContainsKey(go))
                        {
                                DropObject di = m_ItemDict[go];
                                UI_DropBag uis = UIManager.GetInst().GetUIBehaviour<UI_DropBag>();
                                if (uis != null)
                                {
                                        uis.DropItem(di);
                                }
                                GetImage(go, "itemiconcover").enabled = false;
                                GetImage(go, "itemicon").sprite = null;
                                GetImage(go, "itemicon").enabled = false;
                                GetImage(go, "itemquality").enabled = false;
                                GetText(go, "itemcount").text = "";

                                m_ItemDict.Remove(go);
                        }
                }
                else if (UIManager.GetInst().IsUIVisible("UI_RaidCampFood"))
                {
                        if (m_ItemDict.ContainsKey(go))
                        {
                                UIManager.GetInst().GetUIBehaviour<UI_RaidCampFood>().AddFoodItem(m_ItemDict[go]);
                        }
                }
                else
                {
                        if (m_SelectItem != null)
                        {
                                GetImage(m_SelectItem, "itemiconcover").gameObject.SetActive(false);
                                GetImage(m_SelectItem, "itemiconcover").enabled = false;
                        }

                        m_SelectItem = go;
                        GetImage(m_SelectItem, "itemiconcover").gameObject.SetActive(true);
                        GetImage(m_SelectItem, "itemiconcover").enabled = true;
                        UI_RaidSkillSelect uis = UIManager.GetInst().GetUIBehaviour<UI_RaidSkillSelect>();
                        if (uis != null)
                        {
                                if (m_ItemDict.ContainsKey(go))
                                {
                                        uis.SetSelectItem(m_ItemDict[go]);
                                }
                                else
                                {
                                        uis.SetSelectItem(null);
                                }
                        }
                        else
                        {
                                ShowDescGroup(go);
                        }
                }
        }

        void ShowDescGroup(GameObject itemObj)
        {
                if (m_ItemDict.ContainsKey(itemObj))
                {
                        m_ItemDescGroup.SetActive(true);

                        DropObject di = m_ItemDict[itemObj];
                        GetText(m_ItemDescGroup, "itemname").text = di.GetName();
                        GetText(m_ItemDescGroup, "itemdesc").text = di.GetDesc();
                        GetGameObject("btnuse").SetActive(di.CanUseInRaid());
                }
                else
                {
                        m_ItemDescGroup.SetActive(false);
                }
        }

        public void RecoverItem(DropObject di)
        {
                foreach (var param  in m_ItemDict)
                {
                        if (di.id == param.Value.id)
                        {
                                param.Value.nOverlap++;
                                GetText(param.Key, "itemcount").text = param.Value.nOverlap.ToString();
                        }
                }
        }
        public void OnClickExpand()
        {
                m_ItemGroup.GetComponent<Image>().enabled = !m_ItemGroup.GetComponent<Image>().enabled;
                m_ItemGroup.GetComponent<Mask>().enabled = !m_ItemGroup.GetComponent<Mask>().enabled;
                RectTransform rt = m_ItemGroup.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, m_ToggleExpand.isOn ? 168f : 84f);
                m_ToggleExpand.GetComponent<Image>().enabled = !m_ToggleExpand.isOn;
        }
        public void SetItemDescGroupVisible(bool bVisible)
        {
                m_ItemDescGroup.SetActive(bVisible);
        }

        public void OnClickBack(GameObject go)
        {
        }

        public void OnClickUse()
        {
                if (m_SelectItem != null)
                {
                        if (m_ItemDict.ContainsKey(m_SelectItem))
                        {
                                DropObject di = m_ItemDict[m_SelectItem];
                                if (di.nType == (int)Thing_Type.ITEM && di.idCfg == GlobalParams.GetInt("camp_need_item"))
                                {
                                        NetworkManager.GetInst().SendMsgToServer(new Message.CSMsgRaidCamp());
                                }
                                else
                                {
                                        if (CanUseItem(di))
                                        {
                                                ItemManager.GetInst().UseItem(di.id, 1, RaidManager.GetInst().MainHero.ID);
                                                RaidManager.GetInst().UnitTalk(TALK_TYPE.USE_ITEM);
                                        }
                                }
                                m_ItemDescGroup.SetActive(false);
                        }
                }
        }
        bool CanUseItem(DropObject di)
        {
                //检查物品是否可用，并弹出提示（应该写在脚本里的）
                if (di.nType == (int)Thing_Type.ITEM)
                {
                        switch (di.idCfg)
                        {
                                case 117:
                                        {
                                                if (RaidTeamManager.GetInst().GetTeamBright() >= 100)
                                                {
                                                        GameUtility.ShowTipAside(LanguageManager.GetText("brightness_full_hint"));
                                                        return false;
                                                }
                                        }
                                        break;
                        }
                }
                return true;
        }


        public void RecoverSkillSelect()
        {
                foreach (GameObject itemObj in m_ItemDict.Keys)
                {
                        itemObj.GetComponent<Button>().interactable = true;
                }
        }
        public void SetupSkillSelect(Dictionary<int, int> toolDict)
        {
                foreach (var param in m_ItemDict)
                {
                        if (param.Value.nType == 3 && toolDict.ContainsKey(param.Value.idCfg))
                        {
                                param.Key.GetComponent<Button>().interactable = true;
                        }
                        else
                        {
                                param.Key.GetComponent<Button>().interactable = false;
                        }
                }
        }
        public void EnableCampMode(bool bEnable)
        {
                foreach (var param in m_ItemDict)
                {
                        if (bEnable)
                        {
                                if (param.Value.nType == (int)Thing_Type.ITEM && param.Value.GetSubType() == (int)ITEM_SUB_TYPE.FOOD)
                                {
                                        param.Key.GetComponent<Button>().interactable = true;
                                }
                                else
                                {
                                        param.Key.GetComponent<Button>().interactable = false;
                                }
                        }
                        else
                        {
                                param.Key.GetComponent<Button>().interactable = true;
                        }
                }
        }
        void Update()
        {
                if (InputManager.GetInst().GetInputUp(false))
                {
                        SetItemDescGroupVisible(false);
                }
        }
}
