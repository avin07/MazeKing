//using UnityEngine;
//using System.Collections;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;
//using System.Collections.Generic;
//using Message;
//using DG.Tweening;
//using System;
//using HighlightingSystem;


//public class BuildingBehaviour : MonoBehaviour
//{

//        public BuildInfo m_build_info;

//        Build_State m_state = Build_State.None;
//        public GameObject m_plane;
//        MeshRenderer m_plane_mr;

//        public void SetMyPlane(int size)
//        {
//                m_plane = CreatPlane(size);
//                m_plane_mr = m_plane.GetComponent<MeshRenderer>();
//        }


//        public EBuildType GetBuildType()
//        {
//                return m_build_info.type;
//        }

//        Color color_allow = new Color(68 / 255.0f, 199 / 255.0f, 63 / 255.0f, 1.0f); //美术给的颜色
//        Color color_forbid = new Color(1.0f, 131 / 255.0f, 131 / 255.0f, 1.0f);

//        GameObject WallObj;             //墙的节点
//        GameObject DrawingObj;          //图纸的节点
//        GameObject DoorObj;             //门
//        GameObject FurnitureObj;        //家具
//        GameObject DecorationObj;       //装饰物
//        CubeManager cm;


//        public void SetFoundation(BuildInfo fd, int height)
//        {
//                EventTriggerListener.Get(gameObject).onClick = OnBuildClick;
//                gameObject.name = fd.id.ToString();
//                transform.position = new Vector3(fd.pos_client.x, height, fd.pos_client.y);
//                transform.localScale = new Vector3(0.1f, 1, 0.1f);
//                transform.DOScale(1, 0.5f);

//                UpdateBuildInfo(fd.id, fd);
//                BoxCollider bc = gameObject.AddComponent<BoxCollider>();

//                bc.size = new Vector3(fd.size, 0.2f, fd.size);
//                bc.center = new Vector3(0, 0.1f, 0);

//                if (HomeManager.GetInst().GetState() == HomeState.None)
//                {
//                        bc.enabled = true;
//                }
//                else
//                {
//                        bc.enabled = false;
//                }
//        }

//        模型
//        readonly int fall_height = 10;  //掉落高度
//        readonly int magic_num = 10000; //用来区分玩家可操作建筑和其他家园内只能看的建筑
//        public void SetBuild(BuildInfo build_info, bool isNew)
//        {
//                先简单处理             
//                long id = build_info.id;

//                if (id > magic_num)
//                {
//                        EventTriggerListener.Get(gameObject).onClick = OnBuildClick;
//                }

//                BuildingInfoHold build_cfg = build_info.build_cfg;
//                int build_size = build_info.size;
//                Vector2 pos = build_info.pos_client;
//                gameObject.name = "Build" + id;
//                int build_height = 0;
//                if (build_info.type == EBuildType.eObstacle)  //装饰物直接使用模型
//                {
//                        GameObject model = ModelResourceManager.GetInst().GenerateObject(build_cfg.model);
//                        model.transform.SetParent(transform);
//                        model.transform.localPosition = Vector3.zero;
//                        build_height = 4;  //随便调的
//                }
//                else                                          //房间使用方块拼接
//                {
//                        if (id > magic_num)
//                        {
//                                int can_be_moved = HomeManager.GetInst().GetBuildTypeCfg(build_info.build_id).can_be_moved;
//                                if (can_be_moved == 1)
//                                {
//                                        EventTriggerListener.Get(gameObject).onDarg = OnBuildDarg;
//                                        EventTriggerListener.Get(gameObject).onBeginDarg = OnBeginBuildDrag;
//                                        EventTriggerListener.Get(gameObject).onEndDarg = OnEndBuildDrag;
//                                }
//                        }

//                        cm = gameObject.AddComponent<CubeManager>();
//                        int height = HomeManager.GetInst().GetHeightByPos(pos);
//                        transform.position = new Vector3(pos.x, height, pos.y);

//                        墙
//                        List<int> WallList = new List<int>();
//                        CreatWall(build_info, ref WallList);
//                        int wall_height = build_cfg.wall_height;
//                        build_height = wall_height;

//                        门
//                        List<int> DoorList = new List<int>();
//                        CreatDoor(build_info, ref DoorList);

//                        家具位置
//                        Dictionary<string, List<int>> FurnitureDict = new Dictionary<string, List<int>>();
//                        CreatFurniture(build_info, ref FurnitureDict);

//                        CreatDrawing(build_info, WallList, DoorList, FurnitureDict);

//                        创建装饰物
//                        BuildingInfoHold buildcfg = HomeManager.GetInst().GetBuildInfoCfg(build_info.build_id * 100);
//                        CreatDecoration(build_info, buildcfg.room_decoration);

//                        if (isNew)  //要表现生成效果
//                        {
//                                AppMain.GetInst().StartCoroutine(CreatFall(WallObj, DoorObj, cm));
//                        }
//                        else
//                        {

//                                Transform tf;
//                                for (int i = 0; i < WallObj.transform.childCount; i++)
//                                {
//                                        tf = WallObj.transform.GetChild(i);
//                                        tf.SetActive(true);
//                                        tf.SetLocalPositionY(tf.transform.localPosition.y - fall_height);
//                                }
//                                cm.ManualCombine(WallObj.transform, true, true);

//                                if (DoorObj != null)
//                                {
//                                        DoorObj.SetActive(true);
//                                        DoorObj.transform.SetLocalPositionY(0);
//                                }
//                        }
//                }

//                if (id > magic_num)
//                {
//                        UpdateBuildInfo(build_info.id, build_info);
//                        SetMyPlane(build_info.size);
//                        BoxCollider bc = gameObject.AddComponent<BoxCollider>();
//                        bc.size = new Vector3(build_size, build_height, build_size);
//                        bc.center = new Vector3(build_size * 0.5f - 0.5f, build_height * 0.5f, build_size * 0.5f - 0.5f);
//                }
//        }


//        void CreatDecoration(BuildInfo build_info, string decoration)
//        {
//                if (GameUtility.IsStringValid(decoration, ","))
//                {
//                        DecorationObj = new GameObject("DecorationObj");
//                        DecorationObj.transform.SetParent(transform);
//                        DecorationObj.transform.localPosition = Vector3.zero;

//                        string[] temp = decoration.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
//                        for (int i = 0; i < temp.Length; i++)
//                        {
//                                string[] detail = temp[i].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

//                                int index = int.Parse(detail[0]);
//                                int height = int.Parse(detail[1]);
//                                int furniture_id = int.Parse(detail[2]);
//                                int rotation_y = 0;

//                                if (detail.Length == 4)
//                                {
//                                        rotation_y = int.Parse(detail[3]);
//                                }

