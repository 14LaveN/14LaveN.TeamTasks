using TeamTasks.Common.Tests.Abstractions;

namespace TeamTasks.Common.Tests.Abstractions;

[CollectionDefinition(nameof(IntegrationTestCollection))]
public sealed class IntegrationTestCollection : ICollectionFixture<IntegrationTestWebAppFactory>;
