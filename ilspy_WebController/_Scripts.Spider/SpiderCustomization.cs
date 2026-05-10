using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using _Scripts.CosmeticItems;
using _Scripts.Objects;
using _Scripts.Singletons;
using _Scripts.Wardrobe;

namespace _Scripts.Spider;

public class SpiderCustomization : MonoBehaviour
{
	[SerializeField]
	private BodyMovement bodyMovement;

	private float burnAmount;

	private static MaterialPropertyBlock mpb;

	private static readonly int bodyColorLightId = Shader.PropertyToID("_BodyColorLight");

	private static readonly int bodyColorShadowId = Shader.PropertyToID("_BodyColorShadow");

	private static readonly int legColorLightId = Shader.PropertyToID("_LegColorLight");

	private static readonly int legColorShadowId = Shader.PropertyToID("_LegColorShadow");

	private static readonly int jointColorLightId = Shader.PropertyToID("_JointColorLight");

	private static readonly int jointColorShadowId = Shader.PropertyToID("_JointColorShadow");

	private static readonly int colorId = Shader.PropertyToID("_Color");

	private static readonly int burnAmountId = Shader.PropertyToID("_BurnAmount");

	[SerializeField]
	private OutfitSo outfitSo;

	[SerializeField]
	private MeshRenderer[] bodyMeshRenderers;

	[SerializeField]
	private GameObject[] bodyGameObjects;

	[SerializeField]
	private MeshRenderer[] bodyFluffyMeshRenderers;

	[SerializeField]
	private GameObject[] bodyFluffyGameObjects;

	[SerializeField]
	[Range(0f, 1f)]
	private float bodyFluffiness;

	[SerializeField]
	private float bodyShellLengthFactor = 1f;

	[FormerlySerializedAs("defaultBodyColor")]
	[SerializeField]
	private Color bodyColor;

	[SerializeField]
	private MeshRenderer arachnophobiaMeshRenderer;

	[SerializeField]
	private float arachnophobiaShellLengthFactor;

	private bool bodyEnabled = true;

	[SerializeField]
	private MeshRenderer abdomenMeshRenderer;

	[SerializeField]
	private bool abdomenEnabled;

	[SerializeField]
	[Range(0f, 1f)]
	private float abdomenFluffiness;

	[SerializeField]
	private float abdomenShellLengthFactor = 1f;

	[FormerlySerializedAs("defaultAbdomenColor")]
	[SerializeField]
	private Color abdomenColor;

	[SerializeField]
	private MeshRenderer[] legInnerMeshRenderers;

	[SerializeField]
	private MeshRenderer[] legMiddleMeshRenderers;

	[SerializeField]
	private MeshRenderer[] legTipMeshRenderers;

	[SerializeField]
	private GameObject[] completeLegs;

	[SerializeField]
	[Range(0f, 1f)]
	private float[] legSegmentFluffiness = new float[3];

	[SerializeField]
	private float legShellLengthFactor = 1f;

	[SerializeField]
	private Color[] legColors = new Color[3]
	{
		Color.black,
		Color.black,
		Color.black
	};

	private bool[] legsEnabled = new bool[8] { true, true, true, true, true, true, true, true };

	private MeshRenderer[][] legMeshRenderers;

	[SerializeField]
	private MeshRenderer[] jointInnerMeshRenderers;

	[SerializeField]
	private MeshRenderer[] jointMiddleMeshRenderers;

	[SerializeField]
	private MeshRenderer[] jointTipMeshRenderers;

	[SerializeField]
	[Range(0f, 1f)]
	private float[] jointSegmentFluffiness = new float[3];

	[SerializeField]
	private float jointShellLengthFactor = 1f;

	[SerializeField]
	private Color[] jointColors = new Color[3]
	{
		Color.black,
		Color.black,
		Color.black
	};

	private MeshRenderer[][] jointMeshRenderers;

	[SerializeField]
	private Transform normalEyesContainer;

	[SerializeField]
	private Transform ballEyesContainer;

	[FormerlySerializedAs("defaultEyeIndex")]
	[SerializeField]
	private int eyeIndex;

	[FormerlySerializedAs("defaultEyeColorBase")]
	[SerializeField]
	private Color eyeColorBase;

	[FormerlySerializedAs("defaultEyeColorLeft")]
	[SerializeField]
	private Color eyeColorLeft;

	[FormerlySerializedAs("defaultEyeColorRight")]
	[SerializeField]
	private Color eyeColorRight;

	[SerializeField]
	private float[] eyeEffects = new float[6];

	private Eye activeNormalEyes;

	private Eye activeBallEyes;

	[SerializeField]
	private Transform normalHatContainer;

	[SerializeField]
	private Transform ballHatContainer;

	[FormerlySerializedAs("defaultHatIndex")]
	[SerializeField]
	private int hatIndex;

	[SerializeField]
	private Color[] hatColors = new Color[6];

	[SerializeField]
	private float[] hatEffects = new float[6];

	[SerializeField]
	private float hatOffset;

	[SerializeField]
	private float hatOffsetBall;

	private List<CosmeticItemHatSo> hatList;

	private Hat activeNormalHat;

	private Hat activeBallHat;

	[SerializeField]
	private Transform normalAccessoryContainer;

	[SerializeField]
	private Transform ballAccessoryContainer;

	[FormerlySerializedAs("defaultAccessoryIndex")]
	[SerializeField]
	private int accessoryIndex;

	[SerializeField]
	private Color[] accessoryColors = new Color[6];

	[SerializeField]
	private float[] accessoryEffects = new float[6];

	private List<CosmeticItemAccessorySo> accessoryList;

	private Accessory activeNormalAccessory;

	private Accessory activeBallAccessory;

	[SerializeField]
	private Transform[] shoeContainersLeft;

	[SerializeField]
	private Transform[] shoeContainersRight;

	[SerializeField]
	private GameObject[] tipGraphics;

	[FormerlySerializedAs("defaultShoeIndex")]
	[SerializeField]
	private int shoeIndex;

