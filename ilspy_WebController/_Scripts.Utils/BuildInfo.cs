using UnityEngine;

namespace _Scripts.Utils;

public static class BuildInfo
{
	private const uint BUILD_NUMBER_ANDROID = 249u;

	private const uint BUILD_NUMBER_IOS = 59u;

	private const uint BUILD_NUMBER_STANDALONE = 121u;

	private static string fullVersionString;

	public static uint buildNumberAndroid => 249u;

	public static uint buildNumberIOS => 59u;

	public static uint buildNumberStandalone => 121u;

	public static string version => Application.version;

	public static string FullVersionString
	{
		get
		{
			if (string.IsNullOrEmpty(fullVersionString))
			{
				fullVersionString = $"{version}({buildNumberStandalone})";
			}
			return fullVersionString;
		}
	}
}
