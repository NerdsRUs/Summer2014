using UnityEngine;
using System.Collections;

public class PacketWrapper 
{
	static private void SetUserVelocity(Pathing pathing, Vector3 newVelocity)
	{
		EventManager.GetCurrentInstance().AddEvent("SetUserVelocity", newVelocity);
	}
}
