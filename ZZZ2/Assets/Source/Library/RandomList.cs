using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomList<T> : List<T>
{
	public delegate float WeightFunction(T weightedObject); 

	List<T> mRandomList = new List<T>();
	bool mRandomized = true;
	int mSeed;
	WeightFunction mWeightFunction;

	public RandomList()
	{
	}

	public RandomList(T[] array, bool randomize = true)
	{
		mRandomized = randomize;

		for (int i = 0; i < array.Length; i++)
		{
			this.Add(array[i]);
		}

		randomizeList(Common.Range(0, Common.BIG_NUMBER));
	}

	public void randomizeList(int seed)
	{
		mSeed = seed;
		mRandomList.Clear();

		if (mRandomized)
		{
			List<T> tempList = new List<T>(this);

			while (tempList.Count > 0)
			{
				int random = getNextRandom() % tempList.Count;

				mRandomList.Add(tempList[random]);
				tempList.RemoveAt(random);
			}
		}
		else
		{
			for (int i = 0; i < this.Count; i++)
			{
				mRandomList.Add(this[i]);
			}
		}
	}

	public T getNextItem()
	{
		if (mRandomList.Count == 0)
		{
			randomizeList(getNextRandom());

			if (mRandomList.Count == 0)
			{
				return default(T);
			}
		}

		T tempItem = mRandomList[0];

		mRandomList.RemoveAt(0);

		return tempItem;
	}

	int getNextRandom()
	{
		mSeed = Common.getRandomValueFromSeed(mSeed);

		return mSeed;
	}
}