using System;
using UnityEngine;
using UnityEngine.Events;

namespace PhotoMode;

[Serializable]
public class PhotoModeAxisEvent : UnityEvent<Vector2>
{
}
