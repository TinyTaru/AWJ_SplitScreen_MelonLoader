using System;
using UnityEngine;
using _Scripts.Miscellaneous.Halloween;
using _Scripts.Singletons;

namespace _Scripts.Miscellaneous.Christmas;

public class ChristmasLight : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer meshRenderer;

	[SerializeField]
	private Material[] christmasMaterials;

	[SerializeField]
	private Material[] halloweenMaterials;

	private void Start()
	{
		meshRenderer.sharedMaterials = christmasMaterials;
		if (Singleton<KitchenHalloweenController>.Instance != null)
		{
			Singleton<KitchenHalloweenController>.Instance.OnActivateHalloweenEffects += HalloweenController_OnActivateHalloweenEffects;
		}
	}

	private void HalloweenController_OnActivateHalloweenEffects(object sender, EventArgs e)
	{
		meshRenderer.sharedMaterials = halloweenMaterials;
	}
}
