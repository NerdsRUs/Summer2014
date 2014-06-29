using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

public class SampleObject : EngineObject
{
	protected int mParameter1;
	public int mParameter2;

	//Strongly typed factory (Should make a new name for each one, since there is no static inheritence
	static public SampleObject NewSampleObject(EngineObject parent, int objectID, int parameter1, int parameter2)
	{
		//Call the default loosey typed constructor (Uses reflection to call Init() with the required parameters)
		return NewObject<SampleObject>(parent, objectID, parameter1, parameter2);
	}

	//Strongly typed initialization (CAN ONLY HAVE ONE Init FUNCTION IN THE CLASS!)
	public void Init(int paramter1, int parameter2)
	{
		mParameter1 = paramter1;
		mParameter2 = parameter2;
	}

	//Defines the parameters to be used for initialization
	override protected void GetInializationParameters()
	{
		//Must be the same order as in the functions above
		mParameterList.Add(mParameter1);
		mParameterList.Add(mParameter2);
	}
}
