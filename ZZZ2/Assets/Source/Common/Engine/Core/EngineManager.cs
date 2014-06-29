﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EngineManager : EngineObject 
{
	static double UPDATE_TICK = 0.1f;

	Pool<EngineObject> mGameObjects = new Pool<EngineObject>();
	List<EngineEvent> mCurrentEvents = new List<EngineEvent>();
	List<EngineEvent> mPastEvents = new List<EngineEvent>();

	//static EngineManager mCurrentInstance;

	static bool mInitialize = false;
	static bool mDataLoaded = false;
	static bool mIsLoading = false;

	double mLastUpdateTime = -UPDATE_TICK;
	double mTickTime = -UPDATE_TICK;

	EventAPI mEventAPI;

	bool mIsServer;

	//Test stuff
	public bool isServer;

	void Start()
	{
		mIsServer = isServer;

		Init();

		//mCurrentInstance = this;

		if (!mDataLoaded)
		{
			mDataLoaded = true;

			DataManager.LoadData();
		}

		InitStartUpObjects();
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

	public override void OnInit()
	{
		mEventAPI = new EventAPI();
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
		while (mCurrentEvents.Count > 0)
		{
			//EventDebug.addEvent(mCurrentEvents[0]);

			mCurrentEvents[0].Execute(this);
			mCurrentEvents.RemoveAt(0);
		}
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

	/*static public EngineManager GetCurrentInstance()
	{
		return mCurrentInstance;
	}*/

	public void MakeEvent(EngineEvent callEvent)
	{
		mCurrentEvents.Add(callEvent);
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
}
