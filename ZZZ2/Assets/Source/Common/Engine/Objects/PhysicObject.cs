using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class PhysicObject : SyncObject 
{
	const int INTERPOLATION_FRAMES = 2;
	const int GLOBAL_SYNC_RATE = 60;

	const float MIN_SPEED = 0.1f;
	const float MIN_ANGULAR_SPEED = 1.0f;
	const float REPOSITION_TIME = 1.0f;

	Vector3 mPositionOffset;
	float mRotationOffset;

	int mFramesSinceUpdate = INTERPOLATION_FRAMES;

	Vector2 mLastVelocity;
	float mLastAngualrVelocity;

	//public static int mNextSyncFrame = 0;

	/*Vector3 mLastVelocity;
	float mLastAngularVelocity;
	float mLastInertia;

	bool mCancelCollision;*/

	void OnCollisionEnter2D()
	{
		ForceUpdate();

		/*if (IsServer())
		{
			Debug.Log(mInstance.GetEngineTime() + " -> OnCollisionEnter2D");
		}*/

		//CheckCollision();
	}

	void OnCollisionStay2D()
	{
		ForceUpdate();

		/*if (IsServer())
		{
			Debug.Log(mInstance.GetEngineTime() + " -> OnCollisionStay2D");
		}*/

		//CheckCollision();
	}

	void OnCollisionExit2D()
	{
		ForceUpdate();

		/*if (IsServer())
		{
			Debug.Log(mInstance.GetEngineTime() + " -> OnCollisionStay2D");
		}*/

		//CheckCollision();
	}

	//Send sync packets all at the same time to keep things from glitching through other things
	/*protected override void LateUpdate()
	{
 		if (IsOnHost())
		{
			if (Time.frameCount >= mNextSyncFrame || mForceSync)
			{
				SyncData();
			}
		}
	}*/

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

	void FixedUpdate()
	{
		/*if (mCancelCollision)
		{
			rigidbody2D.velocity = mLastVelocity;
			rigidbody2D.angularVelocity = mLastAngularVelocity;
			rigidbody2D.inertia = mLastInertia;

			mCancelCollision = false;
		}

		mLastVelocity = rigidbody2D.velocity;
		mLastAngularVelocity = rigidbody2D.angularVelocity;
		mLastInertia = rigidbody2D.inertia;*/

		

		/*if (mFramesSinceUpdate < INTERPOLATION_FRAMES)
		{
			Vector2 interpolationVelocity = Vector2.zero;
			float interpolationAngularVelocity = 0;

			mFramesSinceUpdate++;

			interpolationVelocity = mPositionOffset / (float)INTERPOLATION_FRAMES / Time.fixedDeltaTime;
			interpolationVelocity = mPositionOffset / (float)INTERPOLATION_FRAMES / Time.fixedDeltaTime;


			Vector2 deltaVelocity = rigidbody2D.velocity - mLastVelocity;
			rigidbody2D.velocity = deltaVelocity + interpolationVelocity;
			mLastVelocity = rigidbody2D.velocity;

			float deltaAngular = rigidbody2D.angularVelocity - mLastAngualrVelocity;
			rigidbody2D.angularVelocity = deltaAngular + interpolationAngularVelocity;
			mLastAngualrVelocity = rigidbody2D.angularVelocity;
		}*/


		if (rigidbody2D.velocity.magnitude < MIN_SPEED)
		{
			rigidbody2D.velocity = Vector3.zero;
		}

		if (Mathf.Abs(rigidbody2D.angularVelocity) < MIN_ANGULAR_SPEED)
		{
			rigidbody2D.angularVelocity = 0;
		}
	}

	protected override bool CheckIsAtRest()
	{
		return rigidbody2D.velocity.magnitude < MIN_SPEED && Mathf.Abs(rigidbody2D.angularVelocity) < MIN_ANGULAR_SPEED;
	}

	override protected void DoSyncData()
	{
		GetEventAPI().UpdatePhysics(this, transform.localPosition, transform.localRotation.eulerAngles, rigidbody2D.velocity, rigidbody2D.angularVelocity);
	}

	void DoUpdate(Vector3 position, Vector3 rotation, Vector3 velocity, float angularVelocity)
	{
		if (IsGraphics() && mIsOnHost)
		{
			return;
		}

		//if (!IsServer())
		/*{
			Debug.Log("GetLastPacketTime: " + GetLastPacketTime() + " mInstance.GetCurrentEvent().GetTime() " + mInstance.GetCurrentEvent().GetTime());
		}*/

		/*if (GetLastPacketTime() >= mInstance.GetCurrentEvent().GetTime())
		{
			return;
		}*/

		Quaternion tempRotation = transform.localRotation;
		tempRotation.eulerAngles = rotation;

		if (mIsOnHost)
		{
			if (IsServer())
			{
				return;
			}

			/*if ((position - transform.position).magnitude < velocity.magnitude * REPOSITION_TIME &&
				Quaternion.Angle(tempRotation, transform.localRotation) < angularVelocity * REPOSITION_TIME)
			{
				return;
			}*/
		}

		DidUpdate();

		/*if ((position - transform.position).sqrMagnitude < REPOSITION_CUTOFF * REPOSITION_CUTOFF && 
			Quaternion.Angle(tempRotation, transform.localRotation) < ROTATION_CUTOFF)
		{
			return;
		}*/

		mPositionOffset = position - transform.localPosition;
		mRotationOffset = Quaternion.Angle(tempRotation, transform.localRotation);

		transform.localPosition = position;
		transform.localRotation = tempRotation;

		rigidbody2D.velocity = velocity;
		rigidbody2D.angularVelocity = angularVelocity;
	}

	public override void DidUpdate()
	{
		base.DidUpdate();

		mFramesSinceUpdate = 0;
	}

	/*public override void DidSyncData()
	{
		mNextSyncFrame = Time.frameCount + GLOBAL_SYNC_RATE;

		base.DidSyncData();
	}*/

	/*override protected bool IsOnHost()
	{
		return IsServer() && !mIsOnHost;
	}*/
}