//                                BuildingFurnitureConfig FurnitureConfig = HomeManager.GetInst().GetFurnitureCfg(furniture_id);
//                                if (FurnitureConfig != null)
//                                {
//                                        GameObject m_ObjectDecoration = ModelResourceManager.GetInst().GenerateObject(FurnitureConfig.model);
//                                        if (m_ObjectDecoration != null)
//                                        {
//                                                m_ObjectDecoration.transform.SetParent(DecorationObj.transform);
//                                                int x = index % build_info.size;
//                                                int z = index / build_info.size;
//                                                int size_x = FurnitureConfig.size_x;
//                                                int size_y = FurnitureConfig.size_y;
//                                                float offset_size_x = size_x * 0.5f - 0.5f;
//                                                float offset_size_y = size_y * 0.5f - 0.5f;
//                                                m_ObjectDecoration.transform.localPosition = new Vector3(x + offset_size_x, height, z + offset_size_y);
//                                                m_ObjectDecoration.transform.localEulerAngles = new Vector3(0, rotation_y, 0);
//                                        }

//                                }
//                        }
//                        cm.ManualCombine(DecorationObj.transform, true, true);
//                }
//        }


//        GameObject EmptyRoot;
//        void CreatWall(BuildInfo build_info, ref List<int> WallList)
//        {
//                BuildingInfoHold build_cfg = build_info.build_cfg;
//                int build_size = build_info.size;
//                Vector2 pos = build_info.pos_client;

//                WallObj = new GameObject("Wall");
//                WallObj.transform.SetParent(transform);
//                WallObj.transform.localPosition = Vector3.zero;

//                EmptyRoot = new GameObject("EmptyRoot");
//                EmptyRoot.transform.SetParent(transform);
//                EmptyRoot.transform.localPosition = Vector3.zero;

//                int build_node = build_cfg.building_nodes;
//                if (build_node <= 0)     //旧版本（以后要删掉）       
//                {
//                        int height = HomeManager.GetInst().GetHeightByPos(pos);
//                        int x, z;
//                        墙
//                        string[] wall_pos = build_cfg.wall_position.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
//                        for (int i = 0; i < wall_pos.Length; i++)
//                        {
//                                WallList.Add(int.Parse(wall_pos[i]));
//                        }
//                        int wall_height = build_cfg.wall_height;
//                        for (int y = 1; y < wall_height + 1; y++)
//                        {
//                                for (int i = 0; i < WallList.Count; i++)
//                                {
//                                        x = WallList[i] % build_size;
//                                        z = WallList[i] / build_size;

//                                        ulong linkState = HomeManager.GetInst().GetWallLink(x, y, z, WallList, build_size, wall_height);
//                                        string branch = GameUtility.ConvertLinkBit(linkState);
//                                        GameObject obj = ModelResourceManager.GetInst().GenerateCommonObject(build_cfg.wall_model, branch);
//                                        if (obj != null)
//                                        {
//                                                obj.transform.SetParent(WallObj.transform);
//                                                obj.transform.localPosition = new Vector3(x, y + fall_height, z);
//                                                obj.SetActive(false);
//                                        }

//                                        GameObject temp = ModelResourceManager.GetInst().GenerateCommonObject(build_cfg.wall_model, "");
//                                        if (temp != null)
//                                        {
//                                                temp.transform.SetParent(EmptyRoot.transform);
//                                                temp.transform.localPosition = new Vector3(x, y + fall_height, z);
//                                                temp.SetActive(false);
//                                        }

//                                        if (y == 1)  //简单粗暴的生成影子
//                                        {
//                                                for (CUBE_SHADOW_STATE css = CUBE_SHADOW_STATE.WS; css <= CUBE_SHADOW_STATE.NE; css++)
//                                                {
//                                                        if (css != CUBE_SHADOW_STATE.TOP)
//                                                        {
//                                                                cm.AddWallShadowMesh((int)pos.x + x, (int)pos.y + z, height, css);
//                                                        }
//                                                }
//                                        }
//                                }
//                        }
//                }
//                else
//                {
//                        BuildNodeConfig build_nodecfg = HomeManager.GetInst().GetBuildNodesConfig(build_node);
//                        if (build_nodecfg != null)
//                        {
//                                Dictionary<int, List<int>> m_Dict = build_nodecfg.m_buildNodeDict;
//                                List<int> m_Pos = build_nodecfg.buildNodePos;
//                                int max_heigh = build_nodecfg.max_height;
//                                if (m_Dict.Count > 0)
//                                {
//                                        List<int> wall = m_Dict[0];    //墙纸用第0层的
//                                        for (int i = 0; i < wall.Count; i++)
//                                        {
//                                                if (wall[i] != 0)
//                                                {
//                                                        int x = m_Pos[i] / 100;
//                                                        int z = m_Pos[i] % 100;
//                                                        WallList.Add(x + z * build_size);
//                                                }
//                                        }
//                                }

//                                int height = HomeManager.GetInst().GetHeightByPos(pos);

//                                foreach (int floor in m_Dict.Keys)
//                                {
//                                        List<int> wall = m_Dict[floor];    //墙纸用第0层的
//                                        int y = floor + 1;
//                                        for (int i = 0; i < wall.Count; i++)
//                                        {
//                                                if (wall[i] != 0)
//                                                {
//                                                        int x = m_Pos[i] / 100;
//                                                        int z = m_Pos[i] % 100;
//                                                        ulong linkState = HomeManager.GetInst().GetWallLink(x, y, z, WallList, build_size, max_heigh);
//                                                        string branch = GameUtility.ConvertLinkBit(linkState);
//                                                        GameObject obj = ModelResourceManager.GetInst().GenerateCommonObject(wall[i], branch);
//                                                        GameObject obj = ModelResourceManager.GetInst().GenerateCommonObject(wall[i]);
//                                                        if (obj != null)
//                                                        {
//                                                                obj.transform.SetParent(WallObj.transform);
//                                                                obj.transform.localPosition = new Vector3(x, y + fall_height, z);
//                                                                obj.SetActive(false);
//                                                        }
//                                                        if (y == 1)  //简单粗暴的生成影子
//                                                        {
//                                                                for (CUBE_SHADOW_STATE css = CUBE_SHADOW_STATE.WS; css <= CUBE_SHADOW_STATE.NE; css++)
//                                                                {
//                                                                        if (css != CUBE_SHADOW_STATE.TOP)
//                                                                        {
//                                                                                cm.AddWallShadowMesh((int)pos.x + x, (int)pos.y + z, height, css);
//                                                                        }
//                                                                }
//                                                        }
//                                                }
//                                        }
//                                }

//                        }
//                }
//        }

//        void CreatDoor(BuildInfo build_info, ref List<int> DoorList)
//        {
//                BuildingInfoHold build_cfg = build_info.build_cfg;
//                int build_size = build_info.size;

//                int x, z;
//                string[] door_pos = build_cfg.door_position.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
//                for (int i = 0; i < door_pos.Length; i++)
//                {
//                        DoorList.Add(int.Parse(door_pos[i]));
//                }
//                int door_model_id = build_cfg.door_model;

//                生成门
//                DoorObj = ModelResourceManager.GetInst().GenerateObject(door_model_id);
//                if (DoorObj != null)
//                {
//                        DoorObj.transform.SetParent(transform);
//                        x = DoorList[0] % build_size;
//                        z = DoorList[0] / build_size;
//                        DoorObj.transform.localPosition = new Vector3(x, fall_height, z) + new Vector3(DoorList.Count * 0.5f - 0.5f, 0, 0);
//                        DoorObj.SetActive(false);
//                }
//        }

