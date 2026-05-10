using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.GeneratedBindings;
using Unity.Services.CloudCode.GeneratedBindings.AWebbingJourneyCloud;
using Unity.Services.Economy;
using Unity.Services.Economy.Model;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.MiniJSON;
using UnityEngine.Purchasing.Security;
using _Scripts.Singletons;

namespace _Scripts.UnityGamingServices;

public class IAPStoreManager : Singleton<IAPStoreManager>
{
	private StoreServiceBindings storeServiceBindings;

	private StoreController storeController;

	private CrossPlatformValidator googleValidator;

	private bool isPurchaseInProgress;

	private bool isInitialized;

	private bool productsAreFetched;

	private List<Product> products;

	private List<ProductDefinition> productDefinitions;

	private Orders orders;

	private readonly HashSet<string> processingTransactions = new HashSet<string>();

	private TaskCompletionSource<bool> purchasesFetchTcs;

	private readonly object purchasesFetchLock = new object();

	private bool initializationFailed;

	public List<Product> Products => products;

	public bool IsInitialized => isInitialized;

	public bool ProductsAreFetched => productsAreFetched;

	public bool InitializationFailed => initializationFailed;

	public bool IsPurchaseInProgress => isPurchaseInProgress;

	public event Action Initialized;

	public event Action ProductsFetched;

	public event Action<string> SuccessfullyPurchased;

	public event Action<string> PurchaseFailed;

	public event Action OnPurchasesRestored;

	private void Start()
	{
		Singleton<PlayerEconomyManager>.Instance.EconomyConfigSynced += PlayerEconomyManager_OnEconomyConfigSynced;
	}

	private void OnDestroy()
	{
		Singleton<PlayerEconomyManager>.Instance.EconomyConfigSynced -= PlayerEconomyManager_OnEconomyConfigSynced;
		UnsubscribeIAPEvents();
	}

	private async void InitializeIAPAsync()
	{
		try
		{
			storeServiceBindings = new StoreServiceBindings(CloudCodeService.Instance);
			storeController = UnityIAPServices.StoreController();
			SubscribeIAPEvents();
			await storeController.Connect();
			Debug.Log("[IAP] Connected to store.");
			BuildProductDefinitionsFromEconomy();
			if (productDefinitions.Count == 0)
			{
				Debug.LogWarning("[IAP] No real-money products found in Economy config.");
				return;
			}
			storeController.FetchProducts(productDefinitions);
			InitializeReceiptValidatorsIfNeeded();
			isInitialized = true;
			this.Initialized?.Invoke();
		}
		catch (Exception)
		{
			Debug.LogError("[IAP] Connection failed");
		}
	}

	private void BuildProductDefinitionsFromEconomy()
	{
		List<RealMoneyPurchaseDefinition> realMoneyPurchases = EconomyService.Instance.Configuration.GetRealMoneyPurchases();
		LogRealPurchasesFromConfig(realMoneyPurchases);
		productDefinitions = new List<ProductDefinition>();
		foreach (RealMoneyPurchaseDefinition item2 in realMoneyPurchases)
		{
			string id = item2.Id;
			string id2 = item2.Id;
			ProductType type = (id.Contains("UNLOCK") ? ProductType.NonConsumable : ProductType.Consumable);
			ProductDefinition item = new ProductDefinition(id, id2, type);
			productDefinitions.Add(item);
		}
		Debug.Log($"[IAP] Prepared {productDefinitions.Count} ProductDefinitions for fetch.");
	}

	private void LogRealPurchasesFromConfig(List<RealMoneyPurchaseDefinition> realMoneyPurchaseDefinitions)
	{
	}

	private void BuildAndFetchProductsWithCatalog()
	{
		ProductCatalog productCatalog = ProductCatalog.LoadDefaultCatalog();
		if (productCatalog == null || productCatalog.allProducts == null || productCatalog.allProducts.Count == 0)
		{
			Debug.LogWarning("[IAP] No products in IAPProductCatalog.json.");
		}
		List<ProductDefinition> list = new List<ProductDefinition>();
		foreach (ProductCatalogItem allProduct in productCatalog.allProducts)
		{
			list.Add(new ProductDefinition(allProduct.id, allProduct.type));
		}
		storeController.FetchProducts(list);
	}

