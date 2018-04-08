//using UnityEngine;
//using System.Collections;

//public class AreaMapEventPointBehaviour : MonoBehaviour 
//{

//        public int Id;


//        void OnMouseUpAsButton()
//        {
//            if (UIManager.GetInst().HasNormalUIOpen())
//            {
//                return;
//            }
//            if (InputManager.GetInst().isClickInuGUI())
//            {
//                return;
//            }
//            Debuger.Log("Click " + this.gameObject.name);

//            GameObject Chessman = AreaMapManager.GetInst().GetMyChessman();
//            if (Chessman != null)
//            {
//                if (Chessman.GetComponent<ChessmanSeekBehaviour>().NowDestination == Id)
//                {
//                    //点击的是同一个点//
//                    if (Chessman.GetComponent<NavMeshAgent>().hasPath)   //寻路过程中点击原来的点弹出菜单的bug//
//                    {
//                        Transform position = transform.FindChild("Position");
//                        Chessman.GetComponent<ChessmanSeekBehaviour>().Go(position, Id);
//                    }
//                    else
//                    {
//                        AreaMapManager.GetInst().ClickBuilding(Id, Chessman.transform.position);
//                    }
//                    return;

//                }
//                if (AreaMapManager.GetInst().IsPointCanGo(Id))
//                {
//                    Transform position = transform.FindChild("Position");
//                    if (position == null)
//                    {
//                        singleton.GetInst().ShowMessage(ErrorOwner.artist, "建筑没有布初始点！");
//                    }
//                    else
//                    {
//                        if (UIManager.GetInst().IsUIVisible("UI_AreaMapMenu"))
//                        {
//                            UIManager.GetInst().CloseUI("UI_AreaMapMenu");
//                        }
//                        Chessman.GetComponent<ChessmanSeekBehaviour>().Go(position,Id);
//                    }
//                }
//                else
//                {
//                    UIManager.GetInst().ShowUI<UI_MessageBox>("UI_MessageBox").SetText("迷雾笼罩的点无法到达！");
//                }
//            }
//            //float test = (transform.position.y - Camera.main.transform.position.y) / Camera.main.transform.forward.y;
//            //Debuger.Log(test);
//            //Camera.main.transform.position = transform.position - test * Camera.main.transform.forward;
//        }

//}
