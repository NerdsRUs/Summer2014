using UnityEngine;
using System.Collections;

public class Script : NewableMonoBehaviour
{
	static public Script NewScript(GameObject parent, EngineManager engineManager, string name = "")
	{
		return NewObject<Script>(parent, engineManager, name);
	}

	public void Init(EngineManager engineManager, string name)
	{
		if (name != "")
		{
			gameObject.name = name;
		}
	}
}
