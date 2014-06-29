using UnityEngine;
using System.Collections;

public class TimelineEventShake : TimelineEvent
{
	static float X_COEFFICIENT = 1.02f;
	static float Y_COEFFICIENT = 2.32f;
	static float Z_COEFFICIENT = 2.82f;

	public Transform transform;
	public bool camera;
	public Vector3 maximums;
	public float speed = 1.0f;

	protected Vector3 mStartPosition;

	protected Perlin mPerlin = new Perlin();

	protected float mTime = 0;

	public override bool initialize(float lengthOffset = 0)
	{
		mStartPosition = getTransform().position;

		return base.initialize(lengthOffset);
	}

	public override bool end()
	{
		getTransform().position = mStartPosition;

		return base.end();
	}

	public override bool update(float deltaTime)
	{
		mTime += deltaTime * speed;

		if (getTransform())
		{
			Vector3 position = new Vector3(mPerlin.Noise(mTime * X_COEFFICIENT), mPerlin.Noise((mTime + getLength()) * Y_COEFFICIENT), mPerlin.Noise((mTime + getLength() * 2)) * Z_COEFFICIENT);
			position.x *= maximums.x;
			position.y *= maximums.y;
			position.z *= maximums.z;

			getTransform().position = mStartPosition + position;
		}

		return base.update(deltaTime);
	}

	protected Transform getTransform()
	{
		if (camera)
		{
			return Camera.mainCamera.transform;
		}

		return transform;
	}
}
