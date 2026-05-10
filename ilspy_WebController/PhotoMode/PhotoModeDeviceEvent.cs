using System;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace PhotoMode;

[Serializable]
public class PhotoModeDeviceEvent : UnityEvent<InputDevice>
{
}
