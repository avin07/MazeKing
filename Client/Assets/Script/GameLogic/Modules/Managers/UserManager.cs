using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Message;
public class UserManager : SingletonObject<UserManager>
{
    public void Init()
    {
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCUserAttrUpdate), OnUpdateProperty);
        MsgMgr.GetInst().RegisterMsgHandler(typeof(SCFormationUpdate), OnUpdateFormation);
    }

    public void OnUpdateProperty(object sender, SCNetMsgEventArgs e)
    {
        SCUserAttrUpdate msg = e.mNetMsg as SCUserAttrUpdate;
        if (msg.ID == PlayerController.GetInst().PlayerID)
        {
            PlayerController.GetInst().OnUpdateProperty(msg.Name, msg.Value);
        }
    }

    Dictionary<int, List<long>> m_FormationDict = new Dictionary<int, List<long>>();
    public List<long> GetFormation(int type)
    {
        if (m_FormationDict.ContainsKey(type))
        {
            return m_FormationDict[type];
        }
        return new List<long>();
    }
    void OnUpdateFormation(object sender, SCNetMsgEventArgs e)
    {
        SCFormationUpdate msg = e.mNetMsg as SCFormationUpdate;
        if (!m_FormationDict.ContainsKey(msg.type))
        {
            m_FormationDict.Add(msg.type, new List<long>());
        }
        m_FormationDict[msg.type].Clear();
        if (!string.IsNullOrEmpty(msg.strPetList))
        {
            foreach (string tmp in msg.strPetList.Split('|'))
            {
                if (string.IsNullOrEmpty(tmp))
                    continue;
                long petId = long.Parse(tmp);
                m_FormationDict[msg.type].Add(petId);
            }
        }
    }
}
