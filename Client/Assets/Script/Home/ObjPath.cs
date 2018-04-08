using UnityEngine;
using System.Collections;
using Pathfinding;
using System.Collections.Generic;
using DG.Tweening;

[RequireComponent(typeof(Seeker))]
public class ObjPath : MonoBehaviour 
{
        float maxWaitTime;
        float minWaitTime;

        Vector2 nowPos;  //根据当前位置来查询下一个可以到达的点//
        Vector3 nextPos;
        Animation anim;
        float speed;
        ObjType type;
        bool canMove = true;
        int serchMaxNum = 0; //0 为无限次
        int serchNum = 0;
        Vector3 targetPos;  //目标位置

        public void SetPetPath()
        {
                speed = 1.2f;
                minWaitTime = 5.0f;
                maxWaitTime = 12.0f;
                type = ObjType.PET;
                nowPos = new Vector2(transform.position.x, transform.position.z);
                Startgo();
        }

        public void SetNpcPath(Vector3 pos)
        {
                if (pos.x < 0)
                {
                        targetPos = transform.position;
                }
                else
                {
                        targetPos = pos;
                }
                speed = UnityEngine.Random.Range(2.0f, 2.5f);
                type = ObjType.NPC;
                nowPos = new Vector2(transform.position.x, transform.position.z);
                Startgo(UnityEngine.Random.Range(2.0f, 5.0f));
        }

        public void SetAnimalPath()
        {
                speed = 1.0f;
                minWaitTime = 1.0f;
                maxWaitTime = 3.0f;
                serchMaxNum = 4;
                type = ObjType.ANIMAL;
                nowPos = new Vector2(transform.position.x, transform.position.z);
                Startgo();
        }

        public void OnTargetReached()
        {
                if (type == ObjType.NPC) //只移动一次//
                {
                        GameUtility.ObjPlayAnim(gameObject, CommonString.idle_001Str, true);
                        return;
                }

                if (serchMaxNum != 0)  //设定了行走次数//
                {
                        serchNum++;
                        if (serchNum > serchMaxNum)
                        {
                                GameObject.Destroy(gameObject, 1.0f);
                                return;
                        }
                }

                Reset();
                if (canMove)
                {
                        Startgo();
                }
        }

        void Reset()
        {
                nowPos = new Vector2(transform.position.x, transform.position.z);
                nowPointIndex = 0;
                pathPoint.Clear();
                jumpState = 0;
                if (moveTweener != null)
                {
                        moveTweener.Kill();
                        moveTweener = null;
                }
                if (go != null)
                {
                        if (AppMain.GetInst() != null)
                        {
                                AppMain.GetInst().StopCoroutine(go);
                        }
                }
                GameUtility.ObjPlayAnim(gameObject, CommonString.idle_001Str, true);
        }

        IEnumerator Go(float time)
        {
                GameUtility.ObjPlayAnim(gameObject, CommonString.idle_001Str, true);
                yield return new WaitForSeconds(time);

                if (type == ObjType.PET) //伙伴
                {
                        nextPos = HomeManager.GetInst().RandomPetPoint(nowPos);
                }
                else if (type == ObjType.NPC) //npc
                {
                        if (targetReached)
                        {
                                yield break;
                        }
                        nextPos = targetPos;
                }
                else                        //小动物
                {
                        nextPos = HomeManager.GetInst().GetAnimalPos();
                }
                SearchPath(nextPos);              
        }

        Coroutine go;
        public void StartGotoTarget(Transform targetTrans)
        {
                canMove = true;
                SearchPath(targetTrans.position);
        }

        public void StartGotoPos(Vector3 pos)
        {
                canMove = true;
                SearchPath(pos);
        }

        public void Startgo() 
        {
                canMove = true;
                go = AppMain.GetInst().StartCoroutine(Go(UnityEngine.Random.Range(minWaitTime, maxWaitTime)));
        }

        public void Startgo(float time) 
        {
                go = AppMain.GetInst().StartCoroutine(Go(time));               
        }

        void Update()
        {
                if (anim != null && anim.IsPlaying("jump_001"))
                {
                        if (anim["jump_001"].normalizedTime >= 0.9f && jumpState == 1)
                        {
                                PlayMoveAnima();
                                jumpState = 0;
                        }

                        if (anim["jump_001"].time <= 0.1f && jumpState == 2)
                        {
                                PlayMoveAnima();
                                jumpState = 0;
                        }
                }
        }

