//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using System.Collections;
//using System.Collections.Generic;

//class UI_SmallMap : UIBehaviour
//{
//        //默认最大边框为640*640///
//        public GameObject button;
//        public GameObject big;
//        public GameObject small;
//        public GameObject mask;
//        public GameObject content;
//        public Image no_move_map;


//        int m_size = 10;                          //迷宫大小//
//        readonly int elem_size = 16;              //图素大小,先生成最大的图其他尺寸用image的缩放来实现//
//        readonly float scale_rito_small = 0.8f;   //缩小系数
//        readonly float scale_rito_big = 1.5f;     //放大系数
//        int room_size = 160;

//        enum MapState
//        {
//                Small,   //右上角小地图形态
//                CanMove, //大地图可移动状态
//                NoMove,  //大地图不可移动状态
//        }

//        MapState m_state = MapState.Small;

//        MapState m_big_state = MapState.CanMove;

//        string floor_info = "";
//        void Awake()
//        {
//                use_scaler_anim = false;
//                floor_info = GetText(small, "floor").text;
//        }

//        public void SetSmallMap(int size, int floor)
//        {
//                GetMapInfo(size);
//                ShowSmallMap();
//                SetFloor(floor);
//        }

//        public void ShowMySmallMap()
//        {
//                CreatTextureAndSprite();
//                ShowSmallMap();
//        }

//        void SetFloor(int floor)
//        {
//                GetText(small, "floor").text = string.Format(floor_info, floor);
//        }

//        public bool isBigState()
//        {
//                return m_state > MapState.Small;
//        }

//        void GetMapInfo(int size)
//        {
//                m_size = size;
//                room_size = m_size * elem_size;
//                m_color = new Color32[room_size * room_size];
//                for (int i = 0; i < m_color.Length; i++)
//                {
//                        m_color[i] = new Color32(0, 0, 0, 0);
//                }
//                GetImage(mask, "map").rectTransform.sizeDelta = new Vector2(room_size, room_size);
//                GetImage(mask, "map").transform.localScale = new Vector3(scale_rito_small, scale_rito_small, 1);

//                GetImage(mask, "role").transform.localScale = new Vector3(scale_rito_small, scale_rito_small, 1);
//                GetImage(mask, "role").transform.localPosition = Vector2.zero;

//                no_move_map.rectTransform.sizeDelta = new Vector2(room_size, room_size);
//                no_move_map.transform.localScale = new Vector3(scale_rito_small, scale_rito_small, 1);

//                GetImage(content, "map").rectTransform.sizeDelta = new Vector2(room_size, room_size);
//                GetImage(content, "map").transform.localScale = new Vector3(scale_rito_big, scale_rito_big, 1);
//                GetImage(content, "role").rectTransform.anchoredPosition = big.GetComponent<RectTransform>().sizeDelta * 0.5f;
//                GetImage(content, "role").transform.localScale = new Vector3(scale_rito_big, scale_rito_big, 1);
//        }


//        void ShowSmallMap()
//        {
//                UIManager.GetInst().SetUIActiveState<UI_RaidSkill>("UI_RaidSkill", true);
//                button.SetActive(false);
//                m_state = MapState.Small;
//                small.SetActive(true);
//                big.SetActive(false);
//                if (RaidManager.GetInst().MainHero != null)
//                {
//                        SetPosition((int)RaidManager.GetInst().MainHero.CurrentPos.x, (int)RaidManager.GetInst().MainHero.CurrentPos.y);
//                }


//        }

//        void ShowBigMap()
//        {
//                UIManager.GetInst().SetUIActiveState<UI_RaidSkill>("UI_RaidSkill", false);
//                button.SetActive(true);
//                m_state = m_big_state;
//                small.SetActive(false);
//                big.SetActive(true);

//                if (m_big_state == MapState.CanMove)
//                {
//                        GetImage("jia").gameObject.SetActive(false);
//                        GetImage("jian").gameObject.SetActive(true);
//                        no_move_map.gameObject.SetActive(false);
//                        content.transform.parent.gameObject.SetActive(true);
//                }
//                else
//                {
//                        GetImage("jia").gameObject.SetActive(true);
//                        GetImage("jian").gameObject.SetActive(false);
//                        no_move_map.gameObject.SetActive(true);
//                        content.transform.parent.gameObject.SetActive(false);
//                }

//                if (RaidManager.GetInst().MainHero != null)
//                {
//                        SetPosition((int)RaidManager.GetInst().MainHero.CurrentPos.x, (int)RaidManager.GetInst().MainHero.CurrentPos.y);
//                }


//        }

//        Color32[] m_color;
//        Texture2D m_texture;
//        Sprite m_sp;

//        public void GetTexturePiexls(RaidNodeBehav node)
//        {
//                if (node.IsHide)
//                        return;

//                int x = (int)node.pos.x;
//                int y = (int)node.pos.y;

//                RaidElemConfig elemCfg = node.elemCfg;
//                RaidFloorConfig floorCfg = node.floorCfg;

