using UnityEngine;
using System.Collections;

public class UI_RaidTaskFinish : UIBehaviour
{
        public void OnClickContinue()
        {
                UIManager.GetInst().CloseUI(this.name);
        }
        public void OnClickExit()
        {
                UIManager.GetInst().CloseUI(this.name);
                RaidManager.GetInst().SendLeave();
                //UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox").SetConfirmAndCancel("", "确定退出迷宫吗？", ConfirmLeave, null, null);
        }

        void ConfirmLeave(object data)
        {
                RaidManager.GetInst().SendLeave();
        }
}
