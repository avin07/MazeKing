using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class VillagerBehaviour : MonoBehaviour, IPointerClickHandler
{
        public int Index;
        public int CharacterID;
        public CharacterConfig CharacterCfg;
        public VillageElementConfig elemCfg;
        void Awake()
        {
                CharacterController cc = this.gameObject.AddComponent<CharacterController>();
                cc.center = Vector3.up;
                cc.height = 2f;
        }
        public GameObject Mod;
        public void SetModel()
        {
                Mod = CharacterManager.GetInst().GenerateModel(CharacterCfg);
                if (Mod != null)
                {
                        Mod.transform.SetParent(this.transform);
                        Mod.transform.localPosition = Vector3.zero;
                        GameUtility.ObjPlayAnim(Mod, CommonString.idle_001Str, true);
                }
        }

        public void OnPointerClick(PointerEventData data)
        {
                VillageManager.GetInst().SelectVillager(this);
                Debuger.Log(this.CharacterID);
        }
        UI_VillagerIcon m_UIElemIcon;
        UI_VillagerIcon UIElemIcon
        {
                get
                {
                        if (m_UIElemIcon == null)
                        {
                                GameObject uiObj = UIManager.GetInst().ShowUI_Multiple<UI_VillagerIcon>("UI_VillagerIcon");
                                uiObj.transform.SetParent(this.transform);
                                uiObj.name = "ElemIcon_" + this.Index;
                                GameUtility.SetLayer(uiObj, "UI");
                                m_UIElemIcon = uiObj.GetComponent<UI_VillagerIcon>();
                                m_UIElemIcon.Setup(this);
                        }
                        return m_UIElemIcon;
                }
        }


        public void SetUIVisible(bool bVisible)
        {
                if (UIElemIcon != null)
                {
                        UIElemIcon.gameObject.SetActive(bVisible);
                }
        }


        public void RefreshTaskIcon()
        {
                if (UIElemIcon != null)
                {
                        if (elemCfg.task_npc_id != 0)
                        {
                                m_UIElemIcon.Setup(this);
                        }
                }
        }
}
