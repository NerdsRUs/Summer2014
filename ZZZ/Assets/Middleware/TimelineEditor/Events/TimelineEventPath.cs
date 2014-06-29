using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TimelineEventPath : TimelineEvent
{
	public bool forcePathVisible = false;

	public Transform moveObject;
	public bool rotateToPath = false;

	public bool attachPath = false;
	public Transform attachStart;
	public Transform attachEnd;

	public Transform[] mNodes;
	public Vector3[] mLocalNodes;

	public bool includeStartPosition = false;
	public TYPE endType = TYPE.CLAMP;

	protected float mElaspedTime = 0;
	protected List<Transform> mSortNodes = new List<Transform>();

	protected bool mIsNested = false;

	protected Vector3 mStartPosition;
	protected Vector3 mLastPosition;

	protected Vector3 mOffsetPosition;

	public enum TYPE
	{
		CLAMP,
		RESET,
		IGNORE,
		RESET_IGNORE_SKIP,
		CLAMP_IGNORE_SKIP
	}

	protected override void Start()
	{
		base.Start();

		mOffsetPosition = transform.position;

		//refreshNodes();
	}

	protected void checkIsNested()
	{
		Debug.Log("Check nested");
		if (Common.isParentOf(transform, moveObject))
		{
			mIsNested = true;

			mLocalNodes = new Vector3[mNodes.Length];
			for (int i = 0; i < mLocalNodes.Length; i++)
			{
				mLocalNodes[i] = transform.TransformPoint(mNodes[i].localPosition) - transform.TransformPoint(mNodes[0].localPosition);
				
				Debug.Log("  Node: " + mLocalNodes[i]);
			}
		}
	}

	public override bool initialize(float lengthOffset = 0)
	{
		mElaspedTime = lengthOffset;

		mStartPosition = moveObject.position;

		if (includeStartPosition)
		{
			mNodes[0].position = mStartPosition;
		}

		if (attachPath && attachStart && attachEnd)
		{
			attachToPoints(attachStart.position, attachEnd.position);
		}

		checkIsNested();

		return base.initialize(lengthOffset);
	}

	public override bool update(float deltaTime)
	{
		float samplePoint = mElaspedTime / getLength();

		samplePoint = Mathf.Clamp01(samplePoint);

		if (attachPath && attachStart && attachEnd)
		{
			attachToPoints(attachStart.position, attachEnd.position);
		}

		if (mIsNested)
		{
			moveObject.transform.position = mStartPosition + iTween.PointOnPath(mLocalNodes, samplePoint);
		}
		else
		{
			moveObject.transform.position = iTween.PointOnPath(mNodes, samplePoint);
		}

		if (rotateToPath)
		{
			if (mIsNested)
			{
				moveObject.transform.LookAt(mStartPosition + iTween.PointOnPath(mLocalNodes, samplePoint + 0.00001f), Vector3.back);
			}
			else
			{
				moveObject.transform.LookAt(iTween.PointOnPath(mNodes, samplePoint + 0.00001f), Vector3.back);
			}
		}

		mElaspedTime += deltaTime;

		return base.update(deltaTime);
	}

	public override bool end()
	{
		float samplePoint = 1;

		switch (endType)
		{
			case TYPE.CLAMP:
				samplePoint = 1;
				break;

			case TYPE.CLAMP_IGNORE_SKIP:
				if (mTimeline.isSkipped())
				{
					samplePoint = -1;
				}
				else
				{
					samplePoint = 1;
				}
				break;

			case TYPE.IGNORE:
				samplePoint = -1;
				break;

			case TYPE.RESET:
				samplePoint = 0;
				break;

			case TYPE.RESET_IGNORE_SKIP:
				if (mTimeline.isSkipped())
				{
					samplePoint = -1;
				}
				else
				{
					samplePoint = 0;
				}
				break;
		}
			
		if (samplePoint > -1)
		{
			if (attachPath && attachStart && attachEnd)
			{
				attachToPoints(attachStart.position, attachEnd.position);
			}

			if (mIsNested)
			{
				moveObject.transform.position = mStartPosition + iTween.PointOnPath(mLocalNodes, samplePoint);
			}
			else
			{
				moveObject.transform.position = iTween.PointOnPath(mNodes, samplePoint);
			}

			if (rotateToPath)
			{
				if (mIsNested)
				{
					moveObject.transform.LookAt(mStartPosition + iTween.PointOnPath(mLocalNodes, samplePoint + 0.00001f), Vector3.back);
				}
				else
				{
					moveObject.transform.LookAt(iTween.PointOnPath(mNodes, samplePoint + 0.00001f), Vector3.back);
				}
			}
		}

		return base.end();
	}

	public override void sample(float samplePoint)
	{
#if UNITY_EDITOR
		checkIsNested();

		if (moveObject && mNodes.Length > 1 && !includeStartPosition)
		{
			if ((!Selection.activeTransform || (Selection.activeTransform != transform && Selection.activeTransform.parent != transform)) && attachPath && attachStart && attachEnd)
			{
				attachToPoints(attachStart.position, attachEnd.position);
			}

			float interpolationPoint = samplePoint / length;


			if (interpolationPoint >= 0 && interpolationPoint <= 1.0f)
			{
				if (!mIsNested)
				{
					moveObject.transform.position = iTween.PointOnPath(mNodes, interpolationPoint);

					if (rotateToPath)
					{
						moveObject.transform.LookAt(iTween.PointOnPath(mNodes, interpolationPoint + 0.0001f), Vector3.back);
					}
				}
			}
		}
#endif

		base.sample(samplePoint);
	}

	protected void attachToPoints(Vector3 start, Vector3 end)
	{
		if (mNodes.Length > 0)
		{
			Vector3 pathStart = mNodes[0].localPosition;
			Vector3 pathEnd = mNodes[mNodes.Length - 1].localPosition;

			Vector3 pathDirection = pathEnd - pathStart;
			Vector3 attachDirection = end - start;

			Quaternion rotation = new Quaternion();
			rotation.SetFromToRotation(pathDirection, attachDirection);

			transform.rotation = rotation;

			float scale = attachDirection.magnitude / pathDirection.magnitude;

			transform.localScale = new Vector3(scale, scale, scale);

			//Vector3 offset = mNodes[0].position - start;

			transform.position = start - (mNodes[0].position - transform.position);
		}
	}

	public override void editorUpdate()
	{
#if UNITY_EDITOR
		if (Selection.activeTransform == transform || (Selection.activeTransform && Selection.activeTransform.parent == transform))
		{
			transform.localScale = Vector3.one;
			transform.rotation = Quaternion.identity;
		}
#endif

		base.editorUpdate();
	}

	protected void refreshNodes()
	{
		mSortNodes.Clear();

		foreach (Transform child in transform)
		{
			if (child != null)
			{
				mSortNodes.Add(child);
			}
		}

		mSortNodes.Sort(delegate(Transform a, Transform b)
						{
							return a.name.CompareTo(b.name);
						}
						);

		mNodes = new Transform[mSortNodes.Count];

		for (int i = 0; i < mSortNodes.Count; i++)
		{
			mNodes[i] = mSortNodes[i];
		}
	}

	void OnDrawGizmos()
	{
#if UNITY_EDITOR
		if (!EditorApplication.isPlaying)
		{
			if (forcePathVisible || Selection.activeTransform == transform || (Selection.activeTransform && Selection.activeTransform.parent == transform))
			{
				refreshNodes();

				if (mNodes.Length > 1)
				{
					iTween.DrawPath(mNodes);
				}

				if (attachStart)
				{
					Gizmos.DrawSphere(attachStart.position, 10);
				}

				if (attachEnd)
				{
					Gizmos.DrawSphere(attachEnd.position, 10);
				}
			}
		}
#endif
	}

#if UNITY_EDITOR

	//Tells the editor if the length is changable in the Timeline editor
	override public bool canModifyLength()
	{
		return true;
	}

#endif
}
