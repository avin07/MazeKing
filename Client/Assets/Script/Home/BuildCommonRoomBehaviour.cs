using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using Message;
using DG.Tweening;
using System;
using HighlightingSystem;


public class BuildCommonRoomBehaviour : BuildBaseBehaviour  //通用房间类建筑
{
        GameObject WallObj;             //墙的节点
        GameObject DoorObj;             //门
        GameObject FurnitureObj;        //家具
        GameObject DecorationObj;       //装饰物
        GameObject DrawingObj;          //图纸的节点
        CubeManager cm;

        protected override void GetModel(BuildInfo build_info, bool isNew)
        {
                InitRoot();

                Vector2 pos = build_info.posClient;
                cm = gameObject.AddComponent<CubeManager>();
                int height = HomeManager.GetInst().GetHeightByPos(pos);
                transform.position = new Vector3(pos.x, height, pos.y);
                //墙
                CreatWall(build_info);
                //门
                CreatDoor(build_info);
                //图纸
                //CreatDrawing(build_info);
                //创建装饰物
                CreatDecoration(build_info);

                if (isNew)  //要表现生成效果
                {
                        AppMain.GetInst().StartCoroutine(CreatFall());
                }
                else
                {
                        Transform tf;
                        for (int i = 0; i < WallObj.transform.childCount; i++)
                        {
                                tf = WallObj.transform.GetChild(i);
                                tf.SetActive(true);
                                tf.SetLocalPositionY(tf.transform.localPosition.y - fall_height);
                        }
                        cm.ManualCombine(WallObj.transform, true, true);
                        if (DoorObj != null)
                        {
                                DoorObj.SetActive(true);
                                DoorObj.transform.SetLocalPositionY(0);
                        }
                }
        }


        void InitRoot()
        {
                if (WallObj == null)
                {
                        WallObj = new GameObject("Wall");
                        WallObj.transform.SetParent(transform);
                        WallObj.transform.localPosition = Vector3.zero;
                }

                if (FurnitureObj == null)
                {
                        FurnitureObj = new GameObject("FurnitureObj");
                        FurnitureObj.transform.SetParent(transform);
                        FurnitureObj.transform.localPosition = Vector3.zero;
                }

                if (DecorationObj == null)
                {
                        DecorationObj = new GameObject("DecorationObj");
                        DecorationObj.transform.SetParent(transform);
                        DecorationObj.transform.localPosition = Vector3.zero;
                }

                if (DrawingObj == null)
                {
                        DrawingObj = new GameObject("Drawing");
                        DrawingObj.transform.SetParent(transform);
                        DrawingObj.transform.localPosition = Vector3.zero;
                }
        }

