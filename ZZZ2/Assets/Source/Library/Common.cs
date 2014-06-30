using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Common
{
	public const int BIG_NUMBER = 9999999;
	public const int BIG_TIME = 36000;


	public const int DAY_RESET_HOUR = 11;

	public const float GAME_WIDTH = 900;
	public const float GAME_HEIGHT = 600;
	public const float DEFAULT_FADE_TME = 0.5f;

	public const string COLOR_HOSTILE = "AA2222";
	public const string COLOR_FRIENDLY = "22AA22";
	public const string COLOR_DESCRIPTION = "DDDD11";
	public const string COLOR_EFFECT = "118811";
	public const string COLOR_GOLD = "FFFF55";
	public const string COLOR_NEGATIVE = "DD1111";
	public const string COLOR_BOP = "E3B419";
	public const string COLOR_BOE = "3ED6C7";
	public static Color COLOR_INACTIVE = new Color(0.5f, 0.5f, 0.5f, 1.0f);

	public static Color COLOR_NORMAL = new Color(0.9f, 0.9f, 0.9f, 1.0f);
	public static Color COLOR_MAGICAL = new Color(0.286f, 0.9176f, 0.3019f, 1.0f);
	public static Color COLOR_RARE = new Color(0.0509f, 0.6823f, 1.0f, 1.0f);
	public static Color COLOR_EPIC = new Color(0.6353f, 0.1804f, 1.0f, 1.0f);
	public static Color COLOR_LEGENDARY = new Color(1.0f, 0.8823f, 0.3058f, 1.0f);
	public static Color COLOR_UNIQUE = new Color(1.0f, 0.5f, .0745f, 1.0f);

	static string mLastScene = "";

	static MD5CryptoServiceProvider mMD5 = new MD5CryptoServiceProvider();

	static public float WORLD_SCALE = 100;

	static private Dictionary<string, object> mServerResources = new Dictionary<string, object>();
	static private Dictionary<string, object> mClientResources = new Dictionary<string, object>();

	static protected Dictionary<string, List<Transform>> mInactiveObjectPool = new Dictionary<string, List<Transform>>();
	static protected Dictionary<Transform, string> mActiveObjectPool = new Dictionary<Transform, string>();

	static Transform mPoolRoot;

	static List<string> mPreloadedObjects = new List<string>();

	static System.Random mRandom = new System.Random();

	static int CLIENT_LAYER = 9;
	static int SERVER_LAYER = 8;

	static EngineManager mServer;
	static EngineManager mClient;

	static public float roundTo(float value, int digits)
	{
		int cutoff = (int)Mathf.Pow(10, digits);

		return Mathf.Floor(value * cutoff + 0.5f) / cutoff;
	}

	static public Vector3 getOffscreenVector()
	{
		return new Vector3(-10000, -10000, 0);
	}

	static public void pauseAnimation(Transform transform, string animation)
	{
		Animation tempAnimation = transform.animation;

		if (tempAnimation != null && tempAnimation[animation])
		{
			tempAnimation[animation].normalizedTime = 0;
			tempAnimation[animation].speed = 0;
		}

		foreach (Transform child in transform)
		{
			pauseAnimation(child, animation);
		}
	}

	static public void unpauseAnimation(Transform transform, string animation)
	{
		Animation tempAnimation = transform.animation;

		if (tempAnimation != null && tempAnimation[animation])
		{
			tempAnimation[animation].speed = 1;
			tempAnimation.Play(animation);
		}

		foreach (Transform child in transform)
		{
			unpauseAnimation(child, animation);
		}
	}

	static public float getAnimationTime(Transform transform, string animation)
	{
		float maxLength = 0;

		Animation tempAnimation = transform.animation;

		if (tempAnimation != null && tempAnimation[animation])
		{
			maxLength = tempAnimation[animation].time;
		}

		foreach (Transform child in transform)
		{
			maxLength = Mathf.Max(maxLength, getAnimationTime(child, animation));
		}

		return maxLength;
	}

	static public float getAnimationLength(Transform transform, string animation)
	{
		float maxLength = 0;

		Animation tempAnimation = transform.animation;

		if (tempAnimation != null && tempAnimation[animation])
		{
			maxLength = tempAnimation[animation].length;
		}

		foreach (Transform child in transform)
		{
			maxLength = Mathf.Max(maxLength, getAnimationLength(child, animation));
		}

		return maxLength;
	}

	static public void playAnimation(Transform transform, string animation, float fadeTime = DEFAULT_FADE_TME, bool reset = false)
	{
		Animation tempAnimation = transform.animation;

		if (tempAnimation != null && tempAnimation[animation])
		{
			tempAnimation[animation].time = 0;

			tempAnimation.CrossFade(animation, fadeTime, PlayMode.StopAll);
		}

		foreach (Transform child in transform)
		{
			playAnimation(child, animation, fadeTime);
		}
	}

	static public bool isPlayingAnimation(Transform transform, string animation)
	{
		Animation tempAnimation = transform.animation;

		if (tempAnimation != null && tempAnimation.IsPlaying(animation))
		{
			return true;
		}

		foreach (Transform child in transform)
		{
			if (isPlayingAnimation(child, animation))
			{
				return true;
			}
		}

		return false;
	}

	static public bool isPausedAnimation(Transform transform, string animation)
	{
		Animation tempAnimation = transform.animation;

		if (tempAnimation != null && (tempAnimation[animation].normalizedTime == 0 || tempAnimation[animation].speed == 0))
		{
			return true;
		}

		foreach (Transform child in transform)
		{
			if (isPausedAnimation(child, animation))
			{
				return true;
			}
		}

		return false;
	}

	static public void setAnimationTime(Transform transform, string animation, float time)
	{
		Animation tempAnimation = transform.animation;

		if (tempAnimation != null && tempAnimation[animation])
		{
			tempAnimation[animation].time = time;
		}

		foreach (Transform child in transform)
		{
			setAnimationTime(child, animation, time);
		}
	}

	static public void setParticleScale(Transform transform, Vector3 scale)
	{
		if (transform.GetComponent<ParticleEmitter>() != null)
		{
			transform.localScale = scale;
		}

		foreach (Transform child in transform)
		{
			setParticleScale(child, scale);
		}
	}

	static string mObjectName;
	static public Transform getTransformOfName(Transform transform, string name)
	{
		mObjectName = transform.name;

		if (mObjectName == name)
		{
			return transform;
		}

		foreach (Transform child in transform)
		{
			Transform tempTransform = getTransformOfName(child, name);

			if (tempTransform)
			{
				return tempTransform;
			}
		}

		return null;
	}

	static public string getColorString(Color color)
	{
		return ((int)(color.r * 255)).ToString("X2") + ((int)(color.g * 255)).ToString("X2") + ((int)(color.b * 255)).ToString("X2");
	}

	static public Transform getChildFromPath(Transform transform, string path)
	{
		string childPath = "";
		string childName = "";

		if (path.IndexOf("/") > -1)
		{
			childPath = path.Substring(path.IndexOf("/") + 1);
			childName = path.Substring(0, path.IndexOf("/"));
		}
		else
		{
			childPath = "";
			childName = path;
		}

		if (childName != "")
		{
			foreach (Transform child in transform)
			{
				if (child.name == childName)
				{
					return getChildFromPath(child, childPath);
				}
			}
		}

		return transform;
	}

	static public Mesh getMesh(Transform transform)
	{
		if (transform.GetComponent<MeshFilter>() != null)
		{
			return transform.GetComponent<MeshFilter>().sharedMesh;
		}

		foreach (Transform child in transform)
		{
			Mesh tempMesh = getMesh(child);

			if (tempMesh != null)
			{
				return tempMesh;
			}
		}

		return null;
	}

	public static Transform getRoot(Transform searchObject)
	{
		if (searchObject.parent != null)
		{
			return getRoot(searchObject.parent);
		}

		return searchObject;
	}

	public static void getAllComponents<T>(Transform searchObject, List<T> list, bool top = true) where T : Component
	{
		if (top)
		{
			list.Clear();
		}

		foreach (Transform child in searchObject)
		{
			getAllComponents(child, list, false);
		}

		T[] tempMaterial = searchObject.GetComponents<T>();

		for (int i = 0; i < tempMaterial.Length; i++)
		{
			list.Add(tempMaterial[i]);
		}
	}

	public static void getAllComponentsByName<T>(Transform searchObject, List<T> list, string name, bool top = true) where T : Component
	{
		if (top)
		{
			list.Clear();
		}

		foreach (Transform child in searchObject)
		{
			getAllComponentsByName(child, list, name, false);
		}

		T[] tempMaterial = searchObject.GetComponents<T>();

		for (int i = 0; i < tempMaterial.Length; i++)
		{
			if (tempMaterial[i].name == name)
			{
				list.Add(tempMaterial[i]);
			}
		}
	}

	static public Renderer getRenderer(Transform transform)
	{
		if (transform.renderer != null && transform.renderer is SkinnedMeshRenderer)
		{
			return transform.renderer;
		}

		foreach (Transform child in transform)
		{
			Renderer tempRenderer = getRenderer(child);

			if (tempRenderer != null)
			{
				return tempRenderer;
			}
		}

		return null;
	}

	static public Transform getFirstByName(Transform transform, string name)
	{
		if (transform.name == name)
		{
			return transform;
		}

		foreach (Transform child in transform)
		{
			Transform tempTransform = getFirstByName(child, name);

			if (tempTransform != null)
			{
				return tempTransform;
			}
		}

		return null;
	}

	static public Renderer getFirstRenderer(Transform transform)
	{
		if (transform.renderer != null && transform.renderer is Renderer)
		{
			return transform.renderer;
		}

		foreach (Transform child in transform)
		{
			Renderer tempRenderer = getFirstRenderer(child);

			if (tempRenderer != null)
			{
				return tempRenderer;
			}
		}

		return null;
	}

	static public string getMD5(string text)
	{
		byte[] data = mMD5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text));

		System.Text.StringBuilder result = new System.Text.StringBuilder();
		foreach (byte b in data)
		{
			result.Append(b.ToString("x2"));
		}

		return result.ToString();
	}

	static public DateTime getDayStart(DateTime time)
	{
		return time.Date;
	}

	static public DateTime getDateTime()
	{
		return TimeZoneInfo.ConvertTimeToUtc(DateTime.Now);
	}

	static public DateTime getLocalDateTime()
	{
		return TimeZoneInfo.ConvertTimeToUtc(DateTime.Now);
	}

	static public DateTime getDateTimeFromTimestamp(uint timestamp)
	{
		DateTime origin = getEpoch();
		return origin.AddSeconds(timestamp);
	}

	static public uint getTimestampFromDateTime(DateTime dateTime)
	{
		DateTime origin = getEpoch();
		TimeSpan diff = dateTime - origin;

		return (uint)diff.TotalSeconds;
	}

	static public int getDay()
	{
		return getDay(getDateTime());
	}

	static public int getDay(DateTime dateTime)
	{
		return (dateTime - (getEpoch().AddHours(DAY_RESET_HOUR))).Days;
	}

	static public float dateTimeToTimeLeft(DateTime time)
	{
		return (float)(time - Common.getDateTime()).TotalSeconds;
	}

	static public string getMySQLDateTime(DateTime dateTime)
	{
		return "'" + dateTime.ToString("yyyy-MM-dd HH:mm:ss") + "'";
	}

	static public string getDateDisplay(DateTime dateTime)
	{
		dateTime = dateTime.ToLocalTime();

		return dateTime.ToString("yyyy/MM/dd HH:mm");
	}

	static public string getDateDisplayShort(DateTime dateTime)
	{
		dateTime = dateTime.ToLocalTime();

		if (getDay() != getDay(dateTime))
		{
			return dateTime.ToString("yyyy/MM/dd");
		}

		return dateTime.ToString("HH:mm");
	}

	static public string getDateTimeString(DateTime dateTime, string format)
	{
		dateTime = dateTime.ToLocalTime();

		return dateTime.ToString(format);
	}

	static public string getTimeDisplayShort(int inputSeconds)
	{
		int day = 60 * 60 * 24;
		int hour = 60 * 60;
		int minute = 60;

		int days = inputSeconds / (day);
		int hours = (inputSeconds % day) / hour;
		int minutes = (inputSeconds % hour) / minute;
		int seconds = (inputSeconds % minute);

		string dayString = "";
		string hourString = "";
		string minuteString = "";
		string secondString = "";

		if (days > 0)
		{
			dayString = days.ToString("00") + ":";
			hourString = hours.ToString("00") + ":";
			minuteString = minutes.ToString("00") + ":";
			secondString = seconds.ToString("00");
		}
		else if (hours > 0)
		{
			hourString = hours.ToString("00") + ":";
			minuteString = minutes.ToString("00") + ":";
			secondString = seconds.ToString("00");
		}
		else if (minutes > 0)
		{
			hourString = hours.ToString("00") + ":";
			minuteString = minutes.ToString("00") + ":";
			secondString = seconds.ToString("00");
		}
		else if (seconds > 0)
		{
			hourString = hours.ToString("00") + ":";
			minuteString = minutes.ToString("00") + ":";
			secondString = seconds.ToString("00");
		}

		return dayString + hourString + minuteString + secondString;
	}

	static public string getTimeDisplay(int inputSeconds)
	{
		int day = 60 * 60 * 24;
		int hour = 60 * 60;
		int minute = 60;

		int days = inputSeconds / (day);
		int hours = (inputSeconds % day) / hour;
		int minutes = (inputSeconds % hour) / minute;
		int seconds = (inputSeconds % minute);

		string dayString = "";
		string hourString = "";
		string minuteString = "";
		string secondString = "";

		if (days > 0)
		{
			dayString = days + "d ";
		}
		if (hours > 0)
		{
			hourString = hours + "h ";
		}
		if (minutes > 0)
		{
			minuteString = minutes + "m ";
		}
		if (seconds > 0)
		{
			secondString = seconds + "s ";
		}

		return dayString + hourString + minuteString + secondString;
	}

	static public string getTimeDisplayAbbreviated(int inputSeconds)
	{
		int month = 60 * 60 * 24 * 30;
		int week = 60 * 60 * 24 * 7;
		int day = 60 * 60 * 24;
		int hour = 60 * 60;
		int minute = 60;

		int months = inputSeconds / (month);
		int weeks = (inputSeconds % month) / (week);
		int days = (inputSeconds % week) / (day);
		int hours = (inputSeconds % day) / hour;
		int minutes = (inputSeconds % hour) / minute;
		int seconds = (inputSeconds % minute);

		string monthString = "";
		string weekString = "";
		string dayString = "";
		string hourString = "";
		string minuteString = "";
		string secondString = "";

		int values = 0;

		if (months > 0 && values < 2)
		{
			monthString = months + (months == 1 ? " month " : " months ");
			values++;
		}
		if (weeks > 0 && values < 2)
		{
			weekString = weeks + (weeks == 1 ? " week " : " weeks ");
			values++;
		}
		if (days > 0 && values < 2)
		{
			dayString = days + (days == 1 ? " day " : " days ");
			values++;
		}
		if (hours > 0 && values < 2)
		{
			hourString = hours + (hours == 1 ? " hour " : " hours ");
			values++;
		}
		if (minutes > 0 && values < 2)
		{
			minuteString = minutes + (minutes == 1 ? " minute " : " minutes ");
			values++;
		}
		if (seconds > 0 && values < 2)
		{
			secondString = seconds + (seconds == 1 ? " second " : " seconds ");
			values++;
		}

		return monthString + weekString + dayString + hourString + minuteString + secondString;
	}

	static public string getTimeDisplayAbbreviatedShort(int inputSeconds)
	{
		int month = 60 * 60 * 24 * 30;
		int week = 60 * 60 * 24 * 7;
		int day = 60 * 60 * 24;
		int hour = 60 * 60;
		int minute = 60;

		int months = inputSeconds / (month);
		int weeks = (inputSeconds % month) / (week);
		int days = (inputSeconds % week) / (day);
		int hours = (inputSeconds % day) / hour;
		int minutes = (inputSeconds % hour) / minute;
		int seconds = (inputSeconds % minute);

		string monthString = "";
		string weekString = "";
		string dayString = "";
		string hourString = "";
		string minuteString = "";
		string secondString = "";

		int values = 0;

		if (months > 0 && values < 2)
		{
			monthString = months + "m ";
			values++;
		}
		if (weeks > 0 && values < 2)
		{
			weekString = weeks + "w ";
			values++;
		}
		if (days > 0 && values < 2)
		{
			dayString = days + "d ";
			values++;
		}
		if (hours > 0 && values < 2)
		{
			hourString = hours + "h ";
			values++;
		}
		if (minutes > 0 && values < 2)
		{
			minuteString = minutes + "m ";
			values++;
		}
		if (seconds > 0 && values < 2)
		{
			secondString = seconds + "s";
			values++;
		}

		return monthString + weekString + dayString + hourString + minuteString + secondString;
	}

	static public string getTimeDisplayLong(int inputSeconds)
	{
		int day = 60 * 60 * 24;
		int hour = 60 * 60;
		int minute = 60;

		int days = inputSeconds / (day);
		int hours = (inputSeconds % day) / hour;
		int minutes = (inputSeconds % hour) / minute;
		int seconds = (inputSeconds % minute);

		string dayString = "";
		string hourString = "";
		string minuteString = "";
		string secondString = "";

		/*if (days > 0)
		{
			dayString = days.ToString("00");
		}*/
		hourString = hours.ToString("00");
		minuteString = minutes.ToString("00");
		secondString = seconds.ToString("00");

		return hourString + ":" + minuteString + ":" + secondString;
	}

	static public DateTime getEpoch()
	{
		return new DateTime(1986, 1, 1);
	}

	static public Transform loadResource(string path, bool silent = false)
	{
		UnityEngine.Object tempObject;

		if (mClientResources.ContainsKey(path))
		{
			tempObject = (GameObject)mClientResources[path];
		}
		else
		{
			tempObject = (GameObject)Resources.Load(path);

			if (tempObject != null)
			{
				mClientResources[path] = tempObject;
			}
			else
			{
				if (!silent)
				{
					Debug.LogError("Could not load resource: " + path);
				}
				return null;
			}
		}

		//SQDebug.log2("Instanticate: " + tempObject.name);
		return ((GameObject)GameObject.Instantiate(tempObject)).transform;
	}

	/*static public bool shouldPool(string path)
	{
		if (mServerResources.ContainsKey(path))
		{
			return ((Component)mServerResources[path]).GetComponent<DontPool>() == null;
		}
		else
		{
			GameObject tempGameObject = (GameObject)Resources.Load(path);

			if (tempGameObject != null)
			{
				return tempGameObject.GetComponent<DontPool>() == null;
			}
			else
			{
				return false;
			}
		}
	}*/

	static public T loadResourceDefination<T>(string path, bool silent = false) where T : Component
	{
		T tempObject;

		if (mServerResources.ContainsKey(path))
		{
			tempObject = (T)mServerResources[path];
		}
		else
		{
			GameObject tempGameObject = (GameObject)Resources.Load(path);

			if (tempGameObject != null)
			{
				tempObject = (T)tempGameObject.GetComponent<T>();

				if (tempObject != null)
				{
					mServerResources[path] = tempObject;
				}
				else
				{
					return default(T);
				}
			}
			else
			{
				return default(T);
			}
		}

		return tempObject;
	}

	static public List<T> loadResourceDefinations<T>(string path, bool silent = false) where T : Component
	{
		List<T> tempObjects;

		if (mServerResources.ContainsKey(path))
		{
			if (mServerResources[path] is List<T>)
			{
				tempObjects = (List<T>)mServerResources[path];
			}
			else
			{
				tempObjects = new List<T>();

				tempObjects.Add((T)mServerResources[path]);
			}
		}
		else
		{
			tempObjects = new List<T>();

			UnityEngine.Object[] tempGameObjects = Resources.LoadAll(path);
			T tempObject;
			GameObject tempGameObject;

			for (int i = 0; i < tempGameObjects.Length; i++)
			{
				tempGameObject = (GameObject)tempGameObjects[i];

				if (tempGameObject != null)
				{
					tempObject = (T)tempGameObject.GetComponent<T>();

					if (tempObject != null)
					{
						tempObjects.Add(tempObject);
					}
				}
			}

			mServerResources[path] = tempObjects;
		}

		return tempObjects;
	}

	static public Rect getScreenRect()
	{
		return new Rect(0, 0, Screen.width, Screen.height);
	}

	//clears user text of injection attacks and wraps in quotes for paramter comparison
	static public string getSecureParameter(object text)
	{
		if (text != null)
		{
			return "'" + text.ToString().Replace("'", "''") + "'";
		}
		else
		{
			return "''";
		}
	}

	static public Vector3 getRelativeScale(Transform transfrom, Transform finalTransform, Vector3 currentScale)
	{
		currentScale.x *= transfrom.localScale.x;
		currentScale.y *= transfrom.localScale.y;
		currentScale.z *= transfrom.localScale.z;

		if (transfrom.parent == null || transfrom == finalTransform)
		{
			return currentScale;
		}
		else
		{
			return getRelativeScale(transfrom.parent, finalTransform, currentScale);
		}
	}

	static public string getTimeLeftString(double timeLeft)
	{
		if (timeLeft > 60 * 60)
		{
			return (int)(timeLeft / (60 * 60)) + "h";
		}
		else if (timeLeft > 60)
		{
			return (int)(timeLeft / 60) + "m";
		}
		else if (timeLeft > 10)
		{
			return (int)(timeLeft) + "s";
		}
		else
		{
			return string.Format("{0:0}s", timeLeft);
		}
	}

	static public string getLongTimeLeftString(float timeLeft)
	{
		if (timeLeft > 60 * 60 * 24)
		{
			return (int)(timeLeft / (60 * 60 * 24)) + "d " + (int)((timeLeft % (60 * 60 * 24)) / (60 * 60)) + "h"; ;
		}
		else if (timeLeft > 60 * 60)
		{
			return (int)(timeLeft / (60 * 60)) + "h " + (int)((timeLeft % (60 * 60)) / 60) + "m"; ;
		}
		else if (timeLeft > 60)
		{
			return (int)(timeLeft / 60) + "m " + (int)(timeLeft % 60) + "s"; ;
		}
		else
		{
			return (int)(timeLeft) + "s";
		}
	}

	static public string getCooldownString(float timeLeft)
	{
		if (timeLeft > 60 * 60)
		{
			return (int)(timeLeft / (60 * 60)) + "h";
		}
		else if (timeLeft > 60)
		{
			return (int)(timeLeft / 60) + "m";
		}
		else
		{
			return string.Format("{0:0.0}s", timeLeft);
		}
	}

	static public string getCommaInt(int number)
	{
		/*string finalString = "" + number;

		int nextComma = 3;
		while (finalString.Length > nextComma)
		{
			finalString = finalString.Insert(finalString.Length - nextComma, ",");

			nextComma += 4;
		}*/

		return number.ToString("n0");
	}

	static public string getFormatedInt(int number, int maxDigits = 3)
	{
		int digits = 1;
		long value = 10;

		while (value <= number)
		{
			value *= 10;
			digits++;
		}

		string finalString = getCommaInt(number);

		if (digits > maxDigits)
		{
			//get 3 most significant digits
			int significantDigits = number / Mathf.Max((int)(value / 1000), 1);

			float finalValue = significantDigits;

			int unitID = (digits - 1) / 3;

			if (digits > maxDigits)
			{
				finalValue /= Mathf.Pow(10, 3 - (digits - unitID * 3));
			}

			finalString = "" + finalValue;

			switch (unitID)
			{
				case 1:
					finalString += "k";
					break;

				case 2:
					finalString += "m";
					break;

				case 3:
					finalString += "b";
					break;

				case 4:
					finalString += "t";
					break;
			}
		}

		return finalString;
	}

	static public string explodeByKey(SortedDictionary<int, SortedDictionary<string, object>> data, string key)
	{
		string explodeString = "";

		for (int i = 0; i < data.Count; i++)
		{
			explodeString += data[i][key];

			if (i < data.Count - 1)
			{
				explodeString += ", ";
			}
		}

		return explodeString;
	}

	static public string explode(List<int> data, string delimiter = ",")
	{
		string explodeString = "";

		for (int i = 0; i < data.Count; i++)
		{
			explodeString += data[i];

			if (i < data.Count - 1)
			{
				explodeString += delimiter;
			}
		}

		return explodeString;
	}

	static public void implode(string data, ref List<int> list, string delimiter = ",")
	{
		int index = data.IndexOf(delimiter);
		string part;

		list.Clear();

		while (index != -1)
		{
			part = data.Substring(0, index);
			data = data.Substring(index + 1);

			list.Add(int.Parse(part));

			index = data.IndexOf(delimiter);
		}

		if (data.Length > 0)
		{
			list.Add(int.Parse(data));
		}
	}

	static public string getUsernameHTML(string userName, int userID, int viewingUser)
	{
		if (userID <= 0 || userID == viewingUser)
		{
			return userName;
		}
		else
		{
			return "<url=UserContext:" + userID + ">[" + userName + "]</url>";
		}
	}

	static public string getRandomString(int minCharacter = 5, int maxCharacters = 10)
	{
		string newString = "";

		int characters = Common.Range(minCharacter, maxCharacters);

		for (int i = 0; i < characters; i++)
		{
			newString += (char)(65 + Common.Range(0, 26));
		}

		return newString;
	}

	static public void getSprites(Transform transform, ref List<Renderer> list)
	{
		Renderer tempSprite = transform.GetComponent<Renderer>();

		if (tempSprite != null)
		{
			list.Add(tempSprite);
		}

		foreach (Transform child in transform)
		{
			getSprites(child, ref list);
		}
	}

	static public void setActive(Transform transform, bool value)
	{
		transform.gameObject.active = value;

		foreach (Transform child in transform)
		{
			setActive(child, value);
		}
	}

	/*static public void showObject(Transform transform, bool automatic = false, bool checkParent = false)
	{
		Renderer renderer = transform.GetComponent<Renderer>();

		if (renderer != null && (!automatic || renderer.GetComponent<DontHideOrShow>() == null))
		{
			renderer.enabled = true;
		}

		HidableSprite hideSprite = transform.GetComponent<HidableSprite>();

		if (hideSprite != null)
		{
			hideSprite.showObject();
		}

		for (int i = 0; i < transform.childCount; i++)
		{
			showObject(transform.GetChild(i), automatic);
		}
	}*/

	/*static public void showObject(Transform transform, bool automatic = false, bool checkParent = false)
	{
		Renderer renderer = transform.GetComponent<Renderer>();
		Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();

		if (renderer != null && (!automatic || renderer.GetComponent<DontHideOrShow>() == null))
		{
			renderer.enabled = true;
		}

		for (int i = 0; i < renderers.Length; i++)
		{
			if (!automatic || renderers[i].GetComponent<DontHideOrShow>() == null)
			{
				renderers[i].enabled = true;
			}
		}

		HidableSprite hideSprite = transform.GetComponent<HidableSprite>();
		HidableSprite[] hideSprites = transform.GetComponentsInChildren<HidableSprite>();

		if (hideSprite != null)
		{
			hideSprite.showObject();
		}

		for (int i = 0; i < hideSprites.Length; i++)
		{
			hideSprites[i].showObject();
		}
	}*/

	/*static public void hideObject(Transform transform, bool automatic = false, bool hideParticles = true)
	{
		Renderer renderer = transform.GetComponent<Renderer>();

		if (renderer != null && (!automatic || renderer.GetComponent<DontHideOrShow>() == null))
		{
			if ((hideParticles || !(renderer is ParticleRenderer)))
			{
				renderer.enabled = false;
			}
		}

		HidableSprite hideSprite = transform.GetComponent<HidableSprite>();

		if (hideSprite != null)
		{
			hideSprite.showObject();
		}

		for (int i = 0; i < transform.childCount; i++)
		{
			hideObject(transform.GetChild(i), automatic);
		}
	}*/

	/*static public void hideObject(Transform transform, bool automatic = false, bool hideParticles = true)
	{
		Renderer renderer = transform.GetComponent<Renderer>();
		Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();

		if (renderer != null && (!automatic || renderer.GetComponent<DontHideOrShow>() == null))
		{
			renderer.enabled = false;
		}

		for (int i = 0; i < renderers.Length; i++)
		{
			if ((hideParticles || renderers[i] is ParticleRenderer) &&
				(!automatic || renderers[i].GetComponent<DontHideOrShow>() == null))
			{
				renderers[i].enabled = false;
			}
		}

		HidableSprite hideSprite = transform.GetComponent<HidableSprite>();
		HidableSprite[] hideSprites = transform.GetComponentsInChildren<HidableSprite>();

		if (hideSprite != null)
		{
			hideSprite.hideObject();
		}

		for (int i = 0; i < hideSprites.Length; i++)
		{
			hideSprites[i].hideObject();
		}
	}*/

	static public T getComponentInParent<T>(Transform transform) where T : Component
	{
		T tempComponent = transform.GetComponent<T>();
		if (tempComponent != null)
		{
			return tempComponent;
		}

		if (transform.parent)
		{
			return getComponentInParent<T>(transform.parent);
		}

		return default(T);
	}

	static public bool isParentOf(Transform child, Transform parent)
	{
		if (child.parent == null)
		{
			return false;
		}

		if (child.parent == parent)
		{
			return true;
		}

		return isParentOf(child.parent, parent);
	}

	static public void preloadObject(string path)
	{
		Transform tempObject = null;

		if (mInactiveObjectPool.ContainsKey(path) && mInactiveObjectPool[path].Count > 0)
		{
			//Already loaded, do nothing
			return;
		}
		else
		{
			if (!mActiveObjectPool.ContainsValue(path))
			{
				tempObject = Common.loadResource(path);

				//Remove object
				returnObjectToPool(tempObject.gameObject);
			}
		}
	}

	static public int getActiveObjects()
	{
		return mActiveObjectPool.Count;
	}

	static public int getInactiveObjects()
	{
		return mInactiveObjectPool.Count;
	}

	static public Transform getNewObject(string path, bool dontSave = false)
	{
		Transform tempObject = null;

		/*if (Common.shouldPool(path) && !dontSave)
		{
			if (mInactiveObjectPool.ContainsKey(path) && mInactiveObjectPool[path].Count > 0)
			{
				tempObject = mInactiveObjectPool[path][0];
				mInactiveObjectPool[path].RemoveAt(0);

				if (tempObject != null)
				{
					tempObject.gameObject.SetActiveRecursively(true);
				}
				else
				{
					//SQDebug.log("Object " + path + " was deleted prematurely.");

					tempObject = Common.loadResource(path);
				}
			}
			else
			{
				tempObject = Common.loadResource(path);
			}

			if (!mActiveObjectPool.ContainsKey(tempObject))
			{
				mActiveObjectPool[tempObject] = path;
			}
		}
		else
		{
			tempObject = Common.loadResource(path);
		}

		if (tempObject != null)
		{
			Common.showObject(tempObject, true);
		}*/

		tempObject = Common.loadResource(path);

		return tempObject;
	}

	static public UnityEngine.Object Instantiate(UnityEngine.Object baseObject)
	{
		return Transform.Instantiate(baseObject);
	}

	static public Transform instantiateNewObject(Transform prefab, bool allowPool = true)
	{
		string path = prefab.gameObject.GetInstanceID().ToString();
		Transform tempObject = null;

		/*if (prefab.GetComponent<DontPool>() == null && allowPool)
		{
			if (mInactiveObjectPool.ContainsKey(path) && mInactiveObjectPool[path].Count > 0 && mInactiveObjectPool[path][0] != null)
			{
				tempObject = mInactiveObjectPool[path][0];
				mInactiveObjectPool[path].RemoveAt(0);

				tempObject.gameObject.SetActiveRecursively(true);
			}
			else
			{
				tempObject = (Transform)Common.Instantiate(prefab);
			}

			if (!mActiveObjectPool.ContainsKey(tempObject))
			{
				mActiveObjectPool[tempObject] = path;
			}
		}
		else
		{
			tempObject = (Transform)Common.Instantiate(prefab);
		}*/

		tempObject = (Transform)Common.Instantiate(prefab);

		return tempObject;
	}

	static public void invalidatePoolObject(GameObject removeObject)
	{
		if (mActiveObjectPool.ContainsKey(removeObject.transform))
		{
			mActiveObjectPool.Remove(removeObject.transform);
		}
	}

	static public void returnObjectToPool(GameObject removeObject)
	{
		if (mPoolRoot == null)
		{
			GameObject tempObejct = GameObject.Find("PoolRoot");
			if (tempObejct)
			{
				mPoolRoot = tempObejct.transform;
			}

			if (mPoolRoot == null)
			{
				mPoolRoot = new GameObject("PoolRoot").transform;
			}
		}

		if (mActiveObjectPool.ContainsKey(removeObject.transform))
		{
			string path = mActiveObjectPool[removeObject.transform];

			if (!mInactiveObjectPool.ContainsKey(path))
			{
				mInactiveObjectPool[path] = new List<Transform>();
			}

			mInactiveObjectPool[path].Add(removeObject.transform);
			mActiveObjectPool.Remove(removeObject.transform);

			removeObject.SetActiveRecursively(false);
			removeObject.transform.parent = mPoolRoot;


			removeObject.transform.position = new Vector3(-BIG_NUMBER, -BIG_NUMBER);

			//Common.hideObject(removeObject.transform, true);
		}
		else
		{
			if (removeObject.transform.parent != mPoolRoot)
			{
				GameObject.Destroy(removeObject);
			}
		}
	}

	static public void removeInactiveObjects()
	{
		foreach (KeyValuePair<string, List<Transform>> pair in mInactiveObjectPool)
		{
			for (int i = 0; i < pair.Value.Count; i++)
			{
				if (pair.Value[i] != null)
				{
					if (pair.Value[i].transform.parent == mPoolRoot)
					{
						GameObject.Destroy(pair.Value[i].gameObject);
					}
				}
			}
		}

		mInactiveObjectPool.Clear();
	}

	

	static public int getTimeScore(float seconds)
	{
		return Common.BIG_TIME - (int)(seconds * 10);
	}

	static public string getTimeString(int score)
	{
		int deciseconds = (Common.BIG_TIME - score) % 10;
		int seconds = (Common.BIG_TIME - score) / 10;
		int minutes = seconds / 60;
		int hours = minutes / 60;
		minutes %= 60;
		seconds %= 60;

		string time = "";

		if (hours > 0)
		{
			time += hours.ToString("00") + ":";
		}
		time += minutes.ToString("00") + ":" + seconds.ToString("00") + "." + deciseconds;

		return time;
	}

	static public int getRandomValueFromSeed(int seed, int iterations)
	{
		System.Random random = new System.Random(seed);

		for (int i = 0; i < iterations; i++)
		{
			random.Next();
		}

		return random.Next();
	}

	static public int getRandomValueFromSeed(int seed)
	{
		System.Random random = new System.Random(seed);

		return random.Next();
	}

	static public bool checkRandom(double rate, int seed = -1)
	{
		if (seed == -1)
		{
			seed = Common.Range(0, Common.BIG_NUMBER);
		}

		return getRandomValueFromSeed(seed) % 10000000 < (double)10000000 * rate;
	}

	static void replaceSection(Transform newSection, Transform oldSection)
	{
		for (int i = 0; i < oldSection.GetChildCount(); i++)
		{
			Transform child = oldSection.GetChild(i);

			Transform tempTransform;

			if ((tempTransform = newSection.Find(child.name)) == null)
			{
				tempTransform = child;

				tempTransform.parent = newSection;
				tempTransform.name = child.name;
				tempTransform.localPosition = child.localPosition;
				tempTransform.localRotation = child.localRotation;
				tempTransform.localScale = child.localScale;

				i--;
			}
			else
			{
				replaceSection(tempTransform, child);
			}
		}
	}

	static public void setRandom(int seed)
	{
		mRandom = new System.Random(seed);
	}

	static public int Range(int x, int y)
	{
		return (int)(x + (y - x) * mRandom.NextDouble());
	}

	static public float Range(float x, float y)
	{
		return (float)(x + (y - x) * mRandom.NextDouble());
	}

	/*static public AudioController getAudioController(bool forceRefresh = false)
	{
		if (Application.isPlaying || Application.isLoadingLevel)
		{
			return AudioController.Instance;
		}
		else
		{
#if UNITY_EDITOR
			if (mEditorController == null || mLastScene != UnityEditor.EditorApplication.currentScene || forceRefresh)
			{
				mLastScene = UnityEditor.EditorApplication.currentScene;

				GameObject tempObject = GameObject.Find("AudioController");

				if (tempObject == null)
				{
					UnityEditor.EditorUtility.DisplayDialog("No AudioController", "Please add \"FinalAssets/Audio/AudioController\" to the scene.", "Okay");
				}
				else
				{
					mEditorController = tempObject.GetComponent<AudioController>();
				}
			}

			return mEditorController;
#endif
		}

		return null;
	}*/

	static public string getBonusTooltip(int baseStat, int bonusStat)
	{
		string bonus = "";

		if (bonusStat > 0)
		{
			bonus = "<" + Common.COLOR_EFFECT + "> +" + bonusStat + "<->";
		}
		else if (bonusStat < 0)
		{
			bonus = "<" + Common.COLOR_NEGATIVE + "> -" + (-bonusStat) + "<->";
		}

		return baseStat + bonus;
	}

	static public string getBonusDisplay(int baseStat, int bonusStat)
	{
		string bonus = "";

		if (bonusStat > 0)
		{
			bonus = "<" + Common.COLOR_EFFECT + ">" + (baseStat + bonusStat) + "<->";
		}
		else if (bonusStat < 0)
		{
			bonus = "<" + Common.COLOR_NEGATIVE + ">" + (baseStat + bonusStat) + "<->";
		}
		else
		{
			bonus += baseStat;
		}

		return bonus;
	}

	static public string getTotalDisplay(float totalStat, float baseStat, string format = "0.0", string ending = "")
	{
		string bonus = "";

		if (baseStat < totalStat)
		{
			bonus = "<" + Common.COLOR_EFFECT + ">" + (totalStat).ToString(format) + ending + "<->";
		}
		else if (baseStat > totalStat)
		{
			bonus = "<" + Common.COLOR_NEGATIVE + ">" + (totalStat).ToString(format) + ending + "<->";
		}
		else
		{
			bonus = totalStat.ToString(format) + ending;
		}

		return bonus;
	}

	static public string getTotalTooltip(float totalStat, float baseStat, string format = "0.0", string ending = "")
	{
		string bonus = "";

		if (baseStat < totalStat)
		{
			bonus = baseStat.ToString(format) + "<" + Common.COLOR_EFFECT + ">+" + (totalStat - baseStat).ToString(format) + "<->" + ending;
		}
		else if (baseStat > totalStat)
		{
			bonus = baseStat.ToString(format) + "<" + Common.COLOR_NEGATIVE + ">-" + (-(totalStat - baseStat)).ToString(format) + "<->" + ending;
		}
		else
		{
			bonus = totalStat.ToString(format) + ending;
		}

		return bonus;
	}

	static public string getStatDisplay(float totalStat, string format = "0.0", string ending = "")
	{
		if (totalStat > 0)
		{
			return "<" + Common.COLOR_EFFECT + ">" + totalStat.ToString(format) + ending + "<->";
		}

		return totalStat.ToString(format) + ending; ;
	}

	static public string float2Stars(float value, int stars)
	{
		string starString = "";
		for (int i = 0; i < stars; i++)
		{
			if (value > (float)i / (float)stars)
			{
				starString += "<star>";
			}
			else
			{
				starString += "<estar>";
			}
		}

		return starString;
	}

	static public object createOrLoad(string fileName, string className)
	{
		System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

		/*if (Application.isEditor && RunServer.mDebugMode)
		{
			fileName += ".cs";

#if UNITY_EDITOR
			try
			{
				using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
				using (TextReader reader = new StreamReader(stream))
				{
					System.CodeDom.Compiler.CodeDomProvider provider = System.CodeDom.Compiler.CodeDomProvider.CreateProvider("CSharp");

					System.CodeDom.Compiler.CompilerParameters compilerParameters = new System.CodeDom.Compiler.CompilerParameters
					{
						GenerateExecutable = false,
						GenerateInMemory = true,
						TreatWarningsAsErrors = false,
						CompilerOptions = "/optimize",
					};

					// Add references to all the assemblies we might need.
					System.Reflection.Assembly executingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
					compilerParameters.ReferencedAssemblies.Add(executingAssembly.Location);

					foreach (System.Reflection.AssemblyName assemblyName in executingAssembly.GetReferencedAssemblies())
					{
						compilerParameters.ReferencedAssemblies.Add(System.Reflection.Assembly.Load(assemblyName).Location);
					}

					// Invoke compilation of the source file.
					System.CodeDom.Compiler.CompilerResults compilerResutls = provider.CompileAssemblyFromSource(compilerParameters, reader.ReadToEnd());

					if (compilerResutls.Errors.Count > 0)
					{
						// Display compilation errors.
						System.Text.StringBuilder builder = new System.Text.StringBuilder();
						foreach (System.CodeDom.Compiler.CompilerError ce in compilerResutls.Errors)
						{
							builder.Append(ce.ToString());
							builder.Append("\n");
						}

						UnityEngine.Debug.LogError("Script compilation error:\n" + builder.ToString());
					}
					else
					{
						assembly = compilerResutls.CompiledAssembly;
					}

					if (assembly != null)
					{
						Type type = assembly.GetType(className);

						if (type != null)
						{
							object loadedScript = Activator.CreateInstance(type);

							return loadedScript;
						}
						else
						{
							UnityEngine.Debug.LogError("Script at '" + fileName + "' needs to have the same name as the file.");
						}

						return null;
					}
				}
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogError("Script compilation error:\n" + e.ToString());
			}
#else
			return assembly.CreateInstance(className);
#endif
		}
		else
		{*/
		return assembly.CreateInstance(className);
		/*}

		return null;*/
	}

	static public float getGameHeight()
	{
		return Screen.height;
	}

	static public float getGameWidth()
	{
		return Screen.width;
	}

	public static int GetBitsPerPixel(TextureFormat format)
	{
		switch (format)
		{
			case TextureFormat.Alpha8: //	 Alpha-only texture format.
				return 8;
			case TextureFormat.ARGB4444: //	 A 16 bits/pixel texture format. Texture stores color with an alpha channel.
				return 16;
			case TextureFormat.RGB24:	// A color texture format.
				return 24;
			case TextureFormat.RGBA32:	//Color with an alpha channel texture format.
				return 32;
			case TextureFormat.ARGB32:	//Color with an alpha channel texture format.
				return 32;
			case TextureFormat.RGB565:	//	 A 16 bit color texture format.
				return 16;
			case TextureFormat.DXT1:	// Compressed color texture format.
				return 4;
			case TextureFormat.DXT5:	// Compressed color with alpha channel texture format.
				return 8;
			/*
			case TextureFormat.WiiI4:	// Wii texture format.
			case TextureFormat.WiiI8:	// Wii texture format. Intensity 8 bit.
			case TextureFormat.WiiIA4:	// Wii texture format. Intensity + Alpha 8 bit (4 + 4).
			case TextureFormat.WiiIA8:	// Wii texture format. Intensity + Alpha 16 bit (8 + 8).
			case TextureFormat.WiiRGB565:	// Wii texture format. RGB 16 bit (565).
			case TextureFormat.WiiRGB5A3:	// Wii texture format. RGBA 16 bit (4443).
			case TextureFormat.WiiRGBA8:	// Wii texture format. RGBA 32 bit (8888).
			case TextureFormat.WiiCMPR:	//	 Compressed Wii texture format. 4 bits/texel, ~RGB8A1 (Outline alpha is not currently supported).
				return 0;  //Not supported yet
			*/
			case TextureFormat.PVRTC_RGB2://	 PowerVR (iOS) 2 bits/pixel compressed color texture format.
				return 2;
			case TextureFormat.PVRTC_RGBA2://	 PowerVR (iOS) 2 bits/pixel compressed with alpha channel texture format
				return 2;
			case TextureFormat.PVRTC_RGB4://	 PowerVR (iOS) 4 bits/pixel compressed color texture format.
				return 4;
			case TextureFormat.PVRTC_RGBA4://	 PowerVR (iOS) 4 bits/pixel compressed with alpha channel texture format
				return 4;
			case TextureFormat.ETC_RGB4://	 ETC (GLES2.0) 4 bits/pixel compressed RGB texture format.
				return 4;
			case TextureFormat.ATC_RGB4://	 ATC (ATITC) 4 bits/pixel compressed RGB texture format.
				return 4;
			case TextureFormat.ATC_RGBA8://	 ATC (ATITC) 8 bits/pixel compressed RGB texture format.
				return 8;
			case TextureFormat.BGRA32://	 Format returned by iPhone camera
				return 32;
			case TextureFormat.ATF_RGB_DXT1://	 Flash-specific RGB DXT1 compressed color texture format.
			case TextureFormat.ATF_RGBA_JPG://	 Flash-specific RGBA JPG-compressed color texture format.
			case TextureFormat.ATF_RGB_JPG://	 Flash-specific RGB JPG-compressed color texture format.
				return 0; //Not supported yet
		}
		return 0;
	}

	public static int CalculateTextureSizeBytes(Texture tTexture)
	{
		if (tTexture != null)
		{
			int tWidth = tTexture.width;
			int tHeight = tTexture.height;
			if (tTexture is Texture2D)
			{
				Texture2D tTex2D = tTexture as Texture2D;
				int bitsPerPixel = GetBitsPerPixel(tTex2D.format);
				int mipMapCount = tTex2D.mipmapCount;
				int mipLevel = 1;
				int tSize = 0;
				while (mipLevel <= mipMapCount)
				{
					tSize += tWidth * tHeight * bitsPerPixel / 8;
					tWidth = tWidth / 2;
					tHeight = tHeight / 2;
					mipLevel++;
				}
				return tSize;
			}

			if (tTexture is Cubemap)
			{
				Cubemap tCubemap = tTexture as Cubemap;
				int bitsPerPixel = GetBitsPerPixel(tCubemap.format);
				return tWidth * tHeight * 6 * bitsPerPixel / 8;
			}
		}

		return 0;
	}

	static public uint calculateHash(string key)
	{
		uint hash;
		uint i;

		for (hash = i = 0; i < key.Length; ++i)
		{
			hash += key[(int)i];
			hash += (hash << 10);
			hash ^= (hash >> 6);
		}
		hash += (hash << 3);
		hash ^= (hash >> 11);
		hash += (hash << 15);

		return hash % Common.BIG_NUMBER;
	}

	static public void pixelizePosition(Transform transform)
	{
		Vector3 position = transform.position;

		position.x = Mathf.RoundToInt(position.x);
		position.y = Mathf.RoundToInt(position.y);

		transform.position = position;
	}

	public static string GetGameObjectPath(GameObject obj)
	{
		string path = "/" + obj.name;
		while (obj.transform.parent != null)
		{
			obj = obj.transform.parent.gameObject;
			path = "/" + obj.name + path;
		}
		return path;
	}

	public static string getMultilineData(string data)
	{
		return data.Replace('|', '\n');
	}

	public static void activateUI(Transform uiHolder, bool active)
	{
		if (active)
		{
			uiHolder.gameObject.active = true;

			for (int i = 0; i < uiHolder.childCount; i++)
			{
				uiHolder.GetChild(i).gameObject.active = true;
			}
		}
		else
		{
			uiHolder.gameObject.SetActiveRecursively(false);
		}
	}

	public static void randomizeList<T>(List<T> list)
	{
		List<int> randomList = new List<int>();

		for (int i = 0; i < list.Count; i++)
		{
			randomList.Add(i);
		}
	}	

	/*public static double getCurrentTime()
	{
		return EngineManager.GetCurrentInstance().GetEngineTime();
	}*/

	public static T GetObjectByTag<T>(string tag) where T : EngineObject
	{
		GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(tag);
		foreach (GameObject currentObject in gameObjects)
		{
			T currentComponent = currentObject.GetComponent<T>();

			if (currentComponent)
			{
				return currentComponent;
			}
		}

		return null;
	}

	public static int GetObjectIDByTag<T>(string tag) where T : EngineObject
	{
		GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(tag);
		foreach (GameObject currentObject in gameObjects)
		{
			T currentComponent = currentObject.GetComponent<T>();

			if (currentComponent)
			{
				return currentComponent.GetObjectID();
			}
		}

		return 0;
	}

	public static EngineManager getClientManager()
	{
		if (mClient == null)
		{
			EngineManager[] gameObjects = GameObject.FindObjectsOfType<EngineManager>();
			foreach (EngineManager currentInstance in gameObjects)
			{
				if (currentInstance.gameObject.layer == CLIENT_LAYER)
				{
					mClient = currentInstance;
					break;
				}
			}
		}

		return mClient;
	}

	public static EngineManager getServerManager()
	{
		if (mServer == null)
		{
			EngineManager[] gameObjects = GameObject.FindObjectsOfType<EngineManager>();
			foreach (EngineManager currentInstance in gameObjects)
			{
				if (currentInstance.gameObject.layer == SERVER_LAYER)
				{
					mServer = currentInstance;
					break;
				}
			}
		}

		return mServer;
	}

	public static bool TypeInheritsFrom(Type parentType, Type baseType)
	{
		Type currentType = parentType;

		while (currentType != null)
		{
			if (currentType.BaseType == baseType)
			{
				return true;
			}

			currentType = currentType.BaseType;
		}

		return false;
	}
}


public static class MyExtensions
{
	public static void CreateDirectory(this DirectoryInfo dirInfo)
	{
		if (dirInfo.Parent != null) CreateDirectory(dirInfo.Parent);
		if (!dirInfo.Exists) dirInfo.Create();
	}
}

public static class StringExtensions
{
	public static string Replace(this string originalString, string oldValue, string newValue, StringComparison comparisonType)
	{
		int startIndex = 0;
		while (true)
		{
			startIndex = originalString.IndexOf(oldValue, startIndex, comparisonType);
			if (startIndex == -1)
				break;

			originalString = originalString.Substring(0, startIndex) + newValue + originalString.Substring(startIndex + oldValue.Length);

			startIndex += newValue.Length;
		}

		return originalString;
	}

}