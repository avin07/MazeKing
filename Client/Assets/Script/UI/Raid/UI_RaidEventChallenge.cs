using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class UI_RaidEventChallenge : UIBehaviour
{
        public Text m_TextTitle;
        public Image m_MonsterIcon;
        public Image m_ItemIcon;
        void Awake()
        {
                m_ItemIcon.gameObject.SetActive(false);
        }
        public void OnClickConfirm()
        {
                //RaidManager.GetInst().ConfirmOpSend(0, RaidManager.GetInst().MainHero, 0);
                UIManager.GetInst().CloseUI(this.name);
        }
        public void SetChallenge(RaidElemConfig cfg)
        {
                if (cfg != null)
                {
                        string icon = ModelResourceManager.GetInst().GetIconRes(cfg.mainModel);
                        ResourceManager.GetInst().LoadIconSpriteSyn(icon, m_MonsterIcon.transform);
                        m_TextTitle.text = LanguageManager.GetText(cfg.name);

//                        int idx = 0;
//                         if (!string.IsNullOrEmpty(cfg.reward))
//                         {
//                                 string[] rewards = cfg.reward.Split(';');
//                                 foreach (string rewardstr in rewards)
//                                 {
//                                         if (string.IsNullOrEmpty(rewardstr))
//                                                 continue;
// 
//                                         string[] tmps = rewardstr.Split(',');
//                                         if (tmps.Length >= 3)
//                                         {
//                                                 DropObject dObj = new DropObject();
//                                                 int.TryParse(tmps[0], out dObj.nType);
//                                                 int.TryParse(tmps[1], out dObj.idCfg);
//                                                 int.TryParse(tmps[2], out dObj.nOverlap);
// 
//                                                 GameObject itemObj = CloneElement(m_ItemIcon.gameObject, "icon" + idx);
//                                                 ResourceManager.GetInst().LoadIconSpriteSyn(dObj.GetIconName(), itemObj.transform);
//                                                 idx++;
//                                         }
//                                 }
//                         }
                }
        }
}
