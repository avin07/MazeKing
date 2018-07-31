using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;


public class UI_BuildingFunction : UIBehaviour
{
        Image m_Bg;
        GameObject btn;
        Text m_Name;
        BuildBaseBehaviour m_buildbehaviour
        {
                get
                {
                        return HomeManager.GetInst().GetSelectBuild();
                }
        }
        Canvas m_canvas;
        void Awake()
        {
                UILevel = UI_LEVEL.MAIN;
                m_Bg = GetImage("bg");
                btn = GetGameObject(m_Bg.gameObject, "btn0");
                btn.SetActive(false);
                m_Name = FindComponent<Text>("bg/name");
                m_canvas = transform.GetComponent<Canvas>();
        }

        Vector2 pos;
        public void SetupPosition(BuildBaseBehaviour bb)
        {
                Vector3 offset = Vector3.zero;
                if(Camera.main != null)
                {
                        if (m_canvas != null)
                        {
                                RectTransformUtility.ScreenPointToLocalPointInRectangle(m_canvas.transform as RectTransform, Camera.main.WorldToScreenPoint(bb.transform.position + offset), m_canvas.worldCamera, out pos);
                                m_Bg.rectTransform.anchoredPosition = pos - Vector2.up * 60;
                        }
                }
        }

        float btn_offset = 20;
        float y_offset = 15;

        public void RefreshMenu(string info) 
        {
                SetChildActive(m_Bg.transform, false);
                float size_x = (btn.transform as RectTransform).sizeDelta.x;
                string[] menu = info.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                int count = menu.Length;
                float width = count * size_x + (count - 1) * btn_offset;

                for (int i = 0; i < count; i++)
                {
                        GameObject temp = GetGameObject("btn" + i);
                        if (temp == null)
                        {
                                temp = CloneElement(btn, "btn" + i);
                        }
                        int id = int.Parse(menu[i]);
                        EventTriggerListener.Get(temp).SetTag(id);
                        EventTriggerListener.Get(temp).onClick = OnMenuClick;
                        temp.SetActive(true);

                        float y = 0;
                        int mid_index = count / 2;
                        if (count % 2 != 0)  //奇数
                        {
                                y = Math.Abs(i - mid_index) * y_offset;
                        }
                        else
                        {
                                if (i < mid_index - 1)
                                {
                                        y = Math.Abs(i - mid_index + 1) * y_offset;
                                }

                                if (i > mid_index)
                                {
                                        y = Math.Abs(i - mid_index) * y_offset;
                                }
 
                        }
                        temp.GetComponent<RectTransform>().anchoredPosition = new Vector2(-width * 0.5f + 0.5f * size_x + i * (size_x + btn_offset), y);
                        ResourceManager.GetInst().LoadIconSpriteSyn(HomeManager.GetInst().GetBuildFunctionIcon(id), temp.transform);
                }

                if (m_buildbehaviour.mBuildInfo.buildCfg != null)
                {
                        ShowBuildName();
                }
                SetupPosition(m_buildbehaviour);
        }

        void Update()
        {
                if (m_buildbehaviour != null)
                {
                        SetupPosition(m_buildbehaviour);
                }
        }

        public void ShowBuildName()
        {
                m_Name.gameObject.SetActive(true);
                m_Name.text = LanguageManager.GetText(m_buildbehaviour.mBuildInfo.buildCfg.name);             
        }

        public void OnMenuClick(GameObject go, PointerEventData eventData)
        {
                Build_Function m_type = (Build_Function)EventTriggerListener.Get(go).GetTag();
                switch (m_type)
                {
                        case Build_Function.Move:  //改为快捷界面
                                break;
                        case Build_Function.LevelUp:
                                MenuLevelUp();
                                break;
                        case Build_Function.Info:
                                MenuInfo();
                                break;
                        case Build_Function.Creat:
                                break;
                        case Build_Function.Clean:
                                MenuClean();
                                break;
                        case Build_Function.Spin:
                                MenuSpin();
                                break;
                        case Build_Function.Cancel:
                                MenuCancel();
                                break;
                        case Build_Function.Quick:
                                MenuQuick();
                                break;
                        case Build_Function.Make:
                                MenuMake();                               
                                break;
                        case Build_Function.Gain:
                                MenuGain();                               
                                return;
                        case Build_Function.Darw:
                                MenuDraw();
                                break;
                        case Build_Function.CurePressure:
                                MenuPressure();                                
                                break;
                        case Build_Function.CheckIn:
                                MenuCheckIn();
                                break;
                        case Build_Function.Refresh:
                                MenuHotelRefresh();
                                break;
                        case Build_Function.SkillUpgrade:
                                MenuSkillUpgrade();
                                break;
                        case Build_Function.TaskBoard:
                                MenuTaskBoard();
                                break;
                        case Build_Function.CureSpecificityCure:
                                MenuSpecificityCure();
                                break;

                        default:
                                break;
                }
        }

