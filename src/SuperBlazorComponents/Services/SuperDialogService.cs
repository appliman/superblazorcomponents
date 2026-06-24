using SuperBlazorComponents.Components.Dialogs;
using SuperBlazorComponents.Components;
using SuperBlazorComponents.Components.SuperDataGrid.Filters;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using SuperBlazorComponents.Components.SuperDateRange;

namespace SuperBlazorComponents.Services;

public class SuperDialogService
{
	private readonly IStringLocalizer _localizer;

	public SuperDialogService(IStringLocalizer localizer)
	{
		_localizer = localizer;
	}

	private TaskCompletionSource<bool>? _confirmTcs;
	private TaskCompletionSource<object?>? _dialogTcs;

	/// <summary>
	/// Événement déclenché lorsque la boîte de dialogue de confirmation doit être affichée.
	/// </summary>
	public event Func<string, string, ConfirmOptions, Task>? OnShow;

	/// <summary>
	/// Événement déclenché lorsqu'une modale dynamique doit être affichée.
	/// </summary>
	public event Func<Type, string, Dictionary<string, object>?, DialogOptions?, Task>? OnOpenDialog;

	/// <summary>
	/// Événement déclenché lorsque la modale dynamique doit être fermée.
	/// </summary>
	public event Func<Task>? OnCloseDialog;

	/// <summary>
	/// Affiche une boîte de dialogue de confirmation et attend la réponse de l'utilisateur.
	/// </summary>
	/// <param name="title">Le titre de la boîte de dialogue.</param>
	/// <param name="message">Le message à afficher.</param>
	/// <param name="confirmOptions">Les options de configuration des boutons.</param>
	/// <returns>True si l'utilisateur confirme, false sinon.</returns>
	public async Task<bool> Confirm(string title, string message, ConfirmOptions? confirmOptions = null)
	{
		if (_confirmTcs is { Task.IsCompleted: false })
		{
			throw new InvalidOperationException("A confirmation dialog is already open.");
		}

		if (OnShow is null)
		{
			throw new InvalidOperationException("No confirmation dialog host is registered.");
		}

		var completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		_confirmTcs = completionSource;

		try
		{
			var options = confirmOptions ?? ConfirmOptions.CreateDefault(_localizer);
			await OnShow.Invoke(title, message, options);
			return await completionSource.Task;
		}
		catch
		{
			Interlocked.CompareExchange(ref _confirmTcs, null, completionSource);
			throw;
		}
	}

	/// <summary>
	/// Définit le résultat de la boîte de dialogue de confirmation (appelé par le composant ConfirmDialog).
	/// </summary>
	/// <param name="result">Le résultat de la confirmation.</param>
	internal void SetResult(bool result)
	{
		var completionSource = Interlocked.Exchange(ref _confirmTcs, null);
		completionSource?.TrySetResult(result);
	}

	/// <summary>
	/// Affiche un composant Blazor dans une modale et attend le résultat.
	/// </summary>
	/// <typeparam name="T">Le type du composant Blazor à afficher.</typeparam>
	/// <param name="title">Le titre de la modale.</param>
	/// <param name="parameters">Les paramètres à passer au composant.</param>
	/// <param name="options">Les options de configuration de la modale.</param>
	/// <returns>Le résultat retourné par le composant via Close().</returns>
	public async Task<dynamic?> OpenAsync<T>(string title, Dictionary<string, object>? parameters = null, DialogOptions? options = null)
		where T : IComponent
	{
		if (_dialogTcs is { Task.IsCompleted: false })
		{
			await Close(null);
		}

		if (OnOpenDialog is null)
		{
			throw new InvalidOperationException("No dialog host is registered.");
		}

		var completionSource = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
		_dialogTcs = completionSource;

		try
		{
			await OnOpenDialog.Invoke(typeof(T), title, parameters, options);
			return await completionSource.Task;
		}
		catch
		{
			Interlocked.CompareExchange(ref _dialogTcs, null, completionSource);
			throw;
		}
	}

