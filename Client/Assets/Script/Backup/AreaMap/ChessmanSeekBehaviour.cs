//using UnityEngine;
//using System.Collections;

//public class ChessmanSeekBehaviour : MonoBehaviour 
//{
    
//        int m_now_destination;  //现在的目的地//
//        public int NowDestination
//        {
//            get
//            {
//                return m_now_destination;
//            }
//            set
//            {
//                m_now_destination = value;
//            }
//        }

//        int m_next_destination;  //目的地//
//        bool isGO = false;       //是否在运动//


//        void Start()
//        {
//            if (Camera.main != null)
//            {
//                Camera.main.transform.position = SetChessmanOnCentre();
//                GetComponent<NavMeshAgent>().enabled = true;
//            }
//        }

        
//        public void Go(Transform DestinationTrans, int next_destination) //要去的目的地
//        {
//            if (DestinationTrans != null)
//            {

//                Destroy(GameObject.Find("Road"));  //清除前面的路径//
//                GameObject Road = new GameObject();
//                Road.name = "Road";

//                m_next_destination = next_destination;
//                NavMeshPath path = new NavMeshPath();
//                if (GetComponent<NavMeshAgent>().CalculatePath(DestinationTrans.position, path))
//                {

//                    DrawRoad(path.corners,Road);

//                    bool isChessmanInCarmer = new Rect(0.1f, 0.1f, 0.8f, 0.8f).Contains(Camera.main.WorldToViewportPoint(transform.position));  //是否在视口内

//                    Vector3 camera_init_position = Camera.main.transform.position;

//                    StartCoroutine(Chessman_GO(camera_init_position, SetChessmanOnCentre(), DestinationTrans, isChessmanInCarmer));

//                }
//                else
//                {
//                    UIManager.GetInst().ShowUI<UI_MessageBox>("UI_MessageBox").SetText("无法到达指定点！");
//                }

//            }
//        }

//        readonly float spacing = 2.5f; 
//        protected void DrawRoad(Vector3[] corners,GameObject road)
//        {
//            for (int i = 0; i < corners.Length; i++)
//            {
//                if (i + 1 < corners.Length )
//                {
//                    float distance = Vector3.Distance(corners[i], corners[i + 1]);   //两米为一段//
//                    int num = Mathf.FloorToInt(distance / spacing);
//                    for (int j = 0; j < num; j++)  
//                    {
//                        Vector3 small_point = Vector3.Lerp(corners[i], corners[i + 1], 1.0f / (float)num * j);
//                        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//                        obj.AddComponent<RoadPointBehaviour>();
//                        obj.GetComponent<SphereCollider>().isTrigger = true;
//                        obj.transform.position = small_point;
//                        obj.name = small_point.ToString();
//                        obj.transform.SetParent(road.transform);
//                    }
//                }
//            }
//            GameObject end = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//            end.AddComponent<RoadPointBehaviour>();
//            end.GetComponent<SphereCollider>().isTrigger = true;
//            MeshRenderer renderer = end.GetComponent<MeshRenderer>();
//            renderer.material = (Material)Resources.Load("Material/qian");
//            end.transform.position = corners[corners.Length - 1];
//            end.name = corners[corners.Length - 1].ToString();
//            end.transform.SetParent(road.transform);
//        }

//        bool isCameraMove = false;
     
//        IEnumerator Chessman_GO(Vector3 camera_init_position, Vector3 camera_now_position, Transform Destination, bool isChessmanInCarmer) //棋子运动
//        {
//            if (!isChessmanInCarmer)   //快速移动相机视角定位人物到中心//
//            {
//                float timeStart = Time.realtimeSinceStartup;
//                while ((Time.realtimeSinceStartup - timeStart) < 0.5f)
//                {
//                    Camera.main.transform.position = Vector3.Lerp(camera_init_position, camera_now_position, (Time.realtimeSinceStartup - timeStart) / 0.5f);
//                    yield return null;
//                }
//                isCameraMove = true;
//            }

//            GetComponent<NavMeshAgent>().SetDestination(Destination.position);
//            isGO = true;
//        }

//        IEnumerator Chessman_Arrived(Vector3 camera_init_position, Vector3 camera_now_position, bool isChessmanInCarmer)   //棋子到达
//        {
//            if(!isChessmanInCarmer)
//            {
//                float timeStart = Time.realtimeSinceStartup;
//                while ((Time.realtimeSinceStartup - timeStart) < 0.5f)
//                {
//                    Camera.main.transform.position = Vector3.Lerp(camera_init_position, camera_now_position, (Time.realtimeSinceStartup - timeStart) / 0.5f);
//                    yield return null;
//                }
//            }

//            AreaMapManager.GetInst().SetCurrDistrictId(NowDestination);
//            AreaMapManager.GetInst().ClickBuilding(NowDestination, gameObject.transform.position);
           
//        }


//        void Update()
//        {
//            if (isGO)
//            {
//                if (!GetComponent<NavMeshAgent>().hasPath)
//                {
//                    NowDestination = m_next_destination;
//                    bool isChessmanInCarmer = new Rect(0.1f, 0.1f, 0.8f, 0.8f).Contains(Camera.main.WorldToViewportPoint(transform.position));  //是否在视口内(优化了视口范围)
//                    Vector3 camera_init_position = Camera.main.transform.position;
//                    StartCoroutine(Chessman_Arrived(camera_init_position, SetChessmanOnCentre(),isChessmanInCarmer));
//                    Destroy(GameObject.Find("Road"));  //清除前面的路径//
//                    isGO = false;
//                }

//            }

//            if (isCameraMove)  //这个时候相机要跟着人走//
//            {
//                Camera.main.transform.position = SetChessmanOnCentre();
//                if (!GetComponent<NavMeshAgent>().hasPath)
//                {
//                    isCameraMove = false;
//                }
//                if (InputManager.GetInst().GetInputDown(false))
//                {
//                    isCameraMove = false;
//                }
//            }
//        }


//        Vector3 SetChessmanOnCentre()  //切换镜头对准人物为屏幕中心
//        {
//            float ratio = (transform.position.y - Camera.main.transform.position.y) / Camera.main.transform.forward.y;  //固定y坐标不变//
//            Vector3 trans = transform.position - ratio * Camera.main.transform.forward;                   //计算出固定y坐标下摄像机的x,y坐标。
//            float x = Mathf.Clamp(trans.x, AreaMapManager.GetInst().CAMERA_BOUND_LEFT, AreaMapManager.GetInst().CAMERA_BOUND_RIGHT);
//            float z = Mathf.Clamp(trans.z, AreaMapManager.GetInst().CAMERA_BOUND_BOTTOM, AreaMapManager.GetInst().CAMERA_BOUND_TOP);
//            return new Vector3(x, Camera.main.transform.position.y, z);

//        }

         


//}