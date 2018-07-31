using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class CutSceneCmd_Camera : CutSceneCommand
{
        public string target;
        public float time;
        public Vector3 pos;
        public Vector3 rot;
        public Vector2 shake_range;

        public CutSceneCmd_Camera(XmlNode node)
                : base(node)
        {
                XMLPARSE_METHOD.GetAttrValueStr(node, "target", ref target, "");
                XMLPARSE_METHOD.GetAttrValueFloat(node, "time", ref time, 0f);
                XMLPARSE_METHOD.GetAttrValueVector3(node, "pos", ref pos, Vector3.zero);
                XMLPARSE_METHOD.GetAttrValueVector3(node, "rot", ref rot, Vector3.zero);

                XMLPARSE_METHOD.GetAttrValueVector2(node, "shake_range", ref shake_range, Vector2.zero);
                
        }
        public override void Exec()
        {
                base.Exec();
                switch (this.cmd_name)
                {
                        case "camera_moveto":
                                {
                                        Vector3 newpos = CutSceneManager.GetInst().CutSceneRoom.GetRotatedPos(this.pos);
                                        Vector3 rotOffset = new Vector3(0f, CutSceneManager.GetInst().CutSceneRoom.RotOffset, 0f);
                                        CutSceneCameraManager.GetInst().MoveTo(newpos, this.rot + rotOffset, time, this.isWait);
                                }
                                break;
                        case "camera_follow":
                                {
                                        GameObject unitObj = CutSceneManager.GetInst().GetTargetObj(this.target);
                                        CutSceneCameraManager.GetInst().SetFollow(unitObj);
                                        CutSceneManager.GetInst().EndWaiting();
                                }
                                break;
                        case "camera_restore":
                                {
                                        CutSceneCameraManager.GetInst().RestoreCamera();
                                        CutSceneManager.GetInst().EndWaiting();
                                }
                                break;
                        case "camera_backup":
                                {
                                        CutSceneCameraManager.GetInst().BackupCamera();
                                        CutSceneManager.GetInst().EndWaiting();
                                }
                                break;
                        case "camera_shake":
                                {
                                        CutSceneCameraManager.GetInst().ShakeCamera(shake_range, time, isWait);
                                }
                                break;
                }
        }
}
