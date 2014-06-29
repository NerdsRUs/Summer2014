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
			EventAPI.SetUserVelocity(Common.GetObjectIDByTag<Pathing>("LocalPlayer"), movement);

			mCurrentVelocity = movement;
		}
	}

	//These events fall through from the NGUI system
	void OnPress(bool isDown)
	{
	}

	void OnClick()
	{
	}

	void OnHover(bool isOver)
	{
	}
}
