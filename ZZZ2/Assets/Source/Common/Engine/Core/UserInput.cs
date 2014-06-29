using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EngineManager))]
public class UserInput : MonoBehaviour
{
	Vector2 mCurrentVelocity;
	EngineManager mManager;

	void Start () 
	{
		UICamera.fallThrough = gameObject;
		mManager = GetComponent<EngineManager>();
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
			mManager.GetEventAPI().SetUserVelocity(mManager.GetObjectByTag<Pathing>("LocalPlayer"), movement);

			mCurrentVelocity = movement;
		}

		if (Input.GetKeyDown(KeyCode.Space))
		{
			mManager.GetEventAPI().CloneObject(mManager.GetObjectByTag<Pathing>("LocalPlayer"));
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
