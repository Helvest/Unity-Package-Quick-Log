#define LogToFile
using System;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace LogToFile
{

	[Serializable]
	public class _QuickLog
	{

		#region Enums

		public enum BuildType
		{
			None,
			Build,
			Debug,
			Editor
		}

		#endregion

		#region Fields

		public Object cachedContext = default;

		private static BuildType _buildType = BuildType.None;

		public BuildType GetBuildType
		{
			get
			{
				if (_buildType == BuildType.None)
				{
					if (Application.isEditor)
					{
						_buildType = BuildType.Editor;
					}
					else if (Debug.isDebugBuild)
					{
						_buildType = BuildType.Debug;
					}
					else
					{
						_buildType = BuildType.Build;
					}
				}

				return _buildType;
			}
		}

		public DebugLevel debugLevelInBuild = DebugLevel.Exception;
		public DebugLevel debugLevelInDebug = DebugLevel.Warning;
		public DebugLevel debugLevelInEditor = DebugLevel.Debug;

		private DebugLevel ActualDebugLevel
		{
			get
			{
				switch (GetBuildType)
				{
					default:
					case BuildType.Build:
						return debugLevelInBuild;
					case BuildType.Debug:
						return debugLevelInDebug;
					case BuildType.Editor:
						return debugLevelInEditor;
				}
			}
		}

		[FormerlySerializedAs("_useDebugActionInBuild")]
		public bool useDebugActionInBuild = false;
		[FormerlySerializedAs("_useDebugActionInDebug")]
		public bool useDebugActionInDebug = false;
		[FormerlySerializedAs("_useDebugActionInEditor")]
		public bool useDebugActionInEditor = true;

		public bool UseDebugAction
		{
			get
			{
				switch (GetBuildType)
				{
					default:
					case BuildType.Build:
						return useDebugActionInBuild;
					case BuildType.Debug:
						return useDebugActionInDebug;
					case BuildType.Editor:
						return useDebugActionInEditor;
				}
			}
		}

		public bool useColor = false;
		public Color defaultColor = Color.white;

		#endregion

		#region Enum

		public enum DebugLevel
		{
			Info = 0,
			Debug = 1,
			Warning = 2,
			Exception = 3,
			Error = 4,
			None = 5
		}

		#endregion

		#region Logs

		public void Log(string msg, DebugLevel level = DebugLevel.Debug, Object context = null, Color color = default)
		{
			if (level < ActualDebugLevel)
			{
				return;
			}

			context = context != null ? context : cachedContext;

			if (Application.isEditor)
			{
				if (color != default)
				{
					msg = AddColorToText(msg, color);
				}
				else if (useColor)
				{
					msg = AddColorToText(msg, defaultColor);
				}
			}

			switch (level)
			{
				case DebugLevel.Info:
				case DebugLevel.Debug:
					Debug.Log(msg, context);
					break;
				case DebugLevel.Warning:
					Debug.LogWarning(msg, context);
					break;
				case DebugLevel.Exception:
				case DebugLevel.Error:
					Debug.LogError(msg, context);
					break;
				case DebugLevel.None:
					break;
			}
		}

		public void LogInfo(string msg, Object context = null)
		{
			Log(msg, DebugLevel.Info, context);
		}

		public void Log(string msg, Object context = null)
		{
			Log(msg, DebugLevel.Debug, context);
		}

		public void LogWarning(string msg, Object context = null)
		{
			Log(msg, DebugLevel.Warning, context);
		}

		public void LogException(Exception e, Object context = null)
		{
			if (DebugLevel.Exception < ActualDebugLevel)
			{
				return;
			}

			context = context != null ? context : cachedContext;
			Debug.LogException(e, context);
		}

		public void LogError(string msg, Object context = null)
		{
			Log(msg, DebugLevel.Error, context);
		}

		#endregion

		public static string AddColorToText(string text, Color color)
		{
			string hexColor = ColorUtility.ToHtmlStringRGB(color);
			return $"<color=#{hexColor}>{text}</color>";
		}
	}

	[Serializable]
	public class QuickLog
	{

		#region Enums

		[Flags]
		public enum DebugLevel
		{
			None = 0,
			AllLog = LogInfo | LogDebug | LogWarning | LogException | LogError,
			LogInfo = 1,
			LogDebug = 1 << 1,
			LogWarning = 1 << 2,
			LogException = 1 << 3,
			LogError = 1 << 4,
			UseDebugAction = 1 << 5,
			Everything = 63
		}

		#endregion

		#region Fields

		public Object cachedContext = default;

#if UNITY_EDITOR
		[SerializeField]
		private bool _useColor = false;
		[SerializeField]
		[ColorUsage(false, false)]
		private Color _defaultColor = Color.white;
#endif

		public DebugLevel debugLevelInBuild = DebugLevel.LogException | DebugLevel.LogError;
		public DebugLevel debugLevelInDebug = DebugLevel.LogWarning | DebugLevel.LogException | DebugLevel.LogError;
		public DebugLevel debugLevelInEditor = DebugLevel.Everything;

#if UNITY_EDITOR
		public DebugLevel GetDebugLevel => debugLevelInEditor;
		public readonly bool isBuild = false;
		public readonly bool isDebugBuild = false;
		public readonly bool isEditor = true;
#elif DEBUG
		public DebugLevel GetDebugLevel => debugLevelInDebug;
		public readonly bool isBuild = true;
		public readonly bool isDebugBuild = true;
		public readonly bool isEditor = false;
#else
		public DebugLevel GetDebugLevel => debugLevelInBuild;
		public readonly bool isBuild = true;
		public readonly bool isDebugBuild = false;
		public readonly bool isEditor = false;
#endif

		#endregion

		#region Logs

		public void Log(string msg, DebugLevel level, Object context = null, Color color = default)
		{
			if (!GetDebugLevel.HasFlag(level))
			{
				return;
			}

			context = context != null ? context : cachedContext;

#if UNITY_EDITOR
			if (color != default)
			{
				msg = AddColorToText(msg, color);
			}
			else if (_useColor)
			{
				msg = AddColorToText(msg, _defaultColor);
			}
#endif

			switch (level)
			{
				case DebugLevel.LogInfo:
				case DebugLevel.LogDebug:
					Debug.Log(msg, context);
					break;
				case DebugLevel.LogWarning:
					Debug.LogWarning(msg, context);
					break;
				case DebugLevel.LogException:
				case DebugLevel.LogError:
					Debug.LogError(msg, context);
					break;
				case DebugLevel.None:
					break;
			}
		}

		#endregion

		#region Logs shortcut

		public void LogInfo(string msg, Object context = null, Color color = default)
		{
			Log(msg, DebugLevel.LogInfo, context, color);
		}

		public void Log(object ob, Object context = null, Color color = default)
		{
			Log(ob.ToString(), DebugLevel.LogDebug, context, color);
		}

		public void Log(string msg, Object context = null, Color color = default)
		{
			Log(msg, DebugLevel.LogDebug, context, color);
		}

		public void LogWarning(string msg, Object context = null, Color color = default)
		{
			Log(msg, DebugLevel.LogWarning, context, color);
		}

		public void LogException(Exception e, Object context = null)
		{
			if (!GetDebugLevel.HasFlag(DebugLevel.LogException))
			{
				return;
			}

			context = context != null ? context : cachedContext;
			Debug.LogException(e, context);
		}

		public void LogError(string msg, Object context = null, Color color = default)
		{
			Log(msg, DebugLevel.LogError, context, color);
		}

		#endregion

		#region AddColorToText

		public static string AddColorToText(string text, Color color)
		{
			string hexColor = ColorUtility.ToHtmlStringRGB(color);
			return $"<color=#{hexColor}>{text}</color>";
		}

		#endregion

	}

}