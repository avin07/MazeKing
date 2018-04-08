using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Message;

public class UI_DropBag : UIBehaviour
{
        public GameObject m_Item0;
        Dictionary<string, DropObject> m_DropList = new Dictionary<string, DropObject>();
        List<GameObject> m_ItemObjlist = new List<GameObject>();
        public void OnClickTakeAll(GameObject go)
        {
                TakeAll();
        }

        void Awake()
        {
                m_ItemObjlist.Add(m_Item0);
        }
        public override void OnClose(float time)
        {
                base.OnClose(time);
                NetworkManager.GetInst().WakeUp();//副本掉落时挂起了，这里唤醒
                RaidManager.GetInst().IsPause = false;
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
        public void SetItems(string info)
        {
                m_DropList.Clear();

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
                                di.nOverlap =int.Parse(tmps[2]);
                                GameObject itemObj = GetItemObj(idx);

                                UIUtility.SetupItemElem(itemObj, di);
                                m_DropList.Add(itemObj.name, di);
                                
                                idx++;
                        }
                }
        }

        public void OnClickItem(GameObject go)
        {
                if (m_DropList.ContainsKey(go.name))
                {
                        UI_RaidBag uis = UIManager.GetInst().GetUIBehaviour<UI_RaidBag>();
                        if (uis != null)
                        {
                                DropObject di = m_DropList[go.name];
                                uis.AddDropItem(ref di);

                                if (di.nOverlap <= 0)
                                {
                                        UIUtility.SetupItemElem(go, null);
                                        m_DropList.Remove(go.name);
                                }
                                else
                                {
                                        GetText(go, "itemcount").text = di.nOverlap.ToString();
                                }
                        }
                }
        }
        public void DropItem(DropObject di)
        {
                GameObject itemObj = null;
                for (int i = 0; i < m_ItemObjlist.Count; i++)
                {
                        if (!m_DropList.ContainsKey(m_ItemObjlist[i].name))
                        {
                                itemObj = GetItemObj(i);
                        }
                }
                if (itemObj == null)
                {
                        itemObj = GetItemObj(m_DropList.Count);
                }
                UIUtility.SetupItemElem(itemObj, di);
                m_DropList.Add(itemObj.name, di);                
        }

        public void TakeAll()
        {
                foreach (GameObject obj in m_ItemObjlist)
                {
                        OnClickItem(obj);
                }
                if (m_DropList.Count <= 0)
                {
                        OnClickClose(null);
                }
        }
        
        public void InitBagDict(List<DropObject> itemDict, Dictionary<int, DropObject> bagDict )
        {
                foreach (DropObject di in itemDict)
                {                        
                        if (!bagDict.ContainsKey(di.TempId))
                        {
                                bagDict.Add(di.TempId, new DropObject(di));
                        }
                        else
                        {
                                bagDict[di.TempId].nOverlap += di.nOverlap;
                        }
                }
        }
        public override void OnClickClose(GameObject go)
        {
                base.OnClickClose(go);
                UI_RaidBag raidbag = UIManager.GetInst().GetUIBehaviour<UI_RaidBag>();

                Dictionary<int, DropObject> oriBagDict = new Dictionary<int, DropObject>();
                Dictionary<int, DropObject> nowBagDict = new Dictionary<int, DropObject>();

                InitBagDict(ItemManager.GetInst().GetObjectList((int)ItemType.BAG_PLACE.RAID), oriBagDict);
                InitBagDict(raidbag.GetItemList(), nowBagDict);

                string dropstr = "";
                string addstr = "";

                foreach (int key in nowBagDict.Keys)
                {
                        DropObject di = nowBagDict[key];
                        //查找现在包里的东西，如果原来有，则比较数量
                        if (oriBagDict.ContainsKey(key))
                        {
                                int delta = oriBagDict[key].nOverlap - di.nOverlap;

                                if (delta > 0)
                                {//原来数量大的，则加到丢弃列表里。
                                        dropstr += di.nType + "&" + di.idCfg + "&" + delta + "|";
                                }
                                else if (delta < 0)
                                {//原来数量小的，加到拾取列表
                                        addstr += di.nType + "&" + di.idCfg + "&" + Mathf.Abs(delta) + "|";
                                }
                        }
                        else
                        {
                                addstr += di.nType + "&" + di.idCfg + "&" + di.nOverlap + "|";
                        }
                }

                //查找原来包里有，现在没有的东西。加到丢弃列表
                foreach (int key in oriBagDict.Keys)
                {
                        DropObject di = oriBagDict[key];
                        if (!nowBagDict.ContainsKey(key))
                        {
                                dropstr += di.nType + "&" + di.idCfg + "&" + di.nOverlap + "|";
                        }
                }
                RaidManager.GetInst().SendAwardDrop(dropstr);
                RaidManager.GetInst().SendAwardAdd(addstr);
                RaidManager.GetInst().UnitTalk(TALK_TYPE.GET_DROP);

        }
}