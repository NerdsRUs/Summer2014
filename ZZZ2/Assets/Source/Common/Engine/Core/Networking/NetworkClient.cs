using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

public class NetworkClient : NetcodeSender
{
	public int port = 4000;

	override protected void Start()
	{
		base.Start();

		BeginConnection();

		Debug.Log("Start " + name);
	}

	void BeginConnection()
	{
		Network.Connect("127.0.0.1", port);
	}

	void OnConnectedToServer()
	{
		if (enabled)
		{
			Debug.Log("OnConnectedToServer");

			mManager.MakeServer();
		}
	}

	void OnDisconnectedFromServer()
	{
		if (enabled)
		{
			Debug.Log("OnDisconnectedFromServer");

			mManager.MakeOffline();
		}
	}

	void OnFailedToConnect()
	{
		if (enabled)
		{
			Debug.Log("OnFailedToConnect");
		}
	}
}
