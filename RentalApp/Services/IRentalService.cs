/*
 * @file IRentalService.cs
 * @brief Interface for rental business logic
 */
namespace RentalApp.Services;

public interface IRentalService
{
    Task<bool> CanRentItemAsync(int itemId, DateTime start, DateTime end);
}
