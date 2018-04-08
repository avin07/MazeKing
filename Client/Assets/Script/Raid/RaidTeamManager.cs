using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Message;
public class RaidTeamManager : SingletonObject<RaidTeamManager>
{
    public enum HERO_STATE
    {
        NONE = 0, // 无
        STEP_OUT = 1,  // 暂离
    }
    GameObject m_HeroRootObj;
    public GameObject HeroRootObj
    {
        get
        {
            if (m_HeroRootObj == null)
            {
                m_HeroRootObj = new GameObject("MY_HERO_ROOT");
            }
            return m_HeroRootObj;
        }
    }
    public HeroUnit MainHero
    {
        get
        {
            if (m_TeamHeroList.Count > 0)
            {
                return m_TeamHeroList[0];
            }
            return null;
        }
    }
    GameObject m_ClickEffect;
    void ShowClickEffect(Vector3 position)
    {
        if (m_ClickEffect != null)
        {
            GameObject.Destroy(m_ClickEffect);
        }

        m_ClickEffect = EffectManager.GetInst().GetEffectObj("effect_raid_move_click");
        if (m_ClickEffect != null)
        {
            m_ClickEffect.transform.position = new Vector3(position.x, 0.1f, position.z);
        }
    }

    List<HeroUnit> m_TeamHeroList = new List<HeroUnit>();
    Dictionary<int, int> m_LeaderSkills = new Dictionary<int, int>();
    Dictionary<long, RaidHero> m_RaidHeroDict = new Dictionary<long, RaidHero>();

    RaidTeam m_RaidTeam = null;
    public int GetTeamBright()
    {
        return m_RaidTeam.GetPropertyInt("bright");
    }
    public int GetTeamBrightLevel()
    {
        int val = m_RaidTeam.GetPropertyInt("bright");
        if (val > 100)
        {
            return 0;
        }
        else
        {
            return (100 - val) / 25;
        }
    }

    public Dictionary<int, int> GetLeaderSkills()
    {
        return m_LeaderSkills;
    }
    public RaidHero GetRaidHero(long id)
    {
        if (m_RaidHeroDict.ContainsKey(id))
        {
            return m_RaidHeroDict[id];
        }
        return null;
    }
    public void ClearHeroDict()
    {
        m_RaidHeroDict.Clear();
    }

