using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Runtime.InteropServices;

public class NetCode : MonoBehaviour
{
	const int MAX_PACKET_LENGTH = 1024;

	public static bool DEBUG = false;

	public string terminalScriptName = "";

	string mTerminalRPCName = "";
	NetCode mTerminalScript;

	protected bool mProcessPackets = true;
	List<Packet> mPacketQueue = new List<Packet>();

	public int mServerPacketID = 1;

	struct Packet
	{
		public string mName;
		public byte[] mData;
		public int mID;
	}

	protected virtual void Update()
	{
		while (mPacketQueue.Count > 0 && mProcessPackets)
		{
			try
			{
				processPacket(mPacketQueue[0].mName, mPacketQueue[0].mData, mPacketQueue[0].mID);
			}
			catch (System.Reflection.TargetInvocationException e)
			{
				Debug.LogError("Packet (" + mPacketQueue[0].mName + ") had error: " + e.InnerException.Message);
				Debug.LogError("    " + e.InnerException.StackTrace);
			}

			mPacketQueue.RemoveAt(0);
		}
	}

    public bool packetsPaused()
    {
        return !mProcessPackets;
    }

	public void pausePackets()
	{
		mProcessPackets = false;
	}

	public void resumePackets()
	{
		mProcessPackets = true;
	}

	void init()
	{
		mTerminalScript = (NetCode)GetComponent(terminalScriptName);

		mTerminalRPCName = terminalScriptName + "RPC";
	}

	protected void doRPC(string name, NetworkPlayer player, params object[] args)
	{
		if (mTerminalScript == null)
		{
			init();
		}

		bool hasConnection = false;
		for (int i = 0; i < Network.connections.Length; i++)
		{
			if (Network.connections[i] == player)
			{
				hasConnection = true;
			}
		}

		if (hasConnection)
		{
			try
			{
				networkView.RPC(mTerminalRPCName, player, name, mServerPacketID, getRPCBytes(name, args));
			}
			catch (Exception e)
			{
				Debug.LogError("Trying to send packet to invalid user");
			}
		}
		else
		{
			Debug.LogError("Logging out user due to disconnection");
		}
	}

	protected void doRPC(string name, RPCMode mode, params object[] args)
	{
		if (mTerminalScript == null)
		{
			init();
		}

		networkView.RPC(mTerminalRPCName, mode, name, mServerPacketID, getRPCBytes(name, args));

		/*if (DEBUG)
		{
			if (Server.isServer())
			{
				string message = "RPC " + name;
				for (int i = 0; i < args.Length; i++)
				{
					message += " " + args[i].ToString();
				}
				SQDebug.log2(Common.getTime() + " : " + mServerPacketID + ") " + message, 5);

				mServerPacketID++;
			}
		}*/
	}

