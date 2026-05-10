using System;
using UnityEngine;
using _Scripts.General;
using _Scripts.LevelSaving;
using _Scripts.Singletons;
using _Scripts.Wardrobe;

namespace _Scripts.Web;

public class WebThread : MonoBehaviour
{
	public class OnWebUpdatedEventArgs : EventArgs
	{
		public float length;
	}

	[Header("References")]
	[SerializeField]
	private CapsuleCollider capsuleCollider;

	[SerializeField]
	private WebThreadVisuals webThreadVisuals;

	[SerializeField]
	private DestroyedWebThreadVisuals destroyedWebThreadPrefab;

	[SerializeField]
	private UniqueID uniqueID;

	public WebJoint[] WebJoints;

	private Color webColor;

	private WebThread webThreadPrefab;

	private float webThickness;

	private WebSo webSo;

	private bool skipUpdate;

	private int webIndex;

	private float length;

	public float Radius => capsuleCollider.radius;

	public Color WebColor => webColor;

	public WebThread WebThreadPrefab => webThreadPrefab;

	public float WebThickness => webThickness;

	public Material WebMaterial => webThreadVisuals.WebMaterial;

	public int WebIndex => webIndex;

	public float Length => length;

	public event EventHandler<OnWebUpdatedEventArgs> OnWebUpdated;

	public WebSo GetWebSo()
	{
		return webSo;
	}

	public void SetWebSo(WebSo newWebSo)
	{
		webSo = newWebSo;
	}

	private void FixedUpdate()
	{
		if (!skipUpdate)
		{
			UpdateWebThread();
		}
	}

	private void UpdateWebThread(bool alwaysExecute = false)
	{
		if (WebJoints[0] == null || WebJoints[1] == null)
		{
			DestroySafely();
		}
		else if (alwaysExecute || !WebJoints[0].Rb.IsSleeping() || !WebJoints[1].Rb.IsSleeping())
		{
			skipUpdate = WebJoints[0].IsKinematic && WebJoints[1].IsKinematic;
			Vector3 position = WebJoints[0].Anchor.position;
			Vector3 position2 = WebJoints[1].Anchor.position;
			Vector3 position3 = (position + position2) / 2f;
			base.transform.position = position3;
			Vector3 forward = position2 - position;
			base.transform.rotation = Quaternion.LookRotation(forward);
			length = forward.magnitude;
			capsuleCollider.height = length;
			this.OnWebUpdated?.Invoke(this, new OnWebUpdatedEventArgs
			{
				length = length
			});
		}
	}

	private void DestroyWebDelayed()
	{
		capsuleCollider.gameObject.layer = 0;
		Singleton<GameController>.Instance.Player.ResetAllLegs();
		DestroySafely();
	}

	private void DestroySafely()
	{
		if (!(this == null))
		{
			DontDestroyMe[] componentsInChildren = base.gameObject.GetComponentsInChildren<DontDestroyMe>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].ResetParent();
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void SetColor(Color newWebColor, WebSo webSo = null)
	{
		webColor = newWebColor;
		webThreadVisuals.SetColor(webColor);
	}

	public void SetupWebThread(WebJoint webJoint1, WebJoint webJoint2, WebThread newWebThreadPrefab, int newWebIndex, Color newWebColor, float webColliderRadius, float webThickness, bool playAnimation = true)
	{
		webThreadPrefab = newWebThreadPrefab;
		this.webThickness = webThickness;
		WebJoints = new WebJoint[2] { webJoint1, webJoint2 };
		webJoint1.AddAttachedWeb(this);
		webJoint2.AddAttachedWeb(this);
		base.transform.gameObject.SetActive(value: true);
		UpdateWebThread(alwaysExecute: true);
		webSo = newWebThreadPrefab.GetWebSo();
		webIndex = newWebIndex;
		SetColor(newWebColor);
		capsuleCollider.radius = webColliderRadius;
		webThreadVisuals.SetWebJoints(WebJoints);
		webThreadVisuals.StartWeb(playAnimation, webThickness, webSo);
		uniqueID.GenerateNewID();
	}

	public void DeleteWebThread(bool destroyImmediate, bool useWebTargetPosition = false, bool playAnimation = false, bool removeConnections = true)
	{
		if (useWebTargetPosition)
		{
			webThreadVisuals.StopWeb(Singleton<WebController>.Instance.WebTarget.position);
		}
		else
		{
			webThreadVisuals.StopWeb(null);
		}
		if (playAnimation)
		{
			for (int i = 0; i < 2; i++)
			{
				if (useWebTargetPosition)
				{
					DestroyedWebThreadVisuals destroyedWebThreadVisuals = UnityEngine.Object.Instantiate(destroyedWebThreadPrefab);
					destroyedWebThreadVisuals.StartWeb(WebJoints[i].Anchor, Singleton<WebController>.Instance.WebTarget.position);
					destroyedWebThreadVisuals.SetColor(webColor, webSo);
				}
				else if (!(WebJoints[i] == null) && !(WebJoints[i].Rb == null) && (WebJoints[i].connectedWebJoints.Count > 1 || WebJoints[i].Rb.isKinematic || WebJoints[i].HasFixedJoint))
				{
					webThreadVisuals.StopWeb(WebJoints[i].transform.position);
					DestroyedWebThreadVisuals destroyedWebThreadVisuals2 = UnityEngine.Object.Instantiate(destroyedWebThreadPrefab);
					destroyedWebThreadVisuals2.StartWeb(WebJoints[i].Anchor, WebJoints[(i + 1) % 2].transform.position);
					destroyedWebThreadVisuals2.SetColor(webColor, webSo);
				}
			}
		}
		WebJoints[0].RemoveSpringJoint(WebJoints[1]);
		WebJoints[1].RemoveSpringJoint(WebJoints[0]);
		if (removeConnections)
		{
			WebJoints[0].RemoveConnectedWebJoint(WebJoints[1]);
			WebJoints[1].RemoveConnectedWebJoint(WebJoints[0]);
		}
		WebJoints[0].RemoveAttachedWeb(this);
		WebJoints[1].RemoveAttachedWeb(this);
		if (destroyImmediate)
		{
			DestroySafely();
		}
		else
		{
			Invoke("DestroyWebDelayed", 0.1f);
		}
	}

	public void SetLayer(int layer)
	{
		base.gameObject.layer = layer;
		capsuleCollider.gameObject.layer = layer;
	}

	public int GetLayer()
	{
		return base.gameObject.layer;
	}
}