        public Vector3 GetFeetPosition()
        {
                if(transform != null)
                {
                        return transform.position;
                }
                else
                {
                        return Vector3.zero;
                }
        }

        List<Vector3> pathPoint = new List<Vector3>(); //寻路点
        int nowPointIndex = 0;                         //当前走到的路点

        void ReachPoint()
        {
                nowPointIndex++;
                Move();
        }

        Tweener moveTweener;
        int jumpState = 0;  //1 上跳 2 下跳 0 不跳
        void Move()
        {
                if (pathPoint.Count > nowPointIndex)
                {

                        float x = pathPoint[nowPointIndex].x;
                        float z = pathPoint[nowPointIndex].z;
                        if (nowPointIndex == pathPoint.Count - 1)
                        {
                                x = pathPoint[nowPointIndex].x + UnityEngine.Random.Range(-0.2500f, 0.2500f);
                                z = pathPoint[nowPointIndex].z + UnityEngine.Random.Range(-0.1500f, 0.1500f);
                        }

                        Vector3 to = new Vector3(x,pathPoint[nowPointIndex].y,z);

                        float distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(x, z));
                        transform.LookAt(new Vector3(x, transform.position.y, z));
                        moveTweener = transform.DOMove(to, distance / speed).OnComplete(ReachPoint);

                        if (pathPoint[nowPointIndex].y > transform.position.y)
                        {
                                GameUtility.ObjPlayAnim(gameObject, "jump_001", false);
                                jumpState = 1;
                                return;
                        }
                        if (pathPoint[nowPointIndex].y < transform.position.y)
                        {
                                GameUtility.RewindAnim(gameObject, "jump_001");
                                jumpState = 2;
                                return;
                        }

                        PlayMoveAnima();
                }
                else
                {
                        if (!targetReached)
                        {
                                targetReached = true;
                                OnTargetReached();
                        }
                }
        }

        void PlayMoveAnima()
        {
                if (speed >= 2.0f)
                {
                        GameUtility.ObjPlayAnim(gameObject, "run_001", true);
                }
                else
                {
                        GameUtility.ObjPlayAnim(gameObject, "walk_001", true);
                }
        }

        protected Seeker seeker;
        protected Path path;
        public bool targetReached = false;

        protected void Awake()
        {
                seeker = GetComponent<Seeker>();
                anim = gameObject.GetComponent<Animation>();
        }

        private bool startHasRun = false;
        protected void Start()
        {
                startHasRun = true;
                OnEnable();
        }

        protected void OnEnable()
        {
                canMove = true;
                if (startHasRun)
                {
                        seeker.pathCallback = OnPathComplete;
                }
                GameUtility.ObjPlayAnim(gameObject, CommonString.idle_001Str, true);
                if (type == ObjType.ANIMAL)
                {
                        Startgo();
                }
        }

        public void OnDisable()
        {
                //if (seeker != null && !seeker.IsDone())
                //{
                //        seeker.GetCurrentPath().Error();
                //}

                if (path != null) path.Release(this);
                path = null;
                Reset();
                canMove = false;
        }


        public void SearchPath(Vector3 targetPosition)
        {
                if (path != null) path.Release(this);
                path = null;

                if (seeker != null)
                {
                        seeker.StartPath(GetFeetPosition(), targetPosition);
                }
        }

        public void OnPathComplete(Path _p)
        {
                ABPath p = _p as ABPath;
                if (p == null) throw new System.Exception("This function only handles ABPaths, do not use special path types");

                //Claim the new path
                p.Claim(this);

                // Path couldn't be calculated of some reason.
                // More info in p.errorLog (debug string)
                if (p.error)
                {
                        p.Release(this);
                        return;
                }

                //Release the previous path
                if (path != null) path.Release(this);

                //Replace the old path
                path = p;

                //Reset some variables
                targetReached = false;

                if (p.endPoint == p.startPoint)
                {
                        targetReached = true;
                        OnTargetReached();
                }
                else
                {
                        pathPoint = p.vectorPath;
                        Move();                        
                }

        }
}


