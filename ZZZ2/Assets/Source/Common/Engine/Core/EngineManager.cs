using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EngineManager : EngineObject 
{
	public static double CLIENT_DELAY_TIME = 0.35f;
	static double UPDATE_TICK = 0.1f;

	[SerializeField]
	bool isGraphical;

	Pool<EngineObject> mGameObjects = new Pool<EngineObject>();
	List<EngineEvent> mCurrentEvents = new List<EngineEvent>();
	List<EngineEvent> mPastEvents = new List<EngineEvent>();

	EngineEvent mCurrentEvent;

	//static EngineManager mCurrentInstance;

	static bool mInitialize = false;
	static bool mDataLoaded = false;
	static bool mIsLoading = false;

	double mLastUpdateTime = -UPDATE_TICK;
	double mTickTime = -UPDATE_TICK;

	EventAPI mEventAPI;

	bool mIsServer;
	bool mIsGraphics;

	[SerializeField]
	private bool useDebugger;
	EventDebug mEventDebug;

	[SerializeField]
	private EngineManager graphicalManager;
	private EngineManager mSyncedClient;

	static public List<EngineManager> mLocalManagers = new List<EngineManager>();

	//Test stuff

	void Start()
	{
		mIsGraphics = isGraphical;

		Init();

		//mCurrentInstance = this;

		if (!mDataLoaded)
		{
			mDataLoaded = true;

			DataManager.LoadData();
		}

		Application.runInBackground = true;

		InitStartUpObjects();

		if (useDebugger)
		{
			GameObject gameObject = new GameObject();

			gameObject.transform.parent = transform;
			gameObject.name = "EventDebug";

			mEventDebug = gameObject.AddComponent<EventDebug>();
		}

		if (graphicalManager != null)
		{
			graphicalManager.SetSyncedClient(this);
		}
	}

	public void Init()
	{
		if (mInstance == null)
		{
			mParent = this;
			mInstance = this;
			mID = mInstance.GetNextID();
			mInstance.AddEngineObject(this);

			OnInit();

			if (!IsGraphics())
			{
				mLocalManagers.Add(this);
			}
		}
	}

	public override void OnInit()
	{
		mEventAPI = GetComponentInChildren<EventAPI>();
		mEventAPI.Init(this);

		base.OnInit();
	}

	void Update()
	{
		if (!mIsLoading)
		{
			if (!mInitialize)
			{
				mInitialize = true;
			}

			DoEvents();

			double elaspedTime = GetEngineTime() - mLastUpdateTime;

			while (elaspedTime > UPDATE_TICK)
			{
				elaspedTime -= UPDATE_TICK;

				mTickTime += UPDATE_TICK;
				mLastUpdateTime += UPDATE_TICK;

				ProcessTick();
			}
		}
	}

	void ProcessTick()
	{
	}

	void DoEvents()
	{
		while (mCurrentEvents.Count > 0 && GetEngineTime() >= mCurrentEvents[0].GetTime())
		{
			if (mEventDebug != null)
			{
				mEventDebug.addEvent(mCurrentEvents[0]);
				mEventDebug.name = "EventDebug (" + mEventDebug.GetEventCount() + ")";
			}

			mCurrentEvent = mCurrentEvents[0];

			mCurrentEvents[0].Execute();

			mCurrentEvent = null;

			mCurrentEvents.RemoveAt(0);
		}
	}

	public double GetEngineTime()
	{
		/*if (!IsServer() && IsGraphics())
		{
			return Time.time - CLIENT_DELAY_TIME;
		}*/

		return Time.time;
	}

	public void AddEngineObject(EngineObject tempObject)
	{
		mGameObjects.addAtIndex(tempObject.GetObjectID(), tempObject);
	}

	public void RemoveEngineObject(EngineObject tempObject)
	{
		mGameObjects.removeFromIndex(tempObject.GetObjectID());
	}

	public int GetNextID()
	{
		return mGameObjects.getUnsedID();
	}

	public int ReserveNextID()
	{
		int nextID = mGameObjects.getUnsedID();

		mGameObjects.reserveIndex(nextID);

		return nextID;
	}

	//Has built-in sanity checks
	public T GetObject<T>(int objectID) where T : EngineObject
	{
		if (objectID <= 0)
		{
			return (T)(EngineObject)this;
		}

		if (mGameObjects.contains(objectID) && mGameObjects[objectID] is T)
		{
			return (T)mGameObjects[objectID];
		}

		throw new System.Exception("Invalid object ID: " + objectID + ", Type: " + typeof(T));
	}

	/*static public EngineManager GetCurrentInstance()
	{
		return mCurrentInstance;
	}*/

	public void MakeEvent(EngineEvent callEvent)
	{
		mCurrentEvents.Add(callEvent);

		mCurrentEvents.Sort(delegate(EngineEvent a, EngineEvent b)
							{
								return (int)((a.GetTime() - b.GetTime()) * 1000);
							});
	}

	override public EventAPI GetEventAPI()
	{
		return mEventAPI;
	}

	public T GetObjectByTag<T>(string tag) where T : EngineObject
	{
		GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(tag);

		foreach (GameObject currentObject in gameObjects)
		{
			T currentComponent = currentObject.GetComponent<T>();

			if (currentComponent && currentComponent.GetInstance() == this)
			{
				return currentComponent;
			}
		}

		return null;
	}

	public int GetObjectIDByTag<T>(string tag) where T : EngineObject
	{
		GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(tag);

		foreach (GameObject currentObject in gameObjects)
		{
			T currentComponent = currentObject.GetComponent<T>();

			if (currentComponent && currentComponent.GetInstance() == this)
			{
				return currentComponent.GetObjectID();
			}
		}

		return 0;
	}

	override public bool IsServer()
	{
		return mIsServer;
	}

	override public bool IsGraphics()
	{
		return mIsGraphics;
	}

	public void MakeServer()
	{
		mIsServer = true;

		mEventAPI.MakeServer();
	}

	public void MakeClient()
	{
		mIsServer = false;

		mEventAPI.MakeClient();
	}

	public void MakeOffline()
	{
		mIsServer = false;

		mEventAPI.MakeOffline();
	}

	public EngineManager GetGraphics()
	{
		return graphicalManager;
	}

	public EngineManager GetSyncedClient()
	{
		return mSyncedClient;
	}

	void SetSyncedClient(EngineManager manager)
	{
		mSyncedClient = manager;
	}

	public EngineEvent GetCurrentEvent()
	{
		return mCurrentEvent;
	}
}