//        void CreatFurniture(BuildInfo build_info, ref Dictionary<string, List<int>> FurnitureDict)
//        {
//                BuildingInfoHold build_cfg = build_info.build_cfg;
//                int build_size = build_info.size;

//                FurnitureDict = GetFurnitureDict(build_cfg, build_size);

//                int building_index = 0; //建造中家具
//                Dictionary<int, int> m_HasFurniture = HomeManager.GetInst().GetBuildHasFurniture(build_cfg.pre_furniture_formula, build_info.strfitment, build_info.building_fitment, ref building_index);
//                Dictionary<int, int> up_needFurniture = GameUtility.ParseCommonStringToDict(build_cfg.up_furniture_formula, ';', ',');
//                FurnitureObj = new GameObject("Furniture");
//                FurnitureObj.transform.SetParent(transform);
//                FurnitureObj.transform.localPosition = Vector3.zero;
//                foreach (int index in up_needFurniture.Keys)//在家具位置生成可以点击特效//
//                {
//                        if (!m_HasFurniture.ContainsKey(index)) //当前位置没有家具
//                        {
//                                SetFurnitureEffect(index, up_needFurniture[index], FurnitureObj, build_size);
//                        }
//                        else
//                        {
//                                if (m_HasFurniture[index] != up_needFurniture[index]) //当前位置可以升级家具
//                                {
//                                        SetFurnitureEffect(index, up_needFurniture[index], FurnitureObj, build_size);
//                                }
//                        }
//                }
//                foreach (int index in m_HasFurniture.Keys)  //家具
//                {
//                        if (index == building_index)
//                        {
//                                SetFurniture(index, m_HasFurniture[index], FurnitureObj, build_size, true);
//                        }
//                        else
//                        {
//                                SetFurniture(index, m_HasFurniture[index], FurnitureObj, build_size);
//                        }
//                }
//        }


//        Dictionary<string, List<int>> GetFurnitureDict(BuildingInfoHold build_cfg, int build_size)
//        {
//                Dictionary<string, List<int>> FurnitureDict = new Dictionary<string, List<int>>();
//                string up_furniture_formula = build_cfg.up_furniture_formula;
//                string[] one_furniture_data = up_furniture_formula.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
//                int furniture_index = 0;
//                int furniture_id = 0;
//                for (int i = 0; i < one_furniture_data.Length; i++)
//                {
//                        string[] furniture = one_furniture_data[i].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
//                        if (furniture.Length == 2)
//                        {
//                                furniture_index = int.Parse(furniture[0]);
//                                furniture_id = int.Parse(furniture[1]);
//                                BuildingFurnitureConfig FurnitureConfig = HomeManager.GetInst().GetFurnitureCfg(furniture_id);
//                                if (FurnitureConfig != null)
//                                {
//                                        int furniture_size_x = FurnitureConfig.size_x;
//                                        int furniture_size_y = FurnitureConfig.size_y;
//                                        List<int> furniture_pos_list = new List<int>();
//                                        for (int j = 0; j < furniture_size_y; j++)
//                                        {
//                                                for (int k = 0; k < furniture_size_x; k++)
//                                                {
//                                                        furniture_pos_list.Add(j * build_size + furniture_index + k);
//                                                }
//                                        }
//                                        FurnitureDict.Add(one_furniture_data[i], furniture_pos_list);
//                                }
//                                else
//                                {
//                                        singleton.GetInst().ShowMessage(ErrorOwner.designer, "家具" + furniture_id + "不存在");
//                                }
//                        }
//                        else
//                        {
//                                singleton.GetInst().ShowMessage(ErrorOwner.designer, "building_info表" + build_cfg.id + "家具格式错误");
//                        }
//                }
//                return FurnitureDict;
//        }

//        void CreatDrawing(BuildInfo build_info, List<int> WallList, List<int> DoorList, Dictionary<string, List<int>> FurnitureDict) //图纸
//        {
//                BuildingInfoHold build_cfg = build_info.build_cfg;
//                int build_size = build_info.size;
//                int x, z;
//                生成图纸
//                if (DrawingObj == null)
//                {
//                        DrawingObj = new GameObject("Drawing");
//                        DrawingObj.transform.SetParent(transform);
//                        DrawingObj.transform.localPosition = Vector3.zero;
//                }

//                if (build_info.work_level <= build_info.level)  //功能等级大于建筑等级，不需要显示没有图纸
//                {
//                        GameObject drawing_piece;
//                        for (int i = 0; i < build_size * build_size; i++)
//                        {
//                                int modeid = 0;
//                                if (WallList.Contains(i))// 墙
//                                {
//                                        modeid = GlobalParams.GetInt("model_drawing_element_wall");
//                                }
//                                else if (DoorList.Contains(i)) //门
//                                {
//                                        modeid = GlobalParams.GetInt("model_drawing_element_door");
//                                }
//                                else
//                                {
//                                        foreach (string name in FurnitureDict.Keys)  //家具
//                                        {
//                                                List<int> FurnitureList = FurnitureDict[name];
//                                                if (FurnitureList.Contains(i))
//                                                {
//                                                        if (FurnitureList.Count == 1)
//                                                        {
//                                                                modeid = GlobalParams.GetInt("model_drawing_element_furniture_1");
//                                                        }
//                                                        else
//                                                        {
//                                                                modeid = GlobalParams.GetInt("model_drawing_element_furniture_2");
//                                                        }
//                                                }
//                                        }

//                                        if (modeid == 0)
//                                        {
//                                                modeid = GlobalParams.GetInt("model_drawing_element_blank");
//                                        }
//                                }

//                                drawing_piece = ModelResourceManager.GetInst().GetDrawingObj(modeid);

//                                x = i % build_size;
//                                z = i / build_size;
//                                drawing_piece.transform.SetParent(DrawingObj.transform);
//                                drawing_piece.transform.localPosition = new Vector3(x, 0.01f, z);
//                                drawing_piece = null;
//                        }
//                        cm.ManualCombine(DrawingObj.transform, false, true);
//                }
//        }

//        IEnumerator CreatFall(GameObject wall, GameObject door, CubeManager cm)
//        {
//                InputManager.GetInst().SwitchInup(false);
//                float time_pause = 0.1f;
//                float time_wall = 0.03f;

//                yield return new WaitForSeconds(time_pause);
//                Transform tf;
//                float end_y;


//                if (EmptyRoot != null)
//                {
//                        for (int i = 0; i < EmptyRoot.transform.childCount; i++)
//                        {
//                                tf = EmptyRoot.transform.GetChild(i);
//                                tf.SetActive(true);
//                                end_y = tf.transform.localPosition.y - fall_height;
//                                tf.DOLocalMoveY(end_y, time_pause);
//                                yield return new WaitForSeconds(time_wall);
//                        }
//                }


//                if (door != null)
//                {
//                        door.SetActive(true);
//                        door.transform.DOLocalMoveY(0, time_pause);
//                }

//                yield return new WaitForSeconds(time_pause);

