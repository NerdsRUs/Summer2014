using UnityEngine;
using System.Collections;

public class TimelineEventTimeline : TimelineEvent
{
	public Timeline timeline;
	public bool waitForEnd;

	public override bool initialize(float lengthOffset = 0)
	{
		if (timeline != null)
		{
			timeline.startPlayback();
			if (lengthOffset > 0)
			{
				timeline.update(startTime / timeline.timeScale, true);
			}

			mTimeline.addSubTimeline(timeline);
		}

		return base.initialize(lengthOffset);
	}

	public override void sample(float samplePoint)
	{
		if (timeline != null)
		{
			timeline.sample(samplePoint);
		}

		base.sample(samplePoint);
	}

	public override bool end()
	{
		if (!waitForEnd)
		{
			timeline.skip();
		}
		else
		{

			if (!timeline.isFinished())
			{
				return true;
			}
		}

		mTimeline.removeSubTimeline(timeline);

		return base.end();
	}

	public override void editorUpdate()
	{
		if (timeline != null)
		{
			if (length == 0)
			{
				length = timeline.length;
			}
		}

		base.editorUpdate();
	}

	override public bool dontSkipOnDiscreet()
	{
		return true;
	}

#if UNITY_EDITOR

	public override bool canModifyLength()
	{
		if (timeline == null)
		{
			length = 0;
		}
		else
		{
			if (length == 0)
			{
				length = timeline.length;
			}
		}

		return true;
	}

#endif
}
