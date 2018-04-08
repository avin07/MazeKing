using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class BuildingInfoHold : ConfigBase
{
        public string name;
        public string icon;
        public string desc;
        public int cost_time;                   //����ʱ��
        public string cost_material;            //���Ĳ���
        public string clean_cost;               //������    
        public string clean_restore;            //������
        public string model;                    //ģ��   
        public int add_reputation;              //��������
        public string minor_function;

        //���ֽܵ�npc��ׯ�õ�
        public int size_x;
        public int size_y;  

        
        public BuildingInfoHold(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
                
        }

}
