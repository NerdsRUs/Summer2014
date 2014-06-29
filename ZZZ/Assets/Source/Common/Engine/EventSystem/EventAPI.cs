using UnityEngine;
using System.Collections;

public class EventAPI {
	
	static void SetUserVelocity(int objectID, Vector3 newVelocity)
	{
		EventManger.GetObject<Pathing>(objectID).SetUserVelocity(newVelocity);
	}
}
