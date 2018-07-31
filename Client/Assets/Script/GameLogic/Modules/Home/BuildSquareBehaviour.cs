using UnityEngine;

public class BuildSquareBehaviour : BuildBaseBehaviour  //广场类类建筑
{

        public override void SetBuild(BuildInfo build_info)
        {
                Operational = false;
                base.SetBuild(build_info);
        }

}
