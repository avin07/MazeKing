using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Message;
using System.Xml;
using System.IO;
using Pathfinding;
using UnityEngine.EventSystems;
using UnityEngine.UI;

class RaidEditor : SingletonBehaviour<RaidEditor>
{
        public int EditorType = 0;

        const string NODE_XML_NAME = "raidmap_random";  //记录所有分片节点的xml
        const string INFO_XML_NAME = "raidmap_random_info";     //记录所有分片信息的xml

        const string HOME_BUILDING_XML_NAME = "building_nodes"; //建筑节点表

        public string GetNodeXmlName()
        {
                if (EditorType == 0)
                {
                        return NODE_XML_NAME + ".xml";
                }
                else
                {
                        return HOME_BUILDING_XML_NAME + ".xml";
                }
        }

        public UI_RaidEditor m_UIEditor;

        bool IsBuildingMode = true;
        public string m_sEditorScene = "10001";

        GameObject m_BuildScene;
        GameObject m_SelectElemObj;
        GameObject m_LastBuildingObj;
        GameObject m_EditorTerrain;
        public GameObject EditorTerrain
        {
                get
                {
                        if (m_EditorTerrain == null)
                        {
                                m_EditorTerrain = GameObject.Find("Terrain");
                        }
                        return m_EditorTerrain;
                }
        }

        int m_nFloorHeight = 0;

        Vector3 m_SelectOriPos;
        Vector2 m_vStartMousePos;
        Vector3 m_vStartDragPos;
        Vector3 m_vStartDragRot;
        Vector3 m_vLastMousePos;

        public Dictionary<int, RaidNodeBehav> m_NodeDict = new Dictionary<int, RaidNodeBehav>();
        RaidNodeBehav GetNodeBehav(int posId)
        {
                RaidNodeBehav node = null;
                if (m_NodeDict.ContainsKey(posId))
                {
                        node = m_NodeDict[posId];
                }
                else
                {
                        GameObject nodeObj = new GameObject();
                        nodeObj.name = "Node" + posId;
                        nodeObj.transform.SetParent(m_BuildScene.transform);
                        nodeObj.transform.position = new Vector3(posId / 100, 0f, posId % 100);

                        node = nodeObj.AddComponent<RaidNodeBehav>();
                        node.id = posId;
                        m_NodeDict.Add(node.id, node);
                }
                return node;
        }
        object m_CurrentCfg;
        RaidNodeBehav m_SelectNode = null;
        public int CurrentGroupIdx = 0;


        GameObject m_ObjectEffect;
        GameObject ObjectEffect
        {
                get
                {
                        if (m_ObjectEffect == null)
                        {
                                m_ObjectEffect = EffectManager.GetInst().GetEffectObj("effect_raid_click_roadbed_002");
                                GameUtility.GetTransform(m_ObjectEffect, "fangkuang").gameObject.SetActive(false);
                        }
                        return m_ObjectEffect;
                }
        }
        public void EnterBuildMode()
        {
                Camera.main.enabled = false;
                GameStateManager.GetInst().GameState = GAMESTATE.RAID_EDITOR;

                Object obj = ResourceManager.GetInst().Load("Scene/" + m_sEditorScene);
                m_BuildScene = GameObject.Instantiate(obj) as GameObject;
                EditorTerrain.transform.position = new Vector3(EditorTerrain.transform.position.x, -1f, EditorTerrain.transform.position.z);
                GameObject.Find("CollisionRoot").SetActive(false);
                m_UIEditor.SetPage(0);
        }

