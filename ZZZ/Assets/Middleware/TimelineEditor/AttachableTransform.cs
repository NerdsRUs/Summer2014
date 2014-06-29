using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttachableTransform : MonoBehaviour
{
	public string name = "";
	public bool offscreen = true;
	public float offscreenOffset = 50;

	protected bool mRemoved = false;

	bool mInitialize = false;

	static protected Dictionary<string, List<Transform>> mAvailableTransforms = new Dictionary<string, List<Transform>>();

	void Start()
	{
		init();		
	}

	void init()
	{
		if (!mInitialize)
		{
			mInitialize = true;

			AttachableTransform.addTransform(name, transform);
		}
	}

	void OnDisable()
	{
		mRemoved = true;

		AttachableTransform.removeTransform(transform);
	}

	void Update()
	{
		init();

		if (offscreen && !mRemoved)
		{
			if (Camera.main.WorldToScreenPoint(transform.position).x < Screen.width + offscreenOffset)
			{
				mRemoved = true;

				AttachableTransform.removeTransform(transform);
			}
		}
	}

	static public void addTransform(string transformName, Transform transform)
	{
		if (!mAvailableTransforms.ContainsKey(transformName))
		{
			mAvailableTransforms[transformName] = new List<Transform>();
		}

		mAvailableTransforms[transformName].Add(transform);
	}

	static public void removeTransform(Transform transform)
	{
		foreach (KeyValuePair<string, List<Transform>> pair in mAvailableTransforms)
		{
			pair.Value.Remove(transform);
		}
	}

	static public Transform getTransform(string transformName)
	{
		if (mAvailableTransforms.ContainsKey(transformName) && mAvailableTransforms[transformName].Count > 0)
		{
			return mAvailableTransforms[transformName][Common.Range(0, mAvailableTransforms[transformName].Count)];
		}

		return null;
	}
}