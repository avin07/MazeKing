using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;



public class UI_NotifyIcon : UIBehaviour
{

        Button btn;
        HashSet<int> BtnOnSet = new HashSet<int>();

        void Awake()
        {
                this.UILevel = UI_LEVEL.MAIN;
                btn = FindComponent<Button>("content/btn");
                btn.gameObject.SetActive(false);
        }

        public void ShowNewTip(NotifyType type, Action<object> onClick, object data)
        {
                int typeNum = (int)type;
                if (BtnOnSet.Contains(typeNum))
                {
                        return;
                }
                else
                {
                        GameObject newBtn = CloneElement(btn.gameObject);
                        newBtn.SetActive(true);
                        newBtn.GetComponent<Button>().onClick.AddListener(() => ClickBtn(typeNum,onClick, data));
                        BtnOnSet.Add(typeNum);
                }

        }

        void ClickBtn(int typeNum, Action<object> onClick, object data)
        {
                onClick(data);
                GameObject.Destroy(btn);
                BtnOnSet.Remove(typeNum);

                if (BtnOnSet.Count == 0)
                {
                        UIManager.GetInst().CloseUI(this.name);
                }
               
        }
}


