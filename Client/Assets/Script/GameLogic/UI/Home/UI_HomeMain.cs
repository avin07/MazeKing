using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using DG.Tweening;
using System.Text;


public class UI_HomeMain : UIBehaviour
{       
        public GameObject BTNGroup;

        RectTransform LongPressIcon;
        Transform statebtn;
        Canvas canvas;
        void Awake()
        {
                this.UILevel = UI_LEVEL.MAIN;
                ResetBuildingState();
                FindComponent<Button>("BTN/editorbtn").onClick.AddListener(OnClickEditor);
                FindComponent<Button>("BTN/taskbtn").onClick.AddListener(OnClickTask);
                FindComponent<Button>("BTN/drawbtn").onClick.AddListener(OnClickFurniture);
                FindComponent<Button>("left/Image/testbtn").onClick.AddListener(OnNewDay);
                FindComponent<Button>("BTN/suitbtn").onClick.AddListener(OnSuit);

                statebtn = transform.Find("BTN/statebtn");
                statebtn.SetActive(false);
                EventTriggerListener.Get(statebtn).onClick = OnEditorStateChange;

                LongPressIcon = transform.Find("mouse") as RectTransform;
                canvas = GetComponent<Canvas>();

                btn = FindComponent<Button>("content/btn");
                btn.gameObject.SetActive(false);

#if UNITY_STANDALONE || UNITY_EDITOR  //测试模型的室外桃园
                FindComponent<Button>("BTN/testbtn").gameObject.SetActive(true);
                FindComponent<Button>("BTN/testbtn").onClick.AddListener(OnTestWorld);
#endif
        }

        public void ShowLongPressIcon(bool bShow)
        {
                UIUtility.SetUIEffect(this.name, LongPressIcon.gameObject, bShow, "effect_set_brick_2");
        }

