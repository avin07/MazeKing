using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;

class UI_NpcVisitor : UIBehaviour
{
    long m_npc_id;
    int visitor_id;
    int get_type;
    int pay_type;
    bool item_enough = true;
    bool is_need_pet = false;

    public Text pay_des;
    public GameObject pay_item;
    public Text get_des;
    public GameObject get_item;
    public GameObject pet_group;

    public void Refresh(long id, int idConfig, string goods)
    {
        m_npc_id = id;
        UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", false);
        PetManager.GetInst().ChoosePetList.Clear();
        NpcConfig nc = NpcManager.GetInst().GetNpcCfg(idConfig);
        NpcHouseVisitorHold nhv = NpcManager.GetInst().GetVisitorCfg(nc.type_id);
        visitor_id = nc.type_id;

        GetText("title").text = LanguageManager.GetText(nhv.name);
        GetText("des").text = LanguageManager.GetText(nhv.desc);
        if (nhv.is_choose == 0)
        {
            is_need_pet = false;
            pet_group.SetActive(false);
        }
        else
        {
            is_need_pet = true;
            pet_group.SetActive(true);
        }

        //获得的显示
        get_type = nhv.get_type;
        string get_desc = LanguageManager.GetText("visitor_get_desc_" + get_type);
        if (get_desc.Equals("visitor_get_desc_" + get_type)) //显示物品
        {
            get_item.SetActive(true);
            get_des.gameObject.SetActive(false);
            string itemlist = goods;
            if (itemlist.Equals(CommonString.zeroStr) || itemlist.Length == 0)
            {
                itemlist = nhv.get_parameter;
            }
            else
            {
                    itemlist = itemlist.Replace(CommonString.ampersandStr, CommonString.commaStr);
                itemlist = itemlist.Replace(CommonString.pipeStr, CommonString.semicolonStr);
            }
            RefreshGetItem(itemlist, get_item);

        }
        else
        {
            get_item.SetActive(false);
            get_des.gameObject.SetActive(true);
            if (get_desc.Contains("{0}"))
            {
                get_des.text = String.Format(get_desc, nhv.get_parameter);
            }
            else
            {
                get_des.text = get_desc;
            }
        }

        //付出的显示
        pay_type = nhv.pay_type;
        string pay_desc = LanguageManager.GetText("visitor_pay_desc_" + pay_type);
        if (pay_desc.Equals("visitor_pay_desc_" + pay_type)) //显示物品
        {
            pay_item.SetActive(true);
            pay_des.gameObject.SetActive(false);
            string itemlist = nhv.pay_parameter;
            item_enough = RefreshNeedItem(itemlist, pay_item);

        }
        else
        {
            pay_item.SetActive(false);
            pay_des.gameObject.SetActive(true);
            if (pay_desc.Contains("{0}"))
            {
                if (pay_type == 1) //恶癖特殊处理
                {
                    string name = "";
                    string[] sick = nhv.pay_parameter.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < sick.Length; i++)
                    {
                        SpecificityHold sfh = PetManager.GetInst().GetSpecificityCfg(int.Parse(sick[i]));
                        name += LanguageManager.GetText(sfh.name) + " ";
                    }
                    pay_des.text = String.Format(pay_desc, name);
                }
                else if (pay_type == 4) //出门
                {
                        pay_des.text = String.Format(pay_desc, UIUtility.GetTimeString3(int.Parse(nhv.pay_parameter)));
                }
                else
                {
                        pay_des.text = String.Format(pay_desc, nhv.pay_parameter);
                }
            }
            else
            {
                pay_des.text = pay_desc;
            }
        }

        NpcInfo ni = NpcManager.GetInst().GetNpc(id);
        if (ni.id != 0)
        {
                GetText("time").text = ni.restTime + "天";
        }

        HomeManager.GetInst().ChangeNpcNewIcon(m_npc_id, nc);

