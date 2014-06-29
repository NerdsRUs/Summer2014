using UnityEngine;
using System.Collections;

public class EngineObject : MonoBehaviour 
{
	public delegate void EventCall<T>(T callObject) where T : EngineObject;

	protected int mID;
	protected EventManager mInstance = null;
	protected EngineObject mParent = null;

	protected int mRandomSeed = -1;

	static public Script mAPIAccess;

	virtual protected void Start()
	{
		if (mInstance == null)
		{
			EventManager instance = Common.getComponentInParent<EventManager>(transform);

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

	virtual protected string GetHolder()
	{
		return "";
	}

	virtual public void Init(EngineObject parent, int objectID = -1)
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

		if (mAPIAccess == null)
		{
			mAPIAccess = new Script();
			mAPIAccess.Init(GetInstance(), -1);
		}
	}

	virtual public void SetParent(EngineObject parent)
	{
		mParent = parent;

		if (GetHolder() != "")
		{
			transform.parent = mParent.GetHolder(GetHolder());
		}
		transform.localPosition = Vector3.zero;
	}

	virtual public void InitFromStart(EventManager instance)
	{
		if (mInstance == null)
		{
			mParent = null;
			mInstance = instance;
			mID = mInstance.GetNextID();
			mInstance.AddEngineObject(this);

			OnInit();
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

	public EventManager GetInstance()
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