        readonly int fall_height = 10;  //掉落高度
        void CreatDecoration(BuildInfo build_info)  //创建装饰物（无功能）
        {
                RoomLayoutConfig rlc = HomeManager.GetInst().GetRoomLayoutConfig(build_info.layout);
                string decoration = rlc.room_decoration;
                if (GameUtility.IsStringValid(decoration, CommonString.commaStr))
                {
                        string[] temp = decoration.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < temp.Length; i++)
                        {
                                string[] detail = temp[i].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                if (detail.Length < 3)
                                {
                                        //singleton.GetInst().ShowMessage(ErrorOwner.designer, build_info.build_id + "装饰物格式错误");
                                        continue;
                                }

                                int index = int.Parse(detail[0]);
                                float height = float.Parse(detail[1]);
                                int furniture_id = int.Parse(detail[2]);
                                int rotation_y = 0;

                                if (detail.Length == 4)
                                {
                                        rotation_y = int.Parse(detail[3]);
                                }

                                BuildingInfoHold FurnitureConfig = HomeManager.GetInst().GetBuildInfoCfg(furniture_id);
                                if (FurnitureConfig != null)
                                {
                                        GameObject m_ObjectDecoration = ModelResourceManager.GetInst().GenerateObject(int.Parse(FurnitureConfig.model));
                                        if (m_ObjectDecoration != null)
                                        {
                                                m_ObjectDecoration.transform.SetParent(DecorationObj.transform);
                                                int x = index % build_info.size_x;
                                                int z = index / build_info.size_y;
                                                int size_x = FurnitureConfig.size_x;
                                                int size_y = FurnitureConfig.size_y;
                                                float offset_size_x = size_x * 0.5f - 0.5f;
                                                float offset_size_y = size_y * 0.5f - 0.5f;
                                                m_ObjectDecoration.transform.localPosition = new Vector3(x + offset_size_x, height, z + offset_size_y);
                                                m_ObjectDecoration.transform.localEulerAngles = new Vector3(0, rotation_y, 0);
                                        }
                                }
                        }
                        cm.ManualCombine(DecorationObj.transform, true, true);
                }
        }

        //void CreatDrawing(BuildInfo build_info) //图纸
        //{
        //        BuildingInfoHold build_cfg = build_info.build_cfg;
        //        int build_size = build_info.size;
        //        int x, z;

        //        GameObject drawing_piece;

        //        for (int i = 0; i < build_size * build_size; i++)
        //        {
        //                int modeid = GlobalParams.GetInt("model_drawing_element_blank");

        //                drawing_piece = ModelResourceManager.GetInst().GetDrawingObj(modeid);

        //                x = i % build_size;
        //                z = i / build_size;
        //                drawing_piece.transform.SetParent(DrawingObj.transform);
        //                drawing_piece.transform.localPosition = new Vector3(x, 0.01f, z);
        //                drawing_piece = null;
        //        }
        //        cm.ManualCombine(DrawingObj.transform, false, true);
        //        DrawingObj.SetActive(false);
        //}

        void CreatWall(BuildInfo build_info)  //创建墙
        {
                BuildingInfoHold build_cfg = build_info.buildCfg;
                int build_size = 10;
                Vector2 pos = build_info.posClient;

                RoomLayoutConfig rlc = HomeManager.GetInst().GetRoomLayoutConfig(build_info.layout);
                //if (rlc == null)
                //{
                //        singleton.GetInst().ShowMessage("不存在roomlayout" + build_info.idLayout);
                //        return;
                //}

                int build_node = rlc.wall_layout;

                BuildNodeConfig build_nodecfg = HomeManager.GetInst().GetBuildNodesConfig(build_node);
                if (build_nodecfg != null)
                {
                        Dictionary<int, List<int>> m_Dict = build_nodecfg.m_buildNodeDict;
                        List<int> m_Pos = build_nodecfg.buildNodePos;

                        int height = HomeManager.GetInst().GetHeightByPos(pos);
                        foreach (int floor in m_Dict.Keys)
                        {
                                List<int> wall = m_Dict[floor];    //墙纸用第0层的
                                int y = floor + 1;
                                for (int i = 0; i < wall.Count; i++)
                                {
                                        if (wall[i] != 0)
                                        {
                                                int x = m_Pos[i] / 100;
                                                int z = m_Pos[i] % 100;
                                                ulong linkState = cm.GetWallLink(x, y, z, build_nodecfg.m_buildHeight, build_size);
                                                string branch = GameUtility.ConvertLinkBit(linkState);
                                                GameObject obj = ModelResourceManager.GetInst().GenerateCommonObject(wall[i], branch);
                                                //GameObject obj = ModelResourceManager.GetInst().GenerateCommonObject(wall[i]);
                                                if (obj != null)
                                                {
                                                        obj.transform.SetParent(WallObj.transform);
                                                        obj.transform.localPosition = new Vector3(x, y + fall_height, z);
                                                        obj.SetActive(false);
                                                }

                                                if (y == 1)  //简单粗暴的生成影子
                                                {
                                                        for (CUBE_SHADOW_STATE css = CUBE_SHADOW_STATE.WS; css <= CUBE_SHADOW_STATE.NE; css++)
                                                        {
                                                                if (css != CUBE_SHADOW_STATE.TOP)
                                                                {
                                                                        cm.AddWallShadowMesh((int)pos.x + x, (int)pos.y + z, height, css, WallObj.transform);
                                                                }
                                                        }
                                                }
                                        }
                                }
                        }

                        //buildHeight = m_Dict.Count;
                        //if (bc != null)
                        //{
                        //        bc.size = new Vector3(build_size, buildHeight, build_size);
                        //        bc.center = new Vector3(offsetSize, buildHeight * 0.5f, offsetSize);
                        //}
                }
        }

        void CreatDoor(BuildInfo build_info)
        {
                RoomLayoutConfig rlc = HomeManager.GetInst().GetRoomLayoutConfig(build_info.layout);
                int door_model_id = rlc.door_model;
                if (door_model_id == 0)
                {
                        return; //没有配门
                }

                int x, z;
                string[] door_pos = rlc.door_position.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                //生成门
                int pos = int.Parse(door_pos[0]);
                DoorObj = ModelResourceManager.GetInst().GenerateObject(door_model_id);
                if (DoorObj != null)
                {
                        DoorObj.transform.SetParent(transform);
                        x = pos % build_info.size_x;
                        z = pos / build_info.size_y;
                        DoorObj.transform.localPosition = new Vector3(x, fall_height, z) + new Vector3(door_pos.Length * 0.5f - 0.5f, 0, 0);
                        DoorObj.SetActive(false);
                }
        }

        IEnumerator CreatFall()
        {
                InputManager.GetInst().SwitchInup(false);
                float time_pause = 0.1f;
                WaitForSeconds waitForOneWall = new WaitForSeconds(0.03f);

                yield return new WaitForSeconds(time_pause);
                Transform tf;
                float end_y;

                if (WallObj != null && WallObj.transform.childCount != 0)
                {
                        for (int i = 0; i < WallObj.transform.childCount; i++)
                        {
                                tf = WallObj.transform.GetChild(i);
                                tf.SetActive(true);
                                end_y = tf.transform.localPosition.y - fall_height;
                                tf.DOLocalMoveY(end_y, time_pause);
                                yield return waitForOneWall;
                        }
                }

                if (DoorObj != null)
                {
                        DoorObj.SetActive(true);
                        DoorObj.transform.DOLocalMoveY(0, time_pause);
                }

                yield return new WaitForSeconds(time_pause);

                if (cm != null && WallObj != null)
                {
                        cm.ManualCombine(WallObj.transform, true, true);
                }
                ShowBuildMenu();
                InputManager.GetInst().SwitchInup(true);
        }

        void SetFurnitureOccluderOn()
        {
                if (FurnitureObj != null)
                {
                        Highlighter hl = FurnitureObj.GetComponent<Highlighter>();
                        if (hl == null)
                        {
                                hl = FurnitureObj.AddComponent<Highlighter>();
                        }
                        hl.ReinitMaterials();
                        hl.OccluderOn();
                }
        }

        //public void EnterFurniture()
        //{
        //        Camera.main.transform.localEulerAngles = new Vector3(90, 0, 0);
        //        float ratio = (transform.position.y - Camera.main.transform.position.y) / Camera.main.transform.forward.y;  //固定y坐标不变//
        //        Vector3 trans = transform.position + new Vector3(offsetSize, 0, offsetSize) - ratio * Camera.main.transform.forward;                   //计算出固定y坐标下摄像机的x,y坐标。

        //        //设定一个基准 6*6 摄像机比物体高11

        //        float y = (mBuildInfo.size / 6.0f) * 11 + transform.position.y + buildHeight;
        //        Camera.main.transform.position = new Vector3(trans.x, y, trans.z);
        //        DrawingObj.SetActive(true);
        //}

        public void PlayBuildFinishEffect(Transform tf, int size_x, int size_y, Vector3 offset) //建筑完成特效
        {
                GameObject effect = EffectManager.GetInst().PlayEffect(GlobalParams.GetString("building_finish_effect"), tf);
                effect.transform.localScale = new Vector3(size_x * 0.5f, 1, size_y * 0.5f);
                effect.transform.position += new Vector3(0, 0.1f, 0) + offset;
        }

        protected override void StarHighlighter()
        {
                SetFurnitureOccluderOn();
                base.StarHighlighter();
        }

        protected override Highlighter GetHighlighter()
        {
                Highlighter h;
                h = WallObj.GetComponent<Highlighter>();
                if (h == null)
                {
                        h = WallObj.AddComponent<Highlighter>();
                        h.SeeThroughOff();
                        h.OccluderOn();
                }
                return h;
        }

        public override void UpdateBuildInfo(BuildInfo new_bi)
        {
                base.UpdateBuildInfo(new_bi);
                if (mBuildInfo.eState == EBuildState.eUpgrage)
                {
                        HideBuildBar();
                }
        }

        protected override bool CompareBuildInfo(BuildInfo new_bi)
        {
                bool needShowMenu = false;

                //if (mBuildInfo.idLayout != new_bi.idLayout)    //造型改变
                //{
                //        Delete();
                //        CreatWall(new_bi);
                //        CreatDoor(new_bi);
                //        CreatDecoration(new_bi);
                //        ChangePlane(new_bi.size);
                //        PlayRoomEffect("effect_building_room_finish_001", false);
                //        AppMain.GetInst().StartCoroutine(CreatFall());
                //}

                //int newSuitId = CompareSuit(mBuildInfo.strSuit, new_bi.strSuit);
                //if (newSuitId != 0)
                //{
                //        PlayRoomEffect("effect_building_room_finish_001", false);
                //        FurnitureSetConfig cfg = HomeManager.GetInst().GetFurnitureSetCfg(newSuitId);
                //        GameUtility.PopupMessage("完成新套装" + LanguageManager.GetText(cfg.name));
                //}

                SetFurnitureOccluderOn();
                return needShowMenu;
        }

        int CompareSuit(string oldSuit, string newSuit)
        {
                List<int> oldSuitList = oldSuit.ToList<int>('|', (s) => int.Parse(s));
                List<int> newSuitList = newSuit.ToList<int>('|', (s) => int.Parse(s));

                for (int i = 0; i < newSuitList.Count; i++)
                {
                        if (!oldSuitList.Contains(newSuitList[i]) && newSuitList[i] != 0)
                        {
                                return newSuitList[i];
                        }
                }
                return 0;
        }

        void Delete()
        {
                cm.Reset();
                GameObject.Destroy(WallObj);
                WallObj = null;
                GameObject.Destroy(DoorObj);
                DoorObj = null;
                GameObject.Destroy(DecorationObj);
                DecorationObj = null;
                InitRoot();
        }

        void ChangePlane(int size)
        {
                //mPlane.transform.localScale = new Vector3(size, size, 1);
                //mPlane.transform.localPosition = new Vector3(offsetSize, 0.05f, offsetSize);
        }

        public override void PlayCleanEffect()
        {
                //string effect_url = HomeManager.GetInst().GetBuildTypeCfg(mBuildInfo.build_id).clean_effect;
                //PlayRoomEffect(effect_url, true);
        }

        //public void PlayRoomEffect(string url, bool isRemove)
        //{
        //        Dictionary<int, List<Vector2>> m_point = new Dictionary<int, List<Vector2>>();
        //        float max = mBuildInfo.size;
        //        for (int i = 0; i < (int)max * 2 - 1; i++)
        //        {
        //                int index = i;
        //                List<Vector2> pointlist = new List<Vector2>();
        //                for (int j = 0; j <= index; j++)
        //                {
        //                        pointlist.Add(new Vector2(j, index - j));
        //                }
        //                m_point.Add(index, pointlist);
        //        }
        //        AppMain.GetInst().StartCoroutine(RoomEffect(m_point, url, isRemove));
        //}

        //IEnumerator RoomEffect(Dictionary<int, List<Vector2>> dict, string url, bool isRemove)
        //{
        //        //string effect_name = url;
        //        //GameObject effect;
        //        //Vector2 pos;
        //        //WaitForSeconds waitForEffect = new WaitForSeconds(0.08f);
        //        //foreach (List<Vector2> point in dict.Values)
        //        //{
        //        //        for (int i = 0; i < point.Count; i++)
        //        //        {
        //        //                if (point[i].x < mBuildInfo.size && point[i].y < mBuildInfo.size)
        //        //                {
        //        //                        pos = point[i] + mBuildInfo.pos;
        //        //                        effect = EffectManager.GetInst().PlayEffect(effect_name, new Vector3(pos.x, HomeManager.GetInst().GetHeightByPos((int)pos.x, (int)pos.y) + 0.2f, pos.y));
        //        //                }
        //        //        }
        //        //        yield return waitForEffect;
        //        //}
        //        //if (isRemove)
        //        //{
        //        //        yield return new WaitForSeconds(0.5f);
        //        //        Destroy(gameObject);
        //        //        HomeManager.GetInst().RemoveBuildingBehaviour(mBuildInfo.id);
        //        //}
        //}

        public override void Cancel()
        {
                string text = "";
                if (mBuildInfo.eState == EBuildState.eUpgrage)
                {

                }
        }


        void ConfirmFitmentCancel(object data)
        {
                //HomeManager.GetInst().SendFitmentCancel();
        }


        public override void Quick()
        {
                string text = String.Empty;
                if (mBuildInfo.eState == EBuildState.eUpgrage)
                {
                        int cost = Mathf.CeilToInt(restTime / GlobalParams.GetInt("diamond_time_build"));
                        text = "您确定要花费" + cost + "钻石快速完成？";
                        UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox").SetConfirmAndCancel("快速建造", text, ConfirmQuickBench, null, null);
                }
        }

        void ConfirmQuickBench(object data)
        {
                int cost = Mathf.CeilToInt(restTime / GlobalParams.GetInt("diamond_time_build"));
                if (CommonDataManager.GetInst().GetNowResourceNum("diamond") >= cost)
                {
                        //HomeManager.GetInst().SendFitmentQuick();
                }
                else
                {
                        GameUtility.PopupMessage("钻石不足！");
                }
        }

        protected override string GetFunctionStr()
        {
                string function = base.GetFunctionStr();
                if (mBuildInfo.eState == EBuildState.eWork)
                {

                }
                return function;
        }

        public void FurnitureEditor()
        {
        }

        public override void OnBuildClick(GameObject go, PointerEventData data)
        {
                base.OnBuildClick(go, data);

        }

        public override void Info()
        {

        }
}

