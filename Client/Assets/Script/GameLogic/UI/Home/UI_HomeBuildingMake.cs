using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

public class UI_HomeBuildingMake : UI_ScrollRectHelp
{
        GameObject two;
        GameObject three;
        GameObject material;
        public GameObject content;
        public GameObject right;

        void Awake()
        {
                IsFullScreen = true;
                two = GetGameObject(content, "2");
                three = GetGameObject(content, "3");
                material = GetGameObject(right, "material0");
        }

        public void OnClickClose()
        {
                UIManager.GetInst().CloseUI(this.name);
                //UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", true);
        }

        List<GameObject> TreeItem = new List<GameObject>();  //按顺序存放//
        public void Refresh()
        {
                //UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", false);
                RefreshLeft();
                GetGameObject(right, "content").SetActive(false);
        }

        int cutoff;
        void RefreshLeft()
        {
                SortedList<int, List<int>> m_Formulalist = HomeManager.GetInst().GetMyFormulaList();
                SortedList<int, List<int>> m_Technologlist = HomeManager.GetInst().GetShowTechnologyList();

                cutoff = PlayerController.GetInst().GetPropertyInt("bench_lower_consume_per");

                for (int j = 0; j < m_Formulalist.Count; j++)
                {
                        GameObject m_two = GetGameObject(content, "2_" + j);
                        if (m_two == null)
                        {
                                m_two = CloneElement(two, "2_" + j);
                        }
                        GetText(m_two, "Text").text = HomeManager.GetInst().GetFormulaClassCfg(m_Formulalist.Keys[j]).name;
                        EventTriggerListener listener_two = EventTriggerListener.Get(m_two);
                        listener_two.onClick = OnClickTreeItem;
                        listener_two.onDrag = OnDarg;
                        listener_two.onBeginDrag = OnBeginDrag;
                        listener_two.onEndDrag = OnEndDrag;
                        listener_two.SetTag(false);
                        m_two.SetActive(true);
                        TreeItem.Add(m_two);
                        for (int k = 0; k < m_Formulalist.Values[j].Count; k++)
                        {
                                GameObject m_three = GetGameObject(three, "3_" + k);
                                if (m_three == null)
                                {
                                        m_three = CloneElement(three, "3_" + k);
                                }
                                int formula_id = m_Formulalist.Values[j][k];
                                GetText(m_three, "Text").text = LanguageManager.GetText(HomeManager.GetInst().GetFormulaCfg(formula_id).name);
                                EventTriggerListener listener_three = EventTriggerListener.Get(m_three);
                                listener_three.onClick = OnClickFormula;
                                listener_three.onDrag = OnDarg;
                                listener_three.onBeginDrag = OnBeginDrag;
                                listener_three.onEndDrag = OnEndDrag;
                                listener_three.SetTag(formula_id);
                                m_three.SetActive(false);
                                TreeItem.Add(m_three);
                        }
                }

                int formulalistCount = m_Formulalist.Count;
                for (int j = formulalistCount; j < m_Technologlist.Count + formulalistCount; j++)
                {
                        GameObject m_two = GetGameObject(content, "2_" + j);
                        if (m_two == null)
                        {
                                m_two = CloneElement(two, "2_" + j);
                        }
                        GetText(m_two, "Text").text = HomeManager.GetInst().GetFormulaClassCfg(m_Technologlist.Keys[j - formulalistCount]).name;
                        EventTriggerListener listener_two = EventTriggerListener.Get(m_two);
                        listener_two.onClick = OnClickTreeItem;
                        listener_two.onDrag = OnDarg;
                        listener_two.onBeginDrag = OnBeginDrag;
                        listener_two.onEndDrag = OnEndDrag;
                        listener_two.SetTag(false);
                        m_two.SetActive(true);
                        TreeItem.Add(m_two);
                        for (int k = 0; k < m_Technologlist.Values[j - formulalistCount].Count; k++)
                        {
                                GameObject m_three = GetGameObject(three, "3_" + k);
                                if (m_three == null)
                                {
                                        m_three = CloneElement(three, "3_" + k);
                                }
                                int technolog_id = m_Technologlist.Values[j - formulalistCount][k];
                                GetText(m_three, "Text").text = LanguageManager.GetText(HomeManager.GetInst().GetTechnologyCfg(technolog_id).name);
                                EventTriggerListener listener_three = EventTriggerListener.Get(m_three);
                                listener_three.onClick = OnClickTechnolog;
                                listener_three.onDrag = OnDarg;
                                listener_three.onBeginDrag = OnBeginDrag;
                                listener_three.onEndDrag = OnEndDrag;
                                listener_three.SetTag(technolog_id);
                                m_three.SetActive(false);
                                TreeItem.Add(m_three);
                        }
                }

                two.SetActive(false);
                three.SetActive(false);
                RefreshTreePositon();  
        }


