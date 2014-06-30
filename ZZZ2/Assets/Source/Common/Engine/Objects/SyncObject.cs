using UnityEngine;
using System.Collections;

public class SyncObject : EngineObject 
{
	const float DEFAULT_SYNC_RATE = 0.5f;
	const float FORCED_SYNC_RATE = 0.1f;

	double mNextSyncTime = 0;
	double mLastSyncTime = 0;

	bool mAtRest = true;
	bool mWasAtRest = true;

	void LateUpdate()
	{
		if (IsOnHost())
		{
			if (GetCurrentTime() >= mNextSyncTime)
			{
				SyncData();
			}
		}
	}

	protected void ForceUpdate()
	{
		if (IsOnHost())
		{
			if (GetCurrentTime() >= mLastSyncTime + FORCED_SYNC_RATE)
			{
				SyncData();
			}
			else
			{
				mNextSyncTime = mLastSyncTime + FORCED_SYNC_RATE;
			}
		}
	}

	void SyncData()
	{
		mLastSyncTime = GetCurrentTime();
		mNextSyncTime = mLastSyncTime + DEFAULT_SYNC_RATE;	

		mAtRest = CheckIsAtRest();

		if (mAtRest == false || mWasAtRest == false)
		{
			DoSyncData();
		}

		mWasAtRest = mAtRest;
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
