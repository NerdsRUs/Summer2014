using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class PhysicObject : SyncObject 
{
	void OnCollisionEnter2D()
	{
		ForceUpdate();
	}

	void OnCollisionExit2D()
	{
		ForceUpdate();
	}

	protected override bool CheckIsAtRest()
	{
		return false;// rigidbody2D.velocity.sqrMagnitude == 0 && Mathf.Abs(rigidbody2D.angularVelocity) == 0;
	}

	override protected void DoSyncData()
	{
		GetEventAPI().UpdatePhysics(this, transform.localPosition, transform.localScale, transform.localRotation.eulerAngles, rigidbody2D.velocity, rigidbody2D.angularVelocity);
	}

	void DoUpdate(Vector3 position, Vector3 scale, Vector3 rotation, Vector3 velocity, float angularVelocity)
	{
		transform.localPosition = position;
		transform.localScale = scale;

		Quaternion tempRotation = transform.localRotation;
		tempRotation.eulerAngles = rotation;
		transform.localRotation = tempRotation;

		rigidbody2D.velocity = velocity;
		rigidbody2D.angularVelocity = angularVelocity;
	}
}
