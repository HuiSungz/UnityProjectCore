
using Cysharp.Threading.Tasks;

namespace ProjectCore.Module
{
    public interface IProjectWorkflow
    {
        UniTask<bool> FlowAsync();
    }
}