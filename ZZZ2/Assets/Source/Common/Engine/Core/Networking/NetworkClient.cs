using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

public class NetworkClient : NewableMonoBehaviour
{
	protected override string GetHolder()
	{
		return "NetworkClients";
	}

	static public Script NewScript(GameObject parent, EngineManager engineManager, string name = "")
	{
		return NewObject<Script>(parent, engineManager, name);
	}

	public void Init(EngineManager EngineManager, string name)
	{
		if (name != "")
		{
			gameObject.name = name;
		}
	}
}
