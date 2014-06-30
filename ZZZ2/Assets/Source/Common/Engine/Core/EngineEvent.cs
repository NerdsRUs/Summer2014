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
	EngineManager mEngineManager;

	static bool mExecutingEvent;

	public void Init(EngineManager instance, string eventName, float time, Vector3 position, params object[] parameters)
	{
		mTime = time;
		mPosition = position;
		mFunctionName = eventName;
		mParameters = parameters;
		mEngineManager = instance;

		for (int i = 0; i < mParameters.Length; i++)
		{
			if (mParameters[i].GetType().BaseType == typeof(EngineObject))
			{
				mParameters[i] = ((EngineObject)mParameters[i]).GetObjectID();
			}
		}
	}

	public void Execute()
	{
		MethodInfo tempMethod = typeof(EventAPI).GetMethod(mFunctionName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);

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

		for (int i = 0; i < tempInfo.Length; i++)
		{
			if (tempInfo[i].ParameterType.BaseType == typeof(EngineObject) && mParameters[i].GetType() == typeof(int))
			{
				mParameters[i] = mEngineManager.GetObject<EngineObject>((int)mParameters[i]);
			}
		}

		mExecutingEvent = true;

		try
		{
			tempMethod.Invoke(mEngineManager.GetEventAPI(), mParameters);
		}
		catch (Exception e)
		{
			Debug.LogError("Event had error: " + e.InnerException.Message + "/n" + e.InnerException.StackTrace);
		}

		mExecutingEvent = false;
	}

	public string GetCommandString()
	{
		if (mFunctionName == "DoObjectFunction")
		{
			return mEngineManager.GetEventAPI().GetCommandString(mParameters);
		}

		MethodInfo tempMethod = typeof(EventAPI).GetMethod(mFunctionName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);

		if (tempMethod == null)
		{
			return "Event function does not exist: '" + mFunctionName + "'";
		}

		ParameterInfo[] tempInfo = tempMethod.GetParameters();

		if (mParameters.Length != tempInfo.Length)
		{
			return "Event function parameter coutns don't match: '" + mFunctionName + "'" + tempInfo.Length + " got " + mParameters.Length;
		}

		string command = mFunctionName + "(";

		for (int i = 0; i < mParameters.Length; i++)
		{
			command += mParameters[i].ToString();

			if (i + 1 < mParameters.Length)
			{
				command += ", ";
			}
		}

		command += ")";

		return command;
	}

	public string GetName()
	{
		if (mFunctionName == "DoObjectFunction")
		{
			return mTime.ToString("0.00") + ") " +  mParameters[0].ToString() + " -> " + mParameters[1].ToString();
		}

		return mTime.ToString("0.00") + ") " + mFunctionName;
	}

	static public bool isExecutingEvent()
	{
		return mExecutingEvent;
	}
}
