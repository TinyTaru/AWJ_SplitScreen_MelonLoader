using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.Singletons;
using _Scripts.Spider;
using _Scripts.UI.ScrollView;
using _Scripts.Wardrobe;

namespace _Scripts.UI.Wardrobe._3._0;

public class OutfitController : MonoBehaviour
{
	[Header("Setup")]
	[SerializeField]
	private WardrobeControllerV3 wardrobeController;

	[SerializeField]
	private Transform outfitButtonContainer;

	[SerializeField]
	private OutfitButton outfitButtonPrefab;

	[SerializeField]
	private RectTransform outfitDeletionPanel;

	[SerializeField]
	private RectTransform outfitSavePanel;

	[SerializeField]
	private RectTransform outfitEditPanel;

	[SerializeField]
	private Button exitButton;

	[SerializeField]
	private TextMeshProUGUI deletionSaveOutfitName;

	[SerializeField]
	private TextMeshProUGUI saveOutfitName;

	[SerializeField]
	private TextMeshProUGUI editOutfitName;

	[SerializeField]
	private TMP_InputField renamingInputField;

	[SerializeField]
	private Button defaultOutfitButton;

	[SerializeField]
	private IndexedVerticalScrollController scrollController;

	private Outfit[] outfits;

	private List<OutfitButton> outfitButtons = new List<OutfitButton>();

	private Outfit selectedOutfit;

	private IEnumerator InvalidFileNameCoroutine()
	{
		Image image = renamingInputField.GetComponent<Image>();
		image.color = Color.crimson;
		yield return new WaitForSecondsRealtime(0.25f);
		image.color = Color.white;
	}

	private void InitializeOutfitButtons()
	{
		ClearButtons();
		if (outfits != null && outfits.Length != 0)
		{
			for (int i = 0; i < outfits.Length; i++)
			{
				OutfitButton outfitButton = Object.Instantiate(outfitButtonPrefab, outfitButtonContainer);
				outfitButton.Setup(this, outfits[i]);
				if (i == 0)
				{
					outfitButton.ToggleOnEnable();
				}
				outfitButtons.Add(outfitButton);
			}
		}
		scrollController.RegisterOutfitButtons(outfitButtons);
		scrollController.ScrollToTop();
		UpdateExitButtonNavigation();
	}

	private void ClearButtons()
	{
		for (int num = outfitButtons.Count - 1; num >= 0; num--)
		{
			Object.Destroy(outfitButtons[num].gameObject);
		}
		outfitButtons = new List<OutfitButton>();
	}

	private void UpdateExitButtonNavigation()
	{
		Navigation navigation = exitButton.navigation;
		if (outfitButtons.Count > 0 && outfitButtons[0] != null)
		{
			Selectable component = outfitButtons[0].transform.GetChild(0).GetComponent<Selectable>();
			if (component != null)
			{
				navigation.mode = Navigation.Mode.Explicit;
				navigation.selectOnDown = component;
			}
			else
			{
				navigation.mode = Navigation.Mode.Explicit;
				navigation.selectOnDown = null;
			}
		}
		else
		{
			navigation.mode = Navigation.Mode.Explicit;
			navigation.selectOnDown = defaultOutfitButton;
		}
		exitButton.navigation = navigation;
	}

	public void ShowOutfitList()
	{
		outfits = SaveController.LoadAllOutfits();
		InitializeOutfitButtons();
	}

	public void ApplyOutfit(Outfit outfit)
	{
		Singleton<CosmeticItemsController>.Instance.ApplyOutfit(outfit);
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
		{
			x.LoadDefaultIndices();
		});
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
		{
			x.Refresh();
		});
	}

	public void ApplyDefaultOutfit()
	{
		Outfit defaultOutfit = Singleton<CosmeticItemsController>.Instance.DefaultOutfit;
		defaultOutfit.arachnophobiaMode = SettingsController.ArachnophobiaMode;
		ApplyOutfit(defaultOutfit);
	}

	public void ShowSaveSlotRenamingPanel(Outfit outfit)
	{
		selectedOutfit = outfit;
		wardrobeController.OpenPanel(outfitEditPanel);
		editOutfitName.text = outfit.name;
		renamingInputField.text = "";
		renamingInputField.onSubmit.AddListener(RenameOutfit);
	}

	public void RenameOutfit(string text)
	{
		string text2 = renamingInputField.text;
		if (!SaveController.RenameOutfit(selectedOutfit.name, text2))
		{
			StartCoroutine(InvalidFileNameCoroutine());
			return;
		}
		renamingInputField.onSubmit.RemoveListener(RenameOutfit);
		ShowOutfitList();
		wardrobeController.GoBack();
	}

	public void SaveCurrentOutfit()
	{
		Singleton<CosmeticItemsController>.Instance.SaveCurrentOutfit();
		saveOutfitName.text = "Outfit_" + (Singleton<CosmeticItemsController>.Instance.GetOutfitCounter() - 1);
	}

	public void ShowSaveSlotDeletionPanel(Outfit outfit)
	{
		selectedOutfit = outfit;
		wardrobeController.OpenPanel(outfitDeletionPanel);
		deletionSaveOutfitName.text = outfit.name;
	}

	public void DeleteOutfit()
	{
		SaveController.DeleteOutfit(selectedOutfit.name);
		ShowOutfitList();
		wardrobeController.GoBack();
	}
}
