using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroManager : SingletonObject<HeroManager>
{
    Dictionary<long, Hero> m_HeroDict = new Dictionary<long, Hero>();
    Dictionary<int, CareerConfigHold> m_CareerDict = new Dictionary<int, CareerConfigHold>();
    public void Init()
    {
        ConfigHoldUtility<CareerConfigHold>.LoadXml("Config/career", m_CareerDict);

        for (int i = 0; i < 4; i++)
        {
            //CreateNewHero((int)i, i, 1);
        }
    }

    public CareerConfigHold GetCareerCfg(int id)
    {
        if (m_CareerDict.ContainsKey(id))
        {
            return m_CareerDict[id];
        }
        return null;
    }

    public string GetCareerIcon(int id)
    {
        if (m_CareerDict.ContainsKey(id))
        {
            return m_CareerDict[id].icon;
        }
        return string.Empty;
    }

    public Hero GetHero(long id)
    {
        if (m_HeroDict.ContainsKey(id))
        {
            return m_HeroDict[id];
        }
        return null;
    }

    public void ClearHeroDict()
    {
        m_HeroDict.Clear();
    }

    public void SaveAllHero()
    {
    }

    public void CreateNewHero(long id, int character, int career, int level)
    {
        CharacterConfig characterCfg = CharacterManager.GetInst().GetCharacterCfg(character);
        CareerConfigHold careerCfg = HeroManager.GetInst().GetCareerCfg(career);
        if (characterCfg != null && careerCfg != null)
        {
            Hero hero = new Hero(careerCfg, level);
            hero.ID = id;
        }
    }
}
