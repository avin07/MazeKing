using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class UI_VillagerIcon : UIBehaviour 
{
        public Image m_Icon;
        public Button m_Button;
        public GameObject m_BaseGroup;

        public Canvas m_Canvas;
        public RectTransform m_BaseRT;
        Vector3 m_WorldPos;
        RaidManager.OpFinishHandler m_FinishHandler;
        object m_HandlerData;
 
        VillagerBehaviour m_BelongVillager;
        public void Setup(VillagerBehaviour villager)
        {
                m_BelongVillager = villager;
                if (villager.elemCfg != null) 
                {
                        if(villager.elemCfg.task_npc_id != 0)
                        {
                                int taskId = 0;
                                int taskNpcState = TaskManager.GetInst().CheckTaskNpcState(villager.elemCfg.task_npc_id, ref taskId);
                                switch (taskNpcState)
                                {
                                        case 0: //npc配置不存在//
                                                return;
                                        case 1: //有任务可以交付//
                                                SetIcon("Npc#wenhao");
                                                break;
                                        case 2: //有任务可以接取//
                                                SetIcon("Npc#tanhao");
                                                break;
                                        case 3: //有任务已接取但没完成//
                                                SetIcon("Npc#wenhao", true);
                                                break;
                                        case 4: //有任务可以接但未达到条件//
                                                SetIcon("Npc#tanhao", true);
                                                break;
                                        case 5: //以上情况都不存在//
                                                if(!string.IsNullOrEmpty(villager.elemCfg.head_icon))
                                                {
                                                        SetIcon(villager.elemCfg.head_icon);
                                                } //换回元素配置图标/
                                                break;
                                }
                        }
                        else
                        {
                                if(!string.IsNullOrEmpty(villager.elemCfg.head_icon))
                                {
                                        SetIcon(villager.elemCfg.head_icon);
                                }
                        }
                }

                m_WorldPos = villager.transform.position + Vector3.up * 1.6f;
        }


        public void SetIcon(string url, bool bGray = false)
        {
                ResourceManager.GetInst().LoadIconSpriteSyn(url, m_Icon.transform);
                m_Icon.SetNativeSize();
                UIUtility.SetImageGray(bGray, m_Icon.transform);
        }


        void Update()
        {
                if (m_BaseGroup == null)
                        return;
       
                if (Camera.main != null)
                {
                        m_BaseRT.anchoredPosition = UIUtility.WorldToCanvas(m_Canvas, m_WorldPos);
                }
        }
        public void OnClickButton()
        {
                VillageManager.GetInst().SelectVillager(m_BelongVillager);
        }
}
