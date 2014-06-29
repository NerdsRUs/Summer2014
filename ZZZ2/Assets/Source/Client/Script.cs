using UnityEngine;
using System.Collections;

public class Script : NewableMonoBehaviour
{
	static public Script NewScript(GameObject parent, EngineManager EngineManager, string name = "")
	{
		return NewObject<Script>(parent, EngineManager, name);
	}

	public void Init(EngineManager EngineManager, string name)
	{
		if (name != "")
		{
			gameObject.name = name;
		}
	}
}
