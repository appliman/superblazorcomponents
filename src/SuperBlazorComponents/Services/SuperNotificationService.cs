using SuperBlazorComponents.Components.Notifications;

namespace SuperBlazorComponents.Services;

public class SuperNotificationService
{
	private readonly List<NotificationMessage> _notifications = new();

	/// <summary>
	/// Détermine si le détail des notifications est rendu en HTML par défaut.
	/// Valeur par défaut : <c>true</c> (rendu HTML). Définir à <c>false</c> pour un rendu en texte brut.
	/// </summary>
	public bool DefaultIsHtml { get; set; } = true;

	/// <summary>
	/// Événement déclenché lorsqu'une notification doit être affichée.
	/// </summary>
	public event Func<Task>? OnChange;

	/// <summary>
	/// Liste des notifications actives.
	/// </summary>
	public IReadOnlyList<NotificationMessage> Notifications => _notifications;

	/// <summary>
	/// Affiche une notification toast.
	/// </summary>
	/// <param name="notificationSeverity">Le niveau de sévérité de la notification.</param>
	/// <param name="summary">Le titre de la notification.</param>
	/// <param name="detail">Le message détaillé de la notification.</param>
	/// <param name="duration">La durée d'affichage en millisecondes (0 pour ne pas fermer automatiquement).</param>
	/// <param name="click">Action à exécuter lors du clic sur la notification.</param>
	/// <param name="closeOnClick">Indique si la notification se ferme au clic.</param>
	/// <param name="payload">Données personnalisées associées à la notification.</param>
	/// <param name="close">Action à exécuter lors de la fermeture de la notification.</param>
	/// <param name="isHtml">Indique si le détail est rendu en HTML. Hérite de <see cref="DefaultIsHtml"/> si non spécifié.</param>
	public async Task Notify(
		NotificationSeverity notificationSeverity,
		string summary = "",
		string detail = "",
		double duration = 3000,
		Action<NotificationMessage>? click = null,
		bool closeOnClick = false,
		object? payload = null,
		Action<NotificationMessage>? close = null,
		bool? isHtml = null)
	{
		var notification = new NotificationMessage
		{
			CreatedAt = DateTimeOffset.UtcNow,
			Severity = notificationSeverity,
			Summary = summary,
			Detail = detail,
			Duration = duration,
			Click = click,
			CloseOnClick = closeOnClick,
			Payload = payload,
			Close = close,
			IsHtml = isHtml ?? DefaultIsHtml
		};

		_notifications.Add(notification);

		if (OnChange != null)
		{
			await OnChange.Invoke();
		}

		if (duration > 0)
		{
			_ = RemoveAfterDelayAsync(notification, duration);
		}
	}

	/// <summary>
	/// Supprime une notification de la liste.
	/// </summary>
	/// <param name="notification">La notification à supprimer.</param>
	public async Task Remove(NotificationMessage notification)
	{
		notification.Close?.Invoke(notification);
		_notifications.Remove(notification);

		if (OnChange != null)
		{
			await OnChange.Invoke();
		}
	}

	/// <summary>
	/// Supprime une notification après un délai.
	/// </summary>
	private async Task RemoveAfterDelayAsync(NotificationMessage notification, double duration)
	{
		await Task.Delay(TimeSpan.FromMilliseconds(duration));

		if (_notifications.Contains(notification))
		{
			await Remove(notification);
		}
	}
}
