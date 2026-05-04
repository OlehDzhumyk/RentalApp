/*
 * @file LocationService.cs
 * @brief Implementation of ILocationService using MAUI Geolocation with DI support
 * @author RentalApp Development Team
 * @date 2026
 */

using Microsoft.Maui.Devices.Sensors;

namespace RentalApp.Services;

public class LocationService : ILocationService
{
    private readonly IGeolocation _geolocation;

    /// <summary>
    /// Initializes a new instance of the LocationService.
    /// Injects IGeolocation to allow mocking in unit tests.
    /// </summary>
    public LocationService(IGeolocation geolocation)
    {
        _geolocation = geolocation;
    }

    /// <inheritdoc/>
    public async Task<(double Latitude, double Longitude)?> GetCurrentLocationAsync()
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));

            // Use the injected instance instead of static Geolocation.Default
            var location = await _geolocation.GetLocationAsync(request);

            if (location != null)
            {
                return (location.Latitude, location.Longitude);
            }
        }
        catch (FeatureNotSupportedException)
        {
            System.Diagnostics.Debug.WriteLine("Location is not supported on this device.");
        }
        catch (PermissionException)
        {
            System.Diagnostics.Debug.WriteLine("Location permission denied.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Geolocation error: {ex.Message}");
        }

        return null;
    }
}
