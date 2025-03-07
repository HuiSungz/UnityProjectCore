
using UnityEngine;

namespace ProjectCore.Base
{
    public abstract class BaseDescriptionSO : ScriptableObject
    {
#pragma warning disable CS0414
        [SerializeField] [TextArea]
        private string _description = "Input your comment.";
    }
}