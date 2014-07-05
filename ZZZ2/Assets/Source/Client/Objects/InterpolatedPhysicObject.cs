using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhysicObject))]
public class InterpolatedPhysicObject : MonoBehaviour
{
	static float DEFAULT_INTERPOLATION_TIME = 5.0f;
	static float DELAY_TIME = 0;//0.5f;
	const float MAX_DESYNC_TIME = 0.1f;

	Transform mSyncedObject;
	PhysicObject mSyncedPhysics;
	Pathing mSyncedPathing;
	Rigidbody2D mSyncedRigidbody;
	EngineManager mThisManager;

	float mLastUpdateTime = -Common.BIG_NUMBER;

	bool mIsSynced = true;
	double mLastDesyncTime = 0;

	//This needs to be OnRecievedID, or something
	void Start()
	{
		PhysicObject tempObject = gameObject.GetComponent<PhysicObject>();

		if (tempObject != null)
		{
			mThisManager = tempObject.GetInstance();

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

	void OnCollisionEnter2D()
	{
		if (!mIsSynced)
		{
			mLastDesyncTime = mThisManager.GetEngineTime();
		}
	}

	bool CheckSynced()
	{
		Vector3 maximumDisplacment = mSyncedRigidbody.velocity * MAX_DESYNC_TIME;
		float maximumAngularDisplacement = mSyncedRigidbody.angularVelocity * MAX_DESYNC_TIME;

		Vector3 projectedTranslation = mSyncedRigidbody.transform.localPosition;// +(Vector3)mSyncedRigidbody.velocity * 0.3f;
		Quaternion projectedRotation = mSyncedRigidbody.transform.localRotation;// Quaternion.Euler(mSyncedRigidbody.transform.localRotation.eulerAngles + new Vector3(0, 0, mSyncedRigidbody.angularVelocity * 0.3f));

		/*if (name == "Cube(ID: 4)")
		{
			Debug.Log("CheckSynced: " + Vector3.Distance(mSyncedRigidbody.transform.localPosition, transform.localPosition) + " > " + maximumDisplacment.magnitude + "  ||  " + Mathf.Abs(Quaternion.Angle(mSyncedRigidbody.transform.localRotation, transform.localRotation)) + " > " + Mathf.Abs(maximumAngularDisplacement));
		}*/

		if (Vector3.Distance(projectedTranslation, transform.localPosition) > maximumDisplacment.magnitude ||
			Mathf.Abs(Quaternion.Angle(projectedRotation, transform.localRotation)) > Mathf.Abs(maximumAngularDisplacement))
		{
			/*if (name == "Cube(ID: 4)")
			{
				Debug.Log("False");

			}*/
			return false;
		}

		return true;
	}
	
	void FixedUpdate()
	{
		if (mIsSynced)
		{
			if (!CheckSynced())
			{
				mIsSynced = false;

				mLastDesyncTime = mThisManager.GetEngineTime();
			}
		}
		else
		{
			if (CheckSynced())
			{
				mIsSynced = true;

				mLastDesyncTime = 0;
			}

			if (mLastDesyncTime != 0)
			{
				double timeSinceLastDesync = mThisManager.GetEngineTime() - mLastDesyncTime - DELAY_TIME;

				float interpolation = (float)timeSinceLastDesync / DEFAULT_INTERPOLATION_TIME;
				interpolation = Mathf.Clamp01(interpolation);

				if (interpolation > 0)
				{
					/*if (name == "Cube(ID: 4)")
					{
						Debug.Log(interpolation);
					}*/

					if (mSyncedObject)
					{
						transform.localPosition = Vector3.Lerp(transform.localPosition, mSyncedObject.localPosition, interpolation);
						transform.localRotation = Quaternion.Lerp(transform.localRotation, mSyncedObject.localRotation, interpolation);
						transform.localScale = Vector3.Lerp(transform.localScale, mSyncedObject.localScale, interpolation);
						//transform.localScale = mSyncedObject.localScale;
					}

					if (mSyncedRigidbody)
					{
						rigidbody2D.velocity = Vector3.Lerp(rigidbody2D.velocity, mSyncedRigidbody.velocity, interpolation);
						rigidbody2D.angularVelocity = Mathf.Lerp(rigidbody2D.angularVelocity, mSyncedRigidbody.angularVelocity, interpolation);
						//rigidbody2D.angularVelocity = mSyncedPhysics.angularVelocity;
					}
				}
				else if (interpolation == 1)
				{
					mIsSynced = true;

					mLastDesyncTime = 0;
				}
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
