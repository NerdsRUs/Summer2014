using UnityEngine;
using System.Collections;

public class TimelineEventEnable : TimelineEvent
{
	public bool enable = false;
	public Transform removeObject;

	public override bool initialize(float lengthOffset = 0)
	{
		if (removeObject)
		{
			Common.setActive(removeObject, enable);
		}

		return base.initialize(lengthOffset);
	}

#if UNITY_EDITOR

	//Tells the editor if the length is changable in the Timeline editor
	override public bool canModifyLength()
	{
		length = 0;

		return false;
	}

#endif
}
