using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

public class EngineObject : MonoBehaviour 
{
	public delegate void EventCall<T>(T callObject) where T : EngineObject;

	protected int mID;
	protected EngineManager mInstance = null;
	protected EngineObject mParent = null;

	protected int mRandomSeed = -1;

	static public Script mAPIAccess;

	protected List<object> mParameterList = new List<object>();

	virtual protected string GetHolder()
	{
		return "";
	}	

	virtual protected void Start()
	{
		if (mInstance == null)
		{
			EngineManager instance = Common.getComponentInParent<EngineManager>(transform);

			if (instance == null)
			{
				Debug.LogError("Object " + name + " didn't have a valid EngineInstance in parent");
			}
			else
			{
				InitFromStart(instance);
			}
		}
	}

	static protected T NewObject<T>(EngineObject parent, int objectID, params object[] parameters) where T : EngineObject
	{
		GameObject gameObject = new GameObject();
		T tempObject = gameObject.AddComponent<T>();

		tempObject.InitObject(parent, objectID);

		gameObject.name = typeof(T).Name + "(ID: " + tempObject.GetObjectID() + ")";

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

	virtual public void InitObject(EngineObject parent, int objectID = -1)
	{
		if (mInstance == null)
		{
			SetParent(parent);
			mInstance = parent.GetInstance();

			if (objectID == -1)
			{
				mID = mInstance.GetNextID();
			}
			else
			{
				mID = objectID;
			}
			mInstance.AddEngineObject(this);

			OnInit();
		}

		//checkDefaultScript();
	}

	virtual protected void GetInializationParameters()
	{
	}

	virtual public void SetParent(EngineObject parent)
	{
		mParent = parent;

		if (GetHolder() != "")
		{
			transform.parent = mParent.GetHolder(GetHolder());
		}
		else
		{
			transform.parent = parent.transform;
		}
		transform.localPosition = Vector3.zero;
	}

	virtual public void InitFromStart(EngineManager instance)
	{
		if (mInstance == null)
		{
			mParent = null;
			mInstance = instance;
			mID = mInstance.GetNextID();
			mInstance.AddEngineObject(this);

			gameObject.name = gameObject.name + "(ID: " + mID + ")";

			OnInit();
		}

		//checkDefaultScript();
	}

	void checkDefaultScript()
	{
		if (mAPIAccess == null)
		{
			mAPIAccess = Script.NewScript(mInstance.gameObject, mInstance, "DefaultAPIAccessScript");
		}
	}

	public Transform GetHolder(string name)
	{
		Transform tempTransform = transform.FindChild(name);
		if (tempTransform == null)
		{
			GameObject gameObject = new GameObject();

			gameObject.name = name;
			gameObject.transform.parent = transform;
			gameObject.transform.localPosition = Vector3.zero;

			tempTransform = gameObject.transform;
		}

		return tempTransform;
	}

	virtual public void OnDestroy()
	{
		if (mInstance != null)
		{
			mInstance.RemoveEngineObject(this);
			mInstance = null;
			mID = 0;
		}
	}

	virtual public void OnInit()
	{
		if (mRandomSeed == -1)
		{
			mRandomSeed = Common.Range(0, Common.BIG_NUMBER);
		}
	}

	public int GetObjectID()
	{
		return mID;
	}

	public EngineManager GetInstance()
	{
		return mInstance;
	}

	public void AddEvent(string functionName, params object[] parameters)
	{
		EngineEvent newEvent = new EngineEvent();

		newEvent.Init(functionName, Time.time, transform.position, parameters);

		mInstance.MakeEvent(newEvent);
	}

	virtual public bool executeCommand(string script)
	{
		ScriptLoader.executeCommands(this, script);

		return true;
	}

	protected int getNextRandom()
	{
		mRandomSeed = Common.getRandomValueFromSeed(mRandomSeed);

		return mRandomSeed;
	}

	protected float getNextRandomFloat()
	{
		mRandomSeed = Common.getRandomValueFromSeed(mRandomSeed);

		return (float)(mRandomSeed % Common.BIG_NUMBER) / (float)Common.BIG_NUMBER;
	}

	protected int Range(int x, int y)
	{
		return (int)(x + (y - x) * getNextRandomFloat());
	}

	protected float Range(float x, float y)
	{
		return (float)(x + (y - x) * getNextRandomFloat());
	}

	public EngineObject getParent()
	{
		return mParent;
	}
}
