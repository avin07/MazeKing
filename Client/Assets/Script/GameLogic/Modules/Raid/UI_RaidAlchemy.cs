using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class UI_RaidAlchemy : UIBehaviour
{
        List<Image> m_ItemIconList = new List<Image>();
        int m_nMaxItemCount = 3;
        List<DropObject> m_ItemList = new List<DropObject>();
        RaidNodeBehav m_BelongNode;
        Image m_IconRet;
        void Awake()
        {
                for (int i = 0; i < 3; i++)
                {
                        Image iconbg = (GetImage("icon" + i));
                        if (iconbg != null)
                        {
                                m_ItemIconList.Add(GetImage(iconbg.gameObject, "icon"));
                        }
                        m_ItemList.Add(null);
                }

                m_IconRet = GetImage(GetImage("icon_ret").gameObject, "icon");
        }

        public override void OnClose(float time)
        {
                base.OnClose(time);
                NetworkManager.GetInst().WakeUp();
        }
        public override void OnClickClose(GameObject go)
        {
                base.OnClickClose(go);
                UIManager.GetInst().CloseUI("UI_RaidBag");
        }

        public void Setup(RaidNodeBehav node)
        {
                m_nMaxItemCount = 3;
                m_BelongNode = node;
        }

        public void OnClickIcon(GameObject go)
        {
                int i = int.Parse(go.name.Replace("icon", ""));
                if (i < m_ItemList.Count)
                {
                        m_ItemIconList[i].sprite = null;
                        m_ItemIconList[i].enabled = false;
                        UI_RaidBag uis = UIManager.GetInst().GetUIBehaviour<UI_RaidBag>();
                        uis.RecoverItem(m_ItemList[i]);
                        m_ItemList[i] = null;
                }
        }

        public bool AddItem(DropObject di)
        {
                for (int i = 0; i < m_nMaxItemCount; i++)
                {
                        if (m_ItemList[i] == null)
                        {
                                ResourceManager.GetInst().LoadIconSpriteSyn(di.GetIconName(), m_ItemIconList[i].transform);
                                m_ItemList[i] = di;
                                return true;
                        }
                }
                return false;
        }
        public void SetResult(string info)
        {
                string[] infos = info.Split('|');
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
                                di.nOverlap = int.Parse(tmps[2]);

                                ResourceManager.GetInst().LoadIconSpriteSyn(di.GetIconName(), m_IconRet.transform);

                                for (int i = 0; i < m_ItemIconList.Count; i++)
                                {
                                        m_ItemIconList[i].enabled = false;
                                }

                                return;
                        }
                }
        }
        public void OnClickConfirm()
        {
                string info = "";
                for (int i = 0; i < m_ItemList.Count; i++)
                {
                        if (m_ItemList[i] != null)
                        {
                                info += m_ItemList[i].idCfg + "&" + m_ItemList[i].nType + "|";
                        }
                }
                if (info != "")
                {
                        RaidManager.GetInst().SendAlchemy(info, m_BelongNode);
                }
                GetButton("confirm").enabled = false;
        }
}