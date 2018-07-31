using UnityEngine;
using System.Collections;

public class UnitAnim : MonoBehaviour 
{
        int m_nModelId;
        public int Model_Id
        {
                get
                {
                        return m_nModelId;
                }
                set
                {
                        m_nModelId = value;
                }
        }

        public string GetAnimName(int actionId)
        {
                return ModelResourceManager.GetInst().GetActionId(Model_Id, actionId);
        }

        public float GetHitTime(int actionId, int index)
        {
                string str = ModelResourceManager.GetInst().GetActionHitTime(Model_Id, actionId);
                if (!string.IsNullOrEmpty(str))
                {
                        string[] tmps = str.Split(',');
                        if (tmps.Length > index && index >= 0)
                        {
                                return float.Parse(tmps[index]) / 1000f;
                        }
                }
                return 0f;
        }

        #region Anim

        string m_sCurrAnim = "";
        public string CurrentAnim
        {
                get
                {
                        return m_sCurrAnim;
                }
                set
                {
                        m_sCurrAnim = value;
                }
        }
        string m_sNextAnim = "";
        public string NextAnim
        {
                get
                {
                        return m_sNextAnim;
                }
        }
        Animation m_AnimCtr;
        public Animation AnimCtr
        {
                get
                {
                        if (m_AnimCtr == null)
                        {
                                m_AnimCtr = GetComponentInChildren<Animation>();
                        }
                        return m_AnimCtr;
                }
        }

        public void ResetAnimCtr()
        {
                m_AnimCtr = GetComponentInChildren<Animation>();
        }

        public float GetAnimTime(int action_id)
        {
                return GetAnimTime(GetAnimName(action_id));                
        }

        public float GetAnimTime(string animName)
        {
                if (AnimCtr != null)
                {
                        if (AnimCtr.GetClip(animName) != null)
                        {
                                return AnimCtr[animName].length;
                        }
                }
                return 0f;
        }

        public void StopAnim()
        {
                AnimCtr.Stop();
        }
        public void PlayAnim(string animName, bool bLoop)
        {              
                PlayAnim(animName, bLoop, "");
        }

        public void CrossFade(string animName)
        {
                if (AnimCtr != null)
                {
                        if (AnimCtr.GetClip(animName) != null)
                        {
                                if (AnimCtr.IsPlaying(animName) == false)
                                {
                                        AnimCtr.CrossFade(animName);
                                }
                        }
                }
        }

        public void PlayAnim(int action_id, bool bLoop, string nextAnim, float speed = 1f)
        {
                PlayAnim(GetAnimName(action_id), false, nextAnim);
        }

        public void PlayAnim(string animName, bool bLoop, string nextAnim, float speed = 1f, bool bNextLoop = true)
        {
                //Debug.Log(this.transform.name + " PlayAnim " + animName + " " + Time.realtimeSinceStartup);
                if (AnimCtr != null)
                {
                        if (AnimCtr.GetClip(animName) != null)
                        {
                                if (AnimCtr.IsPlaying(animName) == false /*|| m_sCurrAnim != animName*/)
                                {
                                        AnimCtr[animName].speed = speed;
                                        AnimCtr[animName].wrapMode = bLoop ? WrapMode.Loop : WrapMode.Default;
                                        AnimCtr.CrossFade(animName);
                                }
                        }

                        if (!string.IsNullOrEmpty(nextAnim))
                        {
                                if (AnimCtr.GetClip(nextAnim) != null)
                                {
                                        AnimCtr[nextAnim].wrapMode = bNextLoop ? WrapMode.Loop : WrapMode.Default;
                                        AnimCtr.CrossFadeQueued(nextAnim);
                                }
                        }
                }
                m_sCurrAnim = animName;
                m_sNextAnim = nextAnim;
        }
        public void Pause(string name, float time)
        {
                //Debug.Log(this.transform.name + " Pause " + name + " " + Time.realtimeSinceStartup);
                StartCoroutine(ProcessPauseAnim(name, time));
        }
        IEnumerator ProcessPauseAnim(string name, float time)
        {
                if (AnimCtr.IsPlaying(name))
                {
                        AnimCtr[name].speed = 0f;
                        yield return new WaitForSeconds(time);
                        AnimCtr[name].speed = 1f;
                }
        }
        #endregion

}