//                if (wall != null)
//                {
//                        for (int i = 0; i < wall.transform.childCount; i++)
//                        {
//                                tf = wall.transform.GetChild(i);
//                                tf.SetActive(true);
//                                tf.transform.SetLocalPositionY(tf.transform.localPosition.y - fall_height);
//                        }
//                }
//                GameObject.Destroy(EmptyRoot);
//                cm.ManualCombine(wall.transform, true, true);
//                ShowBuildMenu();
//                InputManager.GetInst().SwitchInup(true);
//        }

//        public void SetBuildLevelUp(BuildInfo build_info, bool need_wall) //建筑升级 更换墙模型 生成图纸
//        {
//                BuildingInfoHold build_cfg = build_info.build_cfg;
//                int build_size = build_info.size;
//                墙
//                List<int> WallList = new List<int>();
//                string[] wall_pos = build_cfg.wall_position.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
//                for (int i = 0; i < wall_pos.Length; i++)
//                {
//                        WallList.Add(int.Parse(wall_pos[i]));
//                }
//                int wall_height = build_cfg.wall_height;

//                门
//                List<int> DoorList = new List<int>();
//                string[] door_pos = build_cfg.door_position.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
//                for (int i = 0; i < door_pos.Length; i++)
//                {
//                        DoorList.Add(int.Parse(door_pos[i]));
//                }

//                家具位置
//                Dictionary<string, List<int>> FurnitureDict = GetFurnitureDict(build_cfg, build_size);

//                int building_index = 0; //建造中家具
//                Dictionary<int, int> m_HasFurniture = HomeManager.GetInst().GetBuildHasFurniture(build_cfg.pre_furniture_formula, build_info.strfitment, build_info.building_fitment, ref building_index);
//                Dictionary<int, int> up_needFurniture = GameUtility.ParseCommonStringToDict(build_cfg.up_furniture_formula, ';', ',');
//                在家具位置生成可以点击特效//
//                foreach (int index in up_needFurniture.Keys)
//                {
//                        if (!m_HasFurniture.ContainsKey(index)) //当前位置没有家具
//                        {
//                                SetFurnitureEffect(index, up_needFurniture[index], FurnitureObj, build_size);
//                        }
//                        else
//                        {
//                                if (m_HasFurniture[index] != up_needFurniture[index]) //当前位置可以升级家具
//                                {
//                                        SetFurnitureEffect(index, up_needFurniture[index], FurnitureObj, build_size);
//                                }
//                        }
//                }

//                CreatDrawing(build_info, WallList, DoorList, FurnitureDict);

//                int x, z;
//                if (need_wall)
//                {
//                        生成墙
//                        for (int y = 1; y < wall_height + 1; y++)
//                        {
//                                for (int i = 0; i < WallList.Count; i++)
//                                {
//                                        x = WallList[i] % build_size;
//                                        z = WallList[i] / build_size;
//                                        ulong linkState = HomeManager.GetInst().GetWallLink(x, y, z, WallList, build_size, wall_height);
//                                        string branch = GameUtility.ConvertLinkBit(linkState);
//                                        GameObject obj = ModelResourceManager.GetInst().GenerateCommonObject(build_cfg.wall_model, branch);
//                                        if (obj != null)
//                                        {
//                                                obj.transform.SetParent(WallObj.transform);
//                                                obj.transform.localPosition = new Vector3(x, y, z);
//                                        }
//                                }
//                        }
//                        cm.ManualCombine(WallObj.transform, true, true);
//                }

//        }

//        GameObject CreatPlane(int size)
//        {
//                GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
//                GameObject.DestroyImmediate(plane.GetComponent<MeshCollider>());
//                plane.transform.SetParent(gameObject.transform);
//                plane.transform.localEulerAngles = new Vector3(90, 0, 0);
//                plane.transform.localScale = new Vector3(size, size, 1);

//                if (m_build_info.type == EBuildType.eFoundation)
//                {
//                        plane.transform.localPosition = new Vector3(0, 0.05f, 0);
//                }
//                else
//                {
//                        plane.transform.localPosition = new Vector3(size * 0.5f - 0.5f, 0.05f, size * 0.5f - 0.5f);
//                }

//                MeshRenderer m_mr = plane.GetComponent<MeshRenderer>();
//                m_mr.receiveShadows = false;
//                m_mr.castShadows = false;
//                plane.SetActive(false);
//                return plane;
//        }


//        void SetFurnitureEffect(int index, int furniture_id, GameObject root, int build_size)
//        {
//                GameObject m_ObjectEffect = EffectManager.GetInst().GetEffectObj("effect_furniture_build_hint");
//                m_ObjectEffect.transform.SetParent(root.transform);
//                m_ObjectEffect.name = "effect" + index + "_" + furniture_id;
//                int x = index % build_size;
//                int z = index / build_size;
//                BuildingFurnitureConfig FurnitureConfig = HomeManager.GetInst().GetFurnitureCfg(furniture_id);
//                if (FurnitureConfig != null)
//                {
//                        int furniture_size_x = FurnitureConfig.size_x;
//                        int furniture_size_y = FurnitureConfig.size_y;
//                        float offset_size_x = furniture_size_x * 0.5f - 0.5f;
//                        float offset_size_y = furniture_size_y * 0.5f - 0.5f;
//                        m_ObjectEffect.transform.localPosition = new Vector3(x + offset_size_x, 0.02f, z + offset_size_y);
//                        m_ObjectEffect.transform.localScale = new Vector3(furniture_size_x, 0, furniture_size_y);
//                        m_ObjectEffect.AddComponent<BoxCollider>();
//                        m_ObjectEffect.SetActive(false);
//                }
//        }

//        void SetFurniture(int index, int furniture_id, GameObject root, int build_size, bool isbuilding = false) //是否在建造中
//        {
//                BuildingFurnitureConfig FurnitureConfig = HomeManager.GetInst().GetFurnitureCfg(furniture_id);
//                if (FurnitureConfig != null)
//                {
//                        GameObject m_ObjectFurniture = ModelResourceManager.GetInst().GenerateObject(FurnitureConfig.model);
//                        if (m_ObjectFurniture != null)
//                        {
//                                m_ObjectFurniture.transform.SetParent(root.transform);
//                                m_ObjectFurniture.name = "furniture" + index + "_" + furniture_id;
//                                int x = index % build_size;
//                                int z = index / build_size;
//                                int furniture_size_x = FurnitureConfig.size_x;
//                                int furniture_size_y = FurnitureConfig.size_y;
//                                float offset_size_x = furniture_size_x * 0.5f - 0.5f;
//                                float offset_size_y = furniture_size_y * 0.5f - 0.5f;
//                                m_ObjectFurniture.transform.localPosition = new Vector3(x + offset_size_x, 0, z + offset_size_y);

//                                if (isbuilding)
//                                {
//                                        ChangeFurnitureColor(m_ObjectFurniture, Color.cyan);
//                                }
//                                else
//                                {
//                                        ChangeFurnitureColor(m_ObjectFurniture, Color.white);
//                                }
//                        }
//                        else
//                        {
//                                singleton.GetInst().ShowMessage(ErrorOwner.designer, "家具模型" + FurnitureConfig.model + "不存在");
//                        }
//                }
//                else
//                {
//                        singleton.GetInst().ShowMessage(ErrorOwner.designer, "家具" + furniture_id + "不存在");
//                }
//        }

