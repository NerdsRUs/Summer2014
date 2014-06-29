using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;
using System;

#if UNITY_WEBPLAYER
#else
using System.CodeDom.Compiler;

#endif

public class ScriptLoader
{
	static char[] LINE_DELIMITERS = { '|', ';' };
	static char[] DELIMITERS = { ',', ')', '(', '"', ';' };
	static string SCRIPT_PATH = "Assets/src/Scripts/";

	/*public void reloadScripts()
	{
#if UNITY_WEBPLAYER
#else
		System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

		stopWatch.Start();

		DirectoryInfo directory = new DirectoryInfo(SCRIPT_PATH);
		FileInfo[] files = directory.GetFiles("*.cs");

		foreach (FileInfo file in files)
		{
			loadScript(file.Name.Substring(0, file.Name.Length - 3));
		}

		UnityEngine.Debug.Log(stopWatch.ElapsedMilliseconds);
#endif
	}

	public Script loadScript(string scriptName)
	{
#if UNITY_WEBPLAYER
#else
		Assembly assembly = loadAssembly(SCRIPT_PATH + scriptName);

		if (assembly != null)
		{
			Type type = assembly.GetType(scriptName);

			if (type != null)
			{
				Script loadedScript = (Script)Activator.CreateInstance(type);

				loadedmAPIAccess.main();

				return loadedScript;
			}
			else
			{
				UnityEngine.Debug.LogError("Script '" + scriptName + "' needs to have the same name as the file.");
			}

			return null;
		}
#endif

		return null;

	}

	public Assembly loadAssembly(string scriptName)
	{
#if UNITY_WEBPLAYER
#else
		string scriptPath = scriptName + ".cs";
		string binaryPath = scriptName + ".dll";

		Assembly assembly = null;

		try
		{
			using (FileStream stream = new FileStream(scriptPath, FileMode.Open, FileAccess.Read))
			using (TextReader reader = new StreamReader(stream))
			{
				Microsoft.CSharp.CSharpCodeProvider provider = new Microsoft.CSharp.CSharpCodeProvider();

				CompilerParameters compilerParameters = new CompilerParameters
				{
					GenerateExecutable = false,
					GenerateInMemory = true,
					TreatWarningsAsErrors = false,
					CompilerOptions = "/optimize",
				};

				// Add references to all the assemblies we might need.
				Assembly executingAssembly = Assembly.GetExecutingAssembly();
				compilerParameters.ReferencedAssemblies.Add(executingAssembly.Location);

				foreach (AssemblyName assemblyName in executingAssembly.GetReferencedAssemblies())
				{
					compilerParameters.ReferencedAssemblies.Add(Assembly.Load(assemblyName).Location);
				}

				// Invoke compilation of the source file.
				CompilerResults compilerResutls = provider.CompileAssemblyFromSource(compilerParameters, reader.ReadToEnd());

				if (compilerResutls.Errors.Count > 0)
				{
					// Display compilation errors.
					StringBuilder builder = new StringBuilder();
					foreach (CompilerError ce in compilerResutls.Errors)
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
			}
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogError("Script compilation error:\n" + e.ToString());
		}

		return assembly;
#endif

		return null;
	}*/

	static public bool isValidScript(System.Object executingObject, string command)
	{
		if (command != null)
		{
			string[] parts = command.Split(DELIMITERS, StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length > 0)
			{
				string functionName = parts[0];
				MethodInfo tempMethod = executingObject.GetType().GetMethod(functionName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

				if (tempMethod == null)
				{
					return false;
				}

				return true;
			}
		}

		return false;
	}

	static public object executeCommands(System.Object executingObject, string command)
	{
		object returnValue = null;

		if (command != null)
		{
			string[] parts = command.Split(LINE_DELIMITERS, StringSplitOptions.RemoveEmptyEntries);

			for (int i = 0; i < parts.Length; i++)
			{
				object tempReturn = executeCommand(executingObject, parts[i]);
				if (tempReturn != null)
				{
					returnValue = tempReturn;
				}
			}
		}

		return returnValue;
	}

	static public object executeCommand(System.Object executingObject, string command)
	{
		if (command != null)
		{
			string[] parts = command.Split(DELIMITERS, StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length > 0)
			{
				MethodInfo tempMethod = null;
				string functionName = parts[0];
				try
				{
					tempMethod = executingObject.GetType().GetMethod(functionName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
				}
				catch (Exception e)
				{
					Debug.LogError("Could not find get method: " + command + ".\n" + e.Message);
					return null;
				}

				if (tempMethod == null)
				{
					Debug.LogError("Function does not exist: '" + functionName + "' in class " + executingObject.GetType());
					return null;
				}

				ParameterInfo[] tempInfo = tempMethod.GetParameters();

				object tempObject = null;
				object[] parameters = new object[tempInfo.Length];

				for (int i = 0; i < tempInfo.Length; i++)
				{
					if (parts.Length > i + 1)
					{
						if (FromString(tempInfo[i].ParameterType, parts[i + 1], ref tempObject))
						{
							parameters[i] = tempObject;
						}
						else
						{
							Debug.LogError("Function '" + functionName + "' was unable to convert parameter: " + (i + 1));
							return null;
						}
					}
					else
					{
						if (tempInfo[i].DefaultValue != null)
						{
							parameters[i] = tempInfo[i].DefaultValue;
						}
						else
						{
							Debug.LogError("Function '" + functionName + "' has invalid number of parameters: Had" + (parts.Length - 1) + " Needs: " + tempInfo.Length);
							return null;
						}
					}
				}

				return tempMethod.Invoke(executingObject, parameters);
			}
		}

		return null;
	}

	static protected bool FromString(Type type, string parameter, ref object data)
	{
		if (type == typeof(string))
		{
			data = parameter;
		}
		else if (type == typeof(int))
		{
			data = int.Parse(parameter);
		}
		else if (type == typeof(bool))
		{
			data = bool.Parse(parameter);
		}
		else if (type == typeof(char))
		{
			data = char.Parse(parameter);
		}
		else if (type == typeof(double))
		{
			data = double.Parse(parameter);
		}
		else if (type == typeof(float))
		{
			data = float.Parse(parameter);
		}
		else if (type == typeof(long))
		{
			data = long.Parse(parameter);
		}
		else if (type == typeof(short))
		{
			data = short.Parse(parameter);
		}
		else if (type == typeof(uint))
		{
			data = uint.Parse(parameter);
		}
		else if (type == typeof(ulong))
		{
			data = ulong.Parse(parameter);
		}
		else if (type == typeof(ushort))
		{
			data = ushort.Parse(parameter);
		}
		else
		{
			Debug.LogError("Cannot convert variable of type: " + type + " using data: " + parameter);
			return false;
		}

		return true;
	}
}
