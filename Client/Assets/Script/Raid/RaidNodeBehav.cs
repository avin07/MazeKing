using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using HighlightingSystem;

public class RaidNodeBehav : MonoBehaviour, IPointerClickHandler
{
        public int id;  //pos.x * 100 + pos.y为id

        public int posX
        {
                get
                {
                        return (int)this.transform.position.x;
                }
        }
        public int posY
        {
                get
                {
                        return (int)this.transform.position.z;
                }
        }

        public int mapId;
        public int oriCfgNodeId;
        public int groupId;        //元素分组

        bool m_bBlock = false;
        public bool IsBlock
        {
                get
                {
                        return m_bBlock;
                }
                set
                {
                        m_bBlock = value;
                        if (m_bBlock && IsInteractive())
                        {
                                GameUtility.SetLayer(this.gameObject, "NpcObj");
                        }
                        else
                        {
                                GameUtility.SetLayer(this.gameObject, m_bBlock ? "BlockObj" : "NonBlockObj");
                        }
                }
        }
        public GameObject floorObj;
        public GameObject elemObj;

        public List<int> floorList = new List<int>();
        public RaidElemConfig _elemCfg;
        public RaidElemConfig elemCfg
        {
                get
                {
                        return _elemCfg;
                }
                set
                {
                        _elemCfg = value;

                }
        }
        public RaidNodeStruct nodeCfg;  //编辑器导出的配置

        public int elemCount;
        public List<int> compOptions;
        public List<int> compOptionEffects;

        public RaidRoomBehav belongRoom;
        public int ElemId
        {
                get
                {
                        if (elemCfg != null)
                        {
                                return elemCfg.id;
                        }
                        return 0;
                }
        }
        public bool IsElemDone()
        {
                if (elemCfg != null /*&& elemObj != null*/)
                {
                        if (elemCfg.result_number > 0 && elemCount >= elemCfg.result_number)
                        {
                                return true;
                        }
                }
                return false;
        }

        public bool IsExit()
        {
                if (elemCfg != null)
                {
                        if (elemCfg.type == (int)RAID_ELEMENT_TYPE.EXIT)
                        {
                                return true;
                        }
                }
                return false;
        }

        public bool IsInteractive()
        {
                if (elemCfg != null && elemCfg.is_direct_result == 1)
                {
                        return true;
                }
                return false;
        }

        public bool IsCharacterType()
        {
                return elemCfg != null && elemCfg.IsCharacter();
        }
        public bool IsDoor()
        {
                return elemCfg != null && elemCfg.IsDoor();
        }
        public bool IsDice()
        {
                return elemCfg != null && elemCfg.type == (int)RAID_ELEMENT_TYPE.DICE;
        }
        public bool CanUnlockBuild()
        {
                if (elemCfg != null && elemCfg.type == (int)RAID_ELEMENT_TYPE.UNLOCK_BUILDING)
                {
                        if (elemCfg.unlock_building.Count > 1)
                        {
                                if (CommonDataManager.GetInst().IsBuildUnlock(elemCfg.unlock_building[0]) == false)
                                {
                                        return true;
                                }
                        }
                }
                return false;
        }

        public bool IsOptionElem()
        {
                return elemCfg != null && elemCfg.type == (int)RAID_ELEMENT_TYPE.OPTION_NPC;
        }

        public Vector3 GetCenterPosition()
        {
                BoxCollider box = this.GetComponent<BoxCollider>();
                if (box != null)
                {
                                                
                        return this.transform.position + this.transform.right * box.center.x + this.transform.forward * box.center.z;

                }
                return this.transform.position;
        }

        Highlighter Highlighter
        {
                get
                {
                        if (elemObj != null)
                        {
                                Highlighter h = elemObj.GetComponent<Highlighter>();
                                if (h == null)
                                {
                                        h = elemObj.AddComponent<Highlighter>();
                                        h.SeeThroughOff();
                                        h.OccluderOn();
                                }
                                return h;
                        }
                        return null;
                }
        }
        public void StarHighlighter()
        {
                if (Highlighter != null)
                {
                        Highlighter.ReinitMaterials();
                        Highlighter.ConstantOn(Color.white);
                        //Highlighter.FlashingOn();
                }
        }
        public void EndHighlighter()
        {
                if (Highlighter != null)
                {
                        Highlighter.Off();
                }
        }

