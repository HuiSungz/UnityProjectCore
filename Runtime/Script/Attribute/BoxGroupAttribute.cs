
using UnityEngine;

namespace ProjectCore.Attributes
{
    public class BoxGroupAttribute : PropertyAttribute
    {
        public string GroupName { get; private set; }
        public BoxGroupAttribute(string groupName)
        {
            GroupName = groupName;
        }
    }
}