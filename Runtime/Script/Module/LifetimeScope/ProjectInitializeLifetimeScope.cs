
using VContainer;
using VContainer.Unity;

namespace ProjectCore.Module
{
    public class ProjectInitializeLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            
        }

        private void Start()
        {
            Build();
        }
    }
}