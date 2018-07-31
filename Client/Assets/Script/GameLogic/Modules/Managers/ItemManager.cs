using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Message;

class ItemManager : SingletonObject<ItemManager>
{


        Dictionary<int, ItemConfig> m_ItemCfgDict = new Dictionary<int, ItemConfig>();
        Dictionary<int, Dictionary<long, DropObject>> m_ItemDict = new Dictionary<int, Dictionary<long, DropObject>>();
        public void Init()
        {
                ConfigHoldUtility<ItemConfig>.LoadXml("Config/item", m_ItemCfgDict);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgItem), OnAddItem);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgItemUpdate), OnItemUpdate);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgItemDel), OnItemDel);

                for (int place = 0; place < (int)ItemType.BAG_PLACE.MAX; place++)
                {
                        m_ItemDict.Add(place, new Dictionary<long, DropObject>());
                }
        }


        public void UseItem(long id, int num, long idTarget = 0)
        {
                CSMsgItemUse msg = new CSMsgItemUse();
                msg.idItem = id;
                msg.nCount = num;
                msg.idTarget = idTarget;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }


        public ItemConfig GetItemCfg(int id)
        {
                if (m_ItemCfgDict.ContainsKey(id))
                {
                        return m_ItemCfgDict[id];
                }
                else
                {
                        singleton.GetInst().ShowMessage(ErrorOwner.designer, "物品表中没有id为" + id + "的物品");
                }
                return null;
        }

        public string GetItemIconUrl(int id)
        {
                if (m_ItemCfgDict.ContainsKey(id))
                {
                        if (!string.IsNullOrEmpty(m_ItemCfgDict[id].icon))
                        {
                                return m_ItemCfgDict[id].icon;
                        }
                }
                return "";
        }

        void OnAddItem(object sender, SCNetMsgEventArgs e)
        {
                SCMsgItem msg = e.mNetMsg as SCMsgItem;
                DropObject item = new DropObject(msg);

                if (m_ItemDict.ContainsKey(msg.nPlace))
                {
                        if (!m_ItemDict[msg.nPlace].ContainsKey(item.id))
                        {
                                m_ItemDict[msg.nPlace].Add(item.id, item);
                        }
                }
                CommonDataManager.GetInst().UpdateBags(msg.nPlace);

                if (msg.nPlace == (int)ItemType.BAG_PLACE.MAIN)
                {
                        if (GameStateManager.GetInst().GameState > GAMESTATE.OTHER)
                        {
                                
                                AddToNewItemList(item.id);
                                
                        }

                        UIRefreshManager.GetInst().AddDropItem(item.GetIconName(), item.GetName(), msg.nOverlap);
                }                
        }

        void OnItemUpdate(object sender, SCNetMsgEventArgs e)
        {
                SCMsgItemUpdate msg = e.mNetMsg as SCMsgItemUpdate;

                if (m_ItemDict.ContainsKey(msg.nPlace))
                {
                        if (m_ItemDict[msg.nPlace].ContainsKey(msg.id))
                        {
                                if (msg.nOverlap <= 0)
                                {
                                        m_ItemDict[msg.nPlace].Remove(msg.id);
                                }
                                else
                                {
                                        DropObject item = m_ItemDict[msg.nPlace][msg.id];

                                        if (item.nOverlap < msg.nOverlap) //获得
                                        {
                                                if (msg.nPlace == (int)ItemType.BAG_PLACE.MAIN)
                                                {
                                                        //飘包效果
                                                        string url = item.GetIconName();
                                                        string name = LanguageManager.GetText(ItemManager.GetInst().GetItemCfg(item.idCfg).name);
                                                        UIRefreshManager.GetInst().AddDropItem(url, name, msg.nOverlap - item.nOverlap);
                                                }
                                        }
                                        item.nOverlap = msg.nOverlap;                                        
                                }
                        }
                }
                CommonDataManager.GetInst().UpdateBags(msg.nPlace);
        }

        void OnItemDel(object sender, SCNetMsgEventArgs e)
        {
                SCMsgItemDel msg = e.mNetMsg as SCMsgItemDel;
                if (m_ItemDict.ContainsKey(msg.nPlace))
                {
                        if (m_ItemDict[msg.nPlace].ContainsKey(msg.id))
                        {
                                m_ItemDict[msg.nPlace].Remove(msg.id);
                        }
                }

                CommonDataManager.GetInst().UpdateBags(msg.nPlace);
        }

        int CompareRaidBag(DropObject x, DropObject y)
        {
                if (x.Priority != y.Priority)
                {
                        return x.Priority.CompareTo(y.Priority);
                }
                return x.TempId.CompareTo(y.TempId);
        }
        public List<DropObject> GetObjectList(int nPlace)
        {
                List<DropObject> list = new List<DropObject>();
                if (m_ItemDict.ContainsKey(nPlace))
                {
                        list.AddRange(m_ItemDict[nPlace].Values);
                        list.Sort(CompareRaidBag);
                }

                return list;

        }

        public Dictionary<long, DropObject> GetObjectDict(int nPlace)
        {
                if (m_ItemDict.ContainsKey(nPlace))
                {
                        return m_ItemDict[nPlace];
                }
                else
                {
                        Debuger.Log("null");
                }
                return null;
        }

        public List<DropObject> GetMyBagItem()
        {
                return new List<DropObject>(GetObjectDict((int)ItemType.BAG_PLACE.MAIN).Values);
        }

        public List<DropObject> GetMyBagItemBySort()
        {
                List<DropObject> Item_InBag = GetMyBagItem();
                Item_InBag.Sort(CompareItemSort);
                return Item_InBag;
        }

        public List<DropObject> GetMyBagItemByTypeList(List<int> typelist)
        {
                List<DropObject> ItemList = new List<DropObject>();
                foreach (DropObject di in m_ItemDict[(int)ItemType.BAG_PLACE.MAIN].Values)
                {
                        if (typelist.Contains(di.GetSubType()))
                        {
                                ItemList.Add(di);
                        }
                }
                return ItemList;
        }

        public List<DropObject> GetMyBagItemByType(int type)
        {
                List<DropObject> Item_InBag = GetMyBagItem();
                List<DropObject> ItemList = new List<DropObject>();
                for (int i = 0; i < Item_InBag.Count; i++)
                {
                        if (Item_InBag[i].GetSubType() == type)
                        {
                                ItemList.Add(Item_InBag[i]);
                        }
                }
                return ItemList;
        }

        public DropObject GetDropObj(int cfgId, ItemType.BAG_PLACE place, Thing_Type type)
        {
                if (m_ItemDict.ContainsKey((int)place))
                {
                        foreach (DropObject di in m_ItemDict[(int)place].Values)
                        {
                                if (di.nType == (int)type && di.idCfg == cfgId)
                                {
                                        return di;
                                }
                        }
                }
                return null;
        }



        public int GetItemNumByID(int item_id)  //主背包物品//
        {
                int count = 0;
                foreach (long id in m_ItemDict[(int)ItemType.BAG_PLACE.MAIN].Keys)
                {
                        if (m_ItemDict[(int)ItemType.BAG_PLACE.MAIN][id].nType == (int)Thing_Type.ITEM)
                        {
                                if (m_ItemDict[(int)ItemType.BAG_PLACE.MAIN][id].idCfg == item_id)
                                {
                                        count += m_ItemDict[(int)ItemType.BAG_PLACE.MAIN][id].nOverlap;
                                }
                        }
                }
                return count;
        }


        protected int CompareItemSort(DropObject itema, DropObject itemb) //物品里先type从小到大，再品质从大到小，再评分从大到小 //排序
        {
                if (null == itema || null == itemb)
                {
                        return -1;
                }

                ItemConfig ica = GetItemCfg(itema.idCfg);
                ItemConfig icb = GetItemCfg(itemb.idCfg);

                if (ica != null && icb != null)
                {
                        int type_a = ica.type;
                        int type_b = icb.type;
                        if (type_a != type_b)
                        {
                                return type_a - type_b;
                        }


                        int qa_a = ica.quality;
                        int qa_b = icb.quality;
                        if (qa_a != qa_b)
                        {
                                return qa_b - qa_a;
                        }

                        int value_a = ica.value;
                        int value_b = icb.value;
                        if (value_a != value_b)
                        {
                                return value_b - value_a;
                        }
                }

                return (int)(itema.id - itemb.id);
        }


        int m_nNewItemCount = 0;
        public int NewItemCount
        {
                get
                {
                        return m_nNewItemCount;
                }
                set
                {
                        m_nNewItemCount = value;
                        if (UIManager.GetInst().GetUIBehaviour<UI_HomeMain>() != null)
                        {
                                UIManager.GetInst().GetUIBehaviour<UI_HomeMain>().RefreshNewDarwNum("bagbtn", NewItemCount);
                        }
                }
        }
        List<long> m_NewItemList = new List<long>();
        public bool IsNewItem(long id)
        {
                return m_NewItemList.Contains(id);
        }
        public void ClearNewItemList()
        {
                m_NewItemList.Clear();
                NewItemCount = 0;
        }
        public void AddToNewItemList(long id)
        {
                if (!m_NewItemList.Contains(id))
                {
                        NewItemCount++;
                        m_NewItemList.Add(id);
                }
        }
}