        public void OnPointerClick(PointerEventData data)
        {
                if (GameStateManager.GetInst().GameState == GAMESTATE.RAID_PLAYING)
                {
                        if (elemObj != null)
                        {
                                RaidManager.GetInst().SelectTargetNode(this);
                                if (GuideUI != null)
                                {
                                        GuideUI.OnClickNode();
                                        GameObject.Destroy(GuideUI.gameObject);
                                        GuideUI = null;                                        
                                }
                        }
                }
        }

        void OnMouseDown()
        {
                if (GameStateManager.GetInst().GameState == GAMESTATE.RAID_EDITOR)
                {
                        RaidEditor.GetInst().SelectNode(this);
                }
        }

        void OnTriggerEnter(Collider other)
        {
                //Debuger.LogWarning("OnTriggerEnter " + this.id + " " + other.name);
                RaidManager.GetInst().SendTriggerHideTrap(this, other);
        }

        public float FloorY
        {
                get
                {
                        float fOffset = -1f;    //某种历史原因，高度0，砖块实际是在地下，所以默认减1
                        if (floorObj != null)
                        {
                                fOffset += floorObj.transform.localPosition.y;
                        }
                        else if (belongRoom != null)
                        {
                                fOffset += belongRoom.floorHeight;
                        }
                        return fOffset;
                }
        }
        public void UpdateBlockState()
        {
                Vector3 size = Vector3.one;
                bool bBlock = false;
                size = new Vector3(1f, floorList.Count, 1f);

                //if (IsInteractive())
                //{
                        if (elemObj != null)
                        {
                                if ((IsElemDone() && elemCfg.is_result_stop == 1) ||
                                        (!IsElemDone() && elemCfg.is_stop == 1) ||
                                        IsExit())
                                {
                                        bBlock = true;

                                        if (FloorY + floorList.Count > elemCfg.size.y + elemObj.transform.localPosition.y)
                                        {
                                                size = new Vector3(elemCfg.size.x, floorList.Count, elemCfg.size.z);
                                        }
                                        else
                                        {
                                                size = new Vector3(elemCfg.size.x, 0 - FloorY + elemCfg.size.y, elemCfg.size.z);
                                        }
                                }
                                else
                                {
                                        size = new Vector3(elemCfg.size.x, 0 - FloorY, elemCfg.size.z);
                                }
                        }
                //}
                //else 
                if (IsInteractive() == false && bBlock == false)
                {
                        bBlock = (FloorY + floorList.Count) != 0;
                }
                SetBoxSize(size);
                IsBlock = bBlock;
                GameUtility.ReScanPath();
        }
        public void SetBoxSize(Vector3 size)
        {
                BoxCollider box = this.GetComponent<BoxCollider>();
                if (box == null)
                {
                        box = this.gameObject.AddComponent<BoxCollider>();
                }
                box.size = size;
                box.center = new Vector3(size.x / 2f - 0.5f, FloorY + (size.y / 2f), size.z / 2f - 0.5f);
                box.isTrigger = false;
        }

        NODE_STATE m_AnimState = NODE_STATE.NONE;
        string GetAnimName(NODE_STATE state)
        {
                switch (state)
                {
                        case NODE_STATE.IDLE:
                                {
                                        return CommonString.idle_001Str;
                                }
                                break;
                        case NODE_STATE.OPERATING:
                                if (elemCfg != null && !string.IsNullOrEmpty(elemCfg.operation_action))
                                {
                                        return elemCfg.operation_action;
                                }
                                break;
                        case NODE_STATE.RESULT:
                                if (elemCfg != null && !string.IsNullOrEmpty(elemCfg.result_action))
                                {
                                        return elemCfg.result_action;
                                }
                                break;
                }
                return "";
        }

