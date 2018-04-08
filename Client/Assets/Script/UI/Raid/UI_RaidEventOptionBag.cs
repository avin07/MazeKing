using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UI_RaidEventOptionBag : UIBehaviour 
{
        int MAX_COL_NUM = 6;
        int MAX_ROW_NUM = 4;

        public GameObject m_Item0;
        public List<GameObject> m_ItemObjectlist = new List<GameObject>();

        Dictionary<int, DropObject> m_BagOriDict = new Dictionary<int, DropObject>();
        Dictionary<GameObject, DropObject> m_ItemDict = new Dictionary<GameObject, DropObject>();
        DropObject m_SelectObject;
        RaidNodeBehav m_Node;
        int m_nOptionId;

        void Awake()
        {
                m_Item0.SetActive(false);
                //m_ItemObjectlist.Add(m_Item0);
        }

        GameObject GetItem(int idx)
        {
                GameObject obj = null;
                if (idx < m_ItemObjectlist.Count)
                {
                        obj = m_ItemObjectlist[idx];
                }
                else
                {
                        obj = CloneElement(m_Item0, "item" + idx);
                        m_ItemObjectlist.Add(obj);
                }
                return obj;
        }

        public void OnClickBack(GameObject go)
        {
                UIManager.GetInst().CloseUI(this.name);
        }
        public void OnClickConfirm(GameObject go)
        {
                if (m_SelectObject != null)
                {
                        if (m_nOptionId > 0)
                        {
                                string optionData = m_SelectObject != null ? (m_SelectObject.idCfg + CommonString.ampersandStr + m_SelectObject.nType) : CommonString.zeroStr;
                                RaidManager.GetInst().ConfirmOption(m_Node, m_nOptionId, optionData);
                        }
                        else
                        {
                                UI_RaidSkillSelect uis = UIManager.GetInst().GetUIBehaviour<UI_RaidSkillSelect>();
                                uis.SetSelectItem(m_SelectObject);
                        }
                }
                OnClickClose(null);
        }

        void SetObject(GameObject itemObj, DropObject di)
        {
                string iconname = di.GetIconName();
                if (iconname != "")
                {
                        ResourceManager.GetInst().LoadIconSpriteSyn(iconname, GetImage(itemObj, "itemicon").transform);
                        GetImage(itemObj, "itemiconcover").enabled = true;
                        GetText(itemObj, "itemcount").text = di.nOverlap.ToString();
                        EventTriggerListener.Get(itemObj).onClick = OnClickItem;
                        EventTriggerListener.Get(itemObj).SetTag(di);
                }
        }

        public void SetupToolList(List<int> toollist, RaidNodeBehav node)
        {
                m_Node = node;
                m_nOptionId = 0;
                m_SelectObject = null;
                Dictionary<long, DropObject> dict = ItemManager.GetInst().GetObjectDict((int)ItemType.BAG_PLACE.RAID);
                int idx = 0;
                foreach (DropObject di in dict.Values)
                {
                        if (di.nType == 3 && toollist.Contains(di.idCfg))
                        {
                                bool bFound = false;
                                foreach (GameObject itemObj in m_ItemObjectlist)
                                {
                                        DropObject tmp = (DropObject)EventTriggerListener.Get(itemObj).GetTag();
                                        if (tmp.idCfg == di.idCfg)
                                        {
                                                tmp.nOverlap += di.nOverlap;
                                                GetText(itemObj, "itemcount").text = tmp.nOverlap.ToString();
                                                bFound = true;
                                                break;
                                        }
                                }

                                if (!bFound)
                                {
                                        SetObject(GetItem(idx), di);
                                        idx++;
                                }
                        }
                }
                if (idx > 0)
                {
                        GetGameObject("noitemtips").SetActive(false);
                }
        }

        public void Setup(List<int> objectType, RaidNodeBehav node, int optionId = 0)
        {
                if (objectType == null || objectType.Count < 2)
                        return;
                m_Node = node;
                m_nOptionId = optionId;
                m_SelectObject = null;
                Dictionary<long, DropObject> dict = ItemManager.GetInst().GetObjectDict((int)ItemType.BAG_PLACE.RAID);
                int idx = 0;
                foreach (DropObject di in dict.Values)
                {
                        if (di.nType == objectType[0])
                        {
                                //道具要检测小类
                                if (di.nType == (int)Thing_Type.ITEM && di.GetSubType() != objectType[1])
                                        continue;

                                GameObject itemObj = GetItem(idx);
                                SetObject(itemObj, di);
                                idx++;
                        }
                }
        }
        public void OnClickItem(GameObject go, PointerEventData data)
        {
                m_SelectObject = (DropObject)EventTriggerListener.Get(go).GetTag();
        }
}
