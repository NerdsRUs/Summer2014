using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(TimelineEventParticles))]
public class TimelineEventParticlesEditor : Editor
{
	override public void OnInspectorGUI()
	{
		ParticleSystem tempSystem = ((TimelineEventParticles)target).particleSystem;

		if (tempSystem != null)
		{
			float animationLength = tempSystem.duration;
			GUI.enabled = animationLength > 0;

			if (GUILayout.Button("Suggset length"))
			{
				((TimelineEventParticles)target).length = animationLength;
			}
		}

		GUI.enabled = true;

		base.OnInspectorGUI();		
	}
}