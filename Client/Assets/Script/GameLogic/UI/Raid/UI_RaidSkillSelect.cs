using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class UI_RaidSkillSelect : UIBehaviour
{
        public GameObject m_Bg;
        public Text m_TextTitle;
        public Text m_TextDesc;
        public Text m_TextSkillDesc;
        public Image m_SkillIcon;
        public Image m_ItemIcon;
        public GameObject m_Player;

        List<GameObject> m_PlayerObjList = new List<GameObject>();
        void Awake()
        {
                for (int i = 0; i < 5; i++)
                {
                        m_PlayerObjList.Add(GetGameObject("player" + i));
                }
        }

        public override void OnShow(float time)
        {
                base.OnShow(time);
                GameUtility.EnableCameraRaycaster(false);
        }
        public override void OnClose(float time)
        {
                base.OnClose(time);
                GameUtility.EnableCameraRaycaster(true);

                UI_RaidBag uibag = UIManager.GetInst().GetUIBehaviour<UI_RaidBag>();
                if (uibag != null)
                {
                        uibag.RecoverSkillSelect();
                }
        }

        RaidNodeBehav m_Node;
        int m_AvailSkillCfgId;
        List<HeroUnit> m_HeroList = new List<HeroUnit>();
        public void Setup(RaidNodeBehav node)
        {
                m_AvailSkillCfgId = 0;
                m_Node = node;
                m_TextTitle.text = LanguageManager.GetText(node.elemCfg.name);
                m_TextDesc.text = LanguageManager.GetText(node.elemCfg.desc);
                m_TextSkillDesc.text = LanguageManager.GetText(node.elemCfg.operation_desc);

                m_SkillIcon.sprite = null;
                m_ItemIcon.sprite = null;

                m_SelectTool = null;
                SetupHero();
                m_HeroList = new List<HeroUnit>(RaidTeamManager.GetInst().GetHeroList());
                m_nCurrentIndex = 0;

                Dictionary<int, int> dict = GameUtility.ParseCommonStringToDict(m_Node.elemCfg.need_tool_id, ';', ',');
                if (dict.Count > 0)
                {
                        GetGameObject("additem").SetActive(true);

                }
                else
                {
                        GetButton("itembtn").interactable = false;
                        GetGameObject("additem").SetActive(false);
                }
                UI_RaidBag uibag = UIManager.GetInst().GetUIBehaviour<UI_RaidBag>();
                if (uibag != null)
                {
                        uibag.SetupSkillSelect(dict);
                }
        }
        int m_nCurrentIndex = 0;
        HeroUnit m_SelectUnit;
        void SetupHero()
        {
                List<HeroUnit> herolist = RaidTeamManager.GetInst().GetHeroList();
                for (int i = 0; i < m_PlayerObjList.Count; i++)
                {
                        GameObject heroObj = m_PlayerObjList[i];
                        if (i < herolist.Count)
                        {
                                HeroUnit unit = herolist[i];
                                if (unit.IsAlive == false)
                                {
                                        heroObj.SetActive(false);
                                        continue;
                                }
                                heroObj.SetActive(true);
                                
                                CharacterConfig cfg = CharacterManager.GetInst().GetCharacterCfg(unit.CharacterID);
                                string icon = ModelResourceManager.GetInst().GetIconRes(cfg.model_id);

                                Image playerIcon = GetImage(heroObj, "playericon");
                                playerIcon.enabled = true;
                                ResourceManager.GetInst().LoadIconSpriteSyn(icon, playerIcon.transform);
                                UIUtility.SetImageGray(!unit.IsAlive, playerIcon.transform);

                                EventTriggerListener.Get(heroObj).onClick = OnClickHero;
                                EventTriggerListener.Get(heroObj).SetTag(unit);

                /*                                GetImage(heroObj, "hp").fillAmount = unit.hero.Hp / (float)unit.hero.MaxHp;*/
                //                GetText(heroObj, "hp_value").text = unit.hero.Hp + "/" + unit.hero.MaxHp;
                //             SetPressure(heroObj, unit.hero.Pressure);
                SetSkillAvailable(unit, heroObj);

                                if (unit == RaidManager.GetInst().MainHero)
                                {
                                        SelectHero(heroObj);
                                }
                        }
                        else
                        {
                                heroObj.SetActive(false);
                        }
                }
        }
        void SetSkillAvailable(HeroUnit unit, GameObject heroObj)
        {
                bool bAvail = false;
//                 if (unit.hero.GetAdvSkillId() == m_Node.elemCfg.right_adventure_skill)
//                 {
//                         bAvail = true;
//                 }
//                 else
//                 {
//                         AdventureSkillConfig advSkillCfg = unit.hero.GetAdvSkillCfg();
//                         if (advSkillCfg != null && advSkillCfg.effect_element_type.Contains(m_Node.elemCfg.type))
//                         {
//                                 bAvail = true;
//                         }
//                 }
                GetGameObject(heroObj, "skill_available").SetActive(bAvail);
                if (bAvail)
                {
                        UIUtility.SetUIEffect(this.name, heroObj, bAvail, "effect_element_choose_hero");
                }
        }

        public void SetPressure(GameObject obj, int pressure)
        {
                int nowCount = pressure / 10;
                for (int i = 0; i < 20; i++)
                {
                        GetImage(obj, "pressure" + i).enabled = i < nowCount;
                }
                GetText(obj, "pressure_value").text = pressure + "/200";
        }
        public void OnClickHero(GameObject go, PointerEventData data)
        {
                SelectHero(go);
        }

        public void SelectHero(GameObject heroObj)
        {
                heroObj.GetComponent<Toggle>().isOn = true;
                m_SelectUnit = (HeroUnit)EventTriggerListener.Get(heroObj).GetTag();
                if (m_SelectUnit != null)
                {
                        m_SkillIcon.enabled = false;
                        GetText(m_SkillIcon.gameObject, "skill_name").text = "";
                        if (m_SelectUnit != null)
                        {
//                                 SkillLearnConfigHold cfg = m_SelectUnit.hero.GetAdvSkill();
//                                 if (cfg != null)
//                                 {
//                                         m_AvailSkillCfgId = cfg.id;
//                                         m_SkillIcon.enabled = true;
//                                         ResourceManager.GetInst().LoadIconSpriteSyn(cfg.icon, m_SkillIcon.transform);
//                                         GetText(m_SkillIcon.gameObject, "skill_name").text = LanguageManager.GetText(cfg.name);
//                                 }
                        }
                }
                RaidManager.GetInst().SwitchCaptain(m_SelectUnit);
        }

        public void OnClickClose()
        {
                RaidManager.GetInst().ClearTargetNode();
                RaidManager.GetInst().ClearSelectEffect();
                UIManager.GetInst().CloseUI(this.name);
        }

        public void OnClickConfirm()
        {
                RaidManager.GetInst().ClearSelectEffect();
                RaidManager.GetInst().OperateCommonElem(1, m_SelectTool != null ? m_SelectTool.idCfg : 0);
                UIManager.GetInst().CloseUI(this.name);
        }

        public void OnClickLeft()
        {
                m_nCurrentIndex--;
                if (m_nCurrentIndex < 0)
                {
                        m_nCurrentIndex = m_HeroList.Count - 1;
                }

                int idx = RaidTeamManager.GetInst().GetHeroIndexById(m_HeroList[m_nCurrentIndex].hero.ID);
                RaidManager.GetInst().SwitchCaptain(idx);
        }
        public void OnClickRight()
        {
                m_nCurrentIndex++;
                if (m_nCurrentIndex >= m_HeroList.Count)
                {
                        m_nCurrentIndex = 0;
                }

                int idx = RaidTeamManager.GetInst().GetHeroIndexById(m_HeroList[m_nCurrentIndex].hero.ID);
                RaidManager.GetInst().SwitchCaptain(idx);
        }

        public void OnClickItem()
        {
        }

        DropObject m_SelectTool;
        public void SetSelectItem(DropObject di)
        {
                m_SelectTool = di;
                if (di != null)
                {
                        string iconname = di.GetIconName();
                        if (iconname != "")
                        {
                                ResourceManager.GetInst().LoadIconSpriteSyn(iconname, m_ItemIcon.transform);
                        }
                        GetGameObject("additem").SetActive(false);

                        for (int i = 0; i < m_PlayerObjList.Count; i++)
                        {
                                GameObject heroObj = m_PlayerObjList[i];
                                if (heroObj.activeSelf)
                                {
                                        HeroUnit unit = (HeroUnit)EventTriggerListener.Get(heroObj).GetTag();
//                                         AdventureSkillConfig advSkillCfg = unit.hero.GetAdvSkillCfg();
//                                         if (advSkillCfg != null && advSkillCfg.type == 3)
//                                         {
//                                                 GetGameObject(heroObj, "skill_available").SetActive(true);
//                                                 UIUtility.SetUIEffect(this.name, heroObj, true, "effect_element_choose_hero");
//                                         }
//                                         else
//                                         {
//                                                 //UIUtility.SetUIEffect(this.name, heroObj, false, "effect_element_choose_hero");
//                                         }
                                }
                        }
                }
                else
                {
                        m_ItemIcon.enabled = false;
                        m_ItemIcon.sprite = null;
                        GetGameObject("additem").SetActive(true);
                }
        }
}
