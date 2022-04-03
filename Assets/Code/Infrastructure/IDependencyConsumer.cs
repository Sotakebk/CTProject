namespace CTProject.Infrastructure
{
    public interface IDependencyConsumer
    {
        void LoadDependencies(IDependencyProvider dependencyProvider);
    }
}
