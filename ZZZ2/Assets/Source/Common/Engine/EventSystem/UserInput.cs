using UnityEngine;
using System.Collections;

public class UserInput : MonoBehaviour
{
	Vector2 mCurrentVelocity;

	void Start () 
	{
		UICamera.fallThrough = gameObject;
	}
	
	void Update () 
	{
		float horizontalMove = Input.GetAxisRaw("Horizontal");
		float verticalMove = Input.GetAxisRaw("Vertical");

		Vector2 movement = new Vector2(horizontalMove, verticalMove);
		movement.Normalize();

		if(movement == mCurrentVelocity) 
		{

		} 
		else 
		{
			EventAPI.SetUserVelocity(2, movement);

			mCurrentVelocity = movement;
		}
	}
}
