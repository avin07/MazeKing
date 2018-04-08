// using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine.UI;
// 
// class UI_Bag : UIBehaviour
// {
//         Image m_Itembg0;
//         Image m_Item0;
//         Text m_ItemCount0;
//         
//         const int MAX_ITEM_NUM = 10;
//         Image[] m_ItemImage = new Image[MAX_ITEM_NUM];
//         Text[] m_ItemCount = new Text[MAX_ITEM_NUM];
//         void Awake()
//         {
//                 m_Itembg0 = GetImage("itembg0");
// 
//                 m_Item0 = GetImage("item0");
//                 m_ItemCount0 = GetText("count0");
//                 for (int i = 0; i < MAX_ITEM_NUM; i++)
//                 {
//                         Image itemImage ;
//                         if (i == 0)
//                         {
//                                 itemImage = m_Itembg0;
//                         }
//                         else 
//                         {
//                                 itemImage = (Image)Instantiate(m_Itembg0) as Image;
//                                 itemImage.name = "itembg" + i;
//                                 itemImage.transform.SetParent(m_Itembg0.transform.parent);
//                                 float x = m_Itembg0.rectTransform.localPosition.x + (i % 5) * (m_Itembg0.rectTransform.sizeDelta.x + 5.0f);
//                                 float y = m_Itembg0.rectTransform.localPosition.y - (i / 5) * (m_Itembg0.rectTransform.sizeDelta.y + 5.0f);
//                                 itemImage.rectTransform.localPosition = new Vector3(x, y, 0f);
//                                 itemImage.transform.localScale = Vector3.one;
//                         }
//                                 Image im = GetImage(itemImage.gameObject, "item0");
//                                 if (im != null)
//                                 {
//                                         im.name = "item" + i;
//                                         im.enabled = false;
//                                         m_ItemImage[i] = im;
//                                 }
//                                 Text t = GetText(itemImage.gameObject, "count0");
//                                 if (t != null)
//                                 {
//                                         t.name = "count" + i;
//                                         t.text = "";
//                                         m_ItemCount[i] = t;
//                                 }
//                         itemImage.enabled = true;
//                 }
//                 this.UILevel = UI_LEVEL.MAIN;
//         }
// 
//         public override void OnShow()
//         {
//                 base.OnShow();
//                 UpdateBag();
//         }
//         Dictionary<int, Item> m_ItemDict = new Dictionary<int, Item>();
//         Dictionary<int, string> m_ItemFromTreasure = new Dictionary<int, string>();
//         
//         public void UpdateBag()
//         {
//                 Dictionary<int, Item> dict = ItemManager.GetInst().GetItemDict();
//                 for (int i = 0; i < MAX_ITEM_NUM; i++)
//                 {
//                         m_ItemImage[i].enabled = false;
//                         m_ItemImage[i].transform.parent.gameObject.SetActive(false);
//                 }
// 
//                 foreach (Item item in dict.Values)
//                 {
//                         if (item.nPos >= 0 && item.nPos < MAX_ITEM_NUM)
//                         {
//                                 //Debuger.Log(item.nPos + " " + item.nOverlap);
//                                 Image im = m_ItemImage[item.nPos];
//                                 ResourceManager.GetInst().LoadItemIcon(ItemManager.GetInst().GetItemIconUrl(item.itemId), im.transform);
//                                 im.enabled = im.sprite != null;
//                                 im.transform.parent.gameObject.SetActive(true);
//                                 Text t = m_ItemCount[item.nPos];
//                                 t.text = "x" + item.nOverlap.ToString();
//                                 if (!m_ItemDict.ContainsKey(item.nPos))
//                                 {
//                                         m_ItemDict.Add(item.nPos, item);
//                                 }
//                         }
//                 }
//         }
// 
//         public void UpdateItem(Item item)
//         {
//                 if (item.nPos >= 0 && item.nPos < MAX_ITEM_NUM)
//                 {
//                         Image im = m_ItemImage[item.nPos];
//                         if (item.nOverlap > 0)
//                         {
//                                 ResourceManager.GetInst().LoadItemIcon(ItemManager.GetInst().GetItemIconUrl(item.itemId), im.transform);
//                                 im.enabled = im.sprite != null;
//                                 m_ItemCount[item.nPos].text = "x" + item.nOverlap.ToString();
//                         }
//                         else
//                         {
//                                 im.sprite = null;
//                                 im.enabled = false;
//                                 im.transform.parent.gameObject.SetActive(false);
//                                 m_ItemCount[item.nPos].text = "";
//                         }
// 
//                         if (!m_ItemDict.ContainsKey(item.nPos))
//                         {
//                                 m_ItemDict.Add(item.nPos, item);
//                         }
//                         else
//                         {
//                                 m_ItemDict[item.nPos] = item;
//                         }
//                 }
// 
//         }
// 
//         public void OnClickItem(GameObject go)
//         {
//                 if (!UIManager.GetInst().IsUIOpen("UI_Treasure"))
//                         return;
// 
//                 string name = go.name.Replace("itembg", "");
//                 int pos = -1;
//                 int.TryParse(name, out pos);
//                 if (pos >= 0 && pos < MAX_ITEM_NUM)
//                 {
//                         if (m_ItemDict.ContainsKey(pos))
//                         {
//                                 if (UIManager.GetInst().GetUIBehaviour<UI_Treasure>().AddDropItem(m_ItemDict[pos]))
//                                 {
//                                         m_ItemDict.Remove(pos);
// 
//                                         m_ItemImage[pos].sprite = null;
//                                         m_ItemImage[pos].enabled = false;
//                                         m_ItemCount[pos].text = "";
//                                 }
//                         }
//                         else if (m_ItemFromTreasure.ContainsKey(pos))
//                         {
//                                 if (UIManager.GetInst().GetUIBehaviour<UI_Treasure>().AddDropItem(m_ItemFromTreasure[pos]))
//                                 {
//                                         m_ItemFromTreasure.Remove(pos);
// 
//                                         m_ItemImage[pos].sprite = null;
//                                         m_ItemImage[pos].enabled = false;
//                                         m_ItemCount[pos].text = "";
//                                 }
// 
//                         }
//                 }
//         }
// 
//         public bool AddTakeItem(Item item)
//         {
//                 ItemConfig itemCfg = ItemManager.GetInst().GetItemCfg(item.itemId);
//                 if (itemCfg != null)
//                 {
//                         for (int i = 0; i < MAX_ITEM_NUM; i++)
//                         {
//                                 if (m_ItemImage[i].enabled == false)
//                                 {
//                                         if (!m_ItemDict.ContainsKey(i))
//                                         {
//                                                 m_ItemDict.Add(i, item);
//                                         }
//                                         ResourceManager.GetInst().LoadItemIcon(ItemManager.GetInst().GetItemIconUrl(item.itemId), m_ItemImage[i].transform);
//                                         m_ItemImage[i].enabled = true;
//                                         m_ItemCount[i].text = "x" + item.nOverlap.ToString();
//                                         m_ItemImage[i].transform.parent.gameObject.SetActive(true);
//                                         return true;
//                                 }
//                         }
//                 }
//                 return false;
//         }
// 
//         public bool AddTakeItem(string tmp)
//         {
//                 if (string.IsNullOrEmpty(tmp))
//                 {
//                         return false;
//                 }
// 
//                 int id = 0, count = 0;
//                 GameUtility.ParseItemStr(tmp, '&', ref id, ref count);
//                 ItemConfig itemCfg = ItemManager.GetInst().GetItemCfg(id);
//                 if (itemCfg != null)
//                 {
//                         for (int i = 0; i < MAX_ITEM_NUM; i++)
//                         {
//                                 if (m_ItemImage[i].enabled == false)
//                                 {
//                                         if (!m_ItemFromTreasure.ContainsKey(i))
//                                         {
//                                                 m_ItemFromTreasure.Add(i, tmp);
//                                         }
//                                         ResourceManager.GetInst().LoadItemIcon(ItemManager.GetInst().GetItemIconUrl(id), m_ItemImage[i].transform);
//                                         m_ItemImage[i].enabled = true;
//                                         m_ItemCount[i].text = "x" + count.ToString();
//                                         m_ItemImage[i].transform.parent.gameObject.SetActive(true);
//                                         return true;
//                                 }
//                         }
//                 }
//                 return false;
//         }
// 
//         public void SendTakeTreasure()
//         {
//                 if (m_ItemFromTreasure.Count > 0)
//                 {
//                         string info = "";
//                         foreach (var val in m_ItemFromTreasure)
//                         {
//                                 info += val.Value + "|";
// 
//                                 m_ItemImage[val.Key].sprite = null;
//                                 m_ItemImage[val.Key].enabled = false;
//                         }
//                         MazeManager.GetInst().SendTakeTreasure(info);
//                 }
//                 else
//                 {
//                         MazeManager.GetInst().SendTakeTreasure("0");
//                 }
//                 ClearTreasureItem();
//         }
// 
//         public void ClearTreasureItem()
//         {
//                 foreach (int key in m_ItemFromTreasure.Keys)
//                 {
//                         m_ItemImage[key].sprite = null;
//                         m_ItemImage[key].enabled = false;
//                 }
//                 m_ItemFromTreasure.Clear();
// 
//         }
// 
//         public void SetEventSelectItem(string item_open_condition)
//         {
//                 foreach (var pair in m_ItemDict)
//                 {
//                         if (pair.Value.itemId.ToString() == item_open_condition)
//                         {
//                                 GameObject obj = EffectManager.GetInst().GetEffectObj("effect_icon_point");
//                                 if (obj != null)
//                                 {
//                                         obj.transform.SetParent(m_ItemImage[pair.Key].gameObject.transform);
//                                         obj.transform.localPosition = new Vector3(0f, 1f, 0f);
//                                         Quaternion qua = new Quaternion();
//                                         qua.eulerAngles = new Vector3(0f, 90f, 0f);
//                                         obj.transform.localRotation = qua;
//                                         obj.transform.localScale = Vector3.one * 5f;
// 
//                                         //GameUtility.SetLayer(obj, "SceneFront");
//                                 }
// 
//                                 //m_ItemImage[pair.Key]
//                         }
//                 }
//         }
//