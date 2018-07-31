using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using DG.Tweening;
public class UI_DropItem : UIBehaviour
{       

        void Awake()
        {
                this.UILevel = UI_LEVEL.MAIN;
                Transform tf = transform.Find("pos");
                pos_obj = new Transform[tf.childCount];
                for (int i = 0; i < tf.childCount; i++)
                {
                        pos_obj[i] = tf.GetChild(i);
                }        
        }
     

        Transform []pos_obj;
        public void StartItemDropAni(string url)
        {
                GameObject item = CloneElement(pos_obj[0].gameObject);
                Transform tf = item.transform;
                ResourceManager.GetInst().LoadIconSpriteSyn(url, tf);
                DOTween.Sequence()
                        .Append(tf.DOMove(pos_obj[1].position, 0.3f).SetEase(Ease.Linear))
                        .Join(tf.DOScale(Vector3.one * 1.2f, 0.3f))
                        .AppendInterval(0.2f)
                        .Append(tf.DOMove(pos_obj[2].position, 0.6f).SetEase(Ease.Linear))
                        .Join(tf.DOScale(Vector3.one * 0.8f, 0.6f).OnComplete(() => FinishItemAnimation(item)));
        }

        void FinishItemAnimation(GameObject item)
        {
                GameObject.Destroy(item);

        }

        public override void OnClose(float time)
        {
                UIRefreshManager.GetInst().DropListReset();
                base.OnClose(time);
        }

}
