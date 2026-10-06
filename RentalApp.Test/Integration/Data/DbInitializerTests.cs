/*
 * @file DbInitializerTests.cs
 * @brief Integration tests for the demo data seeder
 */

using Microsoft.EntityFrameworkCore;
using RentalApp.Database.Data;
using RentalApp.Database.Models;

namespace RentalApp.Test.Integration.Data;

[Collection("Database collection")]
public class DbInitializerTests : BaseIntegrationTest
{
    [Fact]
    public async Task SeedAsync_ShouldCreateDemoAccountsThatCanLogIn()
    {
        Assert.True(await DbInitializer.SeedAsync(Context));

        var admin = await Context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .SingleAsync(u => u.Email == DbInitializer.AdminEmail);
        Assert.True(BCrypt.Net.BCrypt.Verify(DbInitializer.DemoPassword, admin.PasswordHash));
        Assert.Contains(admin.UserRoles, ur => ur.Role.Name == RoleConstants.Admin);

        var sam = await Context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .SingleAsync(u => u.Email == DbInitializer.UserEmail);
        Assert.DoesNotContain(sam.UserRoles, ur => ur.Role.Name == RoleConstants.Admin);

        Assert.True(await Context.Items.CountAsync() >= 5);
        Assert.All(await Context.Items.ToListAsync(), item => Assert.NotNull(item.Location));
    }

    [Fact]
    public async Task SeedAsync_ShouldDoNothing_WhenUsersAlreadyExist()
    {
        await DbInitializer.SeedAsync(Context);
        var items = await Context.Items.CountAsync();

        Assert.False(await DbInitializer.SeedAsync(Context));
        Assert.Equal(items, await Context.Items.CountAsync());
    }
}
