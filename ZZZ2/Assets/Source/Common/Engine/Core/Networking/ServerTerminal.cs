using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

public class ServerTerminal : NetcodeTerminal
{
	[RPC] public void ServerRPC(string name, int ID, byte[] data)
	{
		mTerminal.DynamicRPC(name, ID, data);
	}
}