//        void ChangeFurnitureColor(GameObject m_ObjectFurniture, Color color)
//        {
//                MeshRenderer[] m_all_mr = m_ObjectFurniture.GetComponentsInChildren<MeshRenderer>();
//                if (m_all_mr.Length > 0)
//                {
//                        for (int i = 0; i < m_all_mr.Length; i++)
//                        {
//                                m_all_mr[i].material.color = color;
//                        }
//                }
//                else
//                {
//                        SkinnedMeshRenderer[] m_all_Skinnedmr = m_ObjectFurniture.GetComponentsInChildren<SkinnedMeshRenderer>();
//                        for (int i = 0; i < m_all_Skinnedmr.Length; i++)
//                        {
//                                m_all_Skinnedmr[i].material.color = color;
//                        }
//                }
//        }

//        void Awake()
//        {
//                GameUtility.SetLayer(gameObject, "BlockObj");
//        }

//        Vector2 old_hit_pos;
//        void UpdateBuildingObjMove()  //统一使用射线检测//
//        {
//                RaycastHit hit = new RaycastHit();
//                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//                if (Physics.Raycast(ray, out hit, Camera.main.farClipPlane, 1 << LayerMask.NameToLayer("Scene")))
//                {
//                        转换后的客户端点击位置//
//                        float x = (int)(hit.point.x - offset_size);
//                        float z = (int)(hit.point.z - offset_size);
//                        if (x != old_hit_pos.x || z != old_hit_pos.y)
//                        {
//                                x = Mathf.Clamp(x, 0, HomeManager.HomeSize - m_build_info.size);
//                                z = Mathf.Clamp(z, 0, HomeManager.HomeSize - m_build_info.size);
//                                int y = HomeManager.GetInst().GetMaxBuildHieght((int)x, (int)z, m_build_info.size);
//                                Vector3 pos_client = new Vector3(x, y, z);
//                                Vector2 pos = new Vector2(x, z);
//                                if (transform.position != pos_client)
//                                {
//                                        transform.position = pos_client;
//                                        CanPutDown = HomeManager.GetInst().CheckCanPutDown(pos, m_build_info);
//                                }
//                                old_hit_pos = new Vector2((int)(hit.point.x - offset_size), (int)(hit.point.z - offset_size));
//                        }
//                }
//        }

//        public void OnBuildDarg(GameObject go, PointerEventData data)
//        {
//                if (m_state != Build_State.CanMove)
//                {
//                        return;
//                }

//                UpdateBuildingObjMove();
//                UpdatePlaneColor();

//        }

//        void UpdatePlaneColor()
//        {
//                if (CanPutDown)
//                {
//                        m_plane_mr.material.color = color_allow;
//                }
//                else
//                {
//                        m_plane_mr.material.color = color_forbid;
//                }
//        }

//        public void OnBeginBuildDrag(GameObject go, PointerEventData data)
//        {
//                if (m_state != Build_State.CanMove)
//                {
//                        return;
//                }
//                HomeManager.GetInst().SetState(HomeState.Move);
//                UIManager.GetInst().SetUIRender<UI_BuildingFunction>("UI_BuildingFunction", false);
//                m_plane.SetActive(true);
//        }

//        public void OnEndBuildDrag(GameObject go, PointerEventData data)
//        {
//                if (m_state != Build_State.CanMove)
//                {
//                        return;
//                }
//                BuildPutDown();
//                UIManager.GetInst().SetUIRender<UI_BuildingFunction>("UI_BuildingFunction", CanPutDown);
//                m_plane.SetActive(!CanPutDown);
//        }

//        void OnBuildClick(GameObject go, PointerEventData data)
//        {
//                BuildClick();
//        }

//        void Update()
//        {
//                if (Time.realtimeSinceStartup - BaseTime < 1.0f)
//                {
//                        return;
//                }
//                else
//                {
//                        BaseTime = Time.realtimeSinceStartup;
//                        UpdateStateCountdown();
//                        UpdateOutput();
//                }
//        }

//        public void DeleteBuildingObj() //建造状态下点击ui就清除建造建造
//        {
//                PlayerCleanEffect();
//                DestroyBuildState();
//                Destroy(gameObject);
//        }

//        public void PlayerCleanEffect()
//        {
//                if (m_build_info.type != EBuildType.eFoundation)
//                {
//                        string effect_url = HomeManager.GetInst().GetBuildTypeCfg(m_build_info.build_id).clean_effect;
//                        int height = HomeManager.GetInst().GetHeightByPos(m_build_info.pos);

//                        if (effect_url.Length > 0)
//                        {
//                                if (m_build_info.type == EBuildType.eObstacle || m_build_info.type == EBuildType.eFire)
//                                {
//                                        EffectManager.GetInst().PlayEffect(effect_url, new Vector3(m_build_info.pos.x + offset_size, height, m_build_info.pos.y + offset_size));
//                                }
//                                else
//                                {
//                                        PlayRoomEffect(effect_url);
//                                }
//                        }
//                }
//        }

//        public void SetBuildState(Build_State state)
//        {
//                m_state = state;
//                if (m_state == Build_State.CanMove)
//                {
//                        StarHighlighter();
//                }

//                if (m_state == Build_State.None)
//                {
//                        EndHighlighter();
//                        UIManager.GetInst().CloseUI("UI_BuildingFunction");
//                        HomeManager.GetInst().SetState(HomeState.None);
//                        if (m_plane != null)
//                        {
//                                m_plane.SetActive(false);
//                        }
//                }
//        }

//        public Build_State GetBuildState()
//        {
//                return m_state;
//        }

//        bool CanPutDown = true; //是否可以放下

//        Highlighter Highlighter
//        {
//                get
//                {
//                        Highlighter h;
//                        GameObject root;
//                        if (WallObj != null)
//                        {
//                                h = WallObj.GetComponent<Highlighter>();
//                                root = WallObj;
//                        }
//                        else
//                        {
//                                h = GetComponent<Highlighter>();
//                                root = gameObject;
//                        }

//                        if (h == null)
//                        {
//                                h = root.AddComponent<Highlighter>();
//                                h.SeeThroughOff();
//                                h.OccluderOn();
//                        }
//                        return h;
//                }
//        }

//        public void StarHighlighter()
//        {
//                HomeManager.GetInst().SetBrickOccluderOn();
//                SetFurnitureOccluderOn();
//                Highlighter.ReinitMaterials();
//                Highlighter.ConstantOn(Color.white);
//        }


//        void SetFurnitureOccluderOn()
//        {
//                if (FurnitureObj != null)
//                {
//                        Highlighter hl = FurnitureObj.GetComponent<Highlighter>();
//                        if (hl == null)
//                        {
//                                hl = FurnitureObj.AddComponent<Highlighter>();
//                        }
//                        hl.ReinitMaterials();
//                        hl.OccluderOn();
//                }
//        }

//        public void EndHighlighter()
//        {
//                Highlighter.Off();
//        }

//        void BuildClick()
//        {
//                if (HomeManager.GetInst().GetState() != HomeState.None)
//                {
//                        return;
//                }