        RefreshPetChoose();
    }


    void RefreshGetItem(string need_info, GameObject root)
    {
        GameObject item = GetGameObject(root, "item0");
        string[] need = need_info.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        SetChildActive(root.transform,false);

        int num = 0;
        string name = "";
        int id = 0;
        string des = "";
        Thing_Type type;

        for (int i = 0; i < need.Length; i++)
        {
            GameObject temp = GetGameObject(root, "item" + i);
            if (temp == null)
            {
                temp = CloneElement(item, "item" + i);
            }
            CommonDataManager.GetInst().SetThingIcon(need[i], temp.transform, null, out name, out num, out id, out des,out type);
            GetText(temp, "num").text = num.ToString();
            temp.SetActive(true);
        }
    }


    bool RefreshNeedItem(string need_info, GameObject root)
    {
        bool can_build = true;
        GameObject item = GetGameObject(root, "item0");
        string[] need = need_info.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        SetChildActive(root.transform,false);

        int num = 0;
        string name = "";
        int id = 0;
        string des = "";
        Thing_Type type;

        for (int i = 0; i < need.Length; i++)
        {
            GameObject temp = GetGameObject(root, "item" + i);
            if (temp == null)
            {
                temp = CloneElement(item, "item" + i);
            }
            CommonDataManager.GetInst().SetThingIcon(need[i], temp.transform,null, out name, out num, out id, out des,out type);
            int has_num = CommonDataManager.GetInst().GetThingNum(need[i], out num);
            if (has_num >= num)
            {
                GetText(temp, "num").text = has_num + " / " + num;
            }
            else
            {
                GetText(temp, "num").text = "<color=red>" + has_num + "</color>" + " / " + num;
                can_build = false;
            }
            temp.SetActive(true);
        }
        return can_build;
    }

    public void ChoosePet()
    {
        List<Pet> pet_list = PetManager.GetInst().GetPetsForVisitorByPay(visitor_id);
        pet_list = PetManager.GetInst().GetPetsForVisitorByGet(pet_list, visitor_id);
        List<long> pet_list_id = new List<long>();
        for (int i = 0; i < pet_list.Count; i++)
        {
            pet_list_id.Add(pet_list[i].ID);
        }
        UIManager.GetInst().ShowUI<UI_PetChoose>("UI_PetChoose").RefreshMyPetList(pet_list_id, PetChoosetype.Visitor, 1);
    }

    public void RefreshPetChoose()
    {
        if (PetManager.GetInst().ChoosePetList.Count == 1)
        {
            Pet m_pet = PetManager.GetInst().GetPet(PetManager.GetInst().ChoosePetList[0]);
            ResourceManager.GetInst().LoadIconSpriteSyn(ModelResourceManager.GetInst().GetIconRes(m_pet.GetPropertyInt("model_id")), GetImage(pet_group, "choose").transform);
        }
        else
        {
            ResourceManager.GetInst().LoadIconSpriteSyn("Bg#jia", GetImage(pet_group, "choose").transform);
        }
    }

    public override void OnClickClose(GameObject go)
    {
        base.OnClickClose(go);
        UIManager.GetInst().ShowUI<UI_HomeMain>("UI_HomeMain");
    }

    public void Ok()
    {
        //以后还要判断获得物品数是否超过包裹上限
        if (is_need_pet)
        {
            if (PetManager.GetInst().ChoosePetList.Count != 1)
            {
                GameUtility.PopupMessage("请选择宠物");
            }
            else
            {
                NpcManager.GetInst().SendHomeVisitor(m_npc_id, PetManager.GetInst().ChoosePetList[0].ToString());
                OnClickClose(gameObject);
            }
        }
        else
        {
            if (!item_enough)
            {
                GameUtility.PopupMessage("物品不足");
            }
            else
            {
                    NpcManager.GetInst().SendHomeVisitor(m_npc_id, CommonString.zeroStr);
                OnClickClose(gameObject);
            }
        }

    }
   
}