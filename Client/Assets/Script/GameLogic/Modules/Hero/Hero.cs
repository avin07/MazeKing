using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 游戏中角色的数据类
/// </summary>
public class Hero
{
    public long ID;
    //名字
    public string Name;
    // 等级
    public int Level;
    // 当前经验
    public int Exp;
    // 当前技能点
    public int Sp;
    //种族
    public int Race;
    /// <summary>
    /// 角色信息
    /// </summary>
    public CharacterConfig CharacterCfg;
    //职业
    public CareerConfigHold CareerCfg;
    //属性色
    public int Color;
    /// <summary>
    /// 性格修正（最大值有正负变化），查表
    /// </summary>
    public int Variation;

    /////（存放进副本前的标准数值，受升级成长影响所以不能查表）
    //生命值
    public int Hp_Std;
    //攻击
    public int Atk_Std;
    //速度
    public int Spd_Std;
    //物防
    public int PDef_Std;
    //魔防
    public int MDef_Std;

    /////技能（武器，饰品，护甲都属于技能类范畴）
    //武器
    public int Weapon;
    //护甲
    public int Armor;
    //饰品
    public int Accessory;
    //奥义
    public int SpecialSkill;
    //A类技能
    public int SkillA;
    //B类技能
    public int SkillB;
    //C类技能
    public int SkillC;
    //D类技能
    public int SkillS;

    public Hero(CareerConfigHold cfg, int _level)
    {
        CareerCfg = cfg;
        Level = _level;
        Hp_Std = cfg.init_hp;
        Atk_Std = cfg.init_atk;
        Spd_Std = cfg.init_spd;
        PDef_Std = cfg.init_pdef;
        MDef_Std = cfg.init_mdef;
    }
}
