using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

/*[CustomEditor(typeof(TimelineEventAnimatedSprite))]
public class TimelineEventAnimatedSpriteEditor : Editor
{
	override public void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		float animationLength = TimelineEventAnimatedSprite.getAnimationLength(((TimelineEventAnimatedSprite)target).animatedObject, ((TimelineEventAnimatedSprite)target).animationName, ((TimelineEventAnimatedSprite)target).recursive);
		GUI.enabled = animationLength > 0;

		if (GUILayout.Button("Suggset length"))
		{
			((TimelineEventAnimatedSprite)target).length = animationLength;
		}

		GUI.enabled = true;
	}
}*/