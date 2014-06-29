using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

public class EventAPIBase 
{
	protected EngineManager mCurrentInstance;

	public void Init(EngineManager instance)
	{
		mCurrentInstance = instance;
	}

	protected void DoObjectFunction(int objectID, string functionName, params object[] parameters)
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

	protected void NewEventFromObject(int objectID, string functionName, params object[] paramaters)
	{
		NewEvent("DoObjectFunction", objectID, functionName, paramaters);
	}

	protected void NewEventFromObject(EngineObject objectID, string functionName, params object[] paramaters)
	{
		NewEvent("DoObjectFunction", objectID, functionName, paramaters);
	}

	protected void NewEvent(string functionName, params object[] paramaters)
	{
		mCurrentInstance.AddEvent(functionName, paramaters);

		if (mCurrentInstance.IsServer())
		{
			NewEventOnClient(0, functionName, paramaters);
		}
	}


	protected void NewEventOnClientFromObject(int clientIdentifer, int objectID, string functionName, params object[] paramaters)
	{
		NewEventOnClient(clientIdentifer, "DoObjectFunction", objectID, functionName, paramaters);
	}

	protected void NewEventOnClientFromObject(int clientIdentifer, EngineObject objectID, string functionName, params object[] paramaters)
	{
		NewEventOnClient(clientIdentifer, "DoObjectFunction", objectID, functionName, paramaters);
	}

	protected void NewEventOnClient(int clientIdentifer, string functionName, params object[] paramaters)
	{
		//Send event to client

		Common.getClientManager().GetEventAPI().NewEvent(functionName, paramaters);
	}	
}
