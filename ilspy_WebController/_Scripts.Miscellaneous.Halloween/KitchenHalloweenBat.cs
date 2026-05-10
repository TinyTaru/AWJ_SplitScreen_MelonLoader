using System;
using Dreamteck.Splines;
using UnityEngine;
using _Scripts.LevelSaving;
using _Scripts.Miscellaneous.Christmas;
using _Scripts.Singletons;

namespace _Scripts.Miscellaneous.Halloween;

public class KitchenHalloweenBat : MonoBehaviour, IInitializable<KitchenHalloweenBatSaveData>, IHasSaveData<KitchenHalloweenBatSaveData>
{
	[SerializeField]
	private GameObject christmasHat;

	[SerializeField]
	private SplineFollower splineFollower;

	[SerializeField]
	private float flyAnimationSpeed = 0.5f;

	private Animator animator;

	private bool christmasHatActive;

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	private void Start()
	{
		animator.SetFloat("FlyAnimationSpeed", flyAnimationSpeed);
		if (Singleton<KitchenChristmasController>.Instance != null)
		{
			Singleton<KitchenChristmasController>.Instance.OnActivateChristmasEffects += ChristmasController_OnActivateChristmasEffects;
			if (Singleton<KitchenChristmasController>.Instance.ChristmasEventIsActive)
			{
				christmasHat.SetActive(value: true);
			}
		}
	}

	public void Initialize(KitchenHalloweenBatSaveData saveData)
	{
		splineFollower.SetPercent(saveData.percent);
		christmasHatActive = saveData.christmasHatActive;
		christmasHat.SetActive(christmasHatActive);
	}

	public KitchenHalloweenBatSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Kitchen Halloween Bat " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		KitchenHalloweenBatSaveData result = default(KitchenHalloweenBatSaveData);
		result.id = id;
		result.christmasHatActive = christmasHatActive;
		result.percent = (float)splineFollower.GetPercent();
		return result;
	}

	private void ChristmasController_OnActivateChristmasEffects(object sender, EventArgs e)
	{
		christmasHatActive = true;
		christmasHat.SetActive(christmasHatActive);
	}
}