        public void SetLongPressIconPosition()
        {
                Vector3 globalMousePos;
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, CameraManager.GetInst().UI_Camera, out globalMousePos))
                {
                        LongPressIcon.position = globalMousePos;
                }
        }

        void OnClickEditor()
        {
                HomeManager.GetInst().StopCameraYAutoTweener();
                transform.Find("BTN/statebtn").SetActive(HomeManager.GetInst().ChangeState());
        }

        void OnTestWorld()
        {
                HomeManager.GetInst().EnterTestWorld();
        }

        void OnNewDay()
        {
                GameUtility.SendGM("/debug,onNewDay");
        }

        void OnSuit()
        {
                UIManager.GetInst().ShowUI<UI_FurnitureSuitList>("UI_FurnitureSuitList").Refresh();
        }

        void OnEditorStateChange(GameObject go, PointerEventData data)
        {
                HomeManager.GetInst().SetBrickEditor();           
        }

        public void SetBtnUrl(string url)
        {
                ResourceManager.GetInst().LoadIconSpriteSyn(url, statebtn);
        }

        public void OnClickMyHero()  //英雄界面
        {

        }

        public void OnClickGO()  //出征界面
        {
                if (HomeManager.GetInst().GetState() == HomeState.BrickEditor)
                {
                        OnClickEditor();
                }
                UIManager.GetInst().ShowUI<UI_WorldMap>("UI_WorldMap");
                AudioManager.GetInst().PlaySE("SE_UI_Button2");
        }

        public void OnClickHeroPub() //酒馆
        {
/*                HeroPubManager.GetInst().GotoHub();*/
                UIManager.GetInst().CloseUI(this.name);

                AudioManager.GetInst().PlaySE("SE_UI_Button2");
        }

        public void OnClickBag()  //包裹
        {
                UIManager.GetInst().ShowUI<UI_AllBag>("UI_AllBag").RefreshGroup(UI_AllBag.BAG_TAB.ALL);
                UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", false);
                AudioManager.GetInst().PlaySE("SE_UI_Button2");
        }

        public void OnClickTask() //任务
        {
                UIManager.GetInst().ShowUI<UI_Task>("UI_Task").Refresh();
                UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", false);

                AudioManager.GetInst().PlaySE("SE_UI_Button2");
        }

        public void OnClickFurniture() //图纸
        {
                UIManager.GetInst().ShowUI<UI_FurnitureList>("UI_FurnitureList").Refresh();

                RefreshNewDarwNum("drawbtn", 0);
                AudioManager.GetInst().PlaySE("SE_UI_Button2");
        }

        public void OnClickNewHero()  //英雄界面
        {
                HomeManager.GetInst().StopCameraYAutoTweener();
                UIManager.GetInst().ShowUI<UI_HeroMain>("UI_HeroMain");

                AudioManager.GetInst().PlaySE("SE_UI_Button2");
        }

        public void Refresh()
        {
                GetText("name").text = PlayerController.GetInst().GetPropertyValue("name");
                RefreshResourceIcon();

                RefreshResource();     //刷新各种资源
                RefreshLockResource(); //刷新资源被锁

                RefreshLevel();
                RefreshExp();
                //RefreshFoodGroup();
                int num = HomeManager.GetInst().CheckNewFurnitureList();
                RefreshNewDarwNum("drawbtn", num);
                RefreshNewDarwNum("bagbtn", ItemManager.GetInst().NewItemCount);
                RefreshNewDarwNum("taskbtn", TaskManager.GetInst().NewTaskCount);
                RefreshNewDarwNum("newherobtn", 0);
        }

        public void RefreshNewDarwNum(string prefix, int num)
        {
                Transform tf = transform.Find("BTN/" + prefix + "/numbg");
                if (num == 0)
                {
                        tf.SetActive(false);
                }
                else
                {
                        tf.SetActive(true);
                        FindComponent<Text>(tf, "num").text = num.ToString();
                }

        }


        public GameObject[] ResourceObj;
        enum ResouceType //顺序和resource表里一致
        {
                none = 0,
                gold = 1,
                diamond = 2,
                wood = 3,
                hide = 4,
                stone = 5,
                crystal = 6,
                kizuna = 7,
                brick = 8,
                max = 9
        }

        void RefreshResourceIcon()
        {
                for (int i = 1; i < (int)ResouceType.max; i++)
                {
                        if (i == 7)
                        {
                                continue;
                        }
                        ResourceManager.GetInst().LoadIconSpriteSyn(CommonDataManager.GetInst().GetResourcesCfg(i).icon, GetImage(ResourceObj[i], "icon").transform);
                        //GetImage(ResourceObj[i], "icon").SetNativeSize();
                }          
        }

        void RefreshResource()
        {
                for (int i = 1; i < (int)ResouceType.max; i++)
                {
                        if (i == 7)
                        {
                                continue;
                        }
                        string attr_name = CommonDataManager.GetInst().GetResourcesCfg(i).attr;
                        long resource_value = PlayerController.GetInst().GetPropertyLong(attr_name);
                        long resource_max = PlayerController.GetInst().GetPropertyLong(attr_name + "_capacity");
                        GetText(ResourceObj[i], "num").text = resource_value.ToString();
                        if (resource_max > 0)  //钻石没有上限
                        {
                                GetImage(ResourceObj[i], "exp").fillAmount = (float)resource_value / resource_max;
                        }
                }      
        }

        public void RefreshResource(string attr_name, string old_value, string new_value)
        {
                long resource_max;
                int index;
                if (!attr_name.Contains("_capacity"))
                {
                        resource_max = PlayerController.GetInst().GetPropertyLong(attr_name + "_capacity");
                        index = (int)(ResouceType)Enum.Parse(typeof(ResouceType), attr_name);

                        if (resource_max > 0) //钻石没有上限
                        {
                                AppMain.GetInst().StartCoroutine(ResourceAnimation(old_value, new_value, resource_max, GetText(ResourceObj[index], "num"), GetImage(ResourceObj[index], "exp")));
                        }
                        else
                        {
                                AppMain.GetInst().StartCoroutine(ResourceAnimation(old_value, new_value, resource_max, GetText(ResourceObj[index], "num"), null));
                        }               
                }
                else
                {
                        string temp = attr_name.Replace("_capacity", "");
                        long resource_value = PlayerController.GetInst().GetPropertyLong(temp);
                        resource_max = PlayerController.GetInst().GetPropertyLong(attr_name);
                        index = (int)(ResouceType)Enum.Parse(typeof(ResouceType), temp);
                        if (resource_max > 0)  //钻石没有上限
                        {
                                GetImage(ResourceObj[index], "exp").fillAmount = (float)resource_value / resource_max;
                        }
                }          
        }

        IEnumerator ResourceAnimation(string old_object, string new_object, long max_value, Text text, Image silde, float duration = 1.5f)
        {

                long old_value = long.Parse(old_object);
                long new_value = long.Parse(new_object);

                float time = Time.realtimeSinceStartup;
                int now_value;

                if (Mathf.Abs(new_value - old_value) <= 10)
                {
                        duration = 0.5f;
                }

                while (Time.realtimeSinceStartup - time <= duration)
                {
                        if (this != null)
                        {
                                now_value = (int)Mathf.Lerp(old_value, new_value, (Time.realtimeSinceStartup - time) / duration);
                                text.text = now_value.ToString();
                                if (silde != null)
                                {
                                        silde.fillAmount = (float)now_value / max_value;
                                }
                                yield return null;
                        }
                }
                if (this != null)
                {
                        text.text = new_value.ToString();
                        if (silde != null)
                        {
                                silde.fillAmount = (float)new_value / max_value;
                        }
                }
        }

        public void RefreshLockResource()
        {
                long lock_resource = 0;
                long resource_max = 0;
                string name = "";
                for (int i = 1; i < (int)ResouceType.max; i++)
                {
                        if (i == 7)
                        {
                                continue;
                        }
                        name = CommonDataManager.GetInst().GetResourcesCfg(i).lock_attr;
                        lock_resource = (long)( PlayerController.GetInst().GetPropertyLong(name));
                        resource_max = PlayerController.GetInst().GetPropertyLong(CommonDataManager.GetInst().GetResourcesCfg(i).attr + "_capacity");
                        if (resource_max > 0)
                        {
                                if (lock_resource > 0)
                                {
                                        GetImage(ResourceObj[i], "lock_exp").gameObject.SetActive(true);
                                        GetImage(ResourceObj[i], "lock").gameObject.SetActive(true);
                                        float delta = (float)lock_resource / resource_max;
                                        if (delta >= 1)
                                        {
                                                delta = 1;
                                        }
                                        GetImage(ResourceObj[i], "lock_exp").fillAmount = delta;
                                        float size_x = GetImage(ResourceObj[i], "lock_exp").rectTransform.sizeDelta.x;
                                        GetImage(ResourceObj[i], "lock").rectTransform.anchoredPosition = Vector2.zero - new Vector2(0.5f * size_x * (1 - delta), 0);
                                }
                                else
                                {
                                        GetImage(ResourceObj[i], "lock_exp").gameObject.SetActive(false);
                                        GetImage(ResourceObj[i], "lock").gameObject.SetActive(false);
                                }
                        }
                }      
        }

        public void RefreshLevel()
        {
                GetText("level").text = PlayerController.GetInst().GetPropertyValue("house_level");
        }

        public void RefreshExp()
        {
                int level = PlayerController.GetInst().GetPropertyInt("house_level");
                int exp = PlayerController.GetInst().GetPropertyInt("house_exp");
                HomeExpHold heh = CommonDataManager.GetInst().GetHomeExpCfg(level + 1);
                if (heh != null)
                {
                        GetImage("exp").fillAmount = (float)exp / heh.need_exp;
                }
        }

        public GameObject food_group;
        public void RefreshFoodGroup()
        {
                GameObject food = GetGameObject(food_group, "food0");
                SetChildActive(food_group.transform,false);
                int now_food = PlayerController.GetInst().GetPropertyInt("food");
                int max_food = GlobalParams.GetInt("int_food_quantity");  //最大值
                if (now_food >= max_food)
                {
                        now_food = max_food;
                }
                float show_food = now_food / 2.0f;
                int show_max_food = max_food / 2;
                for (int i = 0; i < show_max_food; i++)
                {
                        GameObject temp = GetGameObject(food_group, "food" + i);
                        if (temp == null)
                        {
                                temp = CloneElement(food, "food" + i);
                        }
                        if (i < show_food)
                        {
                                if (show_food - 0.5f > i)
                                {
                                        ResourceManager.GetInst().LoadIconSpriteSyn("Bg#shiwu2", temp.transform);
                                }
                                else //半个
                                {
                                        ResourceManager.GetInst().LoadIconSpriteSyn("Bg#shiwu1", temp.transform);
                                }
                        }
                        else
                        {
                                ResourceManager.GetInst().LoadIconSpriteSyn("Bg#shiwu0", temp.transform);
                        }
                        temp.SetActive(true);
                }
        }

        public override void OnShow(float time)
        {
                base.OnShow(time);
                Refresh();
                if (PlayerController.GetInst().IsGuideFinish(5))
                {
                        GetGameObject("fightbtn").SetActive(true);
                }
                else
                {
                        GetGameObject("fightbtn").SetActive(false);
                }
        }
        public void ShowFightBtn()
        {
                GetGameObject("fightbtn").SetActive(true);
        }
        
        void ResetBuildingState()
        {
                foreach (Button btn in gameObject.GetComponentsInChildren<Button>(true))
                {
                        btn.onClick.AddListener(OnBuildingReset);
                }
        }

        public void OnBuildingReset()
        {
                HomeManager.GetInst().ResetBuildingState();
        }


        #region 通知

        Button btn;
        HashSet<int> BtnOnSet = new HashSet<int>();

        public void ShowNewTip(NotifyType type, Action<object> onClick,string des, object data)
        {
                int typeNum = (int)type;
                if (BtnOnSet.Contains(typeNum))
                {
                        return;
                }
                else
                {
                        GameObject newBtn = CloneElement(btn.gameObject);
                        newBtn.SetActive(true);
                        FindComponent<Text>(newBtn, "Text").text = des;
                        newBtn.GetComponent<Button>().onClick.AddListener(() => ClickBtn(typeNum, onClick, newBtn, data));
                        BtnOnSet.Add(typeNum);
                }                          
        }

        void ClickBtn(int typeNum, Action<object> onClick, GameObject go, object data)
        {
                onClick(data);
                GameObject.Destroy(go);
                BtnOnSet.Remove(typeNum);
        }

        #endregion
}


public enum NotifyType
{
        NewNpc = 0,
}