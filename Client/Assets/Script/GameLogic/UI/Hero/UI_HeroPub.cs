//using UnityEngine;
//using System.Collections;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using System.Collections.Generic;
//using System;

//public class UI_HeroPub : UI_ScrollRectHelp
//{
//    public Image bg;
//    public Text time;
//    public GameObject skill;
//    Sprite start_off;
//    public GameObject get;
//    void Awake()
//    {
//        SetBgActive(false);
//        start_off = skill.GetComponent<Image>().sprite;
//        get = GetGameObject("get");
//    }



//    public void SetBgActive(bool isActive)
//    {
//        bg.gameObject.SetActive(isActive);
//    }


//    public void OnClickReturn()  //返回
//    {
//        UIManager.GetInst().CloseUI(this.name);
//        HomeManager.GetInst().LoadHome();
//    }


//    public void OnClickGet()
//    {
//        if (PetManager.GetInst().IsHeroBagFull())
//        {
//                GameUtility.PopupMessage("英雄背包已经满了,请先清理！");
//                return;
//        }


//        if (CommonDataManager.GetInst().GetNowResourceNum("gold") >= int.Parse(GetText(bg.gameObject, "goldneed").text))
//        {
//            HeroPubManager.GetInst().SendHeroGet();
//        }
//        else
//        {
//                GameUtility.PopupMessage("金币不足！");
//        }
//    }

//    int second;
//    void Update()
//    {
//        second = (int)(HeroPubManager.GetInst().GetTime() - Time.realtimeSinceStartup);
//        if (second > 0)
//        {
//            time.text = UIUtility.GetTimeString1(second);
//        }
//        else
//        {
//            time.text = "00:00:00";
//            HeroPubManager.GetInst().SendPubQuery();// 重新请求时间//
//            second = 10; //防止网络原因频繁发消息；
//        }
        
//    }

//    public void UpdateHeroInfo(int id,int desk)
//    {
//        CharacterConfig cc = CharacterManager.GetInst().GetCharacterCfg(id);
//        GetText(bg.gameObject,"name").text = CharacterManager.GetInst().GetCharacterName(id);
//        GetText(bg.gameObject,"career").text = CharacterManager.GetInst().GetCareerSysName(cc.GetPropInt("career_sys"));
//        GetText(bg.gameObject, "des").text = LanguageManager.GetText(cc.GetProp("desc"));
//        SetStar(cc.GetPropInt("star"), cc.GetPropInt("max_star"), bg.gameObject);
//        GetText(bg.gameObject, "goldhave").text = CommonDataManager.GetInst().GetNowResourceNum("gold").ToString();
//        GetText(bg.gameObject, "goldneed").text = HeroPubManager.GetInst().GetDeskCost(desk);

//        GetText(bg.gameObject, "atk").text = cc.GetProp("init_attack");
//        GetText(bg.gameObject, "hp").text = cc.GetProp("init_life");
//        GetText(bg.gameObject, "def").text = cc.GetProp("init_defence");
//        GetText(bg.gameObject, "atk_speed").text = cc.GetProp("init_attack_speed");


//        if (CommonDataManager.GetInst().GetNowResourceNum("gold") >= int.Parse(HeroPubManager.GetInst().GetDeskCost(desk)))
//        {
//                UIUtility.SetUIEffect(this.name, get, true, "pub_button_effect");
//        }
//        else
//        {
//                UIUtility.SetUIEffect(this.name, get, false, "pub_button_effect");
//        }
//        RefreshSkill(cc);

//    }

//    void RefreshSkill(CharacterConfig cc)
//    {
//        GameObject plane = skill.transform.parent.gameObject;


//        for (int i = 0; i < 100; i++)
//        {
//            GameObject temp = GetGameObject(plane, "skill" + i);
//            if (temp != null)
//            {
//                temp.gameObject.SetActive(false);
//            }
//            else
//            {
//                break;
//            }
//        }

