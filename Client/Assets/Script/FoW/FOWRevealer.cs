using UnityEngine;

/// <summary>
/// Fog of War system needs 3 components in order to work:
/// - Fog of War system that will create a height map of your scene and perform all the updates.
/// - Fog of War Image Effect on the camera that will be displaying the fog of war.
/// - Fog of War Revealer on one or more game objects in the world (this class).
/// </summary>

[AddComponentMenu("Fog of War/Revealer")]
public class FOWRevealer : MonoBehaviour
{
	Transform mTrans;

	/// <summary>
	/// Radius of the area being revealed. Everything below X is always revealed. Everything up to Y may or may not be revealed.
	/// </summary>
        ///原先是Vector2,range.y表示外圈size，现在需求长宽不一致，所以用Vector3                
	public Vector3 range = new Vector3(2f, 10f, 10f);

	/// <summary>
	/// What kind of line of sight checks will be performed.
	/// - "None" means no line of sight checks, and the entire area covered by radius.y will be revealed.
	/// - "OnlyOnce" means the line of sight check will be executed only once, and the result will be cached.
	/// - "EveryUpdate" means the line of sight check will be performed every update. Good for moving objects.
	/// </summary>

	public FOWSystem.LOSChecks lineOfSightCheck = FOWSystem.LOSChecks.None;

	/// <summary>
	/// Whether the revealer is actually active or not. If you wanted additional checks such as "is the unit dead?",
	/// then simply derive from this class and change the "isActive" value accordingly.
	/// </summary>

	public bool isActive = true;

	FOWSystem.Revealer mRevealer;

        public FOWSystem.Revealer GetRevealer()
        {
                return mRevealer;
        }

        void Awake ()
	{
		mTrans = transform;
		mRevealer = FOWSystem.CreateRevealer();
	}

	void OnDisable ()
	{
		mRevealer.isActive = false;
	}

	void OnDestroy ()
	{
		FOWSystem.DeleteRevealer(mRevealer);
		mRevealer = null;
	}
        public void SetupRevealer(Vector3 position, Vector3 _range)
        {
                this.transform.position = position;
                this.range = _range;

                mRevealer.pos = position;
                mRevealer.inner = _range.x;
                mRevealer.outer = _range.y;
                mRevealer.outerz = _range.z;
        }

        void LateUpdate ()
	{
		if (isActive)
		{
			if (lineOfSightCheck != FOWSystem.LOSChecks.OnlyOnce) mRevealer.cachedBuffer = null;

			mRevealer.pos = mTrans.position;
			mRevealer.inner = range.x;
			mRevealer.outer = range.y;
                        mRevealer.outerz = range.z;
                        mRevealer.los = lineOfSightCheck;
			mRevealer.isActive = true;
		}
		else
		{
			mRevealer.isActive = false;
			mRevealer.cachedBuffer = null;
		}
	}

	void OnDrawGizmosSelected ()
	{
		if (lineOfSightCheck != FOWSystem.LOSChecks.None && range.x > 0f)
		{
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(transform.position, range.x);
		}
		Gizmos.color = Color.grey;
		Gizmos.DrawWireSphere(transform.position, range.y);
	}

	/// <summary>
	/// Want to force-rebuild the cached buffer? Just call this function.
	/// </summary>

	public void Rebuild () { mRevealer.cachedBuffer = null; }
}