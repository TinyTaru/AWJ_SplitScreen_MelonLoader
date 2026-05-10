using System;

namespace _Scripts.UI.Notifications;

public interface IQueueableNotification
{
	event EventHandler OnPopUpCompleted;

	void ShowMessage();
}
