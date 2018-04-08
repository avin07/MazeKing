using UnityEngine;
using UnityEngine.EventSystems;


public class BuildBoardBehaviour : BuildBaseBehaviour  //公告板建筑
{
        public override void SetBuild(BuildInfo build_info)
        {
                base.SetBuild(build_info);
                ShowBoardBar();
        }


        public void ShowBoardBar()
        {
                //if (PlayerController.GetInst().GetPropertyInt("house_level") < GlobalParams.GetInt("task_pool_open_level"))
                //{
                //        BuildStateBar.ShowInfoTip(true, "Npc#suo");
                //}
                //else
                //{
                        if (TaskManager.GetInst().CheckHasBoardTaskFinish())
                        {
                                BuildStateBar.ShowInfoTip(true, "Npc#wenhao");
                        }
                        else
                        {
                                if (TaskManager.GetInst().GetBoardTaskDic().Count > 0)
                                {
                                        BuildStateBar.ShowInfoTip(true, "Npc#tanhao",string.Empty,true);
                                }
                                else
                                {
                                        BuildStateBar.ShowInfoTip(false);
                                }
                        }
                //}
        }

}