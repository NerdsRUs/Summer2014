using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Pathing : SyncObject 
{
	const int INTERPOLATION_FRAMES = 2;

	Vector2 mMoveVelocity;
	Vector2 mLastVelocity;

	Vector3 mQueuedVelocity;
	Vector3 mQueuedPosition;

	double mQueueTime = 0;

	int mFramesSinceUpdate = INTERPOLATION_FRAMES;


	override protected float GetDefaultSyncRate()
	{
		return 0.5f;
	}

	override protected float GetForcedSyncRate()
	{
		return 0.1f;
	}

	public void SetUserVelocity(Vector3 newVelocity)
	{
		mMoveVelocity = newVelocity;

		//Force update when stopping
		ForceUpdate(mMoveVelocity.sqrMagnitude == 0);

		DidUpdate();
	}

	public void UpdateUserVelocity(double time, Vector3 newVelocity, Vector3 position)
	{
		if (IsGraphics() && mIsOnHost)
		{
			return;
		}

		//Relay timing data to clients
		if (IsServer())
		{
			mInstance.GetEventAPI().UpdateMoveVelocity(this, time, newVelocity, position);
		}
		/*if (IsGraphics())
		{
			DidUpdate();

			return;
		}*/

		DidUpdate();

		//mMoveVelocity = newVelocity;
		//mQueuedVelocity = newVelocity;
		mQueuedPosition = position - transform.localPosition;

		transform.localPosition = position;

		/*if (IsServer())
		{
			Debug.Log("Update position");
		}*/

		mMoveVelocity = newVelocity;

		/*if (mMoveVelocity.sqrMagnitude > 0 && mInstance.GetCurrentEvent() != null)
		{
			double deltaTime = mInstance.GetEngineTime() - mInstance.GetCurrentEvent().GetTime();

			transform.localPosition = (Vector2)position + mMoveVelocity * (float)deltaTime / 2;
		}
		else*/
		{
			//transform.localPosition = position;
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

	protected void FixedUpdate()
	{
		Vector2 interpolationVelocity = Vector2.zero;

		/*if (mFramesSinceUpdate < INTERPOLATION_FRAMES)
		{
			mFramesSinceUpdate++;

			interpolationVelocity = mQueuedPosition / (float)INTERPOLATION_FRAMES / Time.fixedDeltaTime;
		}*/

		Vector2 deltaVelocity = rigidbody2D.velocity - mLastVelocity;

		rigidbody2D.velocity = mMoveVelocity + deltaVelocity + interpolationVelocity;

		mLastVelocity = rigidbody2D.velocity;

		/*if (mFramesSinceUpdate < INTERPOLATION_FRAMES)
		{
			if (IsServer())
			{
				Debug.Log(mFramesSinceUpdate + " -> " + interpolationVelocity * 100 + " " + rigidbody2D.velocity * 100 + " delta: " + deltaVelocity * 100);
			}
		}*/

		//base.LateUpdate();
	}

	protected override bool CheckIsAtRest()
	{
		return mMoveVelocity.sqrMagnitude == 0;
	}

	override protected void DoSyncData()
	{
		GetEventAPI().UpdateMoveVelocity(this, mInstance.GetEngineTime(), mMoveVelocity, transform.localPosition);

		PhysicObject[] tempObjects = gameObject.GetComponents<PhysicObject>();

		for (int i = 0; i < tempObjects.Length; i++)
		{
			tempObjects[i].DidSyncData();
		}
	}

	override public void DidUpdate()
	{
		base.DidUpdate();

		mFramesSinceUpdate = 0;

		PhysicObject[] tempObjects = gameObject.GetComponents<PhysicObject>();
		//Debug.Log(tempObjects.Length);
		for (int i = 0; i < tempObjects.Length; i++)
		{
			tempObjects[i].DidUpdate();
			//tempObjects[i].DidSyncData();
		}
	}

	protected override bool IsOnHost()
	{
		return mIsOnHost && IsGraphics() || IsServer();
	}
}
