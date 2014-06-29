using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Pathing : EngineObject 
{
	Vector3 mMoveVelocity;

	public void SetUserVelocity(Vector3 newVelocity)
	{
		mMoveVelocity = newVelocity;
	}

	virtual protected void FixedUpdate()
	{
		rigidbody2D.velocity = mMoveVelocity;
	}
}
