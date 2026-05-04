/*
 * @file TempViewModel.cs
 * @brief Temporary placeholder view model
 * @author RentalApp Development Team
 * @date 2025
 */

using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace RentalApp.ViewModels;

/// <summary>
/// Temporary view model for placeholder pages.
/// Simple view model that displays basic application information.
/// </summary>
/// <remarks>
/// This is a placeholder implementation for temporary pages.
/// </remarks>
public class TempViewModel
{
    /// <summary>
    /// Gets the application title from AppInfo.
    /// </summary>
    public string Title => AppInfo.Name;

    /// <summary>
    /// Gets the application version from AppInfo.
    /// </summary>
    public string Version => AppInfo.VersionString;

    /// <summary>
    /// Gets a placeholder message.
    /// </summary>
    public string Message => "This is a placeholder page.";

    /// <summary>
    /// Initializes a new instance of the TempViewModel class.
    /// </summary>
    public TempViewModel()
    {
    }
}