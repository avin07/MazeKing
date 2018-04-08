using UnityEngine;
using Message;
using UnityEngine.EventSystems;


public class BuildProduceBehaviour : BuildBaseBehaviour  //生产类建筑
{

        protected override void UpdateStateBar()
        {
                base.UpdateStateBar();
                UpdateOutput();
        }

        public override void OnBuildClick(GameObject go, PointerEventData data)
        {
                if (HomeManager.GetInst().GetState() == HomeState.None)
                {
                        if (isShowResTip) //在有资源收获提示的时候优先响应收获//
                        {
                                HomeManager.GetInst().SendResourceGain(this);
                                HideResTip();
                                return;
                        }
                }

                base.OnBuildClick(go, data);
        }

        public override void UpdateBuildInfo(BuildInfo new_bi)
        {
                base.UpdateBuildInfo(new_bi);
                CheckOutput();
        }

        bool NeedOutputRefresh = false;  //是否需要对产出进行计算//
        float WorkTime;   //生产时间
        float ShowResTipValue;
        float OutputOnceTime;
        int OutputOnce;
        public int ResType;
        public bool isShowResTip = false;

        public void CheckOutput()
        {
                if (mBuildInfo.level > 0)
                {
                        int id = mBuildInfo.buildCfg.id;
                        BuildProduceHold m_BuildProduceCfg = HomeManager.GetInst().GetBuildingProduceCfg(id);
                        if (m_BuildProduceCfg != null)
                        {
                                string[] output_detail = m_BuildProduceCfg.output.Split(',');
                                ResType = int.Parse(output_detail[1]);
                                OutputOnce = int.Parse(output_detail[2]);
                                OutputOnceTime = m_BuildProduceCfg.output_time;
                                ShowResTipValue = m_BuildProduceCfg.reserve * 0.2f;
                                NeedOutputRefresh = true;
                        }       
                }    
        }

        void UpdateOutput()
        {
                if (NeedOutputRefresh)
                {
                        float time = HomeManager.GetInst().GetOutPutTime(mBuildInfo.id);
                        WorkTime = Time.realtimeSinceStartup - time;
                        if (GetNowOutPut() >= ShowResTipValue)
                        {
                                NeedOutputRefresh = false;
                                ShowResTip();
                        } 
                }
        }

        public int GetNowOutPut()
        {
                return (int)(WorkTime / OutputOnceTime) * OutputOnce * (1 + PlayerController.GetInst().GetPropertyInt("produce_augmentation_per"));
        }

        public void ShowResAdd(int value)
        {
                BuildStateBar.ShowResAdd(value);
        }

        void ShowResTip()
        {
                BuildStateBar.ShowResTip(true, ResType);
                isShowResTip = true;
        }

        void HideResTip()
        {
                if (mBuildStateBar != null)
                {
                        BuildStateBar.ShowResTip(false, 0);
                }
                isShowResTip = false;
        }
}