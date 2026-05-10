using UnityEngine;
using _Scripts.Singletons;
using _Scripts.Spider;

namespace _Scripts.Wardrobe;

public class WardrobeIsland : Singleton<WardrobeIsland>
{
	[Header("References")]
	[SerializeField]
	private GameObject wardrobeIsland;

	[SerializeField]
	private SpiderCustomization spiderCustomization;

	[SerializeField]
	private Transform rotationPoint;

	[SerializeField]
	private Transform wardrobeCameraPosition;

	public SpiderCustomization Customization => spiderCustomization;

	public Transform WardrobeCameraPosition => wardrobeCameraPosition;

	private void Start()
	{
		HideWardrobeIsland();
	}

	public void ShowWardrobeIsland()
	{
		wardrobeIsland.SetActive(value: true);
		spiderCustomization.gameObject.SetActive(value: true);
		spiderCustomization.LoadDefaultIndices();
		spiderCustomization.Refresh();
		rotationPoint.localRotation = Quaternion.identity;
	}

	public void HideWardrobeIsland()
	{
		if (spiderCustomization != null)
		{
			spiderCustomization.gameObject.SetActive(value: false);
		}
		if (wardrobeIsland != null)
		{
			wardrobeIsland.SetActive(value: false);
		}
	}

	public void Rotate(float value)
	{
		rotationPoint.Rotate(0f, value * Time.unscaledDeltaTime, 0f, Space.Self);
	}
}
