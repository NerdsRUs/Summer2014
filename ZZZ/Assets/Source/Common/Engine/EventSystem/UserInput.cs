using UnityEngine;
using System.Collections;

public class UserInput : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void SetUserVelocity(Vector3 newVelocity)
	{
		rigidbody2D.velocity = newVelocity;
	}
}