public class DropObject
{
        public long id;
        public int nType;
        public int idCfg;
        public int nPlace;
        public int nOverlap;
        public int nCreateTime;

        public int TempId
        {
                get
                {
                        return idCfg * 100 + nType;
                }
        }

        public int CampSP
        {
                get
                {
                        switch (nType)
                        {
                                case (int)Thing_Type.ITEM:
                                        {
                                                ItemConfig cfg = ItemManager.GetInst().GetItemCfg(idCfg);
                                                if (cfg != null)
                                                {
                                                        return cfg.camp_skill_point;
                                                }
                                        }
                                        break;
                        }
                        return 0;

                }
        }
        public string ToInfoStr()
        {
                return nType + "&" + idCfg + "&" + nOverlap;
        }
        public int GetMaxOverlap()
        {
                switch (nType)
                {
                        case (int)Thing_Type.ITEM:
                                {
                                        ItemConfig cfg = ItemManager.GetInst().GetItemCfg(idCfg);
                                        if (cfg != null)
                                        {
                                                return cfg.stackable;
                                        }
                                }
                                break;
                }
                return 1;
        }

        public string GetSubTypeIcon()
        {
                switch (nType)
                {
                        case (int)Thing_Type.EQUIP:
                                {
                                        EquipHoldConfig cfg = EquipManager.GetInst().GetEquipCfg(idCfg);
                                        if (cfg != null)
                                        {
                                                return cfg.type_icon;
                                        }
                                }
                                break;
                        case (int)Thing_Type.ITEM:
                                {
                                        ItemConfig cfg = ItemManager.GetInst().GetItemCfg(idCfg);
                                        if (cfg != null)
                                        {
                                                return cfg.type_icon;
                                        }
                                }
                                break;
                }
                return "";
        }
        public string GetDesc()
        {
                switch (nType)
                {
                        case (int)Thing_Type.EQUIP:
                                {
                                        EquipHoldConfig cfg = EquipManager.GetInst().GetEquipCfg(idCfg);
                                        if (cfg != null)
                                        {
                                                return LanguageManager.GetText(cfg.desc);
                                        }
                                }
                                break;
                        case (int)Thing_Type.ITEM:
                                {
                                        ItemConfig cfg = ItemManager.GetInst().GetItemCfg(idCfg);
                                        if (cfg != null)
                                        {
                                                return LanguageManager.GetText(cfg.desc);
                                        }
                                }
                                break;
                }
                return "";
        }

