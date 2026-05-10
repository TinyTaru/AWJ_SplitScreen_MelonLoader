using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.Layouts;

public class FlexibleGridLayout : LayoutGroup
{
	public enum FitType
	{
		Square,
		Width,
		Height,
		FixedRows,
		FixedColumns
	}

	public Vector2 spacing;

	[Header("Flexible Grid Layout")]
	public FitType fitType;

	public int rows;

	public int columns;

	private Vector2 cellSize;

	public bool fitX;

	public bool fitY;

	public override void CalculateLayoutInputVertical()
	{
		if (rows < 1)
		{
			rows = 1;
		}
		if (columns < 1)
		{
			columns = 1;
		}
		if (fitType == FitType.Width || fitType == FitType.Height || fitType == FitType.Square)
		{
			fitX = true;
			fitY = true;
			float f = Mathf.Sqrt(base.transform.childCount);
			rows = Mathf.CeilToInt(f);
			columns = Mathf.CeilToInt(f);
		}
		if (fitType == FitType.Width || fitType == FitType.FixedColumns)
		{
			rows = Mathf.CeilToInt((float)base.transform.childCount / (float)columns);
			if (rows == 0)
			{
				rows = 1;
			}
		}
		if (fitType == FitType.Height || fitType == FitType.FixedRows)
		{
			columns = Mathf.CeilToInt((float)base.transform.childCount / (float)rows);
			if (columns == 0)
			{
				columns = 1;
			}
		}
		float width = base.rectTransform.rect.width;
		float height = base.rectTransform.rect.height;
		float num = width / (float)columns - spacing.x / (float)columns * (float)(columns - 1) - (float)(base.padding.left / columns) - (float)(base.padding.right / columns);
		float num2 = height / (float)rows - spacing.y / (float)rows * (float)(rows - 1) - (float)(base.padding.top / rows) - (float)(base.padding.bottom / rows);
		cellSize.x = (fitX ? num : cellSize.x);
		cellSize.y = (fitY ? num2 : cellSize.y);
		for (int i = 0; i < base.rectChildren.Count; i++)
		{
			int num3 = i / columns;
			int num4 = i % columns;
			RectTransform rect = base.rectChildren[i];
			float pos = cellSize.x * (float)num4 + spacing.x * (float)num4 + (float)base.padding.left;
			float pos2 = cellSize.y * (float)num3 + spacing.y * (float)num3 + (float)base.padding.top;
			SetChildAlongAxis(rect, 0, pos, cellSize.x);
			SetChildAlongAxis(rect, 1, pos2, cellSize.y);
		}
	}

	public override void SetLayoutHorizontal()
	{
	}

	public override void SetLayoutVertical()
	{
	}

	public void UpdateFlexibleGridLayout()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.rectTransform);
	}
}
