using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TimelineEventParticles : TimelineEvent
{
	public ParticleSystem particleSystem;

	public bool useVariableLength = false;
	
	protected override void Start()
	{
		base.Start();
	}

	public override bool initialize(float lengthOffset = 0)
	{
		if (particleSystem != null)
		{
			particleSystem.time = 0;
			particleSystem.Play();
		}

		return base.initialize(lengthOffset);
	}

#if UNITY_EDITOR
	public override void sample(float samplePoint)
	{
		if (particleSystem != null && !Application.isPlaying)
		{
			float interpolation = samplePoint / getLength();

			if (interpolation >= 0 && interpolation <= 1)
			{
				particleSystem.Simulate(samplePoint);
			}
		}

		base.sample(samplePoint);
	}

	/*override public void beforeSample(Timeline timeline)
	{
		mMagicalBoxEvents.Clear();
		mPlayingMap.Clear();

		foreach (Transform child in timeline.transform)
		{
			TimelineEvent tempEvent = (TimelineEvent)child.GetComponent<TimelineEvent>();

			if (tempEvent != null)
			{
				if (tempEvent is TimelineEventMagicalBox)
				{
					if (((TimelineEventMagicalBox)tempEvent).particleSystem != null && !mMagicalBoxEvents.Contains(((TimelineEventMagicalBox)tempEvent).particleSystem))
					{
						mMagicalBoxEvents.Add(((TimelineEventMagicalBox)tempEvent).particleSystem);
					}
				}
			}
		}
	}

	override public void afterSample(Timeline timeline)
	{
		for (int i = 0; i < mMagicalBoxEvents.Count; i++)
		{
			MBParticleSystem particleSystem = mMagicalBoxEvents[i];

			if (mPlayingMap.ContainsKey(particleSystem))
			{
				particleSystem.Play();
			}
			else
			{
				particleSystem.Stop();
			}

			if (!Selection.activeTransform || getParticleSystem(Selection.activeTransform) != particleSystem)
			{
				if (!particleSystem.Warping)
				{
					float elapsedRealtime = (float)(EditorApplication.timeSinceStartup - mLastTime);
					particleSystem.GlobalTime = (float)EditorApplication.timeSinceStartup;
					particleSystem.DeltaTime = elapsedRealtime;

					mLastTime = EditorApplication.timeSinceStartup;
				}

				particleSystem.mbUpdate();
				particleSystem.mbRender();

				particleSystem.mbEditorCamera = null;

				if (SceneView.lastActiveSceneView)
				{
					SceneView.lastActiveSceneView.Repaint();
				}
			}
		}
	}*/

	protected ParticleSystem getParticleSystem(Transform transform)
	{
		ParticleSystem tempSystem = transform.GetComponent<ParticleSystem>();

		if (tempSystem)
		{
			return tempSystem;
		}

		foreach (Transform child in transform)
		{
			tempSystem = getParticleSystem(child);

			if (tempSystem != null)
			{
				return tempSystem;
			}
		}

		return null;
	}
#endif

	public override bool end()
	{
		if (particleSystem != null)
		{
			particleSystem.Stop();
		}

		return base.end();
	}

	public override bool canModifyLength()
	{
		if (!useVariableLength && particleSystem != null)
		{
			return false;
		}

		return true;
	}

	public override float getLength()
	{
		if (!useVariableLength && particleSystem != null)
		{
			return particleSystem.duration + particleSystem.startLifetime;
		}

		return base.getLength();
	}
}
