using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Utils;

public static class ArrayAndListExtensions
{
	public static T RandomValue<T>(this T[] array)
	{
		int num = Random.Range(0, array.Length);
		return array[num];
	}

	public static T RandomValue<T>(this List<T> list)
	{
		int index = Random.Range(0, list.Count);
		return list[index];
	}
}