	private void InitializeReceiptValidatorsIfNeeded()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			try
			{
				googleValidator = new CrossPlatformValidator(GooglePlayTangle.Data(), Application.identifier);
				Debug.Log("[IAP] Google receipt validator initialized.");
			}
			catch (Exception ex)
			{
				Debug.LogWarning("[IAP] Validator init skipped/failed: " + ex.Message);
			}
		}
	}

	private void SubscribeIAPEvents()
	{
		if (storeController != null)
		{
			storeController.OnProductsFetched += StoreController_OnProductsFetched;
			storeController.OnProductsFetchFailed += StoreController_OnProductsFetchFailed;
			storeController.OnPurchasesFetched += StoreController_OnPurchasesFetched;
			storeController.OnPurchasesFetchFailed += StoreController_OnPurchasesFetchFailed;
			storeController.OnPurchasePending += StoreController_OnPurchasePending;
			storeController.OnPurchaseConfirmed += StoreController_OnPurchaseConfirmed;
			storeController.OnPurchaseFailed += StoreController_OnPurchaseFailed;
			storeController.OnStoreDisconnected += StoreController_OnStoreDisconnected;
		}
	}

	private void UnsubscribeIAPEvents()
	{
		if (storeController != null)
		{
			storeController.OnProductsFetched -= StoreController_OnProductsFetched;
			storeController.OnProductsFetchFailed -= StoreController_OnProductsFetchFailed;
			storeController.OnPurchasesFetched -= StoreController_OnPurchasesFetched;
			storeController.OnPurchasesFetchFailed -= StoreController_OnPurchasesFetchFailed;
			storeController.OnPurchasePending -= StoreController_OnPurchasePending;
			storeController.OnPurchaseConfirmed -= StoreController_OnPurchaseConfirmed;
			storeController.OnPurchaseFailed -= StoreController_OnPurchaseFailed;
			storeController.OnStoreDisconnected -= StoreController_OnStoreDisconnected;
		}
	}

	private bool ValidateIfGoogle(string receipt)
	{
		if (googleValidator == null)
		{
			return true;
		}
		try
		{
			IPurchaseReceipt[] array = googleValidator.Validate(receipt);
			for (int i = 0; i < array.Length; i++)
			{
				_ = array[i];
			}
			return true;
		}
		catch (IAPSecurityException)
		{
			Debug.LogError("[IAP] Receipt invalid");
			return false;
		}
	}

	private void LogProductsFetched()
	{
		Debug.Log($"[IAP] Products fetched: {products.Count}");
		foreach (Product product in products)
		{
			_ = product;
		}
	}

	private RestoreRequest BuildRestoreRequest()
	{
		Debug.Log($"BuildRestoreRequest for {orders.ConfirmedOrders.Count} orders");
		List<ProductDefinitionDto> list = new List<ProductDefinitionDto>();
		foreach (ProductDefinition productDefinition in productDefinitions)
		{
			ProductDefinitionDto item = new ProductDefinitionDto
			{
				EconomyItemId = productDefinition.id,
				StoreSpecificId = productDefinition.storeSpecificId
			};
			list.Add(item);
		}
		List<PurchaseDto> list2 = new List<PurchaseDto>();
		foreach (ConfirmedOrder confirmedOrder in orders.ConfirmedOrders)
		{
			if (!TryGetPurchaseToken(confirmedOrder, out var purchaseDto))
			{
				continue;
			}
			bool flag = false;
			foreach (ProductDefinition productDefinition2 in productDefinitions)
			{
				if (productDefinition2.storeSpecificId == purchaseDto.ProductId && productDefinition2.type == ProductType.NonConsumable)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				list2.Add(purchaseDto);
			}
		}
		return new RestoreRequest
		{
			PackageName = Application.identifier,
			ProductDefinitions = list,
			Purchases = list2
		};
	}

	private bool TryGetPurchaseToken(ConfirmedOrder confirmedOrder, out PurchaseDto purchaseDto)
	{
		Debug.Log("TryGetPurchaseToken");
		purchaseDto = new PurchaseDto();
		if (confirmedOrder == null || string.IsNullOrEmpty(confirmedOrder.Info.Receipt))
		{
			return false;
		}
		if (!(Json.Deserialize(confirmedOrder.Info.Receipt) is Dictionary<string, object> dictionary))
		{
			return false;
		}
		if (!dictionary.TryGetValue("Payload", out var value) || !(value is string json))
		{
			return false;
		}
		if (!(Json.Deserialize(json) is Dictionary<string, object> dictionary2))
		{
			return false;
		}
		string empty = string.Empty;
		if (dictionary2.TryGetValue("json", out var value2) && value2 is string text)
		{
			empty = text;
		}
		else
		{
			if (!dictionary2.TryGetValue("Json", out var value3) || !(value3 is string text2))
			{
				return false;
			}
			empty = text2;
		}
		if (!(Json.Deserialize(empty) is Dictionary<string, object> dictionary3))
		{
			return false;
		}
		if (!dictionary3.TryGetValue("productId", out var value4) || !(value4 is string productId))
		{
			return false;
		}
		purchaseDto.ProductId = productId;
		if (!dictionary3.TryGetValue("purchaseToken", out value4) || !(value4 is string purchaseToken))
		{
			return false;
		}
		purchaseDto.PurchaseToken = purchaseToken;
		if (!string.IsNullOrEmpty(purchaseDto.ProductId))
		{
			return !string.IsNullOrEmpty(purchaseDto.PurchaseToken);
		}
		return false;
	}

	private void PerformPurchase(string productId)
	{
		if (isPurchaseInProgress)
		{
			Debug.LogWarning("[IAP] Purchase already in progress.");
			return;
		}
		Debug.Log("Perform Purchase: " + productId);
		isPurchaseInProgress = true;
		storeController.PurchaseProduct(productId);
	}

	public void UnlockAllLevels()
	{
		PerformPurchase("LEVEL_UNLOCK_ALL");
	}

	public void UnlockOfficeLevel()
	{
		PerformPurchase("LEVEL_UNLOCK_OFFICE");
	}

	public void UnlockKidsRoomLevel()
	{
		PerformPurchase("LEVEL_UNLOCK_KIDS_ROOM");
	}

	public void BuyCoffee(int amount = 1)
	{
		switch (amount)
		{
		default:
			PerformPurchase("COFFEE_1");
			break;
		case 2:
			PerformPurchase("COFFEE_2");
			break;
		case 3:
			PerformPurchase("COFFEE_3");
			break;
		}
	}

	public async Task RestorePurchases()
	{
	}

	private void PlayerEconomyManager_OnEconomyConfigSynced()
	{
		InitializeIAPAsync();
	}

	private void StoreController_OnProductsFetched(List<Product> fetchedProducts)
	{
		products = fetchedProducts;
		storeController.FetchPurchases();
		LogProductsFetched();
		productsAreFetched = true;
		this.ProductsFetched?.Invoke();
	}

	private void StoreController_OnProductsFetchFailed(ProductFetchFailed productFetchFailed)
	{
		Debug.LogError("[IAP] Product fetch failed");
		TaskCompletionSource<bool> taskCompletionSource = null;
		lock (purchasesFetchLock)
		{
			taskCompletionSource = purchasesFetchTcs;
			purchasesFetchTcs = null;
		}
		taskCompletionSource?.TrySetResult(result: false);
	}

	private async void StoreController_OnPurchasesFetched(Orders fetchedOrders)
	{
		orders = fetchedOrders;
		TaskCompletionSource<bool> taskCompletionSource = null;
		lock (purchasesFetchLock)
		{
			taskCompletionSource = purchasesFetchTcs;
			purchasesFetchTcs = null;
		}
		taskCompletionSource?.TrySetResult(result: true);
		try
		{
			Debug.Log("StoreController_OnPurchasesFetched");
			Debug.Log($"Confirmed orders: {orders.ConfirmedOrders.Count}");
			Debug.Log($"Pending orders: {orders.PendingOrders.Count}");
			Debug.Log($"Deferred orders: {orders.DeferredOrders.Count}");
			if (Singleton<PlayerEconomyManager>.Instance.EconomyDataIsInitialized)
			{
				await RestorePurchases();
			}
			else
			{
				Singleton<PlayerEconomyManager>.Instance.EconomyDataInitialized += PlayerEconomyManager_OnEconomyDataInitialized;
			}
		}
		catch (Exception)
		{
			Debug.LogError("Restoring purchases failed");
		}
	}

	private async void PlayerEconomyManager_OnEconomyDataInitialized()
	{
		try
		{
			await RestorePurchases();
		}
		catch (Exception)
		{
			Debug.LogError("Restoring purchases failed");
		}
	}

	private void StoreController_OnPurchasesFetchFailed(PurchasesFetchFailureDescription purchasesFetchFailureDescription)
	{
		Debug.LogError("[IAP] Purchases fetch failed");
	}

	private async void StoreController_OnPurchasePending(PendingOrder pendingOrder)
	{
		string transactionId = pendingOrder.Info.TransactionID;
		if (!string.IsNullOrEmpty(transactionId))
		{
			if (processingTransactions.Contains(transactionId))
			{
				Debug.LogWarning("[IAP] Duplicate OnPurchasePending ignored. Tx=" + transactionId);
				return;
			}
			processingTransactions.Add(transactionId);
		}
		try
		{
			string text = pendingOrder.CartOrdered.Items().FirstOrDefault()?.Product?.definition?.id;
			if (string.IsNullOrEmpty(text))
			{
				isPurchaseInProgress = false;
				Debug.LogError("[IAP] Pending order has no product id.");
				this.PurchaseFailed?.Invoke("No product id in pending order.");
				return;
			}
			Product product = storeController?.GetProductById(text);
			if (product == null)
			{
				isPurchaseInProgress = false;
				Debug.LogError("[IAP] Product not found in controller: " + text);
				this.PurchaseFailed?.Invoke("Product not found in controller: " + text);
				return;
			}
			Debug.Log("[IAP] Pending purchase");
			string receipt = pendingOrder.Info.Receipt;
			if (!ValidateIfGoogle(receipt))
			{
				isPurchaseInProgress = false;
				Debug.LogError("[IAP] Google receipt validation failed.");
				this.PurchaseFailed?.Invoke("Invalid receipt for " + product.definition.id);
				return;
			}
			PlayerEconomyData playerEconomyData = await storeServiceBindings.ProcessRealMoneyPurchase(product.definition.id, receipt, (double)product.metadata.localizedPrice, product.metadata.isoCurrencyCode);
			if (playerEconomyData == null)
			{
				isPurchaseInProgress = false;
				Debug.LogError("[IAP] Cloud Code returned null economy data.");
				this.PurchaseFailed?.Invoke("Server processing failed for " + product.definition.id);
			}
			else
			{
				Singleton<PlayerEconomyManager>.Instance.HandleEconomyUpdate(playerEconomyData);
				storeController.ConfirmPurchase(pendingOrder);
				Debug.Log("[IAP] Confirmed purchase");
			}
		}
		catch (Exception ex)
		{
			isPurchaseInProgress = false;
			Debug.LogError("[IAP] Error processing pending order");
			this.PurchaseFailed?.Invoke("Purchase failed: " + ex.Message);
		}
		finally
		{
			if (!string.IsNullOrEmpty(transactionId))
			{
				processingTransactions.Remove(transactionId);
			}
		}
	}

	private void StoreController_OnPurchaseConfirmed(Order order)
	{
		isPurchaseInProgress = false;
		if (order is FailedOrder failedOrder)
		{
			Debug.LogWarning($"[IAP] Confirmation failed: {failedOrder.FailureReason}");
			return;
		}
		Product product = order.CartOrdered.Items().FirstOrDefault()?.Product;
		Debug.Log("[IAP] Purchase confirmed");
		this.SuccessfullyPurchased?.Invoke("Purchase confirmed: " + product?.definition.id);
	}

	private void StoreController_OnPurchaseFailed(FailedOrder failedOrder)
	{
		isPurchaseInProgress = false;
		Debug.LogError("[IAP] Purchase failed");
		this.PurchaseFailed?.Invoke($"Purchase failed: {failedOrder.FailureReason}");
	}

	private void StoreController_OnStoreDisconnected(StoreConnectionFailureDescription storeConnectionFailureDescription)
	{
		Debug.LogError("[IAP] Store disconnected");
	}

	private async Task<bool> EnsurePurchasesFetchedAsync()
	{
		if (orders != null)
		{
			return true;
		}
		TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
		lock (purchasesFetchLock)
		{
			purchasesFetchTcs = taskCompletionSource;
		}
		storeController.FetchPurchases();
		return await taskCompletionSource.Task;
	}

	private AppleRestoreRequest BuildAppleRestoreRequest()
	{
		List<ProductDefinitionDto> list = new List<ProductDefinitionDto>();
		foreach (ProductDefinition productDefinition in productDefinitions)
		{
			ProductDefinitionDto item = new ProductDefinitionDto
			{
				EconomyItemId = productDefinition.id,
				StoreSpecificId = productDefinition.storeSpecificId
			};
			list.Add(item);
		}
		HashSet<string> hashSet = new HashSet<string>();
		List<ApplePurchaseJwsDto> list2 = new List<ApplePurchaseJwsDto>();
		foreach (ConfirmedOrder confirmedOrder in orders.ConfirmedOrders)
		{
			Product product = confirmedOrder.CartOrdered.Items().FirstOrDefault()?.Product;
			if (product != null && product.definition.type == ProductType.NonConsumable)
			{
				string text = confirmedOrder.Info?.Apple?.jwsRepresentation;
				if (!string.IsNullOrEmpty(text) && hashSet.Add(text))
				{
					list2.Add(new ApplePurchaseJwsDto
					{
						JwsRepresentation = text
					});
				}
			}
		}
		return new AppleRestoreRequest
		{
			ProductDefinitions = list,
			Purchases = list2
		};
	}
}