//                if (m_state == Build_State.None)
//                {
//                        if (isShowResTip) //在有资源收获提示的时候优先响应收获//
//                        {
//                                HomeManager.GetInst().SendResourceGain(m_build_info.id, ResType);
//                                HideResTip();
//                                return;
//                        }
//                        ShowBuildMenu();
//                }
//                else if (m_state == Build_State.CanMove)
//                {
//                        SetBuildState(Build_State.None);
//                        HomeManager.GetInst().SetSelectBuild(null);
//                }
//        }

//        public void ShowBuildMenu()
//        {
//                string function = "";
//                if (m_build_info.type == EBuildType.eFoundation) //地基特殊处理
//                {
//                        function = "7,8";
//                }
//                else
//                {
//                        //菜单使用功能等级
//                        BuildingInfoHold build_cfg = HomeManager.GetInst().GetBuildInfoCfg(m_build_info.build_id * 100 + m_build_info.work_level);
//                        if (build_cfg == null)
//                        {
//                                return;
//                        }
//                        switch (m_build_info.e_state)
//                        {
//                                case EBuildState.eWork:
//                                        if (Formula_id > 0)
//                                        {
//                                                function = build_cfg.building_function;
//                                        }
//                                        else
//                                        {
//                                                function = build_cfg.function;
//                                        }
//                                        break;
//                                case EBuildState.eClear:
//                                        function = build_cfg.building_function;
//                                        break;
//                                case EBuildState.eBuild:
//                                        function = build_cfg.building_function;
//                                        break;
//                        }
//                }

//                //由于建造家具和升级按钮互斥，配置也没法配出来，所以根据工作等级和建筑等级关系程序来替换//
//                if (m_build_info.work_level <= m_build_info.level)  //功能等级大于建筑等级，不需要显示图纸
//                {
//                        if (function.Contains("2"))
//                        {
//                                function = function.Replace("2", "7");
//                        }
//                }

//                if (HomeManager.GetInst().GetSelectBuild() != this)
//                {
//                        HomeManager.GetInst().SetSelectBuild(this);
//                        SelectAnima();
//                        SetBuildState(Build_State.CanMove);
//                }
//                UIManager.GetInst().ShowUI<UI_BuildingFunction>("UI_BuildingFunction").RefreshMenu(function);
//        }

//        void BuildPutDown()
//        {
//                if (CanPutDown)
//                {
//                        HomeManager.GetInst().SendBuildMove(m_build_info.id, new Vector2(transform.position.x, transform.position.z), m_build_info.face);
//                        SetBuildState(Build_State.CanMove);

//                }
//                HomeManager.GetInst().SetState(HomeState.None);
//        }

//        public void BackToOldPos()
//        {
//                if (!CanPutDown)
//                {
//                        float x = m_build_info.pos_client.x;
//                        float z = m_build_info.pos_client.y;
//                        int y = HomeManager.GetInst().GetHeightByPos((int)x, (int)z);
//                        transform.position = new Vector3(x, y, z);
//                }
//                CanPutDown = true;
//        }

//        public void EnterFurniture()
//        {
//                Camera.main.transform.localEulerAngles = new Vector3(90, 0, 0);
//                float ratio = (transform.position.y - Camera.main.transform.position.y) / Camera.main.transform.forward.y;  //固定y坐标不变//
//                Vector3 trans = transform.position + new Vector3(offset_size, 0, offset_size) - ratio * Camera.main.transform.forward;                   //计算出固定y坐标下摄像机的x,y坐标。

//                float y = GameUtility.LinearEquationInTwoUnknowns(6, 15, 10, 30, m_build_info.size);

//                float offset = (y - trans.y) / Camera.main.transform.forward.y;
//                Camera.main.transform.position = trans + Camera.main.transform.forward * offset;

//                SetFurnitureEffectActive(true);
//        }


//        public float offset_size = 0;
//        bool is_first_build = true;
//        public void UpdateBuildInfo(long id, BuildInfo new_bi)
//        {
//                if (new_bi.type == EBuildType.eFoundation)
//                {
//                        m_build_info = new_bi;
//                        offset_size = m_build_info.size * 0.5f - 0.5f;
//                        GameUtility.SetLayer(gameObject, "InBuildObj");
//                        return;
//                }

//                bool need_show_menu = false;
//                if (is_first_build)
//                {
//                        m_build_info = new_bi;
//                }
//                else //此处对比分析建筑物发生了什么变化
//                {
//                        if (m_build_info.type == EBuildType.eObstacle)  //障碍
//                        {
//                                if (m_build_info.level < new_bi.level)
//                                {

//                                        if (m_build_info.build_cfg.wall_model != new_bi.build_cfg.model)
//                                        {
//                                                if (gameObject != null)
//                                                {
//                                                        GameUtility.DestroyChild(transform);
//                                                }
//                                                GameObject model = ModelResourceManager.GetInst().GenerateObject(new_bi.build_cfg.model);
//                                                model.transform.SetParent(transform);
//                                                model.transform.localPosition = Vector3.zero;
//                                        }
//                                }
//                        }
//                        else if (m_build_info.type == EBuildType.eFire)
//                        {

//                        }
//                        else
//                        {
//                                if (m_build_info.building_fitment.Contains(",") && !new_bi.building_fitment.Contains(","))
//                                {
//                                        string[] temp = m_build_info.building_fitment.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
//                                        int index = int.Parse(temp[0]);
//                                        int fitment_id = int.Parse(temp[1]);

//                                        if (new_bi.strfitment.Contains(m_build_info.building_fitment)) //家具造完了
//                                        {
//                                                Transform fitment = FurnitureObj.transform.Find("furniture" + index + "_" + fitment_id);
//                                                if (fitment != null)
//                                                {
//                                                        BuildingFurnitureConfig FurnitureConfig = HomeManager.GetInst().GetFurnitureCfg(fitment_id);
//                                                        PlayBuildFinishEffect(fitment, FurnitureConfig.size_x, FurnitureConfig.size_y, Vector3.zero);
//                                                        ChangeFurnitureColor(fitment.gameObject, Color.white);
//                                                }
//                                        }
//                                        else  //取消建造了
//                                        {
//                                                SetFurnitureEffect(index, fitment_id, FurnitureObj, m_build_info.size);

//                                                Transform furniture_tf = FurnitureObj.transform.Find("furniture" + index + "_" + fitment_id);
//                                                if (furniture_tf != null)
//                                                {
//                                                        GameObject.Destroy(furniture_tf.gameObject);
//                                                }

//                                                Dictionary<int, int> pre_dict = GameUtility.ParseCommonStringToDict(new_bi.build_cfg.pre_furniture_formula, ';', ',');
//                                                if (pre_dict.ContainsKey(index))
//                                                {
//                                                        SetFurniture(index, pre_dict[index], FurnitureObj, new_bi.size, false);
//                                                }
//                                        }
//                                        need_show_menu = true;
//                                }

//                                if (!m_build_info.building_fitment.Contains(",") && new_bi.building_fitment.Contains(",")) //建造家具了
//                                {
//                                        string[] temp = new_bi.building_fitment.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
//                                        int index = int.Parse(temp[0]);
//                                        int fitment_id = int.Parse(temp[1]);

