using UnityEngine;
using System.Collections;

public class TimelineEvent : MonoBehaviour
{
	public float startTime;

    protected bool mIsPreloading = false;

    public void setPreloading(bool preloading)
    {
        mIsPreloading = preloading;
    }

	//[HideInInspector]
	//public int layer;

	[HideInInspector]
	public float length = 1.0f;

	protected Timeline mTimeline;

	virtual protected void Start()
	{
	}
	 
	virtual public float getLength()
	{
		return length;
	}

	//Returns true when needs to pause (wait for some event)
	virtual public bool initialize(float lengthOffset = 0)
	{
		return false;
	}

	//Returns true when needs to pause (wait for some event)
	virtual public bool update(float deltaTime)
	{
		return false;
	}

	//Returns true when needs to pause (wait for some event)
	virtual public bool end()
	{
		return false;
	}

	//Samples the event at a given frame
	virtual public void sample(float samplePoint)
	{
	}

	//Tells the editor if the length is changable in the Timeline editor
	virtual public bool canModifyLength()
	{
		return true;
	}

	//Used to refresh data in the editor
	virtual public void editorUpdate()
	{
	}

	//Modify the length of the event
	virtual public void setLength(float newLength)
	{
		length = newLength;
	}

	//Called once per event type before sampling tkaes place - use to sync multiple objects
	virtual public void beforeSample(Timeline timeline)
	{
	}

	//Called once per event type after sampling tkaes place - use to sync multiple objects
	virtual public void afterSample(Timeline timeline)
	{
	}

	virtual public bool canSkip()
	{
		return true;
	}

	public TimelineLayer getLayer()
	{
		return transform.parent.GetComponent<TimelineLayer>();
	}

	public void setTimeline(Timeline timeline)
	{
		mTimeline = timeline;
	}

	virtual public bool dontSkipOnDiscreet()
	{
		return false;
	}

    virtual public bool canEnd()
    {
        return true;
    }

	virtual public void preLoad()
	{
	}
}
