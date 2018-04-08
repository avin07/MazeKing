using UnityEngine;
using System.Collections;
using System.Collections.Generic;

class CharacterManager : SingletonObject<CharacterManager>
{
        Dictionary<int, AttributeConfig> m_AttributeDict = new Dictionary<int, AttributeConfig>();

        public void Init()
        {
                ConfigHoldUtility<CharacterConfig>.LoadXml("Config/charactar", m_CharacterDict);

                ConfigHoldUtility<Experience>.LoadXml("Config/experience", m_CharacterExperience);
                ConfigHoldUtility<RaceConfigHold>.LoadXml("Config/race", m_CharacterRace);
                ConfigHoldUtility<CareerConfigHold>.LoadXml("Config/career", m_CharacterCareer);
                ConfigHoldUtility<CareerFamilyConfig>.LoadXml("Config/career_family", m_CharacterCareerSys);
                ConfigHoldUtility<AttributeConfig>.LoadXml("Config/attributes", m_AttributeDict);
        }
        public string GetPropertyName(int id)
        {
                if (m_AttributeDict.ContainsKey(id))
                {
                        return m_AttributeDict[id].name;
                }
                return string.Empty;
        }

        public string GetPropertyMark(int id)
        {
                if (m_AttributeDict.ContainsKey(id))
                {
                        return m_AttributeDict[id].mark;
                }
                return string.Empty;
        }

        public Dictionary<int, AttributeConfig> GetAttributeDict()
        {
                return m_AttributeDict;
        }


        Dictionary<int, CharacterConfig> m_CharacterDict = new Dictionary<int, CharacterConfig>();
        public CharacterConfig GetCharacterCfg(int id)
        {
                if (m_CharacterDict.ContainsKey(id))
                {
                        return m_CharacterDict[id];
                }
                else
                {
                        singleton.GetInst().ShowMessage(ErrorOwner.designer, "角色表中没有id为" + id + "的角色");
                }
                return null;
        }



        public int GetCharacterModelId(int id)
        {
            if (m_CharacterDict.ContainsKey(id))
            {
                return m_CharacterDict[id].modelid;
            }
            return 0;
        }

        public string GetCharacterName(int id)
        {
                return LanguageManager.GetText(m_CharacterDict[id].name);
        }
        public string GetCharacterIcon(int id)
        {
                if (m_CharacterDict.ContainsKey(id))
                {
                        return ModelResourceManager.GetInst().GetIconRes(m_CharacterDict[id].modelid);
                }
                return string.Empty;
        }

        public float GetScale(int id)
        {
                return 1f;
        }

        public GameObject GenerateModel(CharacterConfig cfg)
        {
                if (cfg != null)
                {
                        GameObject mod = ModelResourceManager.GetInst().GenerateObject(cfg.modelid);
                        if (mod != null)
                        {
                                ModelResourceManager.GetInst().GenerateAccessory(mod, cfg.weapon_id);
                                ModelResourceManager.GetInst().GenerateAccessory(mod, cfg.cap_id);
                                ModelResourceManager.GetInst().GenerateAccessory(mod, cfg.accessories_id);
                                return mod;
                        }
                }
                return null;
        }

        public GameObject GenerateModel(int id)
        {
                return GenerateModel(GetCharacterCfg(id));
        }

        public List<CharacterConfig> GetRaceList(int race, List<CharacterConfig> list)
        {
                foreach (CharacterConfig cfg in m_CharacterDict.Values)
                {
                        if (cfg.race == race)
                        {
                                if (!list.Contains(cfg))
                                {
                                        list.Add(cfg);
                                }
                        }
                }
                return list;
        }


        Dictionary<int, Experience> m_CharacterExperience = new Dictionary<int, Experience>();

        public Experience GetCharacterExp(int level)
        {
                if (m_CharacterExperience.ContainsKey(level))
                {
                        return m_CharacterExperience[level];
                }
                return null;
        }

        public int CanLevelUp(long exp)  //能升到几级//
        {

                for (int i = 1; i < m_CharacterExperience.Count + 1; i++)
                {
                        if (exp < m_CharacterExperience[i].total_exp)
                        {
                                return i - 1;
                        }
                }
                return m_CharacterExperience.Count;
        }

        public int GetMaxLevel()
        {
                return m_CharacterExperience.Count;
        }

        Dictionary<int, RaceConfigHold> m_CharacterRace = new Dictionary<int, RaceConfigHold>();

        public string GetRaceBg(int id)
        {
                if (m_CharacterRace.ContainsKey(id))
                {
                        return m_CharacterRace[id].bg;
                }
                return string.Empty;
        }

        public string GetRaceName(int id)
        {
                if (m_CharacterRace.ContainsKey(id))
                {
                        return m_CharacterRace[id].name;
                }
                return string.Empty;
        }

        public int GetRaceNum()
        {
                return m_CharacterRace.Count;
        }


        Dictionary<int, CareerConfigHold> m_CharacterCareer = new Dictionary<int, CareerConfigHold>();

        public CareerConfigHold GetCareerDic(int id)  
        {
                if (m_CharacterCareer.ContainsKey(id))
                {
                        return m_CharacterCareer[id];
                }
                return null;
        }

        public string GetCareerIcon(int id)
        {
                if (m_CharacterCareer.ContainsKey(id))
                {
                        return m_CharacterCareer[id].icon;
                }
                return string.Empty;
        }

        Dictionary<int, CareerFamilyConfig> m_CharacterCareerSys = new Dictionary<int, CareerFamilyConfig>();

        public string GetCareerSysName(int id)
        {
                if (m_CharacterCareerSys.ContainsKey(id))
                {
                        return LanguageManager.GetText( m_CharacterCareerSys[id].name);
                }
                return string.Empty;
        }

        public string GetCareerSysIcon(int id)
        {
            if (m_CharacterCareerSys.ContainsKey(id))
            {
                return m_CharacterCareerSys[id].icon;
            }
            return string.Empty;
        }

        public int GetCareerFamilyCounter(int id, int counterId)
        {
                if (m_CharacterCareerSys.ContainsKey(id))
                {
                        switch (counterId)
                        {
                                case 1:
                                        return m_CharacterCareerSys[id].factor_vs_1;
                                case 2:
                                        return m_CharacterCareerSys[id].factor_vs_2;
                                case 3:
                                        return m_CharacterCareerSys[id].factor_vs_3;
                                case 4:
                                        return m_CharacterCareerSys[id].factor_vs_4;
                                case 5:
                                        return m_CharacterCareerSys[id].factor_vs_5;
                                case 6:
                                        return m_CharacterCareerSys[id].factor_vs_6;
                        }
                }
                return 100;
        }

        public int GetCareerSysNum()
        {
                return m_CharacterCareerSys.Count;
        }
}
