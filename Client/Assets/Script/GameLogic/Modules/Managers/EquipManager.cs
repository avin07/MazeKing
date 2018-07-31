using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Message;
class EquipManager : SingletonObject<EquipManager>
{
        Dictionary<int, EquipHoldConfig> m_EquipCfgDict = new Dictionary<int, EquipHoldConfig>();
        public void Init()
        {
                ConfigHoldUtility<EquipHoldConfig>.LoadXml("Config/equip", m_EquipCfgDict);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgEquip), OnGetEquip);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgEquipDel), OnEquipDel);
        }

        public EquipHoldConfig GetEquipCfg(int id)
        {
                if (m_EquipCfgDict.ContainsKey(id))
                {
                        return m_EquipCfgDict[id];
                }
                else
                {
                        singleton.GetInst().ShowMessage(ErrorOwner.designer, "装备表中没有id为" + id + "的装备");
                }
                return null;
        }


        Dictionary<long, Equip> m_EquipDict = new Dictionary<long, Equip>();

        public List<Equip> GetEquipInBag()
        {
                List<Equip> Equip_InBag = new List<Equip>();
                foreach (Equip equip in m_EquipDict.Values)
                {
                        if (equip.byPlace ==(int)ItemType.BAG_PLACE.MAIN)
                        {
                                Equip_InBag.Add(equip);
                        }
                }
                return Equip_InBag;
        }

        public List<Equip> GetEquipInBagBySort()
        {
                List<Equip> Equip_InBag = GetEquipInBag();
                Equip_InBag.Sort(CompareEquipSort);
                return Equip_InBag;
        }


        public Equip GetEquip(long id)
        {
                if (m_EquipDict.ContainsKey(id))
                {
                        return m_EquipDict[id];
                }
                return null;
        }

        public int GetEquipNumByID(int equip_id)
        {
                int num = 0;
                foreach (long id in m_EquipDict.Keys)
                {
                        if (m_EquipDict[id].equip_configid == equip_id)
                        {
                                num++;
                        }
                }
                return num;
        }
 
        public List<Equip> GetEquipInBagByPart(ItemType.EQUIP_PART ep, Pet pet)
        {

                if (ep == ItemType.EQUIP_PART.MAX)
                {
                        return GetEquipInBag();
                }


                List<Equip> Equip_By_Part = new List<Equip>();
                foreach (Equip equip in m_EquipDict.Values)
                {
                        if (equip.byPlace == (int)ItemType.BAG_PLACE.MAIN)
                        {
                                EquipHoldConfig ec = GetEquipCfg(equip.equip_configid);
                                if (ec != null)
                                {
                                        if (ec.place == (int)ep) //部位吻合//
                                        {
                                                if (pet == null)
                                                {
                                                        Equip_By_Part.Add(equip);
                                                }
                                                else
                                                {
                                                        string[] career = ec.need_career.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                                        for (int j = 0; j < career.Length; j++)
                                                        {
                                                                int career_id = int.Parse(career[j]);
                                                                if (career_id == pet.GetPropertyInt("career") || career_id <= 0) //职业吻合
                                                                {
                                                                        //if (pet.GetPropertyInt("level") >= ec.need_level) //等级吻合//
                                                                        //{
                                                                        Equip_By_Part.Add(equip);
                                                                        break;
                                                                        //}
                                                                }
                                                        }
                                                }
                                                       
                                        }                                       
                                }

                        }
                }
                return Equip_By_Part;
        }


        public List<Equip> GetEquipInBagByPartBySort(ItemType.EQUIP_PART ep, Pet pet = null)
        {
                List<Equip> Equip_By_Part = GetEquipInBagByPart(ep,pet);
                Equip_By_Part.Sort(CompareEquipSort);
                return Equip_By_Part;
        }

        protected int CompareEquipSort(Equip equipa, Equip equipb) //装备里先部位ID从小到大，再品质从大到小，再评分从大到小 //排序
        {
                if (null == equipa || null == equipb)
                {
                        return -1;
                }

                EquipHoldConfig eca = GetEquipCfg(equipa.equip_configid);
                EquipHoldConfig ecb = GetEquipCfg(equipb.equip_configid);

                int part_a = eca.place;
                int part_b = ecb.place;
                if (part_a != part_b)
                {
                        return part_a - part_b;
                }


                int qa_a = eca.quality;
                int qa_b = ecb.quality;
                if (qa_a != qa_b)
                {
                        return qa_b - qa_a;
                }

                int score_a = eca.score;
                int score_b = ecb.score;
                if (score_a != score_b)
                {
                        return score_b - score_a;
                }


                int levela = eca.need_level;
                int levelb = ecb.need_level;
                if (levela != levelb)
                {
                        return levelb - levela;
                }

                return (int)(equipb.id - equipa.id);

        }


        void OnGetEquip(object sender, SCNetMsgEventArgs e)
        {
                //id&idOwner&nEquipType&byPlace&nBagPos&unCreateTime}; // idOwner:所属者ID nEquipType:装备配置id；byPlace：位置；nBagPos：背包坐标；unCreateTime：创建
                SCMsgEquip msg = e.mNetMsg as SCMsgEquip;
                string[] equip_info = msg.info.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                if (equip_info.Length >= 5)
                {
                        long id = 0;
                        long.TryParse(equip_info[0], out id);
                        long idOwner = 0;
                        long.TryParse(equip_info[1], out idOwner);
                        int configid = 0;
                        int.TryParse(equip_info[2], out configid);
                        int place = (int)ItemType.BAG_PLACE.MAIN;
                        int.TryParse(equip_info[3], out place);
                        if (!m_EquipDict.ContainsKey(id))     //新得到
                        {
                                m_EquipDict.Add(id, new Equip(id, idOwner, configid, place));
                        }
                        else   //暂时用来处理穿脱//                                         
                        {

                                Pet old_pet = PetManager.GetInst().GetPet(m_EquipDict[id].owner_id);
                                if (old_pet != null)
                                {
                                        old_pet.SetMyEquip(null, (ItemType.EQUIP_PART)m_EquipDict[id].byPlace);
                                }
                                m_EquipDict[id].owner_id = idOwner;
                                m_EquipDict[id].equip_configid = configid;
                                m_EquipDict[id].byPlace = place;
                        }

                        //把装备存在对应的伙伴身上//
                        Pet pet = PetManager.GetInst().GetPet(idOwner);
                        if (pet != null)
                        {
                                pet.SetMyEquip(m_EquipDict[id], (ItemType.EQUIP_PART)(place));
                        }


                        UI_HeroMain uhm = UIManager.GetInst().GetUIBehaviour<UI_HeroMain>();
                        if (uhm != null)
                        {
                                uhm.UpdateHeroEquipUI();
                        }
                        

                        CommonDataManager.GetInst().UpdateBags(place);

                        if (place < (int)ItemType.EQUIP_PART.MAIN_HAND) 
                        {
                                string url = GetEquipCfg(configid).icon;
                                string name = LanguageManager.GetText(GetEquipCfg(configid).name);
                                UIRefreshManager.GetInst().AddDropItem(url, name, 1);
                        }
                        if (GameStateManager.GetInst().GameState > GAMESTATE.OTHER)
                        {
                                ItemManager.GetInst().AddToNewItemList(id);
                        }
                }
            
        }

        void OnEquipDel(object sender, SCNetMsgEventArgs e)
        {
                SCMsgEquipDel msg = e.mNetMsg as SCMsgEquipDel;
                if (m_EquipDict.ContainsKey(msg.idEquip))
                {
                        if (msg.idOwner != 0)
                        {
                                Pet pet = PetManager.GetInst().GetPet(msg.idOwner);
                                pet.DeleteMyEquip(msg.idEquip);
                                m_EquipDict.Remove(msg.idEquip);
                        }
                        else
                        {

                                int nPlace = GetEquip(msg.idEquip).byPlace;
                                m_EquipDict.Remove(msg.idEquip);
                                CommonDataManager.GetInst().UpdateBags(nPlace);
                        }
                }
                else
                {
                        Debuger.Log("装备id不存在" + msg.idEquip);
                }
            
        }

        public void SendPutOn(long id,long equip_id,int place)
        {
                CSMsgEquipMount msg = new CSMsgEquipMount();
                msg.idPet = id;
                msg.idEquip = equip_id;
                msg.byPlace = place;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

        public void SendTakeOff(long id, long equip_id)
        {
                CSMsgEquipUnmount msg = new CSMsgEquipUnmount();
                msg.idPet = id;
                msg.idEquip = equip_id;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }
}

public class Equip
{
        public long id;
        public int equip_configid;
        public long owner_id;
        public int byPlace;   //当前所在位置，0在背包，大于10就是穿在身上对应的部位//

        public Equip(long msgid, long msgOwenrid, int msgequipId, int msgnPlace)
        {
                id = msgid;
                equip_configid = msgequipId;
                byPlace = msgnPlace;
                owner_id = msgOwenrid;
        }
}
