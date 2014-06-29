using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TimelineEventPosition : TimelineEvent
{
	public Transform moveObject;
	public Transform attachTransform;
	public string transformName = "";

	public bool matchRotation = false;
	public bool matchScale = false;

	protected override void Start()
	{
		base.Start();
	}

	public override bool initialize(float lengthOffset = 0)
	{
		if (attachTransform == null)
		{
			attachTransform = AttachableTransform.getTransform(transformName);
		}

		if (moveObject != null && attachTransform)
		{
			moveObject.transform.position = attachTransform.transform.position;

			if (matchRotation)
			{
				moveObject.transform.rotation = attachTransform.transform.rotation;
			}
			if (matchScale)
			{
				moveObject.transform.localScale = attachTransform.transform.localScale;
			}
		}

		return base.initialize(lengthOffset);
	}

	public override bool update(float deltaTime)
	{
		if (moveObject != null && attachTransform)
		{
			moveObject.transform.position = attachTransform.transform.position;

			if (matchRotation)
			{
				moveObject.transform.rotation = attachTransform.transform.rotation;
			}
			if (matchScale)
			{
				moveObject.transform.localScale = attachTransform.transform.localScale;
			}
		}

		return base.update(deltaTime);
	}

	public override void sample(float samplePoint)
	{
#if UNITY_EDITOR
		if (attachTransform == null)
		{
			attachTransform = AttachableTransform.getTransform(transformName);
		}

		if (moveObject != null && attachTransform)
		{
			moveObject.transform.position = attachTransform.transform.position;

			if (matchRotation)
			{
				moveObject.transform.rotation = attachTransform.transform.rotation;
			}
			if (matchScale)
			{
				moveObject.transform.localScale = attachTransform.transform.localScale;
			}
		}
#endif

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
