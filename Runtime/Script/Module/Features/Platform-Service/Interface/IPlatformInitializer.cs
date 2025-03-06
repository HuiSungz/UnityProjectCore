
using System;

namespace ProjectCore.Module
{
    public interface IPlatformInitializer
    {
        bool Initialized { get; }
        
        void Initialize(Action onInitializeComplete = null);
    }
}