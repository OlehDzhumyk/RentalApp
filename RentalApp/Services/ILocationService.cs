/*
 * @file ILocationService.cs
 * @brief Interface for geolocation services
 * @author RentalApp Development Team
 * @date 2026
 */

namespace RentalApp.Services;

/// <summary>
/// Defines the contract for retrieving the device's current geographic coordinates.
/// </summary>
public interface ILocationService
{
    /// <summary>
    /// Gets the current location of the device.
    /// </summary>
    /// <returns>A tuple containing latitude and longitude, or null if location is unavailable.</returns>
    Task<(double Latitude, double Longitude)?> GetCurrentLocationAsync();
}