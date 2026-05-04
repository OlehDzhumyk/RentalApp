/*
 * @file LocationService.cs
 * @brief Implementation of ILocationService using MAUI Geolocation
 * @author RentalApp Development Team
 * @date 2026
 */

namespace RentalApp.Services;

/// <summary>
/// Provides real GPS coordinates using the Microsoft.Maui.Devices.Sensors Geolocation API.
/// </summary>
public class LocationService : ILocationService
{
    /// <inheritdoc/>
    public async Task<(double Latitude, double Longitude)?> GetCurrentLocationAsync()
    {
        try
        {
            // Request the location with medium accuracy for a balance between speed and precision
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location != null)
            {
                return (location.Latitude, location.Longitude);
            }
        }
        catch (Exception ex)
        {
            // Logging would go here in a full production app
            System.Diagnostics.Debug.WriteLine($"Geolocation error: {ex.Message}");
        }

        return null;
    }
}