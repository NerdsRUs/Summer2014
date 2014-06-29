using UnityEngine;
using System.Collections;

public class EventAPIBase 
{
	static protected EventManager mCurrentInstance;

	static public void SetCurrentInstance(EventManager instance)
	{
		mCurrentInstance = instance;
	}

	static public void Call(string functionName, params object[] paramters)
	{
		EventManager.GetCurrentInstance().AddEvent(functionName, paramters);
	}
}
