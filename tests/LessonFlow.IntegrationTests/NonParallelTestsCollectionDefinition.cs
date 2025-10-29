namespace LessonFlow.IntegrationTests;

[CollectionDefinition("Non-ParallelTests", DisableParallelization = true)]
public class NonParallelTestsCollectionDefinition : ICollectionFixture<CustomWebApplicationFactory>{ }
