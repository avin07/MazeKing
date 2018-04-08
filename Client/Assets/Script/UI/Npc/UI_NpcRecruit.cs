using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UI_NpcRecruit : UIBehaviour
{
        public long npcId;
        Animation anim;
        GameObject npcObj;
        long nextId;
        Transform SkillGroup;
        Transform SkillTip;
        Pet pet;

        void Awake()
        {

                SkillGroup = transform.Find("skillgroup");
                SkillTip = transform.Find("skilltipgroup");
                EventTriggerListener.Get(transform.Find("mod_drag")).onDrag = OnModelDarg;
                EventTriggerListener.Get(transform.Find("mod_drag")).onClick = OnModelClick;
                EventTriggerListener.Get(gameObject).onClick = OnClickTipClose;
                EventTriggerListener.Get(transform.Find("close")).onClick = OnClose;
                FindComponent<Button>("disband").onClick.AddListener(OnDisband);  //遣散
                FindComponent<Button>("get").onClick.AddListener(OnGet);  //招募
                FindComponent<Button>("next").onClick.AddListener(OnNext);  //遣散
                ResourceManager.GetInst().LoadIconSpriteSyn(CommonDataManager.GetInst().GetResourcesCfg(1).icon, FindComponent<Image>("icon").transform);
        }

        public void Refresh(long id)
        {
                HomeManager.GetInst().StopCameraYAutoTweener();
                UIManager.GetInst().SetUIActiveState<UI_HomeMain>("UI_HomeMain", false);
                HomeManager.GetInst().SaveCameraData();

                RefreshUI(id);
        }

        public void ReClick()
        {
                anim.Stop();
                GameUtility.ObjPlayAnim(npcObj, CommonString.skill_001Str, false, 0.5f);
        }

        void RefreshUI(long id)
        {
                HomeManager.GetInst().ResetShowMod();
                npcObj = HomeManager.GetInst().ChangeCameraForNpc(id);
                if (npcId == null)
                {
                        return;
                }

                npcId = id;
                anim = npcObj.GetComponent<Animation>();

                GameUtility.ObjPlayAnim(npcObj, CommonString.skill_001Str, false, 0.5f);

                NpcInfo info = NpcManager.GetInst().GetNpc(id);
                NpcConfig nc = NpcManager.GetInst().GetNpcCfg(info.idConfig);
                if (nc != null)
                {
                        pet = new Pet(nc.type_id);
                        ResourceManager.GetInst().LoadIconSpriteSyn(ModelResourceManager.GetInst().GetIconRes(nc.model), transform.Find("head"));
                        FindComponent<Text>("name").text = LanguageManager.GetText(nc.name);
                        FindComponent<Text>("des").text = LanguageManager.GetText(nc.desc);
                        SetStar(pet, transform.Find("star_group"));
                      
                        FindComponent<Text>("cost").text = nc.price.ToString();
                        if (CommonDataManager.GetInst().GetNowResourceNum("gold") >= nc.price)
                        {
                                FindComponent<Text>("cost").color = Color.black;
                                FindComponent<Button>("get").interactable = true;
                        }
                        else
                        {
                                FindComponent<Text>("cost").color = Color.red;
                                FindComponent<Button>("get").interactable = false;
                        }
                        RefreshSkill();

                        //重置招募npc头顶图标//
                        HomeManager.GetInst().ChangeNpcNewIcon(id, nc);
                }

                nextId = HomeManager.GetInst().GetNextHotelNpcId(id);
                if(id != nextId)
                {
                        FindComponent<Button>("next").gameObject.SetActive(true);
                }
                else
                {
                        FindComponent<Button>("next").gameObject.SetActive(false);
                }
        }

        void RefreshSkill()
        {          
                SetChildActive(SkillGroup, false);
                if (pet == null)
                {
                        return;
                }
                List<int> m_skill = pet.GetAllMyClientSkill();
                for (int i = 0; i < m_skill.Count; i++)
                {
                        SkillLearnConfigHold sfh = SkillManager.GetInst().GetSkillInfo(m_skill[i]);
                        if (sfh != null)
                        {
                                Transform m_skill_tf = null;
                                if (i < SkillGroup.childCount)
                                {
                                        m_skill_tf = SkillGroup.GetChild(i);
                                }
                                else
                                {
                                        m_skill_tf = CloneElement(SkillGroup.GetChild(0).gameObject).transform;
                                }
                                m_skill_tf.SetActive(true);
                                ResourceManager.GetInst().LoadIconSpriteSyn(SkillManager.GetInst().GetSkillIconUrl(m_skill[i]), m_skill_tf);

                                EventTriggerListener listener = EventTriggerListener.Get(m_skill_tf.Find("btn"));
                                listener.onClick = OnSkill;
                                listener.SetTag(m_skill[i]);

                        }
                }
        }

        private void OnSkill(GameObject go, PointerEventData data)
        {
                int id = (int)EventTriggerListener.Get(go).GetTag();
                SetTipGroup(id, go.transform.parent.GetComponent<Image>().sprite);
        }


        void SetTipGroup(int id, Sprite sprite)
        {
                SkillTip.SetActive(true);
                int skillId = id;
                FindComponent<Text>(SkillTip, "skillname").text = SkillManager.GetInst().GetSkillName(skillId);
                FindComponent<Text>(SkillTip, "skilldesc").text = SkillManager.GetInst().GetSkillDesc(skillId);
                FindComponent<Text>(SkillTip, "skilllv").text = "Lv." + 1;
                FindComponent<Image>(SkillTip, "icon").sprite = sprite;

        }


        void OnClickTipClose(GameObject go, PointerEventData data)
        {
                ClickSkillTip();
        }

        void ClickSkillTip()
        {
                if (SkillTip.gameObject.activeInHierarchy)
                {
                        if (EventSystem.current.currentSelectedGameObject == null)
                        {
                                SkillTip.SetActive(false);
                        }
                }
        }

        float rotate_offset = -1.5f;
        void OnModelDarg(GameObject go, PointerEventData eventdata)
        {
                if (npcObj != null)
                {
                        npcObj.transform.Rotate(Vector3.up, eventdata.delta.x * rotate_offset);
                }
        }

        void OnModelClick(GameObject go, PointerEventData eventdata)
        {
                ReClick();
        }


        void Update()
        {              
                if (anim != null && anim.IsPlaying(CommonString.skill_001Str))
                {
                        if (anim[CommonString.skill_001Str].normalizedTime >= 0.9f)
                        {
                                GameUtility.ObjPlayAnim(npcObj, CommonString.idle_001Str, true);
                        }
                }
        }

        public void OnClose(GameObject go, PointerEventData data)
        {
                OnClickClose(null);
                HomeManager.GetInst().ResetHomeCamera();
        }

        void OnDisband()
        {
                HomeManager.GetInst().SendHotelDisband(npcId);
                OnClose(null, null);
        }

        void OnGet()
        {
                SkillTip.SetActive(false);
                HomeManager.GetInst().SendHotelGetHero(npcId);
                //OnClose(null, null);
        }

        void OnNext()
        {
                SkillTip.SetActive(false);
                RefreshUI(nextId);
        }
}
