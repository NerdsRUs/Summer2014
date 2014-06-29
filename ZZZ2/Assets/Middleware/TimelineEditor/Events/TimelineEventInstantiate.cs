using UnityEngine;
using System.Collections;

public class TimelineEventInstantiate : TimelineEvent
{
	public Transform instantiateObject;
	public Transform parent;
	public Transform position;

	public override bool initialize(float lengthOffset = 0)
	{
		if (instantiateObject)
		{
			Transform tempTransform = Common.instantiateNewObject(instantiateObject);

			if (tempTransform && parent)
			{
				tempTransform.parent = parent;
			}
			if (tempTransform && position)
			{
				tempTransform.position = position.position;
			}
		}

		return base.initialize(lengthOffset);
	}

#if UNITY_EDITOR

	public override bool canModifyLength()
	{
		length = 0;

		return false;
	}

#endif
}
