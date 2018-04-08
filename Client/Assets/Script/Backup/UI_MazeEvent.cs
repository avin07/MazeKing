// using UnityEngine;
// using UnityEngine.UI;
// using System.Collections;
// 
// class UI_MazeEvent : UIBehaviour
// {
//         Button m_BtnConfirm;
//         Button m_BtnCancel;
//         Image m_ImageBg;
//         Image m_Icon, m_ConfirmIcon, m_CancelIcon;
//         Text m_TextTitle, m_TextDesc, m_TextConfirm, m_TextCancel, m_TextTip;
//         // Use this for initialization
//         void Awake()
//         {
//                 m_BtnConfirm = GetButton("confirm");
//                 m_BtnCancel = GetButton("cancel");
//                 m_ImageBg = GetImage("bg");
//                 m_Icon = GetImage("icon");
// 
//                 m_TextTitle = GetText("title");
//                 m_TextDesc = GetText("event_desc");
//                 m_TextConfirm = GetText("confirm_desc");
//                 m_TextCancel = GetText("cancel_desc");
//                 m_TextTip = GetText("op_desc");
// 
//                 m_ConfirmIcon = GetImage("confirmicon");
//                 m_CancelIcon = GetImage("cancelicon");
//         }
// 
//         public void OnClickConfirm(GameObject go)
//         {
//                 UIManager.GetInst().CloseUI(this.name);
//                 if (m_TreasureCfg != null)
//                 {
//                         MazeManager.GetInst().SendOpenTreasure(false, /*petId, idItem*/m_nCurrSelectPetID, 0);
//                 }
//                 else
//                 {
//                         MazeManager.GetInst().SendTriggerEvent(false, m_nCurrSelectPetID);
//                 }
//                 MazeEventManager.GetInst().ClearEventSelectCharacter();
//         }
//         public void OnClickCancel(GameObject go)
//         {
//                 UIManager.GetInst().CloseUI(this.name);
//                 if (m_TreasureCfg != null)
//                 {
//                         MazeManager.GetInst().SendOpenTreasure(true, m_nCurrSelectPetID, 0);
//                 }
//                 else
//                 {
//                         MazeManager.GetInst().SendTriggerEvent(true, m_nCurrSelectPetID);
//                 }
//                 MazeEventManager.GetInst().FinishTrigger();
//                 MazeEventManager.GetInst().ClearEventSelectCharacter();
//         }
// 
//         GameObject m_BelongObj;
//         public void SetBelongObj(GameObject go)
//         {
//                 m_BelongObj = go;
//         }
// 
//         long m_nCurrSelectPetID;
//         public void SetIcon(TeamBehav hero)
//         {
//                 if (m_TreasureCfg != null)
//                 {
//                         if (m_TreasureCfg.career_open_condition == "-1")
//                         {
//                                 m_nCurrSelectPetID = 0;
//                                 return;
//                         }
//                         m_nCurrSelectPetID = hero.PetID;
//                         ResourceManager.GetInst().LoadCharactorIcon(ModelResourceManager.GetInst().GetIconRes(hero.Character.CharacterCfg.modelid), m_Icon.transform);
// 
//                         if (m_TreasureCfg.career_open_condition == "0" || m_TreasureCfg.career_open_condition == hero.Character.CharacterId.ToString())
//                         {
//                                 ResourceManager.GetInst().LoadOtherIcon(m_TreasureCfg.right_open_icon, m_ConfirmIcon.transform);
//                                 m_TextConfirm.text = LanguageManager.GetText(m_TreasureCfg.right_open_desc);
//                         }
//                         else
//                         {
//                                 ResourceManager.GetInst().LoadOtherIcon(m_TreasureCfg.wrong_open_icon, m_CancelIcon.transform);
//                                 m_TextConfirm.text = LanguageManager.GetText(m_TreasureCfg.wrong_open_desc);
//                         }
//                 }
//                 else if (m_EventCfg != null)
//                 {
//                         m_nCurrSelectPetID = hero.PetID;
//                         ResourceManager.GetInst().LoadCharactorIcon(ModelResourceManager.GetInst().GetIconRes(hero.Character.CharacterCfg.modelid), m_Icon.transform);
// 
//                         m_BtnConfirm.enabled = true;
//                 }
//         }
//         MazeTreasureConfig m_TreasureCfg;
//         public void SetTreasureDetail(MazeTreasureConfig mtc)
//         {
//                 m_TreasureCfg = mtc;
//                 m_TextConfirm.text = LanguageManager.GetText(mtc.wrong_open_desc);
//                 m_TextCancel.text = LanguageManager.GetText(mtc.wrong_open_desc);
//                 m_TextDesc.text = LanguageManager.GetText(mtc.desc);
//                 m_TextTitle.text = LanguageManager.GetText(mtc.name);
//                 m_TextTip.text = LanguageManager.GetText(mtc.open_condition_desc);
// 
//                 ResourceManager.GetInst().LoadOtherIcon(mtc.open_condition_icon, m_Icon.transform);
//                 ResourceManager.GetInst().LoadOtherIcon(mtc.wrong_open_icon, m_ConfirmIcon.transform);
//                 ResourceManager.GetInst().LoadOtherIcon(mtc.abandon_icon, m_CancelIcon.transform);
// 
//                 MazeEventManager.GetInst().SetEventSelectCharacter(m_TreasureCfg.career_open_condition);
//                 UIManager.GetInst().GetUIBehaviour<UI_Bag>().SetEventSelectItem(m_TreasureCfg.item_open_condition);
//         }
// 
//         public override void OnShow()
//         {
//                 base.OnShow();
//                 m_nCurrSelectPetID = 0;
//                 m_TreasureCfg = null;
//                 m_EventCfg = null;
//                 m_Icon.sprite = null;
//         }
// 
//         MazeEventConfig m_EventCfg;
//         public void SetEvent(MazeEventConfig mec)
//         {
//                 m_EventCfg = mec;
//                 m_TextConfirm.text = LanguageManager.GetText(mec.carry_desc);
//                 m_TextCancel.text = LanguageManager.GetText(mec.abandon_desc);
//                 m_TextTip.text = LanguageManager.GetText(mec.carry_choose_desc);
//                 m_TextDesc.text = LanguageManager.GetText(mec.desc);
//                 m_TextTitle.text = LanguageManager.GetText(mec.name);
// 
//                 ResourceManager.GetInst().LoadOtherIcon(mec.carry_choose_icon, m_Icon.transform);
//                 ResourceManager.GetInst().LoadOtherIcon(mec.carry_icon, m_ConfirmIcon.transform);
//                 ResourceManager.GetInst().LoadOtherIcon(mec.abandon_icon, m_CancelIcon.transform);
//                 m_BtnConfirm.enabled = false;
// 
//                 MazeEventManager.GetInst().SetEventSelectCharacter("0");
//         }
//