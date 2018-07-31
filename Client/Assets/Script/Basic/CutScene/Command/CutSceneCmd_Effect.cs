using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class CutSceneCmd_Effect : CutSceneCommand
{
        public string effect_name;
        public string target;
        public float time;      //<=0表示一次播放，用特效本身的持续时间。> 0表示特效是循环播放的，时间到了销毁
        public string bind;     //绑定在target上的骨骼名。如果为空则不绑定，只在target所在位置播放
        public Vector3 pos;
        public int rotY;
        public string effect_id;

        public CutSceneCmd_Effect(XmlNode node)
                : base(node)
        {
                XMLPARSE_METHOD.GetAttrValueStr(node, "id", ref effect_id, "");
                XMLPARSE_METHOD.GetAttrValueStr(node, "target", ref target, "");
                XMLPARSE_METHOD.GetAttrValueStr(node, "name", ref effect_name, "");
                XMLPARSE_METHOD.GetAttrValueStr(node, "bind", ref bind, "");
                XMLPARSE_METHOD.GetAttrValueFloat(node, "time", ref time, 0f);
                XMLPARSE_METHOD.GetAttrValueInt(node, "rot", ref rotY, 0);
                
                XMLPARSE_METHOD.GetAttrValueVector3(node, "pos", ref pos, Vector3.zero);
        }
        public override void Exec()
        {
                base.Exec();
                switch (cmd_name)
                {
                        case "effect_play":
                                {
                                        PlayEffect();
                                }
                                break;
                        case "effect_stop":
                                {
                                        CutSceneManager.GetInst().StopEffect(effect_id);
                                        if (isWait)
                                        {
                                                CutSceneManager.GetInst().EndWaiting();
                                        }
                                }
                                break;
                }
        }
        void PlayEffect()
        {
                GameObject effectRoot = new GameObject();
                if (!string.IsNullOrEmpty(effect_id))
                {
                        effectRoot.name = effect_id;
                        CutSceneManager.GetInst().AddEffect(effect_id, effectRoot);
                }
                GameObject effectObj = EffectManager.GetInst().GetEffectObj(effect_name);
                if (effectObj != null)
                {
                    effectObj.transform.SetParent(effectRoot.transform);
                    effectObj.transform.localPosition = Vector3.zero;
                    effectObj.transform.localRotation = Quaternion.identity;
                }
                
                if (time > 0)
                {
                        GameObject.Destroy(effectRoot, time);
                        if (isWait)
                        {
                                CutSceneManager.GetInst().SetWaitTime(time);
                        }
                }
                else
                {
                        TimedObjectDestructor tod = effectObj.GetComponentInChildren<TimedObjectDestructor>();
                        time = tod.timeOut;
                        GameObject.Destroy(effectRoot, time);
                        if (isWait)
                        {
                                CutSceneManager.GetInst().SetWaitTime(time);
                        }
                }

                GameObject unitObj = CutSceneManager.GetInst().GetTargetObj(target);
                if (unitObj != null)
                {
                        if (bind != "")
                        {
                                Transform trans = GameUtility.GetTransform(unitObj, bind);
                                effectRoot.transform.SetParent(trans);
                                effectRoot.transform.localPosition = Vector3.zero;
                                effectRoot.transform.localRotation = Quaternion.identity;
                        }
                        else
                        {
                                effectRoot.transform.SetParent(unitObj.transform);
                                effectRoot.transform.position = unitObj.transform.position;
                                effectRoot.transform.rotation = unitObj.transform.rotation;
                        }
                }
                else
                {
                        
                        effectRoot.transform.position = CutSceneManager.GetInst().CutSceneRoom.GetRotatedPos(pos);
                        effectRoot.transform.rotation = Quaternion.Euler(new Vector3(0f, rotY + CutSceneManager.GetInst().CutSceneRoom.RotOffset, 0f));
                }
                //Debuger.Log(effectRoot.transform.position);
        }
}
