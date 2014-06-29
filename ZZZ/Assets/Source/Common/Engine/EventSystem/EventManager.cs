using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EventManager : EngineObject 
{
	static double UPDATE_TICK = 0.1f;

	Pool<EngineObject> mGameObjects = new Pool<EngineObject>();
	List<EngineEvent> mCurrentEvents = new List<EngineEvent>();
	List<EngineEvent> mPastEvents = new List<EngineEvent>();

	static EventManager mCurrentInstance;

	static bool mInitialize = false;
	static bool mDataLoaded = false;
	static bool mIsLoading = false;

	double mLastUpdateTime = -UPDATE_TICK;
	double mTickTime = -UPDATE_TICK;

	void Start()
	{
		Init();

		mCurrentInstance = this;

		if (!mDataLoaded)
		{
			mDataLoaded = true;

			DataManager.LoadData();
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
		}
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
		while (mCurrentEvents.Count > 0)
		{
			//EventDebug.addEvent(mCurrentEvents[0]);

			mCurrentEvents[0].Execute(this);
			mCurrentEvents.RemoveAt(0);
		}
	}

	void OnPress(bool isDown)
	{
	}

	void OnClick()
	{
	}

	void OnHover(bool isOver)
	{
	}

	public double GetEngineTime()
	{
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

	static public EventManager GetCurrentInstance()
	{
		return mCurrentInstance;
	}

	public void AddEvent(EngineEvent.EventCall callEvent, bool executeInstantly = false)
	{
		EngineEvent newEvent = new EngineEvent();

		newEvent.Init(callEvent, Time.time, Vector3.zero);

		MakeEvent(newEvent, executeInstantly);
	}

	public void MakeEvent(EngineEvent callEvent, bool executeInstantly)
	{
		if (executeInstantly)
		{
			//EventDebug.addEvent(callEvent);

			callEvent.Execute(this);
		}
		else
		{
			mCurrentEvents.Add(callEvent);
		}
	}
}
