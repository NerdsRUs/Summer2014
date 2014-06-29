using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

//keeps a static pool of Ts.
public class Pool<T> : List<T>
{
	List<int> mUnusedIDs = new List<int>();
	List<int> mUsedIDs = new List<int>();

	//List<T> mData = new List<T>();
	List<int> mIndexes = new List<int>();

	/*public int Count
	{
		get{ return mData.Count; }
	}*/

	public bool contains(int key)
	{
		return mIndexes.Contains(key);
	}

	public bool hasReserved(int key)
	{
		return mUsedIDs.Contains(key);
	}

	public int getUnsedID()
	{
		if (mUnusedIDs.Count == 0)
		{
			int nextID = 1;

			mUsedIDs.Sort();
			foreach (int ID in mUsedIDs)
			{
				if (ID != nextID)
				{
					return nextID;
				}

				nextID++;
			}

			return nextID;
		}

		mUnusedIDs.Sort();

		return mUnusedIDs[0];
	}

	public void reserveIndex(int index)
	{
		mUnusedIDs.Remove(index);
		if (!mUsedIDs.Contains(index))
		{
			mUsedIDs.Add(index);
		}
	}

	public void clearIndex(int index)
	{
		if (!mUnusedIDs.Contains(index))
		{
			mUnusedIDs.Add(index);
		}
		mUsedIDs.Remove(index);
	}

	public void addAtIndex(int index, T value)
	{
		removeFromIndex(index);

		reserveIndex(index);

		/*if (mData.ContainsKey(index))
		{
			//SQDebug.log("Overwritting data: " + mData[index]);
		}*/

		mIndexes.Add(index);
		Add(value);
	}

	public void removeFromIndex(int index)
	{
		clearIndex(index);

		int dataIndex = mIndexes.IndexOf(index);

		if (Count > dataIndex && dataIndex != -1)
		{
			RemoveAt(dataIndex);
		}
		mIndexes.Remove(index);
	}

	/*public IList<T> asList()
	{
		return mData.asList();
	}*/

	public T this[int i]
	{
		get 
		{ 
			int index = mIndexes.IndexOf(i);

			if (Count > index && index != -1)
			{
				return base[index]; 
			}

			return default(T);
		}
		set { addAtIndex(i, value); }
	}

	/*public IEnumerator<T> GetEnumerator()
	{
		foreach (KeyValuePair<int, T> data in mData)
		{
			//if (data.Value != null)
			//{
				yield return data.Value;
			//}
		}
	}*/

	public T getFirst()
	{
		return base[0];
	}

	public T getIndex(int i)
	{
		return base[i];
	}

	public void clear()
	{
		mIndexes.Clear();
		mUnusedIDs.Clear();
		mUsedIDs.Clear();
		Clear();
	}

	public List<T> getList()
	{
		List<T> tempList = new List<T>();

		for (int i = 0; i < Count; i++)
		{
			tempList.Add(base[i]);
		}

		return tempList;
	}

	public void debug()
	{
		//SQDebug.log("Debug Pool:");
		for (int i = 0; i < Count; i++)
		{
			//SQDebug.log("    " + mIndexes[i] + " -> " + base[i]);
		}
	}
}