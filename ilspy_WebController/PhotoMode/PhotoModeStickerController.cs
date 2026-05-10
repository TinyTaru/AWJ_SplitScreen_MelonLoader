using UnityEngine;
using UnityEngine.UI;

namespace PhotoMode;

public class PhotoModeStickerController : MonoBehaviour
{
	private bool stickerModeOn;

	[Header("Info Bar References")]
	[SerializeField]
	private InfoBarController infoBarController;

	[Header("Sticker References")]
	[SerializeField]
	private Transform stickerOverlay;

	[SerializeField]
	private CanvasGroup stickerCanvas;

	[SerializeField]
	private CanvasGroup stickerCursor;

	[SerializeField]
	private Transform stickerPreview;

	[SerializeField]
	private Image prevSticker;

	[SerializeField]
	private Image currentSticker;

	[SerializeField]
	private Image nextSticker;

	[SerializeField]
	private Button stickerActivateButton;

	[Header("Sticker Settings")]
	[SerializeField]
	private float stickerCursorSpeed;

	[SerializeField]
	private float stickerRotateSpeed;

	[SerializeField]
	private float stickerScaleSpeed;

	[SerializeField]
	private Sprite[] stickerSprites;

	private RectTransform stickerCursorRect;

	private RectTransform stickerPreviewRect;

	private int stickerAmountCount;

	private int stickerSpriteCount;

	private RectTransform[] stickerPool;

	private Vector3 originalStickerScale;

	private Vector2 originalCursorSize;

	private Vector2 originalPreviewSize;

	private void Awake()
	{
		stickerActivateButton.onClick.AddListener(delegate
		{
			ToggleStickerMode(status: true);
		});
		stickerCursorRect = stickerCursor.GetComponent<RectTransform>();
		stickerPreviewRect = stickerPreview.GetComponent<RectTransform>();
		stickerPool = stickerOverlay.GetComponentsInChildren<RectTransform>();
		originalStickerScale = stickerPreview.localScale;
		originalCursorSize = stickerCursorRect.sizeDelta;
		originalPreviewSize = stickerPreviewRect.sizeDelta;
	}

	public bool IsActive()
	{
		return stickerModeOn;
	}

	public void ToggleStickerMode(bool status)
	{
		stickerModeOn = status;
		infoBarController.StickerModeActivation(status);
		UpdateStickerGallery();
		stickerCanvas.alpha = (status ? 1 : 0);
	}

	public void StampSticker()
	{
		if (stickerModeOn)
		{
			stickerPool[stickerAmountCount].position = stickerPreviewRect.position;
			stickerPool[stickerAmountCount].rotation = stickerPreviewRect.rotation;
			stickerPool[stickerAmountCount].sizeDelta = stickerPreviewRect.sizeDelta;
			stickerPool[stickerAmountCount].localScale = stickerPreviewRect.localScale;
			stickerPool[stickerAmountCount].GetComponent<Image>().sprite = stickerPreview.GetComponent<Image>().sprite;
			stickerPool[stickerAmountCount].GetComponent<Image>().color = Color.white;
			stickerAmountCount++;
			if (stickerAmountCount > stickerPool.Length - 1)
			{
				stickerAmountCount = 0;
			}
		}
	}

	public void MoveStickers(Vector2 axis)
	{
		if (stickerModeOn)
		{
			stickerCursor.transform.position += (Vector3)axis * Time.unscaledDeltaTime * stickerCursorSpeed;
		}
	}

	public void RotateStickers(float dir)
	{
		if (stickerModeOn)
		{
			stickerCursor.transform.eulerAngles += new Vector3(0f, 0f, 0f - dir) * Time.unscaledDeltaTime * stickerRotateSpeed;
		}
	}

	public void ScaleStickers(float dir)
	{
		if (stickerModeOn && (dir != 1f || !(stickerPreviewRect.sizeDelta.x >= 250f)) && (dir != -1f || !(stickerPreviewRect.sizeDelta.x <= 50f)))
		{
			stickerCursorRect.sizeDelta += new Vector2(dir, dir) * Time.unscaledDeltaTime * stickerScaleSpeed;
			stickerPreviewRect.sizeDelta += new Vector2(dir, dir) * Time.unscaledDeltaTime * stickerScaleSpeed;
		}
	}

	public void ChangeStickerSprite(int input)
	{
		if (!stickerModeOn)
		{
			return;
		}
		if (input == 1)
		{
			stickerSpriteCount++;
			if (stickerSpriteCount > stickerSprites.Length - 1)
			{
				stickerSpriteCount = 0;
			}
		}
		if (input == -1)
		{
			stickerSpriteCount--;
			if (stickerSpriteCount < 0)
			{
				stickerSpriteCount = stickerSprites.Length - 1;
			}
		}
		stickerPreview.GetComponent<Image>().sprite = stickerSprites[stickerSpriteCount];
		UpdateStickerGallery();
	}

	public void UpdateStickerGallery()
	{
		stickerPreview.GetComponent<Image>().sprite = stickerSprites[(int)Mathf.Repeat(stickerSpriteCount, stickerSprites.Length)];
		prevSticker.sprite = stickerSprites[(int)Mathf.Repeat(stickerSpriteCount - 1, stickerSprites.Length)];
		currentSticker.sprite = stickerSprites[(int)Mathf.Repeat(stickerSpriteCount, stickerSprites.Length)];
		nextSticker.sprite = stickerSprites[(int)Mathf.Repeat(stickerSpriteCount + 1, stickerSprites.Length)];
	}

	public void DeleteSticker()
	{
		if (!stickerModeOn)
		{
			return;
		}
		int num = stickerAmountCount - 1;
		if (num < 0)
		{
			num = stickerPool.Length - 1;
		}
		if (!(stickerPool[num].GetComponent<Image>().color == Color.clear))
		{
			stickerAmountCount--;
			if (stickerAmountCount < 0)
			{
				stickerAmountCount = stickerPool.Length - 1;
			}
			stickerPool[stickerAmountCount].GetComponent<Image>().color = Color.clear;
		}
	}

	public void FlipSticker(bool reset)
	{
		if (stickerModeOn)
		{
			if (!reset)
			{
				stickerPreviewRect.localScale = new Vector3(stickerPreviewRect.localScale.x * -1f, originalStickerScale.y, originalStickerScale.z);
			}
			else
			{
				stickerPreview.localScale = originalStickerScale;
			}
		}
	}

	public void ResetStickers()
	{
		FlipSticker(reset: true);
		RectTransform[] array = stickerPool;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].GetComponent<Image>().color = Color.clear;
		}
		stickerSpriteCount = 0;
		stickerCursorRect.anchoredPosition = Vector3.zero;
		stickerCursor.transform.eulerAngles = Vector3.zero;
		stickerCursorRect.sizeDelta = originalCursorSize;
		stickerPreviewRect.sizeDelta = originalPreviewSize;
	}
}
