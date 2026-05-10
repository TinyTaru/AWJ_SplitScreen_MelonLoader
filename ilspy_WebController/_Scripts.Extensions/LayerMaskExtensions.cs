using UnityEngine;

namespace _Scripts.Extensions;

public static class LayerMaskExtensions
{
	public static bool Contains(this LayerMask mask, int layer)
	{
		return ((int)mask & (1 << layer)) != 0;
	}
}
