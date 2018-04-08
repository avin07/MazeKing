using UnityEngine;
using System.Collections;
using System.Collections.Generic;

class MonsterManager : SingletonObject<MonsterManager>
{
        Dictionary<int, MonsterConfig> m_MonsterDict = new Dictionary<int, MonsterConfig>();
        Dictionary<int, MonsterTeamConfig> m_MonsterTeamDict = new Dictionary<int, MonsterTeamConfig>();
        public void Init()
        {
                ConfigHoldUtility<MonsterConfig>.LoadXml("Config/monster", m_MonsterDict);
                ConfigHoldUtility<MonsterTeamConfig>.LoadXml("Config/monster_team", m_MonsterTeamDict);
        }

        public int GetMonsterCharacterId(int monsterId)
        {
                if (m_MonsterDict.ContainsKey(monsterId))
                {
                        return m_MonsterDict[monsterId].charactar_id;
                }
                return 0;
        }

        public List<int> GetMonsterList(int monsterTeamId)
        {
                List<int> list = new List<int>();
                if (m_MonsterTeamDict.ContainsKey(monsterTeamId))
                {
                        string[] tmps = m_MonsterTeamDict[monsterTeamId].monsterlist.Split(';');
                        foreach (string tmp in tmps)
                        {
                                if (string.IsNullOrEmpty(tmp))
                                        continue;

                                string[] infos = tmp.Split(',');
                                if (infos.Length == 2)
                                {
                                        int id = 0;
                                        int level = 0;
                                        int.TryParse(infos[0], out id);
                                        int.TryParse(infos[1], out level);

                                        list.Add(id);
                                }
                        }
                }
                return list;
        }


        //通过monster的id定位到model表 orz!!!!!!!!!!
        public int GetModelIdByMonsetId(int monster_id)
        {
            int charactar_id = MonsterManager.GetInst().GetMonsterCharacterId(monster_id);
            return CharacterManager.GetInst().GetCharacterModelId(charactar_id);
        }

}
