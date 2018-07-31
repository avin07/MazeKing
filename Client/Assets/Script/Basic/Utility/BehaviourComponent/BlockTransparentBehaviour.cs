using UnityEngine;
using System.Collections;

public class BlockTransparentBehaviour : MonoBehaviour
{

        //得到主人公
        private GameObject hero;
        //记录上次的对象
        private GameObject last_obj;

        void Start()
        {
                hero = RaidManager.GetInst().MainHero.gameObject;

        }
        void Update()
        {
                //为了调式时看的清楚画的线
                Debug.DrawLine(hero.transform.position, transform.position, Color.red);
                RaycastHit hit;

                if (Physics.Linecast(hero.transform.position, transform.position, out hit))
                {
                        last_obj = hit.collider.gameObject;
                        //判断
                        if (!last_obj.CompareTag("MainCamera") && !last_obj.CompareTag("terrain"))
                        {
                                //让遮挡物变半透明
                                Color obj_color = last_obj.GetComponent<Renderer>().material.color;
                                obj_color.a = 0.5f;
                                last_obj.GetComponent<Renderer>().material.SetColor("_Color", obj_color);

                        }
                }//还原
                else if (last_obj != null)
                {
                        Color obj_color = last_obj.GetComponent<Renderer>().material.color;
                        obj_color.a = 1.0f;
                        last_obj.GetComponent<Renderer>().material.SetColor("_Color", obj_color);
                        last_obj = null;
                }

        }

}
