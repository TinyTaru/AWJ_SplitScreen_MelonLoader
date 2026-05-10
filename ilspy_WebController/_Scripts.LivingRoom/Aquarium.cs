using System;
using FMODUnity;
using UnityEngine;
using _Scripts.Objects;

namespace _Scripts.LivingRoom;

public class Aquarium : MonoBehaviour
{
	[SerializeField]
	private AquariumLightControl aquariumLightControl;

	[SerializeField]
	private MeshRenderer glassMeshRenderer;

	[SerializeField]
	private MeshRenderer lampMeshRenderer;

	[SerializeField]
	private MeshRenderer waterMeshRenderer;

	[SerializeField]
	private int lampMaterialIndex;

	[SerializeField]
	private WaterOrbSpawner waterOrbSpawner;

	[Header("Scavenger Hunt Quest")]
	[SerializeField]
	private GameObject scavengerHuntHint;

	[SerializeField]
	private float hintHue = 0.3083f;

	[SerializeField]
	private float hintSaturation = 1f;

	[SerializeField]
	private float hintValue = 0.8325f;

	[SerializeField]
	private float hintTolerance = 0.1f;

	[SerializeField]
	private EventReference hintVisibleSoundReference;

	[SerializeField]
	private EventReference hintInvisibleSoundReference;

	private bool hintVisibleOld;

	private static MaterialPropertyBlock mpb;

	private static readonly int aquariumColorId = Shader.PropertyToID("_Color");

	private static readonly int lampColorId = Shader.PropertyToID("_Color");

	private static readonly int waterShallowColorId = Shader.PropertyToID("_WaterColorShallow");

	private static readonly int waterUnderWaterColorId = Shader.PropertyToID("_WaterColorUnderwater");

	private static readonly int waterDeepColorId = Shader.PropertyToID("_WaterColorDeep");

	public event Action OnScavengerHuntHintVisible;

	private void Start()
	{
		hintVisibleOld = false;
		scavengerHuntHint.SetActive(hintVisibleOld);
		if (aquariumLightControl != null)
		{
			aquariumLightControl.OnColorChanged += AquariumLightControl_OnColorChanged;
		}
	}

	private void Update()
	{
		waterOrbSpawner.TrySpawnWaterOrb();
	}

	private void OnDestroy()
	{
		if (aquariumLightControl != null)
		{
			aquariumLightControl.OnColorChanged -= AquariumLightControl_OnColorChanged;
		}
	}

	private void AquariumLightControl_OnColorChanged(Color color)
	{
		Color.RGBToHSV(color, out var H, out var S, out var V);
		bool num = Mathf.Abs(H - hintHue) < hintTolerance;
		bool flag = Mathf.Abs(S - hintSaturation) < hintTolerance;
		bool flag2 = Mathf.Abs(V - hintValue) < hintTolerance;
		bool flag3 = num && flag && flag2;
		scavengerHuntHint.SetActive(flag3);
		if (!hintVisibleOld && flag3)
		{
			RuntimeManager.CreateInstance(hintVisibleSoundReference).start();
			this.OnScavengerHuntHintVisible?.Invoke();
		}
		else if (hintVisibleOld && !flag3)
		{
			RuntimeManager.CreateInstance(hintInvisibleSoundReference).start();
		}
		hintVisibleOld = flag3;
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		if (glassMeshRenderer != null)
		{
			glassMeshRenderer.GetPropertyBlock(mpb);
			mpb.SetColor(aquariumColorId, color);
			glassMeshRenderer.SetPropertyBlock(mpb);
		}
		if (lampMeshRenderer != null)
		{
			lampMeshRenderer.GetPropertyBlock(mpb, lampMaterialIndex);
			mpb.SetColor(lampColorId, color);
			lampMeshRenderer.SetPropertyBlock(mpb, lampMaterialIndex);
		}
		if (waterMeshRenderer != null)
		{
			float h = H + 0.01f;
			float s = S - 0.07f;
			float v = V - 0.18f;
			Color value = Color.HSVToRGB(h, s, v, hdr: false);
			float h2 = H + 0.02f;
			float s2 = S + 0.09f;
			float v2 = V - 0.08f;
			Color value2 = Color.HSVToRGB(h2, s2, v2, hdr: false);
			float h3 = H - 0.01f;
			float s3 = Mathf.Clamp(S - 0.18f, 0f, 0.8f);
			float v3 = Mathf.Clamp(V - 0.24f, 0.6f, 1f);
			Color value3 = Color.HSVToRGB(h3, s3, v3, hdr: false);
			value3.a = 0.5f;
			waterMeshRenderer.GetPropertyBlock(mpb);
			mpb.SetColor(waterShallowColorId, value);
			mpb.SetColor(waterDeepColorId, value2);
			mpb.SetColor(waterUnderWaterColorId, value3);
			waterMeshRenderer.SetPropertyBlock(mpb);
		}
	}
}
