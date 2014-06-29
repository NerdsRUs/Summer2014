using UnityEngine;
using System.Collections;

public class TweenTimeline : Timeline
{
	public float moveSpeed = 0;

	protected Transform mStartPosition;
	protected Transform mEndPosition;

	public void setPositions(Transform startPosition, Transform endPosition)
	{
		mStartPosition = startPosition;
		mEndPosition = endPosition;

		if (moveSpeed > 0)
		{
			float distance = Vector3.Distance(mStartPosition.position, mEndPosition.position);

			float totalTime = distance / moveSpeed;

			timeScale = mLength / totalTime;
		}
	}

	public Transform getStart()
	{
		return mStartPosition;
	}

	public Transform getEnd()
	{
		return mEndPosition;
	}

	override public bool canUseEvent(string eventName)
	{
		if (eventName.StartsWith("TweenTimelineEvent"))
		{
			return true;
		}

		return base.canUseEvent(eventName);
	}
}
