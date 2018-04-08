using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class UI_GetReward : UIBehaviour
{

        void Awake()
        {
                this.UILevel = UI_LEVEL.TIP;
                mTitle = FindComponent<Image>("Image/Title");
                mItem = FindComponent<Transform>("Image/content/item");
                mConfirm = FindComponent<Button>("Image/Confirm");
                gou = FindComponent<Image>("Image/gou");
                des = FindComponent<Text>("Image/des");
                mConfirm.onClick.AddListener(ClickConfirm);
        }

        Image mTitle;
        Transform mItem;
        Button mConfirm;
        Image gou;
        Text des;
        public Action<int,object> confirmAction;
        object mData;
        int rewardIndex = 0;

        public void SetReward(string reward, int getType, Action<int,object> confirm,object data)
        {
                SetTitle();
                if (getType == 0)  //固定奖励
                {
                        mConfirm.interactable = true;
                        des.text = "获得奖励";
                }
                else            //选择奖励   
                {
                        mConfirm.interactable = false;
                        des.text = "请选择一种奖励";
                }

                RefreshReward(reward, getType);
                confirmAction = confirm;
                mData = data;
        }


        void RefreshReward(string reward,int getType)
        {
                string[] rewardStr = reward.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                SetChildActive(mItem.parent, false);

                int num;
                string name;
                int id;
                string des;
                Thing_Type type;
                Transform item;

                for (int i = 0; i < rewardStr.Length; i++)
                {
                        item = GetChildByIndex(mItem.parent, i);
                        if (item == null)
                        {
                                item = CloneElement(mItem.gameObject).transform;
                        }
                        item.SetActive(true);

                        Image quality_image = FindComponent<Image>(item, "quality");
                        Image icon_image = FindComponent<Image>(item, "icon");
                        Text num_down = FindComponent<Text>(item, "num_down");

                        CommonDataManager.GetInst().SetThingIcon(rewardStr[i], icon_image.transform, quality_image.transform,out name, out num, out id, out des, out type);
                        num_down.text = num.ToString();

                        Button btn = item.GetComponent<Button>();
                        if (getType == 0)
                        {
                                btn.enabled = false;
                        }
                        else
                        {
                                int index = i;
                                btn.enabled = true;
                                btn.onClick.AddListener(() => ClickReward(index,btn.transform));
                        }
                }     
        }


        void ClickReward(int index, Transform tf)
        {
                rewardIndex = index;
                mConfirm.interactable = true;
                gou.transform.SetParent(tf);
                gou.rectTransform.anchoredPosition = Vector2.zero;
        }


        void SetTitle()
        {
                //mTitle.sprite = 
        }

        public void ClickConfirm()
        {
                if (confirmAction != null)
                {
                        if (CommonDataManager.GetInst().IsBagFull())
                        {
                                GameUtility.PopupMessage("背包已经满了,请先清理！");
                                UIManager.GetInst().CloseUI(this.name);
                                return;
                        }
                        else
                        {
                                confirmAction(rewardIndex,mData);                               
                        }
                }
                UIManager.GetInst().CloseUI(this.name);
        }

}


