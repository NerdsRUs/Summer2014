using UnityEngine;
using System.Collections;

public class TimelineEventAnimation : TimelineEvent
{
	public Transform animatedObject;
	public string animationName;
	public float fadeTime = 0.5f;
	public bool recursive = true;
	public bool restart = false;
	public WrapMode wrapMode = WrapMode.Once;

	protected float mAnimationLength = -1;
	protected float mAnimationSpeed = 1;

	public override bool initialize(float lengthOffset = 0)
	{
		if (animatedObject != null)
		{
			mAnimationSpeed = getAnimationLength() / getLength();

			float time = Common.getAnimationTime(animatedObject, animationName);

			float tempFadeTime = Mathf.Max(0, Mathf.Min(getLength() / 2, fadeTime));

			playAnimation(animatedObject, animationName, fadeTime, mAnimationSpeed, recursive, restart, wrapMode);
		}

		return base.initialize(lengthOffset);
	}

	public override void sample(float samplePoint)
	{
		if (samplePoint >= 0 && samplePoint < getLength() && !Application.isPlaying)
		{
			if (animatedObject)
			{
				sampleAnimation(animatedObject, animationName, getAnimationLength() * samplePoint / getLength(), recursive);
			}
		}

		base.sample(samplePoint);
	}

	protected float getAnimationSpeed()
	{
		if (animatedObject)
		{
			return getAnimationLength() / length;
		}

		return 0;
	}

	protected float getAnimationLength()
	{
#if UNITY_EDITOR
		mAnimationLength = TimelineEventAnimation.getAnimationLength(animatedObject, animationName, recursive);
#else
		//Initialize length
		if (mAnimationLength == -1 && animatedObject != null)
		{
			mAnimationLength = TimelineEventAnimation.getAnimationLength(animatedObject, animationName, recursive);
		}
#endif

		return mAnimationLength;
	}

	public override float getLength()
	{


		return base.getLength();
	}





	static public float getAnimationLength(Transform transform, string animation, bool recursive)
	{
		if (!transform)
		{
			return 0;
		}

		float maxLength = 0;

		Animation tempAnimation = transform.animation;

		if (tempAnimation != null && tempAnimation[animation])
		{
			maxLength = tempAnimation[animation].length;
		}

		if (recursive)
		{
			foreach (Transform child in transform)
			{
				maxLength = Mathf.Max(maxLength, getAnimationLength(child, animation, recursive));
			}
		}

		return maxLength;
	}

	static public void playAnimation(Transform transform, string animation, float fadeTime, float speed, bool recursive, bool restart, WrapMode wrapMode)
	{
		if (!transform)
		{
			return;
		}

		Animation tempAnimation = transform.animation;

		if (tempAnimation != null && tempAnimation[animation])
		{
			tempAnimation[animation].speed = speed;

			if (restart)
			{
				tempAnimation[animation].time = 0;
			}
				
			tempAnimation[animation].wrapMode = wrapMode;

			tempAnimation.CrossFade(animation, fadeTime);
		}

		if (recursive)
		{
			foreach (Transform child in transform)
			{
				playAnimation(child, animation, fadeTime, speed, recursive, restart, wrapMode);
			}
		}
	}

	static public void sampleAnimation(Transform transform, string animation, float sampleTime, bool recursive)
	{
		Animation tempAnimation = transform.animation;

		if (tempAnimation != null && tempAnimation[animation])
		{
			tempAnimation.gameObject.SampleAnimation(tempAnimation[animation].clip, sampleTime);
		}

		if (recursive)
		{
			foreach (Transform child in transform)
			{
				sampleAnimation(child, animation, sampleTime, recursive);
			}
		}
	}
}
