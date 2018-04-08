using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
class UI_NpcTrade : UIBehaviour
{
        public GameObject m_MyBagItem;
        public GameObject m_NpcBagItem;
        public GameObject m_MySellItem;
        public GameObject m_NpcSellItem;
        
        public GameObject m_MyBag;
        public GameObject m_NpcBag;
        public GameObject m_MySellList;
        public GameObject m_NpcSellList;

        Dictionary<GameObject, DropObject> m_MyBagItemList = new Dictionary<GameObject, DropObject>();
        Dictionary<GameObject, DropObject> m_NpcBagItemList = new Dictionary<GameObject, DropObject>();
        Dictionary<GameObject, DropObject> m_MySellItemList = new Dictionary<GameObject, DropObject>();
        Dictionary<GameObject, DropObject> m_NpcSellItemList = new Dictionary<GameObject, DropObject>();

        public GameObject m_MyTypeGroup;
        public GameObject m_NpcTypeGroup;
        Dictionary<int, GameObject> m_MyTypeGroupList = new Dictionary<int, GameObject>();
        Dictionary<int, GameObject> m_NpcTypeGroupList = new Dictionary<int, GameObject>();

        NpcConfig m_NpcCfg;
        TradeConfig m_TradeCfg;

        public Image m_BalanceBar;
        public Image m_LeftWeight;
        public Image m_RightWeight;

        public Image m_NpcIcon;
        public Text m_NpcTalk;

        long m_Id;

        public long NpcId
        {
                get
                {
                        return m_Id;
                }
        }

        public GameObject m_MyBagScrollBar;
        public GameObject m_NpcBagScrollBar;
        public GameObject m_MySellScrollBar;
        public GameObject m_NpcSellScrollBar;

        GameObject m_ItemDesc;
        GameObject m_EquipDesc;

