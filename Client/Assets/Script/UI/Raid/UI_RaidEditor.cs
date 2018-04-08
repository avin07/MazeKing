using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

class UI_RaidEditor : UIBehaviour
{
        public GameObject m_UIGroup;
        public GameObject m_Content;
        public GameObject m_BuildItem0;

        public InputField m_IdField, m_SizeXField, m_SizeYField, m_MapTypeField, m_ElemIdField,  m_RotField,m_ElemYField;
        public Toggle m_ToggleFixedMap, m_ToggleRandomMap, m_ToggleTerrain;
        public Toggle m_ToggleRaid, m_ToggleHome;
        public ToggleGroup m_ToggleGroup;
        public Text m_MapHeight;
        public GameObject m_FloorObj0;
        public Button m_BtnSetSize;

        public List<GameObject> m_BuildItemList = new List<GameObject>();
        List<GameObject> m_FloorList = new List<GameObject>();
        int m_nCount;
        Dictionary<string, object> m_IconBuildDict = new Dictionary<string, object>();
        const int PIECE_SIZE = 12;
        Toggle[,] m_NodeToggles = new Toggle[PIECE_SIZE, PIECE_SIZE];
        void Awake()
        {
                m_BuildItemList.Add(m_BuildItem0);
                m_FloorList.Add(m_FloorObj0);
                GameObject toggleRoot = GetGameObject("nodetoggles");
                Toggle toggle0 = GetToggle(toggleRoot, "toggle0");
                m_NodeToggles[0, 0] = toggle0;
                for (int y = 0; y < PIECE_SIZE; y++)
                {
                        for (int x = 0; x < PIECE_SIZE; x++)
                        {
                                if (x == 0 && y == 0)
                                        continue;

                                GameObject toggle_new = CloneElement(toggle0.gameObject, "toggle" + (x * 100 + y).ToString());
                                toggle_new.transform.SetParent(toggleRoot.transform);
                                toggle_new.transform.localScale = Vector3.one;
                                m_NodeToggles[x, y] = toggle_new.GetComponent<Toggle>();
                                GetText(toggle_new, "groupIdx").text = "";
                        }
                }
        }
        GameObject GetIconGroup()
        {
                GameObject icongroup;
                if (m_nCount < m_BuildItemList.Count)
                {
                        icongroup = m_BuildItemList[m_nCount];
                }
                else
                {
                        icongroup = Object.Instantiate(m_BuildItem0.gameObject) as GameObject;
                        icongroup.name = "buildGroup" + m_nCount;
                        icongroup.transform.SetParent(m_BuildItem0.transform.parent);
                        icongroup.transform.localScale = Vector3.one;
                        m_BuildItemList.Add(icongroup);
                }
                EventTriggerListener.Get(icongroup).onDown = OnIconPress;
                return icongroup;
        }

        public void SetPage(int page)
        {
                Debuger.Log("SetPage " + page);
                StartCoroutine(SetIcons(page));
        }

        int m_nPage = 0;
        IEnumerator SetIcons(int page)
        {
                yield return null;
                m_nPage = page;
                m_IconBuildDict.Clear();
                m_nCount = 0;
                foreach (GameObject obj in m_BuildItemList)
                {
                        obj.SetActive(false);
                }

                switch (page)
                {
                        case 0:

                                List<ModelConfig> list = ModelResourceManager.GetInst().GetAllCommonBricks();
                                foreach (ModelConfig cfg in list)
                                {
                                        GameObject icongroup = GetIconGroup();
                                        icongroup.SetActive(true);
                                        GetText(icongroup, "buildname").text = cfg.mark;
                                        m_IconBuildDict.Add(icongroup.name, cfg);
                                        m_nCount++;
                                }
                                break;
                        case 1:
                                foreach (RaidElemConfig cfg in RaidConfigManager.GetInst().m_RaidElemDict.Values)
                                {
                                        GameObject icongroup = GetIconGroup();
                                        icongroup.SetActive(true);
                                        GetText(icongroup, "buildname").text = cfg.mark;
                                        string iconurl = ModelResourceManager.GetInst().GetIconRes(cfg.mainModel);

                                        //GetImage(icongroup, "buildicon").sprite = ResourceManager.GetInst().LoadSprite("Sprite/" + iconurl);
                                        m_IconBuildDict.Add(icongroup.name, cfg);
                                        m_nCount++;
                                }
                                break;
                }
        }

        public void OnClickToggle()
        {
                m_UIGroup.SetActive(!m_UIGroup.activeSelf);
        }

