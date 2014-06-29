using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Diagnostics;

public class TimelineEditor : EditorWindow
{
	const bool CHECK_VALIDITY = false;

	const float MENU_HEIGHT = 50.0f;
	const float HEADER_SIZE = 20.0f;
	const float SLIDER_SIZE = 15.0f;
	const float MINIMUM_EVENT_SIZE = SLIDER_SIZE * 3;

	public GUISkin customGUISkin;

	protected Timeline mActiveTimeline;
	protected List<TimelineLayer> mLayers = new List<TimelineLayer>();

	protected bool mIsPlaying = false;

	protected Vector3 mScrollPosition = new Vector2(0, 0);
	protected float mMaxLayerHeight = 0;

	protected float mXScrollMin = 0;
	protected float mXScrollMax = 1;

	protected float mStartPoint = 0;
	protected float mEndPoint = 0;
	protected float mDisplayTime = 0;

	protected DRAG_TYPE mDragEventType;
	protected Vector3 mMouseClickPosition = new Vector2(0, 0);
	protected bool mDragEventInvalidPlacement;
	protected float mDragEventLength;
	protected float mDragEventStartTime;

	protected Dictionary<TimelineEvent, DraggedEvent> mDragEvents = new Dictionary<TimelineEvent, DraggedEvent>();
	

	protected bool mSnapEnabled = true;

	protected Dictionary<TimelineLayer, float> mLayerSizes = new Dictionary<TimelineLayer, float>();
	protected Dictionary<TimelineLayer, bool> mLayerStates = new Dictionary<TimelineLayer, bool>();

	protected bool mIsInitialized = false;

	protected float mContextMenuTime;
	protected GenericMenu mLayerMenu = new GenericMenu();
	protected int mSelectedLayer;

	protected GenericMenu mEventMenu = new GenericMenu();
	protected TimelineEvent mSelectedEvent;

	protected GenericMenu mGlobalMenu = new GenericMenu();

	//protected int mRenamingLayer = -1;

	protected float mMouseTime = 0;
	protected float mSelectedTime = 0;

	protected bool mDraggingTimer = false;

	static protected TimelineEditor mTimelineEditor;


	static protected Stopwatch mStopwatch;
	static protected long mLastTime = 0;

	static protected double mLastSampleTime = 0;



	protected enum DRAG_TYPE
	{
		MOVE,
		EXPAND_LEFT,
		EXPAND_RIGHT,
	}

	protected class DraggedEvent
	{
		public Rect mDragEventRect;
		public float mDragEventPlaceTime = 0;
		public int mLayerOffset = 0;
		public bool mIsMainEvent = false;
	}

	/*protected class Layer
	{
		public string name;
		public bool open;
		public List<TimelineEvent> events;
		public Timeline timeline;
		public int layerNumber;

		public Layer(Timeline newTimeline, int newLayerNumber)
		{
			timeline = newTimeline;
			layerNumber = newLayerNumber;

			name = timeline.layerNames[layerNumber];
			open = true;
			events = new List<TimelineEvent>();

			refreshEvents();
		}

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

			foreach (Transform child in timeline.transform)
			{
				TimelineEvent tempEvent = child.GetComponent<TimelineEvent>();

				if (tempEvent != null && tempEvent.layer == layerNumber)
				{
					events.Add(tempEvent);
				}
			}
		}
	}*/


	[MenuItem("Timeline/Open")]
	static public void show()
	{
		mTimelineEditor = (TimelineEditor)EditorWindow.GetWindow(typeof(TimelineEditor));

		if (Selection.transforms.Length == 1)
		{
			Timeline tempTimeline = Selection.transforms[0].GetComponent<Timeline>();

			if (tempTimeline != null)
			{
				mTimelineEditor.setActiveTimeline(tempTimeline);

				mTimelineEditor.refreshLayers();
			}
		}
	}

	public void OnSelectionChange()
	{
		if (!EditorApplication.isPlaying && Selection.activeTransform != null)
		{
			Timeline tempTimeline = Selection.activeTransform.GetComponent<Timeline>();

			if (tempTimeline != null)
			{
				setActiveTimeline(tempTimeline);
				refreshLayers();
				initialize();
			}
		}
	}

	static public void setDirty()
	{
		mTimelineEditor.refreshEvents();
	}

	static public void setActiveTimelineStatic(Timeline timeline)
	{
		mTimelineEditor.setActiveTimeline(timeline);
	}

	public void setActiveTimeline(Timeline timeline)
	{
		mActiveTimeline = timeline;
		//mSerialedObject = new SerializedObject(mActiveTimeline);

		//mIsPlaying = false;
	}

	void OnGUI()
	{
		if (!Application.isPlaying && Event.current.type == EventType.keyUp && Event.current.keyCode == KeyCode.Delete)
		{
			removeEvents();
		}

		Rect localRect = position;
		localRect.x = 0;
		localRect.y = 0;

		Rect header;
		Rect scroll;
		Rect layers;
		Rect grid;
		Rect gridFooter;
		Rect footer;

		splitRectVertical(localRect, out header, out scroll, HEADER_SIZE);
		splitRectVertical(scroll, out scroll, out footer, scroll.height - HEADER_SIZE);

		splitRectHorizontal(scroll, out layers, out grid, 100.0f);

		DrawHeaders(header);

		if (mActiveTimeline != null)
		{
			splitRectVertical(grid, out header, out grid, HEADER_SIZE);

			if (Event.current.type == EventType.Layout)
			{
				float lastTime = mMouseTime;
				//Update mouse position
				mMouseTime = ((Event.current.mousePosition.x - grid.x) / (grid.width - SLIDER_SIZE) + mXScrollMin / (mXScrollMax - mXScrollMin)) * mDisplayTime;
				mMouseTime = Mathf.Min(mActiveTimeline.length, Mathf.Max(0, mMouseTime));

				if (mActiveTimeline.isPlaying())
				{
					Repaint();
				}
				else
				{
					if (mMouseTime != lastTime)
					{
						Repaint();
					}
				}
			}

			grid.width -= SLIDER_SIZE;
			splitRectVertical(grid, out grid, out gridFooter, grid.height - SLIDER_SIZE);

			EditorGUI.MinMaxSlider(gridFooter, ref mXScrollMin, ref mXScrollMax, 0, 1);

			mStartPoint = mActiveTimeline.length * mXScrollMin;
			mEndPoint = mActiveTimeline.length * mXScrollMax;

			mDisplayTime = mEndPoint - mStartPoint;

			GUI.Box(grid, "");
			DrawGrid(grid);

			scroll.y += HEADER_SIZE;
			scroll.height -= SLIDER_SIZE + HEADER_SIZE;

			mScrollPosition = GUI.BeginScrollView(scroll, mScrollPosition, new Rect(0, 0, 0, mMaxLayerHeight));
			{
				layers.x = 0;
				layers.y = -HEADER_SIZE;

				DrawLayers(layers, grid);
			}
			GUI.EndScrollView();

			DrawSelectedTime(grid);
			DrawCurrentTime(grid);
			DrawDraggedEvent();
		}

		DrawFooters(footer);
	}

