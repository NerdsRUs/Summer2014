using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhysicObject))]
public class InterpolatedPhysicObject : MonoBehaviour
{
	static float DEFAULT_INTERPOLATION_TIME = 1.0f;

	Transform mSyncedObject;
	PhysicObject mSyncedPhysics;
	Pathing mSyncedPathing;
	Rigidbody2D mSyncedRigidbody;

	float mLastUpdateTime = -Common.BIG_NUMBER;	

	//This needs to be OnRecievedID, or something
	void Start()
	{
		PhysicObject tempObject = gameObject.GetComponent<PhysicObject>();

		if (tempObject != null)
		{
			if (tempObject.GetInstance().GetSyncedClient() != null)
			{
				int objectID = tempObject.GetObjectID();

				tempObject = tempObject.GetInstance().GetSyncedClient().GetObject<PhysicObject>(objectID);

				mSyncedObject = tempObject.transform;
				mSyncedPhysics = tempObject;
				mSyncedPathing = tempObject.GetComponent<Pathing>();
				mSyncedRigidbody = tempObject.rigidbody2D;
			}
		}
	}

	void FixedUpdate()
	{
		float interpolation = GetInterpolationAmount();
		//Debug.Log(interpolation);
		if (interpolation < 1.0f)
		{
			/*if (mSyncedObject)
			{
				transform.localPosition = Vector3.Lerp(transform.localPosition, mSyncedObject.localPosition, interpolation);
				transform.localRotation = Quaternion.Lerp(transform.localRotation, mSyncedObject.localRotation, interpolation);
				transform.localScale = Vector3.Lerp(transform.localScale, mSyncedObject.localScale, interpolation);
				//transform.localScale = mSyncedObject.localScale;
			}*/

			if (mSyncedRigidbody)
			{
				rigidbody2D.velocity = Vector3.Lerp(rigidbody2D.velocity, mSyncedRigidbody.velocity, interpolation);
				rigidbody2D.angularVelocity = Mathf.Lerp(rigidbody2D.angularVelocity, mSyncedRigidbody.angularVelocity, interpolation);
				//rigidbody2D.angularVelocity = mSyncedPhysics.angularVelocity;
			}
		}
	}

	protected float GetInterpolationAmount()
	{
		float lowestInterpolation = 10;

		if (mSyncedPhysics != null)
		{
			float timeSinceUpdate = (float)(mSyncedPhysics.GetInstance().GetEngineTime() - mSyncedPhysics.GetLastUpdateTime());

			lowestInterpolation = Mathf.Clamp01(timeSinceUpdate / DEFAULT_INTERPOLATION_TIME);
		}

		if (mSyncedPathing != null)
		{
			float timeSinceUpdate = (float)(mSyncedPhysics.GetInstance().GetEngineTime() - mSyncedPathing.GetLastUpdateTime());

			lowestInterpolation = Mathf.Clamp01(timeSinceUpdate / DEFAULT_INTERPOLATION_TIME);
		}

		return lowestInterpolation;
	}
}
