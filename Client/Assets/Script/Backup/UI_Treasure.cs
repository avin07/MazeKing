// using UnityEngine;
// using System.Collections;
// using UnityEngine.UI;
// using System.Collections.Generic;
// public class UI_Treasure : UIBehaviour
// {
//         Button m_BtnConfirm;
//         Button m_BtnCancel;
//         Text m_TextTitle, m_TextDesc;
//         List<Image> m_ItemImage = new List<Image>();
//         List<Text> m_ItemCount= new List<Text>();
//         Vector2 m_vItemOffset;
// 
//         void Awake()
//         {
//                 m_BtnConfirm = GetButton("confirm");
//                 m_BtnCancel = GetButton("cancel");
//                 m_TextTitle = GetText("title");
//                 m_TextDesc = GetText("desc");
// 
//                 for (int i = 0; i < 6; i++)
//                 {
//                         m_ItemImage.Add(GetImage("item" + i));
//                         m_ItemCount.Add(GetText("count" + i));
//                         m_ItemCount[i].text = "";
//                 }
//                 m_vItemOffset.x = m_ItemImage[1].rectTransform.position.x - m_ItemImage[0].rectTransform.position.x;
//                 m_vItemOffset.y = m_ItemImage[3].rectTransform.position.y - m_ItemImage[0].rectTransform.position.y;
//         }
// 
//         public void OnClickConfirm(GameObject go)
//         {
//                 UIManager.GetInst().CloseUI(this.name);
// 
//                 UIManager.GetInst().GetUIBehaviour<UI_Bag>().ClearTreasureItem();
//                 MazeManager.GetInst().SendTakeTreasure(m_sOriginalTreasure);
//                 MazeEventManager.GetInst().FinishTrigger();
//         }
// 
//         public void OnClickCancel(GameObject go)
//         {
//                 UIManager.GetInst().CloseUI(this.name);
// 
//                 if (m_BagItemDict.Count > 0)
//                 {
//                         string info = "";
//                         foreach (Item item in m_BagItemDict.Values)
//                         {
//                                 info += item.id + "|";
//                         }
//                         MazeManager.GetInst().SendDropBagItem(info);
//                 }
//                 UIManager.GetInst().GetUIBehaviour<UI_Bag>().SendTakeTreasure();
// 
//                 MazeEventManager.GetInst().FinishTrigger();
//         }
// 
//         public void OnClickItem(GameObject go)
//         {
//                 string name = go.name.Replace("bg", "");
//                 int pos = -1;
//                 int.TryParse(go.name.Replace("itembg", ""), out pos);
//                 if (m_ItemInfoDict.ContainsKey(name))
//                 {
//                         if (UIManager.GetInst().GetUIBehaviour<UI_Bag>().AddTakeItem(m_ItemInfoDict[name]))
//                         {
//                                 m_ItemInfoDict.Remove(name);
// 
//                                 if (pos >= 0 && pos < m_ItemImage.Count)
//                                 {
//                                         m_ItemImage[pos].sprite = null;
//                                         m_ItemImage[pos].enabled = false;
//                                         m_ItemCount[pos].text = "";
//                                 }
//                         }
//                 }
//                 else if (m_BagItemDict.ContainsKey(name))
//                 {
//                         if (UIManager.GetInst().GetUIBehaviour<UI_Bag>().AddTakeItem(m_BagItemDict[name]))
//                         {
//                                 m_BagItemDict.Remove(name);
// 
//                                 if (pos >= 0 && pos < m_ItemImage.Count)
//                                 {
//                                         m_ItemImage[pos].sprite = null;
//                                         m_ItemImage[pos].enabled = false;
//                                         m_ItemCount[pos].text = "";
//                                 }
//                         }
// 
//                 }
//         }
// 
//         public bool AddDropItem(Item item)
//         {
//                 ItemConfig itemCfg = ItemManager.GetInst().GetItemCfg(item.itemId);
//                 if (itemCfg != null)
//                 {
//                         for (int i = 0; i < 6; i++)
//                         {
//                                 if (m_ItemImage[i].enabled == false)
//                                 {
//                                         if (!m_BagItemDict.ContainsKey(m_ItemImage[i].name))
//                                         {
//                                                 m_BagItemDict.Add(m_ItemImage[i].name, item);
//                                         }
// 
//                                         Image im = m_ItemImage[i];
//                                         ResourceManager.GetInst().LoadItemIcon(ItemManager.GetInst().GetItemIconUrl(item.itemId), im.transform);
//                                         im.enabled = im.sprite != null;
//                                         m_ItemCount[i].text = "x" + item.nOverlap.ToString();
//                                         return true;
//                                 }
//                         }
//                 }
//                 return false;
//         }
// 
//         public bool AddDropItem(string tmp)
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
//                         for (int i = 0; i < 6; i++)
//                         {
//                                 if (m_ItemImage[i].enabled == false)
//                                 {
//                                         if (!m_ItemInfoDict.ContainsKey(m_ItemImage[i].name))
//                                         {
//                                                 m_ItemInfoDict.Add(m_ItemImage[i].name, tmp);
//                                         }
// 
//                                         Image im = m_ItemImage[i];
//                                         ResourceManager.GetInst().LoadItemIcon(ItemManager.GetInst().GetItemIconUrl(id), im.transform);
//                                         im.enabled = im.sprite != null;
//                                         m_ItemCount[i].text = "x" + count.ToString();
//                                         return true;
//                                 }
//                         }
//                 }
//                 return false;
//         }
//         public override void OnShow()
//         {
//                 base.OnShow();
//                 m_BagItemDict.Clear();
//                 m_ItemInfoDict.Clear();
//         }
// 
//         Dictionary<string, string> m_ItemInfoDict = new Dictionary<string, string>();
//         Dictionary<string, Item> m_BagItemDict = new Dictionary<string, Item>();
//         string m_sOriginalTreasure;
//         public void SetupItems(string itemstr)
//         {
//                 m_sOriginalTreasure = itemstr;
//                 string[] tmps = itemstr.Split('|');
//                 int idx = 0;
//                 foreach (string tmp in tmps)
//                 {
//                         if (string.IsNullOrEmpty(tmp))
//                                 continue;
//                         int id = 0, count = 0;
//                         GameUtility.ParseItemStr(tmp, '&', ref id, ref count);
//                         ItemConfig itemCfg = ItemManager.GetInst().GetItemCfg(id);
//                         if (itemCfg != null)
//                         {
//                                 if (idx < m_ItemImage.Count)
//                                 {
//                                         ResourceManager.GetInst().LoadItemIcon(ItemManager.GetInst().GetItemIconUrl(id), m_ItemImage[idx].transform);
//                                         m_ItemImage[idx].enabled = m_ItemImage[idx].sprite != null;
//                                         if (!m_ItemInfoDict.ContainsKey(m_ItemImage[idx].name))
//                                         {
//                                                 m_ItemInfoDict.Add(m_ItemImage[idx].name, tmp);
//                                         }
//                                         m_ItemCount[idx].text = count.ToString();
//                                 }
//                                 idx++;
//                         }
//                 }
//         }
// 
//         Transform m_TargetObj;
//         public void SetTargetObj(Transform trans)
//         {
//                 m_TargetObj = trans;
//         }
//         TeamBehav m_OpenActor;
//         Vector3 m_OriActorPos;
//         public void SetOpenActor(TeamBehav actorTb, Vector3 pos)
//         {
//                 m_OpenActor = actorTb;
//                 m_OriActorPos = pos;
//         }
//