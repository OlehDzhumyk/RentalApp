using RentalApp.Database.Models;

namespace RentalApp.Services
{

    /// <summary>
    /// Data transfer object representing the result of an authentication operation.
    /// Matches the expected properties in ViewModels for error handling and success state.
    /// </summary>
    public class AuthenticationResult
    {
        /// <summary>Gets whether the operation was successful.</summary>
        public bool IsSuccess { get; }

        /// <summary>Gets the success or error message.</summary>
        public string Message { get; }

        /// <summary>Gets the user entity if authentication succeeded.</summary>
        public User? User { get; }

        public AuthenticationResult(bool isSuccess, string message, User? user = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            User = user;
        }

        /// <summary>Helper to create a successful result.</summary>
        public static AuthenticationResult Success(string message = "Success", User? user = null)
            => new(true, message, user);

        /// <summary>Helper to create a failed result.</summary>
        public static AuthenticationResult Failure(string message)
            => new(false, message);
    }
}