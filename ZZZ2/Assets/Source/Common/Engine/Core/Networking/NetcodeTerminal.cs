using UnityEngine;
using System.Collections;
using System.Reflection;
using System;

public class NetcodeTerminal : MonoBehaviour
{
	protected NetCode mTerminal;

	public void Init(NetCode terminal)
	{
		mTerminal = terminal;
	}
}
