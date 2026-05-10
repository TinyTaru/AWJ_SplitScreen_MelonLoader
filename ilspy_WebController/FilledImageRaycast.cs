using UnityEngine;
using UnityEngine.UI;

public class FilledImageRaycast : Image
{
	public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
	{
		if (base.sprite == null)
		{
			return false;
		}
		RectTransformUtility.ScreenPointToLocalPointInRectangle(base.rectTransform, screenPoint, eventCamera, out var localPoint);
		Rect rect = base.rectTransform.rect;
		Vector2 vector = new Vector2(Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x), Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y));
		Vector2Int vector2Int = new Vector2Int(Mathf.RoundToInt(vector.x * (float)base.sprite.texture.width), Mathf.RoundToInt(vector.y * (float)base.sprite.texture.height));
		Color pixel = base.sprite.texture.GetPixel(vector2Int.x, vector2Int.y);
		base.alphaHitTestMinimumThreshold = 0.1f;
		if (pixel.a < base.alphaHitTestMinimumThreshold)
		{
			return false;
		}
		if (base.type == Type.Filled)
		{
			if (base.fillMethod == FillMethod.Radial360)
			{
				Vector2 vector2 = new Vector2(0.5f, 0.5f);
				Vector2 vector3 = vector - vector2;
				float num = Mathf.Atan2(vector3.y, vector3.x) * 57.29578f;
				if (num < 0f)
				{
					num += 360f;
				}
				float num2 = 360f * base.fillAmount;
				if (num > num2)
				{
					return false;
				}
			}
			else if (base.fillMethod == FillMethod.Horizontal || base.fillMethod == FillMethod.Vertical)
			{
				if (base.fillMethod == FillMethod.Horizontal && vector.x > base.fillAmount)
				{
					return false;
				}
				if (base.fillMethod == FillMethod.Vertical && vector.y > base.fillAmount)
				{
					return false;
				}
			}
		}
		return true;
	}
}
