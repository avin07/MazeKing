using UnityEngine;
using Message;


public class BuildCureBehaviour : BuildBaseBehaviour  //治疗类
{
        protected override void UpdateStateBar()
        {
                base.UpdateStateBar();
                if (bCure)
                {
                        ShowCureTip();
                }
        }

        bool bCure = false;
        public void ShowCureTip()
        {
                if (bCure)
                {
                        restTime = CureRestTime - Time.realtimeSinceStartup;
                        SetBuildingState((int)restTime, (int)MaxTime);
                     
                        if (restTime <= 0)
                        {
                                bCure = false;
                                restTime = 0;
                                HideBuildBar();
                        }
                }
        }

        public float CureRestTime;
        public float MaxTime;

        public void SetCureTime(float restTime)
        {
                if (restTime > 0)
                {
                        CureRestTime = restTime;
                        MaxTime = CureRestTime - Time.realtimeSinceStartup;
                        bCure = true;
                }
        }

}