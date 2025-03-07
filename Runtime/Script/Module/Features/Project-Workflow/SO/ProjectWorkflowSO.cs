
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using ProjectCore.Base;
using UnityEngine;

namespace ProjectCore.Module
{
    public abstract class ProjectWorkflowSO : BaseAttributeRequiredSO<WorkflowAttribute>, IProjectWorkflow
    {
        private static readonly Dictionary<byte, Type> RegisteredOrders = new();

        protected override void OnEnable()
        {
            base.OnEnable();

            if (GetType().GetCustomAttributes(typeof(WorkflowAttribute), true)
                    .FirstOrDefault() is not WorkflowAttribute workflowAttribute)
            {
                Debug.LogError($"[{GetType().Name}] WorkflowAttribute가 클래스에 정의되어 있지 않습니다.");
                return;
            }
            
            var order = workflowAttribute.Order;
            if (RegisteredOrders.TryGetValue(order, out Type existingType))
            {
                if (existingType == GetType())
                {
                    return;
                }
                
                Debug.LogError($"[{GetType().Name}] Order {order}는 이미 {existingType.Name}에서 사용 중입니다. 중복된 Order 값은 허용되지 않습니다.");
            }
            else
            {
                RegisteredOrders[order] = GetType();
                Debug.Log($"[{GetType().Name}] Order {order} 등록 완료");
            }
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void ValidateWorkflowOrders()
        {
            RegisteredOrders.Clear();
            
            var workflowTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(ProjectWorkflowSO).IsAssignableFrom(t) && !t.IsAbstract);
            
            foreach (var type in workflowTypes)
            {
                if (type.GetCustomAttributes(typeof(WorkflowAttribute), true)
                        .FirstOrDefault() is not WorkflowAttribute attr)
                {
                    continue;
                }
                
                var order = attr.Order;
                if (RegisteredOrders.TryGetValue(order, out Type existingType))
                {
                    UnityEditor.EditorApplication.delayCall += () =>
                    {
                        Debug.LogError($"중복된 WorkflowAttribute Order: {order} - 타입: {type.Name}와 {existingType.Name}");
                    };
                }
                else
                {
                    RegisteredOrders[order] = type;
                }
            }
        }
#endif
        /// <summary>
        /// Need async keyword to use UniTask
        /// </summary>
        /// <returns>Flow Complete</returns>
        public abstract UniTask<bool> FlowAsync();
    }
}