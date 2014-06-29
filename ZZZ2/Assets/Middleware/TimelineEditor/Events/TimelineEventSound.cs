using UnityEngine;
using System.Collections;

public class TimelineEventSound : TimelineEvent
{
	public AudioSource audioSource;

	public override bool initialize(float lengthOffset = 0)
	{
		if (audioSource)
		{
			if (lengthOffset > 0)
			{
				audioSource.time = lengthOffset;
			}
			audioSource.Play(0);
		}

		return base.initialize(lengthOffset);
	}

	public override void editorUpdate()
	{
		if (audioSource)
		{
			length = audioSource.clip.length;
		}

		base.editorUpdate();
	}

#if UNITY_EDITOR

	public override bool canModifyLength()
	{
		return false;
	}

#endif
}
