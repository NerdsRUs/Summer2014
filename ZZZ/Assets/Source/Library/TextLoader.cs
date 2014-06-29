using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class TextLoader
{
	public const float UPDATE_RATE = 60;
	const char DELIMITER = '\t';

	Dictionary<string, int> mIndex = new Dictionary<string, int>();
	bool mDataMapInited = false;

	StringReader mRawData;
	int mTokenCount;

	WWW mExternalLoader;
	double mLastUpdateTime = UPDATE_RATE;
	bool mDataLoaded = false;
	bool mAutoRefresh = false;

	string mLastTextHash = "";

	public TextLoader(string name)
	{
		load(name);
	}

	public TextLoader()
	{
	}

	public void load(string name)
	{
		TextAsset data = (TextAsset)Resources.Load("Data/" + name);

		mRawData = new StringReader(data.text);
	}

	public bool loadExternal(string name, out bool needsRefresh, bool force = false)
	{
		needsRefresh = false;

		if (Common.getDateTime().Second % UPDATE_RATE != 0)
		{
			mAutoRefresh = true;
		}

		if (mExternalLoader == null || force || (Time.time > mLastUpdateTime && mAutoRefresh))
		{
			mAutoRefresh = false;
			mDataLoaded = false;
			mLastUpdateTime = Time.time + UPDATE_RATE;

			//Force unique request
			WWWForm form = new WWWForm();
			form.AddField("random", Common.Range(1, Common.BIG_NUMBER));

			mExternalLoader = new WWW(Application.dataPath + name + ".txt", form);
		}

		if (!mDataLoaded && mExternalLoader.isDone)
		{
			if (mExternalLoader.error == null)
			{
				string newHash = Common.getMD5(mExternalLoader.text);
				//if (mLastTextHash != newHash)
				{
					mLastTextHash = newHash;
					mRawData = new StringReader(mExternalLoader.text);

					mDataMapInited = false;
					needsRefresh = true;
				}
			}
			else
			{
				Debug.LogError("WWW stream (" + mExternalLoader + ") had error: " + mExternalLoader.error);
			}

			mDataLoaded = true;
		}

		return mDataLoaded;
	}

	public IEnumerator<Dictionary<string, string>> GetEnumerator()
	{
		string line;

		while ((line = mRawData.ReadLine()) != null)
		{
			string testLine;
			testLine = line.Replace(" ", "");
			testLine = testLine.Replace("\n", "");
			testLine = testLine.Replace("\t", "");

			if (testLine.Length == 0)
			{
				continue;
			}

			string[] lineData = line.Split(DELIMITER);

			if (!mDataMapInited)
			{
				mDataMapInited = true;

				for (int i = 0; i < lineData.Length; i++)
				{
					mIndex[lineData[i]] = i;
				}

				mTokenCount = lineData.Length;
			}
			else
			{
				if (lineData.Length == mTokenCount)
				{
					Dictionary<string, string> tempData = new Dictionary<string, string>();

					foreach (KeyValuePair<string, int> pair in mIndex)
					{
						tempData[pair.Key] = lineData[pair.Value];
					}

					yield return tempData;
				}
				else
				{
					//SQDebug.log("Bad line data: " + line);
				}
			}
		}
	}
}
