using UnityEngine;

namespace _Scripts.UI.Mobile_UI;

public class SafeArea : MonoBehaviour
{
	private RectTransform rectTransform;

	private Rect safeArea;

	private Vector2 minAnchor;

	private Vector2 maxAnchor;

	private int screenWidth;

	private int screenHeight;

	private ScreenOrientation oldScreenOrientation;

	private void Awake()
	{
		screenWidth = Screen.width;
		screenHeight = Screen.height;
		UpdateSafeArea();
	}

	private void Update()
	{
		ScreenOrientation orientation = Screen.orientation;
		if (orientation != oldScreenOrientation)
		{
			UpdateSafeArea();
		}
		oldScreenOrientation = orientation;
	}

	private void UpdateSafeArea()
	{
		rectTransform = GetComponent<RectTransform>();
		safeArea = Screen.safeArea;
		minAnchor = safeArea.position;
		maxAnchor = minAnchor + safeArea.size;
		minAnchor.x /= screenWidth;
		minAnchor.y /= screenHeight;
		maxAnchor.x /= screenWidth;
		maxAnchor.y /= screenHeight;
		rectTransform.anchorMin = minAnchor;
		rectTransform.anchorMax = maxAnchor;
	}
}
