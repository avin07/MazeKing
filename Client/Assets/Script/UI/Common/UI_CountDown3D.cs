using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;
public class UI_CountDown3D : UIBehaviour  //统一的倒计时脚本//
{
        Canvas mCanvas;
        Text mTextTime;
        Image mValue;
        RectTransform mLoading;
        Text mText;

        public Action<object> onFinish;
        object mData = null;

        float mNeedTime;  //需要倒计时的时间,没有加上Time.realtimeSinceStartup//
        float mTime;      //记录的时间
        float clientIncrementTime = 2.0f;  //和服务器同步的倒计时// 客户端增加一个时间增量，避免客户端计时完毕后服务器还有没。//
        bool bShowText = true;

        enum ProgressType
        {
                FillToEmpty = 0,
                EmptyToFill = 1
        }

        enum Type
        {
                CleanTreasure = 0,
                EditorBrick = 1,
        }

        ProgressType mProgressType = ProgressType.FillToEmpty;
        Type mType;

        void Awake()
        {
                mCanvas = transform.GetComponent<Canvas>();
                mTextTime = FindComponent<Text>("loading/time");
                mValue = FindComponent<Image>("loading/value");
                mText = FindComponent<Text>("loading/Text");
                mLoading = transform.Find("loading") as RectTransform;
        }


        public void SetUp(float needTime, float startTime, Transform root, Action<object> finish, object data, Vector3 offset) //障碍物进度
        {
                transform.SetParent(root);
                transform.localPosition = Vector3.up * 1.5f;
                mNeedTime = needTime + clientIncrementTime;
                mTime = startTime + clientIncrementTime;
                onFinish = finish;
                mData = data;
                bShowText = true;
                mProgressType = ProgressType.FillToEmpty;
                mType = Type.CleanTreasure;
        }


        public void SetUp(float needTime, Vector3 pos,  Action<object> finish) 
        {
                transform.position = pos + Vector3.up;
                mNeedTime = needTime + 0.5f;
                mTime = needTime + Time.realtimeSinceStartup + 0.5f;
                onFinish = finish;
                bShowText = true;
                mData = pos;
                mProgressType = ProgressType.FillToEmpty;
                mType = Type.EditorBrick;
        }

        public void Cancel()
        {
                GameObject.Destroy(gameObject);
        }

        float showTime;
        void Update()
        {
                if (Camera.main == null)
                {
                        return;
                }
                
                showTime = mTime - Time.realtimeSinceStartup;
                if (showTime > 0)
                {
                        if (bShowText)
                        {
                                mTextTime.text = UIUtility.GetTimeString3((int)showTime);
                        }
                        else
                        {
                                mTextTime.text = string.Empty;
                        }
                        if (mProgressType == ProgressType.FillToEmpty)
                        {
                                mValue.fillAmount = showTime / mNeedTime;
                        }
                        else
                        {
                                mValue.fillAmount = 1 - showTime / mNeedTime;
                        }
                }
                else
                {
                        if (onFinish != null)
                        {
                                onFinish(mData);
                        }
                        Cancel();
                }               
        }

}