	void startPlayingFrom(float startTime)
	{
		if (mActiveTimeline != null)
		{
			mIsPlaying = true;

			mActiveTimeline.startPlayback();
			if (startTime > 0)
			{
				mActiveTimeline.update(startTime / mActiveTimeline.timeScale, true);
			}

			mStopwatch = new Stopwatch();
			mStopwatch.Start();

			mLastTime = mStopwatch.ElapsedMilliseconds;
		}
	}

	void stopPlaying()
	{
		if (mActiveTimeline.isFinished())
		{
			mStopwatch = null;
			mIsPlaying = false;
		}
		else
		{
			mActiveTimeline.skip();
		}
	}

	void copyEvent()
	{
		copyEvent(mSelectedEvent);
	}

	void copyEvent(TimelineEvent tempEvent)
	{
		GameObject gameObject = (GameObject)GameObject.Instantiate(tempEvent.gameObject);
		gameObject.transform.parent = tempEvent.transform.parent;
	}

	void createEvent(object className)
	{
		createNewEvent(mContextMenuTime, mSelectedLayer, (string)className);
	}

	void createNewEvent(float time, int layerNumber, string className = "", string name = "")
	{
		if (className == "")
		{
			className = mActiveTimeline.getRootEvent();
		}

		Undo.RegisterUndo(EditorUtility.CollectDeepHierarchy(new UnityEngine.Object[] { mActiveTimeline }), "Create Event");

		GameObject tempObject = new GameObject();

		TimelineEvent tempEvent = (TimelineEvent)tempObject.AddComponent(className);

		tempObject.transform.parent = mActiveTimeline.transform;

		float maxLength = 99999;
		for (int i = 0; i < mLayers[layerNumber].events.Count; i++)
		{
			float length = mLayers[layerNumber].events[i].startTime - time;

			if (length > 0)
			{
				maxLength = Mathf.Min(maxLength, length);
			}
		}

		maxLength = Mathf.Min(maxLength, tempEvent.getLength());

		if (name == "")
		{
			tempEvent.name = "New Event" + (mActiveTimeline.transform.childCount + 1);
		}
		else
		{
			tempEvent.name = name;
		}

		tempEvent.transform.parent = mLayers[layerNumber].transform;
		tempEvent.startTime = time;
		tempEvent.setLength(maxLength);
		mLayers[layerNumber].events.Add(tempEvent);

		Selection.activeObject = tempEvent.gameObject;
	}

	void removeEvent()
	{
		removeEvent(mSelectedEvent);
	}

	void removeEvents()
	{
		if (mActiveTimeline)
		{
			TimelineEvent[] childEvents = mActiveTimeline.transform.GetComponentsInChildren<TimelineEvent>();

			for (int index = 0; index < Selection.gameObjects.Length; index++)
			{
				TimelineEvent tempEvent = Selection.gameObjects[index].GetComponent<TimelineEvent>();

				if (tempEvent != null)
				{
					for (int i = 0; i < childEvents.Length; i++)
					{
						if (tempEvent == childEvents[i])
						{
							removeEvent(tempEvent);
						}
					}
				}
			}
		}
	}

	void toggleEvent()
	{
		Undo.RegisterUndo(EditorUtility.CollectDeepHierarchy(new UnityEngine.Object[] { mActiveTimeline }), "Toggle Event");

		mSelectedEvent.gameObject.active = !mSelectedEvent.gameObject.active;
	}

	void removeEvent(TimelineEvent tempEvent)
	{
		Undo.RegisterUndo(EditorUtility.CollectDeepHierarchy(new UnityEngine.Object[] { mActiveTimeline }), "Remove Event");

		tempEvent.getLayer().events.Remove(tempEvent);

		GameObject.DestroyImmediate(tempEvent.gameObject);
	}

	void createNewLayer()
	{
		Undo.RegisterUndo(EditorUtility.CollectDeepHierarchy(new UnityEngine.Object[] { mActiveTimeline }), "Create Layer");

		GameObject tempObject = new GameObject();

		TimelineLayer tempLayer = (TimelineLayer)tempObject.AddComponent(mActiveTimeline.getLayerClassName());

		tempObject.transform.parent = mActiveTimeline.transform;

		int maxLayer = mActiveTimeline.getMaximumLayer();

		tempLayer.transform.name = "NewLayer" + (maxLayer + 1);
		tempLayer.open = true;
		tempLayer.layerNumber = maxLayer + 1;

		List<TimelineLayer> layers = new List<TimelineLayer>();

		bool isDuplicateName = true;

		while (isDuplicateName)
		{
			isDuplicateName = false;

			foreach (Transform layer in mActiveTimeline.transform)
			{
				TimelineLayer testLayer = (TimelineLayer)layer.GetComponent<TimelineLayer>();

				if (testLayer != tempLayer && testLayer.transform.name == tempLayer.transform.name)
				{
					tempLayer.transform.name += "_";

					isDuplicateName = true;
					break;
				}
			}
		}

		mLayers.Add(tempLayer);
	}

