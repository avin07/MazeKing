using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class UI_RaidEventCurse : UIBehaviour
{
        public Text m_TextTitle;
        public Text m_TextDesc;

        public void OnClickConfirm()
        {
                //RaidManager.GetInst().ConfirmOpSend(0, RaidManager.GetInst().MainHero, 0);
                UIManager.GetInst().CloseUI(this.name);
        }
        public void SetCurse(RaidElemConfig cfg)
        {
                if (cfg != null)
                {
                        m_TextTitle.text = LanguageManager.GetText(cfg.name);
                        m_TextDesc.text = LanguageManager.GetText(cfg.desc);
                }
        }
}
