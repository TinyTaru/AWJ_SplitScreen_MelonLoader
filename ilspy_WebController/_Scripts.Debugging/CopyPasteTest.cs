using System;
using UnityEngine;

namespace _Scripts.Debugging;

public class CopyPasteTest : MonoBehaviour
{
	private const string buildPath = "D:\\Unity\\Projects\\A Webbing Journey\\Builds\\Steam Supporter\\PC\\0.9.0(13)";

	private const string pathWindows = "D:\\Users\\uitz\\Documents\\steamworks_sdk_160\\Demo - Supporter\\sdk\\tools\\ContentBuilder\\content\\Windows";

	private void PerBuildExecute()
	{
		Debug.Log(System.Environment.UserName);
	}
}
