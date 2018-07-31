using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

public class UI_AllBag : UI_ScrollRectHelp
{
        public Transform left;
        public Transform bag;
        public GameObject item;
        public GameObject gou;

        GameObject m_group;

        public enum BAG_TAB
        { 
                ALL = 0,        //全部
                EQUIP = 1,      //装备
                ITEM = 2,       //道具
                MAX = 3,
        }
        BAG_TAB m_tab = BAG_TAB.ALL;

        void Awake()
        {
                for (int i = 0; i < (int)BAG_TAB.MAX; i++)
                {
                        Button btn = GetButton(gameObject, "Tab" + i);
                        EventTriggerListener listener = btn.gameObject.AddComponent<EventTriggerListener>();
                        listener.onClick = OnTab;
                        listener.SetTag(i);
                }

                m_group = GetGameObject(sr.gameObject, "equip0");
                SetGou(false, null);
        }

        private void OnTab(GameObject go, PointerEventData data)
        {
                Button btn = go.GetComponent<Button>();

                BAG_TAB pt = (BAG_TAB)EventTriggerListener.Get(go).GetTag();
                if (pt != m_tab)
                {
                        m_select = null;
                        SetGou(false, null);
                        //m_tab = pt;
                        RefreshGroup(pt);
                }

        }

        public void RefreshGroup(BAG_TAB tab)
        {
                GetBagInfo();
                RefreshLeft(m_select);
                HightLightTab(tab);
                m_tab = tab;
                switch (tab)
                {

                        case BAG_TAB.ALL:
                                RefreshALL();
                                break;
                        case BAG_TAB.EQUIP:
                                RefreshEquip();
                                break;
                        case BAG_TAB.ITEM:
                                RefreshItem();
                                return;
                        default:
                                break;
                }
        
        }


        public override void RefreshCurWnd(object data)
        {
                UpdateBag();
        }

        void UpdateBag()
        {
                RefreshGroup(m_tab);
        }

        void HightLightTab(BAG_TAB tab)
        {
                int index = (int)tab;
                for (int i = 0; i < (int)BAG_TAB.MAX; i++)
                {
                        Image btn = GetImage(gameObject, "Tab" + i);
                        Image left = GetImage(btn.gameObject, "left");
                        Image right = GetImage(btn.gameObject, "right");
                        if (i < index)
                        {
                                btn.enabled = false;
                                left.enabled = true;
                                right.enabled = false;
                        }
                        if (i == index)
                        {
                                btn.enabled = true;
                                left.enabled = false;
                                right.enabled = false;
                        }
                        if (i > index)
                        {
                                btn.enabled = false;
                                left.enabled = false;
                                right.enabled = true;
                        }
                }
        }


        List<Equip> m_equip_list = new List<Equip>();
        List<DropObject> m_item_list = new List<DropObject>();
        int equip_num = 0;
        int item_num = 0;
        int all_num = 0;

        void GetBagInfo()
        {
                m_equip_list = EquipManager.GetInst().GetEquipInBagBySort();
                m_item_list = ItemManager.GetInst().GetMyBagItemBySort();
                equip_num = m_equip_list.Count;
                item_num = m_item_list.Count;
                all_num = equip_num + item_num;
                int max_num = PlayerController.GetInst().GetPropertyInt("bag_capacity");
                if (all_num > max_num)
                {
                        GetText(bag.gameObject, "num").text = "<color=red>" + all_num + "</color>/" + PlayerController.GetInst().GetPropertyInt("bag_capacity");
                }
                else
                {
                        GetText(bag.gameObject, "num").text = "<color=#FDCE95>" + all_num + "</color>/" + PlayerController.GetInst().GetPropertyInt("bag_capacity");
                }
        }

