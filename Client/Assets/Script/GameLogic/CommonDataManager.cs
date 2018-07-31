using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Message;

class CommonDataManager : SingletonObject<CommonDataManager>  //一些好多模块使用的公共表
{

    Dictionary<int, ResourcesConfigHold> m_ResourcesCfgDict = new Dictionary<int, ResourcesConfigHold>();
    Dictionary<int, HintWordConfig> m_HintTextDict = new Dictionary<int, HintWordConfig>();
    Dictionary<int, DialogConfig> m_DialogTextDict = new Dictionary<int, DialogConfig>();
    Dictionary<int, HomeExpHold> m_HomeExpDict = new Dictionary<int, HomeExpHold>();   //家园等级经验

    List<int> extraBuildList = new List<int>();
    public bool IsBuildUnlock(int id)
    {
        return extraBuildList.Contains(id);
    }

    public void Init()
    {
        ConfigHoldUtility<ResourcesConfigHold>.LoadXml("Config/resources", m_ResourcesCfgDict); //资源类型表
        ConfigHoldUtility<HintWordConfig>.LoadXml("Config/hint_word", m_HintTextDict);  //错误提示表
        ConfigHoldUtility<DialogConfig>.LoadXml("Config/dialog", m_DialogTextDict);  //对话
        ConfigHoldUtility<HomeExpHold>.LoadXml("Config/building_home_exp", m_HomeExpDict);

        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgNotProgramError), OnGetErrorInfo);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgTips), OnGetTips);

        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgUnlockBuild), OnGetExtraAll);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgAddUnlockBuild), OnGetExtraOne);

    }
    void OnGetTips(object sender, SCNetMsgEventArgs e)
    {
        SCMsgTips msg = e.mNetMsg as SCMsgTips;
        ShowTip(msg.id);
    }

    public string GetHintWord(int id)
    {
        if (m_HintTextDict.ContainsKey(id))
        {
            return m_HintTextDict[id].text;
        }
        return "";
    }
    public void ShowTip(int tipId, bool bNeedClick = false)
    {
        if (m_HintTextDict.ContainsKey(tipId))
        {
            UIManager.GetInst().ShowUI<UI_RaidAside>("UI_RaidAside").SetText(m_HintTextDict[tipId].text, bNeedClick);
            //GameUtility.ShowTip(m_HintTextDict[tipId].text);
        }
    }

    void OnGetErrorInfo(object sender, SCNetMsgEventArgs e)
    {
        SCMsgNotProgramError msg = e.mNetMsg as SCMsgNotProgramError;
        singleton.GetInst().ShowMessage(ErrorOwner.server, msg.error_info);
    }


    void OnGetExtraAll(object sender, SCNetMsgEventArgs e)
    {
        SCMsgUnlockBuild msg = e.mNetMsg as SCMsgUnlockBuild;
        extraBuildList.Clear();
        if (GameUtility.IsStringValid(msg.strBuild))
        {
            string[] temp = msg.strBuild.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < temp.Length; i++)
            {
                extraBuildList.Add(int.Parse(temp[i]));
                Debug.Log("extraBuildList.Add " + temp[i]);
            }
        }
    }

    void OnGetExtraOne(object sender, SCNetMsgEventArgs e)
    {
        SCMsgAddUnlockBuild msg = e.mNetMsg as SCMsgAddUnlockBuild;
        extraBuildList.Add(msg.idbuildcfg);
    }

    public string GetExtraBuildStr()
    {
        string str = string.Empty;
        for (int i = 0; i < extraBuildList.Count; i++)
        {
            str += extraBuildList[i] + ",";
        }
        return str;
    }

    public ResourcesConfigHold GetResourcesCfg(int id)
    {
        if (m_ResourcesCfgDict.ContainsKey(id))
        {
            return m_ResourcesCfgDict[id];
        }
        else
        {
            singleton.GetInst().ShowMessage(ErrorOwner.designer, "资源表中没有id为" + id + "的资源");
        }
        return null;
    }

    public int GetHeroBagLimit()
    {
        return PlayerController.GetInst().GetPropertyInt("pet_capactity") + GlobalParams.GetInt("init_hero_bag");
    }

    public DialogConfig GetDialogCfg(int id)
    {
        if (m_DialogTextDict.ContainsKey(id))
        {
            return m_DialogTextDict[id];
        }
        return null;
    }

    public void SetThingIcon(string info, Transform iconTf, Transform qualityTf, out string name, out int num, out int id, out string des, out Thing_Type type) //类别，id,数量,设置策划配置的东西的icon
    {
        string url = "";
        string[] details = info.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (details.Length != 3)
        {
            singleton.GetInst().ShowMessage(ErrorOwner.designer, "物品格式配错了！");
        }
        int mType;
        int.TryParse(details[0], out mType);
        int.TryParse(details[1], out id);
        int.TryParse(details[2], out num);
        int quality = 0;
        type = (Thing_Type)mType;
        name = String.Empty;
        des = String.Empty;
        switch (type)
        {
            case Thing_Type.RESOURCE:
                name = LanguageManager.GetText(GetResourcesCfg(id).name);
                des = LanguageManager.GetText(GetResourcesCfg(id).desc);
                url = GetResourcesCfg(id).icon;
                break;
            case Thing_Type.HERO:
                //                 name = LanguageManager.GetText(CharacterManager.GetInst().GetCharacterCfg(id).GetProp("name"));
                //                 des = LanguageManager.GetText(CharacterManager.GetInst().GetCharacterCfg(id).GetProp("desc"));
                //                 url = ModelResourceManager.GetInst().GetIconRes(CharacterManager.GetInst().GetCharacterCfg(id).GetPropInt("model_id"));
                break;
            case Thing_Type.ITEM:
                name = LanguageManager.GetText(ItemManager.GetInst().GetItemCfg(id).name);
                des = LanguageManager.GetText(ItemManager.GetInst().GetItemCfg(id).desc);
                url = ItemManager.GetInst().GetItemIconUrl(id);
                quality = ItemManager.GetInst().GetItemCfg(id).quality;
                break;
            case Thing_Type.EQUIP:
                name = LanguageManager.GetText(EquipManager.GetInst().GetEquipCfg(id).name);
                des = LanguageManager.GetText(EquipManager.GetInst().GetEquipCfg(id).desc);
                url = EquipManager.GetInst().GetEquipCfg(id).icon;
                quality = EquipManager.GetInst().GetEquipCfg(id).quality;
                break;
            default:
                break;
        }
        if (iconTf != null)
        {
            ResourceManager.GetInst().LoadIconSpriteSyn(url, iconTf);
        }
        if (qualityTf != null)
        {
            if (quality > 1)
            {
                ResourceManager.GetInst().LoadIconSpriteSyn("Bg#quality" + quality, qualityTf);
            }
            else
            {
                ResourceManager.GetInst().LoadIconSpriteSyn(String.Empty, qualityTf);
            }
        }
    }

    public int GetThingNum(string info, out int needNum)   //根据类型和id返回拥有物品的数量//
    {
        int type = 0;
        int id = 0;
        string[] details = info.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (details.Length != 3)
        {
            singleton.GetInst().ShowMessage(ErrorOwner.designer, "物品格式配错了！");
        }

        int.TryParse(details[0], out type);
        int.TryParse(details[1], out id);
        needNum = int.Parse(details[2]);

        int num = 0;
        switch ((Thing_Type)type)
        {
            case Thing_Type.RESOURCE:  //上锁//
                num = PlayerController.GetInst().GetPropertyInt(GetResourcesCfg(id).attr) - PlayerController.GetInst().GetPropertyInt(GetResourcesCfg(id).lock_attr);
                break;
            case Thing_Type.HERO:
                num = PetManager.GetInst().GetPetNumByCharacterID(id);
                break;
            case Thing_Type.ITEM:
                num = ItemManager.GetInst().GetItemNumByID(id);
                break;
            case Thing_Type.EQUIP:
                num = EquipManager.GetInst().GetEquipNumByID(id);
                break;
            default:
                break;
        }
        return num;
    }

    //加入折扣
    public bool CheckIsThingEnough(string require, string add_help = "", int cutoff = 0) //add_help 有些配表格式不标准
    {
        if (require.Equals("0") || require.Length == 0)
        {
            return true;
        }

        string[] need = require.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        int need_num = 0;
        int has_num = 0;
        for (int i = 0; i < need.Length; i++)
        {
            has_num = GetThingNum(add_help + need[i], out need_num);
            need_num = Mathf.CeilToInt(need_num * (1 - cutoff * 0.01f));

            if (has_num < need_num)
            {
                return false;
            }
        }
        return true;
    }

    public long GetNowResourceNum(string attr)
    {
        string lock_attr = string.Format("_{0}_lock", attr);
        return (PlayerController.GetInst().GetPropertyLong(attr) - PlayerController.GetInst().GetPropertyLong(lock_attr));
    }

    public bool IsBagFull()
    {
        int item_max = PlayerController.GetInst().GetPropertyInt("bag_capacity");
        int item_count = ItemManager.GetInst().GetObjectDict((int)ItemType.BAG_PLACE.MAIN).Count;
        int equip_count = EquipManager.GetInst().GetEquipInBag().Count;
        if (item_count + equip_count >= item_max)  //包满了
        {
            return true;
        }
        return false;
    }

    public void UpdateBags(int nPlace)
    {
        switch (nPlace)
        {
            case (int)ItemType.BAG_PLACE.MAIN:
                {
                    CommonDataManager.GetInst().OnThingChangeCallBack();
                }
                break;
            case (int)ItemType.BAG_PLACE.RAID:
                {
                    RaidManager.GetInst().RefreshBag();
                }
                break;
        }
    }

    void OnThingChangeCallBack() //道具数量变化对各种界面即使更新时的调用
    {
        UI_AllBag uibag = UIManager.GetInst().GetUIBehaviour<UI_AllBag>();
        if (uibag != null)
        {
            UIRefreshManager.GetInst().AddRefreshWnd(uibag, null);
        }
    }


    public HomeExpHold GetHomeExpCfg(int id)
    {
        if (m_HomeExpDict.ContainsKey(id))
        {
            return m_HomeExpDict[id];
        }
        return null;
    }
}


public enum Thing_Type  //游戏中掉落的物品分类
{
    RESOURCE = 1,  //读resource表
    HERO,          //读charactar表
    ITEM,          //读item表 
    EQUIP,         //读equip表
}