        void MenuLevelUp()//升级
        {
                BuildingLimitHold bl = HomeManager.GetInst().GetBuildingLimitCfg(m_buildbehaviour.mBuildInfo.buildId);
                if (bl == null)
                {
                        GameUtility.PopupMessage("已达当前可建造最大等级！");
                        return;
                }

                int max_level = bl.level;
                if (m_buildbehaviour.mBuildInfo.level < max_level)
                {
                        BuildingInfoHold m_next_hold = HomeManager.GetInst().GetBuildInfoCfg(m_buildbehaviour.mBuildInfo.buildCfg.id + 1);
                        string need = "";
                        if (m_next_hold != null)
                        {
                                need = m_next_hold.cost_material;
                        }
                        else
                        {
                                singleton.GetInst().ShowMessage(ErrorOwner.designer, "建筑" + m_buildbehaviour.mBuildInfo.buildId + "少了" + (m_buildbehaviour.mBuildInfo.level + 1) + "等级的配置");
                        }
                        UIManager.GetInst().ShowUI<UI_NeedConfirm>("UI_NeedConfirm").SetConfirmAndCancel("升级", need, string.Empty, ConfirmLevelUp, null, null);
                }
                else
                {
                        if (m_buildbehaviour.GetBuildType() == EBuildType.eMain)
                        {
                                GameUtility.PopupMessage(LanguageManager.GetText("client_notice_message_2"));
                        }
                        else
                        {
                                GameUtility.PopupMessage(LanguageManager.GetText("client_notice_message_1"));
                        }
                }
        }

        void ConfirmLevelUp(object data)
        {
                HomeManager.GetInst().SendUpgradeBuild();
        }

        void MenuMake()//制作
        {
                UIManager.GetInst().ShowUI<UI_HomeBuildingMake>("UI_HomeBuildingMake").Refresh(); 
        }

        void MenuGain()//获得
        {
                HomeManager.GetInst().SendResourceGain(m_buildbehaviour as BuildProduceBehaviour);
        }

        void MenuDraw()//查看图纸
        {

        }

        void MenuInfo()//信息
        {
                m_buildbehaviour.Info();
        }

        void MenuClean()//清理
        {
                m_buildbehaviour.Clean();
        }

        void ConfirmClean(object data)
        {
                HomeManager.GetInst().SendCleanBuild();
        }

        void MenuSpin()//旋转
        {
                HomeManager.GetInst().RotateBuild();
        }

        void MenuCancel()//取消
        {
                m_buildbehaviour.Cancel();
        }

        void MenuQuick()
        {
                m_buildbehaviour.Quick();
        }

        void MenuPressure()
        {
//                UIManager.GetInst().ShowUI<UI_PressureCure>("UI_PressureCure").Refresh();
        }

        void MenuCheckIn()
        {
                UIManager.GetInst().ShowUI<UI_CheckIn>("UI_CheckIn").Refresh();
        }
        void MenuSkillUpgrade()
        {
                Debug.Log("MenuSkillUpgrade");
                UIManager.GetInst().ShowUI<UI_SkillUpgrade>("UI_SkillUpgrade").Refresh(m_buildbehaviour.mBuildInfo);
        }

        void MenuHotelRefresh()
        {
                int cost = HomeManager.GetInst().GetHotelRefreshCost();
                if (CommonDataManager.GetInst().GetNowResourceNum("diamond") >= cost)
                {
                        UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox").SetConfirmAndCancel("刷新访客", "花费" + cost + "钻石立即刷新访客", ConfirmHotelRefresh, null, null);
                }
                else
                {
                        GameUtility.PopupMessage("需要花费" + cost + "钻石进行刷新，充值才能变得更强！");
                }
        }

        void ConfirmHotelRefresh(object data)
        {
                HomeManager.GetInst().SendHotelRefresh(1);
        }

        void MenuTaskBoard()
        {
                UIManager.GetInst().ShowUI<UI_TaskBoard>("UI_TaskBoard").Refresh();
        }

        void MenuSpecificityCure()
        {
                UIManager.GetInst().ShowUI<UI_SpecificityCure>("UI_SpecificityCure").Refresh();
        }

}

enum Build_Function
{
        Move = 1,                       //移动(去掉菜单变为快捷功能)
        LevelUp = 2,                    //升级
        Make = 3,                       //制作
        Info = 4,                       //信息
        Gain = 5,                       //收获
        Darw = 6,                       //图纸
        Creat = 7,                      //建造
        Clean = 8,                      //清理
        Spin = 9,                       //旋转
        Cancel = 10,                    //取消
        Quick = 11,                     //快速建造
        DiyMaze = 12,                   //自建迷宫
        CurePressure = 13,              //压力治疗
        CheckIn = 14,                   //入住
        Refresh = 15,                   //广播台钻石刷新
        SkillUpgrade = 16,              //技能升级
        TaskBoard = 17,                 //技能升级
        CureSpecificityCure = 18,       //治疗怪癖
}


