using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Pathing : SyncObject 
{
	Vector3 mMoveVelocity;
	Vector2 mLastVelocity;

	public void SetUserVelocity(Vector3 newVelocity)
	{
		mMoveVelocity = newVelocity;

		ForceUpdate();
	}

	public void UpdateUserVelocity(Vector3 newVelocity, Vector3 position)
	{
		mMoveVelocity = newVelocity;

		transform.localPosition = position;
	}

	virtual protected void FixedUpdate()
	{
		Vector2 deltaVelocity = rigidbody2D.velocity - mLastVelocity;

		rigidbody2D.velocity = new Vector2(mMoveVelocity.x, mMoveVelocity.y) + deltaVelocity;

		mLastVelocity = rigidbody2D.velocity;
	}

	protected override bool CheckIsAtRest()
	{
		return mMoveVelocity.sqrMagnitude == 0;
	}

	override protected void DoSyncData()
	{
		GetEventAPI().UpdateMoveVelocity(this, mMoveVelocity, transform.localPosition);
	}
}
