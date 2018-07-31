using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class UI_WorldMap : UIBehaviour
{

        void Awake()
        {
                this.UILevel = UI_LEVEL.MAIN;
        }

        public void ReturnHome()
        {
                GameStateManager.GetInst().GameState = GAMESTATE.OTHER; //锁操作
                UIManager.GetInst().CloseUI(this.name);
                WorldMapManager.GetInst().SetWorldMapActive(false);
                HomeManager.GetInst().SetHomeActive(true);

                AudioManager.GetInst().PlaySE("SE_UI_Button2");
        }
        public override void OnClose(float time)
        {
                base.OnClose(time);
                WorldMapManager.GetInst().SetWorldMapActive(false);
        }

        public override void OnShow(float time)
        {                
                HomeManager.GetInst().SetHomeActive(false);                
                WorldMapManager.GetInst().SetWorldMapActive(true);
                base.OnShow(time);
                RefreshInfo();
        }


        void RefreshInfo()
        {
                GetText("name").text = PlayerController.GetInst().GetPropertyValue("name");
                RefreshExp();
                RefreshLevel();
        }

        public void RefreshLevel()
        {
                GetText("level").text = PlayerController.GetInst().GetPropertyValue("house_level");
        }

        public void RefreshExp()
        {
                int level = PlayerController.GetInst().GetPropertyInt("house_level");
                int exp = PlayerController.GetInst().GetPropertyInt("house_exp");
                HomeExpHold heh = CommonDataManager.GetInst().GetHomeExpCfg(level + 1);
                if (heh != null)
                {
                        GetImage("exp").fillAmount = (float)exp / heh.need_exp;
                }
        }


}
