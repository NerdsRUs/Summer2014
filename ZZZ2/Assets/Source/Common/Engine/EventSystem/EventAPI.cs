using UnityEngine;
using System.Collections;

public class EventAPI : EventAPIBase 
{
	static public void SetUserVelocity(int objectID, Vector3 newVelocity)
	{
		CallOnObject(objectID, "SetUserVelocity", newVelocity);

		//Use the above command to call functions defined in other objects (must be of type EngineObject) - the type will be detected at runtime
		//Further, this function will automatically convert objectIDs to the object type required by the function (again, must be of type EngineObject to convert)
		

		//If you need some complex control over the command, then define a second private function and Call that, as shown below

		//Call("SetUserVelocityAction", objectID, newVelocity);
	}

	/*static private void SetUserVelocityAction(int objectID, Vector3 newVelocity)
	{
		mCurrentInstance.GetObject<Pathing>(objectID).SetUserVelocity(newVelocity);
	}*/
}
