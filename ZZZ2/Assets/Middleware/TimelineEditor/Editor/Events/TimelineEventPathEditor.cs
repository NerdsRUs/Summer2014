using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(TimelineEventPath))]
public class TimelineEventPathEditor : Editor
{
	TimelineEventPath _target;
	GUIStyle style = new GUIStyle();

	void OnEnable()
	{
		_target = (TimelineEventPath)target;

		style.fontStyle = FontStyle.Bold;
		style.normal.textColor = Color.white;
	}

	void OnSceneGUI()
	{
		if (_target)
		{
			if (_target.transform.GetChildCount() < 2)
			{
				GameObject go = new GameObject("1");
				go.transform.parent = _target.transform;

				go = new GameObject("2");
				go.transform.parent = _target.transform;
			}

			//allow path adjustment undo:
			Undo.SetSnapshotTarget(_target, "Adjust iTween Path");

			if (_target.mNodes != null)
			{
				//node handle display:
				for (int i = 0; i < _target.mNodes.Length; i++)
				{
					Handles.Label(_target.mNodes[i].position, "  '" + _target.mNodes[i].name + "'", style);
					_target.mNodes[i].position = Handles.PositionHandle(_target.mNodes[i].position, Quaternion.identity);
				}
			}
		}
	}
}