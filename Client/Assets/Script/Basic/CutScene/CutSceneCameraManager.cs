using UnityEngine;
using System.Collections;

class CutSceneCameraManager : SingletonObject<CutSceneCameraManager>
{
        bool m_bFollow;
        GameObject m_TargetObj;
        
        float m_fMoveTime = 0f;
        float m_fTime = 0f;
        bool m_bWait = false;

        enum CAMERA_STATE
        {
                NONE,
                MOVE,
                FOLLOW,
                FOCUS,
                SHAKE,
        };

        Quaternion m_BakCameraRot;
        Vector3 m_BakCameraPos;
        CAMERA_STATE m_State = CAMERA_STATE.NONE;

        public void SetFollow(GameObject targetObj)
        {
                if (targetObj != null)
                {
                        m_State = CAMERA_STATE.FOLLOW;
                }
                else
                {
                        m_State = CAMERA_STATE.NONE;
                }
                m_TargetObj = targetObj;
        }

        public void MoveTo(Vector3 targetPos, Vector3 targetRot, float time, bool bWait)
        {
                m_bWait = bWait;
                m_State = CAMERA_STATE.MOVE;
                m_fTime = Time.realtimeSinceStartup;
                m_fMoveTime = time;
                iTween.moveToWorld(Camera.main.gameObject, time, 0f, targetPos);
                iTween.RotateToWorld(Camera.main.gameObject, time, 0f, targetRot);
        }

        public void BackupCamera()
        {
                iTween.stop(Camera.main.gameObject);
                m_BakCameraRot = Camera.main.transform.rotation;
                m_BakCameraPos = Camera.main.transform.position;
                                
                //Debuger.Log(m_BakCameraPos + " " + m_BakCameraRot);
        }
        public void RestoreCamera()
        {
                Debuger.Log(m_BakCameraPos + " " + m_BakCameraRot);
                m_State = CAMERA_STATE.NONE;
                Camera.main.transform.rotation = m_BakCameraRot;
                Camera.main.transform.position = m_BakCameraPos;
                iTween.stop(Camera.main.gameObject);
                if (m_bWait)
                {
                        CutSceneManager.GetInst().EndWaiting();
                }
        }
        public void ShakeCamera(Vector2 range, float time, bool bWait)
        {
                CameraShake cs = Camera.main.gameObject.GetComponent<CameraShake>();
                if (cs == null)
                {
                        cs = Camera.main.gameObject.AddComponent<CameraShake>();
                }
                cs.range = range;
                cs.MaxTime = time;
                if (bWait)
                {
                        m_State = CAMERA_STATE.SHAKE;
                        CutSceneManager.GetInst().SetWaitTime(time);
                }
        }

        public void Update()
        {
                switch (m_State)
                {
                        case CAMERA_STATE.NONE:
                                break;
                        case CAMERA_STATE.SHAKE:
                                {
                                        if (Camera.main.gameObject.GetComponent<CameraShake>() == null)
                                        {
                                                m_State = CAMERA_STATE.NONE;
                                                if (m_bWait)
                                                {
                                                        CutSceneManager.GetInst().EndWaiting();
                                                }
                                        }
                                }
                                break;
                        case CAMERA_STATE.MOVE:
                                {
                                        if (Time.realtimeSinceStartup - m_fTime > m_fMoveTime)
                                        {
                                                m_State = CAMERA_STATE.NONE;

                                                if (m_bWait)
                                                {
                                                        CutSceneManager.GetInst().EndWaiting();
                                                }
                                        }
                                }
                                break;
                        case CAMERA_STATE.FOLLOW:
                                {
                                        if (m_TargetObj != null)
                                        {
                                                Vector3 newpos = m_TargetObj.transform.position - Camera.main.transform.forward * RaidManager.GetInst().m_fCameraDist;
                                                Camera.main.transform.position = newpos;
                                        }
                                }
                                break;
                }
        }
}
