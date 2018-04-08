using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_CampEvent : UIBehaviour
{
        public Image m_Pic;
        public Image m_Icon;
        public Text m_Desc;


        public void Setup(RaidCampEventConfig cfg, HeroUnit unit, string param)
        {
                string text = LanguageManager.GetText(cfg.desc);
                if (cfg.range_type == 1)
                {
                        if (unit != null)
                        {
                                string icon = ModelResourceManager.GetInst().GetIconRes(unit.hero.CharacterCfg.modelid);
                                ResourceManager.GetInst().LoadIconSpriteSyn(icon, m_Icon.transform);
                                if (text.Contains("{0}"))
                                {
                                        text = string.Format(text, LanguageManager.GetText(unit.hero.CharacterCfg.name));
                                }
                        }
                }

                if (cfg.id == 3)
                {
                        if (text.Contains("{0}"))
                        {
                                string tmpText = "";
                                string[] itemstrs = param.Split('|');
                                foreach (string tmp in itemstrs)
                                {
                                        if (string.IsNullOrEmpty(tmp))
                                                continue;
                                        string[] tmps = tmp.Split('&');
                                        if (tmps.Length == 2)
                                        {
                                                DropObject dObj= new DropObject();
                                                dObj.nType = int.Parse(tmps[0]);
                                                dObj.idCfg =int.Parse(tmps[1]);

                                                tmpText += dObj.GetName() + CommonString.commaStr;
                                        }
                                }
                                if (tmpText.Length > 0 && tmpText[tmpText.Length - 1] == ',')
                                {
                                        tmpText = tmpText.Remove(tmpText.Length - 1);
                                }
                                text = string.Format(text, tmpText);
                        }
                }
                m_Desc.text = text;
                ResourceManager.GetInst().LoadIconSpriteSyn(cfg.picture, m_Pic.transform);
        }

        public void OnClickNext()
        {
                CampManager.GetInst().ExitCamp();
                UIManager.GetInst().CloseUI(this.name);
                NetworkManager.GetInst().WakeUp();
        }

        void Update()
        {
                if (InputManager.GetInst().GetInputUp(true))
                {
                        OnClickNext();
                }
        }
}