	/// <summary>
	/// Affiche la boîte de dialogue de sélection de période et retourne la plage choisie.
	/// </summary>
	/// <param name="title">Le titre de la modale.</param>
	/// <param name="value">La valeur initiale de la période.</param>
	/// <param name="displayWeekNumbers">Indique si les numéros de semaine doivent être affichés.</param>
	/// <param name="disableFutureDates">Indique si les dates futures doivent être désactivées.</param>
	/// <param name="options">Les options d'affichage de la modale.</param>
	/// <returns>La plage sélectionnée, ou <c>null</c> si l'utilisateur annule.</returns>
	public async Task<SuperDateRangeSelection?> OpenDateRangeDialogAsync(
		string title,
		SuperDateRangeSelection? value = null,
		bool displayWeekNumbers = true,
		bool disableFutureDates = true,
		DialogOptions? options = null)
	{
		var parameters = new Dictionary<string, object>
		{
			[nameof(SuperDateRangeDialog.Value)] = value ?? new SuperDateRangeSelection(null, null, SuperDateRangePreset.AllTime),
			[nameof(SuperDateRangeDialog.DisplayWeekNumbers)] = displayWeekNumbers,
			[nameof(SuperDateRangeDialog.DisableFutureDates)] = disableFutureDates
		};

		options ??= new DialogOptions
		{
			Size = DialogSize.ExtraLarge,
			Width = "1100px"
		};

		var result = await OpenAsync<SuperDateRangeDialog>(title, parameters, options);
		return result as SuperDateRangeSelection;
	}

	/// <summary>
	/// Affiche la boîte de dialogue de filtre numérique et retourne la sélection choisie.
	/// </summary>
	/// <param name="title">Le titre de la modale.</param>
	/// <param name="label">Le libellé affiché pour la valeur filtrée.</param>
	/// <param name="value">La valeur initiale du filtre numérique.</param>
	/// <param name="options">Les options d'affichage de la modale.</param>
	/// <returns>Le filtre sélectionné, ou <c>null</c> si l'utilisateur annule.</returns>
	public async Task<SuperDataGridNumberFilterSelection?> OpenNumberFilterDialogAsync(
		string title,
		string label,
		SuperDataGridNumberFilterSelection? value = null,
		DialogOptions? options = null)
	{
		var parameters = new Dictionary<string, object>
		{
			[nameof(SuperDataGridNumberFilterDialog.Label)] = label,
			[nameof(SuperDataGridNumberFilterDialog.Value)] = value ?? SuperDataGridNumberFilterSelection.Empty
		};

		options ??= new DialogOptions
		{
			Width = "520px"
		};

		var result = await OpenAsync<SuperDataGridNumberFilterDialog>(title, parameters, options);
		return result as SuperDataGridNumberFilterSelection;
	}

	/// <summary>
	/// Affiche la boîte de dialogue de filtre enum et retourne la sélection choisie.
	/// </summary>
	/// <param name="title">Le titre de la modale.</param>
	/// <param name="label">Le libellé affiché pour la colonne filtrée.</param>
	/// <param name="enumType">Le type enum à afficher dans la boîte de dialogue.</param>
	/// <param name="value">La valeur initiale du filtre enum.</param>
	/// <param name="options">Les options d'affichage de la modale.</param>
	/// <returns>La sélection choisie, ou <c>null</c> si l'utilisateur annule.</returns>
	public async Task<SuperDataGridEnumFilterSelection?> OpenEnumFilterDialogAsync(
		string title,
		string label,
		Type enumType,
		SuperDataGridEnumFilterSelection? value = null,
		DialogOptions? options = null)
	{
		ArgumentNullException.ThrowIfNull(enumType);

		var parameters = new Dictionary<string, object>
		{
			[nameof(SuperDataGridEnumFilterDialog.Label)] = label,
			[nameof(SuperDataGridEnumFilterDialog.EnumType)] = enumType,
			[nameof(SuperDataGridEnumFilterDialog.Value)] = value ?? SuperDataGridEnumFilterSelection.Empty
		};

		options ??= new DialogOptions
		{
			Width = "520px"
		};

		var result = await OpenAsync<SuperDataGridEnumFilterDialog>(title, parameters, options);
		return result as SuperDataGridEnumFilterSelection;
	}

	/// <summary>
	/// Ferme la modale dynamique avec un résultat.
	/// </summary>
	/// <param name="result">Le résultat à retourner à l'appelant.</param>
	public async Task Close(dynamic? result = null)
	{
		var completionSource = Interlocked.Exchange(ref _dialogTcs, null);

		try
		{
			if (OnCloseDialog is not null)
			{
				await OnCloseDialog.Invoke();
			}
		}
		finally
		{
			completionSource?.TrySetResult(result);
		}
	}
}