//                if (floorCfg != null)
//                {
//                        Texture2D floor = (Texture2D)Resources.Load("Texture/SmallMap/" + ModelResourceManager.GetInst().GetSmallMapRes(floorCfg.model));
//                        if (floor != null)
//                        {
//                                CopyPixelToMap(x, y, floor.GetPixels32());
//                        }
//                }


//                if (elemCfg != null)
//                {
//                        Texture2D elem = (Texture2D)Resources.Load("Texture/SmallMap/" + ModelResourceManager.GetInst().GetSmallMapRes(elemCfg.model));

//                        if (elem != null && elemCfg.type != (int)RAID_ELEMENT_TYPE.HIDE_TRAP) //隐藏元素不画//
//                        {
//                                Color32[] elem_color = elem.GetPixels32();
//                                if (elemCfg.type == (int)RAID_ELEMENT_TYPE.WALL) //墙壁，描绘黑线//检查上下左右//
//                                {
//                                        for (int i = 0; i < 4; i++)
//                                        {
//                                                elem_color = PaintBlackLine(elem_color, i, NeedBlackLine(i, x, y));
//                                        }
//                                }
//                                CopyPixelToMap(x, y, elem_color);
//                        }
//                }
//        }


//        public void ChangeMapPixels(RaidNodeBehav node) //整合的时候在添加//
//        {
//                int x = (int)node.pos.x;
//                int y = (int)node.pos.y;

//                //图标消失
//                if (node.elemCount <= 0)
//                {
//                        if (node.elemCfg == null || node.elemCfg.is_result_disappear == 1)
//                        {
//                                //暂时都处理为消失
//                                Texture2D floor = (Texture2D)Resources.Load("Texture/SmallMap/" + ModelResourceManager.GetInst().GetSmallMapRes(node.floorCfg.model));
//                                if (floor != null)
//                                {
//                                        CopyPixelToMap(x, y, floor.GetPixels32());
//                                }
//                        }
//                        else
//                        {
//                                Texture2D elem = (Texture2D)Resources.Load("Texture/SmallMap/" + ModelResourceManager.GetInst().GetSmallMapRes(node.elemCfg.model));
//                                if (elem != null)
//                                {
//                                        Color32[] elem_color = elem.GetPixels32();
//                                        for (int i = 0; i < elem_color.Length; i++)
//                                        {
//                                                if (elem_color[i].r < 50)
//                                                {
//                                                        continue;
//                                                }
//                                                byte m_color = (byte)((0.7 * elem_color[i].r) + (0.2 * elem_color[i].g) + (0.1 * elem_color[i].b)); //变灰
//                                                elem_color[i] = new Color32(elem_color[i].r, m_color, m_color, m_color);
//                                        }
//                                        CopyPixelToMap(x, y, elem_color);
//                                }
//                        }

//                }
//                CreatTextureAndSprite();
//        }

//        bool NeedBlackLine(int type, int x, int y)
//        {
//                int node = 0;
//                if (type == 0)
//                {
//                        node = x * 100 + (y + 1);
//                }
//                if (type == 1)
//                {
//                        node = x * 100 + (y - 1);
//                }
//                if (type == 2)
//                {
//                        node = (x - 1) * 100 + y;
//                }
//                if (type == 3)
//                {
//                        node = (x + 1) * 100 + y;
//                }
//                RaidNodeBehav temp = null;
//                temp = RaidManager.GetInst().GetRaidNodeBehav(node);
//                if (temp != null)
//                {
//                        if (temp.elemCfg != null)
//                        {
//                                if (temp.elemCfg.type == (int)RAID_ELEMENT_TYPE.WALL)
//                                {
//                                        return false;
//                                }

//                        }

//                }
//                return true;
//        }

//        Color32[] PaintBlackLine(Color32[] ori, int type, bool paint) //0123,上下左右//
//        {
//                if (!paint)
//                {
//                        return ori;
//                }
//                for (int i = 0; i < ori.Length; i++)
//                {
//                        if (type == 0)
//                        {
//                                if (i >= ori.Length - elem_size)
//                                {
//                                        ori[i] = Color.black;
//                                }
//                        }
//                        if (type == 1)
//                        {
//                                if (i <= elem_size - 1)
//                                {
//                                        ori[i] = Color.black;
//                                }
//                        }
//                        if (type == 2)
//                        {
//                                if (i % 16 == 0)
//                                {
//                                        ori[i] = Color.black;
//                                }
//                        }
//                        if (type == 3)
//                        {
//                                if ((i + 1) % 16 == 0)
//                                {
//                                        ori[i] = Color.black;
//                                }
//                        }
//                }
//                return ori;
//        }


//        void CopyPixelToMap(int x, int y, Color32[] source)  //把小图的像素填入地图
//        {
//                if (x >= m_size || y >= m_size)
//                {
//                        singleton.GetInst().ShowMessage(ErrorOwner.designer, "超出迷宫大小！！！" + "   x=" + x + ",y=" + y + ",size=" + m_size);
//                        return;
//                }
//                int count = 0;
//                for (int i = 0; i < elem_size; i++)
//                {
//                        int start = i * room_size + y * elem_size * elem_size * m_size + x * elem_size;
//                        for (int j = start; j < start + elem_size; j++)
//                        {
//                                if (source[count].a > 50)  //只替换半透明以上像素,防止路面不显示
//                                {
//                                        m_color[j] = source[count];