//        string skill_list_active = cc.GetProp("skill_list");
//        string[] active_skill = skill_list_active.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

//        string skill_list_adventure = cc.GetProp("adventure_skill");
//        string[] adventure_skill = skill_list_adventure.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

//        List<int> skill_list = new List<int>();
//        for (int i = 0; i < active_skill.Length; i++)
//        {
//            skill_list.Add(int.Parse(active_skill[i]));
//        }
//        for (int i = 0; i < adventure_skill.Length; i++)
//        {
//            skill_list.Add(int.Parse(adventure_skill[i]));
//        }

//        int count = 0;
//        for (int i = 0; i < skill_list.Count; i++)
//        {
//            int skill_id = skill_list[i];
//            if (SkillManager.GetInst().GetSkillInfo(skill_id) != null)
//            {
//                GameObject temp = GetGameObject(plane, "skill" + count);
//                if (temp == null)
//                {
//                    temp = GameObject.Instantiate(skill) as GameObject;
//                    temp.transform.SetParent(plane.transform);
//                    temp.name = "skill" + count;
//                    temp.transform.localScale = Vector3.one;
//                    temp.transform.localPosition = Vector3.zero;
//                }
//                ResourceManager.GetInst().LoadIconSpriteSyn(SkillManager.GetInst().GetSkillIconUrl(skill_id), temp.transform);
//                temp.gameObject.SetActive(true);
//                count++;
//                EventTriggerListener listener = EventTriggerListener.Get(temp);
//                listener.SetTag(skill_id);
//                listener.onClick = OnShowSkillTip;
//            }         
//        }

//        StartCoroutine(WaitForGird(plane));
//    }


//    void OnShowSkillTip(GameObject go, PointerEventData data)
//    {
//        HeroPubManager.GetInst().SetCameraVague(true);
//        UI_TipInfo uti = UIManager.GetInst().ShowUI<UI_TipInfo>("UI_TipInfo");

//        int skill_id = (int)EventTriggerListener.Get(go).GetTag();
//        uti.SetName(SkillManager.GetInst().GetSkillName(skill_id));
//        uti.SetText(SkillManager.GetInst().GetSkillDesc(skill_id));
//    }




//    public IEnumerator WaitForGird(GameObject plane)
//    {
//        yield return new WaitForEndOfFrame();
//        //设置content大小//

//        float height = plane.GetComponent<RectTransform>().anchoredPosition.y * (-1) + plane.GetComponent<RectTransform>().sizeDelta.y + 40.0f;
//        GameObject content = GetGameObject(bg.gameObject, "content");
//        content.GetComponent<RectTransform>().sizeDelta = new Vector2(content.GetComponent<RectTransform>().sizeDelta.x, height);
//    }

//    void SetStar(int now_star, int max_star, GameObject pet_btn)
//    {
//        foreach (Transform tf in pet_btn.GetComponentsInChildren<Transform>(true))
//        {
//            if (tf.name.Contains("Clone"))
//            {
//                GameObject.Destroy(tf.gameObject);
//            }
//        }
//        for (int i = 1; i < now_star; i++)
//        {
//            CreatStar(true, pet_btn);
//        }
//        for (int j = now_star; j < max_star; j++)
//        {
//            CreatStar(false, pet_btn);
//        }
//    }

//    void CreatStar(bool is_on, GameObject pet_btn) //是否是点亮的
//    {
//        Image star = null;
//        Image star0 = GetImage(pet_btn, "star0");
//        star = GameObject.Instantiate(star0) as Image;
//        star.transform.SetParent(star0.transform.parent);
//        star.transform.localScale = Vector3.one;
//        star.transform.localPosition = Vector3.zero;
//        if (!is_on)
//        {
//            star.sprite = start_off;
//        }
//    }

//    public void  RefreshGold()
//    {
//            GetText(bg.gameObject, "goldhave").text = CommonDataManager.GetInst().GetNowResourceNum("gold").ToString();
//    }
   
//}