        void RefreshTreePositon()
        {
                float y = 0;
                for (int i = 0; i < TreeItem.Count; i++)
                {
                        GameObject obj = TreeItem[i];
                        if (obj.activeSelf)
                        {
                                RectTransform rt = obj.GetComponent<RectTransform>();
                                obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(rt.anchoredPosition.x, y);
                                y -= rt.rect.height;
                        }
                }
                StartCoroutine(WaitForsizeDelta(y * (-1)));
        }

        IEnumerator WaitForsizeDelta(float height)
        {
                yield return new WaitForEndOfFrame();

                RectTransform rt = content.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, height + 10.0f);
                rt.anchoredPosition = Vector2.zero;
        }

        void OnClickTreeItem(GameObject go, PointerEventData data) //点击非最后一级的菜单菜单
        {
                bool tag = (bool)EventTriggerListener.Get(go).GetTag();
                tag = !tag;
                SetTreeItemBtnState(go, tag);

                string name = go.name;
                int sign = int.Parse(name.Split('_')[0]);
                int index = TreeItem.IndexOf(go);
                for (int i = index + 1; i < TreeItem.Count; i++)
                {
                        int flag = int.Parse(TreeItem[i].name.Split('_')[0]);
                        if (flag - sign == 1) //下一级菜单
                        {
                                TreeItem[i].SetActive(!TreeItem[i].activeSelf);
                                CloseTreeItem(TreeItem[i]);
                        }

                        if (flag <= sign)
                        {
                                break;
                        }

                }
                RefreshTreePositon();
        }

        void SetTreeItemBtnState(GameObject item, bool is_off) //设置菜单打开关闭的样式
        {
                GameObject line = GetGameObject(item, "line");
                if (line == null)
                {
                        return;
                }
                if (is_off)
                {
                        line.SetActive(true);
                        Quaternion qua = new Quaternion();
                        qua.eulerAngles = Vector3.zero;
                        GetImage(item, "Image").rectTransform.rotation = qua;
                }
                else
                {
                        line.SetActive(false);
                        Quaternion qua = new Quaternion();
                        qua.eulerAngles = new Vector3(0, 0, 90f);
                        GetImage(item, "Image").rectTransform.rotation = qua;

                }
                EventTriggerListener.Get(item).SetTag(is_off);
        }

        void CloseTreeItem(GameObject item)  //关闭该节点下的菜单
        {
                SetTreeItemBtnState(item, false);

                string name = item.name;
                int sign = int.Parse(name.Split('_')[0]);
                int index = TreeItem.IndexOf(item);
                for (int i = index + 1; i < TreeItem.Count; i++)
                {
                        int flag = int.Parse(TreeItem[i].name.Split('_')[0]);
                        if (flag - sign == 1) //下一级菜单
                        {
                                TreeItem[i].SetActive(false);
                        }
                        if (flag <= sign)
                        {
                                break;
                        }
                }
        }


        #region 为了让科技和配方在一个界面显示 ，这里是配方

        public void RefreshFormulaRight()  
        {
                right.transform.Find("content/finish").SetActive(false);

                can_make_max = 10;
                formulahold = HomeManager.GetInst().GetFormulaCfg(FormulaId);
                if (formulahold == null)
                {
                        return;
                }
                GetGameObject(right, "content").SetActive(true);
                SetGameObjectHide("material", right);
                string require = formulahold.require;
                string[] detail = require.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                int num = 0;
                string name = "";
                int id = 0;
                string des = "";
                Thing_Type type;
                for (int i = 0; i < detail.Length; i++)
                {
                        GameObject m_material = GetGameObject(right, "material" + i);
                        if (m_material == null)
                        {
                                m_material = CloneElement(material, "material" + i);
                        }
                        RectTransform rt = m_material.GetComponent<RectTransform>();
                        RectTransform ori_rt = material.GetComponent<RectTransform>();
                        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, ori_rt.anchoredPosition.y - i * (ori_rt.rect.height + 10.0f));
                        m_material.SetActive(true);
                        Transform tf = GetImage(m_material, "icon").transform;
                        CommonDataManager.GetInst().SetThingIcon(detail[i], tf, null, out name, out num, out id, out des, out type);
                        GetText(m_material, "name").text = name;
                        int has_num = CommonDataManager.GetInst().GetThingNum(detail[i], out num);

                        num = Mathf.CeilToInt(num * (1 - cutoff * 0.01f));

                        int max = has_num / num;
                        if (max < can_make_max)
                        {
                                can_make_max = max;
                        }

                        if (max < 1)
                        {
                                GetText(m_material, "has_num").text = "<color=red>" + has_num.ToString() + "</color>";
                        }
                        else
                        {
                                GetText(m_material, "has_num").text = has_num.ToString();
                        }
                        GetText(m_material, "need_num").text = (num * make_num).ToString();
                }
                RefreshFormulaBottom();
        }

        void RefreshFormulaBottom()
        {
                right.transform.Find("content/cost").SetActive(true);
                int num = 0;
                string name = "";
                int id = 0;
                string des = "";
                Thing_Type type;
                Transform tf = GetImage(right, "out_icon").transform;
                CommonDataManager.GetInst().SetThingIcon(formulahold.output, tf, null, out name, out num, out id, out des, out type);
                GetText(right, "out_name").text = name;
                GetText(right, "des").text = des;
                GetText(right, "cost_day").text = UIUtility.GetTimeString3(formulahold.make_time);
                GetText(right, "make_num").text = make_num.ToString();
                GetGameObject(right, "make").SetActive(true);
                EventTriggerListener.Get(GetGameObject(right, "make")).onClick = OnMakeFurmula;
        }

        int FormulaId = 0;
        BuildingMakeFormulaHold formulahold;
        void OnClickFormula(GameObject go, PointerEventData data)//点击最子集菜单
        {
                int id = (int)EventTriggerListener.Get(go).GetTag();
                if (id == FormulaId)
                {
                        return;
                }
                FormulaId = id;
                RefreshFormulaRight();
        }

        int make_num = 1;
        int can_make_max = 10;

        void OnMakeFurmula(GameObject go, PointerEventData data)
        {
                if (can_make_max <= 0)
                {
                        GameUtility.PopupMessage("制作材料不足！");
                }
                else
                {
                        //制作//
                        HomeManager.GetInst().SendMakeFormula(FormulaId);
                }
        }

        #endregion

        #region 为了让科技和配方在一个界面显示 ，这里是科技

        int TechnologId = 0;
        BuildingTechnologyConfig Technologhold;

        public void RefreshTechnologRight()
        {
                can_make_max = 10;
                Technologhold = HomeManager.GetInst().GetTechnologyCfg(TechnologId);
                if (Technologhold == null)
                {
                        return;
                }
                GetGameObject(right, "content").SetActive(true);
                SetGameObjectHide("material", right);
                string require = Technologhold.cost;
                string[] detail = require.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                int num = 0;
                string name = "";
                int id = 0;
                string des = "";
                Thing_Type type;
                for (int i = 0; i < detail.Length; i++)
                {
                        GameObject m_material = GetGameObject(right, "material" + i);
                        if (m_material == null)
                        {
                                m_material = CloneElement(material, "material" + i);
                        }
                        RectTransform rt = m_material.GetComponent<RectTransform>();
                        RectTransform ori_rt = material.GetComponent<RectTransform>();
                        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, ori_rt.anchoredPosition.y - i * (ori_rt.rect.height + 10.0f));
                        m_material.SetActive(true);
                        Transform tf = GetImage(m_material, "icon").transform;
                        CommonDataManager.GetInst().SetThingIcon(detail[i], tf, null, out name, out num, out id, out des, out type);
                        GetText(m_material, "name").text = name;
                        int has_num = CommonDataManager.GetInst().GetThingNum(detail[i], out num);

                        num = Mathf.CeilToInt(num * (1 - cutoff * 0.01f));

                        int max = has_num / num;
                        if (max < can_make_max)
                        {
                                can_make_max = max;
                        }

                        if (max < 1)
                        {
                                GetText(m_material, "has_num").text = "<color=red>" + has_num.ToString() + "</color>";
                        }
                        else
                        {
                                GetText(m_material, "has_num").text = has_num.ToString();
                        }
                        GetText(m_material, "need_num").text = (num * make_num).ToString();
                }

                int series = TechnologId / 100;
                int level = TechnologId % 100;
                if (HomeManager.GetInst().GetTechnologyLevel(series) == level)
                {
                        right.transform.Find("content/finish").SetActive(true);
                        GetGameObject(right, "make").SetActive(false);
                }
                else
                {
                        right.transform.Find("content/finish").SetActive(false);
                        GetGameObject(right, "make").SetActive(true);
                }
                RefreshTechnologBottom();
        }

        void RefreshTechnologBottom()
        {
                right.transform.Find("content/cost").SetActive(false);
                ResourceManager.GetInst().LoadIconSpriteSyn(Technologhold.icon, GetImage(right, "out_icon"));
                GetText(right, "out_name").text = LanguageManager.GetText(Technologhold.name);
                GetText(right, "des").text = LanguageManager.GetText(Technologhold.describe);

                EventTriggerListener.Get(GetGameObject(right, "make")).onClick = OnMakeTechnolog;
        }

        void OnClickTechnolog(GameObject go, PointerEventData data)//点击最子集菜单
        {
                int id = (int)EventTriggerListener.Get(go).GetTag();
                if (id == TechnologId)
                {
                        return;
                }
                TechnologId = id;
                RefreshTechnologRight();
        }


        void OnMakeTechnolog(GameObject go, PointerEventData data)
        {
                if (can_make_max <= 0)
                {
                        GameUtility.PopupMessage("制作材料不足！");
                }
                else
                {
                        //制作//
                        HomeManager.GetInst().SendMakeTechnology(TechnologId);
                }
        }


        #endregion
}

