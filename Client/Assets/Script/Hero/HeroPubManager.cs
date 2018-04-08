using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Message;

class HeroPubManager : SingletonObject<HeroPubManager>
{
        Dictionary<int, PubConfigHold> m_PubDict = new Dictionary<int, PubConfigHold>();
        public void Init()
        {
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCPetMsgPubQuery), OnGetPubTime);
                MsgMgr.GetInst().RegisterMsgHandler(typeof(SCPetMsgPubHire), OnPubPetGet);
                ConfigHoldUtility<PubConfigHold>.LoadXml("Config/pub", m_PubDict);
        }

        public string GetDeskCost(int desk)
        {
                if (m_PubDict.ContainsKey(desk))
                {
                        return m_PubDict[desk].need_money.ToString();
                }
                return CommonString.zeroStr;
        }


        void OnGetPubTime(object sender, SCNetMsgEventArgs e)
        {
                SCPetMsgPubQuery msg = e.mNetMsg as SCPetMsgPubQuery;
                rest_time = (long)(msg.time + Time.realtimeSinceStartup);
        }


        void OnPubPetGet(object sender, SCNetMsgEventArgs e)
        {
                SCPetMsgPubHire msg = e.mNetMsg as SCPetMsgPubHire;
                if (msg.error_type == 0)
                {

                }
                else
                {
                        GameUtility.PopupMessage("招募错误！");
                        m_select_hero.GetComponent<PubHeroBehaviour>().Disappear(0, 1.0f);
                }
                can_choose = true;
        }


        public void GetHero()
        {
                GetEffectObj.transform.SetParent(m_select_hero.transform);
                GetEffectObj.transform.localPosition = Vector3.zero;
                m_select_hero.GetComponent<PubHeroBehaviour>().Disappear(0, 1.0f);
        }

        public void ChangeHero(string name, int index, int characterID, int desk)
        {
                string modName = name.Split('_')[0] + index;
                GameObject obj = point.transform.Find(modName).gameObject;

                if (obj != null)
                {
                        if (obj.transform.Find(modName + "_child") != null)
                        {
                                GameObject.Destroy(obj.transform.Find(modName + "_child").gameObject);
                        }
                }
                //ModelLoadBehaviour mlb = obj.GetComponent<ModelLoadBehaviour>();
                CharacterConfig characterCfg = CharacterManager.GetInst().GetCharacterCfg(characterID);
                if (characterCfg != null)
                {
                        GameObject newMod = CharacterManager.GetInst().GenerateModel(characterCfg);
                        newMod.name = modName + "_child";
                        newMod.transform.SetParent(obj.transform);
                        newMod.transform.localPosition = Vector3.zero;
                        newMod.transform.localRotation = Quaternion.identity;
                        HeroPub hp = new HeroPub(characterID, desk, index);
                        MyModleOperte(newMod, hp);
                }
        }

        string name = "";
        GameObject root;
        GameObject point;
        public Camera camera;
        long rest_time = 0;
        UI_HeroPub phb;


        int desk = 0;   //当前选中的
        int charactar_id = 0;     //当前选中的
        int index = 0;

        GameObject m_select_hero;
        bool can_choose = true;

        public void SetHeroInfo(int m_desk, int m_id, int m_index, GameObject go)
        {
                desk = m_desk;
                charactar_id = m_id;
                index = m_index;
                m_select_hero = go;
        }

        public long GetTime()
        {
                return rest_time;
        }

        public bool CanChoose()
        {
                return can_choose;
        }

        public UI_HeroPub GetMyHeroPub()
        {
                return phb;
        }

        public void GotoHub()
        {
                can_choose = true;
                SendPubQuery();
                name = GlobalParams.GetString("pub_scene");

                AssetBundle ab = ResourceManager.GetInst().LoadAB("Scene/10002");
                AppMain.GetInst().StartCoroutine(LoadSence(ab, name));
//                 AssetBundleStruct m_abs = new AssetBundleStruct("AreaMap/" + name, MapLoadCallBack, name, ResourceManager.AssetBundleKind.Sence);
//                 AppMain.GetInst().StartCoroutine(ResourceManager.GetInst().ResourceLoad(m_abs));
        }

