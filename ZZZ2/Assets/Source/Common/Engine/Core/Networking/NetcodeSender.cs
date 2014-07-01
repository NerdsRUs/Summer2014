using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

[RequireComponent(typeof(EngineManager))]
public class NetcodeSender : MonoBehaviour
{
	protected EngineManager mManager;

	virtual protected void Start()
	{
		mManager = GetComponent<EngineManager>();
	}
}
