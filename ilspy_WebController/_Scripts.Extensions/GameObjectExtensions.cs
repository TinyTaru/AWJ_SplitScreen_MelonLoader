using UnityEngine;

namespace _Scripts.Extensions;

public static class GameObjectExtensions
{
	public static string GetFullPath(this GameObject obj)
	{
		if (obj == null)
		{
			return "Null GameObject";
		}
		string text = obj.name;
		Transform parent = obj.transform.parent;
		while (parent != null)
		{
			text = parent.name + "/" + text;
			parent = parent.parent;
		}
		return text;
	}
}
