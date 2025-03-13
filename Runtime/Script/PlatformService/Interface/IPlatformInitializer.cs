
using System;

namespace ProjectCore.PlatformService
{
    public interface IPlatformInitializer
    {
        bool Initialized { get; }
        
        void Initialize(Action onInitializeComplete = null);
    }
}