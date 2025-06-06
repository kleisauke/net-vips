using Xunit;

namespace NetVips.Tests;

[CollectionDefinition(nameof(NonParallelTestCollection), DisableParallelization = true)]
public class NonParallelTestCollection : ICollectionFixture<TestsFixture>;