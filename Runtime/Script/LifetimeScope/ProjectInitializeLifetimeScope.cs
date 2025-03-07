
using VContainer;
using VContainer.Unity;

namespace ProjectCore
{
    public class ProjectInitializeLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<ProjectEntryPoint>(Lifetime.Scoped);
        }
        
        private void Start()
        {
            Build();
        }
    }
}