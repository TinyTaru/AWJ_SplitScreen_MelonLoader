using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using _Scripts.General;
using _Scripts.Singletons;

namespace _Scripts.Miscellaneous;

public class CollectibleStyleChanger : MonoBehaviour
{
	[Header("References")]
	[FormerlySerializedAs("normalGameObject")]
	[SerializeField]
	private GameObject buttonGameObject;

	[FormerlySerializedAs("koumpounophobiaGameObject")]
	[SerializeField]
	private GameObject coinGameObject;

	[SerializeField]
	private GameObject hexagonGameObject;

	[SerializeField]
	private GameObject flowerGameObject;

	[SerializeField]
	private GameObject cookieGameObject;

	[SerializeField]
	private GameObject bananaGameObject;

	[SerializeField]
	private GameObject spiderWebGameObject;

	[SerializeField]
	private GameObject rubberDuckGameObject;

	[SerializeField]
	private GameObject heartGameObject;

	[SerializeField]
	private GameObject starGameObject;

	[SerializeField]
	private GameObject invisibleGameObject;

	private GameObject[] allGameObjects;

	private void Awake()
	{
		allGameObjects = new GameObject[11]
		{
			buttonGameObject, coinGameObject, heartGameObject, cookieGameObject, bananaGameObject, hexagonGameObject, flowerGameObject, starGameObject, spiderWebGameObject, rubberDuckGameObject,
			invisibleGameObject
		};
	}

	private void OnEnable()
	{
		if (allGameObjects.Any((GameObject x) => x == null))
		{
			Debug.LogError("One of the GameObjects in CollectibleStyleChanger is null!", this);
		}
		if (Singleton<SettingsController>.Instance != null)
		{
			SettingsController.OnSettingsUpdated += SettingsController_OnSettingsUpdated;
		}
		UpdateGameObjects();
	}

	private void OnDisable()
	{
		if (Singleton<SettingsController>.Instance != null)
		{
			SettingsController.OnSettingsUpdated -= SettingsController_OnSettingsUpdated;
		}
	}

	private void UpdateGameObjects()
	{
		if (!allGameObjects.Any((GameObject x) => x == null))
		{
			GameObject[] array = allGameObjects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: false);
			}
			switch (SettingsController.CollectibleStyle)
			{
			case CollectibleStyleType.Button:
				buttonGameObject.SetActive(value: true);
				break;
			case CollectibleStyleType.Coin:
				coinGameObject.SetActive(value: true);
				break;
			case CollectibleStyleType.Hexagon:
				hexagonGameObject.SetActive(value: true);
				break;
			case CollectibleStyleType.Flower:
				flowerGameObject.SetActive(value: true);
				break;
			case CollectibleStyleType.Cookie:
				cookieGameObject.SetActive(value: true);
				break;
			case CollectibleStyleType.Banana:
				bananaGameObject.SetActive(value: true);
				break;
			case CollectibleStyleType.SpiderWeb:
				spiderWebGameObject.SetActive(value: true);
				break;
			case CollectibleStyleType.RubberDuck:
				rubberDuckGameObject.SetActive(value: true);
				break;
			case CollectibleStyleType.Heart:
				heartGameObject.SetActive(value: true);
				break;
			case CollectibleStyleType.Star:
				starGameObject.SetActive(value: true);
				break;
			case CollectibleStyleType.Invisible:
				invisibleGameObject.SetActive(value: true);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	public GameObject GetActiveGameObject()
	{
		return allGameObjects.FirstOrDefault((GameObject go) => go.activeSelf);
	}

	private void SettingsController_OnSettingsUpdated(object sender, EventArgs e)
	{
		UpdateGameObjects();
	}
}
