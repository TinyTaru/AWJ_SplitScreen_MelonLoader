using UnityEngine;
using _Scripts.General;

namespace _Scripts.Objects;

[DisallowMultipleComponent]
public class AutomaticSort : MonoBehaviour
{
	[SerializeField]
	private SortingCategory sortingCategory;

	[SerializeField]
	private FurnitureSubCategory furnitureSubCategory;

	[SerializeField]
	private ObjectsSubCategory objectSubCategory;

	private void Awake()
	{
		Object.Destroy(this);
	}

	public SortingCategory GetSortingCategory()
	{
		return sortingCategory;
	}

	public FurnitureSubCategory GetFurnitureSubCategory()
	{
		return furnitureSubCategory;
	}

	public ObjectsSubCategory GetObjectsSubCategory()
	{
		return objectSubCategory;
	}
}