        void RefreshALL()
        {
                SetChildActive(this.item.transform.parent, false);
                int bag_num = PlayerController.GetInst().GetPropertyInt("bag_capacity"); 
                int show_num = bag_num  < all_num ? all_num : bag_num;

                for (int i = 0; i < show_num; i++)
                {
                        Transform temp = GetChildByIndex(this.item.transform.parent, i);
                        if (temp == null)
                        {
                                temp = CloneElement(this.item, i.ToString()).transform;
                        }
                        EventTriggerListener listeren = EventTriggerListener.Get(temp);
                        listeren.onClick = OnClick;
                        listeren.onDrag = OnDarg;
                        listeren.onBeginDrag = OnBeginDrag;
                        listeren.onEndDrag = OnEndDrag;

                        Image quality_image = FindComponent<Image>(temp, "quality");
                        Image icon_image = FindComponent<Image>(temp, "icon");
                        Text num_down = FindComponent<Text>(temp, "num_down");
                        Image newlabel = FindComponent<Image>(temp, "newlabel");
                        if (i < all_num)
                        {

                                temp.GetComponent<Button>().interactable = true;
                                quality_image.enabled = true;
                                icon_image.enabled = true;

                                if (i < equip_num)       //装备
                                {
                                        Equip equip = m_equip_list[i];
                                        listeren.SetTag(equip);
                                        EquipHoldConfig ec = EquipManager.GetInst().GetEquipCfg(equip.equip_configid);
                                        if (ec != null)
                                        {
                                                int quality = ec.quality;
                                                if (quality > 1)
                                                {
                                                        ResourceManager.GetInst().LoadIconSpriteSyn("Bg#quality" + quality, quality_image.transform);
                                                }
                                                else
                                                {
                                                        quality_image.enabled = false;
                                                }
                                                ResourceManager.GetInst().LoadIconSpriteSyn(ec.icon, icon_image.transform);
                                                num_down.text = "";
                                                if (ItemManager.GetInst().IsNewItem(equip.id))
                                                {
                                                        newlabel.enabled = true;
                                                }
                                        }
                                }
                                else //物品
                                {
                                        DropObject item = m_item_list[i - equip_num];
                                        listeren.SetTag(item);
                                        ItemConfig ic = ItemManager.GetInst().GetItemCfg(item.idCfg);
                                        if (ic != null)
                                        {
                                                int quality = ic.quality;
                                                if (quality > 1)
                                                {
                                                        ResourceManager.GetInst().LoadIconSpriteSyn("Bg#quality" + quality, quality_image.transform);
                                                }
                                                else
                                                {
                                                        quality_image.enabled = false;
                                                }

                                                ResourceManager.GetInst().LoadIconSpriteSyn(ic.icon, FindComponent<Image>(temp, "icon").transform);
                                                num_down.text = item.nOverlap.ToString();
                                                if (ItemManager.GetInst().IsNewItem(item.id))
                                                {
                                                        newlabel.enabled = true;
                                                }
                                        }
                                }
                        }
                        else
                        {
                                temp.GetComponent<Button>().interactable = false;
                                quality_image.enabled = false;
                                num_down.text = "";
                                icon_image.enabled = false;
                                newlabel.enabled = false;
                        }          
                        temp.SetActive(true);
                }
                GetGameObject(sr.gameObject, "content").GetComponent<RectTransform>().anchoredPosition = Vector2.up * (-100);
        
        }

        void RefreshEquip()
        {
                SetChildActive(this.item.transform.parent, false);
                for (int i = 0; i < equip_num; i++)
                {
                        Transform temp = GetChildByIndex(this.item.transform.parent, i);
                        if (temp == null)
                        {
                                temp = CloneElement(this.item, i.ToString()).transform;
                        }
                        EventTriggerListener listeren = EventTriggerListener.Get(temp);
                        listeren.onClick = OnClick;
                        listeren.onDrag = OnDarg;
                        listeren.onBeginDrag = OnBeginDrag;
                        listeren.onEndDrag = OnEndDrag;

                        Image quality_image = FindComponent<Image>(temp, "quality");
                        Image icon_image = FindComponent<Image>(temp, "icon");
                        Text num_down = FindComponent<Text>(temp, "num_down");

                        temp.GetComponent<Button>().interactable = true;
                        quality_image.enabled = true;
                        icon_image.enabled = true;

                        Equip equip = m_equip_list[i];
                        listeren.SetTag(equip);
                        EquipHoldConfig ec = EquipManager.GetInst().GetEquipCfg(equip.equip_configid);
                        if (ec != null)
                        {
                                int quality = ec.quality;
                                if (quality > 1)
                                {
                                        ResourceManager.GetInst().LoadIconSpriteSyn("Bg#quality" + quality, quality_image.transform);
                                }
                                else
                                {
                                        quality_image.enabled = false;
                                }
                                ResourceManager.GetInst().LoadIconSpriteSyn(ec.icon, icon_image.transform);
                                num_down.text = "";   //强化
                        }
                        temp.SetActive(true);
                }
                GetGameObject(sr.gameObject, "content").GetComponent<RectTransform>().anchoredPosition = Vector2.up * (-100);
        }