        /// <summary>
        /// ITEM优先级
        /// </summary>
        /// <returns></returns>
        public int Priority
        {
                get
                {
                        switch (nType)
                        {
                                case (int)Thing_Type.ITEM:
                                        {
                                                ItemConfig cfg = ItemManager.GetInst().GetItemCfg(idCfg);
                                                if (cfg != null)
                                                {
                                                        return cfg.priority;
                                                }
                                        }
                                        break;
                        }
                        return 999;
                }
        }

        public string GetName()
        {
                switch (nType)
                {
                        case (int)Thing_Type.EQUIP:
                                {
                                        EquipHoldConfig cfg = EquipManager.GetInst().GetEquipCfg(idCfg);
                                        if (cfg != null)
                                        {
                                                return LanguageManager.GetText(cfg.name);
                                        }
                                }
                                break;
                        case (int)Thing_Type.ITEM:
                                {
                                        ItemConfig cfg = ItemManager.GetInst().GetItemCfg(idCfg);
                                        if (cfg != null)
                                        {
                                                return LanguageManager.GetText(cfg.name);
                                        }
                                }
                                break;
                }
                return "";

        }

        public string GetSubTypeName()
        {
                switch (nType)
                {
                        case (int)Thing_Type.EQUIP:
                                {
                                        return LanguageManager.GetText("equip_type_name_" + GetSubType());
                                }
                        case (int)Thing_Type.ITEM:
                                {
                                        return LanguageManager.GetText("item_type_name_" + GetSubType());
                                }
                }
                return "";
        }

