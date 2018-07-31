using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class BuildingTypeHold : ConfigBase
{
        public int function_type;
        public int size_x;
        public int size_y;
        public int height;
        public int elevation_group;             //������
        public string clean_effect;
        public int can_be_moved;                //1 ���ƶ� 0 �����ƶ�
        public string function_bottom;
        public int page_in_list;
        public int take_capacity;               //ռ���ݻ�
        public string bubble_hint;
        public List<int> mSuit = new List<int>(); //��װ

        public BuildingTypeHold(XmlNode child) : base(child)
        {
        }

        public override void InitSelf(XmlNode child)
        {
                SetupFields(child);
                mSuit = XMLPARSE_METHOD.GetNodeInnerIntList(child, "in_set_list", ',', 0);
        }
}

public enum EBuildType  
{
        eBegin = 0,
        eMain = 1,              //����
        eBench = 2,             //����  
        eGharry = 3,            //���  
        eHotel = 4,             //��ļ 
        eStorage = 5,           //�ֿ� 
        //eCemetery = 6,        //Ĺ��
        eTaskBoard = 7,         //���񹫸��
        eCure = 8,              //ҽ�Ʒ�
        eStay = 9,              //����
        eObstacle = 10,         //�ϰ���
        eFire = 11,             //ʥ��
        //eRoom = 12,             //�շ��� ��ǰ�汾��������Ϣ����Ҫ�����������������
        eWall = 13,             //ǽ
        eDoor = 14,             //��
        eDecoration = 15,       //װ����
        eBookShelves = 16,      //�����
        eSpecificity = 17,      //������

        eProduce = 41,          //������ ����
        ePower = 42,            //������
        eSquare = 43,           //�㳡��
        //eDevelop = 44,          //������

        //eFoundation = 999,    //�ػ�  �ͻ��˶��е�//   
        eEnd,
}
