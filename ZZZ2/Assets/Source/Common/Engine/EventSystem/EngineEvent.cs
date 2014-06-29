using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

public class EngineEvent
{
	float mTime;
	Vector3 mPosition;
	string mFunctionName;
	object[] mParameters;

	static bool mExecutingEvent;

	public void Init(string eventName, float time, Vector3 position, params object[] parameters)
	{
		mTime = time;
		mPosition = position;
		mFunctionName = eventName;
		mParameters = parameters;
	}

	public void Execute(EventManager instance)
	{
		MethodInfo tempMethod = typeof(EventAPI).GetMethod(mFunctionName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);

		if (tempMethod == null)
		{
			Debug.LogError("Event function does not exist: '" + mFunctionName + "'");
			return;
		}

		ParameterInfo[] tempInfo = tempMethod.GetParameters();

		if (mParameters.Length != tempInfo.Length)
		{
			Debug.LogError("Event function parameter coutns don't match: '" + mFunctionName + "'" + tempInfo.Length + " got " + mParameters.Length);
			return;
		}

		EventAPI.SetCurrentInstance(instance);
		mExecutingEvent = true;

		try
		{
			tempMethod.Invoke(null, mParameters);
		}
		catch (Exception e)
		{
			Debug.LogError("Event had error: " + e.InnerException.Message + "/n" + e.InnerException.StackTrace);
		}

		mExecutingEvent = false;
	}

	public string getCommandString()
	{
		return mFunctionName;
	}

	public string getName()
	{
		return mTime.ToString("0.00") + ") " + mFunctionName;
	}

	static public bool isExecutingEvent()
	{
		return mExecutingEvent;
	}
}