        void Awake()
        {
                m_MyBagItem.SetActive(false);
                m_NpcBagItem.SetActive(false);
                m_MySellItem.SetActive(false);
                m_NpcSellItem.SetActive(false);

                m_BalanceBar.rectTransform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
                m_LeftWeight.rectTransform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
                m_RightWeight.rectTransform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));

                m_ItemDesc = GetGameObject("itemdescgroup");
                m_EquipDesc = GetGameObject("equipdescgroup");
        }

        public void SetTrade(long id, string sell_list, NpcConfig npcCfg)
        {
                StartCoroutine(ProcessSetTrade(id, sell_list, npcCfg));
        }
        IEnumerator ProcessSetTrade(long id, string sell_list, NpcConfig npcCfg)
        {
                yield return null;
                if (GameStateManager.GetInst().GameState == GAMESTATE.HOME)
                {
                        UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", false);
                }


                m_Id = id;
                m_NpcCfg = npcCfg;
                
                foreach (GameObject obj in m_MyBagItemList.Keys)
                {
                        GameObject.Destroy(obj);
                }
                m_MyBagItemList.Clear();
                foreach (GameObject obj in m_NpcBagItemList.Keys)
                {
                        GameObject.Destroy(obj);
                } 
                m_NpcBagItemList.Clear();
                foreach (GameObject obj in m_MySellItemList.Keys)
                {
                        GameObject.Destroy(obj);
                } 
                m_MySellItemList.Clear();
                foreach (GameObject obj in m_NpcSellItemList.Keys)
                {
                        GameObject.Destroy(obj);
                } 
                m_NpcSellItemList.Clear();

                int npcItemCount = 0;
                m_TradeCfg = NpcManager.GetInst().GetTradeCfg(npcCfg.type_id);
                if (m_TradeCfg != null)
                {
                        npcItemCount = m_TradeCfg.number_limited;
                        string[] trade_types = m_TradeCfg.own_trade_type.Split(';');
                        Dictionary<int, List<int>> tradeTypeList = new Dictionary<int, List<int>>();
                        foreach (string tmp in trade_types)
                        {
                                if (string.IsNullOrEmpty(tmp))
                                        continue;
                                string[] tmps = tmp.Split(',');
                                if (tmps.Length == 2)
                                {
                                        int mainType = int.Parse(tmps[0]);
                                        int subType = int.Parse(tmps[1]);
                                        if (!tradeTypeList.ContainsKey(mainType))
                                        {
                                                tradeTypeList.Add(mainType, new List<int>());
                                        }
                                        tradeTypeList[mainType].Add(subType);
                                }
                        }

                        Dictionary<long, DropObject> itemDict= ItemManager.GetInst().GetObjectDict(m_TradeCfg.bag_type);
                        foreach (DropObject dObj in itemDict.Values)
                        {
                                if (tradeTypeList.ContainsKey(dObj.nType))
                                {
                                        if (tradeTypeList[dObj.nType].Contains(dObj.GetSubType()))
                                        {
                                                AddDropObjToBag(new DropObject(dObj), "MyBag", m_MyBag, ref m_MyBagItemList);
                                        }
                                }
                        }
                }

                if (npcItemCount == 0)
                {
                        npcItemCount = PlayerController.GetInst().GetPropertyInt("npc_item_count");
                }
                for (int i = 0; i < npcItemCount; i++)
                {
                        GameObject itemObj = CloneElement(m_NpcBagItem, "NpcBag_" + i);
                        itemObj.transform.SetParent(GetGameObject(m_NpcTypeGroup,"itemgroup").transform);
                        GetText(itemObj, "count").text = "";                        
                        GetText(itemObj, "value").text = "";
                        GetGameObject(itemObj, "valueicon").SetActive(false);
                }
                int idx = 0;
                if (!string.IsNullOrEmpty(sell_list))
                {
                        string[] infos = sell_list.Split('|');
                        foreach (string itemstr in infos)
                        {
                                if (string.IsNullOrEmpty(itemstr))
                                        continue;

                                string[] tmps = itemstr.Split('&');
                                if (tmps.Length >= 3)
                                {
                                        DropObject dObj = new DropObject();
                                        dObj.nType = int.Parse(tmps[0]);
                                        dObj.idCfg = int.Parse(tmps[1]);
                                        dObj.nOverlap = int.Parse(tmps[2]);

                                        if (AddDropObjToBag(dObj, "NpcBag", m_NpcBag, ref m_NpcBagItemList, "NpcBag_" + idx))
                                        {
                                                idx++;
                                        }
                                }
                        }
                }
                string npcicon = ModelResourceManager.GetInst().GetIconRes(npcCfg.model);
                if (!string.IsNullOrEmpty(npcicon))
                {
                        ResourceManager.GetInst().LoadIconSpriteSyn(npcicon, m_NpcIcon.transform);
                }
                m_NpcTalk.text = LanguageManager.GetText(npcCfg.desc);

                GetText("mytotalvalue").text = "";
                GetText("npctotalvalue").text = "";

                UpdateTotalValue();

                HomeManager.GetInst().ChangeNpcNewIcon(NpcId, m_NpcCfg);

//                 NpcInfo ni = NpcManager.GetInst().GetNpc(id);
//                 if (ni.id != 0)
//                 {
//                         GetText("time1").gameObject.SetActive(true);
//                         GetText("time2").gameObject.SetActive(true);
//                         GetText("time2").text = ni.restTime + "天";
//                 }
//                 else
//                 {
//                         GetText("time1").gameObject.SetActive(false);
//                         GetText("time2").gameObject.SetActive(false);
//                 }

        }

        bool AddDropObjToBag(DropObject dObj, string targetPrefix, GameObject parent, ref Dictionary<GameObject, DropObject> dict, string objname = "")
        {
                foreach (KeyValuePair<GameObject, DropObject> param in dict)
                {
                        if (param.Value != null && param.Value.TempId == dObj.TempId)
                        {
                                param.Value.nOverlap += dObj.nOverlap;
                                GetText(param.Key, "count").text = param.Value.nOverlap.ToString();
                                return false;
                        }
                }

                if (targetPrefix == "MyBag" )
                {
                        GameObject subTypeGroup = m_MyTypeGroup;
                        int type = dObj.nType * 100 + dObj.GetSubType();

                        parent = subTypeGroup.transform.Find("itemgroup").gameObject;
                        GetText(subTypeGroup, "subtype").text = dObj.GetSubTypeName();
                }
                else if (targetPrefix == "NpcBag")
                {
                        GameObject subTypeGroup = m_NpcTypeGroup;

                        parent = subTypeGroup.transform.Find("itemgroup").gameObject;
                        GetText(subTypeGroup, "subtype").text = dObj.GetSubTypeName();
                }

                GameObject itemObj = null;
                switch (targetPrefix)
                {
                        case "MyBag":
                                itemObj = CloneElement(m_MyBagItem, targetPrefix + CommonString.underscoreStr + dObj.id);
                                itemObj.transform.SetParent(parent.transform);
                                break;
                        case "NpcBag":
                                if (!string.IsNullOrEmpty(objname))
                                {
                                        itemObj = GetGameObject(parent, objname);
                                }
                                else
                                {
                                        if (targetPrefix == "NpcBag")
                                        {
                                                foreach (GameObject obj in dict.Keys)
                                                {
                                                        if (dict[obj] == null)
                                                        {
                                                                itemObj = obj;
                                                                break;
                                                        }
                                                }
                                        }
                                }
                                break;
                        case"MySell":
                                itemObj = CloneElement(m_MySellItem, targetPrefix + CommonString.underscoreStr + dObj.id);
                                itemObj.transform.SetParent(parent.transform);
                                break;
                        case "NpcSell":
                                itemObj = CloneElement(m_NpcSellItem, targetPrefix + CommonString.underscoreStr + dObj.id);
                                itemObj.transform.SetParent(parent.transform);
                                break;
                }
                if (itemObj != null)
                {
                        ResourceManager.GetInst().LoadIconSpriteSyn(dObj.GetIconName(), GetImage(itemObj, "icon").transform);
                        if (dict.ContainsKey(itemObj))
                        {
                                dict[itemObj] = dObj;
                        }
                        else
                        {
                                dict.Add(itemObj, dObj);
                        }

                        GetGameObject(itemObj, "valueicon").SetActive(true);
                        GetText(itemObj, "count").text = dObj.nOverlap.ToString();
                        GetText(itemObj, "value").text = GetValue(dObj, targetPrefix.Contains("Npc")).ToString();
                        
                }
                return true;
        }

        int GetValue(DropObject dObj, bool bNpc)
        {
                if (m_TradeCfg != null)
                {
                        string spec_value_per = "";
                        int normal_value_per = 0;
                        if (bNpc)
                        {
                                spec_value_per = m_TradeCfg.buy_special_value_per;
                                normal_value_per = m_TradeCfg.buy_value_per - PlayerController.GetInst().GetPropertyInt("npc_lower_price");
                        }
                        else
                        {
                                spec_value_per = m_TradeCfg.sell_special_value_per;
                                normal_value_per = m_TradeCfg.sell_value_per;
                        }

                        string tmp = dObj.nType + CommonString.commaStr + dObj.GetSubType().ToString() + CommonString.commaStr;
                        string[] infos = spec_value_per.Split(';');
                        foreach (string info in infos)
                        {
                                if (info.Contains(tmp))
                                {
                                        int percent = 0;
                                        int.TryParse(info.Replace(tmp, ""), out percent);
                                        if (percent > 0)
                                        {
                                                if (bNpc)
                                                {
                                                        percent -= PlayerController.GetInst().GetPropertyInt("npc_lower_price");
                                                }
                                                return (int)(dObj.GetValue() * percent / 100f);
                                        }
                                }
                        }

                        return (int)(dObj.GetValue() * normal_value_per / 100f);
                }
                return 0;
        }

        void CheckSubTypeGroup(bool isMyBag, int type)
        {
                if (isMyBag && m_MyTypeGroupList.ContainsKey(type))
                {
                        Transform trans = m_MyTypeGroupList[type].transform.Find("itemgroup");
                        if (trans != null)
                        {
                                if (trans.childCount <= 2)
                                {
                                        GameObject.Destroy(m_MyTypeGroupList[type]);
                                        m_MyTypeGroupList.Remove(type);
                                }
                        }
                }
                if (!isMyBag && m_NpcTypeGroupList.ContainsKey(type))
                {
                        Transform trans = m_NpcTypeGroupList[type].transform.Find("itemgroup");
                        if (trans != null)
                        {
                                if (trans.childCount <= 2)
                                {
                                        GameObject.Destroy(m_NpcTypeGroupList[type]);
                                        m_NpcTypeGroupList.Remove(type);
                                }
                                //Debuger.Log(trans.name + " " + trans.childCount);
                        }
                }
        }

        bool UpdateDropObjCount(GameObject go, DropObject dObj, int count = 1)
        {
                dObj.nOverlap -= count;
                GetText(go, "count").text = dObj.nOverlap.ToString();

                return dObj.nOverlap <= 0;
        }

        float m_fTargetAngle = 0f;
        float m_fFromAngle = 0f;
        void UpdateTotalValue()
        {
                int myValue = 0;
                int npcValue = 0;
                foreach (DropObject dObj in m_MySellItemList.Values)
                {
                        myValue += GetValue(dObj, false) * dObj.nOverlap;
                }
                foreach (DropObject dObj in m_NpcSellItemList.Values)
                {
                        npcValue += GetValue(dObj, true) * dObj.nOverlap;
                }
                GetText("mytotalvalue").text = myValue.ToString();
                GetText("npctotalvalue").text = npcValue.ToString();
                if (myValue <= 0 || npcValue <= 0)
                {
                        GetButton("btnconfirm").interactable = false;
                }
                else
                {
                        GetButton("btnconfirm").interactable = myValue >= npcValue;
                }
                if (myValue >= npcValue)
                {
                        GetText("mytotalvalue").color = new Color(255f / 255f, 236f / 255f, 218f / 255f);
                }
                else
                {
                        GetText("mytotalvalue").color = Color.red;
                }

                float delta = (myValue - npcValue) / 50f;
                m_fTargetAngle = Mathf.Clamp(delta, -50f, 50f);
                if (m_fTargetAngle < 0f)
                {
                        m_fTargetAngle += 360f;
                }
                m_fFromAngle = m_BalanceBar.rectTransform.rotation.eulerAngles.z;

                StartCoroutine(RotateBalance());
        }

        IEnumerator RotateBalance()
        {
                float time = 0f;
                
                while (time < 0.3f)
                {
                        m_BalanceBar.rectTransform.rotation = Quaternion.Euler(new Vector3(0f, 0f, Mathf.LerpAngle(m_fFromAngle, m_fTargetAngle, time / 0.3f)));
                        m_LeftWeight.rectTransform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
                        m_RightWeight.rectTransform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
                        time += Time.deltaTime;
                        yield return null;
                }
                m_BalanceBar.rectTransform.rotation = Quaternion.Euler(new Vector3(0f, 0f, m_fTargetAngle));
                m_LeftWeight.rectTransform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
                m_RightWeight.rectTransform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
        }

        void MoveItem(ref Dictionary<GameObject, DropObject> srcDict, GameObject srcItemObj, string targetPrefix, ref Dictionary<GameObject, DropObject> targetDict, GameObject targetParent)
        {
                if (srcDict.ContainsKey(srcItemObj))
                {
                        DropObject srcDi = srcDict[srcItemObj];
                        if (srcDi == null)
                                return;

                        if (srcDi.nOverlap <= 0)
                        {
                                return;
                        }
                        
                        DropObject targetDi = new DropObject();
                        targetDi.idCfg = srcDict[srcItemObj].idCfg;
                        if (srcDi.nType == (int)Thing_Type.ITEM && srcDi.GetSubType() == 3)
                        {
                                targetDi.nOverlap = srcDi.nOverlap >= 10 ? 10 : srcDi.nOverlap;
                        }
                        else
                        {
                                targetDi.nOverlap = 1;
                        }
                        targetDi.nType = srcDict[srcItemObj].nType;

                        AddDropObjToBag(targetDi, targetPrefix, targetParent, ref targetDict);
                        

                        srcDi.nOverlap -= targetDi.nOverlap;
                        GetText(srcItemObj, "count").text = srcDi.nOverlap.ToString();

                        if (srcDi.nOverlap <= 0)
                        {
                                if (!srcItemObj.name.Contains("NpcBag_"))
                                {
                                        srcDict.Remove(srcItemObj);
                                        GameObject.Destroy(srcItemObj);                                        
                                }
                                else
                                {
                                        GetImage(srcItemObj, "icon").enabled = false;
                                        GetGameObject(srcItemObj, "valueicon").SetActive(false);
                                        GetText(srcItemObj, "count").text = "";
                                        GetText(srcItemObj, "value").text = "";
                                        srcDict[srcItemObj] = null;
                                }
                        }
                }
        }

        public void OnClickItem(GameObject go)
        {
                m_ItemDesc.SetActive(false);
                if (go.name.Contains("MyBag_"))
                {
                        if (m_MyBagItemList.ContainsKey(go))
                        {
                                ShowItemTips(m_MyBagItemList[go]);
                        }
                        MoveItem(ref m_MyBagItemList, go, "MySell", ref m_MySellItemList, m_MySellList);
                }
                else if (go.name.Contains("NpcBag_"))
                {
                        if (m_NpcBagItemList.ContainsKey(go))
                        {
                                ShowItemTips(m_NpcBagItemList[go]);
                        }
                        MoveItem(ref m_NpcBagItemList, go, "NpcSell", ref m_NpcSellItemList, m_NpcSellList);
                }
                else if (go.name.Contains("MySell_"))
                {
                        MoveItem(ref m_MySellItemList, go, "MyBag", ref m_MyBagItemList, m_MyBag);
                }
                else if (go.name.Contains("NpcSell_"))
                {
                        MoveItem(ref m_NpcSellItemList, go, "NpcBag", ref m_NpcBagItemList, m_NpcBag);
                }
                UpdateTotalValue();
        }

        public void OnClickConform()
        {
                if (m_TradeCfg.bag_type == (int)ItemType.BAG_PLACE.RAID && CheckRaidBag() == false)
                {
                        return;
                }
                string selllist = "";
                foreach (DropObject dObj in m_MySellItemList.Values)
                {
                        selllist += dObj.ToInfoStr();
                        selllist += CommonString.pipeStr;
                }
                string buylist = "";
                foreach (DropObject dObj in m_NpcSellItemList.Values)
                {
                        buylist += dObj.ToInfoStr();
                        buylist += CommonString.pipeStr;
                }
                NpcManager.GetInst().SendTrade(m_Id, selllist, buylist);
        }

        bool CheckRaidBag()
        {
                int maxBagSize = PlayerController.GetInst().GetPropertyInt("maze_capacity");
                Dictionary<long, DropObject> dict = new Dictionary<long,DropObject>();
                foreach (DropObject dObj in ItemManager.GetInst().GetObjectDict((int)ItemType.BAG_PLACE.RAID).Values)
                {
                        dict.Add(dObj.id, new DropObject(dObj));
                }

                List<DropObject> mySellList = new List<DropObject>();
                foreach (DropObject dObj in m_MySellItemList.Values)
                {
                        mySellList.Add(new DropObject(dObj));
                }
                List<DropObject> npcSellList = new List<DropObject>();
                foreach (DropObject dObj in m_NpcSellItemList.Values)
                {
                        npcSellList.Add(new DropObject(dObj));
                } 
                

                List<long> toRemove = new List<long>();
                foreach (DropObject dObj in mySellList)
                {
                        foreach (DropObject di in dict.Values)
                        {
                                if (di.TempId == dObj.TempId && di.nOverlap > 0 && dObj.nOverlap > 0)
                                {
                                        di.nOverlap -= dObj.nOverlap;
                                        if (di.nOverlap < 0)
                                        {
                                                dObj.nOverlap = 0 - di.nOverlap;
                                                di.nOverlap = 0;
                                                toRemove.Add(di.id);
                                                Debuger.Log(di.idCfg + " count=" + di.nOverlap);
                                        }
                                        else
                                        {
                                                dObj.nOverlap = 0;
                                                break;
                                        }
                                }
                        }
                }
                foreach (long id in toRemove)
                {
                        dict.Remove(id);
                }

                int tmpid = 1;
                foreach (DropObject dObj in npcSellList)
                {
                        foreach (DropObject di in dict.Values)
                        {
                                if (dObj.nOverlap > 0)
                                {
                                        if (di.TempId == dObj.TempId)
                                        {
                                                if (di.nOverlap + dObj.nOverlap <= di.GetMaxOverlap())
                                                {
                                                        di.nOverlap += dObj.nOverlap;
                                                        dObj.nOverlap = 0;
                                                        break;
                                                }
                                                else
                                                {
                                                        dObj.nOverlap = di.nOverlap + dObj.nOverlap - di.GetMaxOverlap();
                                                        di.nOverlap = di.GetMaxOverlap();
                                                }
                                        }
                                }
                        }

                        if (dObj.nOverlap > 0)
                        {
                                dict.Add(tmpid, dObj);
                                tmpid++;
                                dObj.id = tmpid;
                        }
                }
                if (dict.Count > maxBagSize)
                {
                        Debuger.Log("CheckBagSize " + dict.Count + " bagMaxSize=" + maxBagSize);
                        return false;
                }
                return true;
        }

        public override void OnClickClose(GameObject go)
        {
            base.OnClickClose(go);
            if (GameStateManager.GetInst().GameState == GAMESTATE.HOME)
            {
                UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", true);
            }
        }
        public void ShowItemTips(DropObject di)
        {
                if (di == null)
                        return;

                if (di.nType == (int)Thing_Type.ITEM)
                {
                        m_ItemDesc.SetActive(true);
                        m_EquipDesc.SetActive(false);
                        GetText(m_ItemDesc, "itemname").text = di.GetName();
                        GetText(m_ItemDesc, "itemdesc").text = di.GetDesc();
                }
                else if (di.nType == (int)Thing_Type.EQUIP)
                {
                        m_ItemDesc.SetActive(false);
                        ShowBagEquipTip(di.idCfg);
                }
        }
        void ShowBagEquipTip(int idCfg)
        {
                m_EquipDesc.SetActive(true);
                ShowEquipTip(idCfg, m_EquipDesc.transform);
//                 EquipHoldConfig ec = EquipManager.GetInst().GetEquipCfg(equip.equip_configid);
//                 int part = ec.place;
//                 Equip m_equip = m_select_hero.GetMyEquipPart((ItemType.EQUIP_PART)part);  //穿戴的装备
//                 if (m_equip != null)
//                 {
//                         (BodyEquipTip as RectTransform).SetAnchoredPositionX(refer_pos0_x);
//                         (BagEquipTip as RectTransform).SetAnchoredPositionX((BodyEquipTip as RectTransform).sizeDelta.x + equip_tip_offset_x + (BodyEquipTip as RectTransform).anchoredPosition.x);
//                         BodyEquipTip.SetActive(true);
//                         ShowEquipTip(m_equip, BodyEquipTip);
//                 }
//                 else
//                 {
//                         (BagEquipTip as RectTransform).SetAnchoredPositionX(refer_pos1_x);
//                         BodyEquipTip.SetActive(false);
//                 }

        }
        void ShowEquipTip(int idCfg, Transform root)
        {
                EquipHoldConfig ec = EquipManager.GetInst().GetEquipCfg(idCfg);
                if (ec != null)
                {
                        Image quality_image = FindComponent<Image>(root, "quality");
                        Image icon_image = FindComponent<Image>(root, "icon");
                        Text name = FindComponent<Text>(root, "name");
                        int quality = ec.quality;
                        if (quality > 1)
                        {
                                ResourceManager.GetInst().LoadIconSpriteSyn("Bg#quality" + quality, root.Find("quality"));
                        }
                        else
                        {
                                quality_image.enabled = false;
                        }
                        ResourceManager.GetInst().LoadIconSpriteSyn(ec.icon, icon_image.transform);
                        name.text = LanguageManager.GetText(ec.name);
                        FindComponent<Text>(root, "part").text = ItemType.part_des[ec.place - (int)ItemType.EQUIP_PART.MAIN_HAND];
                        FindComponent<Text>(root, "is_two_hands").text = ec.is_two_hands > 0 ? "是" : "否";
                        FindComponent<Text>(root, "score").text = ec.score.ToString();
                        FindComponent<Text>(root, "des").text = LanguageManager.GetText(ec.desc);

                        //需求等级
                        int need_level = ec.need_level;
                        Text level_text = FindComponent<Text>(root, "level_text");
                        Text need_level_text = FindComponent<Text>(root, "need_level");
                        level_text.color = Color.black;
                        need_level_text.color = Color.black;
                        need_level_text.text = need_level.ToString();

                        //需求职业
                        Transform need_career = root.Find("need_career");
                        SetChildActive(need_career, false);
                        GameObject original_career = need_career.GetChild(0).gameObject;
                        string[] career = ec.need_career.Split(',' );
                        for (int i = 0; i < career.Length; i++)
                        {
                                int career_id = int.Parse(career[i]);
                                Transform temp = null;
                                if (i < need_career.childCount)
                                {
                                        temp = need_career.GetChild(i);
                                }
                                else
                                {
                                        temp = CloneElement(original_career).transform;
                                }
                                if (career_id == -1)
                                {
                                        ResourceManager.GetInst().LoadIconSpriteSyn("Career#quanbu", temp);
                                }
                                else
                                {
                                        string career_url = CharacterManager.GetInst().GetCareerIcon(career_id);
                                        ResourceManager.GetInst().LoadIconSpriteSyn(career_url, temp);
                                }
                                temp.SetActive(true);
                        }
                        FindComponent<Text>(root, "career_text").color = Color.red;

                        Transform attr_group = root.Find("attradd");
                        GameObject original_attr = attr_group.GetChild(0).gameObject;
                        SetChildActive(attr_group, false);
                        string[] attr_value = ec.attributes.Split(';');
                        for (int i = 0; i < attr_value.Length; i++)
                        {
                                Transform temp = null;
                                if (i < attr_group.childCount)
                                {
                                        temp = attr_group.GetChild(i);
                                }
                                else
                                {
                                        temp = CloneElement(original_attr).transform;
                                }
                                string[] attr = attr_value[i].Split(',');
                                FindComponent<Text>(temp, "name").text = CharacterManager.GetInst().GetPropertyMark(int.Parse(attr[0]));
                                string attr_name = CharacterManager.GetInst().GetPropertyName(int.Parse(attr[0]));
                                if (attr_name.Contains("per"))
                                {
                                        FindComponent<Text>(temp, "value").text = "+" + attr[1] + "%";
                                }
                                else
                                {
                                        FindComponent<Text>(temp, "value").text = "+" + attr[1];
                                }
                                temp.SetActive(true);
                        }
                }
        }

}
