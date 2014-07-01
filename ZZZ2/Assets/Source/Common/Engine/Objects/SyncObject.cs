using UnityEngine;
using System.Collections;

public class SyncObject : EngineObject 
{
	const float DEFAULT_SYNC_RATE = 1.0f;
	const float FORCED_SYNC_RATE = 0.05f;

	double mNextSyncTime = 0;
	double mLastSyncTime = 0;

	bool mAtRest = true;
	bool mWasAtRest = true;
	bool mForceSync = true;

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

	public void DidSyncData()
	{
		mLastSyncTime = GetCurrentTime();
		mNextSyncTime = mLastSyncTime + GetDefaultSyncRate();

		mForceSync = false;
	}

	void SyncData()
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
		return IsServer();
	}
}
