namespace ReforaTec.Api.IntegrationTests.Fixtures;

[CollectionDefinition(Name)]
public class IntegrationTestCollection : ICollectionFixture<ReforaTecWebApplicationFactory>
{
    public const string Name = "Integration";
}
