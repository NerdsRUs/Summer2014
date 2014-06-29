using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class PhysicObject : EngineObject 
{
	const float DEFAULT_SYNC_RATE = 0.5f;
	const float COLLISION_SYNC_RATE = 0.1f;

	double mNextSyncTime = 0;
	double mLastSyncTime = 0;

	void LateUpdate()
	{
		if (GetCurrentTime() >= mNextSyncTime)
		{
			SyncData();

			mLastSyncTime = GetCurrentTime();
			mNextSyncTime = mLastSyncTime + DEFAULT_SYNC_RATE;			
		}
	}

	void OnCollisionEnter2D()
	{
		HadCollision();
	}

	void OnCollisionExit2D()
	{
		HadCollision();
	}

	void HadCollision()
	{
		//Force atleast COLLISION_SYNC_RATE seconds between updates
		if (GetCurrentTime() >= mLastSyncTime + COLLISION_SYNC_RATE)
		{
			SyncData();
		}
		else
		{
			mNextSyncTime = mLastSyncTime + COLLISION_SYNC_RATE;
		}
	}

	void SyncData()
	{
		if (rigidbody2D.velocity.sqrMagnitude > 0 || rigidbody2D.angularVelocity > 0)
		{
			GetEventAPI().UpdatePhysics(this, transform.localPosition, transform.localScale, transform.localRotation.eulerAngles, rigidbody2D.velocity, rigidbody2D.angularVelocity);
		}
	}

	void DoUpdate(Vector3 position, Vector3 scale, Vector3 rotation, Vector3 velocity, float angularVelocity)
	{
		transform.localPosition = position;
		transform.localScale = scale;

		Quaternion tempRotation = transform.localRotation;
		tempRotation.eulerAngles = rotation;
		transform.localRotation = tempRotation;

		rigidbody2D.velocity = velocity;
		rigidbody2D.angularVelocity = angularVelocity;
	}
}
