using System;
using System.Linq;
using UnityEngine;

namespace SplineMesh;

public static class UOUtility
{
	public static GameObject Create(string name, GameObject parent, params Type[] components)
	{
		GameObject gameObject = new GameObject(name, components);
		gameObject.transform.parent = parent.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localRotation = Quaternion.identity;
		return gameObject;
	}

	public static GameObject Instantiate(GameObject prefab, Transform parent)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(prefab, parent);
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		return gameObject;
	}

	public static void Destroy(GameObject go)
	{
		if (Application.isPlaying)
		{
			UnityEngine.Object.Destroy(go);
		}
		else
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	public static void Destroy(Component comp)
	{
		if (Application.isPlaying)
		{
			UnityEngine.Object.Destroy(comp);
		}
		else
		{
			UnityEngine.Object.DestroyImmediate(comp);
		}
	}

	public static void DestroyChildren(GameObject go)
	{
		foreach (Transform item in go.transform.Cast<Transform>().ToList())
		{
			Destroy(item.gameObject);
		}
	}
}
