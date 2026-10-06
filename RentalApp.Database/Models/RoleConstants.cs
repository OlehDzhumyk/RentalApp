namespace RentalApp.Database.Models;

/// <summary>
/// Names of the roles seeded by the initial migration.
/// </summary>
public static class RoleConstants
{
    public const string Admin = "Admin";
    public const string User = "User";

    public static readonly string[] AllRoles = { Admin, User };
}
