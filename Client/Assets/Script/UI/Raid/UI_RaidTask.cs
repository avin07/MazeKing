using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class UI_RaidTask : UIBehaviour
{
        public GameObject m_RequireIcon;
        public GameObject m_RewardIcon;
        public Text m_RequireText;
        public Text m_TaskDesc;
        public Button m_BtnSubmit;
        public Image m_TaskFinish;
        int m_nTaskId;
        RaidTaskConfig m_TaskCfg;
        public void SetTask(RaidNodeBehav node)
        {
                if (node.elemCfg != null)
                {
                        m_nTaskId = node.elemCfg.raid_task_id * 100 + RaidManager.GetInst().GetCurrentRaidLevel();
                        m_TaskCfg = RaidConfigManager.GetInst().GetTaskConfig(m_nTaskId);
                        //Debuger.Log(m_nTaskId);
                        if (m_TaskCfg != null)
                        {
                                m_TaskDesc.text = LanguageManager.GetText(m_TaskCfg.detail);

                                if (m_TaskCfg.target_id.Count > 0 && m_TaskCfg.req_quantity.Count > 0)
                                {
                                        DropObject targetItem = new DropObject();
                                        targetItem.idCfg = m_TaskCfg.target_id[0];
                                        targetItem.nType = (int)Thing_Type.ITEM;

                                        ResourceManager.GetInst().LoadIconSpriteSyn(targetItem.GetIconName(), GetImage(m_RequireIcon, "icon").transform);

                                        UpdateTaskCount();
                                }

                                int idx = 0;
                                if (!string.IsNullOrEmpty(m_TaskCfg.reward_item_list))
                                {
                                        string[] infos = m_TaskCfg.reward_item_list.Split(';');
                                        foreach (string info in infos)
                                        {
                                                if (string.IsNullOrEmpty(info))
                                                        continue;

                                                DropObject di = new DropObject(info, ',');
                                                if (idx == 0)
                                                {
                                                        SetReward(di, m_RewardIcon);
                                                }
                                                else
                                                {
                                                        SetReward(di, CloneElement(m_RewardIcon));
                                                }
                                        }
                                }
                        }
                }
        }

        public void UpdateTaskCount()
        {
                int current = 0;
                if (m_TaskCfg != null && m_TaskCfg.target_id.Count > 0)
                {
                        RAID_TASK_INFO taskinfo = TaskManager.GetInst().GetBranchTask(m_TaskCfg.id);
                        if (taskinfo != null)
                        {
                                Dictionary<int, int> dict = GameUtility.ParseCommonStringToDict(taskinfo.countstr, '|', '&');
                                if (dict.ContainsKey(m_TaskCfg.target_id[0]))
                                {
                                        current = dict[m_TaskCfg.target_id[0]];
                                }

                                m_BtnSubmit.gameObject.SetActive(taskinfo.state == 0);
                                m_TaskFinish.gameObject.SetActive(taskinfo.state == 1);
                        }
                }
                m_RequireText.text = current + CommonString.divideStr + m_TaskCfg.req_quantity[0];
        }

        void SetReward(DropObject di, GameObject obj)
        {
                ResourceManager.GetInst().LoadIconSpriteSyn(di.GetIconName(), GetImage(obj, "icon").transform);
        }

        public void OnClickSubmit()
        {
                TaskManager.GetInst().SendTaskSubmit(m_nTaskId);
                OnClickClose(null);
        }
}