	void removeLayer()
	{
		removeLayer(mSelectedLayer);
	}

	void removeLayer(int layerID)
	{
		Undo.RegisterUndo(EditorUtility.CollectDeepHierarchy(new UnityEngine.Object[] { mActiveTimeline }), "Remove Layer");

		while (mLayers[layerID].events.Count > 0)
		{
			removeEvent(mLayers[layerID].events[0]);
		}

		GameObject.DestroyImmediate(mLayers[layerID].gameObject);

		/*string[] layerName = mActiveTimeline.layerNames;

		mActiveTimeline.layerNames = new string[layerName.Length - 1];

		int index = 0;
		for (int i = 0; i < layerName.Length; i++)
		{
			if (i != layerID)
			{
				mActiveTimeline.layerNames[index] = layerName[i];

				index++;
			}

			if (i > layerID)
			{
				for (int j = 0; j < mLayers[i].events.Count; j++)
				{
					mLayers[i].events[j].layer--;
				}
			}
		}*/
	}

	public void refreshLayers()
	{
		mLayerStates.Clear();
		mLayers.Clear();

		if (mActiveTimeline != null)
		{
			foreach (Transform layer in mActiveTimeline.transform)
			{
				TimelineLayer tempLayer = layer.GetComponent<TimelineLayer>();

				if (tempLayer != null)
				{
					mLayers.Add(tempLayer);

					mLayerStates[tempLayer] = tempLayer.open;
				}
			}
		}

		mLayers.Sort(
			delegate(TimelineLayer a, TimelineLayer b)
			{
				return a.layerNumber - b.layerNumber;
			}
		);
	}

	void refreshEvents()
	{
		if (mActiveTimeline != null)
		{
			for (int i = 0; i < mLayers.Count; i++)
			{
				if (mLayers[i] != null)
				{
					mLayers[i].refreshEvents();
				}
			}
		}
	}

	/*void renameLayer()
	{
		mRenamingLayer = mSelectedLayer;
	}*/

	/*void resetName(int layerIndex)
	{
		Undo.RegisterUndo(EditorUtility.CollectDeepHierarchy(new UnityEngine.Object[] { mActiveTimeline }), "Rename Layer");

		mActiveTimeline.layerNames[layerIndex] = mLayers[layerIndex].name;

		mRenamingLayer = -1;
	}*/

	void changeEvent(object className)
	{
		Undo.RegisterUndo(EditorUtility.CollectDeepHierarchy(new UnityEngine.Object[] { mActiveTimeline }), "Change Event");

		if (mSelectedEvent.GetType().Name != className)
		{
			float oldStartTime = mSelectedEvent.startTime;
			int oldLayer = mSelectedEvent.getLayer().layerNumber;
			string oldName = mSelectedEvent.name;

			removeEvent(mSelectedEvent);
			createNewEvent(oldStartTime, oldLayer, (string)className, oldName);
		}
	}

	protected void initialize()
	{
		mLayerMenu = new GenericMenu();
		mEventMenu = new GenericMenu();
		
		AppDomain application = AppDomain.CurrentDomain;
		System.Reflection.Assembly[] assemblies = application.GetAssemblies();

		foreach (System.Reflection.Assembly assembly in assemblies)
		{
			try
			{
				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					if (mActiveTimeline.canUseEvent(type.Name))
					{
						Type typeBase = type;
						int depth = 0;

						while (typeBase.Name != mActiveTimeline.getRootEvent() && typeBase.Name != "Editor" && typeBase.Name != "MonoBehavior" && depth < 20)
						{
							typeBase = typeBase.BaseType;

							depth++;
						}
						//Time out after 20 inheritances

						if (typeBase.Name == mActiveTimeline.getRootEvent())
						{
							string shortName = type.Name.Substring(type.Name.IndexOf(mActiveTimeline.getRootEvent()) + mActiveTimeline.getRootEvent().Length);

							if (shortName != "")
							{
								mEventMenu.AddItem(new GUIContent("Change Event Type/" + shortName), false, changeEvent, type.Name);
								mLayerMenu.AddItem(new GUIContent("New Event/" + shortName), false, createEvent, type.Name);
							}
						}
					}
				}
			}
			catch
			{
			}
		}

		//mLayerMenu.AddItem(new GUIContent("Rename Layer"), false, renameLayer);
		mLayerMenu.AddSeparator("");
		mLayerMenu.AddItem(new GUIContent("Remove Layer"), false, removeLayer);



		mEventMenu.AddItem(new GUIContent("Copy Event"), false, copyEvent);
		mEventMenu.AddItem(new GUIContent("Toggle Active"), false, toggleEvent);
		mEventMenu.AddSeparator("");
		mEventMenu.AddItem(new GUIContent("Remove Event"), false, removeEvent);

