using System.Globalization;
using UnityEngine;

namespace _Scripts.Debugging;

public class DebugLogMessage : MonoBehaviour
{
	public void Write(string message)
	{
		Debug.Log(message);
	}

	public void Write(float value)
	{
		Debug.Log(value.ToString(CultureInfo.InvariantCulture));
	}
}
