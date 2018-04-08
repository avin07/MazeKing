//using UnityEngine;
//using System.Collections;
//using UnityEngine.UI;
//public class UI_MainMenu : UIBehaviour
//{
//        void Awake()
//        {
//            use_scaler_anim = false;
//            this.UILevel = UI_LEVEL.MAIN;
//        }



//        public void OnClickMyHome() //家园
//        {
//            HomeManager.GetInst().LoadHome();
//            UIManager.GetInst().CloseUI(this.name);
//        }



//        public void OnClickHeroPub() //酒馆
//        {
//            HeroPubManager.GetInst().GotoHub();
//            UIManager.GetInst().CloseUI(this.name);
//        }

//        public void OnClickTestRaid() 
//        {
//                CameraManager.GetInst().SetFadeCameraActive(false);
//                RaidManager.GetInst().SendEnterRaid();
//                UIManager.GetInst().CloseUI(this.name);
//        }

//        public void OnClickBag()  //临时包裹
//        {
//            UIManager.GetInst().ShowUI<UI_AllBag>("UI_AllBag").RefreshGroup(UI_AllBag.BAG_TAB.EQUIP);
//            UIManager.GetInst().CloseUI(this.name);
//        }
//}
