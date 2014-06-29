using UnityEngine;
using System.Collections;

public class Pathing : EngineObject 
{
	public void SetUserVelocity(Vector3 newVelocity)
	{
		rigidbody2D.velocity = newVelocity;
	}
}
