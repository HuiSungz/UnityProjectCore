
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectCore.Module;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ProjectCore
{
    public class ProjectEntryPoint : IAsyncStartable
    {
        #region Fields

        [Inject] private ProjectModules _projectModules;

        #endregion
        
        public async UniTask StartAsync(CancellationToken cancellation = new())
        {
            try
            {
                var workflowModule = _projectModules.GetModule<MProjectWorkflowSO>();
                if(!workflowModule)
                {
                    Debug.LogError("Project Workflow Module is Null");
                    return;
                }

                var isSuccess = await workflowModule.ExecuteAsync();
                if (!isSuccess)
                {
                    Debug.LogError("Project Workflow Execute Fail");
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"ProjectEntryPoint Error Exception Catch : {exception.Message}");
                throw;
            }
        }
    }
}
