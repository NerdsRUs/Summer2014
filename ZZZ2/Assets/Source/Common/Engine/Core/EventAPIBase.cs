using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

public class EventAPIBase 
{
	protected EngineManager mCurrentInstance;
	protected NetCode mNetCode;

	public void Init(EngineManager instance)
	{
		mCurrentInstance = instance;

		mNetCode = new NetCode();
		mNetCode.Init("EventAPI", instance.GetComponent<NetworkView>());
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
			if (Common.TypeInheritsFrom(tempInfo[i].ParameterType, typeof(EngineObject)) && parameters[i].GetType() == typeof(int))
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
			Debug.LogError("Event object function had error: " + e.Message + "/n" + e.StackTrace);
		}
	}

	public string GetCommandString(params object[] parameters)
	{
		string functionName = (string)parameters[1];

		int objectID;
		
		if (parameters[0].GetType() == typeof(int))
		{
			objectID  = (int)parameters[0];
		}
		else if (Common.TypeInheritsFrom(parameters[0].GetType(), typeof(EngineObject)))
		{
			objectID = ((EngineObject)parameters[0]).GetObjectID();
		}
		else
		{
			return "Event object function's call object ID is invalid: " + functionName + " " + parameters[0].GetType();
		}

		EngineObject callObject = mCurrentInstance.GetObject<EngineObject>(objectID);

		if (callObject == null)
		{
			return "Event object function's call object does not exist: " + objectID;
		}

		MethodInfo tempMethod = callObject.GetType().GetMethod(functionName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

		if (tempMethod == null)
		{
			return "Event object function does not exist: '" + functionName + "' " + objectID;
		}

		ParameterInfo[] tempInfo = tempMethod.GetParameters();

		if (parameters.Length - 2 != tempInfo.Length)
		{
			return "Event object function parameter counts don't match: '" + functionName + "' " + objectID + " " + tempInfo.Length + " got " + (parameters.Length - 2);
		}

		string command = functionName + "(";

		for (int i = 2; i < parameters.Length; i++)
		{
			if (parameters[i].GetType() == typeof(object[]))
			{
				command += Convert.ChangeType(((object[])parameters[i])[0], tempInfo[i - 2].ParameterType).ToString();
			}
			else
			{
				command += Convert.ChangeType(parameters[i], tempInfo[i - 2].ParameterType).ToString();
			}

			if (i + 1 < parameters.Length)
			{
				command += ", ";
			}
		}

		command += ")";

		return command;
	}

	//Adds event to server and clients
	protected void NewObjectEvent(int objectID, string functionName, params object[] parameters)
	{
		NewEvent("DoObjectFunction", objectID, functionName, parameters);
	}

	protected void NewObjectEvent(EngineObject objectID, string functionName, params object[] parameters)
	{
		NewEvent("DoObjectFunction", objectID, functionName, parameters);
	}

	protected void NewEvent(string functionName, params object[] parameters)
	{
		mCurrentInstance.AddEvent(functionName, parameters);

		//Send to all other clients

		if (mCurrentInstance.IsServer())
		{
			NewEventAllRemote(functionName, parameters);
		}
	}


	//Adds event to server only
	protected void NewObjectEventLocalOnly(int objectID, string functionName, params object[] parameters)
	{
		NewEventLocalOnly("DoObjectFunction", objectID, functionName, parameters);
	}

	protected void NewObjectEventLocalOnly(EngineObject objectID, string functionName, params object[] parameters)
	{
		NewEventLocalOnly("DoObjectFunction", objectID, functionName, parameters);
	}

	protected void NewEventLocalOnly(string functionName, params object[] parameters)
	{
		mCurrentInstance.AddEvent(functionName, parameters);		
	}


	//Adds event to all clients
	protected void NewObjectEventAllRemote(int objectID, string functionName, params object[] parameters)
	{
		NewEventAllRemote("DoObjectFunction", objectID, functionName, parameters);
	}

	protected void NewObjectEventAllRemote(EngineObject objectID, string functionName, params object[] parameters)
	{
		NewEventAllRemote("DoObjectFunction", objectID, functionName, parameters);
	}

	protected void NewEventAllRemote(string functionName, params object[] parameters)
	{
		//Send to all other clients

		NewEventRemote(0, functionName, parameters);
	}


	//Adds event to specified client
	protected void NewObjectEventRemote(int remoteIdentifer, int objectID, string functionName, params object[] parameters)
	{
		NewEventRemote(remoteIdentifer, "DoObjectFunction", objectID, functionName, parameters);
	}

	protected void NewObjectEventRemote(int remoteIdentifer, EngineObject objectID, string functionName, params object[] parameters)
	{
		NewEventRemote(remoteIdentifer, "DoObjectFunction", objectID, functionName, parameters);
	}

	protected void NewEventRemote(int remoteIdentifer, string functionName, params object[] parameters)
	{
		//Send event to client

		Common.getClientManager().GetEventAPI().NewEvent(functionName, parameters);
	}	
}
