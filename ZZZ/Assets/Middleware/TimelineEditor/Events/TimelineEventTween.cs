using UnityEngine;
using System.Collections;

public class TimelineEventTween : TimelineEvent
{
	public Transform tweenTransform;
	public Vector3 startPosition;
	public Vector3 endPosition;

	public override void sample(float samplePoint)
	{


		base.sample(samplePoint);
	}

#if UNITY_EDITOR

	//Tells the editor if the length is changable in the Timeline editor
	override public bool canModifyLength()
	{
		return true;
	}

#endif
}
