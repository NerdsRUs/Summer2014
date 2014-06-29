using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DataLoader<T> where T : DataLoader<T>, new()
{
	static public Dictionary<int, T> mDataList = new Dictionary<int, T>();

	static public void init(string filename)
	{
		TextLoader data = new TextLoader(filename);

		foreach (Dictionary<string, string> dataMap in data)
		{
			T tempData = new T();

			tempData.loadFromTextData(dataMap);
		}
	}

	static public T getData(int ID)
	{
		if (mDataList.ContainsKey(ID))
		{
			return mDataList[ID];
		}
		else
		{
			//SQDebug.log("Couldn't find ID: " + ID);
			return null;
		}
	}

	virtual protected void loadFromTextData(Dictionary<string, string> data)
	{
	}

	virtual protected void addToDataMap(T newData)
	{
	}
}