        public float PlayAnim(NODE_STATE state, bool bLoop = false)
        {
                if (state == m_AnimState)
                {
                        return 0f;
                }
                string animName = GetAnimName(state);
                if (!string.IsNullOrEmpty(animName) && !animName.Equals(CommonString.zeroStr))
                {
                        if (animName == CommonString.idle_001Str)
                        {
                                bLoop = true;
                        }
                        GameUtility.ObjPlayAnim(elemObj, animName, bLoop);
                        m_AnimState = state;

                        return GameUtility.GetAnimTime(elemObj, animName);
                }
                return 0f;
        }
        public void PlayResultEffect()
        {
                if (elemCfg != null)
                {
                        if (!string.IsNullOrEmpty(elemCfg.result_effect))
                        {
                                EffectManager.GetInst().PlayEffect(elemCfg.result_effect, GetCenterPosition());
                        }
                        if (!string.IsNullOrEmpty(elemCfg.result_sound))
                        {
                                AudioManager.GetInst().PlaySE(elemCfg.result_sound);
                        }
                }
        }
        public void PlayOperatingEffect()
        {
                if (elemCfg != null)
                {
                        if (!string.IsNullOrEmpty(elemCfg.operation_effect))
                        {
                                EffectManager.GetInst().PlayEffect(elemCfg.operation_effect, this.transform, true);
                        }
                }
        }

        public void StopPlay()
        {
                if (elemObj != null)
                {
                        GameUtility.ObjStopAnim(elemObj);
                }
        }
        public void Replay()
        {
                m_AnimState = NODE_STATE.NONE;
                StartPlay();
        }
        public void StartPlay()
        {
                if (elemObj != null)
                {
                        if (IsElemDone())
                        {
                                PlayAnim(NODE_STATE.OPERATING);
                                PlayAnim(NODE_STATE.RESULT);
                        }
                        else
                        {
                                PlayAnim(NODE_STATE.IDLE);
                        }
                }
        }
        public void InitFloorObjs(float floorHeight = 0f)
        {
                if (floorObj != null)
                {
                        GameObject.Destroy(floorObj);
                        floorObj = null;
                }

                for (int i = 0; i < floorList.Count; i++)
                {
                        string branch = "";
                        //i == 0 ? "floor" : "";

                        //string branch = BrickSceneManager.GetInst().GetMeshLinkState(posX, posY, i);
                        GameObject obj = ModelResourceManager.GetInst().GenerateCommonObject(floorList[i], branch);
                        if (obj != null)
                        {
                                if (floorObj == null)
                                {
                                        floorObj = new GameObject();
                                }
                                obj.transform.SetParent(floorObj.transform);
                                obj.transform.localPosition = Vector3.up * i;
                        }
                }
                if (floorObj != null)
                {
                        floorObj.name = "FloorObj_" + this.id;
                        floorObj.transform.SetParent(this.transform);
                        floorObj.transform.localPosition = Vector3.up * floorHeight;

                        ArrangeNode(floorObj, true);
                }
        }
        public void InitFloorMesh(List<int> list = null)
        {
                if (list == null)
                {
                        list = floorList;
                }
                for (int i = 0; i < list.Count; i++)
                {
                        if (list[i] <= 0)
                                continue;

                        //this.belongRoom.AddMesh(posX, posY, i, list[i]);
                }
        }

