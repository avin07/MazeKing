//using UnityEngine;
//using UnityEngine.EventSystems;


//public class BuildDevelopBehaviour : BuildBaseBehaviour  //篝火
//{
//        public override void OnBuildClick(GameObject go, PointerEventData data)
//        {
//                if (HomeManager.GetInst().GetState() != HomeState.None)
//                {
//                        return;
//                }
//                //SelectAnima();

//                if (PlayerController.GetInst().GetPropertyInt("house_level") < GlobalParams.GetInt("campfire_open_level"))
//                {
//                        GameUtility.PopupMessage(string.Format(LanguageManager.GetText("client_notice_message_4"), GlobalParams.GetString("campfire_open_level")));
//                }
//                else
//                {
//                        UIManager.GetInst().ShowUI<UI_Pet>("UI_Pet");   
//                }
//        }


//        public override void SetBuild(BuildInfo build_info)
//        {
//                base.SetBuild(build_info);
//                ShowLockBar();
//        }


//        public void ShowLockBar()
//        {
//                if (PlayerController.GetInst().GetPropertyInt("house_level") < GlobalParams.GetInt("campfire_open_level"))
//                {
//                        BuildStateBar.ShowInfoTip(true, "Npc#suo");
//                }
//                else
//                {
//                        BuildStateBar.ShowInfoTip(false);
//                }
//        }



//}