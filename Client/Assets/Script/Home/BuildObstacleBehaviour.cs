using UnityEngine;
using System;

public class BuildObstacleBehaviour : BuildBaseBehaviour  //装饰类建筑
{
        protected override bool CompareBuildInfo(BuildInfo new_bi)
        {
                if (mBuildInfo.level < new_bi.level)
                {
                        if (!mBuildInfo.buildCfg.model.Equals(new_bi.buildCfg.model))
                        {
                                if (ModelRoot != null)
                                {
                                        GameUtility.DestroyChild(ModelRoot.transform);
                                }
                                GetModel(new_bi);    
                        }
                }
                return false;
        }

}