    public void Init()
    {
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCRaidLeaderSkill), OnRaidLeaderSkill);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidHero), OnRaidHero);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidHeroAttr), OnRaidHeroAttr);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidHeroLeave), OnRaidHeroLeave);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidHeroJoin), OnRaidHeroJoin);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidTeam), OnRaidTeam);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCMsgRaidTeamAttr), OnRaidTeamAttr);
    }

    #region Message
    void OnRaidHero(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidHero msg = e.mNetMsg as SCMsgRaidHero;
        if (msg.idHero > 0 && !m_RaidHeroDict.ContainsKey(msg.idHero))
        {
            m_RaidHeroDict.Add(msg.idHero, new RaidHero(msg));
        }
    }
    void OnRaidHeroAttr(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidHeroAttr msg = e.mNetMsg as SCMsgRaidHeroAttr;
        if (m_RaidHeroDict.ContainsKey(msg.idHero))
        {
            m_RaidHeroDict[msg.idHero].OnUpdateProperty(msg.propname, msg.value);
        }
    }
    void OnRaidHeroJoin(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidHeroJoin msg = e.mNetMsg as SCMsgRaidHeroJoin;

        HeroUnit unit = GetHeroUnitByID(msg.idHero);
        if (unit == null)
        {
            RaidHero hero = GetRaidHero(msg.idHero);
            unit = AddTeamHero(m_TeamHeroList.Count, msg.idHero);
            if (unit != null)
            {
                unit.transform.position = MainHero.transform.position;
            }

            ResetFollowUnit();
        }
        UI_RaidMain uis = UIManager.GetInst().GetUIBehaviour<UI_RaidMain>();
        if (uis != null)
        {
            uis.SetupHero(GetHeroList());
        }
    }
    void OnRaidHeroLeave(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidHeroLeave msg = e.mNetMsg as SCMsgRaidHeroLeave;

        HeroUnit unit = GetHeroUnitByID(msg.idHero);
        if (unit != null)
        {
            m_TeamHeroList.Remove(unit);
            GameObject.Destroy(unit.gameObject);
            ResetFollowUnit();
        }
        UI_RaidMain uis = UIManager.GetInst().GetUIBehaviour<UI_RaidMain>();
        if (uis != null)
        {
            uis.SetupHero(GetHeroList());
        }
    }
    void OnRaidLeaderSkill(object sender, SCNetMsgEventArgs e)
    {
        SCRaidLeaderSkill msg = e.mNetMsg as SCRaidLeaderSkill;
        m_LeaderSkills = GameUtility.ParseCommonStringToDict(msg.info, '|', '&');
    }
    void OnRaidTeam(object sener, SCNetMsgEventArgs e)
    {
        SCMsgRaidTeam msg = e.mNetMsg as SCMsgRaidTeam;
        m_RaidTeam = new RaidTeam(msg);
    }
    void OnRaidTeamAttr(object sender, SCNetMsgEventArgs e)
    {
        SCMsgRaidTeamAttr msg = e.mNetMsg as SCMsgRaidTeamAttr;
        if (m_RaidTeam != null)
        {
            m_RaidTeam.OnUpdateProperty(msg.name, msg.value);
        }
    }
    #endregion
    #region TEAM_LIST
    public HeroUnit GetFirstHero()
    {
        if (m_TeamHeroList.Count > 0)
        {
            return m_TeamHeroList[0];
        }
        return null;
    }
    public bool HasSupporter()
    {
        foreach (HeroUnit unit in m_TeamHeroList)
        {
            if (unit.hero.PetID <= 0)
                return true;
        }
        return false;
    }
    public List<HeroUnit> GetHeroList()
    {
        return m_TeamHeroList;
    }
    public int GetHeroIndexById(long heroId)
    {
        for (int idx = 0; idx < m_TeamHeroList.Count; idx++)
        {
            if (m_TeamHeroList[idx].hero.ID == heroId)
            {
                return idx;
            }
        }
        return -1;
    }
    public int GetHeroIndex(HeroUnit unit)
    {
        return m_TeamHeroList.IndexOf(unit);
    }
    public HeroUnit GetHeroUnit(int idx)
    {
        if (idx < m_TeamHeroList.Count)
        {
            return m_TeamHeroList[idx];
        }
        return null;
    }
    public HeroUnit GetHeroUnitByID(long idPet)
    {
        return m_TeamHeroList.Find((x) =>
        {
            return x.ID == idPet;
        });
    }

    HeroUnit AddTeamHero(int index, long idHero)
    {
        RaidHero hero = GetRaidHero(idHero);
        if (hero == null)
        {
            Debuger.Log("Get Pet Error " + idHero);
            return null;
        }
        CharacterConfig cfg = hero.CharacterCfg;
        if (cfg != null)
        {
            GameObject unitObj = new GameObject();// CharacterManager.GetInst().CloneModel(pet.CharacterID);
            unitObj.transform.SetParent(HeroRootObj.transform);
            unitObj.name = "InvadeHero_" + hero.ID;
            GameUtility.SetLayer(unitObj, "Character");

            HeroUnit unit = unitObj.AddComponent<HeroUnit>();
            unit.CharacterID = hero.CharacterID;
            unit.ID = hero.ID;
            unit.hero = hero;
            unit.Index = index;
            hero.BelongUnit = unit;
            if (unit.IsAlive)
            {
                unit.LoadModel();
            }
            m_TeamHeroList.Add(unit);
            return unit;
        }
        return null;
    }

    void AddToTeamList(HeroUnit unit)
    {
        //列表里已有，则加在佣兵前面
        if (m_TeamHeroList.Count > 0)
        {
            int idx = m_TeamHeroList.FindIndex((x) =>
            {
                return x.hero.PetID <= 0;
            });
            if (idx >= 0)
            {
                m_TeamHeroList.Insert(idx, unit);
            }
            else
            {
                m_TeamHeroList.Add(unit);
            }
        }
        else
        {
            m_TeamHeroList.Add(unit);
        }
    }

    public List<HeroUnit> InitTeamMember(string teaminfo)
    {
        string[] matrixs = teaminfo.Split('|');

        for (int i = 0; i < matrixs.Length; i++)
        {
            if (string.IsNullOrEmpty(matrixs[i]) || matrixs[i].Equals(CommonString.zeroStr))
                continue;
            AddTeamHero(i, long.Parse(matrixs[i]));
        }

        foreach (HeroUnit unit in new List<HeroUnit>(m_TeamHeroList))
        {
            if (unit.hero.Hp <= 0)
            {
                MoveUnitToLast(unit);
            }
        }
        ResetFollowUnit();
        return m_TeamHeroList;
    }

    public void ClearTeam()
    {
        GameObject.Destroy(m_HeroRootObj);
        m_HeroRootObj = null;
        m_TeamHeroList.Clear();
    }
    public void RemoveHero(HeroUnit unit)
    {
        m_TeamHeroList.Remove(unit);
        ResetFollowUnit();
    }

    public void MoveUnitToLast(HeroUnit unit)
    {
        m_TeamHeroList.Remove(unit);
        m_TeamHeroList.Add(unit);
        //AddToTeamList(unit);
        ResetFollowUnit();
    }

    public void ResetFollowUnit()
    {
        if (m_TeamHeroList.Count > 0)
        {
            for (int i = 0; i < m_TeamHeroList.Count; i++)
            {
                if (i == 0)
                {
                    m_TeamHeroList[0].FollowingUnit = null;
                }
                else
                {
                    m_TeamHeroList[i].FollowingUnit = m_TeamHeroList[i - 1];
                }
            }
        }
    }

    int CompareTeamIndex(HeroUnit unit0, HeroUnit unit1)
    {
        return unit0.Index.CompareTo(unit1.Index);
    }

    public void SwitchUnit(HeroUnit nextcaptain, float time = 0f)
    {
        //if (index < m_TeamHeroList.Count)
        {
            //HeroUnit nextcaptain = m_TeamHeroList[index];
            List<HeroUnit> list = new List<HeroUnit>();
            list.Add(nextcaptain);
            m_TeamHeroList.Sort(CompareTeamIndex);
            for (int i = 0; i < m_TeamHeroList.Count; i++)
            {
                if (m_TeamHeroList[i].Index != nextcaptain.Index)
                {
                    list.Add(m_TeamHeroList[i]);
                }
            }
            m_TeamHeroList = list;
            ResetFollowUnit();
        }
    }
    #endregion TEAM_LIST
    #region TEAM_ACTION
    Vector3 GetHeroPosition(int index, Vector3 mainTargetPosition)
    {
        GameUtility.RotateTowards(MainHero.transform, mainTargetPosition);
        return mainTargetPosition + MainHero.transform.right * 2f * (index % 2) - MainHero.transform.forward * 2f * (index / 2);
    }

    public void TeamGotoBattle(RaidBattlePointBehav battlePoint, Quaternion qua)
    {
        for (int i = 0; i < m_TeamHeroList.Count; i++)
        {
            AppMain.GetInst().StartCoroutine(ProcessGotoBattle(m_TeamHeroList[i], battlePoint.posArray[i], qua));
        }
    }
    IEnumerator ProcessGotoBattle(HeroUnit unit, Vector3 pos, Quaternion qua)
    {
        unit.GoTo(pos, true);
        while (unit.PathComp.canMove)
        {
            yield return null;
        }
        unit.transform.rotation = qua;
    }
    public void TeamGoto(Vector3 position)
    {
        if (MainHero == null)
            return;

        int talkIdx = -1;
        if (MainHero.PathComp.canMove == false)
        {
            talkIdx = UnityEngine.Random.Range(0, m_TeamHeroList.Count);
        }

        GameUtility.RotateTowards(MainHero.transform, position);

        for (int i = 0; i < m_TeamHeroList.Count; i++)
        {
            if (i == talkIdx)
            {
                RaidManager.GetInst().UnitTalk(TALK_TYPE.MOVE_START, m_TeamHeroList[i]);
            }
            AppMain.GetInst().StartCoroutine(ProcessTeamGoto(m_TeamHeroList[i], GetHeroPosition(i, position), i * 0.1f));
        }
        ShowClickEffect(position);
    }
    IEnumerator ProcessTeamGoto(HeroUnit unit, Vector3 position, float delay)
    {
        yield return new WaitForSeconds(delay);
        unit.GoTo(position);
    }
    public void TeamSetPosition(Vector3 position)
    {
        for (int i = 0; i < m_TeamHeroList.Count; i++)
        {
            m_TeamHeroList[i].transform.position = GetHeroPosition(i, position);
        }
    }
    public void TeamRotateTo(Vector3 position)
    {
        foreach (HeroUnit unit in RaidTeamManager.GetInst().GetHeroList())
        {
            GameUtility.RotateTowards(unit.transform, position);
        }
    }
    public void TeamStop()
    {
        foreach (HeroUnit unit in m_TeamHeroList)
        {
            unit.StopSeek();
        }
    }
    #endregion TEAM_ACTION

    public void SetupTeam()
    {
        m_RaidTeam = new RaidTeam(200);

    }
}
