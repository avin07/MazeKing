using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;
public class UI_CountDown2D : UIBehaviour  //统一的倒计时脚本//
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
                LongPressForEditorBrick = 1,
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


        Vector3 mPos;
        public void SetUp(float needTime)  //放砖快捷操作进度
        {
                //mPos = Input.mousePosition;
                mNeedTime = needTime;
                mTime = needTime + Time.realtimeSinceStartup;
                bShowText = false;
                mProgressType = ProgressType.EmptyToFill;
                mType = Type.LongPressForEditorBrick;
                mText.text = string.Empty;
        }


        public void Cancel()
        {
                GameObject.Destroy(gameObject);
        }

        Vector3 pos;
        float showTime;
        void Update()
        {
                if (Camera.main == null)
                {
                        return;
                }

                if (mType == Type.LongPressForEditorBrick)
                {
                        RectTransformUtility.ScreenPointToWorldPointInRectangle(mCanvas.transform as RectTransform, Input.mousePosition, mCanvas.worldCamera, out pos);
                        mLoading.position = pos;
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