        public void Update()
        {
                if (GameStateManager.GetInst().GameState == GAMESTATE.RAID_EDITOR)
                {
                        UpdateInput();
                }
        }
        void UpdateCameraMove()
        {
                float angleX = Camera.main.transform.rotation.eulerAngles.x;
                Camera.main.transform.Rotate(angleX * -1, 0f, 0f);
                Camera.main.transform.position = m_vStartDragPos - (Input.mousePosition.x - m_vStartMousePos.x) / 10f * Camera.main.transform.right - (Input.mousePosition.y - m_vStartMousePos.y) / 10f * Camera.main.transform.forward;
                Camera.main.transform.Rotate(angleX, 0f, 0f);
        }
        void UpdateBuildingObjMove()
        {
                RaycastHit hit = new RaycastHit();
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, Camera.main.farClipPlane, 1 << LayerMask.NameToLayer("Scene")))
                {
                        Vector3 pos = new Vector3((int)hit.point.x, 0f, (int)hit.point.z);
                        m_SelectElemObj.transform.position = pos;
                }
        }
        void UpdateCameraRotate()
        {
                Quaternion qua = new Quaternion();
                qua.eulerAngles = m_vStartDragRot + new Vector3(-(Input.mousePosition.y - m_vStartMousePos.y) / 50f, (Input.mousePosition.x - m_vStartMousePos.x) / 50f, 0f);
                Camera.main.transform.rotation = qua;
        }
        void UpdateInput()
        {
                if (UIManager.GetInst().IsUIVisible("UI_RaidEditorCreate"))
                        return;

                if (Input.GetMouseButtonDown(0))
                {
                        m_vStartMousePos = Input.mousePosition;
                        m_vStartDragPos = Camera.main.transform.position;
                }

                if (Input.GetMouseButtonDown(1))
                {
                        m_vStartMousePos = Input.mousePosition;
                        m_vStartDragRot = Camera.main.transform.rotation.eulerAngles;
                        return;
                }
                m_vLastMousePos = Input.mousePosition;

                if (m_SelectBrick != null)
                {
                        if (Input.GetMouseButtonUp(1))
                        {
                                GameObject.Destroy(m_SelectBrick);
                                m_SelectBrick = null;
                                m_CurrentCfg = null;
                                return;
                        }
                        if (InputManager.GetInst().IsPointerOverUgui())
                        {
                                return;
                        }
                        if (Input.GetMouseButtonUp(0))
                        {
                                PlaceBrick();
                        }
                        else
                        {
                                RaycastHit hit = new RaycastHit();
                                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                                if (Physics.Raycast(ray, out hit, Camera.main.farClipPlane))
                                {
                                        
                                        Vector3 pos = new Vector3(Mathf.RoundToInt(hit.point.x), (int)hit.point.y + 1f, Mathf.RoundToInt(hit.point.z));
                                        m_SelectBrick.transform.position = pos;
                                }
                        }
                }
                else if (m_SelectElemObj != null)
                {
                        if (Input.GetMouseButtonUp(1))
                        {
                                GameObject.Destroy(m_SelectElemObj);
                                m_SelectElemObj = null;
                                m_CurrentCfg = null;
                                return;
                        }
                        if (InputManager.GetInst().IsPointerOverUgui())
                        {
                                return;
                        }
                        if (Input.GetMouseButtonUp(0))
                        {
                                int id = (int)m_SelectElemObj.transform.position.x * 100 + (int)m_SelectElemObj.transform.position.z;
                                AddNodeElement(id, (RaidElemConfig)m_CurrentCfg, m_SelectElemObj.transform.position.y);
                                GameObject.Destroy(m_SelectElemObj);
                                m_SelectElemObj = null;
                        }
                        else
                        {
                                RaycastHit hit = new RaycastHit();
                                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                                if (Physics.Raycast(ray, out hit, Camera.main.farClipPlane))
                                {
                                        Vector3 pos = new Vector3(Mathf.RoundToInt(hit.point.x), (int)hit.point.y + 0.01f, Mathf.RoundToInt(hit.point.z));
                                        m_SelectElemObj.transform.position = pos;
                                }
                        }
                }
                else
                {

                        if (Input.GetMouseButton(1))
                        {
                                UpdateCameraRotate();
                        }
                        if (InputManager.GetInst().IsPointerOverUgui())
                        {
                                return;
                        }
                        if (Input.GetMouseButton(0))
                        {
                                UpdateCameraMove();
                        }

                }

                if (Input.GetAxis("Mouse ScrollWheel") != 0)
                {
                        Camera.main.transform.position += Camera.main.transform.forward * Input.GetAxis("Mouse ScrollWheel") * 10f;
                }

                if (GameStateManager.GetInst().GameState == GAMESTATE.RAID_EDITOR)
                {
                        if (Input.GetKeyUp(KeyCode.D))
                        {
                                DeleteSelected();
                        }
                }
        }

        #region 元素放置

        void AddNodeElement(int nodeId, RaidElemConfig elemCfg, float posY = 0f)
        {                
                RaidNodeBehav node = GetNodeBehav(nodeId);

                if (node.elemObj != null)
                {
                        GameObject.Destroy(node.elemObj);
                        node.elemCfg = null;
                }
                
                node.elemCfg = elemCfg;
                node.InitElemObj();
                if (node.elemObj != null)
                {
                        node.elemObj.transform.localPosition +=Vector3.up * posY;
                }
                //node.elemObj.transform.SetParent(node.transform);

                //obj.transform.localPosition = Vector3.zero;
                node.UpdateBlockState();
        }
        public void SelectNode(RaidNodeBehav node)
        {
                m_SelectNode = node;
                ObjectEffect.transform.localScale = node.GetComponent<BoxCollider>().size;
                ObjectEffect.transform.position = node.transform.position + node.GetComponent<BoxCollider>().center;
                ObjectEffect.SetActive(true);
                m_UIEditor.SetNode(node);
        }
        GameObject InitElemObj(object cfg)
        {
                if (cfg == null)
                {
                        return null;
                }
                GameObject tmpObj = null;
                if (cfg is RaidElemConfig)
                {
                        RaidElemConfig elemCfg = (RaidElemConfig)cfg;
                        tmpObj = new GameObject();

                        if (elemCfg.IsCharacter())
                        {
                                GameObject obj = ModelResourceManager.GetInst().GenerateObject(elemCfg.mainModel);
                                GameUtility.ObjPlayAnim(tmpObj, CommonString.idle_001Str, true);
                                if (obj != null)
                                {
                                        obj.transform.SetParent(tmpObj.transform);
                                        obj.transform.localPosition = new Vector3(elemCfg.size.x / 2f - 0.5f, 0, elemCfg.size.z / 2f - 0.5f);
                                }
                        }
                        else
                        {
                                for (int i = 0; i < elemCfg.model.Count; i++)
                                {
                                        GameObject obj = ModelResourceManager.GetInst().GenerateObject(elemCfg.model[i]);
                                        if (obj != null)
                                        {
                                                obj.transform.SetParent(tmpObj.transform);
                                                obj.transform.localPosition = new Vector3(elemCfg.size.x / 2f - 0.5f, i, elemCfg.size.z / 2f-0.5f);
                                                if (obj.name.Contains("model_common_"))
                                                {
                                                        obj.transform.localPosition += Vector3.up;//元素的砖默认比地板高一格
                                                }
                                                BoxCollider box = obj.GetComponentInChildren<BoxCollider>();
                                                if (box != null)
                                                {
                                                        Component.Destroy(box);
                                                }
                                        }
                                }
                        }
                        if (tmpObj != null)
                        {
                                tmpObj.transform.SetParent(m_BuildScene.transform);
                        }
                }
                return tmpObj;
        }

        public void DeleteBuilding(RaidNodeBehav bb)
        {
                Debuger.Log("DeleteBuilding " + bb.id);
                GameObject.Destroy(bb.gameObject);
        }
        public void DeleteSelected()
        {
                if (m_SelectNode != null)
                {
                        if (m_NodeDict.ContainsKey(m_SelectNode.id))
                        {
                                m_NodeDict.Remove(m_SelectNode.id);
                        }
                        GameObject.Destroy(m_SelectNode.gameObject);
                }
                m_SelectNode = null;
                ObjectEffect.SetActive(false);
        }

        public void StartPlaceBuilding(RaidElemConfig cfg)
        {
//                 if (EditorType == 1)
//                         return;

                m_CurrentCfg = cfg;
                m_SelectElemObj = InitElemObj(cfg);
        }

        public void CopyObject(int xOffset, int yOffset)
        {
                int posId = (int)(m_LastBuildingObj.transform.position.x + xOffset) * 100 + (int)(m_LastBuildingObj.transform.position.z + yOffset);
                //if (m_NormalFloorDict.ContainsKey(posId) || m_RaidNodeDict.ContainsKey(posId))
                {
                        if (m_LastBuildingObj != null)
                        {
                                GameObject obj = GameObject.Instantiate(m_LastBuildingObj) as GameObject;
                                obj.transform.position = new Vector3(m_LastBuildingObj.transform.position.x + xOffset, m_LastBuildingObj.transform.position.y, m_LastBuildingObj.transform.position.z + yOffset);

                                //AddNodeElement(obj, (RaidElemConfig)m_CurrentCfg);
                                m_LastBuildingObj = obj;
                        }
                }
        }
        #endregion

        #region 地形放置
        GameObject m_SelectBrick;

        public void SelectBrick(ModelConfig cfg)
        {
                m_CurrentCfg = cfg;
                if (m_SelectBrick == null)
                {
                        m_SelectBrick = ModelResourceManager.GetInst().GenerateCommonObject(cfg.id);
                }
        }
        public void PlaceBrick()
        {
                int x = (int)m_SelectBrick.transform.position.x;
                int y = (int)m_SelectBrick.transform.position.z;
                int id = x * 100 + y;
                RaidNodeBehav node = GetNodeBehav(id);
                if (node != null)
                {
                        ModelConfig cfg = m_CurrentCfg as ModelConfig;
                        node.floorList.Add(cfg.id);
                        node.InitFloorObjs(m_nFloorHeight);
                        node.UpdateBlockState();
                        if (node == m_SelectNode)
                        {
                                m_UIEditor.SetNode(m_SelectNode);
                        }
                }
        }

        #endregion
        Vector2 m_vSize;
        public void SetTerrainSize(int sizex, int sizey)
        {
                Terrain comp = EditorTerrain.GetComponent<Terrain>();
                comp.terrainData.size = new Vector3(sizex, comp.terrainData.size.y, sizey);
                m_vSize = new Vector2(sizex, sizey);
                comp.Flush();
        }
        public void SetCurrentElemRot(int rotY)
        {
                if (m_SelectNode != null)
                {
                        if (m_SelectNode.elemObj != null)
                        {
                                Quaternion qua = m_SelectNode.elemObj.transform.localRotation;
                                qua.eulerAngles = new Vector3(qua.eulerAngles.x, rotY, qua.eulerAngles.z);
                                m_SelectNode.elemObj.transform.localRotation = qua;
                        }
                }
        }
        public void SetCurrentElemId(int elemId)
        {
                if (m_SelectNode != null)
                {
                        if (m_SelectNode.elemCfg == null || m_SelectNode.elemCfg.id != elemId)
                        {
                                m_SelectNode.elemCfg = RaidConfigManager.GetInst().GetElemCfg(elemId);
                                m_SelectNode.ResetElemObj();
                        }                                
                }
        }
        public void SetCurrentElemY(int posY)
        {
                if (m_SelectNode != null)
                {
                        if (m_SelectNode.elemObj != null)
                        {
                                m_SelectNode.elemObj.transform.localPosition = Vector3.up * posY;
                        }
                }
        }
        public void SetCurrentFloor(int idx, string idStr)
        {
                if (m_SelectNode != null && m_SelectNode.floorList != null)
                {
                        if (idx >= 0 && idx < m_SelectNode.floorList.Count)
                        {
                                int id = 0;
                                int.TryParse(idStr, out id);
                                m_SelectNode.floorList[idx] = id;
                                for (int i = m_SelectNode.floorList.Count - 1; i >= 0; i--)
                                {
                                        if (m_SelectNode.floorList[i] <= 0)
                                        {
                                                m_SelectNode.floorList.RemoveAt(i);
                                        }
                                        else
                                        {
                                                break;
                                        }
                                }
                        }
                        m_SelectNode.InitFloorObjs(m_nFloorHeight);
                        m_SelectNode.UpdateBlockState();
                        m_UIEditor.SetNode(m_SelectNode);
                }
        }

        public void ChangeFloorHeight(int delta)
        {
                m_nFloorHeight += delta;
                m_UIEditor.m_MapHeight.text = m_nFloorHeight.ToString();
                foreach (RaidNodeBehav node in m_NodeDict.Values)
                {
                        if (node.floorObj != null)
                        {
                                node.floorObj.transform.localPosition = Vector3.up * m_nFloorHeight;
                                node.UpdateBlockState();
                        }
                }
        }

        bool HasMap(int mapId)
        {
                string xmlName = INFO_XML_NAME + ".xml";
                if (File.Exists(xmlName))
                {
                        XmlDocument xml = new XmlDocument();
                        XmlReaderSettings settings = new XmlReaderSettings();
                        settings.IgnoreComments = true;
                        XmlReader reader = XmlReader.Create(xmlName, settings);
                        xml.Load(reader);
                        reader.Close();

                        return HasMap(xml.SelectSingleNode("dataroot"), mapId);
                }
                return false;
        }
        public bool HasMap(XmlNode rootNode, int mapId)
        {
                if (rootNode != null)
                {
                        foreach (XmlNode node in rootNode.ChildNodes)
                        {
                                int id = 0;
                                XMLPARSE_METHOD.GetAttrValueInt(node, "id", ref id, 0);
                                if (id == mapId)
                                {
                                        return true;
                                }
                        }
                }
                return false;                
        }

        public void CreateMap(int mapId, int sizeX, int sizeY, string floorList, string wallList, int door_EW, int door_NS, int wallHeight)
        {
                if (HasMap(mapId))
                {
                        Debuger.Log("已有地图" + mapId);
                }
                
                SetTerrainSize(sizeX, sizeY);
                ClearAll();

                string[] floors = floorList.Split(',');
                string[] walls = wallList.Split(',');

                RaidElemConfig doorElem_EW = RaidConfigManager.GetInst().GetElemCfg(door_EW);
                RaidElemConfig doorElem_NS = RaidConfigManager.GetInst().GetElemCfg(door_NS);

                for (int x = 0; x < sizeX; x++)
                {
                        for (int y = 0; y < sizeY; y++)
                        {
                                int nodeId = x * 100 + y;

                                RaidNodeBehav node = GetNodeBehav(nodeId);

                                int floorid = 0;
                                int.TryParse(floors[UnityEngine.Random.Range(0, floors.Length)], out floorid);
                                node.floorList.Add(floorid);

                                if (x == 0 || x == sizeX - 1)
                                {
                                        if (y == sizeY / 2 - 1 && doorElem_NS != null)
                                        {
                                                int id = x * 100 + y;
                                                AddNodeElement(id, doorElem_NS);
                                        }
                                        if (walls.Length > 0)
                                        {
                                                for (int i = 0; i < wallHeight; i++)
                                                {
                                                        int wallid = 0;
                                                        if (y == sizeY / 2 - 1 || y == sizeY / 2)
                                                        {
                                                                if (doorElem_NS != null && i >= doorElem_NS.size.y)
                                                                {
                                                                        int.TryParse(walls[UnityEngine.Random.Range(0, walls.Length)], out wallid);
                                                                }
                                                        }
                                                        else
                                                        {
                                                                int.TryParse(walls[UnityEngine.Random.Range(0, walls.Length)], out wallid);
                                                        }
                                                        
                                                        node.floorList.Add(wallid);
                                                }
                                        }
                                }
                                else if (y == 0 || y == sizeY - 1)
                                {
                                        if (x == sizeX /2 - 1 && doorElem_EW != null)
                                        {
                                                //GameObject obj = InitElemObj(doorElem_EW);
                                                //obj.transform.position = new Vector3(x, 0f, y);
                                                AddNodeElement(x * 100 + y, doorElem_EW);
                                        }
                                        if (walls.Length > 0)
                                        {
                                                for (int i = 0; i < wallHeight; i++)
                                                {
                                                        int wallid = 0;
                                                        if (x == sizeX / 2 - 1 || x == sizeX / 2)
                                                        {
                                                                if (doorElem_EW != null && i >= doorElem_EW.size.y)
                                                                {
                                                                        int.TryParse(walls[UnityEngine.Random.Range(0, walls.Length)], out wallid);
                                                                }
                                                        }
                                                        else
                                                        {
                                                                int.TryParse(walls[UnityEngine.Random.Range(0, walls.Length)], out wallid);
                                                        }
                                                        node.floorList.Add(wallid);
                                                }
                                        }
                                }
                                for (int i = node.floorList.Count - 1; i >= 0; i--)
                                {
                                        if (node.floorList[i] <= 0)
                                        {
                                                node.floorList.RemoveAt(i);
                                        }
                                        else
                                        {
                                                break;
                                        }
                                }

                                node.InitFloorObjs(m_nFloorHeight);                                
                                node.UpdateBlockState();
                        }
                }
        }

        public void ClearAll()
        {
                foreach (RaidNodeBehav node in m_NodeDict.Values)
                {
                        GameObject.Destroy(node.floorObj);
                        GameObject.Destroy(node.elemObj);
                        GameObject.Destroy(node.gameObject);
                }
                m_NodeDict.Clear();
        }

        #region SAVE/LOAD

        private void AddElementAttribute(XmlDocument xmlDoc, XmlNode rootNode, string attributeName, string attributeValue)
        {
                XmlElement attributeElem = xmlDoc.CreateElement(attributeName);
                rootNode.AppendChild(attributeElem);
                attributeElem.InnerText = attributeValue;
        }

        XmlNode GetRootNode(XmlDocument xmlDoc)
        {
                XmlNode rootNode = xmlDoc.SelectSingleNode("dataroot");
                if (rootNode == null)
                {
                        XmlElement rootElem = xmlDoc.CreateElement("dataroot");
                        rootNode = xmlDoc.AppendChild(rootElem);
                }
                return rootNode;
        }

        bool RemoveXmlNode(int removeId, XmlNode rootNode, bool bInner = true, int tmp = 1)
        {
                bool bExist = false;
                List<XmlNode> toRemove = new List<XmlNode>();
                foreach (XmlNode node in rootNode.ChildNodes)
                {
                        int id = 0;
                        if (!bInner)
                        {
                                XMLPARSE_METHOD.GetAttrValueInt(node, "id", ref id, 0);
                        }
                        else
                        {
                                XMLPARSE_METHOD.GetNodeInnerInt(node, "id", ref id, 0);
                        }
                        if (id / tmp == removeId)
                        {
                                bExist = true;
                                toRemove.Add(node);
                        }
                }
                foreach (XmlNode node in toRemove)
                {
                        rootNode.RemoveChild(node);
                }
                return bExist;
        }

        XmlDocument GetXmlDoc(string xmlName, bool bNeedBackup = true)
        {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", ""));
                if (File.Exists(xmlName))
                {
                        if (bNeedBackup)
                        {
                                File.Copy(xmlName, xmlName + ".bak", true);
                        }

                        XmlReaderSettings settings = new XmlReaderSettings();
                        settings.IgnoreComments = true;
                        XmlReader reader = XmlReader.Create(xmlName, settings);
                        xmlDoc.Load(reader);
                        reader.Close();
                }
                return xmlDoc;
        }

        public void SaveToXml()
        {
                int mapid = 0;
                int.TryParse(m_UIEditor.m_IdField.text, out mapid);
                if (mapid <= 0)
                {
                        GameUtility.PopupMessage("请先填写地图id");
                        return;
                }

                string clientXmlName = GetNodeXmlName();

                if (!string.IsNullOrEmpty(clientXmlName))
                {
                        XmlDocument xmlDoc = GetXmlDoc(clientXmlName);
                        XmlNode rootNode = GetRootNode(xmlDoc);
                        bool bExist = HasMap(rootNode, mapid);

                        if (EditorType == 0)
                        {
                                if (bExist)
                                {
                                        UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox").SetConfirmAndCancel("", "迷宫分片ID : " + mapid + " 已存在，确定覆盖吗？", ConfirmSaveRaid, null, xmlDoc);
                                }
                                else
                                {
                                        ConfirmSaveRaid(xmlDoc);
                                }
                        }
                        else
                        {
                                if (bExist)
                                {
                                        UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox").SetConfirmAndCancel("", "建筑ID : " + mapid + " 已存在，确定覆盖吗？", ConfirmSaveBuilding, null, xmlDoc);
                                }
                                else
                                {
                                        ConfirmSaveBuilding(xmlDoc);
                                }
                        }
                }
        }
        void ConfirmSaveBuilding(object data)
        {
                int mapid = 0;
                int.TryParse(m_UIEditor.m_IdField.text, out mapid);
                string clientXmlName = GetNodeXmlName();
                XmlDocument clientXml = data as XmlDocument;
                XmlNode clientRootNode = clientXml.SelectSingleNode("dataroot");
                RemoveXmlNode(mapid, clientRootNode, false);
                List<XML_NODE_DATA> tmplist = new List<XML_NODE_DATA>();

                if (clientRootNode != null)
                {
                        XmlElement mapElem = clientXml.CreateElement(HOME_BUILDING_XML_NAME);
                        mapElem.SetAttribute("id", mapid.ToString());
                        XmlNode mapNode = clientRootNode.AppendChild(mapElem);

                        foreach (RaidNodeBehav node in m_NodeDict.Values)
                        {
                                if (node.id < 0)
                                        continue;
                                if (node.floorList == null || node.floorList.Count <= 0)
                                        continue;

                                XmlElement clientNode = clientXml.CreateElement("Node");
                                XmlNode newNode = clientRootNode.AppendChild(clientNode);

                                int rotY = node.elemObj != null ? (int)node.elemObj.transform.localRotation.eulerAngles.y : 0;
                                int posY = node.elemObj != null ? (int)node.elemObj.transform.localPosition.y : 0;

                                clientNode.SetAttribute("id", node.id.ToString());
                                clientNode.SetAttribute("floorlist", GameUtility.ListToString<int>(node.floorList, ','));
                                clientNode.SetAttribute("elem_id", node.elemCfg != null ? node.elemCfg.id.ToString() : "0");
                                clientNode.SetAttribute("elem_rot", rotY.ToString());
                                clientNode.SetAttribute("elem_posy", posY.ToString());
                                clientNode.SetAttribute("elem_model_id", node.elemCfg != null ? node.elemCfg.mainModel.ToString() : "0");

                                mapNode.AppendChild(clientNode);
                                tmplist.Add(new XML_NODE_DATA(newNode));
                        }
                        clientXml.Save(clientXmlName);
                        if (!m_XmlNodeDict.ContainsKey(mapid))
                        {
                                m_XmlNodeDict.Add(mapid, tmplist);
                        }
                        else
                        {
                                m_XmlNodeDict[mapid] = tmplist;
                        }
                }
        }

        void ConfirmSaveRaid(object data)
        {
                int mapid = 0;
                int.TryParse(m_UIEditor.m_IdField.text, out mapid);

                string clientXmlName = NODE_XML_NAME + ".xml";
                string serverXmlName = NODE_XML_NAME + "_s.xml";
                int enteranceId = 0;
                XmlDocument clientXml = data as XmlDocument;
                XmlNode clientRootNode = clientXml.SelectSingleNode("dataroot");
                RemoveXmlNode(mapid, clientRootNode, false);

                XmlDocument serverXml = GetXmlDoc(serverXmlName);
                XmlNode serverRootNode = GetRootNode(serverXml);
                RemoveXmlNode(mapid, serverRootNode, true, 10000);

                List<XML_NODE_DATA> tmplist = new List<XML_NODE_DATA>();
                int doorstate = 0;
                if (clientRootNode != null)
                {
                        XmlElement mapElem = clientXml.CreateElement(NODE_XML_NAME);
                        mapElem.SetAttribute("id", mapid.ToString());
                        XmlNode mapNode = clientRootNode.AppendChild(mapElem);

                        foreach (RaidNodeBehav node in m_NodeDict.Values)
                        {
                                if (node.id < 0)
                                        continue;
                                int rotY = node.elemObj != null ? (int)node.elemObj.transform.localRotation.eulerAngles.y : 0;
                                int posY = node.elemObj != null ? (int)node.elemObj.transform.localPosition.y : 0;

                                if (node.elemCfg != null)
                                {
                                        if (node.elemCfg.type == (int)RAID_ELEMENT_TYPE.ENTRANCE)
                                        {
                                                enteranceId = node.id;
                                        }
                                        if (node.IsDoor())
                                        {
                                                if (node.posY == (int)m_vSize.y - 1)
                                                {
                                                        doorstate |= GameUtility.GetFlagValInt(0);
                                                }
                                                else if (node.posY == 0)
                                                {
                                                        doorstate |= GameUtility.GetFlagValInt(2);
                                                }
                                                else if (node.posX == 0)
                                                {
                                                        doorstate |= GameUtility.GetFlagValInt(1);
                                                }
                                                else if (node.posX == (int)m_vSize.x - 1)
                                                {
                                                        doorstate |= GameUtility.GetFlagValInt(3);
                                                }
                                        }
                                }


                                XmlElement clientNode = clientXml.CreateElement("Node");
                                XmlNode newNode = clientRootNode.AppendChild(clientNode);

                                clientNode.SetAttribute("id", node.id.ToString());
                                string floorInfo = GameUtility.ListToString<int>(node.floorList, ',');
                                clientNode.SetAttribute("floorlist", floorInfo);
                                clientNode.SetAttribute("elem_id", node.elemCfg != null ? node.elemCfg.id.ToString() : "0");
                                clientNode.SetAttribute("elem_rot", rotY.ToString());
                                clientNode.SetAttribute("elem_posy", posY.ToString());
                                clientNode.SetAttribute("group_id", node.groupId.ToString());
                                mapNode.AppendChild(clientNode);
                                tmplist.Add(new XML_NODE_DATA(newNode));

                                if (IsServerNeed(node.ElemId))
                                {
                                        XmlElement serverElem = serverXml.CreateElement(NODE_XML_NAME + "_s");
                                        XmlNode serverNode = serverRootNode.AppendChild(serverElem);
                                        int id = mapid * 10000 + node.id;
                                        AddElementAttribute(serverXml, serverNode, "id", id.ToString());
                                        AddElementAttribute(serverXml, serverNode, "elem_id", node.elemCfg != null ? node.elemCfg.id.ToString() : "0");
                                }
                        }
                        clientXml.Save(clientXmlName);
                        serverXml.Save(serverXmlName);
                        if (!m_XmlNodeDict.ContainsKey(mapid))
                        {
                                m_XmlNodeDict.Add(mapid, tmplist);
                        }
                        else
                        {
                                m_XmlNodeDict[mapid] = tmplist;
                        }
                }

                string clientInfoXmlName = INFO_XML_NAME + ".xml";
                {
                        XmlDocument clientInfoXml = GetXmlDoc(clientInfoXmlName);
                        XmlNode clientInfoRootNode = GetRootNode(clientInfoXml);
                        RemoveXmlNode(mapid, clientInfoRootNode);

                        XmlElement mapElement = clientInfoXml.CreateElement(INFO_XML_NAME);
                        XmlNode mapNode = clientInfoRootNode.AppendChild(mapElement);

                        AddElementAttribute(clientInfoXml, mapNode, "id", mapid.ToString());
                        AddElementAttribute(clientInfoXml, mapNode, "sizex", m_UIEditor.m_SizeXField.text);
                        AddElementAttribute(clientInfoXml, mapNode, "sizey", m_UIEditor.m_SizeYField.text);
                        AddElementAttribute(clientInfoXml, mapNode, "type", m_UIEditor.m_MapTypeField.text);
                        AddElementAttribute(clientInfoXml, mapNode, "entrance", enteranceId.ToString());
                        AddElementAttribute(clientInfoXml, mapNode, "is_random", m_UIEditor.m_ToggleRandomMap.isOn ? "1" : "0");
                        AddElementAttribute(clientInfoXml, mapNode, "CameraPos", Camera.main.transform.position.ToString());
                        AddElementAttribute(clientInfoXml, mapNode, "floor_height", m_nFloorHeight.ToString());
                        AddElementAttribute(clientInfoXml, mapNode, "door_state", doorstate.ToString());
                        clientInfoXml.Save(clientInfoXmlName);

                        if (!m_XmlInfoDict.ContainsKey(mapid))
                        {
                                m_XmlInfoDict.Add(mapid, new XML_INFO_DATA(mapNode));
                        }
                        else
                        {
                                m_XmlInfoDict[mapid] = new XML_INFO_DATA(mapNode);
                        }
                }
        }

        public void ConvertToServerXml()
        {
                if (EditorType == 1)
                        return;
                string nodeXmlName = NODE_XML_NAME + ".xml";
                if (File.Exists(nodeXmlName))
                {
                        XmlDocument xmlDoc = GetXmlDoc(nodeXmlName, false);
                        XmlNode rootNode = GetRootNode(xmlDoc);

                        string serverNodeXmlName = NODE_XML_NAME + "_s.xml";

                        //                         XmlDocument serverXml = GetXmlDoc(serverNodeXmlName);
                        // 
                        XmlDocument serverXml = new XmlDocument();
                        serverXml.AppendChild(serverXml.CreateXmlDeclaration("1.0", "utf-8", ""));

                        XmlNode serverRoot = GetRootNode(serverXml);

                        List<XmlNode> toremove = new List<XmlNode>();
                        if (rootNode != null)
                        {
                                foreach (XmlNode mapNode in rootNode.ChildNodes)
                                {
                                        int mapid = 0;
                                        XMLPARSE_METHOD.GetAttrValueInt(mapNode, "id", ref mapid, 0);
                                        if (mapid > 0)
                                        {
                                                foreach (XmlNode node in mapNode.ChildNodes)
                                                {
                                                        XML_NODE_DATA nodeData = new XML_NODE_DATA(node);
                                                        RaidElemConfig elemCfg = RaidConfigManager.GetInst().GetElemCfg(nodeData.elemid);
                                                        if (IsServerNeed(nodeData.elemid))
                                                        {
                                                                XmlElement serverElem = serverXml.CreateElement(NODE_XML_NAME + "_s");
                                                                XmlNode serverNode = serverRoot.AppendChild(serverElem);

                                                                AddElementAttribute(serverXml, serverNode, "id", (nodeData.id + mapid * 10000).ToString());
//                                                                AddElementAttribute(serverXml, serverNode, "floorlist", GameUtility.ListToString<int>(nodeData.floorlist,','));
                                                                AddElementAttribute(serverXml, serverNode, "elem_id", nodeData.elemid.ToString());
//                                                                 AddElementAttribute(serverXml, serverNode, "elem_rot", nodeData.rotY.ToString());
//                                                                 AddElementAttribute(serverXml, serverNode, "elem_posy", nodeData.posY.ToString());
                                                        }
                                                }
                                        }
                                }
                        }
                        serverXml.Save(serverNodeXmlName);
                }
        }

        public void UpdateAllXml()
        {
                if (EditorType == 1)
                        return;

                string clientXmlName = NODE_XML_NAME + ".xml";

                Dictionary<int, int> doorstate = new Dictionary<int, int>();
                if (File.Exists(clientXmlName))
                {
                        XmlDocument xml = GetXmlDoc(clientXmlName);
                        XmlNode rootNode = GetRootNode(xml);
                        if (rootNode != null)
                        {
                                foreach (XmlNode mapNode in rootNode.ChildNodes)
                                {
                                        int mapid = 0;
                                        XMLPARSE_METHOD.GetAttrValueInt(mapNode, "id", ref mapid, 0);
                                        if (mapid > 0)
                                        {
                                                int sizex = 0, sizey = 0;
                                                if (m_XmlInfoDict.ContainsKey(mapid))
                                                {
                                                        sizex = m_XmlInfoDict[mapid].sizex;
                                                        sizey = m_XmlInfoDict[mapid].sizey;
                                                }
                                                if (!doorstate.ContainsKey(mapid))
                                                {
                                                        doorstate.Add(mapid, 0);
                                                }
                                                List<XmlNode> toRemove = new List<XmlNode>();
                                                List<XmlElement> toAdd = new List<XmlElement>();

                                                foreach (XmlNode infoNode in mapNode.ChildNodes)
                                                {
                                                        toRemove.Add(infoNode);

                                                        XML_NODE_DATA nodeData = new XML_NODE_DATA(infoNode);
                                                        RaidElemConfig elemCfg = RaidConfigManager.GetInst().GetElemCfg(nodeData.elemid);
                                                        if (elemCfg != null && elemCfg.IsDoor())
                                                        {

                                                                if (nodeData.NodeId % 100 == sizey - 1)
                                                                {
                                                                        doorstate[mapid] |= GameUtility.GetFlagValInt(0);
                                                                }
                                                                else if (nodeData.NodeId % 100 == 0)
                                                                {
                                                                        doorstate[mapid] |= GameUtility.GetFlagValInt(2);
                                                                }
                                                                else if (nodeData.NodeId / 100 == sizex - 1)
                                                                {
                                                                        doorstate[mapid] |= GameUtility.GetFlagValInt(3);
                                                                }
                                                                else if (nodeData.NodeId / 100 == 0)
                                                                {
                                                                        doorstate[mapid] |= GameUtility.GetFlagValInt(1);
                                                                }
                                                        }
                                                        XmlElement attributeElem = xml.CreateElement("Node");
                                                        attributeElem.SetAttribute("id", nodeData.NodeId.ToString());
                                                        string tmp = GameUtility.ListToString<int>(nodeData.floorlist, ',');
                                                        attributeElem.SetAttribute("floorlist", tmp.ToString());
                                                        attributeElem.SetAttribute("elem_id", nodeData.elemid.ToString());
                                                        attributeElem.SetAttribute("elem_rot", nodeData.rotY.ToString());
                                                        attributeElem.SetAttribute("elem_posy", nodeData.posY.ToString());
                                                        attributeElem.SetAttribute("group_id", nodeData.group_id.ToString());

                                                        toAdd.Add(attributeElem);
                                                }

                                                foreach (XmlNode node in toRemove)
                                                {
                                                        mapNode.RemoveChild(node);
                                                }
                                                foreach (XmlElement node in toAdd)
                                                {
                                                        mapNode.AppendChild(node);
                                                }
                                        }
                                }
                        }
                        xml.Save(clientXmlName);
                }

                string clientInfoXmlName = INFO_XML_NAME + ".xml";

                if (File.Exists(clientInfoXmlName))
                {
                        XmlDocument xml = GetXmlDoc(clientInfoXmlName);
                        XmlNode rootNode = GetRootNode(xml);

                        if (rootNode != null)
                        {
                                foreach (XmlNode infoNode in rootNode.ChildNodes)
                                {
                                        XML_INFO_DATA infoData = new XML_INFO_DATA(infoNode);
                                        infoNode.RemoveAll();

                                        AddElementAttribute(xml, infoNode, "id", infoData.id.ToString());
                                        AddElementAttribute(xml, infoNode, "sizex", infoData.sizex.ToString());
                                        AddElementAttribute(xml, infoNode, "sizey", infoData.sizey.ToString());
                                        AddElementAttribute(xml, infoNode, "type", infoData.type.ToString());
                                        AddElementAttribute(xml, infoNode, "entrance", infoData.entrance.ToString());
                                        AddElementAttribute(xml, infoNode, "is_random", infoData.is_random.ToString());
                                        AddElementAttribute(xml, infoNode, "CameraPos", infoData.camPos.ToString());
                                        AddElementAttribute(xml, infoNode, "floor_height", infoData.floor_height.ToString());
//                                         if (infoData.door_state > 0)
//                                         {
//                                                 AddElementAttribute(xml, infoNode, "door_state", infoData.door_state.ToString());
//                                         }
//                                         else
                                        {
                                                if (doorstate.ContainsKey(infoData.id))
                                                {
                                                        AddElementAttribute(xml, infoNode, "door_state", doorstate[infoData.id].ToString());
                                                }
                                        }
                                }
                        }
                        xml.Save(clientInfoXmlName);
                }

        }

        Dictionary<int, XML_INFO_DATA> m_XmlInfoDict = new Dictionary<int, XML_INFO_DATA>();
        Dictionary<int, List<XML_NODE_DATA>> m_XmlNodeDict = new Dictionary<int, List<XML_NODE_DATA>>();
        public void LoadAllXml()
        {
                Debuger.Log(EditorType);
                if (EditorType == 0)
                {
                        LoadInfoXml();
                }

                LoadNodeXml(GetNodeXmlName());
        }
        void LoadInfoXml()
        {
                m_XmlInfoDict.Clear();
                string clientInfoXmlName = INFO_XML_NAME + ".xml";

                if (File.Exists(clientInfoXmlName))
                {
                        XmlDocument xml = GetXmlDoc(clientInfoXmlName, false);
                        XmlNode rootNode = GetRootNode(xml);

                        if (rootNode != null)
                        {
                                foreach (XmlNode node in rootNode.ChildNodes)
                                {
                                        int id = 0;
                                        XMLPARSE_METHOD.GetNodeInnerInt(node, "id", ref id, 0);
                                        if (!m_XmlInfoDict.ContainsKey(id))
                                        {
                                                m_XmlInfoDict.Add(id, new XML_INFO_DATA(node));
                                        }
                                }
                        }
                }
        }
        void LoadNodeXml(string clientXmlName)
        {
                Debuger.Log("LoadNodeXml " + clientXmlName);
                m_XmlNodeDict.Clear();

                if (File.Exists(clientXmlName))
                {
                        XmlDocument xml = GetXmlDoc(clientXmlName, false);
                        XmlNode rootNode = GetRootNode(xml);

                        if (rootNode != null)
                        {
                                for (int i = 0; i < rootNode.ChildNodes.Count; i++)
                                {
                                        XmlNode mapNode = rootNode.ChildNodes.Item(i);
                                        int mapid = 0;
                                        int.TryParse(mapNode.Attributes.GetNamedItem("id").Value, out mapid);
                                        if (!m_XmlNodeDict.ContainsKey(mapid))
                                        {
                                                m_XmlNodeDict.Add(mapid, new List<XML_NODE_DATA>());
                                        }

                                        foreach (XmlNode node in mapNode.ChildNodes)
                                        {
                                                m_XmlNodeDict[mapid].Add(new XML_NODE_DATA(node));
                                        }
                                }
                        }
                }
        }
        public void LoadMap()
        {
                int mapid = 0;
                int.TryParse(m_UIEditor.m_IdField.text, out mapid);
                if (mapid <= 0)
                {
                        GameUtility.PopupMessage("请先填写地图id");
                        return;
                }
                if (EditorType == 0)
                {
                        if (m_XmlInfoDict.ContainsKey(mapid))
                        {
                                ClearAll();
                                XML_INFO_DATA infoData = m_XmlInfoDict[mapid];
                                m_UIEditor.m_SizeXField.text = infoData.sizex.ToString();
                                m_UIEditor.m_SizeYField.text = infoData.sizey.ToString();
                                m_UIEditor.m_MapTypeField.text = infoData.type.ToString();
                                m_UIEditor.m_ToggleRandomMap.isOn = infoData.is_random == 1;
                                m_UIEditor.m_ToggleFixedMap.isOn = infoData.is_random != 1;
                                m_UIEditor.m_MapHeight.text = infoData.floor_height.ToString();
                                m_nFloorHeight = infoData.floor_height;
                                SetTerrainSize(infoData.sizex, infoData.sizey);
                                if (infoData.camPos != Vector3.zero)
                                {
                                        Camera.main.transform.position = infoData.camPos;
                                }
                        }
                        else
                        {
                                GameUtility.PopupMessage("没有这张地图" + mapid);
                                return;
                        }
                }

                if (m_XmlNodeDict.ContainsKey(mapid))
                {
                        if (EditorType == 1)
                        {
                                ClearAll();
                        }
                        foreach (XML_NODE_DATA nodeData in m_XmlNodeDict[mapid])
                        {
                                GameObject nodeObj = new GameObject("Node" + nodeData.NodeId);
                                nodeObj.transform.SetParent(m_BuildScene.transform);
                                nodeObj.transform.position = new Vector3(nodeData.x, 0f, nodeData.y);

                                RaidNodeBehav node = nodeObj.AddComponent<RaidNodeBehav>();
                                node.id = nodeData.NodeId;
                                node.groupId = nodeData.group_id;
                                node.elemCfg = RaidConfigManager.GetInst().GetElemCfg(nodeData.elemid);
                                node.elemCount = 0;
                                node.floorList = nodeData.floorlist;
                                for (int i = node.floorList.Count - 1; i >= 0; i--)
                                {
                                        if (node.floorList[i] <= 0)
                                        {
                                                node.floorList.RemoveAt(i);
                                        }
                                        else
                                        {
                                                break;
                                        }
                                }
                                node.InitFloorObjs(m_nFloorHeight);
                                node.InitElemObj();
                                //node.elemObj = InitElemObj(node.elemCfg);
                                if (node.elemObj != null)
                                {
                                        //node.elemObj.transform.SetParent(nodeObj.transform);
                                        node.elemObj.transform.localPosition += Vector3.up * nodeData.posY;
                                        node.elemObj.transform.localRotation = Quaternion.Euler(Vector3.up * nodeData.rotY);
                                }
                                node.UpdateBlockState();

                                if (!m_NodeDict.ContainsKey(node.id))
                                {
                                        m_NodeDict.Add(node.id, node);
                                }
                        }
                }
                m_UIEditor.RefreshAllnodeToggle();
        }
        bool IsServerNeed(int elemId)
        {
                if (elemId > 0)
                {
                        RaidElemConfig cfg = RaidConfigManager.GetInst().GetElemCfg(elemId);
                        if (cfg != null && cfg.type != (int)RAID_ELEMENT_TYPE.DECORATION)
                        {
                                //有元素时不是装饰，服务端就需要
                                return true;
                        }
                }

                return false;
        }

        class XML_INFO_DATA
        {
                public int id;
                public int sizex;
                public int sizey;
                public int type;
                public int is_random;
                public int entrance;
                public int floor_height;
                public int door_state;
                public Vector3 camPos;
                public XML_INFO_DATA(XmlNode child)
                {
                        XMLPARSE_METHOD.GetNodeInnerInt(child, "id", ref id, 0);
                        XMLPARSE_METHOD.GetNodeInnerInt(child, "sizex", ref sizex, 0);
                        XMLPARSE_METHOD.GetNodeInnerInt(child, "sizey", ref sizey, 0);
                        XMLPARSE_METHOD.GetNodeInnerInt(child, "type", ref type, 0);
                        XMLPARSE_METHOD.GetNodeInnerInt(child, "is_random", ref is_random, 1);
                        XMLPARSE_METHOD.GetNodeInnerInt(child, "entrance", ref entrance, 0);
                        XMLPARSE_METHOD.GetNodeInnerVec3(child, "CameraPos", ref camPos, Vector3.zero);
                        XMLPARSE_METHOD.GetNodeInnerInt(child, "floor_height", ref floor_height, 0);
                        XMLPARSE_METHOD.GetNodeInnerInt(child, "door_state", ref door_state, 0);


                        //                         XMLPARSE_METHOD.GetNodeInnerText(child, "monster_index", ref monsterIds, "");
                        //                         XMLPARSE_METHOD.GetNodeInnerText(child, "trap_index", ref trapIds, "");

                }
        }

        class XML_NODE_DATA
        {
                public int id = 0;
                public List<int> floorlist = new List<int>();
                public int elemid = 0;
                public int parentid = 0;
                public int rotY = 0;
                public int posY = 0;
                public int group_id = 0;
                public int MapId
                {
                        get
                        {
                                return id / 10000;
                        }
                }
                public int NodeId
                {
                        get
                        {
                                return id % 10000;
                        }
                }
                public int x
                {
                        get
                        {
                                return id % 10000 / 100;
                        }
                }
                public int y
                {
                        get
                        {
                                return id % 100;
                        }
                }

                public XML_NODE_DATA(XmlNode child)
                {
                        XMLPARSE_METHOD.GetAttrValueInt(child, "id", ref id, 0);                        
                        XMLPARSE_METHOD.GetAttrValueInt(child, "elem_id", ref elemid, 0);
                        XMLPARSE_METHOD.GetAttrValueInt(child, "elem_rot", ref rotY, 0);
                        XMLPARSE_METHOD.GetAttrValueInt(child, "elem_posy", ref posY, 0);
                        XMLPARSE_METHOD.GetAttrValueInt(child, "group_id", ref group_id, 0);

                        XMLPARSE_METHOD.GetAttrValueListInt(child, "floorlist", ref floorlist);
                }
        }
        #endregion

        int m_nDirectIdx = 0;
        public void RotateMap(int idx)
        {
                if (idx != m_nDirectIdx)
                {
                        m_nDirectIdx = idx;
                        foreach (RaidNodeBehav node in m_NodeDict.Values)
                        {
                                int oriX = node.id / 100;
                                int oriY = node.id % 100;

                                float x = oriX, y=oriY;
                                float rot = 0f;
                                switch (idx)
                                {
                                        case 0:
                                                x = m_vSize.x - oriX - 1;
                                                y = m_vSize.y - oriY - 1;
                                                rot = 180f;
                                                break;
                                        case 1:
                                                x = oriY;
                                                y = m_vSize.x - oriX - 1;
                                                rot = 90f;
                                                break;
                                        default:
                                        case 2:
                                                x = oriX;
                                                y = oriY;
                                                rot = 0f;
                                                break;
                                        case 3:
                                                x = m_vSize.y - oriY - 1;
                                                y = oriX;
                                                rot = 270f;
                                                break;
                                }
                                iTween.moveToWorld(node.gameObject, 1f, 0f, new Vector3(x, node.transform.position.y, y));
                                iTween.rotateTo(node.gameObject, 1f, 0f, new Vector3(0f, rot, 0f));
                                //node.transform.localRotation = Quaternion.Euler(new Vector3(0f, rot, 0f));
                        }
                }
        }
        
}