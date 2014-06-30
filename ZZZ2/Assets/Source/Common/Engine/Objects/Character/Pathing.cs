using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Pathing : SyncObject 
{
	//Should be about latency x2?
	static float LAG_TIME = 0.1f;

	Vector3 mMoveVelocity;
	Vector2 mLastVelocity;

	Vector3 mQueuedVelocity;
	Vector2 mQueuedPosition;

	double mQueueTime = 0;

	override protected float GetDefaultSyncRate()
	{
		return 0.5f;
	}

	override protected float GetForcedSyncRate()
	{
		return 0.05f;
	}

	public void SetUserVelocity(Vector3 newVelocity)
	{
		mMoveVelocity = newVelocity;

		//Force update when stopping
		ForceUpdate(mMoveVelocity.sqrMagnitude == 0);
	}

	public void UpdateUserVelocity(Vector3 newVelocity, Vector3 position)
	{
		if (mQueueTime != 0)
		{
			DoQueuedUpdate();
		}

		//Delay all movement by the lag time (Don't delay stop commands
		if (mMoveVelocity.sqrMagnitude != 0)
		{
			mMoveVelocity = newVelocity;

			if (IsServer())
			{
				Debug.Log("Update position");
			}

			transform.localPosition = position;
		}
		else
		{
			mQueuedVelocity = newVelocity;
			mQueuedPosition = position;

			mQueueTime = GetCurrentTime() + LAG_TIME;
		}
	}

	void DoQueuedUpdate()
	{
		mQueueTime = 0;

		mMoveVelocity = mQueuedVelocity;

		if (IsServer())
		{
			Debug.Log("Update position");
		}

		transform.localPosition = mQueuedPosition;
	}

	void Update()
	{
		if (mQueueTime != 0 && GetCurrentTime() >= mQueueTime)
		{
			DoQueuedUpdate();			
		}
	}

	override protected void LateUpdate()
	{
		Vector2 deltaVelocity = rigidbody2D.velocity - mLastVelocity;

		rigidbody2D.velocity = new Vector2(mMoveVelocity.x, mMoveVelocity.y) + deltaVelocity;

		mLastVelocity = rigidbody2D.velocity;

		base.LateUpdate();
	}

	protected override bool CheckIsAtRest()
	{
		return mMoveVelocity.sqrMagnitude == 0;
	}

	override protected void DoSyncData()
	{
		GetEventAPI().UpdateMoveVelocity(this, mMoveVelocity, transform.localPosition);

		/*PhysicObject[] tempObjects = gameObject.GetComponents<PhysicObject>();

		for (int i = 0; i < tempObjects.Length; i++)
		{
			tempObjects[i].DidSyncData();
		}*/
	}
}
