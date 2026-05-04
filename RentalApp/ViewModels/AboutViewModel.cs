/*
 * @file AboutViewModel.cs
 * @brief About page view model for displaying application information
 * @author RentalApp Development Team
 * @date 2026
 */

using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace RentalApp.ViewModels;

/// <summary>
/// View model for the About page that displays application information.
/// </summary>
public class AboutViewModel
{
    /// <summary>Gets the application title.</summary>
    public string Title => AppInfo.Name;

    /// <summary>Gets the application version.</summary>
    public string Version => AppInfo.VersionString;

    /// <summary>Gets the URL for more information.</summary>
    public string MoreInfoUrl => "https://aka.ms/maui";

    /// <summary>Gets a descriptive message about the application technology stack.</summary>
    public string Message => "This app is written in XAML and C# with .NET MAUI.";

    /// <summary>Command to show more information in the browser.</summary>
    public ICommand ShowMoreInfoCommand { get; }

    public AboutViewModel()
    {
        ShowMoreInfoCommand = new AsyncRelayCommand(ShowMoreInfo);
    }

    private async Task ShowMoreInfo() =>
        await Launcher.Default.OpenAsync(MoreInfoUrl);
}