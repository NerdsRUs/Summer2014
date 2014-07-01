using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

public class NetworkServer : NetcodeSender
{
	public int maxPlayers = 4;
	public int port = 4000;

	override protected void Start()
	{
		base.Start();

		SetUpServer();

		Debug.Log("Start " + name);
	}

	void SetUpServer()
	{
		Network.InitializeServer(maxPlayers, port, true);
	}

	void OnServerInitialized()
	{
		if (enabled)
		{
			Debug.Log("OnServerInitialized");

			mManager.MakeServer();
		}
	}

	void OnPlayerConnected()
	{
		if (enabled)
		{
			Debug.Log("OnPlayerConnected");
		}
	}

	void OnPlayerDisconnected()
	{
		if (enabled)
		{
			Debug.Log("OnPlayerDisconnected");
		}
	}
}