        public void InitElemObj()
        {
                if (elemCfg == null)
                {
                        return;
                }

                if (Application.loadedLevelName != "RaidEditor")
                {
                        if (elemCfg.CanShow() == false)
                        {
                                return;
                        }
                        if (elemCfg.mainModel == 6201)
                        {
                                return;
                        }
                }

                if (elemCfg.IsCharacter())
                {
                        elemObj = ModelResourceManager.GetInst().GenerateObject(elemCfg.mainModel);
                        GameUtility.ObjPlayAnim(elemObj, CommonString.idle_001Str, true);
                }
                else
                {
                        for (int i = 0; i < elemCfg.model.Count; i++)
                        {
                                if (!ModelResourceManager.GetInst().IsCommonCube(elemCfg.model[i]))
                                {
                                        GameObject obj = ModelResourceManager.GetInst().GenerateObject(elemCfg.model[i]);
                                        if (obj != null)
                                        {
                                                if (elemObj == null)
                                                {
                                                        elemObj = new GameObject();
                                                }
                                                obj.transform.SetParent(elemObj.transform);

                                                obj.transform.localPosition = new Vector3(0f, i, 0f);
                                                if (nodeCfg != null)
                                                {
                                                        obj.transform.localPosition += Vector3.up * nodeCfg.elem_posy;
                                                }
                                        }
                                }
                        }
                }
                if (elemObj != null)
                {
                        elemObj.transform.SetParent(this.transform);
                        elemObj.name = "ElemObj_" + this.id;
                        elemObj.transform.localPosition = new Vector3(elemCfg.size.x / 2f - 0.5f, 0f, elemCfg.size.z / 2f - 0.5f);

                        if (nodeCfg != null && this.nodeCfg.elem_rot > 0)
                        {
                                elemObj.transform.localRotation = Quaternion.Euler(new Vector3(0f, this.nodeCfg.elem_rot, 0f));
                        }
                        else
                        {
                                elemObj.transform.localRotation = Quaternion.identity;
                        }

                        if (IsInteractive())
                        {
                                SetElemInteractive();
                        }
                        else
                        {
                                ArrangeNode(elemObj);
                        }
                }
        }

        public void ResetElemObj(bool bPlayAnim = true)
        {
                GameObject.Destroy(this.elemObj);
                this.elemObj = null;
                InitElemObj();
                UpdateBlockState();
                SetElemInteractive();
                CheckInteractiveElemIcon();
                if (bPlayAnim)
                {
                        StartPlay();
                }
        }
        public void RewindAnim()
        {
                string animname = GetAnimName(m_AnimState);
                GameUtility.RewindAnim(elemObj, animname);
                m_AnimState = NODE_STATE.IDLE;
        }

        

