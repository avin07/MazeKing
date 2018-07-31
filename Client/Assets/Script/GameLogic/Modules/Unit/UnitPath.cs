using UnityEngine;
using System.Collections;

public class UnitPath : AIPath 
{
        public HeroUnit BelongUnit
        {
                get
                {
                        return m_BelongUnit;
                }
                set
                {
                        m_BelongUnit = value;
                }
        }

        HeroUnit m_BelongUnit;
        public bool IsSearchComp
        {
                get { return canSearchAgain; }
        }
        public override void OnTargetReached()
        {
                base.OnTargetReached();

                if (GetComponent<HeroUnit>() != null)
                {
                        GetComponent<HeroUnit>().OnTargetReached();
                }
        }

        public override void OnPathComplete(Pathfinding.Path _p)
        {
                base.OnPathComplete(_p);

                if (_p.vectorPath.Count > 1)
                {
                        if (BelongUnit != null)
                        {
                                BelongUnit.SetTargetDirection(_p.vectorPath[_p.vectorPath.Count - 1], _p.vectorPath[_p.vectorPath.Count - 2]);
                        }
                }
        }
}
