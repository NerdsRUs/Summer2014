using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

public class EventAPIBase 
{
	static protected EventManager mCurrentInstance;

	static protected void DoObjectFunction(int objectID, string functionName, params object[] parameters)
	{
		EngineObject callObject = mCurrentInstance.GetObject<EngineObject>(objectID);

		if (callObject == null)
		{
			Debug.LogError("Event object function's call object does not exist: " + objectID);
			return;
		}

		MethodInfo tempMethod = callObject.GetType().GetMethod(functionName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

		if (tempMethod == null)
		{
			Debug.LogError("Event object function does not exist: '" + functionName + "' " + objectID);
			return;
		}

		ParameterInfo[] tempInfo = tempMethod.GetParameters();

		if (parameters.Length != tempInfo.Length)
		{
			Debug.LogError("Event object function parameter counts don't match: '" + functionName + "' " + objectID + " " + tempInfo.Length + " got " + parameters.Length);
			return;
		}

		for (int i = 0; i < tempInfo.Length; i++)
		{
			if (tempInfo[i].ParameterType.BaseType == typeof(EngineObject) && parameters[i].GetType() == typeof(int))
			{
				parameters[i] = mCurrentInstance.GetObject<EngineObject>((int)parameters[i]);
			}
		}

		try
		{
			tempMethod.Invoke(callObject, parameters);
		}
		catch (Exception e)
		{
			Debug.LogError("Event object function had error: " + e.InnerException.Message + "/n" + e.InnerException.StackTrace);
		}
	}

	static public void SetCurrentInstance(EventManager instance)
	{
		mCurrentInstance = instance;
	}

	static public void CallOnObject(int objectID, string functionName, params object[] paramters)
	{
		Call("DoObjectFunction", objectID, functionName, paramters);
	}

	static public void Call(string functionName, params object[] paramters)
	{
		EventManager.GetCurrentInstance().AddEvent(functionName, paramters);
	}
}
