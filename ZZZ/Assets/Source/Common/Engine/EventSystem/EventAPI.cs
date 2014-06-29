using UnityEngine;
using System.Collections;

public class EventAPI 
{
	static EventManager mCurrentInstance;

	static public void SetCurrentInstance(EventManager instance)
	{
		mCurrentInstance = instance;
	}

	static void SetUserVelocity(int objectID, Vector3 newVelocity)
	{
		mCurrentInstance.GetObject<Pathing>(objectID).SetUserVelocity(newVelocity);
	}
}
