using UnityEngine;
using System.Collections;

public class UserInput : MonoBehaviour 
{
	void Start () 
	{
		UICamera.fallThrough = gameObject;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void SetUserVelocity(Vector3 newVelocity)
	{
		rigidbody2D.velocity = newVelocity;
	}
}
