using UnityEngine;
using System.Collections;

public class UserInput : EngineObject 
{
	Vector2 mCurrentVelocity;

	/*void Start () 
	{
		UICamera.fallThrough = gameObject;
	}*/
	
	// Update is called once per frame
	void Update () {
		float horizontalMove = Input.GetAxisRaw("Horizontal");
		float verticalMove = Input.GetAxisRaw("Vertical");

		Vector2 movement = new Vector2(horizontalMove, verticalMove);
		movement.Normalize();

		if(movement == mCurrentVelocity) {

		} else {
			AddEvent(() => EventAPI.SetUserVelocity(3, movement));
			mCurrentVelocity = movement;
		}
	}
}
