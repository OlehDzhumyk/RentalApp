/*
 * @file RoleItem.cs
 * @brief Represents a role item in the user interface with assignment state.
 * @author RentalApp Development Team
 * @date 2026
 */

using CommunityToolkit.Mvvm.ComponentModel;

namespace RentalApp.ViewModels;

/// <summary>
/// Represents a role item in the user interface with assignment state.
/// This class handles UI-specific logic for displaying role status, 
/// including dynamic button text and color transitions.
/// </summary>
public partial class RoleItem : ObservableObject
{
    /// <summary>Unique identifier for the role.</summary>
    [ObservableProperty]
    public partial int Id { get; set; }

    /// <summary>The display name of the role.</summary>
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    /// <summary>A brief description of what the role permits.</summary>
    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this role is assigned to the current user.
    /// Updating this value triggers notifications for ButtonText and ButtonColor.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ButtonText))]
    [NotifyPropertyChangedFor(nameof(ButtonColor))]
    public partial bool IsAssigned { get; set; }

    /// <summary>
    /// Gets the appropriate text for the action button based on assignment state.
    /// </summary>
    public string ButtonText => IsAssigned ? "Remove" : "Add";

    /// <summary>
    /// Gets the visual color for the action button.
    /// Red (#dc3545) for removal, Green (#28a745) for assignment.
    /// </summary>
    public Color ButtonColor => IsAssigned
        ? Color.FromArgb("#dc3545")
        : Color.FromArgb("#28a745");
}