	public byte[] getRPCBytes(string RPCname, params object[] objects)
	{
		MethodInfo tempMethod = mTerminalScript.GetType().GetMethod(RPCname, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

		if (tempMethod == null)
		{
			Debug.LogError("RPC function does not exist: '" + RPCname + "' in class " + mTerminalScript.GetType());
			return null;
		}

		ParameterInfo[] tempInfo = tempMethod.GetParameters();

		if (objects.Length != tempInfo.Length)
		{
			Debug.LogError("RPC function '" + RPCname + "' parameter counts don't match. Expected " + tempInfo.Length + " got " + objects.Length);
			return null;
		}

		List<byte[]> tempBytes = new List<byte[]>();

		for (int i = 0; i < tempInfo.Length; i++)
		{
			if (tempInfo[i].ParameterType != objects[i].GetType())
			{
				Debug.LogError("RPC function '" + RPCname + "' parameter " + (i + 1) + " is the wrong type. Expected " + tempInfo[i].ParameterType + " got " + objects[i].GetType());
				return null;
			}

			tempBytes.Add(ToByteArray(objects[i], MAX_PACKET_LENGTH));
		}

		int totalSize = 0;

		for (int i = 0; i < tempBytes.Count; i++)
		{
			totalSize += tempBytes[i].Length;
		}

		if (totalSize > 0)
		{
			byte[] finalBytes = new byte[totalSize];
			int currentOffset = 0;

			for (int i = 0; i < tempBytes.Count; i++)
			{
				System.Array.Copy(tempBytes[i], 0, finalBytes, currentOffset, tempBytes[i].Length);

				currentOffset += tempBytes[i].Length;
			}

			return finalBytes;
		}
		else
		{
			byte[] finalBytes = new byte[1];
			finalBytes[0] = 0;

			return finalBytes;
		}
	}




	protected void dynamicRPC(string name, int ID, byte[] data)
	{
		Packet newPacket;
		newPacket.mName = name;
		newPacket.mData = data;
		newPacket.mID = ID;

		mPacketQueue.Add(newPacket);
	}

	void processPacket(string name, byte[] data, int ID = 0)
	{
		int size;
		object tempObject = null;
		int currentOffset = 0;

		/*if (!FromByteArray(typeof(string), data, 0, out size, ref tempObject))
		{
			SQDebug.log("Malformed RPC call: " + name);
			return;
		}*/

		string RPCname = name;
		MethodInfo tempMethod = this.GetType().GetMethod(RPCname, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

		if (tempMethod == null)
		{
			Debug.LogError("RPC function does not exist: '" + name + "' in class " + GetType());
			return;
		}

		ParameterInfo[] tempInfo = tempMethod.GetParameters();

		object[] parameters = new object[tempInfo.Length];

		for (int i = 0; i < tempInfo.Length; i++)
		{
			if (FromByteArray(tempInfo[i].ParameterType, data, currentOffset, out size, ref tempObject))
			{
				parameters[i] = tempObject;
			}
			else
			{
				Debug.LogError("RPC function '" + name + "' was unable to convert parameter: " + (i + 1));
				return;
			}

			currentOffset += size;
		}

		/*if (DEBUG)
		{
			if (Client.isClient())
			{
				string message = "RPC " + name;
				for (int i = 0; i < parameters.Length; i++)
				{
					message += " " + parameters[i].ToString();
				}
				SQDebug.log2(Common.getTime() + " : " + ID + ") " + message, 5);
			}
		}*/

		tempMethod.Invoke(this, parameters);
	}

	protected bool FromByteArray(Type type, byte[] rawValue, int index, out int size, ref object data)
	{
		//Pack length info in the first 4 bytes
		if (type == typeof(byte[]))
		{
			size = BitConverter.ToInt32(rawValue, index);

			byte[] tempData = new byte[size];

			Array.Copy(rawValue, index + 4, tempData, 0, size);

			size += 4;

			data = (object)tempData;

			return true;
		}
		//Strings need to be handled seperately
		else if (type == typeof(string))
		{
			for (int i = index; i < rawValue.Length; i++)
			{
				if (rawValue[i] == 0)
				{
					//Found end of string

					size = i - index + 1;

					data = (object)Encoding.ASCII.GetString(rawValue, index, i - index);

					return true;
				}
			}

			size = 0;

			return false;
		}
		//Server side only
#if UNITY_WEBPLAYER
#else
		else if (type == typeof(UnityEngine.NetworkPlayer))
		{
			size = Marshal.SizeOf(type);

			if (rawValue.Length >= index + size)
			{
				byte[] tempBytes = new byte[size];
				Array.Copy(rawValue, index, tempBytes, 0, size);

				GCHandle handle = GCHandle.Alloc(tempBytes, GCHandleType.Pinned);
				data = (object)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), type);
				handle.Free();

				return true;
			}
			else
			{
				return false;
			}
		}
#endif
		else
		{
			if (type == typeof(int))
			{
				size = sizeof(int);
				data = BitConverter.ToInt32(rawValue, index);
			}
			else if (type == typeof(bool))
			{
				size = sizeof(bool);
				data = BitConverter.ToBoolean(rawValue, index);
			}
			else if (type == typeof(char))
			{
				size = sizeof(char);
				data = BitConverter.ToChar(rawValue, index);
			}
			else if (type == typeof(double))
			{
				size = sizeof(double);
				data = BitConverter.ToDouble(rawValue, index);
			}
			else if (type == typeof(float))
			{
				size = sizeof(float);
				data = BitConverter.ToSingle(rawValue, index);
			}
			else if (type == typeof(long))
			{
				size = sizeof(long);
				data = BitConverter.ToInt64(rawValue, index);
			}
			else if (type == typeof(short))
			{
				size = sizeof(short);
				data = BitConverter.ToInt16(rawValue, index);
			}
			else if (type == typeof(uint))
			{
				size = sizeof(uint);
				data = BitConverter.ToUInt32(rawValue, index);
			}
			else if (type == typeof(ulong))
			{
				size = sizeof(ulong);
				data = BitConverter.ToUInt64(rawValue, index);
			}
			else if (type == typeof(ushort))
			{
				size = sizeof(ushort);
				data = BitConverter.ToUInt16(rawValue, index);
			}
			else if (type == typeof(Vector3))
			{
				size = sizeof(float) * 3;
				data = new Vector3(BitConverter.ToSingle(rawValue, index), BitConverter.ToSingle(rawValue, index + sizeof(float)), BitConverter.ToSingle(rawValue, index + sizeof(float)*2));
			}
			else
			{
				size = 0;
				Debug.LogError("Cannot convert variable of type: " + type);
				return false;
			}

			return true;
		}
	}

