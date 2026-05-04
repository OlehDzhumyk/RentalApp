/*
 * @file AuthenticationResult.cs
 * @brief DTO for authentication operation results
 */

using RentalApp.Database.Models;

namespace RentalApp.Services;

public class AuthenticationResult
{
    // Changed IsSuccess to Success to match our previous service logic
    public bool IsSuccess { get; }
    public string Message { get; }
    public User? User { get; }

    public AuthenticationResult(bool success, string message, User? user = null)
    {
        IsSuccess = success;
        Message = message;
        User = user;
    }

    public static AuthenticationResult SuccessResult(string message = "Success", User? user = null)
        => new(true, message, user);

    public static AuthenticationResult Failure(string message)
        => new(false, message);
}
