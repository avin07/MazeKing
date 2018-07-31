using UnityEngine;
using DG.Tweening;

public class BuildFireBehaviour : BuildBaseBehaviour  //圣火房建筑
{
        protected override bool CompareBuildInfo(BuildInfo new_bi)
        {
                return false;
        }


}