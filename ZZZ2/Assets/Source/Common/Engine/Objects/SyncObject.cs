using UnityEngine;
using System.Collections;

public class SyncObject : EngineObject 
{
	const float DEFAULT_SYNC_RATE = 1.0f;
	const float FORCED_SYNC_RATE = 0.05f;
	protected static float DEFAULT_INTERPOLATION_TIME = 0.1f;

	double mNextSyncTime = 0;
	double mLastSyncTime = 0;

	bool mAtRest = true;
	bool mWasAtRest = true;
	protected bool mForceSync = true;

	protected bool mIsOnHost = false;	

	protected double mLastUpdateTime = -Common.BIG_NUMBER;
	protected double mLastPacketTime = -Common.BIG_NUMBER;

	virtual public void DidUpdate()
	{
		mLastUpdateTime = mInstance.GetEngineTime();
		mLastPacketTime = mInstance.GetCurrentEvent().GetTime();
	}

	protected float GetInterpolationAmount()
	{
		float timeSinceUpdate = (float)(mInstance.GetEngineTime() - mLastUpdateTime);

		return Mathf.Clamp01(timeSinceUpdate / DEFAULT_INTERPOLATION_TIME);
	}

	void Start()
	{
		if (gameObject.tag == "LocalPlayer")
		{
			SetIsOnHost(true);
		}
	}

	virtual protected float GetDefaultSyncRate()
	{
		return DEFAULT_SYNC_RATE;
	}

	virtual protected float GetForcedSyncRate()
	{
		return FORCED_SYNC_RATE;
	}

	virtual protected void LateUpdate()
	{
		if (IsOnHost())
		{
			if (GetCurrentTime() >= mNextSyncTime || mForceSync)
			{
				SyncData();
			}
		}
	}

	protected void ForceUpdate(bool forceSync = false)
	{
		if (IsOnHost())
		{
			mForceSync = forceSync;

			if (GetCurrentTime() >= mLastSyncTime + GetForcedSyncRate())
			{
				SyncData();
			}
			else
			{
				mNextSyncTime = mLastSyncTime + GetForcedSyncRate();
			}
		}
	}

	virtual public void DidSyncData()
	{
		mLastSyncTime = GetCurrentTime();
		mNextSyncTime = mLastSyncTime + GetDefaultSyncRate();

		mForceSync = false;
	}

	protected void SyncData()
	{
		mAtRest = CheckIsAtRest();

		if (mAtRest == false || mWasAtRest == false)
		{
			DoSyncData();
		}

		mWasAtRest = mAtRest;

		DidSyncData();
	}

	virtual protected void DoSyncData()
	{
	}

	virtual protected bool CheckIsAtRest()
	{
		return true;
	}

	virtual protected bool IsOnHost()
	{
		return IsServer();// || mIsOnHost;
	}

	public void SetIsOnHost(bool isOnHost)
	{
		mIsOnHost = isOnHost;
	}

	public double GetLastUpdateTime()
	{
		return mLastUpdateTime;
	}

	public double GetLastPacketTime()
	{
		return mLastPacketTime;
	}
}
