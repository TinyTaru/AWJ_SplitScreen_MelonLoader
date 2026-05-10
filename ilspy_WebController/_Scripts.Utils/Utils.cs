using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Utils;

public static class Utils
{
	public static float ExponentialDecay(float a, float b, float decay, float deltaTime)
	{
		return b + (a - b) * Mathf.Exp((0f - decay) * deltaTime);
	}

	public static Vector3 ExponentialDecay(Vector3 a, Vector3 b, float decay, float deltaTime)
	{
		return b + (a - b) * Mathf.Exp((0f - decay) * deltaTime);
	}

	public static void SetLayerRecursive(GameObject obj, int newLayer)
	{
		obj.layer = newLayer;
		foreach (Transform item in obj.transform)
		{
			SetLayerRecursive(item.gameObject, newLayer);
		}
	}

	public static bool IsLayerInLayerMask(int layerToCheck, LayerMask layerMask)
	{
		return ((int)layerMask & (1 << layerToCheck)) != 0;
	}

	public static string FormatTime(float timeInSeconds, int millisecondDigits = 3)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(timeInSeconds);
		if (timeSpan.Minutes == 0)
		{
			return millisecondDigits switch
			{
				0 => $"{timeSpan.Seconds:0}", 
				1 => $"{timeSpan.Seconds:0}.{timeSpan.Milliseconds / 100:0}", 
				2 => $"{timeSpan.Seconds:0}.{timeSpan.Milliseconds / 10:00}", 
				_ => $"{timeSpan.Seconds:0}.{timeSpan.Milliseconds:000}", 
			};
		}
		return millisecondDigits switch
		{
			0 => $"{timeSpan.Minutes:0}:{timeSpan.Seconds:00}", 
			1 => $"{timeSpan.Minutes:0}:{timeSpan.Seconds:00}.{timeSpan.Milliseconds / 100:0}", 
			2 => $"{timeSpan.Minutes:0}:{timeSpan.Seconds:00}.{timeSpan.Milliseconds / 10:00}", 
			_ => $"{timeSpan.Minutes:0}:{timeSpan.Seconds:00}.{timeSpan.Milliseconds:000}", 
		};
	}

	public static TKey GetKeyFromValue<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TValue value)
	{
		foreach (KeyValuePair<TKey, TValue> item in dictionary)
		{
			if (EqualityComparer<TValue>.Default.Equals(item.Value, value))
			{
				return item.Key;
			}
		}
		return default(TKey);
	}

	public static float ConvertVerticalToHorizontalFOV(float verticalFOVDeg, float aspectRatio)
	{
		float num = verticalFOVDeg * (MathF.PI / 180f);
		return 2f * Mathf.Atan(Mathf.Tan(num / 2f) * aspectRatio) * 57.29578f;
	}

	public static float ConvertHorizontalToVerticalFOV(float horizontalFOVDeg, float aspectRatio)
	{
		float num = horizontalFOVDeg * (MathF.PI / 180f);
		return 2f * Mathf.Atan(Mathf.Tan(num / 2f) / aspectRatio) * 57.29578f;
	}

	public static List<GameObject> FindChildrenByName(GameObject parent, string name)
	{
		Transform[] componentsInChildren = parent.GetComponentsInChildren<Transform>(includeInactive: true);
		List<GameObject> list = new List<GameObject>();
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			if (transform.name == name)
			{
				list.Add(transform.gameObject);
			}
		}
		return list;
	}

	public static Vector3 RandomVector3()
	{
		return new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
	}
}