//                                        Transform effect_tf = FurnitureObj.transform.Find("effect" + index + "_" + fitment_id);
//                                        if (effect_tf != null)
//                                        {
//                                                GameObject.Destroy(effect_tf.gameObject);
//                                        }

//                                        删除原来位置上的
//                                        for (int i = 0; i < FurnitureObj.transform.childCount; i++)
//                                        {
//                                                if (FurnitureObj.transform.GetChild(i).name.Contains("furniture" + index + "_"))
//                                                {
//                                                        GameObject.Destroy(FurnitureObj.transform.GetChild(i).gameObject);
//                                                }
//                                        }

//                                        SetFurniture(index, fitment_id, FurnitureObj, new_bi.size, true);
//                                        need_show_menu = true;
//                                }

//                                if (m_build_info.work_level < new_bi.work_level)    //功能等级升级, 播放特效，图纸消失
//                                {
//                                        PlayRoomEffect("effect_building_room_finish_001");
//                                        need_show_menu = true;
//                                }

//                                if (m_build_info.level < new_bi.level)    //建筑等级提升， 播放特效，替换墙，生成新的图纸
//                                {
//                                        PlayBuildFinishEffect(transform, m_build_info.size, m_build_info.size, new Vector3(offset_size, 0, offset_size));


//                                        if (m_build_info.build_cfg.wall_model != new_bi.build_cfg.wall_model)
//                                        {
//                                                if (WallObj != null)
//                                                {
//                                                        GameUtility.DestroyChild(WallObj.transform);
//                                                }
//                                                SetBuildLevelUp(new_bi, true);
//                                        }
//                                        else
//                                        {
//                                                SetBuildLevelUp(new_bi, false);
//                                        }
//                                        need_show_menu = true;
//                                }
//                        }
//                        SetFurnitureOccluderOn(); //                  
//                }

//                m_build_info = new_bi;
//                offset_size = m_build_info.size * 0.5f - 0.5f;

//                SetPostionByServer();
//                CheckOutput();
//                CheckCanLevelUp();
//                if (m_build_info.e_state == EBuildState.eWork)
//                {
//                        HideBuildBar();
//                }
//                if (m_build_info.e_state == EBuildState.eBuild)
//                {
//                        if (m_build_info.building_fitment.Contains(","))
//                        {
//                                string[] temp = m_build_info.building_fitment.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
//                                int index = int.Parse(temp[0]);
//                                int fitment_id = int.Parse(temp[1]);
//                                BuildingFurnitureConfig FurnitureConfig = HomeManager.GetInst().GetFurnitureCfg(fitment_id);
//                                max_time = FurnitureConfig.cost_time;
//                        }
//                        else  //服务器错误消息的错误处理
//                        {
//                                HideBuildBar();
//                        }
//                }

//                if (is_first_build)
//                {
//                        is_first_build = false;
//                }

//                if (need_show_menu)
//                {
//                        if (HomeManager.GetInst().GetSelectBuild() == this)
//                        {
//                                ShowBuildMenu();
//                        }
//                }

//                GameUtility.ReScanPath();

//        }

//        float rest_time;
//        int max_time;
//        float buffer_time = 1.0f;

//        float BaseTime = Time.realtimeSinceStartup;
//        void UpdateStateCountdown()
//        {
//                switch (m_build_info.e_state)
//                {
//                        case EBuildState.eClear:
//                                break;
//                        case EBuildState.eWork:
//                                if (Formula_id > 0)
//                                {
//                                        ShowMakeTip();
//                                        return;
//                                }
//                                else
//                                {
//                                        return;
//                                }
//                        case EBuildState.eBuild:
//                                {

//                                }
//                                break;


//                }
//                if (max_time != 0)
//                {
//                        rest_time = max_time - m_build_info.pass_time + m_build_info.client_time - Time.realtimeSinceStartup + buffer_time;     //客户端延时一点//    
//                        SetBuildingState((int)rest_time, max_time);
//                        if (rest_time <= buffer_time)
//                        {
//                                HomeManager.GetInst().SendBuildStateReq(m_build_info.id);
//                                m_build_info.e_state = EBuildState.eWork;
//                        }
//                }
//                else
//                {
//                        HomeManager.GetInst().SendBuildStateReq(m_build_info.id);
//                        m_build_info.e_state = EBuildState.eWork;
//                }
//        }

//        public void PlayBuildFinishEffect(Transform tf, int size_x, int size_y, Vector3 offset) //建筑完成特效
//        {
//                GameObject effect = EffectManager.GetInst().PlayEffect(GlobalParams.GetString("building_finish_effect"), tf);
//                effect.transform.localScale = new Vector3(size_x * 0.5f, 1, size_y * 0.5f);
//                effect.transform.position += new Vector3(0, 0.1f, 0) + offset;
//        }

//        public void PlayMakeFinishEffect() //制作完成特效
//        {
//                GameObject effect = EffectManager.GetInst().PlayEffect(GlobalParams.GetString("make_finish_effect"), transform);
//                effect.transform.position += new Vector3(0, 2.0f, 0);
//        }

//        public void PlayRoomEffect(string url)
//        {
//                Dictionary<int, List<Vector2>> m_point = new Dictionary<int, List<Vector2>>();
//                float max = m_build_info.size;
//                for (int i = 0; i < (int)max * 2 - 1; i++)
//                {
//                        int index = i;
//                        List<Vector2> pointlist = new List<Vector2>();
//                        for (int j = 0; j <= index; j++)
//                        {
//                                pointlist.Add(new Vector2(j, index - j));
//                        }
//                        m_point.Add(index, pointlist);
//                }
//                AppMain.GetInst().StartCoroutine(RoomEffect(m_point, url));
//        }

//        IEnumerator RoomEffect(Dictionary<int, List<Vector2>> dict, string url)
//        {
//                string effect_name = url;
//                GameObject effect;
//                Vector2 pos;
//                foreach (List<Vector2> point in dict.Values)
//                {
//                        for (int i = 0; i < point.Count; i++)
//                        {
//                                if (point[i].x < m_build_info.size && point[i].y < m_build_info.size)
//                                {
//                                        pos = point[i] + m_build_info.pos;
//                                        effect = EffectManager.GetInst().PlayEffect(effect_name, new Vector3(pos.x, HomeManager.GetInst().GetHeightByPos((int)pos.x, (int)pos.y) + 0.2f, pos.y));
//                                }
//                        }
//                        yield return new WaitForSeconds(0.08f);
//                }
//                if (DrawingObj != null)
//                {
//                        GameUtility.DestroyChild(DrawingObj.transform);
//                }
//        }


//        void SetPostionByServer()
//        {
//                int y = HomeManager.GetInst().GetHeightByPos(m_build_info.pos);
//                transform.position = new Vector3(m_build_info.pos_client.x, y, m_build_info.pos_client.y);
//                transform.localEulerAngles = Vector3.zero;
//        }

