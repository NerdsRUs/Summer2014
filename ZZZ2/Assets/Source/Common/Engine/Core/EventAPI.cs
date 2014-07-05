using UnityEngine;
using System.Collections;

public class EventAPI : EventAPIBase 
{
	//Can use objectID(int), or the actual Class interchangably
	public void SetUserVelocity(Pathing objectID, Vector3 newVelocity)
	{
		if (mCurrentInstance.IsGraphics())
		{
			NewObjectEventLocalOnly(objectID, "SetUserVelocity", newVelocity);
		}
		/*else
		{
			NewObjectEventServerOnly(objectID, "SetUserVelocity", newVelocity);
		}*/
		//NewEventLocalOnly(mCurrentInstance.GetEngineTime() + EngineManager.CLIENT_DELAY_TIME, "DoObjectFunction", objectID, "SetUserVelocity", newVelocity);

		//NewEventAllRemote(mCurrentInstance.GetEngineTime(), "DoObjectFunction", objectID, "UpdateUserVelocity", newVelocity, objectID.transform.localPosition);

		//Use the above command to create events from functions defined in other objects (must be of type EngineObject) - the type will be detected at runtime
		//Further, this function will automatically convert objectIDs to the object type required by the function (again, must be of type EngineObject to convert)
		

		//If you need some complex control over the command, then define a second private function and Call that, as shown below

		//NewEvent("SetUserVelocityAction", objectID, newVelocity);
	}

	/*static private void SetUserVelocityAction(int objectID, Vector3 newVelocity)
	{
		mCurrentInstance.GetObject<Pathing>(objectID).SetUserVelocity(newVelocity);
	}*/

	public void UpdateMoveVelocity(Pathing objectID, double time, Vector3 newVelocity, Vector3 position)
	{
		//double test = 0;
		
		/*if (!mCurrentInstance.IsServer())
		{
			test = EngineManager.CLIENT_DELAY_TIME;
		}*/

		/*if (newVelocity.sqrMagnitude == 0 && mCurrentInstance.IsServer())
		{
			NewEventAllRemote(mCurrentInstance.GetEngineTime() + test, "DoObjectFunction", objectID, "UpdateUserVelocity", newVelocity, position);
		}
		else
		{
			//NewEventAllRemote(mCurrentInstance.GetEngineTime() + test, "DoObjectFunction", objectID, "UpdateUserVelocity", newVelocity, position);
			NewObjectEventAllRemote(objectID, "UpdateUserVelocity", newVelocity, position);
		}*/

		NewObjectEventAllRemote(objectID, "UpdateUserVelocity", time, newVelocity, position);
	}	

	public void UpdatePhysics(PhysicObject objectID, Vector3 position, Vector3 rotation, Vector3 velocity, float angularVelocity)
	{
		//double test = 0;

		/*if (!mCurrentInstance.IsServer())
		{
			test = EngineManager.CLIENT_DELAY_TIME;
		}*/

		//if (mCurrentInstance.IsServer())
		//{
			//NewEventAllRemote(mCurrentInstance.GetEngineTime() + test, "DoObjectFunction", objectID, "DoUpdate", position, scale, rotation, velocity, angularVelocity);
			NewObjectEventAllRemote(objectID, "DoUpdate", position, rotation, velocity, angularVelocity);
		//}
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
