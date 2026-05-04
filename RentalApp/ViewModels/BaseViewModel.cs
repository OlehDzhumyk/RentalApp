/*
 * @file BaseViewModel.cs
 * @brief Base view model class providing common functionality for all view models
 * @author RentalApp Development Team
 * @date 2026
 */

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RentalApp.ViewModels;

/// <summary>
/// Base view model class that provides common properties and functionality.
/// Extends ObservableObject to provide property change notifications and includes
/// common properties like IsBusy, Title, and error handling.
/// </summary>
public partial class BaseViewModel : ObservableObject
{
    /// <summary>
    /// Indicates whether the view model is currently performing a busy operation.
    /// </summary>
    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    /// <summary>
    /// The title of the current page or view.
    /// </summary>
    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    /// <summary>
    /// The current error message, if any.
    /// </summary>
    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether there is currently an error state.
    /// </summary>
    [ObservableProperty]
    public partial bool HasError { get; set; }

    /// <summary>
    /// Sets an error message and updates the error state.
    /// </summary>
    /// <param name="message">The error message to set.</param>
    protected void SetError(string message)
    {
        ErrorMessage = message;
        HasError = !string.IsNullOrEmpty(message);
    }

    /// <summary>
    /// Clears the current error state.
    /// </summary>
    protected void ClearError()
    {
        ErrorMessage = string.Empty;
        HasError = false;
    }

    /// <summary>
    /// Command to clear the current error state.
    /// </summary>
    [RelayCommand]
    private void ClearErrorCommand()
    {
        ClearError();
    }
}