using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Message;

class PetPieceManager : SingletonObject<PetPieceManager>
{
    Dictionary<long, PetPiece> m_PetPieceList = new Dictionary<long, PetPiece>();
    public void Init()
    {
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCPetMsgCompose), OnPetCompose);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCPetMsgPetPieceSync), OnGetPieceAll);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCPetMsgPetPiece), OnGetOnePiece);
    }

    void OnPetCompose(object sender, SCNetMsgEventArgs e)
    {
        SCPetMsgCompose msg = e.mNetMsg as SCPetMsgCompose;
        if (msg.error_type == 0)
        {
            //UIManager.GetInst().GetUIBehaviour<UI_Pet>().RefreshGroup(UI_Pet.PET_TAB.PIECE);
        }

    }

    void OnGetPieceAll(object sender, SCNetMsgEventArgs e)
    {
        SCPetMsgPetPieceSync msg = e.mNetMsg as SCPetMsgPetPieceSync;
        string info = msg.info; //{id&idConfig&overlap|id&idConfig&overlap|}
        string[] one = info.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < one.Length; i++)
        {
            string[] detail = one[i].Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            if (detail.Length == 3)
            {
//                PetPiece pp;
//                pp.character_id = int.Parse(detail[1]);
//                pp.num = int.Parse(detail[2]);
///*                pp.star = CharacterManager.GetInst().GetCharacterCfg(pp.character_id).GetPropInt("star");*/
//                long id = long.Parse(detail[0]);
//                pp.id = id;
//                m_PetPieceList.Add(id, pp);
            }
        }

    }

    void OnGetOnePiece(object sender, SCNetMsgEventArgs e)
    {
//        SCPetMsgPetPiece msg = e.mNetMsg as SCPetMsgPetPiece;
//        string[] detail = msg.info.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
//        if (detail.Length == 3)
//        {
//            PetPiece pp;
//            pp.character_id = int.Parse(detail[1]);
//            pp.num = int.Parse(detail[2]);
///*            pp.star = CharacterManager.GetInst().GetCharacterCfg(pp.character_id).GetPropInt("star"); */
//            long id = long.Parse(detail[0]);
//            pp.id = id;
//            if (m_PetPieceList.ContainsKey(id))
//            {
//                m_PetPieceList[id] = pp;
//            }
//            else
//            {
//                m_PetPieceList.Add(id, pp);
//            }
//        }
    }


    public List<PetPiece> GetMyPetPieceListSort()
    {
        List<PetPiece> piecelist = new List<PetPiece>();
        foreach (long id in m_PetPieceList.Keys)
        {
            piecelist.Add(m_PetPieceList[id]);
        }
        piecelist.Sort(ComparePiece);
        return piecelist;
    }

    public List<PetPiece> GetMyPetByCareerAndRaceSort(int career_sys, int race) //按照职业和种族分类分类//
    {
        List<PetPiece> piecelist = new List<PetPiece>();
        foreach (long id in m_PetPieceList.Keys)
        {
            PetPiece piece = m_PetPieceList[id];
            CharacterConfig cc = CharacterManager.GetInst().GetCharacterCfg(piece.character_id);

            if (career_sys <= 0)
            {
                if (race <= 0)
                {
                    piecelist.Add(piece);
                }
                else
                {
//                     if (cc.GetPropInt("race") == race)
//                     {
//                         piecelist.Add(piece);
//                     }

                }
            }
            else
            {
                //if (cc.GetPropInt("career_sys") == career_sys)
                //{
                //    if (race <= 0)
                //    {
                //        piecelist.Add(piece);
                //    }
                //    else
                //    {
                //        if (cc.GetPropInt("race") == race)
                //        {
                //            piecelist.Add(piece);
                //        }
                //    }
                //}
            }
        }
        piecelist.Sort(ComparePiece);
        return piecelist;

    }

    protected int ComparePiece(PetPiece peta, PetPiece petb) // 按星级>按等级 //排序
    {
        int mob_starsa = peta.star;
        int mob_starsb = petb.star;
        if (mob_starsa != mob_starsb)
        {
            return mob_starsb - mob_starsa;
        }
        return (int)(petb.character_id - peta.character_id);
    }

    public void SendPetCompose(long id)
    {
        CSMsgPetCompose msg = new CSMsgPetCompose();
        msg.piece_id = id;
        NetworkManager.GetInst().SendMsgToServer(msg);
    }

    public void SendPieceQue()
    {
        CSMsgPetPieceQuery msg = new CSMsgPetPieceQuery();
        NetworkManager.GetInst().SendMsgToServer(msg);
    }
      
}


struct PetPiece
{
    public long id;
    public int character_id;
    public int num;
    public int star; // 排序用//
}