//                                }
//                                count++;
//                        }
//                }
//        }


//        public void SetPosition(int x, int y)  //输入人物的坐标// 地图原点和锚点都是左下角
//        {
//                if (m_state == MapState.Small)
//                {
//                        Vector2 role_positon = new Vector2(elem_size * x + elem_size * 0.5f, elem_size * y + elem_size * 0.5f) * scale_rito_small;
//                        Vector2 center = mask.GetComponent<RectTransform>().sizeDelta * 0.5f;
//                        GetImage(mask, "map").rectTransform.anchoredPosition = center - role_positon;
//                }
//                else if (m_state == MapState.NoMove)
//                {
//                        GetImage(no_move_map.gameObject, "role").rectTransform.anchoredPosition = new Vector2(x, y) * elem_size;
//                }
//                else
//                {
//                        content.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
//                        Vector2 role_positon = new Vector2(elem_size * x + elem_size * 0.5f, elem_size * y + elem_size * 0.5f) * scale_rito_big;
//                        Vector2 center = big.GetComponent<RectTransform>().sizeDelta * 0.5f;
//                        GetImage(content, "map").rectTransform.anchoredPosition = center - role_positon;
//                }
//                //SetMask(FOWSystem.instance.texture1);

//        }

//        public void ClickSmallMap()
//        {
//                ShowBigMap();
//        }



//        public void CreatTextureAndSprite()
//        {
//                if (m_texture != null)
//                {
//                        Destroy(m_texture);
//                }
//                if (m_sp != null)
//                {
//                        Destroy(m_sp);
//                }
//                m_texture = new Texture2D(room_size, room_size, TextureFormat.ARGB32, false);  //生成地图
//                m_texture.filterMode = FilterMode.Point;
//                m_texture.wrapMode = TextureWrapMode.Clamp;
//                m_texture.SetPixels32(m_color);
//                m_texture.Apply(false);
//                m_sp = Sprite.Create(m_texture, new Rect(0, 0, room_size, room_size), new Vector2(0.5f, 0.5f));
//                m_sp.name = "SmallMap";
//                GetImage(mask, "map").sprite = m_sp;
//                no_move_map.sprite = m_sp;
//                GetImage(content, "map").sprite = m_sp;
//                //SetMask(FOWSystem.instance.texture1);
//        }

//        public void OtherButton()
//        {
//                ShowSmallMap();
//        }


//        void SetMask(Texture2D tex)
//        {
//                if (m_state == MapState.Small)
//                {
//                        GetImage(mask, "map").material.SetTexture("_MaskTex", tex);
//                }
//                else if (m_state == MapState.CanMove)
//                {
//                        GetImage(content, "map").material.SetTexture("_MaskTex", tex);
//                }
//                else
//                {
//                        no_move_map.material.SetTexture("_MaskTex", tex);
//                }
//        }

//        void Update()
//        {
//                UpdateDarg();
//        }

//        void UpdateDarg()
//        {
//                if (m_state == MapState.CanMove)
//                {
//                        float x = content.GetComponent<RectTransform>().anchoredPosition.x;
//                        float y = content.GetComponent<RectTransform>().anchoredPosition.y;
//                        float max_x = big.GetComponent<RectTransform>().sizeDelta.x - GetImage(content, "map").rectTransform.anchoredPosition.x - GetImage(content, "map").rectTransform.sizeDelta.x * 0.5f * scale_rito_big;
//                        float min_x = -1 * (GetImage(content, "map").rectTransform.anchoredPosition.x + GetImage(content, "map").rectTransform.sizeDelta.x * 0.5f * scale_rito_big);
//                        float max_y = big.GetComponent<RectTransform>().sizeDelta.y - GetImage(content, "map").rectTransform.anchoredPosition.y - GetImage(content, "map").rectTransform.sizeDelta.y * 0.5f * scale_rito_big;
//                        float min_y = -1 * (GetImage(content, "map").rectTransform.anchoredPosition.y + GetImage(content, "map").rectTransform.sizeDelta.y * 0.5f * scale_rito_big);
//                        if (x > max_x)
//                        {
//                                content.GetComponent<RectTransform>().anchoredPosition = new Vector2(max_x, y);
//                        }
//                        if (x < min_x)
//                        {
//                                content.GetComponent<RectTransform>().anchoredPosition = new Vector2(min_x, y);
//                        }
//                        if (y > max_y)
//                        {
//                                content.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, max_y);
//                        }
//                        if (y < min_y)
//                        {
//                                content.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, min_y);
//                        }
//                }
//        }


//        public void ChangeBigState()
//        {
//                if (m_big_state == MapState.CanMove)
//                {
//                        m_big_state = MapState.NoMove;
//                }
//                else
//                {
//                        m_big_state = MapState.CanMove;
//                }
//                ShowBigMap();
//        }
//}