	[SerializeField]
	private Color[] shoeColors = new Color[6];

	[SerializeField]
	private float[] shoeEffects = new float[6];

	private List<Shoe> activeShoes;

	private void OnEnable()
	{
		if (Singleton<SettingsController>.Instance != null)
		{
			SettingsController.OnSettingsUpdated += SettingsController_OnSettingsUpdated;
		}
		BurnableObject component = GetComponent<BurnableObject>();
		if (component != null)
		{
			component.BurnAmountChanged += BurnableObject_OnBurnAmountChanged;
		}
		if (Singleton<GameController>.Instance != null)
		{
			Singleton<GameController>.Instance.OnContinueGame += GameController_OnContinueGame;
		}
	}

	private void OnDisable()
	{
		if (Singleton<SettingsController>.Instance != null)
		{
			SettingsController.OnSettingsUpdated -= SettingsController_OnSettingsUpdated;
		}
		BurnableObject component = GetComponent<BurnableObject>();
		if (component != null)
		{
			component.BurnAmountChanged -= BurnableObject_OnBurnAmountChanged;
		}
		if (Singleton<GameController>.Instance != null)
		{
			Singleton<GameController>.Instance.OnContinueGame -= GameController_OnContinueGame;
		}
	}

	private void Awake()
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		legMeshRenderers = new MeshRenderer[3][];
		legMeshRenderers[0] = legInnerMeshRenderers;
		legMeshRenderers[1] = legMiddleMeshRenderers;
		legMeshRenderers[2] = legTipMeshRenderers;
		jointMeshRenderers = new MeshRenderer[3][];
		jointMeshRenderers[0] = jointInnerMeshRenderers;
		jointMeshRenderers[1] = jointMiddleMeshRenderers;
		jointMeshRenderers[2] = jointTipMeshRenderers;
		hatList = Singleton<CosmeticItemsController>.Instance.GetAllItemsOfType<CosmeticItemHatSo>();
		accessoryList = Singleton<CosmeticItemsController>.Instance.GetAllItemsOfType<CosmeticItemAccessorySo>();
		activeShoes = new List<Shoe>();
		if (bodyMovement.IsPlayer)
		{
			LoadDefaultIndices();
			SaveSpiderCustomization();
		}
		else if (outfitSo != null)
		{
			LoadOutfit();
		}
		Refresh();
	}

	public void LoadOutfit()
	{
		bodyEnabled = outfitSo.bodyEnabled;
		bodyColor = outfitSo.bodyColor;
		bodyFluffiness = outfitSo.bodyFluffiness;
		abdomenEnabled = outfitSo.abdomenEnabled;
		abdomenColor = outfitSo.abdomenColor;
		abdomenFluffiness = outfitSo.abdomenFluffiness;
		legsEnabled = outfitSo.legsEnabled;
		legColors = outfitSo.legColors;
		legSegmentFluffiness = outfitSo.legSegmentFluffiness;
		jointColors = outfitSo.jointColors;
		jointSegmentFluffiness = outfitSo.jointSegmentFluffiness;
		eyeIndex = outfitSo.eyeIndex;
		eyeColorBase = outfitSo.eyeColorBase;
		eyeColorLeft = outfitSo.eyeColorLeft;
		eyeColorRight = outfitSo.eyeColorRight;
		eyeEffects = outfitSo.eyeEffects;
		hatIndex = outfitSo.hatIndex;
		hatColors = outfitSo.hatColors;
		hatEffects = outfitSo.hatEffects;
		accessoryIndex = outfitSo.accessoryIndex;
		accessoryColors = outfitSo.accessoryColors;
		accessoryEffects = outfitSo.accessoryEffects;
		shoeIndex = outfitSo.shoeIndex;
		shoeColors = outfitSo.shoeColors;
		shoeEffects = outfitSo.shoeEffects;
	}

	private void OnBodyColorChanged()
	{
		SetBodyColor(bodyColor);
	}

	public void SetBodyColor(Color color)
	{
		MeshRenderer[] array;
		if (!Application.isPlaying)
		{
			array = bodyMeshRenderers;
			foreach (MeshRenderer meshRenderer in array)
			{
				SetColor(meshRenderer, color);
			}
			return;
		}
		bodyColor = color;
		array = bodyMeshRenderers;
		foreach (MeshRenderer meshRenderer2 in array)
		{
			SetColor(meshRenderer2, color);
		}
		array = bodyFluffyMeshRenderers;
		foreach (MeshRenderer meshRenderer3 in array)
		{
			SetColor(meshRenderer3, color);
			SimpleShell component = meshRenderer3.GetComponent<SimpleShell>();
			if (component != null && Application.isPlaying)
			{
				component.UpdateColors(color, color * 0.6f);
			}
		}
		if (arachnophobiaMeshRenderer != null)
		{
			arachnophobiaMeshRenderer.GetPropertyBlock(mpb);
			mpb.SetColor(bodyColorLightId, color);
			mpb.SetColor(bodyColorShadowId, color * 0.6f);
			arachnophobiaMeshRenderer.SetPropertyBlock(mpb);
		}
		SimpleShellArachnophobia component2 = arachnophobiaMeshRenderer.GetComponent<SimpleShellArachnophobia>();
		if (component2 != null)
		{
			component2.UpdateBodyColors(color, color * 0.6f);
		}
	}

	public void SetBodyFluffiness(float value)
	{
		if (!Application.isPlaying)
		{
			return;
		}
		bodyFluffiness = value;
		bool flag = bodyFluffiness > 0f;
		GameObject[] array = bodyGameObjects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(bodyEnabled && !flag);
		}
		array = bodyFluffyGameObjects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(bodyEnabled && flag);
		}
		MeshRenderer[] array2 = bodyFluffyMeshRenderers;
		for (int i = 0; i < array2.Length; i++)
		{
			SimpleShell component = array2[i].GetComponent<SimpleShell>();
			if (component != null)
			{
				component.SetLength(bodyFluffiness * bodyShellLengthFactor);
			}
		}
		SimpleShellArachnophobia component2 = arachnophobiaMeshRenderer.GetComponent<SimpleShellArachnophobia>();
		if (component2 != null)
		{
			component2.enabled = flag;
			component2.SetLength(bodyFluffiness * arachnophobiaShellLengthFactor);
		}
		UpdateHatOffsets();
	}

	private void UpdateBodyBurnAmount()
	{
		MeshRenderer[] array = bodyMeshRenderers;
		foreach (MeshRenderer meshRenderer in array)
		{
			SetBurnAmount(meshRenderer, burnAmount);
		}
		array = bodyFluffyMeshRenderers;
		foreach (MeshRenderer meshRenderer2 in array)
		{
			SetBurnAmount(meshRenderer2, burnAmount);
			SimpleShell component = meshRenderer2.GetComponent<SimpleShell>();
			if (component != null && Application.isPlaying)
			{
				component.SetBurnAmount(burnAmount);
			}
		}
		SetBurnAmount(arachnophobiaMeshRenderer, burnAmount);
		SimpleShellArachnophobia component2 = arachnophobiaMeshRenderer.GetComponent<SimpleShellArachnophobia>();
		if (component2 != null)
		{
			component2.SetBurnAmount(burnAmount);
		}
	}

	public void SetBodyEnabled(bool value)
	{
		bodyEnabled = value;
		bool flag = bodyFluffiness > 0f;
		MeshRenderer[] array = bodyMeshRenderers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(bodyEnabled && !flag);
		}
		GameObject[] array2 = bodyFluffyGameObjects;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].SetActive(bodyEnabled && flag);
		}
		arachnophobiaMeshRenderer.gameObject.SetActive(bodyEnabled);
	}

	private void OnAbdomenColorChanged()
	{
		SetAbdomenColor(abdomenColor);
	}

	private void OnAbdomenEnabledChanged()
	{
		SetAbdomenEnabled(abdomenEnabled);
	}

	public void SetAbdomenEnabled(bool value)
	{
		if (!(abdomenMeshRenderer == null))
		{
			abdomenEnabled = value;
			abdomenMeshRenderer.gameObject.SetActive(abdomenEnabled);
		}
	}

	public void SetAbdomenColor(Color color)
	{
		if (abdomenMeshRenderer == null)
		{
			return;
		}
		if (!Application.isPlaying)
		{
			SetColor(abdomenMeshRenderer, color);
			return;
		}
		abdomenColor = color;
		SetColor(abdomenMeshRenderer, color);
		SimpleShell component = abdomenMeshRenderer.GetComponent<SimpleShell>();
		if (component != null)
		{
			component.UpdateColors(color, color * 0.6f);
		}
	}

	public void SetAbdomenFluffiness(float value)
	{
		abdomenFluffiness = value;
		if (Application.isPlaying)
		{
			SimpleShell component = abdomenMeshRenderer.GetComponent<SimpleShell>();
			if (component != null)
			{
				component.SetLength(abdomenFluffiness * abdomenShellLengthFactor);
			}
		}
	}

	private void UpdateAbdomenBurnAmount()
	{
		if (!(abdomenMeshRenderer == null))
		{
			SetBurnAmount(abdomenMeshRenderer, burnAmount);
			SimpleShell component = abdomenMeshRenderer.GetComponent<SimpleShell>();
			if (component != null)
			{
				component.SetBurnAmount(burnAmount);
			}
		}
	}

	private void OnLegColorChanged()
	{
		for (int i = 0; i < 3; i++)
		{
			SetLegColor(i, legColors[i]);
		}
	}

	public void SetLegColor(int index, Color color)
	{
		MeshRenderer[] array;
		if (!Application.isPlaying)
		{
			array = index switch
			{
				0 => legInnerMeshRenderers, 
				1 => legMiddleMeshRenderers, 
				2 => legTipMeshRenderers, 
				_ => Array.Empty<MeshRenderer>(), 
			};
			foreach (MeshRenderer meshRenderer in array)
			{
				SetColor(meshRenderer, color);
			}
			return;
		}
		legColors[index] = color;
		SetShoeLegColor();
		array = legMeshRenderers[index];
		foreach (MeshRenderer meshRenderer2 in array)
		{
			SetColor(meshRenderer2, color);
			SimpleShell component = meshRenderer2.GetComponent<SimpleShell>();
			if (component != null)
			{
				component.UpdateColors(color, color * 0.6f);
			}
		}
		if (index == 0)
		{
			if (arachnophobiaMeshRenderer != null)
			{
				arachnophobiaMeshRenderer.GetPropertyBlock(mpb);
				mpb.SetColor(legColorLightId, color);
				mpb.SetColor(legColorShadowId, color * 0.6f);
				arachnophobiaMeshRenderer.SetPropertyBlock(mpb);
			}
			SimpleShellArachnophobia component2 = arachnophobiaMeshRenderer.GetComponent<SimpleShellArachnophobia>();
			if (component2 != null)
			{
				component2.UpdateLegColors(color, color * 0.6f);
			}
		}
	}

	public void SetLegFluffiness(int index, float value)
	{
		if (!Application.isPlaying)
		{
			return;
		}
		legSegmentFluffiness[index] = value;
		MeshRenderer[] array = legMeshRenderers[index];
		for (int i = 0; i < array.Length; i++)
		{
			SimpleShell component = array[i].GetComponent<SimpleShell>();
			if (component != null)
			{
				component.SetLength(legSegmentFluffiness[index] * legShellLengthFactor);
			}
		}
		SetShoeLegFluffiness();
	}

	private void SetLegsEnabled(bool[] value)
	{
		legsEnabled = value;
		for (int i = 0; i < completeLegs.Length; i++)
		{
			completeLegs[i].SetActive(legsEnabled[i]);
		}
	}

	public void EnableLeg(int index, bool value)
	{
		legsEnabled[index] = value;
		for (int i = 0; i < completeLegs.Length; i++)
		{
			completeLegs[i].SetActive(legsEnabled[i]);
		}
	}

	private void UpdateLegBurnAmount()
	{
		for (int i = 0; i < legMeshRenderers.Length; i++)
		{
			MeshRenderer[] array = legMeshRenderers[i];
			foreach (MeshRenderer meshRenderer in array)
			{
				SetBurnAmount(meshRenderer, burnAmount);
				SimpleShell component = meshRenderer.GetComponent<SimpleShell>();
				if (component != null)
				{
					component.SetBurnAmount(burnAmount);
				}
			}
			SetBurnAmount(arachnophobiaMeshRenderer, burnAmount);
			SimpleShellArachnophobia component2 = arachnophobiaMeshRenderer.GetComponent<SimpleShellArachnophobia>();
			if (component2 != null)
			{
				component2.SetBurnAmount(burnAmount);
			}
		}
	}

	private void OnJointColorChanged()
	{
		for (int i = 0; i < 3; i++)
		{
			SetJointColor(i, jointColors[i]);
		}
	}

	public void SetJointColor(int index, Color color)
	{
		MeshRenderer[] array;
		if (!Application.isPlaying)
		{
			array = index switch
			{
				0 => jointInnerMeshRenderers, 
				1 => jointMiddleMeshRenderers, 
				2 => jointTipMeshRenderers, 
				_ => Array.Empty<MeshRenderer>(), 
			};
			foreach (MeshRenderer meshRenderer in array)
			{
				SetColor(meshRenderer, color);
			}
			return;
		}
		jointColors[index] = color;
		array = jointMeshRenderers[index];
		foreach (MeshRenderer meshRenderer2 in array)
		{
			SetColor(meshRenderer2, color);
			SimpleShell component = meshRenderer2.GetComponent<SimpleShell>();
			if (component != null)
			{
				component.UpdateColors(color, color * 0.6f);
			}
		}
		if (index == 0)
		{
			if (arachnophobiaMeshRenderer != null)
			{
				arachnophobiaMeshRenderer.GetPropertyBlock(mpb);
				mpb.SetColor(jointColorLightId, color);
				mpb.SetColor(jointColorShadowId, color * 0.6f);
				arachnophobiaMeshRenderer.SetPropertyBlock(mpb);
			}
			SimpleShellArachnophobia component2 = arachnophobiaMeshRenderer.GetComponent<SimpleShellArachnophobia>();
			if (component2 != null)
			{
				component2.UpdateJointColors(color, color * 0.6f);
			}
		}
	}

	public void SetJointFluffiness(int index, float value)
	{
		if (!Application.isPlaying)
		{
			return;
		}
		jointSegmentFluffiness[index] = value;
		MeshRenderer[] array = jointMeshRenderers[index];
		for (int i = 0; i < array.Length; i++)
		{
			SimpleShell component = array[i].GetComponent<SimpleShell>();
			if (component != null)
			{
				component.SetLength(jointSegmentFluffiness[index] * jointShellLengthFactor);
			}
		}
	}

	private void UpdateJointBurnAmount()
	{
		for (int i = 0; i < jointMeshRenderers.Length; i++)
		{
			MeshRenderer[] array = jointMeshRenderers[i];
			foreach (MeshRenderer meshRenderer in array)
			{
				SetBurnAmount(meshRenderer, burnAmount);
				SimpleShell component = meshRenderer.GetComponent<SimpleShell>();
				if (component != null)
				{
					component.SetBurnAmount(burnAmount);
				}
			}
			SetBurnAmount(arachnophobiaMeshRenderer, burnAmount);
			SimpleShellArachnophobia component2 = arachnophobiaMeshRenderer.GetComponent<SimpleShellArachnophobia>();
			if (component2 != null)
			{
				component2.SetBurnAmount(burnAmount);
			}
		}
	}

	private void ChangeEyeRuntime()
	{
		if (base.gameObject.activeSelf)
		{
			for (int i = 0; i < normalEyesContainer.childCount; i++)
			{
				UnityEngine.Object.Destroy(normalEyesContainer.GetChild(i).gameObject);
			}
			activeNormalEyes = UnityEngine.Object.Instantiate(GetEyeItem().eyeSo.eye, normalEyesContainer);
			for (int j = 0; j < ballEyesContainer.childCount; j++)
			{
				UnityEngine.Object.Destroy(ballEyesContainer.GetChild(j).gameObject);
			}
			activeBallEyes = UnityEngine.Object.Instantiate(GetEyeItem().eyeSo.eye, ballEyesContainer);
		}
	}

	public void ChangeEye(int newEyeIndex, bool resetColorAndEffects = true)
	{
		eyeIndex = newEyeIndex;
		ChangeEyeRuntime();
		SetEyeColorBase(eyeColorBase);
		SetEyeColorLeft(eyeColorLeft);
		SetEyeColorRight(eyeColorRight);
		for (int i = 0; i < 6; i++)
		{
			SetEyeEffect(i, eyeEffects[i]);
		}
	}

	private CosmeticItemEyeSo GetEyeItem()
	{
		if (Singleton<CosmeticItemsController>.Instance == null)
		{
			return null;
		}
		int validEyeIndex = Singleton<CosmeticItemsController>.Instance.GetValidEyeIndex(eyeIndex);
		return Singleton<CosmeticItemsController>.Instance.GetCosmeticItem(validEyeIndex) as CosmeticItemEyeSo;
	}

	public void SetEyeColorBase(Color color)
	{
		eyeColorBase = color;
		if (activeNormalEyes != null)
		{
			activeNormalEyes.SetEyeColor(0, color);
		}
		if (activeBallEyes != null)
		{
			activeBallEyes.SetEyeColor(0, color);
		}
	}

	public void SetEyeColorLeft(Color color)
	{
		eyeColorLeft = color;
		if (activeNormalEyes != null)
		{
			activeNormalEyes.SetEyeColor(1, color);
		}
		if (activeBallEyes != null)
		{
			activeBallEyes.SetEyeColor(1, color);
		}
	}

	public void SetEyeColorRight(Color color)
	{
		eyeColorRight = color;
		if (activeNormalEyes != null)
		{
			activeNormalEyes.SetEyeColor(2, color);
		}
		if (activeBallEyes != null)
		{
			activeBallEyes.SetEyeColor(2, color);
		}
	}

	public void SetEyeEffect(int index, float value)
	{
		if (index < GetEyeItem().eyeSo.eye.NumberOfEffects)
		{
			eyeEffects[index] = value;
			if (activeNormalEyes != null)
			{
				activeNormalEyes.SetEyeEffect(index, value);
			}
			if (activeBallEyes != null)
			{
				activeBallEyes.SetEyeEffect(index, value);
			}
		}
	}

	private void ChangeHatRuntime()
	{
		if (base.gameObject.activeSelf)
		{
			for (int i = 0; i < normalHatContainer.childCount; i++)
			{
				UnityEngine.Object.Destroy(normalHatContainer.GetChild(i).gameObject);
			}
			activeNormalHat = UnityEngine.Object.Instantiate(GetHatItem().hatSo.hat, normalHatContainer);
			for (int j = 0; j < ballHatContainer.childCount; j++)
			{
				UnityEngine.Object.Destroy(ballHatContainer.GetChild(j).gameObject);
			}
			activeBallHat = UnityEngine.Object.Instantiate(GetHatItem().hatSo.hat, ballHatContainer);
			UpdateHatOffsets();
		}
	}

	private void UpdateHatOffsets()
	{
		if (activeNormalHat != null)
		{
			activeNormalHat.transform.localPosition = Vector3.up * bodyFluffiness * hatOffset;
		}
		if (activeBallHat != null)
		{
			activeBallHat.transform.localPosition = Vector3.up * bodyFluffiness * hatOffsetBall;
		}
	}

	public void ChangeHat(int newHatIndex, bool resetColorsAndEffects = true)
	{
		hatIndex = newHatIndex;
		ChangeHatRuntime();
		if (resetColorsAndEffects)
		{
			for (int i = 0; i < 6; i++)
			{
				Hat hat = GetHatItem().hatSo.hat;
				SetHatColor(i, hat.GetDefaultColor(i));
				SetHatEffect(i, hat.GetDefaultEffect(i));
			}
		}
		UpdateHatBurnAmount();
	}

	public void SetHatColor(int index, Color color)
	{
		if (index < GetHatItem().hatSo.hat.NumberOfColors)
		{
			hatColors[index] = color;
			if (activeNormalHat != null)
			{
				activeNormalHat.SetHatColor(index, color);
			}
			if (activeBallHat != null)
			{
				activeBallHat.SetHatColor(index, color);
			}
		}
	}

	public void SetHatEffect(int index, float value)
	{
		if (index < GetHatItem().hatSo.hat.NumberOfEffects)
		{
			hatEffects[index] = value;
			if (activeNormalHat != null)
			{
				activeNormalHat.SetHatEffect(index, value);
			}
			if (activeBallHat != null)
			{
				activeBallHat.SetHatEffect(index, value);
			}
		}
	}

	private CosmeticItemHatSo GetHatItem()
	{
		if (Singleton<CosmeticItemsController>.Instance == null)
		{
			return null;
		}
		int validHatIndex = Singleton<CosmeticItemsController>.Instance.GetValidHatIndex(hatIndex);
		return Singleton<CosmeticItemsController>.Instance.GetCosmeticItem(validHatIndex) as CosmeticItemHatSo;
	}

	public void EnableHat(bool value)
	{
		activeNormalHat.gameObject.SetActive(value);
		activeBallHat.gameObject.SetActive(value);
	}

	private void UpdateHatBurnAmount()
	{
		if (activeNormalHat != null)
		{
			activeNormalHat.SetHatBurnAmount(burnAmount);
		}
		if (activeBallHat != null)
		{
			activeBallHat.SetHatBurnAmount(burnAmount);
		}
	}

	private void ChangeAccessoryRuntime()
	{
		if (base.gameObject.activeSelf)
		{
			for (int i = 0; i < normalAccessoryContainer.childCount; i++)
			{
				UnityEngine.Object.Destroy(normalAccessoryContainer.GetChild(i).gameObject);
			}
			activeNormalAccessory = UnityEngine.Object.Instantiate(GetAccessoryItem().accessorySo.accessory, normalAccessoryContainer);
			for (int j = 0; j < ballAccessoryContainer.childCount; j++)
			{
				UnityEngine.Object.Destroy(ballAccessoryContainer.GetChild(j).gameObject);
			}
			activeBallAccessory = UnityEngine.Object.Instantiate(GetAccessoryItem().accessorySo.accessory, ballAccessoryContainer);
		}
	}

	public void SetAccessoryColor(int index, Color color)
	{
		if (index < GetAccessoryItem().accessorySo.accessory.NumberOfColors)
		{
			accessoryColors[index] = color;
			if (activeNormalAccessory != null)
			{
				activeNormalAccessory.SetAccessoryColor(index, color);
			}
			if (activeBallAccessory != null)
			{
				activeBallAccessory.SetAccessoryColor(index, color);
			}
		}
	}

	public void SetAccessoryEffect(int index, float value)
	{
		if (index < GetAccessoryItem().accessorySo.accessory.NumberOfEffects)
		{
			accessoryEffects[index] = value;
			if (activeNormalAccessory != null)
			{
				activeNormalAccessory.SetAccessoryEffect(index, value);
			}
			if (activeBallAccessory != null)
			{
				activeBallAccessory.SetAccessoryEffect(index, value);
			}
		}
	}

	public void ChangeAccessory(int newAccessoryIndex, bool resetColorsAndEffects = true)
	{
		accessoryIndex = newAccessoryIndex;
		ChangeAccessoryRuntime();
		if (resetColorsAndEffects)
		{
			for (int i = 0; i < 6; i++)
			{
				Accessory accessory = GetAccessoryItem().accessorySo.accessory;
				SetAccessoryColor(i, accessory.GetDefaultColor(i));
				SetAccessoryEffect(i, accessory.GetDefaultEffect(i));
			}
		}
		UpdateAccessoryBurnAmount();
	}

	private CosmeticItemAccessorySo GetAccessoryItem()
	{
		if (Singleton<CosmeticItemsController>.Instance == null)
		{
			return null;
		}
		int validAccessoryIndex = Singleton<CosmeticItemsController>.Instance.GetValidAccessoryIndex(accessoryIndex);
		return Singleton<CosmeticItemsController>.Instance.GetCosmeticItem(validAccessoryIndex) as CosmeticItemAccessorySo;
	}

	private void UpdateAccessoryBurnAmount()
	{
		if (activeNormalAccessory != null)
		{
			activeNormalAccessory.SetAccessoryBurnAmount(burnAmount);
		}
		if (activeBallAccessory != null)
		{
			activeBallAccessory.SetAccessoryBurnAmount(burnAmount);
		}
	}

	private void ChangeShoeRuntime()
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		CosmeticItemShoeSo shoeItem = GetShoeItem();
		if (shoeItem == null)
		{
			return;
		}
		if (shoeIndex == Singleton<CosmeticItemsController>.Instance.GetDefaultShoeIndex())
		{
			GameObject[] array = tipGraphics;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
			for (int j = 0; j < 3; j++)
			{
				SetLegColor(j, legColors[j]);
			}
			SetLegFluffiness(2, legSegmentFluffiness[2]);
		}
		else
		{
			GameObject[] array = tipGraphics;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(!shoeItem.shoeSo.shoe.DisableDefaultTip);
			}
		}
		activeShoes = new List<Shoe>();
		Transform[] array2 = shoeContainersLeft;
		foreach (Transform transform in array2)
		{
			for (int k = 0; k < transform.childCount; k++)
			{
				UnityEngine.Object.Destroy(transform.GetChild(k).gameObject);
			}
			activeShoes.Add(UnityEngine.Object.Instantiate(shoeItem.shoeSo.shoe, transform));
		}
		array2 = shoeContainersRight;
		foreach (Transform transform2 in array2)
		{
			for (int l = 0; l < transform2.childCount; l++)
			{
				UnityEngine.Object.Destroy(transform2.GetChild(l).gameObject);
			}
			Shoe shoe = UnityEngine.Object.Instantiate(shoeItem.shoeSo.shoe, transform2);
			shoe.Flip();
			activeShoes.Add(shoe);
		}
		UpdateShoeBurnAmount();
	}

	private void SetShoeLegFluffiness()
	{
		if (activeShoes == null)
		{
			return;
		}
		foreach (Shoe activeShoe in activeShoes)
		{
			activeShoe.SetLegFluffiness(legSegmentFluffiness[2], legShellLengthFactor);
		}
	}

	private void SetShoeLegColor()
	{
		if (activeShoes == null)
		{
			return;
		}
		foreach (Shoe activeShoe in activeShoes)
		{
			if (activeShoe != null)
			{
				activeShoe.SetLegColorFluffy(legColors[2], legColors[2] * 0.6f);
				activeShoe.SetLegColor(legColors[2]);
			}
		}
	}

	public void ChangeShoe(int newShoeIndex, bool resetColors = true)
	{
		shoeIndex = newShoeIndex;
		ChangeShoeRuntime();
		if (resetColors)
		{
			for (int i = 0; i < 6; i++)
			{
				Shoe shoe = GetShoeItem().shoeSo.shoe;
				SetShoeColor(i, shoe.GetDefaultColor(i));
				SetShoeEffect(i, shoe.GetDefaultEffect(i));
			}
			SetShoeLegFluffiness();
			SetShoeLegColor();
		}
	}

	public void SetShoeColor(int index, Color color)
	{
		if (index >= GetShoeItem().shoeSo.shoe.NumberOfColors)
		{
			return;
		}
		shoeColors[index] = color;
		if (activeShoes == null)
		{
			return;
		}
		foreach (Shoe activeShoe in activeShoes)
		{
			if (activeShoe != null)
			{
				activeShoe.SetShoeColor(index, color);
			}
		}
	}

	public void SetShoeEffect(int index, float value)
	{
		if (index >= GetShoeItem().shoeSo.shoe.NumberOfEffects)
		{
			return;
		}
		shoeEffects[index] = value;
		if (activeShoes == null)
		{
			return;
		}
		foreach (Shoe activeShoe in activeShoes)
		{
			if (activeShoe != null)
			{
				activeShoe.SetShoeEffect(index, value);
			}
		}
	}

	private CosmeticItemShoeSo GetShoeItem()
	{
		if (Singleton<CosmeticItemsController>.Instance == null)
		{
			return null;
		}
		int validShoeIndex = Singleton<CosmeticItemsController>.Instance.GetValidShoeIndex(shoeIndex);
		return Singleton<CosmeticItemsController>.Instance.GetCosmeticItem(validShoeIndex) as CosmeticItemShoeSo;
	}

	private void UpdateShoeBurnAmount()
	{
		if (activeShoes == null)
		{
			return;
		}
		foreach (Shoe activeShoe in activeShoes)
		{
			if (activeShoe != null)
			{
				activeShoe.SetShoeBurnAmount(burnAmount);
				activeShoe.SetLegBurnAmount(burnAmount);
			}
		}
	}

	private void SetColor(MeshRenderer meshRenderer, Color color)
	{
		if (!(meshRenderer == null))
		{
			if (mpb == null)
			{
				mpb = new MaterialPropertyBlock();
			}
			meshRenderer.GetPropertyBlock(mpb);
			mpb.SetColor(colorId, color);
			meshRenderer.SetPropertyBlock(mpb);
		}
	}

	private void SetBurnAmount(MeshRenderer meshRenderer, float burnAmount)
	{
		if (!(meshRenderer == null))
		{
			if (mpb == null)
			{
				mpb = new MaterialPropertyBlock();
			}
			meshRenderer.GetPropertyBlock(mpb);
			mpb.SetFloat(burnAmountId, burnAmount);
			meshRenderer.SetPropertyBlock(mpb);
		}
	}

	public void LoadDefaultIndices()
	{
		bodyEnabled = Singleton<CosmeticItemsController>.Instance.LoadBodyEnabled();
		bodyColor = Singleton<CosmeticItemsController>.Instance.LoadBodyColor();
		bodyFluffiness = Singleton<CosmeticItemsController>.Instance.LoadBodyFluffiness();
		abdomenEnabled = Singleton<CosmeticItemsController>.Instance.LoadAbdomenEnabled();
		abdomenColor = Singleton<CosmeticItemsController>.Instance.LoadAbdomenColor();
		abdomenFluffiness = Singleton<CosmeticItemsController>.Instance.LoadAbdomenFluffiness();
		legsEnabled = Singleton<CosmeticItemsController>.Instance.LoadLegsEnabled();
		for (int i = 0; i < 3; i++)
		{
			legColors[i] = Singleton<CosmeticItemsController>.Instance.LoadLegColor(i);
			jointColors[i] = Singleton<CosmeticItemsController>.Instance.LoadJointColor(i);
			legSegmentFluffiness[i] = Singleton<CosmeticItemsController>.Instance.LoadLegFluffiness(i);
			jointSegmentFluffiness[i] = Singleton<CosmeticItemsController>.Instance.LoadJointFluffiness(i);
		}
		eyeIndex = Singleton<CosmeticItemsController>.Instance.LoadEye();
		eyeColorBase = Singleton<CosmeticItemsController>.Instance.LoadEyeColorBase();
		eyeColorLeft = Singleton<CosmeticItemsController>.Instance.LoadEyeColorLeft();
		eyeColorRight = Singleton<CosmeticItemsController>.Instance.LoadEyeColorRight();
		hatIndex = Singleton<CosmeticItemsController>.Instance.LoadHat();
		accessoryIndex = Singleton<CosmeticItemsController>.Instance.LoadAccessory();
		shoeIndex = Singleton<CosmeticItemsController>.Instance.LoadShoe();
		for (int j = 0; j < 6; j++)
		{
			Eye eye = GetEyeItem().eyeSo.eye;
			if (j < eye.NumberOfEffects)
			{
				eyeEffects[j] = (Singleton<CosmeticItemsController>.Instance.EyeEffectExists(j) ? Singleton<CosmeticItemsController>.Instance.LoadEyeEffect(j) : eye.GetDefaultEffect(j));
			}
			Hat hat = GetHatItem().hatSo.hat;
			if (j < hat.NumberOfColors)
			{
				hatColors[j] = (Singleton<CosmeticItemsController>.Instance.HatColorExists(j) ? Singleton<CosmeticItemsController>.Instance.LoadHatColor(j) : hat.GetDefaultColor(j));
			}
			if (j < hat.NumberOfEffects)
			{
				hatEffects[j] = (Singleton<CosmeticItemsController>.Instance.HatEffectExists(j) ? Singleton<CosmeticItemsController>.Instance.LoadHatEffect(j) : hat.GetDefaultEffect(j));
			}
			Accessory accessory = GetAccessoryItem().accessorySo.accessory;
			if (j < accessory.NumberOfColors)
			{
				accessoryColors[j] = (Singleton<CosmeticItemsController>.Instance.AccessoryColorExists(j) ? Singleton<CosmeticItemsController>.Instance.LoadAccessoryColor(j) : accessory.GetDefaultColor(j));
			}
			if (j < accessory.NumberOfEffects)
			{
				accessoryEffects[j] = (Singleton<CosmeticItemsController>.Instance.AccessoryEffectExists(j) ? Singleton<CosmeticItemsController>.Instance.LoadAccessoryEffect(j) : accessory.GetDefaultEffect(j));
			}
			Shoe shoe = GetShoeItem().shoeSo.shoe;
			if (j < shoe.NumberOfColors)
			{
				shoeColors[j] = (Singleton<CosmeticItemsController>.Instance.ShoeColorExists(j) ? Singleton<CosmeticItemsController>.Instance.LoadShoeColor(j) : shoe.GetDefaultColor(j));
			}
			if (j < shoe.NumberOfEffects)
			{
				shoeEffects[j] = (Singleton<CosmeticItemsController>.Instance.ShoeEffectExists(j) ? Singleton<CosmeticItemsController>.Instance.LoadShoeEffect(j) : shoe.GetDefaultEffect(j));
			}
		}
	}

	private void SaveSpiderCustomization()
	{
		Singleton<CosmeticItemsController>.Instance.SaveBodyEnabled(bodyEnabled);
		Singleton<CosmeticItemsController>.Instance.SaveBodyColor(bodyColor);
		Singleton<CosmeticItemsController>.Instance.SaveBodyFluffiness(bodyFluffiness);
		Singleton<CosmeticItemsController>.Instance.SaveAbdomenEnabled(abdomenEnabled);
		Singleton<CosmeticItemsController>.Instance.SaveAbdomenColor(abdomenColor);
		Singleton<CosmeticItemsController>.Instance.SaveAbdomenFluffiness(abdomenFluffiness);
		Singleton<CosmeticItemsController>.Instance.SaveLegsEnabled(legsEnabled);
		for (int i = 0; i < 3; i++)
		{
			Singleton<CosmeticItemsController>.Instance.SaveLegColor(i, legColors[i]);
			Singleton<CosmeticItemsController>.Instance.SaveJointColor(i, jointColors[i]);
			Singleton<CosmeticItemsController>.Instance.SaveLegFluffiness(i, legSegmentFluffiness[i]);
			Singleton<CosmeticItemsController>.Instance.SaveJointFluffiness(i, jointSegmentFluffiness[i]);
		}
		Singleton<CosmeticItemsController>.Instance.SaveEye(eyeIndex);
		Singleton<CosmeticItemsController>.Instance.SaveEyeColorBase(eyeColorBase);
		Singleton<CosmeticItemsController>.Instance.SaveEyeColorLeft(eyeColorLeft);
		Singleton<CosmeticItemsController>.Instance.SaveEyeColorRight(eyeColorRight);
		Singleton<CosmeticItemsController>.Instance.SaveHat(hatIndex);
		Singleton<CosmeticItemsController>.Instance.SaveAccessory(accessoryIndex);
		Singleton<CosmeticItemsController>.Instance.SaveShoe(shoeIndex);
		for (int j = 0; j < 6; j++)
		{
			Eye eye = GetEyeItem().eyeSo.eye;
			if (j < eye.NumberOfEffects)
			{
				Singleton<CosmeticItemsController>.Instance.SaveEyeEffect(j, eyeEffects[j]);
			}
			else
			{
				Singleton<CosmeticItemsController>.Instance.DeleteEyeEffect(j);
			}
			Hat hat = GetHatItem().hatSo.hat;
			if (j < hat.NumberOfColors)
			{
				Singleton<CosmeticItemsController>.Instance.SaveHatColor(j, hatColors[j]);
			}
			else
			{
				Singleton<CosmeticItemsController>.Instance.DeleteHatColor(j);
			}
			if (j < hat.NumberOfEffects)
			{
				Singleton<CosmeticItemsController>.Instance.SaveHatEffect(j, hatEffects[j]);
			}
			else
			{
				Singleton<CosmeticItemsController>.Instance.DeleteHatEffect(j);
			}
			Accessory accessory = GetAccessoryItem().accessorySo.accessory;
			if (j < accessory.NumberOfColors)
			{
				Singleton<CosmeticItemsController>.Instance.SaveAccessoryColor(j, accessoryColors[j]);
			}
			else
			{
				Singleton<CosmeticItemsController>.Instance.DeleteAccessoryColor(j);
			}
			if (j < accessory.NumberOfEffects)
			{
				Singleton<CosmeticItemsController>.Instance.SaveAccessoryEffect(j, accessoryEffects[j]);
			}
			else
			{
				Singleton<CosmeticItemsController>.Instance.DeleteAccessoryEffect(j);
			}
			Shoe shoe = GetShoeItem().shoeSo.shoe;
			if (j < shoe.NumberOfColors)
			{
				Singleton<CosmeticItemsController>.Instance.SaveShoeColor(j, shoeColors[j]);
			}
			else
			{
				Singleton<CosmeticItemsController>.Instance.DeleteShoeColor(j);
			}
			if (j < shoe.NumberOfEffects)
			{
				Singleton<CosmeticItemsController>.Instance.SaveShoeEffect(j, shoeEffects[j]);
			}
			else
			{
				Singleton<CosmeticItemsController>.Instance.DeleteShoeEffect(j);
			}
		}
	}

	public void Refresh()
	{
		SetBodyEnabled(bodyEnabled);
		SetBodyColor(bodyColor);
		SetBodyFluffiness(bodyFluffiness);
		SetAbdomenEnabled(abdomenEnabled);
		SetAbdomenColor(abdomenColor);
		SetAbdomenFluffiness(abdomenFluffiness);
		SetLegsEnabled(legsEnabled);
		for (int i = 0; i < 3; i++)
		{
			SetLegColor(i, legColors[i]);
			SetJointColor(i, jointColors[i]);
			SetLegFluffiness(i, legSegmentFluffiness[i]);
			SetJointFluffiness(i, jointSegmentFluffiness[i]);
		}
		ChangeEye(eyeIndex, resetColorAndEffects: false);
		SetEyeColorBase(eyeColorBase);
		SetEyeColorLeft(eyeColorLeft);
		SetEyeColorRight(eyeColorRight);
		ChangeHat(hatIndex, resetColorsAndEffects: false);
		ChangeAccessory(accessoryIndex, resetColorsAndEffects: false);
		ChangeShoe(shoeIndex, resetColors: false);
		for (int j = 0; j < 6; j++)
		{
			if (j < GetEyeItem().eyeSo.eye.NumberOfEffects)
			{
				SetEyeEffect(j, eyeEffects[j]);
			}
			if (j < GetHatItem().hatSo.hat.NumberOfColors)
			{
				SetHatColor(j, hatColors[j]);
			}
			if (j < GetHatItem().hatSo.hat.NumberOfEffects)
			{
				SetHatEffect(j, hatEffects[j]);
			}
			if (j < GetAccessoryItem().accessorySo.accessory.NumberOfColors)
			{
				SetAccessoryColor(j, accessoryColors[j]);
			}
			if (j < GetAccessoryItem().accessorySo.accessory.NumberOfEffects)
			{
				SetAccessoryEffect(j, accessoryEffects[j]);
			}
			if (j < GetShoeItem().shoeSo.shoe.NumberOfColors)
			{
				SetShoeColor(j, shoeColors[j]);
			}
			if (j < GetShoeItem().shoeSo.shoe.NumberOfEffects)
			{
				SetShoeEffect(j, shoeEffects[j]);
			}
		}
		SetShoeLegColor();
		SetShoeLegFluffiness();
	}

	private void SettingsController_OnSettingsUpdated(object sender, EventArgs e)
	{
		if ((bool)this)
		{
			Refresh();
		}
	}

	private void BurnableObject_OnBurnAmountChanged(float value)
	{
		burnAmount = value;
		UpdateBodyBurnAmount();
		UpdateAbdomenBurnAmount();
		UpdateLegBurnAmount();
		UpdateJointBurnAmount();
		UpdateHatBurnAmount();
		UpdateAccessoryBurnAmount();
		UpdateShoeBurnAmount();
	}

	private void GameController_OnContinueGame(object sender, EventArgs e)
	{
		UpdateHatBurnAmount();
		UpdateAccessoryBurnAmount();
		UpdateShoeBurnAmount();
	}
}
