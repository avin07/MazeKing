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
        public int elevation_group;             //海拔组
        public string clean_effect;
        public int can_be_moved;                //1 可移动 0 不可移动
        public string function_bottom;
        public int page_in_list;
        public int take_capacity;               //占用容积
        public string bubble_hint;
        public List<int> mSuit = new List<int>(); //套装

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
        eMain = 1,              //主城
        eBench = 2,             //制作  
        eGharry = 3,            //马厩  
        eHotel = 4,             //招募 
        eStorage = 5,           //仓库 
        //eCemetery = 6,        //墓地
        eTaskBoard = 7,         //任务公告板
        eCure = 8,              //医疗房
        eStay = 9,              //居民房
        eObstacle = 10,         //障碍类
        eFire = 11,             //圣火房
        //eRoom = 12,             //空房间 当前版本的所有消息处理都要基于这个类型类区别！
        eWall = 13,             //墙
        eDoor = 14,             //门
        eDecoration = 15,       //装饰类
        eBookShelves = 16,      //书架类
        eSpecificity = 17,      //特性类

        eProduce = 41,          //生产类 暂无
        ePower = 42,            //属性类
        eSquare = 43,           //广场类
        //eDevelop = 44,          //养成类

        //eFoundation = 999,    //地基  客户端独有的//   
        eEnd,
}