	protected byte[] ToByteArray(object value, int maxLength)
	{
		//Pack length info in the first 4 bytes
		if (value.GetType() == typeof(byte[]))
		{
			int tempLength = ((byte[])value).Length;
			byte[] tempByteArray = new byte[tempLength + 4];

			byte[] lengthBytes = BitConverter.GetBytes((Int32)tempLength);

			Array.Copy(lengthBytes, 0, tempByteArray, 0, 4);
			Array.Copy((byte[])value, 0, tempByteArray, 4, tempLength);

			return tempByteArray;
		}
		//Strings need to be handled seperately
		else if (value.GetType() == typeof(string))
		{
			byte[] rawdata = Encoding.ASCII.GetBytes((string)value + "\0");

			return rawdata;
		}
		else
		{
			byte[] rawdata;
			if (value.GetType() == typeof(int))
			{
				rawdata = BitConverter.GetBytes((int)value);
			}
			else if (value.GetType() == typeof(bool))
			{
				rawdata = BitConverter.GetBytes((bool)value);
			}
			else if (value.GetType() == typeof(char))
			{
				rawdata = BitConverter.GetBytes((char)value);
			}
			else if (value.GetType() == typeof(double))
			{
				rawdata = BitConverter.GetBytes((double)value);
			}
			else if (value.GetType() == typeof(float))
			{
				rawdata = BitConverter.GetBytes((float)value);
			}
			else if (value.GetType() == typeof(long))
			{
				rawdata = BitConverter.GetBytes((long)value);
			}
			else if (value.GetType() == typeof(short))
			{
				rawdata = BitConverter.GetBytes((short)value);
			}
			else if (value.GetType() == typeof(uint))
			{
				rawdata = BitConverter.GetBytes((uint)value);
			}
			else if (value.GetType() == typeof(ulong))
			{
				rawdata = BitConverter.GetBytes((ulong)value);
			}
			else if (value.GetType() == typeof(ushort))
			{
				rawdata = BitConverter.GetBytes((ushort)value);
			}
			else if (value.GetType() == typeof(Vector3))
			{
				rawdata = new byte[sizeof(float) * 3];

				Array.Copy(BitConverter.GetBytes(((Vector3)value).x), 0, rawdata, 0, sizeof(float));
				Array.Copy(BitConverter.GetBytes(((Vector3)value).y), 0, rawdata, sizeof(float), sizeof(float));
				Array.Copy(BitConverter.GetBytes(((Vector3)value).z), 0, rawdata, sizeof(float) * 2, sizeof(float));
			}
			else if (value.GetType() == typeof(UnityEngine.NetworkPlayer))
			{
				rawdata = BitConverter.GetBytes(int.Parse(value.ToString()));
			}
			else
			{
				Debug.LogError("Cannot convert variable of type: " + value.GetType());
				return null;
			}

			if (maxLength < rawdata.Length)
			{
				byte[] temp = new byte[maxLength];
				Array.Copy(rawdata, temp, maxLength);
				return temp;
			}
			else
			{
				return rawdata;
			}
		}
	}
}