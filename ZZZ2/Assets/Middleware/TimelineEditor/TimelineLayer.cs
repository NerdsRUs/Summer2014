using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TimelineLayer : MonoBehaviour
{
	protected static char[] seperator = new char[] { '|', '/'};

	const float LAYER_OPEN_HEGHT = 70.0f;
	const float LAYER_CLOSED_HEGHT = 20.0f;

	public bool open;
	public List<TimelineEvent> events = new List<TimelineEvent>();
	//public Timeline timeline;
	public int layerNumber;

	public int randomChance = 0;

	public float getHeight()
	{
		if (open)
		{
			return LAYER_OPEN_HEGHT;
		}
		else
		{
			return LAYER_CLOSED_HEGHT;
		}
	}

	public void refreshEvents()
	{
		events.Clear();

		//foreach (Transform layer in transform.parent)
		//{
			foreach (Transform child in transform)
			{
				TimelineEvent tempEvent = child.GetComponent<TimelineEvent>();

				if (tempEvent != null)// && tempEvent.layer == layerNumber)
				{
					events.Add(tempEvent);
				}
			}
		//}

		/*for (int i = 0; i < events.Count; i++)
		{
			events[i].transform.parent = transform;
		}*/
	}

	virtual public bool isActive(Timeline timeline)
	{
		if (randomChance > 0)
		{
			int random = timeline.getRandom();
			//SQDebug.log("Random chane: " + random);
			if (randomChance <= random % 100)
			{
				return false;
			}
		}

		return true;
	}

	virtual public bool canPlayEvent(Timeline timeline, TimelineEvent thisEvent)
	{
		return true;
	}
}
