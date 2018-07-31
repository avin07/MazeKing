using UnityEngine;
using System;

class ScreenLog : SingletonObject<ScreenLog>
{
        public void AddLog()
        {

        }

        bool bShowLog = true;
        public bool EnableSSAO = true;
        public void _OnGUI()
        {


                
//                  GUI.Label(new Rect(20, 40, 100, 100), Screen.currentResolution.width + " " + Screen.currentResolution.height);
                 
// 
//                 Texture2D tex = new Texture2D(1,1);
//                 tex.SetPixel(0,0, Color.black);
//                 GUI.DrawTexture(new Rect(0f, Screen.height / 2f, Screen.width, 1f), tex);
//                 GUI.DrawTexture(new Rect(Screen.width / 2f, 0f, 1f, Screen.height), tex);

//                 if (bShowLog)
//                 {
//                         GUI.Label(new Rect(200, 60, 100, 100), Camera.main.transform.position.ToString());
//                         GUI.Label(new Rect(200, 90, 100, 100), Camera.main.transform.rotation.eulerAngles.ToString());
//                 }
//                 if (GUI.Button(new Rect(0, 400, 100, 100), EnableSSAO.ToString()))
//                 {
//                         EnableSSAO = !EnableSSAO;
//                         SSAOPro ssao = Camera.main.GetComponent<SSAOPro>();
//                         if (ssao != null)
//                         {
//                                 ssao.enabled = EnableSSAO;
//                         }
//                 }
        }

        public void Update()
        {
#if UNITY_STANDALONE
                if (Input.GetKeyDown(KeyCode.P))
                {
                        bShowLog = !bShowLog;
                }

                if (Input.GetKey(KeyCode.LeftShift))
                {
                        if (Input.GetKeyUp(KeyCode.C))
                        {
                            if (UIManager.GetInst().IsUIVisible("UI_Chat"))
                                {
                                        UIManager.GetInst().CloseUI("UI_Chat");
                                }
                                else
                                {
                                        UIManager.GetInst().ShowUI<UI_Chat>(("UI_Chat"));
                                }
                        }
                }
                
                 if (Input.GetKeyUp(KeyCode.F1))
                 {
                     if (UIManager.GetInst().IsUIVisible("UI_Chat"))
                     {
                         UIManager.GetInst().CloseUI("UI_Chat");
                     }
                     else
                     {
                         UIManager.GetInst().ShowUI<UI_Chat>(("UI_Chat"));
                     }
                 }

#endif
        }
}