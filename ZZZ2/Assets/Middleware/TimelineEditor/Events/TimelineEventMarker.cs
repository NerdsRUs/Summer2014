using UnityEngine;
using System.Collections;

public class TimelineEventMarker : TimelineEvent
{
	public EVENT eventName;

	public enum EVENT
	{
		DO_SKILL,
		SHOW_DAMAGE,
		FADE_OUT_TIME,
		REMOVE,
		SHOW_UI,
	}

	public override bool canModifyLength()
	{
		length = 0;

		return false;
	}
}
