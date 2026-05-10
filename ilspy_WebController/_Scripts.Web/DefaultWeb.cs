using System.Collections.Generic;
using Dreamteck.Splines;
using UnityEngine;
using _Scripts.CosmeticItems;
using _Scripts.LevelSaving;
using _Scripts.Singletons;

namespace _Scripts.Web;

public class DefaultWeb : MonoBehaviour, IInitializable<DefaultWebSaveData>, IHasSaveData<DefaultWebSaveData>
{
	[SerializeField]
	private Color webColor = Color.white;

	[SerializeField]
	private WebThread webThread;

	[SerializeField]
	private float webThickness = 2f / 3f;

	[SerializeField]
	private CosmeticItemWebSo cosmeticItemWebSo;

	private LineRenderer[] webLineRenderers;

	private SplineComputer[] splines;

	private List<WebThread> defaultWebThreadList;

	private bool isInitialized;

	public LineRenderer[] WebLineRenderers => webLineRenderers;

	public SplineComputer[] Splines => splines;

	public bool IsInitialized => isInitialized;

	private void Awake()
	{
		webLineRenderers = GetComponentsInChildren<LineRenderer>();
		splines = GetComponentsInChildren<SplineComputer>();
	}

	private void OnEnable()
	{
		if (!isInitialized)
		{
			WebThread webThread = ((this.webThread == null) ? null : this.webThread);
			if (webThread != null)
			{
				webThread.SetWebSo(cosmeticItemWebSo.webSo);
			}
			int itemIndex = Singleton<CosmeticItemsController>.Instance.GetItemIndex(cosmeticItemWebSo);
			defaultWebThreadList = Singleton<WebController>.Instance.CreateDefaultWeb(this, webColor, webThickness, webThread, itemIndex);
			LineRenderer[] array = webLineRenderers;
			for (int i = 0; i < array.Length; i++)
			{
				Object.Destroy(array[i]);
			}
			SplineComputer[] array2 = splines;
			for (int i = 0; i < array2.Length; i++)
			{
				Object.Destroy(array2[i]);
			}
			isInitialized = true;
		}
	}

	public void Initialize(DefaultWebSaveData saveData)
	{
		isInitialized = saveData.isInitialized;
		if (defaultWebThreadList == null || !isInitialized)
		{
			return;
		}
		foreach (WebThread defaultWebThread in defaultWebThreadList)
		{
			Singleton<WebController>.Instance.DestroyWebThread(defaultWebThread);
		}
	}

	public DefaultWebSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Default Web " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		DefaultWebSaveData result = default(DefaultWebSaveData);
		result.id = id;
		result.name = base.gameObject.name;
		result.isInitialized = isInitialized;
		return result;
	}
}