        public int GetSubType()
        {
                switch (nType)
                {
                        case (int)Thing_Type.EQUIP:
                                {
                                        EquipHoldConfig cfg = EquipManager.GetInst().GetEquipCfg(idCfg);
                                        if (cfg != null)
                                        {
                                                return cfg.type;
                                        }
                                }
                                break;
                        case (int)Thing_Type.ITEM:
                                {
                                        ItemConfig cfg = ItemManager.GetInst().GetItemCfg(idCfg);
                                        if (cfg != null)
                                        {
                                                return cfg.type;
                                        }
                                }
                                break;
                }
                return 0;

        }
        public int GetValue()
        {
                switch (nType)
                {
                        case (int)Thing_Type.EQUIP:
                                {
                                        EquipHoldConfig cfg = EquipManager.GetInst().GetEquipCfg(idCfg);
                                        if (cfg != null)
                                        {
                                                return cfg.value;
                                        }
                                }
                                break;
                        case (int)Thing_Type.ITEM:
                                {
                                        ItemConfig cfg = ItemManager.GetInst().GetItemCfg(idCfg);
                                        if (cfg != null)
                                        {
                                                return cfg.value;
                                        }
                                }
                                break;
                }
                return 0;
        }

        public bool CanUseInRaid()
        {
                switch (nType)
                {
                        case (int)Thing_Type.ITEM:
                                {
                                        ItemConfig cfg = ItemManager.GetInst().GetItemCfg(idCfg);
                                        if (cfg != null)
                                        {
                                                return cfg.is_use == 1 && cfg.use_place == 1;
                                        }
                                }
                                break;
                }
                return false;
        }

        public DropObject()
        {
                id = 0;
        }

        public DropObject(DropObject other)
        {
                id = other.id;
                idCfg = other.idCfg;
                nType = other.nType;
                nPlace = other.nPlace;
                nOverlap = other.nOverlap;
                nCreateTime = other.nCreateTime;
        }
        public DropObject(Message.SCMsgItem msg)
        {
                id = msg.id;
                nType = msg.nType;
                idCfg = msg.idCfg;
                nPlace = msg.nPlace;
                nOverlap = msg.nOverlap;
                nCreateTime = msg.nCreateTime;
        }

        public DropObject(string itemstr, char split = '&')
        {
                if (!string.IsNullOrEmpty(itemstr))
                {
                        string[] tmps = itemstr.Split(split);
                        if (tmps.Length >= 3)
                        {
                                int.TryParse(tmps[0], out nType);
                                int.TryParse(tmps[1], out idCfg);
                                int.TryParse(tmps[2], out nOverlap);
                        }
                }
        }

        public string GetQualityIconName()
        {
                if (nType == (int)Thing_Type.EQUIP)
                {
                        EquipHoldConfig equipCfg = EquipManager.GetInst().GetEquipCfg(idCfg);
                        if (equipCfg != null)
                        {
                                if (equipCfg.quality > 1)
                                {
                                        return "Bg#quality" + equipCfg.quality;
                                }
                        }
                }
                return "";
        }

        public string GetIconName()
        {
                string iconname = "";
                switch (nType)
                {
                        case (int)Thing_Type.RESOURCE:
                                {
                                        ResourcesConfigHold cfg = CommonDataManager.GetInst().GetResourcesCfg(idCfg);
                                        iconname = cfg.icon;
                                }
                                break;
                        case (int)Thing_Type.ITEM:
                                {
                                        ItemConfig itemcfg = ItemManager.GetInst().GetItemCfg(idCfg);
                                        iconname = itemcfg.icon;
                                }
                                break;
                        case (int)Thing_Type.EQUIP:
                                {
                                        EquipHoldConfig equipCfg = EquipManager.GetInst().GetEquipCfg(idCfg);
                                        iconname = equipCfg.icon;
                                }
                                break;
                        case (int)Thing_Type.HERO:
                                {
                                        CharacterConfig characterCfg = CharacterManager.GetInst().GetCharacterCfg(idCfg);
                                        //iconname = ModelResourceManager.GetInst().GetIconRes(characterCfg.modelid);
                                }
                                break;
                }
                return iconname;
        }

}


public class ItemType
{

    public enum BAG_PLACE
    {
        MAIN = 0,    // 主背包
        RAID = 1,    // 迷宫背包
        MAX = 2,
    }

    public enum EQUIP_PART
    {
        MAIN_HAND = 11, //主手
        OFF_HAND = 12,  //副手
        BODY = 13,      //身体
        HEAD = 14,      //头
        FEET = 15,      //脚
        JEWELRY = 16,   //饰品
        MAX = 17,
    }

    public static string[] part_des = { "主手", "副手", "衣服", "帽子", "鞋子", "首饰" };
}

public enum ITEM_SUB_TYPE
{
    NONE,
    MEDICINAL,                   //1, 药材
    FOOD,                           // 2, 食材
    WOOD,                         // 3, 木材
    MINEAL,                        // 4, 矿石
    USE,                               // 5, 使用
    FRIENDLY,                     // 6, 好感度道具
    SPECIAL,                        // 7, 特殊
    MAX,
}
