using UnityEngine;

namespace _Scripts.Objects;

[RequireComponent(typeof(Light))]
public class KitchenLight : MonoBehaviour
{
	private Light kitchenLight;

	private float defaultIntensity;

	private void Awake()
	{
		kitchenLight = GetComponent<Light>();
		defaultIntensity = kitchenLight.intensity;
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SwitchOffHalf()
	{
		float value = kitchenLight.intensity - 0.5f * defaultIntensity;
		value = Mathf.Clamp(value, 0f, defaultIntensity);
		kitchenLight.intensity = value;
	}

	public void SwitchOnHalf()
	{
		float value = kitchenLight.intensity + 0.5f * defaultIntensity;
		value = Mathf.Clamp(value, 0f, defaultIntensity);
		kitchenLight.intensity = value;
	}
}
