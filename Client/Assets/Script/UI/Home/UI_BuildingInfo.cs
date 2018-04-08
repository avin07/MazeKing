using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class UI_BuildingInfo : UIBehaviour
{
        void Awake()
        {
                IsFullScreen = true;
                FindComponent<Button>("bg/close").onClick.AddListener(OnClickClose);
        }

        public void Refresh(BuildInfo info)
        {
                UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", false);
                UIManager.GetInst().ShowUI<UI_HomeMain>("UI_HomeMain");
                FindComponent<Text>("bg/name").text = LanguageManager.GetText(info.buildCfg.name);
                FindComponent<Text>("bg/level").text = info.level + "级";
                FindComponent<Text>("bg/des").text = LanguageManager.GetText(info.buildCfg.desc);
                FindComponent<Text>("bg/funcdes").text = string.Empty;

        }

        public void OnClickClose()
        {
                UIManager.GetInst().CloseUI(this.name);
                UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", true);
        }
}

