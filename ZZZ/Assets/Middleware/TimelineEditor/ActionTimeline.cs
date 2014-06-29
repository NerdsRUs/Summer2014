using UnityEngine;
using System.Collections;

public class ActionTimeline : Timeline
{
	public int priority = 1;

	protected float mFadeOutTime = 0;

	public override void OnEnable()
	{
		base.OnEnable();

		mFadeOutTime = getEventTime(TimelineEventMarker.EVENT.FADE_OUT_TIME);
	}

	override public bool canPlayNext()
	{
		if (mFadeOutTime == 0)
		{
			return isFinished();
		}

		return mCurrentTime >= mFadeOutTime;
	}
}
