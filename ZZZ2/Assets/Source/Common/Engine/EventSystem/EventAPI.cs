using UnityEngine;
using System.Collections;

public class EventAPI : EventAPIBase 
{

	static public void SetUserVelocity(int objectID, Vector3 newVelocity)
	{
		Call("SetUserVelocityAction", objectID, newVelocity);
	}

	static private void SetUserVelocityAction(int objectID, Vector3 newVelocity)
	{
		mCurrentInstance.GetObject<Pathing>(objectID).SetUserVelocity(newVelocity);		
	}
	
}
