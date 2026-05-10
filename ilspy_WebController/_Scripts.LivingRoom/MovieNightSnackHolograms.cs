using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.Objects;

namespace _Scripts.LivingRoom;

public class MovieNightSnackHolograms : MonoBehaviour
{
	[SerializeField]
	private MagneticLock hologramChips;

	[SerializeField]
	private MagneticLock hologramChocolate;

	[SerializeField]
	private MagneticLock hologramPizza;

	[SerializeField]
	private GameObject chipsGfx;

	[SerializeField]
	private GameObject chocolateGfx;

	[SerializeField]
	private GameObject pizzaGfx;

	[SerializeField]
	private float graphicSwapPeriod = 2f;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onHologramsEnabledEvent;

	[SerializeField]
	private UnityEvent onChipsPlacedEvent;

	[SerializeField]
	private UnityEvent onChocolatePlacedEvent;

	[SerializeField]
	private UnityEvent onPizzaPlacedEvent;

	private Coroutine graphicSwapperCoroutine;

	private void Start()
	{
		DisableHolograms();
	}

	private IEnumerator GraphicSwapperCoroutine()
	{
		while (true)
		{
			pizzaGfx.SetActive(value: false);
			chipsGfx.SetActive(value: true);
			yield return new WaitForSeconds(graphicSwapPeriod);
			chipsGfx.SetActive(value: false);
			chocolateGfx.SetActive(value: true);
			yield return new WaitForSeconds(graphicSwapPeriod);
			chocolateGfx.SetActive(value: false);
			pizzaGfx.SetActive(value: true);
			yield return new WaitForSeconds(graphicSwapPeriod);
		}
	}

	private void DisableHolograms()
	{
		hologramChips.gameObject.SetActive(value: false);
		hologramChocolate.gameObject.SetActive(value: false);
		hologramPizza.gameObject.SetActive(value: false);
	}

	private void DisableMagneticLocks()
	{
		hologramChips.enabled = false;
		hologramChocolate.enabled = false;
		hologramPizza.enabled = false;
		pizzaGfx.SetActive(value: false);
		chipsGfx.SetActive(value: false);
		chocolateGfx.SetActive(value: false);
		StopCoroutine(graphicSwapperCoroutine);
	}

	public void EnableHolograms()
	{
		hologramChips.gameObject.SetActive(value: true);
		hologramChocolate.gameObject.SetActive(value: true);
		hologramPizza.gameObject.SetActive(value: true);
		graphicSwapperCoroutine = StartCoroutine(GraphicSwapperCoroutine());
		onHologramsEnabledEvent?.Invoke();
	}

	public void ChipsPlaced()
	{
		DisableMagneticLocks();
		onChipsPlacedEvent.Invoke();
	}

	public void ChocolatePlaced()
	{
		DisableMagneticLocks();
		onChocolatePlacedEvent.Invoke();
	}

	public void PizzaPlaced()
	{
		DisableMagneticLocks();
		onPizzaPlacedEvent.Invoke();
	}
}
