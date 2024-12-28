using Wordflux.Tests.Integration.Containers;

namespace Wordflux.Tests.Integration.TestFixture;

[CollectionDefinition(nameof(SharedTestCollection))]
public class SharedTestCollection : ICollectionFixture<DockerFixtures>;