		mGlobalMenu.AddItem(new GUIContent("New Layer"), false, createNewLayer);
	}

	virtual protected void Update()
	{
		refreshEvents();

		//This should only be called when updating the script
		if (mActiveTimeline != null && mActiveTimeline.transform.childCount != mLayers.Count)
		{
			mIsInitialized = false;
			refreshLayers();
		}

		if (!mIsInitialized)
		{
			initialize();

			mIsInitialized = true;
		}


		mLayerSizes.Clear();

		float currentSize = 0;
		for (int i = 0; i < mLayers.Count; i++)
		{
			if (mLayers[i] != null)
			{
				currentSize += mLayers[i].getHeight();

				mLayerSizes[mLayers[i]] = currentSize;
			}
		}

		mMaxLayerHeight = currentSize;		

#if UNITY_EDITOR
		//Only in the editor

		//Play the current timeline
		if (mIsPlaying && mActiveTimeline != null && mStopwatch != null)
		{
			long delta = mStopwatch.ElapsedMilliseconds - mLastTime;
			mLastTime = mStopwatch.ElapsedMilliseconds;

			if (!Application.isPlaying || !mActiveTimeline.autoUpdate)
			{
				mActiveTimeline.update(delta / 1000.0f);
			}

			if (mActiveTimeline.isFinished())
			{
				mIsPlaying = false;
			}
		}

		if (mActiveTimeline == null)
		{
			mIsPlaying = false;
		}
		else
		{
			if (mIsPlaying)
			{
				if (mLastSampleTime != mActiveTimeline.getCurrentTime())
				{
					mActiveTimeline.sample((float)mActiveTimeline.getCurrentTime());

					mLastSampleTime = mActiveTimeline.getCurrentTime();
				}
			}
			else
			{
				if (mLastSampleTime != mSelectedTime)
				{
					mActiveTimeline.sample(mSelectedTime);

					mLastSampleTime = mSelectedTime;
				}
			}
		}
#endif
	}

	void splitRectVertical(Rect rect, out Rect top, out Rect bottom, float splitPoint)
	{
		top = rect;
		bottom = rect;

		top.height = splitPoint;

		bottom.y += splitPoint;
		bottom.height -= splitPoint;
	}

	void splitRectHorizontal(Rect rect, out Rect left, out Rect right, float splitPoint)
	{
		left = rect;
		right = rect;

		left.width = splitPoint;

		right.x += splitPoint;
		right.width -= splitPoint;
	}

	void bevel(ref Rect rect, float bevelAmount)
	{
		rect.x += bevelAmount;
		rect.y += bevelAmount;
		rect.width -= bevelAmount * 2;
		rect.height -= bevelAmount * 2;
	}

	Vector2 translate(Vector2 position, Vector2 offset)
	{
		return new Vector2(position.x + offset.x, position.y + offset.y);
	}

	void checkEventValidity(int currentLayer, TimelineEvent tempEvent, float draggedStartPoint, float eventLength, Rect gridRect, ref Rect eventRect)
	{
		//Snap to other events
		int snapRange = 0;
		if (mSnapEnabled)
		{
			snapRange = 10;
		}

		//Convert to grid space
		draggedStartPoint -= gridRect.x;

		mDragEventInvalidPlacement = false;

		float percentLength = eventLength / mDisplayTime;

		List<TimelineEvent> tempEvents = mLayers[currentLayer].events;
		for (int k = 0; k < tempEvents.Count; k++)
		{
			if (tempEvents[k] != tempEvent)
			{
				float otherViewStart = tempEvents[k].startTime / mDisplayTime - mXScrollMin / (mXScrollMax - mXScrollMin);
				float otherPercentLength = tempEvents[k].getLength() / mDisplayTime;

				float distance = draggedStartPoint - (otherViewStart + otherPercentLength) * gridRect.width;
				float leftDistance = (otherViewStart) * gridRect.width - (draggedStartPoint + percentLength * gridRect.width);

				if (mSnapEnabled)
				{
					if (distance < snapRange && distance > -25)
					{
						eventRect.x -= distance;
						draggedStartPoint -= distance;
					}

					if (leftDistance < snapRange && leftDistance > -25)
					{
						eventRect.x += leftDistance;
						draggedStartPoint += leftDistance;
					}
				}

				if (distance > -25 || leftDistance > -25)
				{
					mDragEventInvalidPlacement = mDragEventInvalidPlacement || false;
				}
				else
				{
					mDragEventInvalidPlacement = mDragEventInvalidPlacement || true;
				}
			}
		}
	}

	//Draw the footer
	void DrawFooters(Rect footerRect)
	{
		Rect nextElement;

		splitRectHorizontal(footerRect, out nextElement, out footerRect, 150);

		GUI.Label(nextElement, "Mouse: " + mMouseTime);

		splitRectHorizontal(footerRect, out nextElement, out footerRect, 20);
		splitRectHorizontal(footerRect, out nextElement, out footerRect, 200);

		GUI.enabled = false;
		if (mActiveTimeline)
		{
			GUI.enabled = true;
		}
		mSelectedTime = EditorGUI.FloatField(nextElement, "Selected: ", mSelectedTime);
		if (mActiveTimeline)
		{
			mSelectedTime = Mathf.Max(0, Mathf.Min(mActiveTimeline.length, mSelectedTime));
		}
		GUI.enabled = true;

		splitRectHorizontal(footerRect, out nextElement, out footerRect, 20);
		splitRectHorizontal(footerRect, out nextElement, out footerRect, 200);

		mSnapEnabled = EditorGUI.Toggle(nextElement, "Snap", mSnapEnabled);
	}

	protected virtual Type getValidType()
	{
		return typeof(Timeline);
	}

	//Draw the header
	protected virtual void DrawHeaders(Rect headerRect)
	{
		Rect nextElement;

		splitRectHorizontal(headerRect, out nextElement, out headerRect, 150);

		Timeline lastTime = mActiveTimeline;
		setActiveTimeline((Timeline)EditorGUI.ObjectField(nextElement, mActiveTimeline, getValidType()));
		//mSerialedObject = new SerializedObject(mActiveTimeline);

		if (mActiveTimeline != lastTime)
		{
			mIsInitialized = false;

			refreshLayers();
		}

		splitRectHorizontal(headerRect, out nextElement, out headerRect, 20);
		splitRectHorizontal(headerRect, out nextElement, out headerRect, 200);

		if (mActiveTimeline != null)
		{
			float length = EditorGUI.FloatField(nextElement, "Max Time", mActiveTimeline.length);
			if (length < 0.1f)
			{
				length = 0.1f;
			}

			if (!float.IsNaN(length))
			{
				mActiveTimeline.length = length;
			}
		}

		GUI.enabled = true;

		DrawPlayButton();

		splitRectHorizontal(headerRect, out nextElement, out headerRect, 20);
		splitRectHorizontal(headerRect, out nextElement, out headerRect, 60);

		GUI.enabled = mActiveTimeline != null;
		if (GUI.Button(nextElement, "Resize"))
		{
			mActiveTimeline.resetLength();
		}

		splitRectHorizontal(headerRect, out nextElement, out headerRect, 20);
		splitRectHorizontal(headerRect, out nextElement, out headerRect, 80);

		GUI.enabled = mActiveTimeline != null;
		if (GUI.Button(nextElement, "New Layer"))
		{
			createNewLayer();
		}

		splitRectHorizontal(headerRect, out nextElement, out headerRect, 20);
		splitRectHorizontal(headerRect, out nextElement, out headerRect, 80);

		GUI.enabled = GUI.enabled && mLayers.Count > 0;

		if (GUI.Button(nextElement, "New Event"))
		{
			createNewEvent(mSelectedTime, 0);
		}

		//GUI.enabled = GUI.enabled && (EditorApplication.isPlaying || EditorApplication.isPaused);

		/*splitRectHorizontal(headerRect, out nextElement, out headerRect, 20);
		splitRectHorizontal(headerRect, out nextElement, out headerRect, 70);

		if (GUI.Button(nextElement, "Play"))
		{
			CinematicUI.getUI().clearBubbles();

			Client.getStage().startCinematic(mActiveTimeline.transform.name);
		}*/

		/*splitRectHorizontal(headerRect, out nextElement, out headerRect, 20);
		splitRectHorizontal(headerRect, out nextElement, out headerRect, 70);

		if (GUI.Button(nextElement, "Load"))
		{
			if (mActiveTimeline != null)
			{
			}
		}*/

		GUI.enabled = true;

		DrawPlayButton();
	}

	protected virtual void DrawPlayButton()
	{
		//GUI.enabled = !(EditorApplication.isPlaying || EditorApplication.isPaused);
		GUI.enabled = true;

		if (mIsPlaying)
		{
			if (GUI.Button(new Rect(position.width - 100, 0, 100, 20), "Stop"))
			{
				stopPlaying();
			}
		}
		else
		{
			if (GUI.Button(new Rect(position.width - 100, 0, 100, 20), "Play"))
			{
				if (EditorApplication.isPlaying)
				{
					startPlayingFrom(mSelectedTime);
				}
				else
				{
					startPlayingFrom(0);
				}
			}
		}

		GUI.enabled = true;
	}

	//Draw the layers
	void DrawLayers(Rect layerRect, Rect gridRect)
	{
		bool contextEnabled = false;

		Handles.color = customGUISkin.GetStyle("MinorTimestamp").normal.textColor;
		float startY = layerRect.y + HEADER_SIZE;

		Vector3 mousePosition = Event.current.mousePosition;

		bool mouseUpUsed = false;

		for (int i = 0; i < mLayers.Count; i++)
		{
			if (mLayers[i] != null)
			{
				float layerHeight = mLayers[i].getHeight();

				//Draw labels
				Rect thisLayerRect = new Rect(0, startY, layerRect.width, layerHeight);

				bevel(ref thisLayerRect, 2.0f);

				GUI.Box(thisLayerRect, "");

				if (Event.current.type == EventType.mouseDown && thisLayerRect.Contains(mousePosition))
				{
					Selection.activeGameObject = mLayers[i].gameObject;
				}

				/*if (mRenamingLayer == i)
				{
					mLayers[i].name = GUI.TextField(thisLayerRect, mLayers[i].name);

					if ((Event.current.type == EventType.MouseUp && !thisLayerRect.Contains(mousePosition)) ||
						(Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return))
					{
						resetName(i);

						this.Repaint();
					}
				}
				else
				{*/
				mLayers[i].open = EditorGUI.Foldout(thisLayerRect, mLayers[i].open, mLayers[i].name);
				//}

				//Draw events
				for (int j = 0; j < mLayers[i].events.Count; j++)
				{
					TimelineEvent tempEvent = mLayers[i].events[j];

					if (tempEvent != null)
					{
						float startTime = tempEvent.startTime;
						float length = tempEvent.getLength();

						float viewStart = startTime / mDisplayTime - mXScrollMin / (mXScrollMax - mXScrollMin);
						float percentLength = length / mDisplayTime;

						//Not visible
						if (viewStart + percentLength < 0 || viewStart > 1)
						{
							continue;
						}

						bool isSelected = false;
						for (int index = 0; index < Selection.gameObjects.Length; index++)
						{
							if (Selection.gameObjects[index] == tempEvent.gameObject)
							{
								isSelected = true;
							}
						}

						if (viewStart < 0)
						{
							percentLength += viewStart;
							viewStart = 0;
						}
						if (viewStart + percentLength > 1)
						{
							percentLength = 1 - viewStart;
						}
						if (percentLength * gridRect.width < MINIMUM_EVENT_SIZE)
						{
							percentLength = MINIMUM_EVENT_SIZE / gridRect.width;
						}

						Rect eventRect = new Rect(gridRect.x + viewStart * gridRect.width, thisLayerRect.y - 3, percentLength * gridRect.width, layerHeight);

						//bevel(ref eventRect, 2.0f);

						//Draw event context
						if (Event.current.type == EventType.ContextClick && eventRect.Contains(mousePosition))
						{
							mSelectedEvent = tempEvent;
							mEventMenu.ShowAsContext();

							contextEnabled = true;
						}

						//Sperate the event's action rect and display rect
						Rect actionRect = eventRect;

						//Add the length slider tabs
						Rect leftSlider = new Rect();
						Rect rightSlider = new Rect();

						if (tempEvent.gameObject.active && tempEvent.canModifyLength())
						{
							splitRectHorizontal(actionRect, out leftSlider, out actionRect, SLIDER_SIZE);
							splitRectHorizontal(actionRect, out actionRect, out rightSlider, actionRect.width - SLIDER_SIZE);

							if (mDragEvents.Count == 0 && Event.current.type == EventType.MouseDown && leftSlider.Contains(mousePosition))
							{
								mMouseClickPosition = mousePosition;
							}
							else if (mDragEvents.Count == 0 && Event.current.type == EventType.MouseDrag && leftSlider.Contains(mMouseClickPosition))
							{
								mDragEvents[tempEvent] = new DraggedEvent();
								mDragEventType = DRAG_TYPE.EXPAND_LEFT;

								Undo.RegisterUndo(EditorUtility.CollectDeepHierarchy(new UnityEngine.Object[] { mActiveTimeline }), "Expand Left");
							}

							if (mDragEvents.Count == 0 && Event.current.type == EventType.MouseDown && rightSlider.Contains(mousePosition))
							{
								mMouseClickPosition = mousePosition;
							}
							else if (mDragEvents.Count == 0 && Event.current.type == EventType.MouseDrag && rightSlider.Contains(mMouseClickPosition))
							{
								mDragEvents[tempEvent] = new DraggedEvent();
								mDragEventType = DRAG_TYPE.EXPAND_RIGHT;
								mDragEvents[tempEvent].mDragEventPlaceTime = tempEvent.startTime;

								Undo.RegisterUndo(EditorUtility.CollectDeepHierarchy(new UnityEngine.Object[] { mActiveTimeline }), "Expand Right");
							}
						}

						//Inspector
						if (mDragEvents.Count == 0 && Event.current.type == EventType.MouseUp && actionRect.Contains(mousePosition) && !mDraggingTimer)
						{
							if (Event.current.control)
							{
								UnityEngine.Object[] tempObjects = new GameObject[Selection.gameObjects.Length + 1];

								bool wasInList = false;

								int index = 0;
								for (index = 0; index < Selection.gameObjects.Length; index++)
								{
									tempObjects[index] = Selection.gameObjects[index];

									if (tempObjects[index] == tempEvent)
									{
										wasInList = true;
										break;
									}
								}

								if (!wasInList)
								{
									tempObjects[index] = tempEvent.gameObject;

									Selection.objects = tempObjects;
								}
							}
							else
							{
								Selection.activeObject = tempEvent.gameObject;
							}

							Repaint();

							mouseUpUsed = true;
						}

						Vector2 deltaMouse = Vector2.zero;

						//Drag and drop
						if (tempEvent.gameObject.active)
						{
							if (Event.current.type == EventType.MouseDown)//mDragEvents.Count == 0 && Event.current.type == EventType.MouseDown && actionRect.Contains(mousePosition))
							{
								mMouseClickPosition = mousePosition;
							}
							else if (mDragEvents.Count == 0 && Event.current.type == EventType.MouseDrag && actionRect.Contains(mMouseClickPosition))
							{
								if (isSelected && Selection.gameObjects.Length > 1)
								{
									for (int k = 0; k < Selection.gameObjects.Length; k++)
									{
										TimelineEvent tempSelectionEvent = (TimelineEvent)Selection.gameObjects[k].GetComponent<TimelineEvent>();

										if (tempSelectionEvent != null)
										{
											mDragEvents[tempSelectionEvent] = new DraggedEvent();
											mDragEvents[tempSelectionEvent].mLayerOffset = tempEvent.getLayer().layerNumber - tempSelectionEvent.getLayer().layerNumber;
										}
									}
								}

								mDragEvents[tempEvent] = new DraggedEvent();
								mDragEvents[tempEvent].mLayerOffset = 0;
								mDragEvents[tempEvent].mIsMainEvent = true;

								mDragEventStartTime = mMouseTime;
								mDragEventType = DRAG_TYPE.MOVE;
								mDragEventLength = tempEvent.getLength();

								Undo.RegisterUndo(EditorUtility.CollectDeepHierarchy(new UnityEngine.Object[] { mActiveTimeline }), "Move event");
							}
							else if (mDragEvents.Count > 0)
							{
								deltaMouse = mousePosition - mMouseClickPosition;
							}
						}

						//This event is being dragged
						if (mDragEvents.ContainsKey(tempEvent))
						{
							switch (mDragEventType)
							{
								case DRAG_TYPE.EXPAND_RIGHT:
									if (Event.current.type == EventType.MouseUp || mouseOverWindow != this)
									{
										if (!mDragEventInvalidPlacement || !CHECK_VALIDITY)
										{
											float endPoint = ((mDragEvents[tempEvent].mDragEventPlaceTime + mDragEventLength) / mDisplayTime - mXScrollMin / (mXScrollMax - mXScrollMin)) * gridRect.width + gridRect.x;

											tempEvent.length = mDragEventLength;

											refreshEvents();
										}

										mMouseClickPosition = new Vector2(0, 0);
										mDragEvents.Clear();

										mouseUpUsed = true;
									}
									else
									{
										float mousePoint = (mMouseTime / mDisplayTime - mXScrollMin / (mXScrollMax - mXScrollMin)) * gridRect.width + gridRect.x;
										float startPoint = eventRect.x;

										eventRect.width = mousePoint - eventRect.x;
										eventRect.y += HEADER_SIZE * 2;

										float tempLength = eventRect.width / gridRect.width * mDisplayTime;

										checkEventValidity(i, tempEvent, eventRect.x, tempLength, gridRect, ref eventRect);

										eventRect.width -= startPoint - eventRect.x;
										eventRect.x += startPoint - eventRect.x;

										mDragEvents[tempEvent].mDragEventRect = eventRect;

										mDragEventLength = Mathf.Max(0, eventRect.width / gridRect.width * mDisplayTime + (Mathf.Max(0, mXScrollMin - tempEvent.startTime)) / (mXScrollMax - mXScrollMin) * mDisplayTime);
									}
									break;

								case DRAG_TYPE.EXPAND_LEFT:
									if (Event.current.type == EventType.MouseUp || mouseOverWindow != this)
									{
										if (!mDragEventInvalidPlacement || !CHECK_VALIDITY)
										{
											float delta = tempEvent.startTime - mDragEvents[tempEvent].mDragEventPlaceTime;

											tempEvent.startTime = mDragEvents[tempEvent].mDragEventPlaceTime;
											tempEvent.length += delta;

											refreshEvents();
										}

										mMouseClickPosition = new Vector2(0, 0);
										mDragEvents.Clear();

										mouseUpUsed = true;
									}
									else
									{
										float startPoint = (mMouseTime / mDisplayTime - mXScrollMin / (mXScrollMax - mXScrollMin)) * gridRect.width + gridRect.x;

										float delta = eventRect.x - startPoint;

										float rightPoint = eventRect.x + eventRect.width;

										eventRect.x = startPoint;
										eventRect.width += delta;
										eventRect.y += HEADER_SIZE * 2;

										float tempLength = eventRect.width / gridRect.width * mDisplayTime;

										checkEventValidity(i, tempEvent, eventRect.x, tempLength, gridRect, ref eventRect);

										if (eventRect.x > rightPoint - MINIMUM_EVENT_SIZE)
										{
											eventRect.x = rightPoint - MINIMUM_EVENT_SIZE;
											eventRect.width = MINIMUM_EVENT_SIZE;
										}
										else
										{
											eventRect.width += startPoint - eventRect.x;
										}

										mDragEvents[tempEvent].mDragEventRect = eventRect;

										mDragEvents[tempEvent].mDragEventPlaceTime = Mathf.Min(tempEvent.startTime + tempEvent.length, ((eventRect.x - gridRect.x) / gridRect.width + mXScrollMin / (mXScrollMax - mXScrollMin)) * mDisplayTime);
										mDragEventLength = tempEvent.startTime + tempEvent.length - mDragEvents[tempEvent].mDragEventPlaceTime;//Mathf.Max(0, eventRect.width / gridRect.width * mDisplayTime + mXScrollMin / (mXScrollMax - mXScrollMin) * mDisplayTime);
									}
									break;

								case DRAG_TYPE.MOVE:
									//Find layer
									int currentLayer = 0;

									float tempLayerHeight = 0;
									for (; currentLayer < mLayers.Count - 1; currentLayer++)
									{
										if (mousePosition.y < tempLayerHeight + mLayers[currentLayer].getHeight())
										{
											break;
										}

										tempLayerHeight += mLayers[currentLayer].getHeight();
									}

									currentLayer -= mDragEvents[tempEvent].mLayerOffset;

									currentLayer = Mathf.Max(0, Mathf.Min(currentLayer, mLayers.Count - 1));

									//Clipping
									eventRect.x += deltaMouse.x;
									eventRect.y = mLayerSizes[mLayers[currentLayer]] + HEADER_SIZE * 2 - mLayers[currentLayer].getHeight();

									if (eventRect.x < gridRect.x)
									{
										eventRect.x = gridRect.x;
									}
									if (eventRect.xMax > gridRect.xMax)
									{
										eventRect.x = gridRect.xMax - eventRect.width;
									}

									mDragEventInvalidPlacement = false;

									//only snap when it's a single selection
									if (mDragEvents.Count == 1)
									{
										checkEventValidity(currentLayer, tempEvent, eventRect.x, tempEvent.getLength(), gridRect, ref eventRect);
									}

									mDragEvents[tempEvent].mDragEventPlaceTime = mMouseTime - mDragEventStartTime + startTime;

									if (mDragEvents[tempEvent].mDragEventPlaceTime < mXScrollMin * mActiveTimeline.length)
									{
										mDragEvents[tempEvent].mDragEventPlaceTime = mXScrollMin * mActiveTimeline.length;
									}
									else if (mDragEvents[tempEvent].mDragEventPlaceTime + tempEvent.getLength() > mXScrollMax * mActiveTimeline.length)
									{
										mDragEvents[tempEvent].mDragEventPlaceTime = mXScrollMax * mActiveTimeline.length - tempEvent.getLength();
									}
									else
									{
										mDragEvents[tempEvent].mDragEventPlaceTime = ((eventRect.x - gridRect.x) / gridRect.width + mXScrollMin / (mXScrollMax - mXScrollMin)) * mDisplayTime;
									}

									if (mDragEvents[tempEvent].mIsMainEvent && (Event.current.type == EventType.MouseUp || mouseOverWindow != this))
									{
										mMouseClickPosition = new Vector2(0, 0);

										if (!mDragEventInvalidPlacement || !CHECK_VALIDITY)
										{
											foreach (KeyValuePair<TimelineEvent, DraggedEvent> pair in mDragEvents)
											{
												int tempLayer = currentLayer - pair.Value.mLayerOffset;
												tempLayer = Mathf.Max(0, Mathf.Min(tempLayer, mLayers.Count - 1));

												pair.Key.transform.parent = mLayers[tempLayer].transform;
												pair.Key.startTime = pair.Value.mDragEventPlaceTime;
											}

											refreshEvents();
										}

										mDragEvents.Clear();

										mouseUpUsed = true;
									}
									else
									{
										mDragEvents[tempEvent].mDragEventRect = eventRect;
									}
									break;
							}

							this.Repaint();
						}
						else
						{
							Color tempColor = GUI.color;
							if (isSelected)
							{
								tempColor = Color.green;
							}
							else if (!tempEvent.gameObject.active)
							{
								tempColor = Color.black;
							}

							DrawEvent(tempEvent, actionRect, tempEvent.startTime, tempEvent.getLength(), tempColor);

							if (tempEvent.gameObject.active && tempEvent.canModifyLength())
							{
								GUI.Button(leftSlider, "");
								GUI.Button(rightSlider, "");
							}
						}
					}
				}

				//Draw dividing lines
				Vector3 a = new Vector3(0, startY, 0);
				Vector3 b = new Vector3(gridRect.xMax, startY, 0);
				Handles.DrawLine(a, b);

				startY += layerHeight;

				if (i == mLayers.Count - 1)
				{
					a = new Vector3(0, startY, 0);
					b = new Vector3(gridRect.xMax, startY, 0);
					Handles.DrawLine(a, b);
				}

				//Draw layer context
				if (!contextEnabled)
				{
					Rect contextRect = thisLayerRect;
					contextRect.width += gridRect.width;

					if (Event.current.type == EventType.ContextClick && contextRect.Contains(mousePosition))
					{
						mSelectedLayer = i;
						mLayerMenu.ShowAsContext();

						mContextMenuTime = mMouseTime;

						contextEnabled = true;
					}
				}
			}
		}

		//Draw global context
		if (!contextEnabled)
		{
			if (Event.current.type == EventType.ContextClick)
			{
				mGlobalMenu.ShowAsContext();
			}
		}

		//Clear selection
		if (!mouseUpUsed && Event.current.type == EventType.MouseUp && !mDraggingTimer)
		{
			Selection.activeObject = mActiveTimeline.gameObject;
		}
	}

	//Draw dragged event
	void DrawDraggedEvent()
	{
		foreach (KeyValuePair<TimelineEvent, DraggedEvent> pair in mDragEvents)
		{
			Color tempColor = GUI.color;
			if (mDragEventInvalidPlacement && CHECK_VALIDITY)
			{
				tempColor = Color.red;
			}

			if (pair.Value.mDragEventRect.width < MINIMUM_EVENT_SIZE)
			{
				pair.Value.mDragEventRect.width = MINIMUM_EVENT_SIZE;
			}

			pair.Value.mDragEventRect.y -= mScrollPosition.y;

			DrawEvent(pair.Key, pair.Value.mDragEventRect, pair.Value.mDragEventPlaceTime, mDragEventLength, tempColor);
		}
	}

	void DrawEvent(TimelineEvent eventData, Rect eventRect, float startTime, float length, Color color)
	{
		Color beforeColor = GUI.color;
		GUI.color = color;

		GUI.Box(eventRect, eventData.name + ": \n(" + eventData.GetType().Name.Substring(eventData.GetType().Name.IndexOf(mActiveTimeline.getRootEvent()) + mActiveTimeline.getRootEvent().Length) + ")\n" + startTime + "\n" + length);

		GUI.color = beforeColor;
	}

	void DrawGrid(Rect pixelRect)
	{
		float[] grids = { 0.001F, 0.0025F, 0.005F, 0.01F, 0.025F, 0.05F, 0.1F, 0.2F, 0.5F, 1.0F, 2.0F, 5.0F, 10.0F, 20.0F, 50.0F, 100.0F, 200.0F, 500.0F, 1000.0F, 2000.0F, 5000.0F, 10000.0F };
		float step = grids[0];

		for (int i = 0; i < grids.Length; i++)
		{
			if (mDisplayTime / (pixelRect.width / 10) < grids[i])
			{
				step = grids[i];
				break;
			}
		}

		float indexSize = step * pixelRect.width / mDisplayTime;

		int startInt = Mathf.FloorToInt(mStartPoint);
		float remainder = (mStartPoint - startInt) * indexSize;

		int lineIndex = Mathf.FloorToInt(mStartPoint / step);
		for (float x = pixelRect.xMin - remainder; x <= pixelRect.xMax; x += indexSize)
		{
			if (x >= pixelRect.xMin)
			{
				if (lineIndex % 5 == 0)
				{
					Handles.color = customGUISkin.GetStyle("MajorTimestamp").normal.textColor;

					GUI.Label(new Rect(x, HEADER_SIZE, 50, HEADER_SIZE), string.Format("{0:F}", lineIndex * step));
				}
				else
				{
					Handles.color = customGUISkin.GetStyle("MinorTimestamp").normal.textColor;
				}
				Handles.DrawLine(new Vector2(x, pixelRect.y), new Vector2(x, pixelRect.height + pixelRect.y));
			}

			lineIndex++;
		}
		return;
	}

	void DrawSelectedTime(Rect pixelRect)
	{
		Rect numbersRect = new Rect(pixelRect.x, HEADER_SIZE, pixelRect.width, HEADER_SIZE);

		if ((Event.current.type == EventType.MouseDrag && mDraggingTimer) || (Event.current.type == EventType.MouseDown && numbersRect.Contains(Event.current.mousePosition)))
		{
			mMouseClickPosition = new Vector2(0, 0);
			mDragEvents.Clear();

			mSelectedTime = mMouseTime;

			mDraggingTimer = true;
		}
		else if (Event.current.type == EventType.MouseUp)
		{
			mDraggingTimer = false;

			mMouseClickPosition = new Vector2(0, 0);
			mDragEvents.Clear();
		}

		float selectedPosition = (mSelectedTime / mDisplayTime - mXScrollMin / (mXScrollMax - mXScrollMin)) * pixelRect.width + pixelRect.x;

		if (selectedPosition > pixelRect.x && selectedPosition < pixelRect.xMax)
		{
			Handles.color = customGUISkin.GetStyle("SelectedTimestamp").normal.textColor;

			Handles.DrawLine(new Vector2(selectedPosition, pixelRect.y - HEADER_SIZE), new Vector2(selectedPosition, pixelRect.height + pixelRect.y));
		}
	}

	void DrawCurrentTime(Rect pixelRect)
	{
		if (mActiveTimeline != null && mIsPlaying)
		{
			float selectedPosition = (float)((mActiveTimeline.getCurrentTime() / mDisplayTime - mXScrollMin / (mXScrollMax - mXScrollMin)) * pixelRect.width + pixelRect.x);

			if (selectedPosition > pixelRect.x && selectedPosition < pixelRect.xMax)
			{
				Handles.color = customGUISkin.GetStyle("CurrentTimestamp").normal.textColor;

				Handles.DrawLine(new Vector2(selectedPosition, pixelRect.y - HEADER_SIZE), new Vector2(selectedPosition, pixelRect.height + pixelRect.y));
			}
		}
	}
}