using UnityEngine;
using System.Collections;

public class EventAPI : EventAPIBase 
{
	//Can use objectID(int), or the actual Class interchangably
	public void SetUserVelocity(Pathing objectID, Vector3 newVelocity)
	{
		NewObjectEventLocalOnly(objectID, "SetUserVelocity", newVelocity);

		//Use the above command to create events from functions defined in other objects (must be of type EngineObject) - the type will be detected at runtime
		//Further, this function will automatically convert objectIDs to the object type required by the function (again, must be of type EngineObject to convert)
		

		//If you need some complex control over the command, then define a second private function and Call that, as shown below

		//NewEvent("SetUserVelocityAction", objectID, newVelocity);
	}

	/*static private void SetUserVelocityAction(int objectID, Vector3 newVelocity)
	{
		mCurrentInstance.GetObject<Pathing>(objectID).SetUserVelocity(newVelocity);
	}*/

	public void UpdateMoveVelocity(Pathing objectID, Vector3 newVelocity, Vector3 position)
	{
		NewObjectEventAllRemote(objectID, "UpdateUserVelocity", newVelocity, position);
	}	

	public void UpdatePhysics(PhysicObject objectID, Vector3 position, Vector3 scale, Vector3 rotation, Vector3 velocity, float angularVelocity)
	{
		NewObjectEvent(objectID, "DoUpdate", position, scale, rotation, velocity, angularVelocity);
	}

	//Test junk
	public void CloneObject(Pathing pathing)
	{
		NewEvent("CloneObjectAction", pathing);
	}

	private void CloneObjectAction(Pathing pathing)
	{
		SampleObject.NewSampleObject(pathing, -1, 1, 2);
	}
}
