using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Pathing : EngineObject 
{
	Vector3 mMoveVelocity;
	Vector2 mLastVelocity;

	public void SetUserVelocity(Vector3 newVelocity)
	{
		mMoveVelocity = newVelocity;
	}

	virtual protected void FixedUpdate()
	{
		Vector2 deltaVelocity = rigidbody2D.velocity - mLastVelocity;

		rigidbody2D.velocity = new Vector2(mMoveVelocity.x, mMoveVelocity.y) + deltaVelocity;

		mLastVelocity = rigidbody2D.velocity;
	}
}
