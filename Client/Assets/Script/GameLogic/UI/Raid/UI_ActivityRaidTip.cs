using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class UI_ActivityRaidTip : UIBehaviour
{
        public Image head;
        public Text text;

        public void Refresh(int id)
        {
                RaidInfoHold raid_info = RaidConfigManager.GetInst().GetRaidInfoCfg(id);
                if (raid_info != null)
                {
                        text.text = LanguageManager.GetText(raid_info.info_dialog);
                }
        }

        public void OnClickOk()
        {
                Queue<int> m_queue = WorldMapManager.GetInst().GetTipQue();
                if (m_queue.Count > 0)
                {
                        Refresh(m_queue.Dequeue());
                }
                else
                {
                        OnClickClose(null);
                }
        }
}
