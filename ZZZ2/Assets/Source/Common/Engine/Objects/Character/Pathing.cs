using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Pathing : SyncObject 
{
	Vector2 mMoveVelocity;
	Vector2 mLastVelocity;

	Vector3 mQueuedVelocity;
	Vector3 mQueuedPosition;

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

		DidUpdate();
	}

	public void UpdateUserVelocity(Vector3 newVelocity, Vector3 position)
	{
		if (IsGraphics())
		{
			DidUpdate();

			return;
		}

		DidUpdate();

		/*mMoveVelocity = newVelocity;
		mQueuedVelocity = newVelocity;
		mQueuedPosition = position;*/

		/*if (IsServer())
		{
			Debug.Log("Update position");
		}*/

		if (mMoveVelocity.sqrMagnitude > 0 && mInstance.GetCurrentEvent() != null)
		{
			double deltaTime = mInstance.GetEngineTime() - mInstance.GetCurrentEvent().GetTime();

			transform.localPosition = (Vector2)position + mMoveVelocity * (float)deltaTime / 2;
		}
		else
		{
			transform.localPosition = position;
		}
	}

	/*void DoQueuedUpdate()
	{
		mQueueTime = 0;

		mMoveVelocity = mQueuedVelocity;

		if (IsServer())
		{
			Debug.Log("Update position");
		}

		transform.localPosition = mQueuedPosition;
	}*/

	/*void Update()
	{
		
	}*/

	override protected void LateUpdate()
	{
		/*Vector2 speedModifier = Vector2.zero;

		if (IsServer())
		{
			float interpolation = GetInterpolationAmount();

			if (interpolation < 1.0f)
			{
				Vector3 projectedPosition = mQueuedPosition + mQueuedVelocity * interpolation * DEFAULT_INTERPOLATION_TIME;

				speedModifier = projectedPosition - transform.localPosition;
				speedModifier = speedModifier.normalized * mMoveVelocity.magnitude / 2;
			}
		}*/

		Vector2 deltaVelocity = rigidbody2D.velocity - mLastVelocity;

		rigidbody2D.velocity = mMoveVelocity + deltaVelocity;// +speedModifier;

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

		PhysicObject[] tempObjects = gameObject.GetComponents<PhysicObject>();

		for (int i = 0; i < tempObjects.Length; i++)
		{
			tempObjects[i].DidSyncData();
		}
	}

	override public void DidUpdate()
	{
		base.DidUpdate();

		PhysicObject[] tempObjects = gameObject.GetComponents<PhysicObject>();
		//Debug.Log(tempObjects.Length);
		for (int i = 0; i < tempObjects.Length; i++)
		{
			tempObjects[i].DidUpdate();
			//tempObjects[i].DidSyncData();
		}
	}

	/*protected override bool IsOnHost()
	{
		return mIsOnHost;
	}*/
}
