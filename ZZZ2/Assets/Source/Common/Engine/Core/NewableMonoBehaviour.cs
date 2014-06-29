using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

public class NewableMonoBehaviour : MonoBehaviour 
{
	virtual protected string GetHolder()
	{
		return "";
	}

	static protected T NewObject<T>(GameObject parent, params object[] parameters) where T : NewableMonoBehaviour
	{
		GameObject gameObject = new GameObject();
		T tempObject = gameObject.AddComponent<T>();
		tempObject.SetParent(parent);

		gameObject.name = typeof(T).Name + "(Non-Synced)";

		tempObject.InitByParameters(parameters);

		return tempObject;
	}

	private void InitByParameters(params object[] parameters)
	{
		MethodInfo tempMethod = this.GetType().GetMethod("Init", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

		if (tempMethod == null)
		{
			Debug.LogError("Object initialization does not exist: '" + this.GetType() + ".Init" + "'");
			return;
		}

		ParameterInfo[] tempInfo = tempMethod.GetParameters();

		if (parameters.Length != tempInfo.Length)
		{
			Debug.LogError("Object initialization parameter coutns don't match: '" + this.GetType() + ".Init" + "' (" + tempInfo.Length + ") got " + parameters.Length);
			return;
		}

		try
		{
			tempMethod.Invoke(this, parameters);
		}
		catch (Exception e)
		{
			Debug.LogError("Object initialization had error: " + e.InnerException.Message + "/n" + e.InnerException.StackTrace);
		}
	}

	/*virtual protected void GetInializationParameters()
	{
	}*/

	virtual public void SetParent(GameObject parent)
	{
		if (GetHolder() != "")
		{
			transform.parent = GetParentHolder(parent, GetHolder());
		}
		else
		{
			transform.parent = parent.transform;
		}
		transform.localPosition = Vector3.zero;
	}

	public Transform GetParentHolder(GameObject parent, string name)
	{
		Transform tempTransform = null;

		if (parent != null)
		{
			tempTransform = parent.transform.FindChild(name);
			if (tempTransform == null)
			{
				GameObject gameObject = new GameObject();

				gameObject.name = name;
				gameObject.transform.parent = transform;
				gameObject.transform.localPosition = Vector3.zero;

				tempTransform = gameObject.transform;
			}
		}

		return tempTransform;
	}
}
