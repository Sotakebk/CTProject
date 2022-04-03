namespace CTProject.Infrastructure
{
    public interface IDependencyProvider
    {
        T GetDependency<T>();
    }
}