//         void MapLoadCallBack(WWW www, AssetBundleStruct abs)
//         {
//                 if (www != null)
//                 {
//                         AppMain.GetInst().StartCoroutine(LoadSence(www.assetBundle, (string)abs.tag));
//                 }
//         }

        IEnumerator LoadSence(AssetBundle ab, string tag)
        {
                AsyncOperation async = Application.LoadLevelAsync(tag);
                while (async.isDone == false || async.progress < 1.0f)
                {
                        yield return null;
                }
                ab.Unload(false);
                InitSence();
        }



        struct HeroPub
        {
                public int desk;
                public int charactar_id;
                public int index;
                public HeroPub(int m_id, int m_desk, int m_index)
                {
                        desk = m_desk;
                        charactar_id = m_id;
                        index = m_index;
                }
        }

        void InitSence()
        {
                phb = UIManager.GetInst().ShowUI<UI_HeroPub>("UI_HeroPub");
                GameStateManager.GetInst().GameState = GAMESTATE.OTHER;

                root = GameObject.Find(name);
                Transform cameraTrans = root.transform.Find("Main Camera");
                if (cameraTrans != null)
                {
                        camera = cameraTrans.GetComponent<Camera>();
                        camera.cullingMask = 1 << LayerMask.NameToLayer("Default");
                }
                SetCameraVague(false);
                point = root.transform.Find("Point").gameObject;
                DownLoadModel("three_star_list", 1);
                DownLoadModel("four_star_list", 2);
                DownLoadModel("five_star_list", 3);
        }

        void DownLoadModel(string name, int desk)
        {
                string info = PlayerController.GetInst().GetPropertyValue(name);
                string[] temp = info.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < temp.Length; i++)
                {
                        if (temp[i].Equals('0'))  //说明该位置没有宠物
                        {
                                continue;
                        }
                        string modName = name.Split('_')[0] + (i + 1);
                        GameObject obj = point.transform.Find(modName).gameObject;
                        //ModelLoadBehaviour mlb = obj.AddComponent<ModelLoadBehaviour>();

                        CharacterConfig characterCfg = CharacterManager.GetInst().GetCharacterCfg(int.Parse(temp[i]));
                        if (characterCfg != null)
                        {
                                GameObject newMod = CharacterManager.GetInst().GenerateModel(characterCfg);
                                newMod.name = modName + "_child";
                                newMod.transform.SetParent(obj.transform);
                                newMod.transform.localPosition = Vector3.zero;
                                newMod.transform.localRotation = Quaternion.identity;

                                HeroPub hp = new HeroPub(int.Parse(temp[i]), desk, (i + 1));
                                MyModleOperte(newMod, hp);
                        }
                }
        }

        public void SetCameraVague(bool isVague)
        {
                if (camera != null)
                {
                        DepthOfField34 dof = camera.GetComponent<DepthOfField34>();
                        if (dof != null)
                        {
                                if (dof.enabled != isVague)
                                {
                                        dof.enabled = isVague;
                                }
                        }
                }
        }

        GameObject m_SelectCircle;
        public GameObject SelectCircleObj
        {
                get
                {
                        if (m_SelectCircle == null)
                        {
                                m_SelectCircle = EffectManager.GetInst().GetEffectObj("pub_select_effect");
                        }
                        return m_SelectCircle;
                }
        }

        GameObject m_GetEffect;
        public GameObject GetEffectObj
        {
                get
                {
                        if (m_GetEffect == null)
                        {
                                m_GetEffect = EffectManager.GetInst().GetEffectObj("pub_disappear_effect ");
                        }
                        return m_GetEffect;
                }
        }

        void MyModleOperte(GameObject go, object tag)
        {
                GameUtility.ObjPlayAnim(go, CommonString.idle_001Str, true);
                PubHeroBehaviour phb = go.AddComponent<PubHeroBehaviour>();
                HeroPub m_hp = (HeroPub)tag;
                phb.charactar_id = m_hp.charactar_id;
                phb.desk = m_hp.desk;
                phb.index = m_hp.index;
                go.AddComponent<BoxCollider>();
        }

        public void SendPubQuery()
        {
                CSPetMsgPubQuery msg = new CSPetMsgPubQuery();
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

        public void SendHeroGet()
        {
                CSPetMsgPubHire msg = new CSPetMsgPubHire();
                msg.desk = desk;
                msg.idCharacter = charactar_id;
                msg.index = index;
                NetworkManager.GetInst().SendMsgToServer(msg);
                GameUtility.ObjPlayAnim(m_select_hero, CommonString.skill_001Str, true, 0.5f);
                can_choose = false;
                if (phb != null)
                {
                        phb.SetBgActive(false);
                }
        }
}