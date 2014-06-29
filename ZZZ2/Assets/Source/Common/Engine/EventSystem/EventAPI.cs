using UnityEngine;
using System.Collections;

public class EventAPI 
{
	static EventManager mCurrentInstance;

	static public void SetCurrentInstance(EventManager instance)
	{
		mCurrentInstance = instance;
	}

	static public void SetUserVelocity(int objectID, Vector3 newVelocity)
	{
		Debug.Log(mCurrentInstance);
		mCurrentInstance.GetObject<Pathing>(objectID).SetUserVelocity(newVelocity);
	}
}