        public void OnClickNew()
        {
                UIManager.GetInst().ShowUI<UI_RaidEditorCreate>("UI_RaidEditorCreate");
        }

        public void OnClickDelete(GameObject go)
        {
                RaidEditor.GetInst().DeleteSelected();
        }
        public void OnClickSave(GameObject go)
        {
                RaidEditor.GetInst().SaveToXml();
        }
        public void OnClickLoadAll()
        {
                RaidEditor.GetInst().LoadAllXml();
        }
        
        public void OnClickLoad(GameObject go)
        {
                RaidEditor.GetInst().LoadMap();
        }
        public void OnClickCopy(GameObject go)
        {
                switch (go.name)
                {
                        case "copy_down":
                                RaidEditor.GetInst().CopyObject(0, -1);
                                break;
                        case "copy_up":
                                RaidEditor.GetInst().CopyObject(0, 1);
                                break;
                        case "copy_right":
                                RaidEditor.GetInst().CopyObject(1, 0);
                                break;
                        case "copy_left":
                                RaidEditor.GetInst().CopyObject(-1,0);
                                break;
                }
        }

        public void OnClickPage(GameObject go)
        {
                int page = int.Parse(go.name.Replace("btnpage", ""));
                SetPage(page);
        }
        public void OnIconPress(GameObject go, PointerEventData data)
        {
                if (m_IconBuildDict.ContainsKey(go.name))
                {
                        switch (m_nPage)
                        {
                                case 0:
                                        RaidEditor.GetInst().SelectBrick(m_IconBuildDict[go.name] as ModelConfig);
                                        break;
                                case 1:
                                        RaidEditor.GetInst().StartPlaceBuilding(m_IconBuildDict[go.name] as RaidElemConfig);
                                        break;
                        }
                }
        }

        public void OnToggleShowTerrain(GameObject go)
        {
                RaidEditor.GetInst().EditorTerrain.SetActive(m_ToggleTerrain.isOn);
        }

        public void OnClickSetSize(GameObject go)
        {
                int sizex = 0;
                int sizey = 0;
                int.TryParse(m_SizeXField.text, out sizex);
                int.TryParse(m_SizeYField.text, out sizey);
                if (sizex > 0 && sizey > 0)
                {
                        RaidEditor.GetInst().SetTerrainSize(sizex, sizey);
                }
                else
                {
                        GameUtility.PopupMessage("请输入地形大小");
                }
        }

        public bool SetMapInfo(string mapIdText, string typeText, bool bFixedMap, int sizeX, int sizeY)
        {
                m_IdField.text = mapIdText;
                m_SizeXField.text = sizeX.ToString();
                m_SizeYField.text = sizeY.ToString();
                m_MapTypeField.text = typeText;
                m_ToggleFixedMap.isOn = bFixedMap;
                m_ToggleRandomMap.isOn = !bFixedMap;

                return true;
        }
        public void OnClickSetElemId()
        {
                int elemId = 0;
                int.TryParse(m_ElemYField.text, out elemId);
                RaidEditor.GetInst().SetCurrentElemId(elemId);
        }
        public void OnClickSetRot()
        {
                int rot = 0;
                int.TryParse(m_RotField.text, out rot);
                RaidEditor.GetInst().SetCurrentElemRot(rot);
        }
        public void OnClickElemY()
        {
                int posY = 0;
                int.TryParse(m_ElemYField.text, out posY);
                RaidEditor.GetInst().SetCurrentElemY(posY);
        }
        public void OnClickSetFloorId(GameObject go)
        {
                int idx = int.Parse(go.name.Replace("floor", ""));
                RaidEditor.GetInst().SetCurrentFloor(idx, GetInputField(go, "floorId_TF").text);
        }
        public void OnClickAddFloorHeight()
        {
                RaidEditor.GetInst().ChangeFloorHeight(1);
        }
        public void OnClickMinusFloorHeight()
        {
                RaidEditor.GetInst().ChangeFloorHeight(-1);
        }
        public void SetNode(RaidNodeBehav node)
        {
                GetText("nodeid").text = node.id.ToString();
                if (node.elemObj != null)
                {
                        m_RotField.text = node.elemObj.transform.rotation.y.ToString();
                        m_ElemYField.text = node.elemObj.transform.position.y.ToString();
                }
                else
                {
                        m_RotField.text = "0";
                        m_ElemYField.text = "0";
                }
                GetInputField("elemId_TF").text = node.ElemId.ToString();
                foreach (GameObject obj in m_FloorList)
                {
                        obj.SetActive(false);
                }
                if (node.floorList != null)
                {
                        for (int i = 0; i < node.floorList.Count; i++)
                        {
                                GameObject obj = null;
                                if (i < m_FloorList.Count)
                                {
                                        obj = m_FloorList[i];
                                        
                                }
                                else
                                {
                                        obj = CloneElement(m_FloorObj0);
                                        obj.name = "floor" + i;
                                        m_FloorList.Add(obj);
                                }
                                obj.SetActive(true);
                                GetInputField(obj, "floorId_TF").text = node.floorList[i].ToString();
                        }
                }
        }