//        public void SetFurnitureEffectActive(bool isActive)
//        {
//                Transform Furniture = transform.Find("Furniture");
//                if (Furniture != null)
//                {
//                        for (int i = 0; i < Furniture.childCount; i++)
//                        {
//                                Transform tf = Furniture.GetChild(i);
//                                if (tf.name.Contains("effect"))
//                                {
//                                        tf.SetActive(isActive);
//                                        if (isActive)
//                                        {
//                                                GameUtility.SetLayer(tf.gameObject, "InBuildObj");
//                                                EventTriggerListener.Get(tf.gameObject).onClick = OnFurnitureEffectClick;
//                                        }
//                                        else
//                                        {
//                                                EventTriggerListener.Get(tf.gameObject).onClick = null;
//                                        }
//                                }
//                        }
//                }
//        }

//        void OnFurnitureEffectClick(GameObject go, PointerEventData data)
//        {
//                UIManager.GetInst().ShowUI<UI_CreatFurniture>("UI_CreatFurniture").Refresh(data.pointerPress.name);
//        }

//        void SelectAnima()  //选中反馈
//        {
//                transform.DOLocalJump(transform.localPosition, 0.3f, 1, 0.3f);
//        }


//        #region 各种状态

//        UI_BuildState m_Build_State;  //用到的时候再去生成//
//        UI_BuildState Build_State_State
//        {
//                get
//                {
//                        if (m_Build_State == null)
//                        {
//                                GameObject m_State = UIManager.GetInst().ShowUI_Multiple<UI_BuildState>("UI_BuildState");
//                                m_State.transform.SetParent(transform);
//                                m_State.transform.localPosition = new Vector3(offset_size, 3.0f, offset_size);
//                                m_Build_State = m_State.GetComponent<UI_BuildState>();
//                        }
//                        return m_Build_State;
//                }
//        }

//        public void SetBuildingState(int rest_time, int max_time, string url = "")
//        {
//                Build_State_State.ShowBar(rest_time, max_time, url);
//        }

//        void HideBuildBar()
//        {
//                if (m_Build_State != null)
//                {
//                        SetBuildingState(0, 0);
//                }
//        }

//        void CheckCanLevelUp()
//        {
//                if (m_build_info.e_state == EBuildState.eWork)
//                {
//                        BuildingLimitHold bl = HomeManager.GetInst().GetBuildingLimitCfg(m_build_info.build_id);
//                        if (bl == null)
//                        {
//                                return;
//                        }

//                        int max_level = bl.level;
//                        if (m_build_info.level < max_level)
//                        {
//                                BuildingInfoHold m_next_hold = HomeManager.GetInst().GetBuildInfoCfg(m_build_info.build_cfg.id + 1);
//                                if (m_next_hold != null)
//                                {
//                                        if (CommonDataManager.GetInst().CheckIsThingEnough(m_next_hold.level_up_cost))
//                                        {
//                                                Build_State_State.ShowLevelup(true);
//                                                return;
//                                        }
//                                }
//                        }
//                }
//                HideBuildLevelupIcon();
//        }

//        void HideBuildLevelupIcon()
//        {
//                if (m_Build_State != null)
//                {
//                        Build_State_State.ShowLevelup(false);
//                }
//        }



//        void HideBuildInfoTip()
//        {
//                if (m_Build_State != null)
//                {
//                        Build_State_State.ShowInfoTip(false);
//                }
//        }

//        void ShowResTip()
//        {
//                Build_State_State.ShowResTip(true, ResType);
//                isShowResTip = true;
//        }

//        void HideResTip()
//        {
//                if (m_Build_State != null)
//                {
//                        Build_State_State.ShowResTip(false, 0);
//                }
//                isShowResTip = false;
//        }

//        void DestroyBuildState()
//        {
//                if (m_Build_State != null)
//                {
//                        GameObject.Destroy(m_Build_State.gameObject); ;
//                }
//        }

//        public void SetStateIsShow(bool isshow)
//        {
//                if (m_Build_State != null)
//                {
//                        m_Build_State.gameObject.SetActive(isshow);
//                }
//        }

//        #endregion

//        #region 制造类状态

//        int m_formula_id = 0;
//        float m_formula_rest_time = 0;

//        public int Formula_id
//        {
//                set
//                {
//                        m_formula_id = value;
//                }
//                get
//                {
//                        return m_formula_id;
//                }
//        }

//        public float FormulaRestTime
//        {
//                set
//                {
//                        m_formula_rest_time = value;
//                }
//                get
//                {
//                        return m_formula_rest_time;
//                }
//        }

//        public void SetMakeInfo(SCMsgBuildBench msg)
//        {
//                Formula_id = msg.m_idFormula;
//                FormulaRestTime = msg.rest_time + Time.realtimeSinceStartup;
//                ShowMakeTip();
//        }

//        public void ShowMakeTip()
//        {
//                if (Formula_id != 0)
//                {
//                        rest_time = FormulaRestTime - Time.realtimeSinceStartup + buffer_time;
//                        SetBuildingState((int)rest_time, HomeManager.GetInst().GetFormulaCfg(Formula_id).make_time);
//                        int num = 0;
//                        string name = "";
//                        int id = 0;
//                        string des = "";
//                        Thing_Type type;
//                        CommonDataManager.GetInst().SetThingIcon(HomeManager.GetInst().GetFormulaCfg(Formula_id).output, Build_State_State.GetBarIcon(), out name, out num, out id, out des, out type);
//                        if (rest_time <= buffer_time)
//                        {
//                                HomeManager.GetInst().SendBuildStateReq(m_build_info.id);
//                                Formula_id = 0;
//                        }
//                }
//                else
//                {
//                        HideBuildBar();
//                }
//        }

//        #endregion

//        #region 生产类建筑

//        bool NeedOutputRefresh = false;  //是否需要对产出进行计算//
//        float OutputTime;
//        float ShowResTipValue;
//        float OutputPerSecond;
//        public int ResType;
//        public bool isShowResTip = false;

//        public void CheckOutput()
//        {
//                if (GetBuildType() == EBuildType.eProduce && m_build_info.work_level >= 1)
//                {
//                        int id = m_build_info.build_id * 100 + m_build_info.work_level;
//                        BuildProduceHold m_BuildProduceCfg = HomeManager.GetInst().GetBuildingProduceCfg(id);
//                        string[] output_detail = m_BuildProduceCfg.output.Split(',');
//                        ResType = int.Parse(output_detail[1]);
//                        OutputPerSecond = int.Parse(output_detail[2]) / m_BuildProduceCfg.output_time;
//                        ShowResTipValue = m_BuildProduceCfg.reserve * 0.2f;
//                        NeedOutputRefresh = true;
//                }
//        }

//        void UpdateOutput()
//        {
//                if (NeedOutputRefresh)
//                {
//                        float time = HomeManager.GetInst().GetOutPutTime(m_build_info.id);
//                        OutputTime = Time.realtimeSinceStartup - time;
//                        if (OutputTime * OutputPerSecond >= ShowResTipValue)
//                        {
//                                NeedOutputRefresh = false;
//                                ShowResTip();
//                        }
//                }
//        }

//        public void ShowResAdd(int value)
//        {
//                Build_State_State.ShowResAdd(value);
//        }

//        #endregion
//}

