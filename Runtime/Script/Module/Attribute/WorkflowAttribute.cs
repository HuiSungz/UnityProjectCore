
using System;

namespace ProjectCore.Module
{
    [AttributeUsage(AttributeTargets.Class)]
    public class WorkflowAttribute : Attribute
    {
        public byte Order { get; private set; }

        public WorkflowAttribute(byte order)
        {
            Order = order;
        }
    }
}