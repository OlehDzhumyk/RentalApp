using Xunit;

namespace RentalApp.Test.Integration;

[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<object>
{
}
