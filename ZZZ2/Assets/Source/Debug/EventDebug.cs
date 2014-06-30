using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EventDebug : MonoBehaviour
{
	int mEventCount = 0;

	public void addEvent(EngineEvent newEvent)
	{
		GameObject gameObject = new GameObject();
		EventDebugObject tempObject = gameObject.AddComponent<EventDebugObject>();

		tempObject.mStackTrace = Environment.StackTrace;
		tempObject.mCommand = newEvent.GetCommandString();

		tempObject.name = newEvent.GetName();

		gameObject.transform.parent = transform;
		gameObject.transform.localPosition = Vector3.zero;

		mEventCount++;
	}

	public int GetEventCount()
	{
		return mEventCount;
	}

	/*public void addEventStart(EngineEvent newEvent)
	{
		GameObject gameObject = new GameObject();
		EventDebugObject tempObject = gameObject.AddComponent<EventDebugObject>();

		tempObject.mStackTrace = Environment.StackTrace;
		tempObject.mCommand = newEvent.getCommandString();

		tempObject.name = newEvent.getName();

		gameObject.transform.parent = transform;
		gameObject.transform.localPosition = Vector3.zero;
	}*/
}
