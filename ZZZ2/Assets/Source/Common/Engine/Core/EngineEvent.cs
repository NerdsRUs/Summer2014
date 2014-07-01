using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

public class EngineEvent
{
	double mTime;
	Vector3 mPosition;
	string mFunctionName;
	object[] mParameters;
	EngineManager mEngineManager;

	static bool mExecutingEvent;

	public static void ConvertParameters(EngineManager manager, ParameterInfo[] parameterInfo, ref object[] parameters)
	{
		for (int i = 0; i < parameterInfo.Length; i++)
		{
			if (Common.TypeInheritsFrom(parameterInfo[i].ParameterType, typeof(EngineObject)) && parameters[i].GetType() == typeof(int))
			{
				parameters[i] = manager.GetObject<EngineObject>((int)parameters[i]);
			}

			if (parameterInfo[i].ParameterType == typeof(object[]))
			{
				object[] tempObjects = new object[parameters.Length - i];

				for (int j = 0; j < tempObjects.Length; j++)
				{
					tempObjects[j] = parameters[i + j];
				}

				parameters[i] = tempObjects;

				object[] newParameters = new object[i + 1];
				for (int j = 0; j < newParameters.Length; j++)
				{
					newParameters[j] = parameters[j];
				}

				parameters = newParameters;
			}
		}
	}

	public void Init(EngineManager instance, string eventName, float time, Vector3 position, params object[] parameters)
	{
		mTime = time;
		mPosition = position;
		mFunctionName = eventName;
		mParameters = parameters;
		mEngineManager = instance;
	}

	public void Execute()
	{
		MethodInfo tempMethod = typeof(EventAPI).GetMethod(mFunctionName, BindingFlags.OptionalParamBinding | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);

		if (tempMethod == null)
		{
			Debug.LogError("Event(" + mFunctionName + ") function does not exist: '" + mFunctionName + "'");
			return;
		}

		ParameterInfo[] tempInfo = tempMethod.GetParameters();
		bool isSubFunction = false;

		for (int i = 0; i < tempInfo.Length; i++)
		{
			if (tempInfo[i].ParameterType == typeof(object[]))
			{
				isSubFunction = true;
			}
		}

		if (!isSubFunction && mParameters.Length != tempInfo.Length)
		{
			Debug.LogError("Event(" + mFunctionName + ") function parameter coutns don't match: '" + mFunctionName + "'" + tempInfo.Length + " got " + mParameters.Length);
			return;
		}

		ConvertParameters(mEngineManager, tempInfo, ref mParameters);

		mExecutingEvent = true;

		try
		{
			tempMethod.Invoke(mEngineManager.GetEventAPI(), mParameters);
		}
		catch (Exception e)
		{
			if (e.InnerException != null)
			{
				Debug.LogError("Event(" + mFunctionName + ") had error: " + e.InnerException.Message + "/n" + e.InnerException.StackTrace);
			}
			else
			{
				Debug.LogError("Event(" + mFunctionName + ") had error: " + e.Message + "/n" + e.StackTrace);
			}
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

	public double GetTime()
	{
		return mTime;
	}
}
