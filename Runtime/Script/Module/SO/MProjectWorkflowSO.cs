
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectCore.Module
{
    [ProjectModule("Project Workflow", true, 990)]
    public class MProjectWorkflowSO : BaseProjectModuleSO
    {
        #region Fields

        [SerializeField] private ProjectWorkflowSO[] _projectWorkflows;
        private List<IProjectWorkflow> _sortedWorkflow;
        
        public override string Name => "Workflow";

        #endregion
        
        public override void ConfigureInitialize()
        {
            _sortedWorkflow = SortWorkflowsByOrder(_projectWorkflows).ToList();
        }

        private IEnumerable<IProjectWorkflow> SortWorkflowsByOrder(IEnumerable<ProjectWorkflowSO> workflows)
        {
            return workflows.OrderBy(workflow =>
            {
                var attributeOrder = workflow.GetType()
                    .GetCustomAttributes(typeof(WorkflowAttribute), true)
                    .FirstOrDefault() as WorkflowAttribute;

                return attributeOrder?.Order ?? byte.MaxValue;
            });
        }

        public async UniTask<bool> ExecuteAsync()
        {
            if (_sortedWorkflow == null || _sortedWorkflow.Count == 0)
            {
                Debug.LogWarning($"[{Name}] Workflow이 정의되어 있지 않습니다.");
                return true;
            }
            
            var isSucceeded = false;
            foreach (var workflow in _sortedWorkflow)
            {
                try
                {
                    isSucceeded = await workflow.FlowAsync();
                    
                    if (isSucceeded)
                    {
                        await UniTask.NextFrame();
                        continue;
                    }
                    
                    Debug.LogError($"[{Name}] 워크플로우 실행 실패: {workflow.GetType().Name}");
                    break;
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                    Debug.LogError($"[{Name}] 워크플로우 실행 중 예외 발생: {workflow.GetType().Name}");
                    return false;
                }
            }

            return isSucceeded;
        }
    }
}