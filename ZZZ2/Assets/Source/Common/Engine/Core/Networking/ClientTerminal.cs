﻿using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

public class ClientTerminal : NetcodeTerminal
{
	[RPC] public void DynamicRPC(string name, int ID, byte[] data)
	{
		mTerminal.DynamicRPC(name, ID, data);
	}
}