        public void UpdateCameraPos()
        {
                GetText("camerapos").text = Camera.main.transform.position.ToString();
                GetText("camerarot").text = Camera.main.transform.rotation.eulerAngles.ToString();
        }
        void Update()
        {
                UpdateCameraPos();
        }

        public void OnClickUpdateAll()
        {
                RaidEditor.GetInst().UpdateAllXml();
        }
        public void OnClickConvertToServer()
        {
                RaidEditor.GetInst().ConvertToServerXml();//UpdateAllXml();
        }
        public void OnClickBirdView()
        {
                Camera.main.fieldOfView = 10f;
                Camera.main.farClipPlane = 1000f;
                Camera.main.transform.position = new Vector3(3.5f, 75f, 5.5f);
                Camera.main.transform.rotation = Quaternion.Euler(new Vector3(90f, 0f, 0f));
        }
        public void OnClickSideView(GameObject go)
        {
                Camera.main.fieldOfView = 40f;
                Camera.main.farClipPlane = 1000f;
                Camera.main.transform.position = new Vector3(13f, 16f, -5f);
                Camera.main.transform.rotation = Quaternion.Euler(new Vector3(52f, 315f, 0f));
        }
        
        public void OnClickToggle(int index)
        {
                if (m_ToggleGroup.AnyTogglesOn())
                {
                        RaidEditor.GetInst().CurrentGroupIdx = index;
                        GetGameObject("nodetoggles").SetActive(true);
                }
                else
                {
                        GetGameObject("nodetoggles").SetActive(false);
                        RaidEditor.GetInst().CurrentGroupIdx = 0;
                }
        }
        public void RefreshAllnodeToggle()
        {
                Dictionary<int, RaidNodeBehav> dict = RaidEditor.GetInst().m_NodeDict;
                foreach (RaidNodeBehav node in dict.Values)
                {
                        int x = node.id / 100;
                        int y = node.id % 100;
                        if (x < PIECE_SIZE && y < PIECE_SIZE)
                        {
                                if (node.groupId > 0)
                                {
                                        m_NodeToggles[x, y].isOn = true;
                                        GetText(m_NodeToggles[x, y].gameObject, "groupIdx").text = node.groupId.ToString();

                                }
                                else
                                {
                                        GetText(m_NodeToggles[x, y].gameObject, "groupIdx").text = "";
                                        m_NodeToggles[x, y].isOn = false;
                                }
                        }
                }
        }

        public void OnClickNodeToggle(Toggle toggle)
        {
                int nodeId = int.Parse(toggle.gameObject.name.Replace("toggle", ""));
                if (RaidEditor.GetInst().CurrentGroupIdx > 0)
                {
                        Dictionary<int, RaidNodeBehav> dict = RaidEditor.GetInst().m_NodeDict;
                        if (dict.ContainsKey(nodeId))
                        {
                                if (toggle.isOn)
                                {
                                        GetText(toggle.gameObject, "groupIdx").text = RaidEditor.GetInst().CurrentGroupIdx.ToString();
                                        dict[nodeId].groupId = RaidEditor.GetInst().CurrentGroupIdx;
                                }
                                else
                                {
                                        GetText(toggle.gameObject, "groupIdx").text = "";
                                        dict[nodeId].groupId = 0;
                                }
                        }
                        else
                        {
                                toggle.isOn = false;
                                GetText(toggle.gameObject, "groupIdx").text = "";
                        }
                }
                else
                {
                        toggle.isOn = false;
                        GetText(toggle.gameObject, "groupIdx").text = "";
                }
        }

        public void OnClickToggleEditor(Toggle toggle)
        {
                RaidEditor.GetInst().EditorType = toggle == m_ToggleRaid ? 0 : 1;
        }
        public void OnClickToggleDirection(Toggle toggle)
        {
                if (toggle.isOn)
                {
                        int idx = int.Parse(toggle.name.Replace("toggle", ""));
                        RaidEditor.GetInst().RotateMap(idx);
                }
        }
}
