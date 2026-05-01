using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.Camera;

public class FollowTarget : MonoBehaviour
{
	public enum FollowType
	{
		Null,
		Spider,
		Root,
		Dialogue
	}

	[SerializeField]
	private Vector3 defaultWorldOffset = Vector3.zero;

	[SerializeField]
	private Vector3 defaultSpiderOffset = new Vector3(0f, 3f, 0f);

	[SerializeField]
	private Vector3 defaultCameraOffset = Vector3.zero;

	[SerializeField]
	private Transform cameraController;

	[SerializeField]
	private FollowType followFollowType;

	[SerializeField]
	private FollowType lookAtFollowType;

	[SerializeField]
	private DialogueFocus dialogueTarget;

	[SerializeField]
	private float decay;

	private Transform target;

	private Transform lookAtTarget;

	private Vector3 overwriteOffset;

	private bool isOverwrite;

	private Vector3 worldOffset;

	private Vector3 spiderOffset;

	private Vector3 cameraOffset;

	private void Start()
	{
		SetFollowType(followFollowType);
		ResetCameraOffsets();
		switch (lookAtFollowType)
		{
		default:
			lookAtTarget = null;
			break;
		case FollowType.Spider:
			lookAtTarget = Singleton<GameController>.Instance.Player.transform;
			break;
		case FollowType.Root:
			lookAtTarget = Singleton<GameController>.Instance.Player.Root;
			break;
		}
	}

	private void FixedUpdate()
	{
		if (Singleton<GameController>.Instance.State != GameController.GameState.Cutscene)
		{
			Vector3 vector = target.right * spiderOffset.x + target.up * spiderOffset.y + target.forward * spiderOffset.z;
			Vector3 vector2 = cameraOffset.x * cameraController.right + cameraOffset.y * cameraController.up + cameraOffset.z * cameraController.forward;
			if (!isOverwrite)
			{
				_ = target.position + worldOffset + vector + vector2;
			}
			else
			{
				_ = target.position + overwriteOffset;
			}
			base.transform.position = target.position + target.up;
			if (lookAtTarget != null)
			{
				base.transform.LookAt(lookAtTarget, Vector3.up);
			}
			Vector3 shoulderOffset = base.transform.InverseTransformVector(target.up * spiderOffset.y);
			Singleton<CameraController>.Instance.SetShoulderOffset(shoulderOffset);
		}
	}

	private void UpdateOffsets()
	{
		worldOffset = defaultWorldOffset;
		spiderOffset = defaultSpiderOffset;
		cameraOffset = defaultCameraOffset;
	}

	public void ResetCameraOffsets()
	{
		isOverwrite = false;
		worldOffset = defaultCameraOffset;
		spiderOffset = defaultSpiderOffset;
		cameraOffset = defaultCameraOffset;
	}

	public void ChangeCameraOffset(Vector3 newOffset)
	{
		isOverwrite = true;
		overwriteOffset = newOffset;
	}

	public void SetFollowType(FollowType newFollowType)
	{
		followFollowType = newFollowType;
		switch (followFollowType)
		{
		default:
			target = null;
			break;
		case FollowType.Spider:
			target = Singleton<GameController>.Instance.Player.transform;
			break;
		case FollowType.Root:
			target = Singleton<GameController>.Instance.Player.Root;
			break;
		case FollowType.Dialogue:
			dialogueTarget.UpdateDialogueConversant();
			target = dialogueTarget.transform;
			break;
		}
	}

	public void SetWorldOffset(float x, float y, float z)
	{
		worldOffset = new Vector3(x, y, z);
	}

	public void SetSpiderOffset(float x, float y, float z)
	{
		spiderOffset = new Vector3(x, y, z);
	}

	public void SetCameraOffset(float x, float y, float z)
	{
		cameraOffset = new Vector3(x, y, z);
	}
}
