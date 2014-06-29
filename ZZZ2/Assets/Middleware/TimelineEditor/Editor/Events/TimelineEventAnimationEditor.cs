using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(TimelineEventAnimation))]
public class TimelineEventAnimationEditor : Editor
{
	override public void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		float animationLength = TimelineEventAnimation.getAnimationLength(((TimelineEventAnimation)target).animatedObject, ((TimelineEventAnimation)target).animationName, ((TimelineEventAnimation)target).recursive);
		GUI.enabled = animationLength > 0;

		if (GUILayout.Button("Suggset length"))
		{
			((TimelineEventAnimation)target).length = animationLength;
		}

		GUI.enabled = true;
	}
}