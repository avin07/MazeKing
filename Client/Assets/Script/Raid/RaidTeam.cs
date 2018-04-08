using UnityEngine;
using System.Collections;
using Message;

public class RaidTeam : PropertyBase
{
        public RaidTeam(SCMsgRaidTeam msg)
        {
                SetProperty("bright", msg.nBright);
                m_PropertyHandlers.Add("bright", OnBrightUpdate);
        }

        void OnBrightUpdate(string name, string oldval, string newval)
        {
                if (GameStateManager.GetInst().GameState == GAMESTATE.RAID_PLAYING)
                {
                        RaidManager.GetInst().UpdateBrightness();
                }
        }
}
