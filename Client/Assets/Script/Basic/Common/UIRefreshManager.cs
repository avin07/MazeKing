using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//刷新界面优化,物品掉落特效
class UIRefreshManager : SingletonObject<UIRefreshManager>
{

        #region UI界面刷新
        private class RefreshWndInfo
        {
                public UIBehaviour ub;
                public object data;
                public float waittime;

                public RefreshWndInfo(UIBehaviour _wnd, object _data, float _waittime)
                {
                        ub = _wnd;
                        data = _data;
                        waittime = _waittime;
                }
        }

        private Dictionary<UIBehaviour, RefreshWndInfo> mRefreshWndDic = new Dictionary<UIBehaviour, RefreshWndInfo>();
        private List<UIBehaviour> mRemoveList = new List<UIBehaviour>();
        private static float MAX_WAIT_REFRESH_TIME = 0.1f;

        void WndRefreshUpdate()
        {
                var RefreshWndDic_Enumerator = mRefreshWndDic.GetEnumerator();
                try
                {
                        while (RefreshWndDic_Enumerator.MoveNext())
                        {
                                if (Time.realtimeSinceStartup - RefreshWndDic_Enumerator.Current.Value.waittime >= MAX_WAIT_REFRESH_TIME)
                                {
                                        mRemoveList.Add(RefreshWndDic_Enumerator.Current.Key);
                                }
                        }
                }
                finally
                {
                        RefreshWndDic_Enumerator.Dispose();
                }


                if (mRemoveList.Count > 0)
                {
                        for (int i = 0; i < mRemoveList.Count; i++)
                        {
                                UIBehaviour ub = mRemoveList[i];
                                if (ub != null && mRefreshWndDic.ContainsKey(ub))
                                {
                                        ub.RefreshCurWnd(mRefreshWndDic[ub].data);
                                }
                                else
                                {
                                        mRefreshWndDic.Remove(ub);
                                }
                                mRefreshWndDic.Remove(ub);
                        }
                        mRemoveList.Clear();
                }
        }

        public void AddRefreshWnd(UIBehaviour ub, object data)
        {
                //Debuger.LogError("AddRefreshWnd = " + ub.name);
                if (ub != null)
                {
                        if (mRefreshWndDic.ContainsKey(ub))
                        {
                                mRefreshWndDic[ub].data = data;
                        }
                        else
                        {
                                mRefreshWndDic.Add(ub, new RefreshWndInfo(ub, data, Time.realtimeSinceStartup));
                        }
                }
        }

        #endregion

        #region 物品掉落

        Queue<string> m_ItemGetQueue = new Queue<string>();
       
        public void AddDropItem(string info,string name, int num)
        {
                if (GameStateManager.GetInst().GameState == GAMESTATE.HOME)
                {
                        m_ItemGetQueue.Enqueue(info);
                        GameUtility.PopupMessage("获得" + name + "X" + num);
                }
        }

        float DROP_INTERVAL_TIME = 0.2f;
        float last_drop_time = 0;
        string drop_url;

        void DropItemUpdate()
        {
                if (m_ItemGetQueue.Count > 0 && Time.realtimeSinceStartup - last_drop_time >= DROP_INTERVAL_TIME)
                {
                        if (GameStateManager.GetInst().GameState == GAMESTATE.HOME)
                        {
                                drop_url = m_ItemGetQueue.Dequeue();
                                UIManager.GetInst().ShowUI<UI_DropItem>("UI_DropItem").StartItemDropAni(drop_url);
                                last_drop_time = Time.realtimeSinceStartup;
                        }
                }
        }

        public void DropListReset()
        {
                m_ItemGetQueue.Clear();
        }

        #endregion

        public void OnUpdate()
        {
                WndRefreshUpdate();
                DropItemUpdate();
        }

}
