using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class CutSceneCmd_Unit : CutSceneCommand
{
        public string target;
        public string anim;
        public string nextanim;
        public float time;
        public int model_id;
        public int rot;
        public float distance;

        public Vector3 target_pos;
        public string target_posname;
        public string rotate_to;
        public bool bLoop;

        public CutSceneCmd_Unit(XmlNode node)
                : base(node)
        {
                XMLPARSE_METHOD.GetAttrValueStr(node, "target", ref target, "");
                XMLPARSE_METHOD.GetAttrValueStr(node, "anim", ref anim, "");
                XMLPARSE_METHOD.GetAttrValueStr(node, "nextanim", ref nextanim, "");

                XMLPARSE_METHOD.GetAttrValueStr(node, "posname", ref target_posname, "");
                
                XMLPARSE_METHOD.GetAttrValueVector3(node, "pos", ref target_pos, Vector3.zero);
                XMLPARSE_METHOD.GetAttrValueFloat(node, "time", ref time, 0f);
                XMLPARSE_METHOD.GetAttrValueFloat(node, "distance", ref distance, 0f);
                XMLPARSE_METHOD.GetAttrValueInt(node, "rot", ref rot, 0);

                XMLPARSE_METHOD.GetAttrValueStr(node, "rotate_to", ref rotate_to, "");
                
                XMLPARSE_METHOD.GetAttrValueInt(node, "model_id", ref model_id, 0);
                XMLPARSE_METHOD.GetAttrValueBool(node, "loop", ref bLoop, true);
        }

        Vector3 GetMoveTargetPos()
        {
                return CutSceneManager.GetInst().CutSceneRoom.GetRotatedPos(target_pos);
        }

        GameObject unitObj;
        void MoveTo()
        {
                if (unitObj != null)
                {
                        Vector3 tmpPos = GetMoveTargetPos();
                        iTween.moveToWorld(unitObj, time, 0f, tmpPos, iTween.EasingType.linear);
                        PlayAnim();
                }
        }
        void MoveForward()
        {
                if (unitObj != null)
                {
                        Vector3 tmpPos = unitObj.transform.position + unitObj.transform.forward * distance;
                        iTween.moveToWorld(unitObj, time, 0f, tmpPos, iTween.EasingType.linear);
                        PlayAnim();
                }
        }

        void CreateUnit()
        {
                if (unitObj == null)
                {
                        unitObj = ModelResourceManager.GetInst().GenerateCommonObject(model_id);
                        if (unitObj != null)
                        {
                                unitObj.transform.position = CutSceneManager.GetInst().CutSceneRoom.GetRotatedPos(target_pos);

                                Quaternion qua = Quaternion.identity;
                                qua.eulerAngles = new Vector3(qua.eulerAngles.x, rot + CutSceneManager.GetInst().CutSceneRoom.RotOffset, qua.eulerAngles.z);
                                unitObj.transform.localRotation = qua;
                                CutSceneManager.GetInst().AddUnit(target, model_id, unitObj);

                                PlayAnim();
                        }
                }
        }

        void DeleteUnit()
        {
                CutSceneManager.GetInst().DeleteUnit(target);
        }

        void RotateUnit()
        {
                if (unitObj != null)
                {
                        if (!string.IsNullOrEmpty(rotate_to))
                        {
                                GameObject roteteToObj = CutSceneManager.GetInst().GetTargetObj(rotate_to);
                                Vector3 dir = roteteToObj.transform.position - unitObj.transform.position;
                                Quaternion rotation = Quaternion.LookRotation(dir);
                                iTween.RotateToWorld(unitObj, time, 0f, new Vector3(unitObj.transform.rotation.eulerAngles.x, rotation.eulerAngles.y, unitObj.transform.rotation.eulerAngles.z));
                        }
                        else
                        {
                                iTween.RotateToWorld(unitObj, time, 0f, new Vector3(unitObj.transform.rotation.eulerAngles.x, unitObj.transform.rotation.eulerAngles.y + rot, unitObj.transform.rotation.eulerAngles.z));
                        }

                        
                        PlayAnim();
                }
        }


        void ShowUnit()
        {
                if (unitObj != null)
                {
                        RaidNodeBehav node = unitObj.GetComponentInParent<RaidNodeBehav>();
                        if (node != null)
                        {
                                //node.IsStoryHide = false;
                                unitObj.SetActive(true);
                                PlayAnim();
                        }
                }
        }
        void PlayAnim()
        {
                if (anim == "")
                        return;

                if (unitObj != null)
                {
                        if (time > 0f)
                        {
                                GameUtility.ObjPlayAnim(unitObj, anim, bLoop);
                                if (bLoop)
                                {
                                        AppMain.GetInst().StartCoroutine(WaitAnimEnd(time));
                                }
                                else
                                {
                                        AppMain.GetInst().StartCoroutine(WaitAnimEnd(GameUtility.GetAnimTime(unitObj, anim)));
                                }
                        }
                        else
                        {
                                GameUtility.ObjPlayAnim(unitObj, anim, bLoop);
                                if (bLoop)
                                {
                                }
                                else
                                {
                                        time = GameUtility.GetAnimTime(unitObj, anim);
                                        AppMain.GetInst().StartCoroutine(WaitAnimEnd(time));
                                }
                        }

                        //Debuger.LogError("PlayAnim " + anim + " " + nextanim + " " + time);
                }
        }

        IEnumerator WaitAnimEnd(float time)
        {
                //Debuger.Log(time);
                yield return new WaitForSeconds(time);

                if (unitObj != null)
                {
                        GameUtility.ObjPlayAnim(unitObj, nextanim, true);
                }
        }


        public override void Exec()
        {
                base.Exec();
                unitObj = CutSceneManager.GetInst().GetTargetObj(this.target);


                switch (this.cmd_name)
                {
                        case "unit_create":
                                {
                                        CreateUnit();
                                }
                                break;
                        case "unit_delete":
                                {
                                        DeleteUnit();
                                }
                                break;
                        case "unit_show":
                                {
                                        //ShowUnit();
                                }
                                break;
                        case "unit_rotate":
                                {
                                        RotateUnit();
                                }
                                break;
                        case "unit_moveto":
                                {
                                        MoveTo();
                                }
                                break;
                        case "unit_forward":
                                {
                                        MoveForward();
                                }
                                break;
                        case "unit_play":
                                {
                                        PlayAnim();
                                }
                                break;
                }

                if (this.isWait)
                {
                        CutSceneManager.GetInst().SetWaitTime(time);
                }
                else
                {
                        CutSceneManager.GetInst().EndWaiting();
                }
        }
}