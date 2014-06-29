using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(TimelineEvent))]
public class TimelineEventEditor : Editor
{
	override public void OnInspectorGUI()
	{
		//UnityEngine.Object newObject;
		//newObject = EditorGUILayout.ObjectField("Script", target, typeof(TimelineEvent));

		base.OnInspectorGUI();

		/*if (GUI.changed)
		{
			TimelineEditor.setDirty();
		}*/
	}
}