using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class UI_RaidEditorCreate : UIBehaviour 
{
        const int PIECE_SIZE = 12;
        public InputField m_MapId;
        public InputField m_MapType;

        public InputField m_InitFloor;
        public InputField m_WallHeight;
        public InputField m_InitWall;
        public InputField m_InitDoor_EW;
        public InputField m_InitDoor_NS;

        public Toggle isFixed;

        public InputField m_InitSize;

        void Awake()
        {
                this.gameObject.AddComponent<InputNav>();
        }

        public void OnClickConfirm()
        {
                if (string.IsNullOrEmpty(m_MapId.text))
                {
                        GameUtility.PopupMessage("请输入地图ID");
                        return;
                }

                if (string.IsNullOrEmpty(m_MapType.text))
                {
                        GameUtility.PopupMessage("请输入地图类型");
                }

                
                int wallHeight = 0;
                int.TryParse(m_WallHeight.text, out wallHeight);
                int door_NS = 0;
                int.TryParse(m_InitDoor_NS.text, out door_NS);

                int door_EW = 0;
                int.TryParse(m_InitDoor_EW.text, out door_EW);

                int size = 0;
                int.TryParse(m_InitSize.text, out size);
                RaidEditor.GetInst().CreateMap(int.Parse(m_MapType.text), size, size, m_InitFloor.text, m_InitWall.text, door_EW, door_NS, wallHeight);

                UI_RaidEditor uis = RaidEditor.GetInst().m_UIEditor;
                if (uis != null)
                {
                        uis.SetMapInfo(m_MapId.text, m_MapType.text, isFixed.isOn, size, size);
                }
                UIManager.GetInst().CloseUI(this.name);
        }

        public void OnClickCancel()
        {
                UIManager.GetInst().CloseUI(this.name);
        }
}