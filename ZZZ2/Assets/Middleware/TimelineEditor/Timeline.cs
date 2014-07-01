using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Timeline : MonoBehaviour
{
	//note that the timeline can only go forward in time, so when drawing in the editor one needs to reset and redraw from 0
	//this means we can't have constant update draws, and instead need to have a "Play from this point" button

	const string EVENT_PREFIX = "TimelineEvent";
	
	public const float MINIMUM_TICK = 1000.0f;

	public bool autoPlayback = false;
	public bool autoUpdate = false;
	public bool loop = false;
	
	public float timeScale = 1.0f;

	public bool trimWhiteSpace = false;

	public bool playInConstantTime = false;
	public float phase = 0;
	public bool randomizePhase = false;
	[HideInInspector]
	//public string[] layerNames = null;
	public float length = 10;

	protected bool discretTimeTicks = false;

	protected List<TimelineEvent> mAllEvents = new List<TimelineEvent>();
	protected List<TimelineEvent> mEvents = new List<TimelineEvent>();
	protected List<TimelineEvent> mActiveEvents = new List<TimelineEvent>();
	protected int mCurrentEvent = 0;

	protected double mCurrentTime = 0;
	protected double mStartTime = 0;

	protected bool mEnabled = false;
	protected float mLength = 0;

	//Only one timeline of each name can be active at a time
	protected List<string> mActiveTimelines = new List<string>();

	protected System.Random mRandom = new System.Random();

	protected Dictionary<string, string> mVariables = new Dictionary<string, string>();

	protected float mLastUpdateTime = 0;

	protected bool mIsSkipped = false;
	protected bool mJumpingAhead = false;
	protected float mRandomTime;

	protected double mDiscreetTime;
	protected int mDiscreetTicks = 0;

	protected bool mTryingToNext = false;
	protected bool mFirstPlay = true;

	bool mDynamicLayers = false;

	protected List<Timeline> mSubtimelines = new List<Timeline>();

	bool mPlaySounds = true;

	virtual public void OnEnable()
	{
		if (autoPlayback)
		{
			startPlayback();
		}
	}

	virtual public void Update()
	{
		if (autoUpdate)
		{
			update(Time.deltaTime);
		}
	}

	//begins the Timeline's playback
	public void startPlayback(int randomSeed = 0, bool shouldSkipToFirstEvent = true)
	{
		mPlaySounds = true;

		mDiscreetTicks = 0;

		if (randomSeed != 0)
		{
			mRandom = new System.Random(randomSeed);
		}
		else
		{
			mRandom.Next();
		}

		mCurrentTime = 0;
		
		refreshEventList();

		mIsSkipped = false;
		mEnabled = true;

		if (playInConstantTime)
		{
			mJumpingAhead = true;
			mLastUpdateTime = Time.realtimeSinceStartup - Mathf.Repeat(Time.realtimeSinceStartup, mLength);
		}
		else if (phase > 0 && mFirstPlay)
		{
			mJumpingAhead = true;
			mRandomTime = phase * mLength;
		}
		else if (randomizePhase && mFirstPlay)
		{
			mJumpingAhead = true;
			mRandomTime = Common.Range(0, mLength);
		}

        if (shouldSkipToFirstEvent && discretTimeTicks)
		{
			next<TimelineEventDiscreet>();
		}

        if (shouldSkipToFirstEvent)
        {
#if UNITY_EDITOR
            update(mStartTime, true);
#else
		update(0, false);
#endif
        }

		mFirstPlay = false;
	}

	public bool isFirstPlay()
	{
		return mFirstPlay;
	}

	public void checkNext()
	{
		if (mTryingToNext && discretTimeTicks)
		{
			next<TimelineEventDiscreet>();
		}
	}

	//Returns true when paused
	virtual public bool update(double deltaTime, bool skipEvents = false)
	{
		bool paused = false;

		if (playInConstantTime)
		{
			deltaTime = (Time.realtimeSinceStartup + mRandomTime) - mLastUpdateTime;
            mLastUpdateTime = (Time.realtimeSinceStartup + mRandomTime);
		}
		else if ((phase > 0 || randomizePhase) && mFirstPlay)
		{
			deltaTime = mRandomTime;
		}
		
		if (mEnabled)
		{
			deltaTime *= timeScale;

			for (int i = 0; i < mActiveEvents.Count; i++)
			{
				bool eventOver = false;

				if (mActiveEvents[i] is TimelineEventDiscreet && ((TimelineEventDiscreet)mActiveEvents[i]).useDiscretTime)
				{
					eventOver = !(mDiscreetTime < mActiveEvents[i].startTime + mActiveEvents[i].getLength());
				}
				else
				{
					eventOver = !(mCurrentTime + deltaTime < mActiveEvents[i].startTime + mActiveEvents[i].getLength());
				}

				if (eventOver)
				{
					paused = paused || mActiveEvents[i].end();

					if (paused)
					{
						break;
					}
					mActiveEvents.RemoveAt(i);
					i--;
				}
			}

			if (!discretTimeTicks)
			{
				mDiscreetTime = mCurrentTime;
			}

			if (!paused)
			{
				while (mEvents.Count > 0 && mDiscreetTime >= mEvents[0].startTime)
				{
					if (mEvents[0].getLayer().canPlayEvent(this, mEvents[0]))
					{
#if UNITY_EDITOR
						if (!Application.isPlaying && (!mEvents[0].canSkip() || !skipEvents || mDiscreetTime + deltaTime <= mEvents[0].startTime + mEvents[0].getLength() || mEvents[0].getLength() == 0))
						{
							TimelineEvent tempEvent = mEvents[0];

							paused = paused || tempEvent.initialize((float)(mEvents[0].startTime - (mDiscreetTime + deltaTime)));

							if (paused)
							{
								break;
							}

                            if (!mActiveEvents.Contains(tempEvent))
                            {
							    mActiveEvents.Add(tempEvent);
                            }
						}
						else
#endif
						{
							TimelineEvent tempEvent = mEvents[0];
                            paused = paused || tempEvent.initialize((float)(mEvents[0].startTime - (mDiscreetTime + deltaTime)));

							if (paused)
							{
								break;
							}

							if (!mActiveEvents.Contains(tempEvent))
                            {
							    mActiveEvents.Add(tempEvent);
                            }
						}
					}

					mEvents.RemoveAt(0);
				}
			}

			//This is to catch same-frame events
			if (!paused)
			{
				for (int i = 0; i < mActiveEvents.Count; i++)
				{
					bool eventOver = false;

					if (mActiveEvents[i] is TimelineEventDiscreet && ((TimelineEventDiscreet)mActiveEvents[i]).useDiscretTime)
					{
						eventOver = !(mDiscreetTime < mActiveEvents[i].startTime + mActiveEvents[i].getLength());
					}
					else
					{
						eventOver = !(mCurrentTime + deltaTime < mActiveEvents[i].startTime + mActiveEvents[i].getLength());
					}

                    if (!eventOver || !mActiveEvents[i].canEnd())
					{
						paused = paused || mActiveEvents[i].update((float)deltaTime);

						if (paused)
						{
							break;
						}
					}
					else
					{
						paused = paused || mActiveEvents[i].end();

						if (paused)
						{
							break;
						}
						mActiveEvents.RemoveAt(i);
						i--;
					}
				}
			}

			if (!paused)
			{
				mCurrentTime += deltaTime;
				if (!discretTimeTicks)
				{
					mDiscreetTime = mCurrentTime;
				}
			}
		}

		if (mEnabled && isFinished())
		{
			if (loop && !mIsSkipped)
			{
				startPlayback();
			}
			else
			{
				mEnabled = false;

				mFirstPlay = true;
			}
		}

		if (playInConstantTime && deltaTime > 0 && !skipEvents)
		{
			mJumpingAhead = false;
		}
		else if ((phase > 0 || randomizePhase) && mFirstPlay)
		{
			mJumpingAhead = false;
		}

		return paused;
	}

    public bool isFinished()
    {
        return ((trimWhiteSpace || mDiscreetTime >= mLength) && mEvents.Count == 0 && mActiveEvents.Count == 0) || !mEnabled;
    }

    public bool isPlaying()
    {
        return mEnabled;
    }

	protected void refreshEventList()
	{
		mLength = length;

		if (mAllEvents.Count == 0 || mDynamicLayers)
		{
			mAllEvents.Clear();

			List<TimelineLayer> layers = new List<TimelineLayer>();

			foreach (Transform layer in transform)
			{
				TimelineLayer tempLayer = (TimelineLayer)layer.GetComponent<TimelineLayer>();

				if (tempLayer != null)
				{
					layers.Add(tempLayer);
				}
			}

			layers.Sort(
				delegate(TimelineLayer a, TimelineLayer b)
				{
					return a.layerNumber - b.layerNumber;
				}
			);

			for (int i = 0; i < layers.Count; i++)
			{
				if (!mDynamicLayers)
				{
					if (layers[i].randomChance > 0)
					{
						mDynamicLayers = true;
					}
				}

				if (layers[i].isActive(this) && !mActiveTimelines.Contains(layers[i].name))
				{
					mActiveTimelines.Add(layers[i].name);

					foreach (Transform child in layers[i].transform)
					{
						TimelineEvent tempEvent = (TimelineEvent)child.GetComponent<TimelineEvent>();

						if (tempEvent != null)
						{
							tempEvent.setTimeline(this);
							//mLength = Mathf.Max(mLength, tempEvent.length + tempEvent.startTime);

							if (!mEvents.Contains(tempEvent))
							{
								mAllEvents.Add(tempEvent);
							}
						}
					}
				}
			}

			mAllEvents.Sort(
				delegate(TimelineEvent a, TimelineEvent b)
				{
					if ((int)((a.startTime - b.startTime) * MINIMUM_TICK) == 0)
					{
						return a.getLayer().layerNumber - b.getLayer().layerNumber;
					}

					return (int)((a.startTime - b.startTime) * MINIMUM_TICK);
				}
			);
		}

		mEvents.Clear();
		mActiveTimelines.Clear();

		for (int i = 0; i < mAllEvents.Count; i++)
		{
			mEvents.Add(mAllEvents[i]);
		}
	}

	//Jumps to the end of the timeline
	public void skip()
	{
		mIsSkipped = true;

		mCurrentTime = float.PositiveInfinity;
		mDiscreetTime = float.PositiveInfinity;

		update(float.PositiveInfinity);

		mEnabled = false;

		mFirstPlay = true;
	}

	//Jumps to the next event of type T
	//If none exist, then skips
	//return true to block
	public bool next<T>()
	{
		if (discretTimeTicks)
		{
			mTryingToNext = true;

			for (int i = 0; i < mEvents.Count; i++)
			{
				if (mEvents[i].dontSkipOnDiscreet())
				{
					mCurrentTime = mEvents[i].startTime;
					mDiscreetTime = mEvents[i].startTime;

                    //SQDebug.log("Next:" + mDiscreetTime);

					if (update(0))
					{
						mTryingToNext = true;
						return true;
					}

					if (mSubtimelines.Count > 0)
					{
						//Timeline displayed event
						if (mSubtimelines[0].getDiscreetTicks() > 0)
						{
							return true;
						}
					}

					i = 0;
				}

				if (mEvents[i] is T || mEvents[i] is TimelineEventDiscreet)
				{
					break;
				}
			}
		}

		for (int i = 0; i < mSubtimelines.Count; i++ )
		{
			if (mSubtimelines[i].discretTimeTicks)
			{
				if (mSubtimelines[i].next<T>())
				{
					mTryingToNext = false;
					return true;
				}
			}
		}

		for (int i = 0; i < mEvents.Count; i++)
		{
			if (mEvents[i] is T || mEvents[i] is TimelineEventDiscreet)
			{
				mCurrentTime = mEvents[i].startTime;
				mDiscreetTime = mEvents[i].startTime;

                //SQDebug.log("Next:" + mDiscreetTime);

				mDiscreetTicks++;

				mTryingToNext = false;
				return true;
			}
		}

		skip();

		mTryingToNext = false;
		return false;
	}


	//Editor functions
	public virtual string getRootEvent()
	{
		return EVENT_PREFIX;
	}

	virtual public bool canUseEvent(string eventName)
	{
		return eventName.StartsWith(getRootEvent());
	}

	public double getCurrentTime()
	{
		return mCurrentTime;
	}

	public void setStartTime(float startTime)
	{
		mStartTime = startTime;
	}

	public void resetLength()
	{
		length = 0;

		foreach (Transform layer in transform)
		{
			foreach (Transform child in layer)
			{
				TimelineEvent tempEvent = (TimelineEvent)child.GetComponent<TimelineEvent>();

				if (tempEvent != null && !(tempEvent is TimelineEventSound)) 
				{
					length = Mathf.Max(length, tempEvent.getLength() + tempEvent.startTime);
				}
			}
		}
	}

	public float getEventTime(TimelineEventMarker.EVENT eventName, float defaultTime = 0)
	{
		foreach (Transform layer in transform)
		{
			foreach (Transform child in layer)
			{
				TimelineEventMarker tempEvent = (TimelineEventMarker)child.GetComponent<TimelineEventMarker>();

				if (tempEvent != null && tempEvent.eventName == eventName)
				{
					return tempEvent.startTime * timeScale;
				}
			}
		}

		return defaultTime;
	}

	public void sample(float time)
	{
		List<string> eventTypes = new List<string>();

		foreach (Transform layer in transform)
		{
			foreach (Transform child in layer)
			{
				TimelineEvent tempEvent = (TimelineEvent)child.GetComponent<TimelineEvent>();

				if (tempEvent != null)
				{
					if (!eventTypes.Contains(tempEvent.GetType().Name))
					{
						eventTypes.Add(tempEvent.GetType().Name);

						tempEvent.beforeSample(this);
					}
				}
			}
		}

		foreach (Transform layer in transform)
		{
			foreach (Transform child in layer)
			{
				TimelineEvent tempEvent = (TimelineEvent)child.GetComponent<TimelineEvent>();

				if (tempEvent != null)
				{
					float eventTime = time - tempEvent.startTime;

					tempEvent.sample(eventTime);
					tempEvent.editorUpdate();
				}
			}
		}

		eventTypes.Clear();

		foreach (Transform layer in transform)
		{
			foreach (Transform child in layer)
			{
				TimelineEvent tempEvent = (TimelineEvent)child.GetComponent<TimelineEvent>();

				if (tempEvent != null)
				{
					if (!eventTypes.Contains(tempEvent.GetType().Name))
					{
						eventTypes.Add(tempEvent.GetType().Name);

						tempEvent.afterSample(this);
					}
				}
			}
		}
	}

	public int getRandom()
	{
		return mRandom.Next();
	}

	public void addVariable(string name, string value)
	{
		mVariables[name] = value;
	}

	public string replaceVariables(string text)
	{
		string finalText = text;

		foreach (KeyValuePair<string, string> pair in mVariables)
		{
			finalText = finalText.Replace("[" + pair.Key + "]", pair.Value);
		}

		return finalText;
	}

	virtual public string getLayerClassName()
	{
		return "TimelineLayer";
	}

	public bool isSkipped()
	{
		return mIsSkipped || mJumpingAhead;
	}

	public void addSubTimeline(Timeline timeline)
	{
		mSubtimelines.Add(timeline);
	}

	public void removeSubTimeline(Timeline timeline)
	{
		mSubtimelines.Remove(timeline);
	}

	public int getDiscreetTicks()
	{
		return mDiscreetTicks;
	}

	virtual public bool canPlayNext()
	{
		return isFinished();
	}

	public int getMaximumLayer()
	{
		List<TimelineLayer> layers = new List<TimelineLayer>();

		foreach (Transform layer in transform)
		{
			TimelineLayer tempLayer = (TimelineLayer)layer.GetComponent<TimelineLayer>();

			if (tempLayer != null)
			{
				layers.Add(tempLayer);
			}
		}

		layers.Sort(
			delegate(TimelineLayer a, TimelineLayer b)
			{
				return a.layerNumber - b.layerNumber;
			}
		);

		if (layers.Count > 0)
		{
			return layers[layers.Count - 1].layerNumber;
		}

		return 0;
	}

	public bool isMuted()
	{
		return !mPlaySounds;
	}

	public void mute()
	{
		mPlaySounds = false;
	}
}
