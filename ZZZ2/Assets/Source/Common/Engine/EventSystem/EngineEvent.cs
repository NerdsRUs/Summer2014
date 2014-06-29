using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

public class EngineEvent : MonoBehaviour
{
	public delegate void EventCall();

	float mTime;
	Vector3 mPosition;
	EventCall mAction;

	static bool mExecutingEvent;

	public void Init(EventCall eventCall, float time, Vector3 position)
	{
		mTime = time;
		mPosition = position;
		mAction = eventCall;
	}

	public void Execute(EventManager instance)
	{
		EventAPI.SetCurrentInstance(instance);
		mExecutingEvent = true;

		try
		{
			mAction();
		}
		catch (Exception e)
		{
			Debug.LogError("Event had error: " + e.Message + "/n" + e.StackTrace);
		}

		mExecutingEvent = false;
	}

	public string getCommandString()
	{
		return mAction.Method.Name;
	}

	public string getName()
	{
		return mTime.ToString("0.00") + ") " + mAction.Method.Name;
	}

	static public bool isExecutingEvent()
	{
		return mExecutingEvent;
	}
}
