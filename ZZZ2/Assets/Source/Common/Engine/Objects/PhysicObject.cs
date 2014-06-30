using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class PhysicObject : SyncObject 
{
	/*Vector3 mLastVelocity;
	float mLastAngularVelocity;
	float mLastInertia;

	bool mCancelCollision;*/

	void OnCollisionEnter2D()
	{
		ForceUpdate();

		//CheckCollision();
	}

	/*void OnCollisionStay()
	{
		CheckCollision();
	}*/

	void OnCollisionExit2D()
	{
		ForceUpdate();

		//CheckCollision();
	}

	/*void CheckCollision()
	{
		if (!IsOnHost())
		{
			if (GetObjectID() == 2)
			{
				Debug.Log("Cancel collision " + Time.frameCount + " " + mLastAngularVelocity + " -> " + rigidbody2D.angularVelocity);
			}

			mCancelCollision = true;
		}
	}*/

	/*void FixedUpdate()
	{
		if (mCancelCollision)
		{
			rigidbody2D.velocity = mLastVelocity;
			rigidbody2D.angularVelocity = mLastAngularVelocity;
			rigidbody2D.inertia = mLastInertia;

			mCancelCollision = false;
		}

		mLastVelocity = rigidbody2D.velocity;
		mLastAngularVelocity = rigidbody2D.angularVelocity;
		mLastInertia = rigidbody2D.inertia;
	}*/

	protected override bool CheckIsAtRest()
	{
		return rigidbody2D.velocity.sqrMagnitude == 0 && Mathf.Abs(rigidbody2D.angularVelocity) == 0;
	}

	override protected void DoSyncData()
	{
		GetEventAPI().UpdatePhysics(this, transform.localPosition, transform.localScale, transform.localRotation.eulerAngles, rigidbody2D.velocity, rigidbody2D.angularVelocity);
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