        void RefreshItem()
        {
                SetChildActive(this.item.transform.parent, false);
                for (int i = 0; i < item_num; i++)
                {
                        Transform temp = GetChildByIndex(this.item.transform.parent, i);
                        if (temp == null)
                        {
                                temp = CloneElement(this.item, i.ToString()).transform;
                        }
                        EventTriggerListener listeren = EventTriggerListener.Get(temp);
                        listeren.onClick = OnClick;
                        listeren.onDrag = OnDarg;
                        listeren.onBeginDrag = OnBeginDrag;
                        listeren.onEndDrag = OnEndDrag;

                        Image quality_image = FindComponent<Image>(temp, "quality");
                        Image icon_image = FindComponent<Image>(temp, "icon");
                        Text num_down = FindComponent<Text>(temp, "num_down");


                        temp.GetComponent<Button>().interactable = true;
                        quality_image.enabled = true;
                        icon_image.enabled = true;

                        DropObject item = m_item_list[i];
                        listeren.SetTag(item);
                        ItemConfig ic = ItemManager.GetInst().GetItemCfg(item.idCfg);
                        if (ic != null)
                        {
                                int quality = ic.quality;
                                if (quality > 1)
                                {
                                        ResourceManager.GetInst().LoadIconSpriteSyn("Bg#quality" + quality, quality_image.transform);
                                }
                                else
                                {
                                        quality_image.enabled = false;
                                }
                                ResourceManager.GetInst().LoadIconSpriteSyn(ic.icon, FindComponent<Image>(temp, "icon").transform);
                                num_down.text = item.nOverlap.ToString();
                        }
                        temp.SetActive(true);
                }
                GetGameObject(sr.gameObject, "content").GetComponent<RectTransform>().anchoredPosition = Vector2.up * (-100);

        }

        object m_select = null;

        void OnClick(GameObject go, PointerEventData data)
        {
                if (!can_click)
                {
                        return;
                }
                object select = EventTriggerListener.Get(go).GetTag();
                if (select == null) //
                {
                        return;
                }
                m_select = select;
                SetGou(true, go.transform);
                RefreshLeft(m_select);
    
        }

        void SetGou(bool ishow,Transform father)
        {
                if (ishow)
                {
                        gou.transform.SetParent(father);
                        gou.transform.localPosition = Vector3.zero;
                }
                gou.SetActive(ishow);
        }


        void RefreshLeft(object select)
        {
                GetButton(left.gameObject, "use").gameObject.SetActive(false);
                FindComponent<Text>(left, "name").text = "";
                FindComponent<Text>(left, "des").text = "";
                if (select != null)
                {
                        if (select is Equip)
                        {
                                Equip equip = select as Equip;
                                EquipHoldConfig ec = EquipManager.GetInst().GetEquipCfg(equip.equip_configid);
                                FindComponent<Text>(left, "name").text = LanguageManager.GetText(ec.name);
                                FindComponent<Text>(left, "des").text = LanguageManager.GetText(ec.desc);

                        }
                        if (select is DropObject)
                        {
                                DropObject item = select as DropObject;
                                ItemConfig ic = ItemManager.GetInst().GetItemCfg(item.idCfg);

                                if (ic == null)
                                {
                                        return;
                                }

                                if (ic.is_use == 1 && ic.use_place == 0)
                                {
                                        GetButton(left.gameObject, "use").gameObject.SetActive(true);
                                }
                                FindComponent<Text>(left, "name").text = LanguageManager.GetText(ic.name);
                                FindComponent<Text>(left, "des").text = LanguageManager.GetText(ic.desc);
                        }
                }
                else
                {
                        SetGou(false, null);
                }
        }

        public void OnUse()
        {
                DropObject item = m_select as DropObject;
                ItemManager.GetInst().UseItem(item.id, 1); //暂时都用1
                m_select = null;
        }

        public void OnClickClose()
        {
                UIManager.GetInst().CloseUI(this.name);
                UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", true);
                ItemManager.GetInst().ClearNewItemList();
        }

}

