using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class UI_NewFurniture : UIBehaviour
{
        GameObject mBuild0;
        void Awake()
        {
                mBuild0 = GetGameObject("build0");
        }

        public void SetInfo(int buildId)
        {
                mBuild0.SetActive(true);

                BuildingInfoHold infoCfg = HomeManager.GetInst().GetBuildInfoCfg(buildId * 100 + 1);
                BuildingTypeHold typeCfg = HomeManager.GetInst().GetBuildTypeCfg(buildId);
                if (infoCfg == null || typeCfg == null)
                {
                        OnClickClose(null);
                        return;
                }
                Transform temp = mBuild0.transform;
                Transform canbuild = temp.Find("canbuild");

                Image newlabel = FindComponent<Image>(temp, "bg/newlabel");
                temp.name = typeCfg.id.ToString();
                FindComponent<Text>(temp, "bg/name").text = LanguageManager.GetText(infoCfg.name);
                FindComponent<Text>(temp, "bg/des").text = LanguageManager.GetText(infoCfg.desc);
                ResourceManager.GetInst().LoadIconSpriteSyn(infoCfg.icon, FindComponent<Image>(temp, "bg/icon"));
                Button btn = FindComponent<Button>(temp, "bg");
                btn.onClick.RemoveAllListeners();

                int capacity = typeCfg.take_capacity;
                FindComponent<Text>(canbuild, "capacity").text = capacity.ToString();
                int buildNowNum = HomeManager.GetInst().GetBuildTypeInHome(buildId).Count;   //当前已建造

                canbuild.SetActive(true);

                newlabel.enabled = HomeManager.GetInst().IsNewFurniture(buildId);
                BuildingLimitHold limit = HomeManager.GetInst().GetBuildingLimitCfg(buildId);
                if (limit != null)
                {
                        canbuild.Find("num_tip").SetActive(true);
                        if (limit.max_number >= 100)
                        {
                                FindComponent<Text>(canbuild, "num").text = "∞";
                        }
                        else
                        {
                                FindComponent<Text>(canbuild, "num").text = (limit.max_number - buildNowNum).ToString();
                        }
                }
                else  //程序判定为没有建造数量限制
                {
                        canbuild.Find("num_tip").SetActive(false);
                        FindComponent<Text>(canbuild, "num").text = string.Empty;
                }

                string creatCost = infoCfg.cost_material;
                bool canBuild = RefreshNeed(creatCost, canbuild);
                FindComponent<Text>(canbuild, "times").text = UIUtility.GetTimeString3(infoCfg.cost_time);

                //UIUtility.SetGroupGray(false, temp.gameObject);
        }

        bool RefreshNeed(string needInfo, Transform root)
        {
                bool canBuild = true;
                Transform content = root.Find("cost");
                Transform item = content.GetChild(0);
                string[] need = needInfo.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                SetChildActive(content, false);

                int num = 0;
                string name = string.Empty;
                int id = 0;
                string des = string.Empty;
                Thing_Type type;

                for (int i = 0; i < need.Length; i++)
                {
                        Transform temp = GetChildByIndex(content, i);
                        if (temp == null)
                        {
                                temp = CloneElement(item.gameObject).transform;
                        }
                        CommonDataManager.GetInst().SetThingIcon(need[i], temp.Find("icon"), null, out name, out num, out id, out des, out type);
                        int has_num = CommonDataManager.GetInst().GetThingNum(need[i], out num);
                        if (has_num >= num)
                        {
                                FindComponent<Text>(temp, "item_num").text = num.ToString();
                        }
                        else
                        {
                                FindComponent<Text>(temp, "item_num").text = "<color=red>" + num + "</color>";
                                canBuild = false;
                        }
                        temp.SetActive(true);
                }
                return canBuild;
        }

}