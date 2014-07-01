using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

public class EventAPIBase : NetCode
{
	public NetworkView networkView;

	protected EngineManager mCurrentInstance;
	protected NetCode mNetCode;

	NetcodeTerminal mOldTerminal;

	public void Init(EngineManager instance)
	{
		mCurrentInstance = instance;

		Init(null, null, "", null);
	}

	public void MakeOffline()
	{
		if (mOldTerminal != null)
		{
			GameObject.Destroy(mOldTerminal);

			mOldTerminal = null;
		}

		Init(null, null, "", null);
	}

	public void MakeClient()
	{
		if (mOldTerminal != null)
		{
			GameObject.Destroy(mOldTerminal);
		}

		mOldTerminal = networkView.gameObject.AddComponent<ClientTerminal>();
		mOldTerminal.Init(mNetCode);

		Init(this, mOldTerminal, "ServerRPC", networkView);
	}

	public void MakeServer()
	{
		if (mOldTerminal != null)
		{
			GameObject.Destroy(mOldTerminal);
		}

		mOldTerminal = networkView.gameObject.AddComponent<ServerTerminal>();
		mOldTerminal.Init(mNetCode);

		Init(this, mOldTerminal, "ClientRPC", networkView);
	}

	void NormalizeParameters(ref object[] parameters)
	{
		object[] newParameters;

		for (int i = 0; i < parameters.Length; i++)
		{
			if (parameters[i].GetType() == typeof(object[]))
			{
				object[] nestedParameters = (object[])parameters[i];

				NormalizeParameters(ref nestedParameters);

				newParameters = new object[parameters.Length + nestedParameters.Length - 1];

				for (int j = 0; j < newParameters.Length; j++)
				{
					if (j < i)
					{
						newParameters[j] = parameters[j];
					}
					else if (j >= i && j < i + nestedParameters.Length)
					{
						newParameters[j] = nestedParameters[j - i];
					}
					else
					{
						newParameters[j] = parameters[j - nestedParameters.Length];
					}
				}

				parameters = newParameters;
			}
			else
			{
				if (Common.TypeInheritsFrom(parameters[i].GetType(), typeof(EngineObject)))
				{
					parameters[i] = ((EngineObject)parameters[i]).GetObjectID();
				}
			}
		}
	}

	protected void DoObjectFunction(int objectID, string functionName, params object[] parameters)
	{
		for (int i = 0; i < parameters.Length; i++)
		{
			Debug.Log(i + " -> " + parameters[i]);
		}

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

		//Network version sends data as a byte array, need to parse it based on the sub function
		if (parameters[0].GetType() == typeof(byte[]))
		{
			parameters = mNetCode.GetParameterListFromBytes(tempMethod, (byte[])parameters[0]);
		}

		if (parameters.Length != tempInfo.Length)
		{
			Debug.LogError("Event object function parameter counts don't match: '" + functionName + "' " + objectID + " expected " + tempInfo.Length + " got " + parameters.Length);
			return;
		}

		try
		{
			tempMethod.Invoke(callObject, parameters);
		}
		catch (Exception e)
		{
			if (e.InnerException != null)
			{
				Debug.LogError("Event object function had error: " + e.InnerException.Message + "/n" + e.InnerException.StackTrace);
			}
			else
			{
				Debug.LogError("Event object function had error: " + e.Message + "/n" + e.StackTrace);
			}			
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
			return "Event object function parameter counts don't match: '" + functionName + "' " + objectID + " expected " + tempInfo.Length + " got " + (parameters.Length - 2);
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
		NormalizeParameters(ref parameters);
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
		NormalizeParameters(ref parameters);
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

		if (EngineManager.mLocalManagers.Count > 1)
		{
			for (int i = 0; i < EngineManager.mLocalManagers.Count; i++)
			{
				if (EngineManager.mLocalManagers[i] != mCurrentInstance)
				{
					EngineManager.mLocalManagers[i].GetEventAPI().NewEventLocalOnly(functionName, parameters);
				}
			}
		}
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
		NormalizeParameters(ref parameters);

		MakeRPC(RPCMode.Others, functionName, parameters);
	}

	void MakeRPC(RPCMode mode, params object[] parameters)
	{
		if (mOldTerminal != null)
		{
			DoRPC("NewEventLocalOnly", RPCMode.Others, parameters);
		}
	}
}