        public void SetElemInteractive()
        {
                if (elemObj != null)
                {
                        elemObj.tag = CommonString.interactiveStr;
                        foreach (MeshFilter mf in elemObj.GetComponentsInChildren<MeshFilter>(true))
                        {
                                mf.gameObject.tag = CommonString.interactiveStr;
                                //mf.gameObject.AddComponent<MeshCollider>();
                        }
                        foreach (SkinnedMeshRenderer smr in elemObj.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                        {
                                smr.gameObject.tag = CommonString.interactiveStr;
                                //smr.gameObject.AddComponent<BoxCollider>();
                        }
                }
        }
        public void SetFloorInteractive()
        {
                if (floorObj != null)
                {
                        floorObj.tag = CommonString.interactiveStr;
                        foreach (MeshFilter mf in floorObj.GetComponentsInChildren<MeshFilter>(true))
                        {
                                mf.gameObject.tag = CommonString.interactiveStr;
                        }
                }
        }

        /// <summary>
        /// 节点是否可用
        /// </summary>
        /// <returns></returns>
        public bool IsNodeElemAvail()
        {
                //                 if (this.elemObj == null)
                //                 {
                //                         return false;
                //                 }
                if (this.elemCfg != null && this.elemCount < this.elemCfg.result_number)
                {
                        return true;
                }
                return false;
        }

        UI_RaidElemIcon m_UIElemIcon;
        public void SetUIVisible(bool bVisible)
        {
                if (m_UIElemIcon != null)
                {
                        m_UIElemIcon.gameObject.SetActive(bVisible);
                }
        }

        public void CheckInteractiveElemIcon()
        {
                if (elemObj != null && IsNodeElemAvail() && IsInteractive())
                {
                        if (m_UIElemIcon == null)
                        {
                                GameObject uiObj = UIManager.GetInst().ShowUI_Multiple<UI_RaidElemIcon>("UI_RaidElemIcon");
                                uiObj.transform.SetParent(this.transform);
                                uiObj.name = "ElemIcon_" + this.id;
                                GameUtility.SetLayer(uiObj, "UI");
                                m_UIElemIcon = uiObj.GetComponent<UI_RaidElemIcon>();
                                m_UIElemIcon.Setup(this);
                        }
                }
                else
                {
                        if (m_UIElemIcon != null)
                        {
                                GameObject.Destroy(m_UIElemIcon.gameObject);
                                m_UIElemIcon = null;
                        }
                }
        }
        public UI_GuideScene GuideUI;


        /// <summary>
        /// 房间内节点默认自身可见（只是被房间控制可见性）
        /// 野外节点通过迷雾控制自身可见
        /// </summary>
        public void SetNodeVisible(bool bVisible = true)
        {
                if (this.elemObj != null)
                {
                        this.elemObj.SetActive(bVisible);
                }
                if (this.floorObj != null)
                {
                        this.floorObj.SetActive(bVisible);
                }
        }

        public float PlayLoading(int skillId)
        {
                if (elemCfg != null && elemCfg.operation_time > 0)
                {
                        if (m_UIElemIcon != null)
                        {
                                return elemCfg.operation_time;
                        }
                }
                return 0f;
        }

        public void ArrangeNode(GameObject tmpObj, bool bFloor = false)
        {
                if (tmpObj == null)
                        return;

                if (this.belongRoom != null)
                {
                        //门的互动元素，或者非互动元素
                        if (!IsInteractive() || IsDoor())
                        {
                                this.belongRoom.ArrangeObj(tmpObj, this);
                        }
                }
        }

        public void ShowAppear(float delay)
        {
                if (elemObj != null && elemObj.transform.parent != null /*&& elemObj.transform.parent.gameObject.activeSelf == false*/)
                {
                        StartCoroutine(UpdateShowNode(elemObj, delay, true));
                }
                if (floorObj != null && floorObj.transform.parent != null /*&& floorObj.transform.parent.gameObject.activeSelf == false*/)
                {
                        StartCoroutine(UpdateShowNode(floorObj, delay));
                }
        }

        public void ShowElemAppear(float delay)
        {
                if (elemObj != null)
                {
                        StartCoroutine(UpdateShowNode(elemObj, delay, true));
                }
        }

        IEnumerator UpdateShowNode(GameObject obj, float delay, bool bElem = false)
        {

                if (obj != null)
                {
                        obj.SetActive(false);
                }

                if (delay > 0)
                {
                        yield return new WaitForSeconds(delay);
                }
                float time = 0.5f;
                if (obj != null && obj.activeSelf == false && obj.GetComponent<iTween>() == null)
                {
                        obj.SetActive(true);
                        Vector3 pos = obj.transform.localPosition;
                        obj.transform.localPosition = obj.transform.localPosition + Vector3.up * 20f;
                        iTween.moveTo(obj, time, 0f, pos);

                }
                yield return new WaitForSeconds(time);
                if (bElem)
                {
                        EffectManager.GetInst().PlayEffect("effect_raid_element_down", this.transform);
                        UpdateBlockState();
                        StartPlay();
                }

        }

        public void ShowDisappear(float delay)
        {
                if (elemObj != null)
                {
                        StartCoroutine(UpdateNodeDisappear(elemObj, delay, true));
                }
                if (floorObj != null)
                {
                        StartCoroutine(UpdateNodeDisappear(floorObj, delay));
                }
        }


        IEnumerator UpdateNodeDisappear(GameObject obj, float delay, bool bElem = false)
        {
                if (delay > 0)
                {
                        yield return new WaitForSeconds(delay);
                }
                float time = 1f;
                if (obj != null)
                {
                        obj.SetActive(true);
                        Vector3 pos = obj.transform.localPosition -Vector3.up * 50f;
                        //obj.transform.localPosition = obj.transform.localPosition;
                        iTween.moveTo(obj, time, 0f, pos);
                }
                yield return new WaitForSeconds(time);
                obj.SetActive(false);
        }
}

public enum NODE_STATE
{
        NONE,
        IDLE,
        OPERATING,
        RESULT,
};
