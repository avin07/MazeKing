using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ItemType
{

        public enum BAG_PLACE
        {
                MAIN = 0,    // 主背包
                RAID = 1,    // 迷宫背包
                MAX  = 2,     
        }

        public enum EQUIP_PART
        {
                MAIN_HAND = 11, //主手
                OFF_HAND = 12,  //副手
                BODY = 13,      //身体
                HEAD = 14,      //头
                FEET = 15,      //脚
                JEWELRY = 16,   //饰品
                MAX = 17,
        }

        public static string[] part_des = { "主手","副手","衣服","帽子","鞋子","首饰"};
}
