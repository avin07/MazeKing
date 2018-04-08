//using UnityEngine;
//using System.Collections;
//using System;
//using System.Collections.Generic;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;


//public class UI_DiyRaidInfo : UIBehaviour
//{

//        public Text[] ResourceText;


//        public void Refresh()
//        {
//                for (int i = 0; i < ResourceText.Length; i++)
//                {
//                        ResourceText[i].text = "???";
//                }
//        }


//        public void RefreshResource(string info)
//        {
//                string[] count = info.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
//                if (!info.Equals("0"))
//                {
//                        for (int i = 0; i < ResourceText.Length; i++)
//                        {
//                                ResourceText[i].text = count[i];
//                        }
//                }

//        }

//        public void ShowFormation()
//        {
//                UI_RaidFormation uis = UIManager.GetInst().ShowUI<UI_RaidFormation>("UI_RaidFormation");
//                uis.Setup(this);
//                UIManager.GetInst().CloseUI("UI_WorldMap");
//        }


//        public override void OnClickClose(GameObject go)
//        {
//                base.OnClickClose(go);
//        }

        
//}

